# Guía de uso — Modo Manual

El modo **Manual** permite operar el banco PC7866 comando a comando: activar salidas individuales, leer entradas analógicas, ajustar filtros/coeficientes y ejecutar un test completo de las 48 salidas. Está pensado para pruebas puntuales, calibración y diagnóstico de hardware, sin necesidad de referencias ni parámetros de ensayo guardados en BD.

Se accede desde el menú superior **Manual**.

## 1. Conexión al dispositivo

En la barra superior del panel:

1. Selecciona el **Puerto** COM (usa el botón 🔄 para refrescar la lista si el dispositivo se conectó después de abrir la app).
2. Selecciona los **Baudios** (por defecto se usa el valor de `appsettings.json`, típicamente 115200).
3. Pulsa **Conectar**. El indicador de estado se pone en verde (`● COMx`) si la apertura del puerto fue correcta.
4. Pulsa **Desconectar** para liberar el puerto.

Mientras no haya conexión, el resto de secciones (Diagnosis, Salidas, Analógica) permanecen deshabilitadas.

## 2. Diagnosis

Grupo **Diagnosis**: envía el comando `D` al dispositivo.

- **Diagnosis total**: ejecuta el diagnóstico completo.
- **Diag 1 / Diag 2 / Diag 3 / Diag 4**: ejecutan un sub-diagnóstico individual.

La trama enviada y la respuesta (`O`=OK / `N`=NOK) se muestran en el **Log** inferior.

## 3. Salidas (matriz de 48 checkboxes)

Grupo **Salidas**: representa las 48 salidas del banco (`S01`…`S48`, 3 × MCP23017 de 16 bits cada uno).

- Marca o desmarca cualquier checkbox para activar/desactivar esa salida individual. Cada cambio envía automáticamente el comando `S` con la trama de 12 dígitos hexadecimales (48 bits) que se muestra en **Trama:**.
- **Todas ON** / **Todas OFF**: activan o desactivan las 48 salidas de una vez.
- **Test completo**: recorre las 48 salidas una a una (activa solo esa salida, lee `R` y `F`, calcula la resistencia) y muestra el resultado de cada una en el log. Útil para verificar todo el banco sin tener que ir salida por salida a mano.

## 4. Lectura analógica y cálculo de resistencia

Grupo **Analógica**:

- **Leer RAW**: envía `R`, lectura cruda de los 4 canales analógicos (sin filtrar).
- **Leer filtrada**: envía `F`, lectura filtrada de los 4 canales y calcula automáticamente:
  - `Vain = canal1 − canal2`
  - `Ve = canal3 − canal4`
  - `R = Vain / (Ve − Vain) × 390 Ω`
  - Los valores de `Vain`, `Ve`, `Ve−Vain` y la resistencia resultante se muestran en el panel de resultado (R = ∞ si la salida está abierta o fuera de rango 0–1000 Ω).

Esta es la misma fórmula que usa el modo automático para evaluar cada paso del ensayo.

## 5. Filtros y coeficientes

Grupo **Filtro**:

- **Flags de filtro**: campo hexadecimal + botón para enviar `I` con el sub-comando de flags.
- **Coeficientes (C1…C10)**: cada fila tiene un campo de texto (admite valor decimal con `.`/`,` o hexadecimal directo) y un botón para enviarlo individualmente mediante `I`.

## 6. Guardar / leer parámetros en memoria no volátil

Grupo **Guardar** (comando `G`):

- **Escribir**: graba en memoria no volátil los flags/coeficientes actuales.
- **Leer**: carga desde memoria no volátil hacia RAM.
- **Ver**: lee el estado actual (flags + 3 coeficientes) y rellena los campos de la sección de Filtro con los valores leídos del dispositivo.

Tras **Escribir** o **Leer**, la app ejecuta automáticamente **Ver** para refrescar los campos con el valor real que quedó en el dispositivo.

## 7. Reset

Botón **Reset** (grupo Reset): pide confirmación y, si se acepta, envía el comando `Q` para reiniciar el microcontrolador. Tras un reset hay que volver a **Conectar**.

## 8. Log

El panel inferior muestra todas las tramas enviadas (`➡️ TX`) y recibidas (`⬅️ RX`), además de avisos y errores (timeout, puerto no abierto, etc.). Botón **Limpiar log** para vaciarlo.

## Notas

- El resultado del **Test completo** se intenta guardar en BD (tabla `resultados`/`resultados_detalle`) sin referencia asociada (`ReferenciaId = null`), solo si hay conexión de BD configurada y disponible; si falla, se registra un aviso en el log y la operación no se interrumpe.
- El modo manual no requiere referencias ni parámetros de ensayo — para pruebas de producción con criterios de OK/NOK por referencia, usa el [modo automático](GUIA_MODO_AUTOMATICO.md).
