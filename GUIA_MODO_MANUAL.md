# Guía de uso — Modo Manual

El modo **Manual** permite operar el banco PC7866 comando a comando: diagnosticar hardware, configurar la dirección de pines de los MCP23017, seleccionar pista de medida, activar salidas individuales, leer entradas analógicas (RAW/filtradas), enviar la configuración de placa y ejecutar un test completo de las 96 salidas. Está pensado para pruebas puntuales, calibración y diagnóstico de hardware, sin necesidad de referencias ni parámetros de ensayo guardados en BD.

Se accede desde el menú superior **Manual**.

## 1. Conexión al dispositivo

En la barra superior del panel:

1. Selecciona el **Puerto** COM (usa el botón 🔄 para refrescar la lista si el dispositivo se conectó después de abrir la app).
2. Selecciona los **Baudios** (por defecto se usa el valor de `appsettings.json`, típicamente 115200).
3. Pulsa **Conectar**. El indicador de estado se pone en verde (`● COMx`) si la apertura del puerto fue correcta.
4. Pulsa **Desconectar** para liberar el puerto.

Mientras no haya conexión, el resto de secciones (Diagnosis, Modo MCP, Pista, Salidas, Analógica, Config. placa, Reset) permanecen deshabilitadas.

## 2. Diagnosis

Grupo **Diagnosis**: envía el comando `D` al dispositivo.

- **Diagnosis total**: ejecuta el diagnóstico completo (`DT`).
- **ADS1115**: diagnóstico del conversor analógico 0x48 (`D1`).
- **MCP 0-5**: un botón por cada uno de los 6 posibles MCP23017 (0x20-0x25), envía `D2`..`D7`.
- **Versión**: lee la versión de compilación del firmware (`DV`).
- **Leer config.**: lee la configuración I2C actual (`DG`).
- **Temperatura**: lee la temperatura (`DC`).

La trama enviada y la respuesta (`O`=OK / `N`=NOK) se muestran en el **Log** inferior.

## 3. Configuración de dirección de pines (M)

Grupo **Modo MCP**: selecciona un chip (0-5, con su dirección I2C 0x20-0x25), el modo (Entrada/Salida) y una máscara de 16 bits en hexadecimal (1 = aplicar el modo a ese pin, 0 = no modificar). El botón **Enviar** construye y envía la trama `M<chip><E|S><máscara:4hex>`.

## 4. Selección de pista de medida (P)

Grupo **Pista**: número de pista (0-48) a conectar en los multiplexores analógicos 74HC4067. El botón **Seleccionar** envía `Pnn`. `0` desconecta el punto común de medida.

## 5. Salidas (matriz de 96 checkboxes)

Grupo **Salidas**: representa hasta 96 salidas (6 × MCP23017 de 16 bits cada uno). Cada checkbox se etiqueta `chip.pin` (p.ej. `2.07` = chip 2, pin 7) y equivale al bit `chip*16 + pin`.

- Marca o desmarca cualquier checkbox para activar/desactivar esa salida individual. Cada cambio envía automáticamente una trama `S<chip><estados:4hex>` solo para el chip afectado (no hace falta reenviar los demás chips).
- **Todas ON** / **Todas OFF**: activan o desactivan las 96 salidas de una vez (una trama `S` por cada uno de los 6 chips).
- **Test completo**: recorre las 96 salidas una a una (activa solo esa salida, lee `F0..F3`, calcula la resistencia) y muestra el resultado de cada una en el log. Útil para verificar todo el banco sin tener que ir salida por salida a mano.

## 6. Lectura analógica y cálculo de resistencia

Grupo **Analógica**: el canal (0-3) se elige en el desplegable **Canal**.

- **Leer RAW**: envía `R<canal>`, lectura cruda del canal analógico seleccionado (sin filtrar).
- **Leer filtrada**: envía `F<canal>`, lectura filtrada (en voltios) del canal seleccionado.
- **Leer todo + calcular R**: envía `F0`, `F1`, `F2`, `F3` en secuencia y calcula automáticamente:
  - `Vain = canal0 − canal1`
  - `Ve = canal2 − canal3`
  - `R = Vain / (Ve − Vain) × 390 Ω`
  - Los valores de `Vain`, `Ve`, `Ve−Vain` y la resistencia resultante se muestran en el panel de resultado (R = ∞ si la salida está abierta o fuera de rango 0–1000 Ω).

Esta es la misma fórmula (y la misma secuencia F0-F3) que usa el modo automático para evaluar cada paso del ensayo.

## 7. Configuración de placa (I)

Grupo **Config. placa**: envía la trama `I` con la configuración que luego usará el modo automático para cada referencia: **nº de MCP** activos (0-6), posición de pin (0-15, o vacío/libre) de **INH1-INH4**, **referencia** de placa (texto, informativo), **muestras** para el promedio analógico y **retardo** (ms) antes de leer tras un `F`/`R`. El botón **Enviar** construye y envía `I<numMcps><inh1><inh2><inh3><inh4><referencia:7><muestras:2><retardo:3>`.

## 8. Reset

Botón **Reset** (grupo Reset): pide confirmación y, si se acepta, envía el comando `Q` para reiniciar el microcontrolador. Tras un reset hay que volver a **Conectar**.

## 9. Log

El panel inferior muestra todas las tramas enviadas (`➡️ TX`) y recibidas (`⬅️ RX`), además de avisos y errores (timeout, puerto no abierto, etc.). Botón **Limpiar log** para vaciarlo.

## Notas

- El resultado del **Test completo** se intenta guardar en BD (tabla `resultados`/`resultados_detalle`) sin referencia asociada (`ReferenciaId = null`), solo si hay conexión de BD configurada y disponible; si falla, se registra un aviso en el log y la operación no se interrumpe.
- El modo manual no requiere referencias ni parámetros de ensayo — para pruebas de producción con criterios de OK/NOK por referencia, usa el [modo automático](GUIA_MODO_AUTOMATICO.md), que además explica en detalle técnico paso a paso la secuencia interna (`I` → `P` → `S` → `F0..F3` → cálculo de R → clasificación).
