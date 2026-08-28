# Ejemplo de tramas serie enviadas (5 puntos de ensayo)

Ejemplo con una referencia de solo **5 pasos** (`NPasoEnsayo` 1-5), `NumMcps = 2` (chips `0` y `1`, direcciones I2C 0x20/0x21), para ver exactamente qué comandos manda `RunningState` en cada fase.

## Datos de partida (ParametroEnsayo)

| Paso | Contacto | Canal (`P`) | Arriba (chip.pin) | Abajo (chip.pin) |
|------|----------|-------------|--------------------|--------------------|
| 1 | Pin1 | 01 | 1.0 | 1.1 |
| 2 | Pin2 | 02 | 1.2 | 1.3 |
| 3 | Pin3 | 03 | 1.4 | 2.0 |
| 4 | Pin4 | 04 | 2.1 | 2.2 |
| 5 | Pin5 | 05 | 2.3 | 2.4 |

(`chip` es 1-based: 1→0x20, 2→0x21; `pin` es 0-15).

## Fase A — Resistencia

**1. Configuración global (una sola vez)** — todos los "arriba" a salida, todos los "abajo" a salida, luego "arriba" a 5V y "abajo" a 0V:

```
M0S001F      // chip0: pines 0,1,2,3,4 como SALIDA (máscara 0x1F = arriba{0,2,4} | abajo{1,3})
M1S001F      // chip1: pines 0,1,2,3,4 como SALIDA (máscara 0x1F = arriba{1,3} | abajo{0,2,4})
S00015       // chip0: arriba a HIGH → pines 0,2,4 = 1 (0x15)
S1000A       // chip1: arriba a HIGH → pines 1,3 = 1 (0x0A)
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
M0S001F      // (misma máscara que antes, ya están como salida)
M1S001F
S00000       // chip0 → todo a 0V
S10000       // chip1 → todo a 0V
```

**2. Por cada paso**: su "abajo" pasa a entrada, su "arriba" a 5V, se selecciona pista y se lee `F0`; al terminar se restaura:

```
// Paso 1 (Pin1: arriba 1.0, abajo 1.1)
M0E0002      // chip0 pin1 (abajo) → ENTRADA
S00001       // chip0 pin0 (arriba) → HIGH
P01
F0
S00000       // arriba → 0V
M0S0002      // abajo → SALIDA de nuevo
S00000       // abajo → 0V

// Paso 2 (Pin2: arriba 1.2, abajo 1.3)
M0E0008
S00004
P02
F0
S00000
M0S0008
S00000

// Paso 3 (Pin3: arriba 1.4, abajo 2.0)
M1E0001      // abajo está en chip1 (pin0)
S00010       // arriba en chip0 (pin4)
P03
F0
S00000
M1S0001
S10000

// Paso 4 (Pin4: arriba 2.1, abajo 2.2)
M1E0004
S10002
P04
F0 
S10000
M1S0004
S10000

// Paso 5 (Pin5: arriba 2.3, abajo 2.4)
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
