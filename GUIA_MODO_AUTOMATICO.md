# Guía de uso — Modo Automático

El modo **Automático** ejecuta el ensayo resistivo completo de una referencia, paso a paso, aplicando los parámetros (salidas, resistencia nominal, tolerancia, resistencia mínima de cortocircuito) guardados en base de datos, y clasifica cada contacto como **Ok / Nok / Cortocircuito / Abierto**.

Se accede desde el menú superior **Automático** (panel por defecto al abrir la aplicación).

## Requisito previo: referencias y parámetros

Antes de poder lanzar un ensayo automático hace falta tener, en el panel **Parámetros**:

1. Al menos una **Referencia** creada (nombre, imagen opcional para mostrar los puntos de medida).
2. Los **Parámetros de ensayo** de esa referencia: por cada paso/contacto se define la salida a activar (bits `S`, o mediante los selectores MCP arriba/abajo + canal), la resistencia nominal, la tolerancia y la resistencia mínima de cortocircuito.

Sin al menos un parámetro guardado para la referencia seleccionada, el botón **Iniciar ensayo** permanece deshabilitado.

## 1. Conexión al dispositivo

Igual que en modo manual: selecciona **Puerto** y **Baudios** y pulsa **Conectar**. El estado de conexión se muestra en `lblConnectionStatus` (verde = conectado).

## 2. Seleccionar referencia

Grupo **Ensayo**:

- **Referencia**: desplegable con las referencias existentes en BD. Botón 🔄 (**Refrescar referencias**) para recargar la lista si se creó/editó una referencia desde el panel de Parámetros mientras este panel estaba abierto.
- Al seleccionar una referencia se cargan sus parámetros de ensayo y, si tiene imagen asociada, se muestra en el panel **Imagen** con un punto de color por cada paso (gris = no medido todavía).
- **Operario**: nombre de quien ejecuta la prueba (obligatorio para iniciar).
- **Lote**: identificador de lote (opcional, se guarda junto al resultado).

## 3. Ejecutar el ensayo

- **Iniciar ensayo**: solo se habilita si hay conexión serie, una referencia seleccionada con parámetros y el campo **Operario** no vacío. Al pulsarlo:
  - Se resetean los puntos de la imagen a gris y se limpia la tabla de resultados.
  - La máquina de estados (`TestStateMachine`) recorre cada parámetro de ensayo en orden: activa la(s) salida(s) correspondiente(s) (`S`), lee la analógica filtrada (`F`), calcula `R = Vain/(Ve−Vain) × 390 − Offset` y compara contra nominal ± tolerancia y contra la resistencia mínima de cortocircuito.
  - La barra de progreso y la etiqueta de paso actual se actualizan en vivo (`lblCurrentStep`, `lblMachineState`).
- **Abortar ensayo**: cancela el ensayo en curso en cualquier momento (se marca como cancelado en el log, no se guarda resultado final).

## 4. Resultados en vivo

Por cada paso completado:

- Se pinta el punto correspondiente en la imagen de la referencia: verde = Ok, rojo = Nok/Cortocircuito/Abierto (según color asignado en la vista).
- Se añade una fila a la tabla de resultados con: número de paso, nombre de contacto, resistencia medida, nominal ± tolerancia y resultado (✅ OK / ❌ NOK), con la fila coloreada en verde o rojo.
- El log muestra la trama enviada/recibida y el resultado numérico de cada paso.

Al terminar todos los pasos (o si se aborta), se muestra el resultado global (`✅ BUENO` / `❌ MALO`, con el conteo de pasos OK) y, si hay conexión a BD, el resultado y su detalle se guardan automáticamente:

- Tabla `resultados`: cabecera del ensayo (referencia, fecha, operario, lote, resultado global).
- Tabla `resultados_detalle`: una fila por paso con resistencia medida, valores RAW, y estado (`Ok`/`Nok`/`Cortocircuito`/`Abierto`).

## 5. Consultar el histórico

Los resultados guardados se pueden revisar más tarde desde el panel **Informes**, incluyendo el detalle por paso de cada ensayo (`ResultadoDetalleForm`).

## Notas

- Si la BD no está disponible al abrir el panel, se registra un aviso en el log pero el panel sigue funcionando en local (no se podrán cargar referencias ni guardar resultados hasta restablecer la conexión).
- El modo automático no permite operar salidas individuales fuera del flujo del ensayo — para eso usa el [modo manual](GUIA_MODO_MANUAL.md).
