# Ejemplo de tramas serie enviadas (5 puntos de ensayo)

Ejemplo con una referencia de solo **5 pasos** (`NPasoEnsayo` 1-5), `NumMcps = 2` (chips `0` y `1`, direcciones I2C 0x20/0x21), para ver exactamente qué comandos manda `RunningState` en cada fase.

## Datos de partida (ParametroEnsayo)

| Paso | Contacto | Canal (`P`) | Arriba (chip.pin) | Abajo (chip.pin) |
|------|----------|-------------|--------------------|--------------------|
| 1 | Pin1 | 01 | 0.1 | 0.2 |
| 2 | Pin2 | 02 | 0.3 | 0.4 |
| 3 | Pin3 | 03 | 0.5 | 1.1 |
| 4 | Pin4 | 04 | 1.2 | 1.3 |
| 5 | Pin5 | 05 | 1.4 | 1.5 |

(`chip` es 0-based: 0→0x20, 1→0x21; `pin` es 1-16, no existe el pin 0. Bit interno = `chip*16 + (pin-1)`.)

## Fase A — Resistencia

**1. Configuración global (una sola vez)** — todos los chips en uso se configuran completos como salida (máscara `FFFF`, independientemente de cuántos pines usen), luego "arriba" a 5V y "abajo" a 0V (los pines no usados de ese chip quedan a 0V por defecto):

```
M0SFFFF      // chip0: los 16 pines como SALIDA
M1SFFFF      // chip1: los 16 pines como SALIDA
S00015       // chip0: arriba a HIGH → bits 0,2,4 (pines 1,3,5) = 1 (0x15)
S1000A       // chip1: arriba a HIGH → bits 1,3 (pines 2,4) = 1 (0x0A)
```

**2. Por cada paso**: seleccionar pista y leer las 4 analógicas filtradas:

```
P01  F0  F1  F2  F3    // paso 1 (Pin1)
P02  F0  F1  F2  F3    // paso 2 (Pin2)
P03  F0  F1  F2  F3    // paso 3 (Pin3)
P04  F0  F1  F2  F3    // paso 4 (Pin4)
P05  F0  F1  F2  F3    // paso 5 (Pin5)
```

Con cada `F0..F3` se calcula `Vain = F0-F1`, `Ve = F2-F3`, `R = Vain/(Ve-Vain)×390 - Offset`.

## Fase B — Cortocircuito

**1. Configuración global (una sola vez)** — todo "arriba"+"abajo" a salida y a 0V (todo a masa):

```
M0SFFFF      // (misma máscara que antes, ya están como salida)
M1SFFFF
S00000       // chip0 → todo a 0V
S10000       // chip1 → todo a 0V
```

**2. Por cada paso**: su "abajo" pasa a entrada, su "arriba" a 5V, se selecciona pista y se lee `F0`; al terminar se restaura:

```
// Paso 1 (Pin1: arriba 0.1, abajo 0.2)
M0E0002      // chip0 bit1 (abajo, pin2) → ENTRADA
S00001       // chip0 bit0 (arriba, pin1) → HIGH
P01
F0
S00000       // arriba → 0V
M0S0002      // abajo → SALIDA de nuevo
S00000       // abajo → 0V

// Paso 2 (Pin2: arriba 0.3, abajo 0.4)
M0E0008
S00004
P02
F0
S00000
M0S0008
S00000

// Paso 3 (Pin3: arriba 0.5, abajo 1.1)
M1E0001      // abajo está en chip1 (bit0, pin1)
S00010       // arriba en chip0 (bit4, pin5)
P03
F0
S00000
M1S0001
S10000

// Paso 4 (Pin4: arriba 1.2, abajo 1.3)
M1E0004
S10002
P04
F0 
S10000
M1S0004
S10000

// Paso 5 (Pin5: arriba 1.4, abajo 1.5)
M1E0010
S10008
P05
F0
S10000
M1S0010
S10000
```

Si la lectura de `F0` cae por debajo de **2,5V** (`CORTOCIRCUITO_VOLTAGE_THRESHOLD`), ese paso se marca `Cortocircuito`.

## Cierre del ensayo

```
P00          // desconectar multiplexor
S00000       // chip0 → todas las salidas a 0V
S10000       // chip1 → todas las salidas a 0V
```

> Referencia de tramas: [Pc7866Commands.cs](Models/Pc7866Commands.cs). Lógica completa: [RunningState.cs](Services/StateMachine/States/RunningState.cs). Detalle paso a paso: [GUIA_MODO_AUTOMATICO.md](GUIA_MODO_AUTOMATICO.md).
