# PC7866 - Test resistivo Embega

Aplicacion Windows Forms para ejecutar ensayos resistivos del banco PC7866.

La aplicacion permite:
- Conectar por puerto serie al dispositivo.
- Ejecutar pruebas manuales y automaticas.
- Gestionar referencias y parametros de ensayo.
- Guardar y consultar resultados en MariaDB.

## Estado actual

El proyecto ya incluye funcionalidad operativa en:
- Modo manual: diagnosis, activacion de salidas, lecturas analogicas y test completo.
- Modo automatico: ejecucion por pasos con maquina de estados y progreso visual.
- Parametros: gestion de referencias y parametros de ensayo.
- Informes: consulta de historico y detalle de resultados.
- Configuracion: puerto serie, conexion BD y opciones generales mediante `appsettings.json`.

## Tecnologias

- .NET SDK: `net10.0-windows`
- UI: Windows Forms
- Base de datos: MariaDB / MySQL
- Acceso a datos: Dapper + MySqlConnector

Dependencias NuGet principales:
- `Dapper`
- `MySqlConnector`
- `System.IO.Ports`
- `PdfSharpCore`
- `SixLabors.ImageSharp`

## Requisitos

- Windows 10/11
- .NET SDK compatible con `net10.0-windows`
- MariaDB o MySQL accesible desde el PC
- Dispositivo conectado por puerto COM

## Puesta en marcha

1. Restaurar paquetes:

```bash
dotnet restore
```

2. Compilar:

```bash
dotnet build
```

3. Ejecutar:

```bash
dotnet run
```

## Configuracion

La configuracion se persiste en `appsettings.json` junto al ejecutable.

Ejemplo de claves principales:

```json
{
  "DefaultPortName": "COM3",
  "DefaultBaudRate": 115200,
  "DefaultTimeout": 5000,
  "DatabaseServer": "127.0.0.1",
  "DatabaseName": "parensayos",
  "DatabaseUser": "of7866",
  "DatabasePassword": "<valor cifrado DPAPI>",
  "DatabasePort": 3306,
  "MaxRetries": 3,
  "DelayBetweenCommandsMs": 100,
  "AutoSaveResults": true,
  "ApplicationTitle": "PC7866 - Test Resistivo Embega",
  "ShowDetailedLogs": true
}
```

Notas:
- La contrasena de BD se guarda cifrada con DPAPI a nivel de usuario Windows.
- Si se usa un fichero antiguo con password en texto plano, al guardar configuracion se reescribe cifrado.

## Protocolo serie (resumen)

Comandos principales:
- `D`: Diagnosis
- `S`: Activacion de salidas (48 bits, 12 hex)
- `R`: Lectura analogica RAW
- `F`: Lectura analogica filtrada
- `I`: Escritura de flags/coeficientes
- `G`: Guardar/cargar/ver parametros en memoria no volatil
- `Q`: Reset del microcontrolador

Respuestas:
- `O`: OK
- `N`: NOK

En modo manual y automatico se utilizan los builders de `Pc7866Commands` para construir tramas validas.

## Estructura del proyecto

- `Configuration/`: configuracion global y persistencia.
- `Models/`: modelos de dominio, comandos y resultados.
- `Services/SerialCommunication/`: puerto serie y parser de respuestas.
- `Services/Database/`: repositorio Dapper y creacion de esquema.
- `Services/StateMachine/`: ejecucion del ensayo automatico por estados.
- `Views/`: paneles UI (manual, automatico, parametros, informes).
- `Utils/`: utilidades como logging.

## Base de datos

Al iniciar, el repositorio puede crear/asegurar estas tablas:
- `referencias`
- `parametros_ensayo`
- `resultados`
- `resultados_detalle`

Detalles relevantes:
- Se usan claves foraneas entre resultados, detalle y referencias.
- `resultados.referencia_id` y `resultados_detalle.parametro_ensayo_id` se migran a nullable para compatibilidad con tests manuales.
- Para circuito abierto se guarda `-1` en `resistencia_medida` y se representa como infinito en UI.

## Flujo de uso recomendado

1. Configurar puerto serie y conexion BD en configuracion.
2. Crear o revisar referencias y parametros de ensayo.
3. Ejecutar test automatico con operario y lote.
4. Revisar resultado global y detalle por paso.
5. Consultar historico en informes.

## Solucion de problemas

- No aparecen puertos COM:
  - Verificar cable/controlador USB-Serial.
  - Pulsar refresco de puertos en panel manual/automatico.
- Error de conexion a BD:
  - Revisar host, puerto, usuario y password en configuracion.
  - Confirmar permisos de usuario sobre el esquema.
- Tramas NOK o timeout:
  - Validar baudrate y puerto.
  - Probar diagnosis en modo manual antes de ejecutar automatico.

## Documentacion adicional

- Arquitectura tecnica detallada: `ARCHITECTURE.md`
