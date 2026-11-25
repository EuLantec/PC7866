# PC7866 - Diagramas

Diagramas Mermaid del proyecto, alineados con el código (`RunningState`, `TestStateMachine`, `TestRepository`, `Pc7866Commands`). Documentación relacionada: [README](../README.md) · [ARCHITECTURE](ARCHITECTURE.md) · [Modo automático](GUIA_MODO_AUTOMATICO.md) · [Modo manual](GUIA_MODO_MANUAL.md) · [Ejemplo de tramas](EJEMPLO_COMANDOS_SERIE.md).

## Índice

1. [Arquitectura por capas](#1-arquitectura-por-capas)
2. [Hardware y circuito de medida](#2-hardware-y-circuito-de-medida)
3. [Máquina de estados del ensayo](#3-máquina-de-estados-del-ensayo)
4. [Secuencia de un ensayo automático](#4-secuencia-de-un-ensayo-automático)
5. [Algoritmo de medición de un paso](#5-algoritmo-de-medición-de-un-paso)
6. [Clasificación del estado de un paso](#6-clasificación-del-estado-de-un-paso)
7. [Modelo de datos](#7-modelo-de-datos)
8. [Clases principales](#8-clases-principales)
9. [Navegación de la aplicación](#9-navegación-de-la-aplicación)

---

## 1. Arquitectura por capas

`Form1` crea **un único** `SerialPortService` y lo comparte entre el panel manual y el automático. Cada panel que necesita BD crea su propio `TestRepository` a partir de `AppSettings`.

```mermaid
flowchart TB
    subgraph UI["Views - WinForms"]
        Form1["Form1<br/>menú + panelContent"]
        Manual[ManualControlPanel]
        Auto[AutomaticTestPanel]
        Params[ParametersPanel]
        Reports[ReportsPanel]
        Config[ConfigurationForm]
        Detalle[ResultadoDetalleForm]
        About[AboutForm]
    end

    subgraph SVC["Services"]
        subgraph SM["StateMachine"]
            Machine[TestStateMachine]
            Ctx[TestContext]
            Init[InitializingState]
            Run[RunningState]
            Done[CompletedState]
        end
        subgraph COM["SerialCommunication"]
            Serial["SerialPortService<br/>ISerialPortService"]
            Parser[CommandParser]
        end
        subgraph DBL["Database"]
            Repo["TestRepository<br/>ITestRepository"]
        end
    end

    subgraph MOD["Models"]
        Cmds["Pc7866Commands<br/>builders de tramas"]
        Dom["Referencia · ParametroEnsayo<br/>Resultado · ResultadoDetalle"]
    end

    subgraph UTL["Utils y Configuration"]
        Settings["AppSettings<br/>appsettings.json"]
        Logger["Logger<br/>%LocalAppData%/PC7866/Logs"]
        IE[ParametroImportExport]
        Theme[UiTheme]
    end

    HW[("Placa PC7866<br/>USB-Serie")]
    DB[("MariaDB / MySQL")]

    Form1 --> Manual & Auto & Params & Reports
    Form1 --> Config & About
    Reports --> Detalle

    Auto --> Machine
    Manual -->|"Semiautomático: 1 paso"| Machine
    Machine --> Ctx
    Machine --> Init & Run & Done
    Init & Run --> Cmds
    Init & Run --> Serial
    Run --> Parser
    Manual --> Cmds
    Manual --> Serial
    Serial <-->|"ASCII + CRLF"| HW

    Auto & Manual & Params & Reports --> Repo
    Repo <-->|"Dapper + MySqlConnector"| DB
    Repo --> Dom

    Params --> IE
    Config --> Settings
    Repo --> Settings
    Auto & Manual --> Logger
    Form1 --> Theme
```

---

## 2. Hardware y circuito de medida

Cada contacto se mide con un divisor resistivo: la excitación sale de un pin MCP23017 ("arriba", 5V), pasa por una resistencia fija de 390 Ω, el nodo intermedio se lleva al ADS1115 mediante los multiplexores 74HC4067 (comando `P`) y la resistencia a medir cierra contra otro pin MCP23017 ("abajo", 0V).

```mermaid
flowchart LR
    PC["PC Windows<br/>PC7866.exe"] <-->|"USB / COM<br/>115200 baud"| MCU["Microcontrolador"]
    MCU <-->|I2C| MCP["MCP23017 x 0..6<br/>0x20 - 0x25<br/>16 pines por chip"]
    MCU <-->|I2C| ADS["ADS1115<br/>0x48 - canales F0..F3"]
    MCU -->|"Pnn"| MUX["Multiplexores<br/>74HC4067<br/>pistas 0-48"]

    MCP -->|"pin arriba: 5V"| RREF["R fija 390 ohm"]
    RREF --> NODO(("Nodo<br/>intermedio"))
    NODO --> RX["Resistencia a medir<br/>contacto de la placa"]
    RX -->|"pin abajo: 0V"| MCP
    NODO -->|"un canal por contacto"| MUX
    MUX -->|"punto común de medida"| ADS
```

Fórmula (idéntica en modo manual y automático):

```text
Vain   = F0 - F1
Ve     = F2 - F3
R_bruta = Vain / (Ve - Vain) * 390            (solo válida si Ve - Vain > 0)
R       = Pendiente * R_bruta + Offset         (calibración lineal, solo en la medida principal)
Abierto si R_bruta <= 0  o  R_bruta > R cortocircuito del modelo (1000 ohm si no definido)
```

---

## 3. Máquina de estados del ensayo

`TestStateMachine.RunAsync` ejecuta `ExecuteAsync` de cada estado en bucle hasta llegar a `Idle`, `Error` o `Aborted`. **`Error` y `Aborted` no pasan por `CompletedState`.**

```mermaid
stateDiagram-v2
    [*] --> Initializing: RunAsync

    Initializing --> Running: puerto abierto y comando I respondido con O
    Initializing --> Error: puerto cerrado o respuesta distinta de O

    Running --> Running: siguiente paso
    Running --> Completed: todos los pasos medidos
    Running --> Error: algún MCP no responde O al ponerlo como salida a 0V
    Running --> Aborted: cancelación detectada al inicio de un paso

    Completed --> Idle: reporte final
    Idle --> [*]
    Error --> [*]
    Aborted --> [*]

    note right of Running
        Al terminar: P00 y todas las salidas a 0V.
        ResultadoGlobal = true solo si todos los pasos son Ok.
    end note
    note right of Error
        RunAsync devuelve igualmente el Resultado
        con ResultadoGlobal = false.
    end note
```

> Cancelar **durante** un comando serie lanza `OperationCanceledException`, que sale de `RunAsync` y la UI no guarda nada. Cancelar **entre** pasos devuelve el resultado parcial con estado `Aborted`, que la UI sí muestra y guarda.

---

## 4. Secuencia de un ensayo automático

```mermaid
sequenceDiagram
    autonumber
    actor Op as Operario
    participant UI as AutomaticTestPanel
    participant SM as TestStateMachine
    participant ST as Estados
    participant SP as SerialPortService
    participant HW as Placa PC7866
    participant DB as TestRepository

    Op->>UI: Conectar puerto COM
    UI->>SP: Open(puerto, baudios)
    Op->>UI: Selecciona Referencia
    UI->>DB: GetParametrosByReferenciaAsync(refId)
    DB-->>UI: List ParametroEnsayo
    Op->>UI: Operario + Lote + Iniciar ensayo
    UI->>SM: RunAsync(ref, parametros, operario, lote, serial, parser, ...)

    Note over SM,HW: Fase Initializing
    SM->>ST: InitializingState.ExecuteAsync
    ST->>SP: I (config de placa)
    SP->>HW: trama I
    HW-->>SP: O

    Note over SM,HW: Fase Running
    SM->>ST: RunningState.ExecuteAsync
    ST->>SP: M..FFFF + S..0000 por cada MCP
    loop Por cada paso, ordenado por NPasoEnsayo
        ST->>SP: S arriba=5V, P pista, F0 (y F1..F3 solo en el primer paso)
        ST->>SP: M abajo=entrada, F0
        ST->>SP: S arriba=0V, M abajo=salida, S abajo=0V
        SP->>HW: tramas
        HW-->>SP: respuestas
        ST-->>UI: Progress + StepCompleted(paso, detalle)
        UI->>UI: Pinta bola, fila de la tabla y log
    end
    ST->>SP: P00 y S..0000 por cada MCP

    Note over SM: Fase Completed
    SM->>ST: CompletedState.ExecuteAsync
    ST-->>UI: Progress final (X/Y pasos OK)

    SM-->>UI: Resultado
    UI->>UI: ShowFinalResult (BUENO / MALO)
    UI->>DB: InsertResultadoAsync + InsertDetalleAsync por paso
    DB-->>UI: Id del resultado
```

---

## 5. Algoritmo de medición de un paso

Lógica de `RunningState.MedirPasoAsync`. El paso se restaura siempre en el bloque `finally`.

```mermaid
flowchart TD
    A([Inicio del paso]) --> B{"Selectores arriba y<br/>abajo configurados?"}
    B -->|no| NOK1[/"Estado = Nok"/]
    B -->|si| C["S: pin arriba a 5V<br/>el pin abajo ya está a 0V"]
    C --> D["P: seleccionar pista<br/>CanalMultiplexor"]
    D --> E{"Respuesta de P<br/>empieza por O?"}
    E -->|no| NOK1
    E -->|si| F["Leer F0"]
    F --> G{"F0 válido?"}
    G -->|no| NOK1
    G -->|si| H{"F1, F2, F3<br/>ya en caché?"}
    H -->|no| I["Leer F1, F2, F3<br/>solo en el primer paso"]
    I --> J{"Lecturas válidas?"}
    J -->|no| NOK1
    J -->|si| K
    H -->|si| K["Calcular Vain, Ve, R_bruta"]
    K --> L["Clasificar la medida principal<br/>ver diagrama 6"]

    L --> M["M: pin abajo a ENTRADA<br/>alta impedancia, arriba sigue a 5V"]
    M --> N{"Respuesta de M<br/>empieza por O?"}
    N -->|no| R
    N -->|si| O["Leer F0 de nuevo<br/>P sigue seleccionada"]
    O --> Q{"F0 válido?"}
    Q -->|no| CORTO1[/"Estado = Cortocircuito<br/>fail-safe"/]
    Q -->|si| S["R_corto con la misma fórmula<br/>y F1..F3 en caché, sin calibración"]
    S --> T{"Umbral mayor que 0 y<br/>R_corto entre 0 y el umbral<br/>de la Referencia?"}
    T -->|si| CORTO1
    T -->|no| R
    CORTO1 --> R

    R["Resultado = Estado es Ok"] --> U
    NOK1 --> U["finally: restaurar<br/>S arriba=0V · M abajo=salida · S abajo=0V"]
    U --> V(["Devolver ResultadoDetalle"])
```

---

## 6. Clasificación del estado de un paso

Se aplica a la medida principal, antes de la fase de cortocircuito. Después, la fase de cortocircuito puede sobrescribir el estado con `Cortocircuito`.

```mermaid
flowchart TD
    A["Ve - Vain mayor que 1e-9?"] -->|no| AB
    A -->|si| B["R_bruta = Vain / denom * 390"]
    B --> C{"R_bruta mayor que 0<br/>y hasta R cortocircuito<br/>del modelo (1000 ohm por defecto)?"}
    C -->|no| AB
    C -->|si| D["R = Pendiente * R_bruta + Offset<br/>Pendiente 0 se trata como 1"]
    D --> E{"ResistenciaMinima mayor que 0<br/>y R menor que ResistenciaMinima?"}
    E -->|si| CC["Cortocircuito"]
    E -->|no| F{"Diferencia con Nominal<br/>dentro de la Tolerancia?"}
    F -->|si| OK["Ok"]
    F -->|no| NOK["Nok"]
    AB["Abierto<br/>R = -1, se muestra como infinito"]

    classDef ok fill:#2e7d32,stroke:#1b5e20,color:#ffffff
    classDef nok fill:#c62828,stroke:#8e0000,color:#ffffff
    classDef corto fill:#ef6c00,stroke:#b53d00,color:#ffffff
    classDef abierto fill:#1565c0,stroke:#003c8f,color:#ffffff
    class OK ok
    class NOK nok
    class CC corto
    class AB abierto
```

Color de la bola en el mapa de contactos: gris sin medir · verde `Ok` · rojo `Nok` · naranja `Cortocircuito` · azul `Abierto`.

---

## 7. Modelo de datos

Esquema creado y migrado por `TestRepository.InitializeDatabaseAsync`. `parametro_ensayo_id` **no** tiene clave foránea, y `resultados.referencia_id` y `resultados_detalle.parametro_ensayo_id` admiten `NULL` por compatibilidad con pruebas manuales.

```mermaid
erDiagram
    referencias ||--o{ parametros_ensayo : "tiene pasos"
    referencias ||--o{ resultados : "se ensaya en"
    resultados ||--o{ resultados_detalle : "contiene"
    parametros_ensayo |o..o{ resultados_detalle : "referencia lógica, sin FK"

    referencias {
        int id PK
        bool b_activa
        varchar referencia
        text descripcion
        datetime fecha_creacion
        datetime fecha_modificacion
        longblob imagen
        varchar modelo_placa "7 digitos, comando I"
        int num_mcps "0-6"
        int inh1_pos "NULL = libre"
        int inh2_pos
        int inh3_pos
        int inh4_pos
        int muestras
        int retardo_ms
        float resistencia_cortocircuito "umbral del modelo"
    }

    parametros_ensayo {
        int id PK
        int referencia_id FK
        varchar nombre_contacto
        int n_paso_ensayo
        float resistencia_nominal
        float tolerancia
        float pendiente_val
        float offset_val
        float resistencia_minima
        int mcp_arriba_chip
        int mcp_arriba_pin
        int mcp_abajo_chip
        int mcp_abajo_pin
        int canal_multiplexor "pista 0-48"
        int pos_x
        int pos_y
        datetime fecha_creacion
        datetime fecha_modificacion
    }

    resultados {
        int id PK
        int referencia_id FK "nullable"
        datetime fecha_prueba
        bool resultado
        varchar operario
        varchar lote
    }

    resultados_detalle {
        int id PK
        int resultado_id FK
        int parametro_ensayo_id "nullable, sin FK"
        varchar nombre_contacto
        int n_paso_ensayo
        float resistencia_medida "-1 si abierto"
        float resistencia_cortocircuito "-1 si abierto"
        int valor_raw_vain
        int valor_raw_ve
        bool resultado
        varchar estado_medicion "Ok Nok Cortocircuito Abierto"
        datetime timestamp_medicion
    }
```

---

## 8. Clases principales

```mermaid
classDiagram
    class ITestState {
        <<interface>>
        +TestState StateId
        +ExecuteAsync(TestContext) Task~TestState~
    }
    class InitializingState
    class RunningState
    class CompletedState
    class TestStateMachine {
        +TestState CurrentState
        +event StateChanged
        +RunAsync(...) Task~Resultado~
    }
    class TestContext {
        +Referencia Referencia
        +List~ParametroEnsayo~ Parametros
        +Resultado Resultado
        +ISerialPortService SerialPort
        +CommandParser Parser
        +int TimeoutMs
        +CancellationToken CancellationToken
        +IProgress Progress
        +Action CommandLogger
        +event StepCompleted
    }
    class TestState {
        <<enumeration>>
        Idle
        Initializing
        Running
        Completed
        Error
        Aborted
    }
    class EstadoMedicion {
        <<enumeration>>
        Ok
        Nok
        Cortocircuito
        Abierto
    }
    class ISerialPortService {
        <<interface>>
        +bool IsOpen
        +GetAvailablePorts() string[]
        +Open(port, baud) bool
        +Close()
        +SendCommandAsync(cmd, timeoutMs, ct) Task~string~
        +SendDataAsync(data, ct) Task
        +ClearBuffers()
    }
    class SerialPortService
    class CommandParser {
        +ParseFilteredValue(resp) float?
        +ParseRawValue(resp) int?
    }
    class ITestRepository {
        <<interface>>
        +GetAllReferenciasAsync()
        +GetParametrosByReferenciaAsync(id)
        +InsertResultadoAsync(resultado)
        +InsertDetalleAsync(detalle)
        +InitializeDatabaseAsync()
    }
    class TestRepository
    class Referencia
    class ParametroEnsayo
    class Resultado
    class ResultadoDetalle

    ITestState <|.. InitializingState
    ITestState <|.. RunningState
    ITestState <|.. CompletedState
    ISerialPortService <|.. SerialPortService
    ITestRepository <|.. TestRepository

    TestStateMachine o-- ITestState : por TestState
    TestStateMachine ..> TestContext : crea
    TestContext --> ISerialPortService
    TestContext --> CommandParser
    TestContext --> Referencia
    TestContext --> Resultado
    Referencia "1" --> "*" ParametroEnsayo
    Resultado "1" --> "*" ResultadoDetalle
    ResultadoDetalle --> EstadoMedicion
    ITestState ..> TestState
```

---

## 9. Navegación de la aplicación

Los paneles se crean la primera vez que se abren y después solo se muestran u ocultan. El panel automático es el que se abre al iniciar.

```mermaid
flowchart LR
    Start([Inicio]) --> F1[Form1]
    F1 -->|por defecto| A["Automático<br/>ejecutar ensayo por referencia"]
    F1 --> M["Manual<br/>comando a comando + Semiautomático"]
    F1 --> P["Parámetros<br/>referencias, pasos, mapa X/Y,<br/>import/export CSV y JSON"]
    F1 --> R["Informes<br/>histórico + exportar CSV"]
    F1 --> C["Configuración<br/>COM, BD, opciones"]
    F1 --> AB[Acerca de]
    F1 --> X([Salir])
    R --> D[Detalle por paso]
    C -.->|persiste| J[("appsettings.json")]
    P -.->|"crea referencias y pasos"| BD[("MariaDB")]
    A -.->|"lee referencias, guarda resultados"| BD
    R -.->|"lee resultados"| BD
```
