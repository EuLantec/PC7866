# Ejemplo de tramas serie enviadas (5 puntos de ensayo)

Ejemplo con una referencia de solo **5 pasos** (`NPasoEnsayo` 1-5), `NumMcps = 2` (chips `0` y `1`, direcciones I2C 0x20/0x21), para ver exactamente qué comandos manda `RunningState` en cada paso.

El ensayo automático es **punto a punto**: primero se pone toda la placa a 0V y luego, por cada paso, se mide su resistencia, se comprueba su cortocircuito y se restaura ese paso antes de continuar con el siguiente (no hay dos pasadas globales).

## Datos de partida (ParametroEnsayo)

| Paso | Contacto | Canal (`P`) | Arriba (chip.pin) | Abajo (chip.pin) |
|------|----------|-------------|--------------------|--------------------|
| 1 | Pin1 | 01 | 0.1 | 0.2 |
| 2 | Pin2 | 02 | 0.3 | 0.4 |
| 3 | Pin3 | 03 | 0.5 | 1.1 |
| 4 | Pin4 | 04 | 1.2 | 1.3 |
| 5 | Pin5 | 05 | 1.4 | 1.5 |

(`chip` es 0-based: 0→0x20, 1→0x21; `pin` es 1-16, no existe el pin 0. Bit interno = `chip*16 + (pin-1)`.)

## Configuración inicial (una sola vez)

Todos los MCP de la placa (`0..NumMcps-1`, se usen o no) se configuran completos como **salida** (máscara `FFFF`) y se ponen a **0V/LOW**, dejando el banco entero a masa:

```
M0SFFFF      // chip0: los 16 pines como SALIDA
S00000       // chip0: todo a 0V
M1SFFFF      // chip1: los 16 pines como SALIDA
S10000       // chip1: todo a 0V
```

## Por cada paso

Cada paso hace tres bloques: **resistencia**, **cortocircuito** y **restaurar**.

### Paso 1 (Pin1: arriba 0.1, abajo 0.2 — ambos en chip0)

```
// Resistencia: "arriba" a 5V ("abajo" ya está a 0V), seleccionar pista y leer analógicas
S00001       // chip0 bit0 (arriba, pin1) → HIGH/5V
P01          // seleccionar pista 1
F0           // F0 varía con cada resistencia
F1  F2  F3   // SOLO en el primer paso: F1/F2/F3 son fijos en automático → se cachean y reutilizan

// Cortocircuito: "abajo" pasa a entrada (alta impedancia); "arriba" sigue a 5V, la pista sigue en P01
M0E0002      // chip0 bit1 (abajo, pin2) → ENTRADA
F0           // leer tensión; si < 4,5V → Cortocircuito

// Restaurar el paso
S00000       // arriba → 0V
M0S0002      // abajo → SALIDA de nuevo
S00000       // abajo → 0V
```

Con `F0..F3` se calcula `Vain = F0-F1`, `Ve = F2-F3`, `R_bruta = Vain/(Ve-Vain)×390`. El abierto/cortocircuito se detecta sobre `R_bruta` (igual que el modo manual) y solo a las lecturas válidas se les aplica la calibración lineal `R = Pendiente×R_bruta + Offset` (`Pendiente=1`/`Offset=0` por defecto dejan el valor bruto).

### Paso 2 (Pin2: arriba 0.3, abajo 0.4 — ambos en chip0)

```
S00004       // arriba (chip0 bit2, pin3) → 5V
P02
F0           // F1/F2/F3 ya están cacheados, no se vuelven a leer
M0E0008      // abajo (chip0 bit3, pin4) → ENTRADA
F0
S00000       // arriba → 0V
M0S0008      // abajo → SALIDA
S00000       // abajo → 0V
```

### Paso 3 (Pin3: arriba 0.5, abajo 1.1 — arriba en chip0, abajo en chip1)

```
S00010       // arriba (chip0 bit4, pin5) → 5V
P03
F0
M1E0001      // abajo está en chip1 (bit0, pin1) → ENTRADA
F0
S00000       // arriba (chip0) → 0V
M1S0001      // abajo (chip1) → SALIDA
S10000       // abajo (chip1) → 0V
```

### Paso 4 (Pin4: arriba 1.2, abajo 1.3 — ambos en chip1)

```
S10002       // arriba (chip1 bit1, pin2) → 5V
P04
F0
M1E0004      // abajo (chip1 bit2, pin3) → ENTRADA
F0
S10000       // arriba → 0V
M1S0004      // abajo → SALIDA
S10000       // abajo → 0V
```

### Paso 5 (Pin5: arriba 1.4, abajo 1.5 — ambos en chip1)

```
S10008       // arriba (chip1 bit3, pin4) → 5V
P05
F0
M1E0010      // abajo (chip1 bit4, pin5) → ENTRADA
F0
S10000       // arriba → 0V
M1S0010      // abajo → SALIDA
S10000       // abajo → 0V
```

## Cierre del ensayo

```
P00          // desconectar multiplexor de medida
S00000       // chip0 → todas las salidas a 0V
S10000       // chip1 → todas las salidas a 0V
```

## Notas

- El tiempo de asentamiento entre cambiar el estado eléctrico (`M`/`S`/`P`) y leer se controla con la constante `SETTLE_DELAY_MS` de `RunningState.cs`. Si las lecturas salieran inestables (R≈0 o falso cortocircuito), subir ese valor.
- El umbral de cortocircuito es la constante `CORTOCIRCUITO_VOLTAGE_THRESHOLD` (4,5V) de `RunningState.cs`.
- El paso siempre se restaura en un bloque `finally`, incluso si hubo una excepción durante la medición, para no dejar salidas activas entre contactos.

> Referencia de tramas: [Pc7866Commands.cs](Models/Pc7866Commands.cs). Lógica completa: [RunningState.cs](Services/StateMachine/States/RunningState.cs). Detalle paso a paso: [GUIA_MODO_AUTOMATICO.md](GUIA_MODO_AUTOMATICO.md).
