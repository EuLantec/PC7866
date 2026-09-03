# PC7866 - Arquitectura y Documentación Técnica

## 📐 Estructura del Proyecto

```
PC7866/
├── Models/                          # Modelos de dominio, comandos y resultados
│   ├── Referencia.cs                # Referencia/producto + configuración de placa
│   ├── ParametroEnsayo.cs           # Paso de ensayo (contacto, selectores, calibración)
│   ├── Resultado.cs                 # Cabecera de un ensayo
│   ├── ResultadoDetalle.cs          # Resultado por paso + EstadoMedicion
│   ├── Pc7866Commands.cs            # Builders de tramas del protocolo serie
│   └── ...                          # TestParameters, MeasurementResult, DeviceResponse, etc.
│
├── Services/
│   ├── SerialCommunication/
│   │   ├── ISerialPortService.cs    # Interfaz de comunicación
│   │   ├── SerialPortService.cs     # Implementación puerto serie
│   │   └── CommandParser.cs         # Parser de respuestas analógicas
│   ├── Database/
│   │   ├── ITestRepository.cs       # Interfaz del repositorio
│   │   └── TestRepository.cs        # Acceso a MariaDB (Dapper) + creación/migración de esquema
│   └── StateMachine/
│       ├── ITestState.cs            # Interfaz de estado
│       ├── TestContext.cs           # Contexto compartido del ensayo
│       ├── TestStateMachine.cs      # Orquestador de estados
│       └── States/                  # InitializingState, RunningState, CompletedState
│
├── Views/                           # Paneles WinForms
│   ├── ManualControlPanel.cs        # Modo manual (diagnosis, salidas, analógica)
│   ├── AutomaticTestPanel.cs        # Modo automático (ejecución de ensayo)
│   ├── ParametersPanel.cs           # Referencias y parámetros de ensayo
│   ├── ReportsPanel.cs              # Informes / histórico
│   ├── ConfigurationForm.cs         # Configuración (puerto, BD, opciones)
│   └── ...                          # ResultadoDetalleForm
│
├── Utils/
│   ├── Logger.cs                    # Sistema de logging
│   └── ParametroImportExport.cs     # Import/export CSV y JSON de parámetros
│
└── Configuration/
    └── AppSettings.cs               # Configuración global (persistida en appsettings.json)
```

---

## 🔧 Componentes Implementados

### 1️⃣ **SerialPortService** - Comunicación Serie USB

**Características:**
- ✅ Comunicación asíncrona (async/await)
- ✅ Manejo de timeouts
- ✅ Eventos para datos recibidos y errores
- ✅ Thread-safe (usa SemaphoreSlim)
- ✅ Buffer de recepción inteligente
- ✅ Configuración flexible (baudrate, paridad, bits)

**Ejemplo de uso:**
```csharp
var serialPort = new SerialPortService();

// Conectar
serialPort.Open("COM3", 9600);

// Enviar comando y esperar respuesta
string response = await serialPort.SendCommandAsync("*IDN?", 5000);

// Cerrar
serialPort.Close();
```

**Eventos disponibles:**
- `DataReceived` - Datos recibidos del dispositivo
- `ErrorOccurred` - Error en comunicación
- `PortOpened` - Puerto abierto exitosamente
- `PortClosed` - Puerto cerrado

---

### 2️⃣ **CommandParser** - Parser de Comandos

**Funciones:**
- Parsea respuestas del dispositivo
- Extrae valores numéricos
- Valida respuestas contra patrones esperados
- Detecta códigos de error

**Ejemplo:**
```csharp
var parser = new CommandParser();

// Parsear respuesta
var response = parser.ParseResponse("RESISTANCE=1234.56");

// Extraer valor numérico
decimal? value = parser.ExtractNumericValue(response.RawData);

// Validar patrón
bool valid = parser.ValidateResponse(response.RawData, @"RESISTANCE=\d+");
```

---

### 3️⃣ **Modelos de Datos**

#### TestParameters
Define la configuración de un test:
- Secuencia de comandos
- Timeouts
- Tolerancias
- Metadata

#### MeasurementCommand
Comando individual con:
- Comando a enviar
- Patrón de respuesta esperado
- Delay después del comando
- Criticidad (¿fallar test si falla?)

#### TestResult
Resultado completo:
- Estado (Passed/Failed/Error)
- Lista de mediciones individuales
- Duración
- Observaciones

---

### 4️⃣ **ManualControlPanel** - Interfaz Modo Manual

**Funcionalidades:**
- ✅ Selección de puerto y velocidad
- ✅ Conexión/desconexión
- ✅ Diagnosis, configuración de MCP, matriz de 96 salidas y lectura analógica
- ✅ Log en tiempo real con timestamps
- ✅ Manejo de errores y timeouts
- ✅ Indicador de estado en barra inferior

