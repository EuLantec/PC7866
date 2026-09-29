# Guía de uso — Modo Manual

El modo **Manual** permite operar el banco PC7866 comando a comando: diagnosticar hardware, configurar la dirección de pines de los MCP23017, seleccionar pista de medida, activar salidas individuales, leer entradas analógicas (RAW/filtradas) y enviar la configuración de placa. Está pensado para pruebas puntuales, calibración y diagnóstico de hardware, sin necesidad de referencias ni parámetros de ensayo guardados en BD.

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

Grupo **Salidas**: representa hasta 96 salidas (6 × MCP23017 de 16 bits cada uno, chip 0-5 = direcciones I2C 0x20-0x25). Cada checkbox se etiqueta `chip.pin` con el pin mostrado 1-16 (p.ej. `2.07` = chip 2, pin 7) y equivale al bit `chip*16 + (pin-1)`.

- Marca o desmarca cualquier checkbox para activar/desactivar esa salida individual. Cada cambio envía automáticamente una trama `S<chip><estados:4hex>` solo para el chip afectado (no hace falta reenviar los demás chips).
- **Todas ON** / **Todas OFF**: activan o desactivan las 96 salidas de una vez (una trama `S` por cada uno de los 6 chips).

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

El mapa de imagen y las bolas con el nombre de cada contacto pertenecen a la configuración de referencias y al modo automático; el modo manual trabaja directamente con comandos y no carga mapas.

## 7. Configuración de placa (I)

Grupo **Config. placa**: envía la trama `I` con la configuración que luego usará el modo automático para cada referencia: **nº de MCP** activos (0-6), posición de pin (0-15, o vacío/libre) de **INH1-INH4**, **referencia** de placa (texto, informativo), **muestras** para el promedio analógico y **retardo** (ms) antes de leer tras un `F`/`R`. El botón **Enviar** construye y envía `I<numMcps><inh1><inh2><inh3><inh4><referencia:7><muestras:2><retardo:3>`.

## 8. Reset

Botón **Reset** (grupo Reset): pide confirmación y, si se acepta, envía el comando `Q` para reiniciar el microcontrolador. Tras un reset hay que volver a **Conectar**.

## 9. Log

El panel inferior muestra todas las tramas enviadas (`➡️ TX`) y recibidas (`⬅️ RX`), además de avisos y errores (timeout, puerto no abierto, etc.). Botón **Limpiar log** para vaciarlo.

## 10. Semiautomático — probar un solo contacto

Grupo **Semiautomático**: permite ejecutar el ensayo completo (resistencia + cortocircuito) de un único contacto de una referencia guardada en BD, sin lanzar el ensayo completo del modo automático ni guardar el resultado.

1. Elige el **Modelo** (Referencia) en el desplegable; al seleccionarlo se cargan sus contactos, se rellenan los campos de **Config. placa** (nº MCP, INH1-4, modelo de placa, muestras, retardo) con los valores guardados de esa referencia y se envía automáticamente la trama `I` de configuración. El botón 🔄 refresca la lista de modelos.
2. Elige el **Contacto** (paso) a probar.
3. Pulsa **▶ Probar contacto (R + Cortocircuito)**. Esto ejecuta internamente la misma máquina de estados que usa el modo automático (`TestStateMachine.RunAsync`) mediante `_stateMachine.RunAsync(...)`, con una lista de un solo paso — es decir, la secuencia de tramas, el cálculo de la resistencia y la comprobación de cortocircuito son exactamente iguales a las del [modo automático](GUIA_MODO_AUTOMATICO.md).
4. El resultado se muestra en la etiqueta inferior con el formato:
   ```
   {Contacto}: {Estado}   R = {valor} Ω   R cortocircuito = {valor} Ω
   ```
   - `R` es la resistencia principal medida del contacto; `R cortocircuito` es la resistencia calculada en la segunda fase (pin "abajo" como entrada), comparada contra `Referencia.ResistenciaCortocircuito` (umbral del modelo).
   - Cualquiera de las dos resistencias se muestra como `∞` si la resistencia bruta calculada es `≤ 0` o `> 1000 Ω` (umbral de "abierto"), en cuyo caso el estado es **Abierto**.
   - El color del texto refleja el estado: verde=Ok, rojo=Nok, naranja=Cortocircuito, azul=Abierto.
5. Este ensayo puntual **no se guarda en base de datos** — para eso usa el ensayo completo del modo automático.

## Notas

- El modo manual no requiere referencias ni parámetros de ensayo para las secciones de comandos crudos (1-9) — para pruebas de producción con criterios de OK/NOK por referencia (y guardado en BD), usa el [modo automático](GUIA_MODO_AUTOMATICO.md), que además explica en detalle técnico paso a paso la secuencia interna (`I` → `P` → `S` → `F0..F3` → cálculo de R → clasificación) que también reutiliza la sección **Semiautomático** de este modo.
