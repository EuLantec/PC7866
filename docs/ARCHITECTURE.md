# PC7866 - Arquitectura y documentación técnica

Aplicación Windows Forms (`net10.0-windows`) para ensayos resistivos del banco PC7866: comunica por puerto serie con el microcontrolador de la placa, mide resistencia y cortocircuito punto a punto y guarda los resultados en MariaDB/MySQL.

> Los diagramas (arquitectura, circuito de medida, máquina de estados, secuencias, modelo de datos y clases) están en [diagrama.md](diagrama.md).

## Índice

- [Estructura del proyecto](#-estructura-del-proyecto)
- [Componentes](#-componentes)
- [Flujo de un ensayo](#-flujo-de-un-ensayo)
- [Patrones de diseño](#-patrones-de-diseño)
- [Convenciones](#-convenciones-de-código)
- [Depuración](#-depuración)
- [Documentación relacionada](#-documentación-relacionada)

---

## 📐 Estructura del proyecto

```
PC7866/
├── Program.cs                       # Punto de entrada (STAThread, lanza Form1)
├── Form1.cs                         # Ventana principal: menú, panel contenedor, barra de estado
│
├── Configuration/
│   └── AppSettings.cs               # Singleton; persiste en appsettings.json (clave BD cifrada con DPAPI)
│
├── Models/                          # Modelos de dominio y protocolo
│   ├── Referencia.cs                # Producto + configuración de placa (comando "I") + umbral de cortocircuito
│   ├── ParametroEnsayo.cs           # Paso de ensayo: selectores arriba/abajo, pista, nominal, tolerancia, calibración
│   ├── Resultado.cs                 # Cabecera de un ensayo (operario, lote, resultado global)
│   ├── ResultadoDetalle.cs          # Resultado por paso + enum EstadoMedicion
│   ├── Pc7866Commands.cs            # Builders de tramas del protocolo serie
│   └── DeviceResponse, TestParameters, MeasurementCommand, MeasurementResult, TestResult
│                                    # Modelos genéricos heredados de la plantilla inicial; el ensayo actual NO los usa
│
├── Services/
│   ├── SerialCommunication/
│   │   ├── ISerialPortService.cs    # Contrato del puerto serie
│   │   ├── SerialPortService.cs     # Implementación (System.IO.Ports, async, thread-safe)
│   │   └── CommandParser.cs         # Parseo de respuestas F (voltios) y R (RAW)
│   ├── Database/
│   │   ├── ITestRepository.cs       # Contrato del repositorio
│   │   └── TestRepository.cs        # Dapper + MySqlConnector; crea y migra el esquema
│   └── StateMachine/
│       ├── ITestState.cs            # Contrato de estado
│       ├── TestContext.cs           # Contexto compartido + TestProgressReport + enum TestState
│       ├── TestStateMachine.cs      # Orquestador (RunAsync)
│       └── States/                  # InitializingState, RunningState, CompletedState
│
├── Views/                           # Paneles y formularios WinForms
│   ├── AutomaticTestPanel.cs        # Modo automático: ejecución del ensayo y mapa de contactos
│   ├── ManualControlPanel.cs        # Modo manual: comandos sueltos + Semiautomático
│   ├── ParametersPanel.cs           # Referencias, pasos de ensayo, imagen y posiciones X/Y
│   ├── ReportsPanel.cs              # Histórico, filtros y exportación CSV
│   ├── ResultadoDetalleForm.cs      # Detalle por paso de un resultado
│   ├── ConfigurationForm.cs         # Puerto serie, BD y opciones
│   └── AboutForm.cs
│
└── Utils/
    ├── Logger.cs                    # Singleton; log diario en archivo
    ├── ParametroImportExport.cs     # Import/export de parámetros en CSV y JSON
    └── UiTheme.cs                   # Tema visual de paneles y formularios
```

---

## 🔧 Componentes

### 1️⃣ SerialPortService - comunicación serie

- Comunicación asíncrona; un comando a la vez (`SemaphoreSlim`), con timeout y `CancellationToken`.
- Cada trama se envía en ASCII terminada en `\r\n`. La respuesta se completa al recibir terminador o tras un breve periodo de inactividad (comandos `F`/`R`, que no lo envían). `DT` es multilínea.
- `SendCommandAsync` lanza `TimeoutException` si no llega respuesta y `OperationCanceledException` si se cancela.

```csharp
var serial = new SerialPortService();
serial.Open("COM4", 115200);

string resp = await serial.SendCommandAsync("DT", timeoutMs: 5000); // diagnosis total
string f0   = await serial.SendCommandAsync("F0");                  // lectura filtrada canal 0

serial.Close();
```

Eventos: `DataReceived`, `ErrorOccurred`, `PortOpened`, `PortClosed`.

`Form1` crea **una única instancia** y la comparte con los paneles Manual y Automático.

### 2️⃣ Pc7866Commands y protocolo serie

Todas las tramas se construyen con `Pc7866Commands`, nunca a mano.

| Comando | Trama | Descripción |
|---------|-------|-------------|
| `D` | `DT`, `D1`, `D2`..`D7`, `DV`, `DG`, `DC` | Diagnosis total, ADS1115, MCP 0x20-0x25, versión, config I2C, temperatura |
| `M` | `M<chip><E\|S><máscara 4 hex>` | Dirección (entrada/salida) de pines de un MCP23017 |
| `S` | `S<chip><estados 4 hex>` | Estado de las 16 salidas de un MCP (una trama por chip) |
| `P` | `Pnn` | Selección de pista en los multiplexores (`00` desconecta, `01`-`48`) |
| `R` / `F` | `Rn` / `Fn` | Lectura RAW / filtrada (voltios) del canal ADS 0-3 |
| `I` | `I<nMcp><inh1..4><modelo:7><muestras:2><retardo:3>` | Configuración de placa |
| `Q` | `Q` | Reset del microcontrolador |

Respuesta: `O` (OK) o `N` (NOK). Detalle de tramas reales: [EJEMPLO_COMANDOS_SERIE.md](EJEMPLO_COMANDOS_SERIE.md).

### 3️⃣ CommandParser

| Método | Uso |
|--------|-----|
| `ParseFilteredValue(resp)` | `float?` con el valor en voltios de una respuesta `F`; `null` si no es válida |
| `ParseRawValue(resp)` | `int?` con el valor RAW de una respuesta `R` |
| `ParseResponse`, `ExtractNumericValue`, `ValidateResponse`, `FormatCommand` | Utilidades genéricas |

### 4️⃣ Modelos de dominio

- **Referencia**: nombre, descripción, imagen, activa/inactiva y configuración de placa (`ModeloPlaca`, `NumMcps`, `Inh1Pos`..`Inh4Pos`, `Muestras`, `RetardoMs`) más `ResistenciaCortocircuito` (umbral común a todos los pasos).
- **ParametroEnsayo**: un paso. Selectores `McpArriba*`/`McpAbajo*` (chip 0-5, pin 1-16), `CanalMultiplexor` (pista 0-48), `ResistenciaNominal`, `Tolerancia`, `ResistenciaMinima`, calibración `Pendiente`/`Offset` y posición `PosX`/`PosY` en la imagen.
- **Resultado**: cabecera (`FechaPrueba`, `Operario`, `Lote`, `ResultadoGlobal`) con su lista de `Detalles`.
- **ResultadoDetalle**: resistencia medida, resistencia de la fase de cortocircuito y `EstadoMedicion` (`Ok`, `Nok`, `Cortocircuito`, `Abierto`).

### 5️⃣ Máquina de estados

`TestStateMachine.RunAsync` encadena `Initializing → Running → Completed → Idle`. `Error` y `Aborted` terminan el bucle sin pasar por `Completed`.

| Estado | Responsabilidad |
|--------|-----------------|
| `InitializingState` | Comprueba el puerto y envía el comando `I` con la configuración de la referencia |
| `RunningState` | Pone todos los MCP a 0V y mide cada paso (resistencia + cortocircuito); al final `P00` y salidas a 0V |
| `CompletedState` | Emite el reporte final (`X/Y pasos OK`) |

`TestContext` transporta referencia, parámetros, puerto, parser, `CancellationToken`, `IProgress<TestProgressReport>`, `CommandLogger` y el evento `StepCompleted`.

La máquina se usa desde `AutomaticTestPanel` (ensayo completo) y desde `ManualControlPanel` (Semiautomático, un solo paso y sin guardar en BD). Detalle del algoritmo en [GUIA_MODO_AUTOMATICO.md](GUIA_MODO_AUTOMATICO.md).

### 6️⃣ TestRepository - base de datos

- `InitializeDatabaseAsync` crea la base si no existe, crea las tablas `referencias`, `parametros_ensayo`, `resultados` y `resultados_detalle` con `CREATE TABLE IF NOT EXISTS` y migra columnas con `ALTER TABLE ... ADD COLUMN IF NOT EXISTS`.
- Cada panel que necesita BD crea su propio repositorio con `AppSettings.Instance.GetConnectionString()`.
- Esquema completo en el [diagrama ER](diagrama.md#7-modelo-de-datos).

### 7️⃣ Paneles (Views)

| Panel | Función |
|-------|---------|
| `ManualControlPanel` | Conexión, diagnosis, modo MCP, pista, matriz de 96 salidas, lectura analógica con cálculo de R, config. de placa, reset, log y Semiautomático |
| `AutomaticTestPanel` | Selección de referencia, operario y lote; ejecución con progreso, tabla de resultados y mapa de contactos con colores de estado; guarda en BD |
| `ParametersPanel` | CRUD de referencias y pasos, duplicar referencia, imagen y posiciones X/Y, import/export CSV y JSON |
| `ReportsPanel` | Histórico con filtros, detalle por paso y exportación CSV (individual y general) |
| `ConfigurationForm` | Puerto serie, conexión a BD y opciones generales |

Guías de uso: [Modo manual](GUIA_MODO_MANUAL.md) · [Modo automático](GUIA_MODO_AUTOMATICO.md).

### 8️⃣ Logger

- Singleton. Archivo diario `log_YYYYMMDD.txt` en `%LocalAppData%\PC7866\Logs\`.
- Niveles: Debug, Info, Warning, Error.

```csharp
Logger.Instance.Info("Conexión establecida");
Logger.Instance.Error($"Error: {ex.Message}");
```

### 9️⃣ AppSettings

Singleton cargado desde `appsettings.json` junto al ejecutable. Valores por defecto:

| Grupo | Clave | Defecto |
|-------|-------|---------|
| Serie | `DefaultPortName` / `DefaultBaudRate` / `DefaultTimeout` | `COM4` / `115200` / `5000` ms |
| BD | `DatabaseServer` / `DatabaseName` / `DatabaseUser` / `DatabasePort` | `localhost` / `control_electrico_salud` / `root` / `3306` |
| Test | `MaxRetries` / `DelayBetweenCommandsMs` / `AutoSaveResults` | `3` / `100` / `true` |
| UI | `ApplicationTitle` / `ShowDetailedLogs` | `PC7866 - Test Resistivo Embega` / `true` |

La contraseña de BD se guarda cifrada con DPAPI (usuario de Windows).

---

## 🔁 Flujo de un ensayo

1. El operario conecta el puerto, elige referencia, escribe operario (obligatorio) y lote.
2. `AutomaticTestPanel` llama a `TestStateMachine.RunAsync`.
3. `InitializingState` envía `I`; `RunningState` mide cada paso en orden de `NPasoEnsayo` y dispara `StepCompleted`.
4. Al terminar se muestra BUENO/MALO y se guarda `Resultado` + `ResultadoDetalle` en BD (si hay conexión).

Casos especiales:

- `Error` (puerto cerrado, `I` rechazado o fallo al inicializar los MCP): se devuelve el resultado con `ResultadoGlobal = false` y la UI lo guarda.
- Cancelar entre pasos (`Aborted`): se devuelve el resultado parcial, que la UI guarda.
- Cancelar durante un comando serie lanza `OperationCanceledException`: la UI registra «Ensayo cancelado» y no guarda nada.

Diagramas: [secuencia](diagrama.md#4-secuencia-de-un-ensayo-automático) · [algoritmo por paso](diagrama.md#5-algoritmo-de-medición-de-un-paso) · [clasificación](diagrama.md#6-clasificación-del-estado-de-un-paso).

---

## 🔍 Patrones de diseño

1. **Singleton**: `Logger`, `AppSettings`.
2. **Repository**: `ITestRepository` / `TestRepository` (Dapper sobre MariaDB).
3. **State**: `TestStateMachine` + `ITestState` (`Initializing`, `Running`, `Completed`).
4. **Inversión de dependencias**: los estados y paneles dependen de `ISerialPortService` / `ITestRepository`.
5. **Eventos y progreso**: `StepCompleted`, `StateChanged`, `IProgress<TestProgressReport>`, eventos del puerto serie.
6. **Async/await** en toda la E/S (serie y BD).

---

## 📝 Convenciones de código

- Nullable reference types habilitado.
- Namespaces file-scoped.
- Sufijo `Async` en métodos asíncronos.
- Interfaces con prefijo `I`.
- Campos privados con prefijo `_`.
- Comentarios XML en APIs públicas.

---

## 🐛 Depuración

- Logs en `%LocalAppData%\PC7866\Logs\log_YYYYMMDD.txt`.
- En vivo: ventana Output de Visual Studio y el log de cada panel. En modo automático, las tramas `TX`/`RX` y los cálculos (`CALC`, `CALC-CORTO`) aparecen en el log si está activado el log de comandos.

---

## 📦 Dependencias NuGet

| Paquete | Uso |
|---------|-----|
| `System.IO.Ports` | Puerto serie |
| `MySqlConnector` + `Dapper` | Acceso a MariaDB/MySQL |
| `PdfSharpCore` + `SixLabors.ImageSharp` | Referenciados en el proyecto; los informes se exportan hoy a CSV |

Versiones exactas en [PC7866.csproj](../PC7866.csproj).

---

## 📚 Documentación relacionada

- [README.md](../README.md): visión general, puesta en marcha y solución de problemas.
- [diagrama.md](diagrama.md): diagramas Mermaid.
- [GUIA_MODO_MANUAL.md](GUIA_MODO_MANUAL.md) · [GUIA_MODO_AUTOMATICO.md](GUIA_MODO_AUTOMATICO.md) · [EJEMPLO_COMANDOS_SERIE.md](EJEMPLO_COMANDOS_SERIE.md).