**Controles:**
- ComboBox para puertos disponibles
- ComboBox para velocidades (9600-115200)
- TextBox para comandos
- Log estilo terminal (fondo negro, texto verde)
- Botones: Conectar, Desconectar, Enviar, Limpiar

---

### 5️⃣ **Logger** - Sistema de Logging

**Características:**
- Singleton pattern
- Logs guardados en: `%LocalAppData%\PC7866\Logs\`
- Archivo diario: `log_YYYYMMDD.txt`
- Niveles: Debug, Info, Warning, Error
- También escribe en Debug output

**Uso:**
```csharp
Logger.Instance.Info("Conexión establecida");
Logger.Instance.Error($"Error: {ex.Message}");
```

---

### 6️⃣ **AppSettings** - Configuración Global

**Parámetros configurables:**
```csharp
// Puerto serie
DefaultPortName = "COM1"
DefaultBaudRate = 9600
DefaultTimeout = 5000

// Base de datos
DatabaseServer = "localhost"
DatabaseName = "pc7866_test"
DatabaseUser = "root"
DatabasePort = 3306

// Test
MaxRetries = 3
DelayBetweenCommandsMs = 100
AutoSaveResults = true
```

---

## 🚀 Cómo Probar el Modo Manual

1. **Compilar el proyecto:**
   ```bash
   dotnet build
   ```

2. **Ejecutar:**
   ```bash
   dotnet run
   ```

3. **Usar la interfaz:**
   - Seleccionar puerto COM
   - Seleccionar velocidad (ej: 115200)
   - Clic en "Conectar"
   - Lanzar una diagnosis (`DT`) o activar salidas desde la matriz
   - Ver la trama y la respuesta (`O`/`N`) en el log

---

## 📦 Dependencias NuGet

```xml
<PackageReference Include="System.IO.Ports" />
<PackageReference Include="MySqlConnector" />
<PackageReference Include="Dapper" />
<PackageReference Include="PdfSharpCore" />
<PackageReference Include="SixLabors.ImageSharp" />
```

(Ver versiones exactas en [`PC7866.csproj`](PC7866.csproj).)

---

## 🎯 Estado del proyecto

Los módulos principales están implementados y operativos:

- **Base de datos MariaDB** — `TestRepository` crea/asegura el esquema (`CREATE DATABASE`/`CREATE TABLE IF NOT EXISTS`) y migra columnas nuevas con `ALTER TABLE ... ADD COLUMN IF NOT EXISTS`.
- **Máquina de estados** — `TestStateMachine` + estados `Initializing` → `Running` → `Completed`.
- **Modo automático** — `AutomaticTestPanel` ejecuta el ensayo punto a punto y guarda resultados en BD.
- **Mapa de contactos** — `ParametersPanel` y `AutomaticTestPanel` dibujan una bola por cada parámetro colocado sobre la imagen. La bola contiene `NombreContacto` y, durante el ensayo, usa el color del estado de la medición.
- **Import/export** — parámetros de ensayo en CSV y JSON (`ParametroImportExport`).
- **Informes** — histórico y detalle por paso (`ReportsPanel`, `ResultadoDetalleForm`), con exportación PDF.

---

## 🔍 Patrones de Diseño Utilizados

1. **Singleton**: `Logger`, `AppSettings`
2. **Repository Pattern**: `ITestRepository` / `TestRepository` (Dapper sobre MariaDB)
3. **State Machine**: `TestStateMachine` + `ITestState` (Initializing/Running/Completed)
4. **Dependency Injection**: Uso de interfaces (`ISerialPortService`, `ITestRepository`)
5. **Event-Driven**: Eventos para comunicación asíncrona y `StepCompleted` por paso
6. **Async/Await**: Todas las operaciones I/O son asíncronas

---

## 📝 Convenciones de Código

- ✅ Nullable reference types habilitado
- ✅ Namespaces file-scoped
- ✅ Async suffix en métodos asíncronos
- ✅ Interfaces con prefijo `I`
- ✅ Campos privados con `_` prefix
- ✅ Comentarios XML en APIs públicas

---

## 🐛 Debugging

**Logs se guardan en:**
```
%LocalAppData%\PC7866\Logs\log_YYYYMMDD.txt
```

**Para ver logs en tiempo real:**
- Visual Studio: Output window (Debug)
- Interfaz: Log de comunicación

---

## 📚 Referencias

- [System.IO.Ports Documentation](https://docs.microsoft.com/dotnet/api/system.io.ports)
- [Async/Await Best Practices](https://docs.microsoft.com/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
- [Windows Forms .NET](https://docs.microsoft.com/dotnet/desktop/winforms/)
