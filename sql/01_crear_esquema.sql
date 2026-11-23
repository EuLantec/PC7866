-- Esquema completo de la base de datos PC7866 (MariaDB 10.x / MySQL 8+).
-- Equivale al estado final que deja InitializeDatabaseAsync (Services/Database/TestRepository.cs)
-- tras aplicar todas sus migraciones. Uso para un PC nuevo:
--     mysql -u root -p < sql/01_crear_esquema.sql
-- Es idempotente: se puede ejecutar varias veces sin perder datos.
-- Si el nombre de la base de datos en appsettings.json (DatabaseName) es otro, cambiarlo aquí.

CREATE DATABASE IF NOT EXISTS `parensayos` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `parensayos`;

-- Referencia (modelo) + configuración de placa.
-- resistencia_cortocircuito: umbral de cortocircuito del modelo (Ω). También es el valor a partir
-- del cual la resistencia se considera infinita (abierto). Si es 0 se usa 1000 Ω por defecto.
CREATE TABLE IF NOT EXISTS referencias (
    id                        INT AUTO_INCREMENT PRIMARY KEY,
    b_activa                  BOOLEAN      NOT NULL DEFAULT TRUE,
    referencia                VARCHAR(255) NOT NULL,
    descripcion               TEXT,
    fecha_creacion            DATETIME     NOT NULL,
    fecha_modificacion        DATETIME     NOT NULL,
    imagen                    LONGBLOB,
    modelo_placa              VARCHAR(10)  NOT NULL DEFAULT '',
    num_mcps                  INT          NOT NULL DEFAULT 6,
    inh1_pos                  INT          NULL,
    inh2_pos                  INT          NULL,
    inh3_pos                  INT          NULL,
    inh4_pos                  INT          NULL,
    muestras                  INT          NOT NULL DEFAULT 1,
    retardo_ms                INT          NOT NULL DEFAULT 0,
    resistencia_cortocircuito FLOAT        NOT NULL DEFAULT 0
);

-- Un registro por contacto/paso de ensayo de una referencia.
CREATE TABLE IF NOT EXISTS parametros_ensayo (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    referencia_id       INT         NOT NULL,
    nombre_contacto     VARCHAR(20) NOT NULL,
    n_paso_ensayo       INT         NOT NULL,
    resistencia_nominal FLOAT       NOT NULL DEFAULT 0,
    tolerancia          FLOAT       NOT NULL DEFAULT 0,
    pendiente_val       FLOAT       NOT NULL DEFAULT 1,
    offset_val          FLOAT       NOT NULL DEFAULT 0,
    resistencia_minima  FLOAT       NOT NULL DEFAULT 0,
    mcp_arriba_chip     INT         NOT NULL DEFAULT 0,
    mcp_arriba_pin      INT         NOT NULL DEFAULT 0,
    mcp_abajo_chip      INT         NOT NULL DEFAULT 0,
    mcp_abajo_pin       INT         NOT NULL DEFAULT 0,
    canal_multiplexor   INT         NOT NULL DEFAULT 0,
    fecha_creacion      DATETIME    NOT NULL,
    fecha_modificacion  DATETIME    NOT NULL,
    pos_x               INT         NOT NULL DEFAULT 0,
    pos_y               INT         NOT NULL DEFAULT 0,
    FOREIGN KEY (referencia_id) REFERENCES referencias(id)
);

-- Cabecera de cada ensayo. referencia_id queda a NULL si se borra la referencia.
CREATE TABLE IF NOT EXISTS resultados (
    id            INT AUTO_INCREMENT PRIMARY KEY,
    referencia_id INT          NULL,
    fecha_prueba  DATETIME     NOT NULL,
    resultado     BOOLEAN      NOT NULL,
    operario      VARCHAR(100),
    lote          VARCHAR(100),
    FOREIGN KEY (referencia_id) REFERENCES referencias(id)
);

-- Detalle por contacto. parametro_ensayo_id es una referencia lógica (sin FK) y puede ser NULL.
-- resistencia_medida / resistencia_cortocircuito: -1 si la lectura es abierta (infinito).
-- estado_medicion: Ok | Nok | Cortocircuito | Abierto.
CREATE TABLE IF NOT EXISTS resultados_detalle (
    id                        INT AUTO_INCREMENT PRIMARY KEY,
    resultado_id              INT         NOT NULL,
    parametro_ensayo_id       INT         NULL,
    nombre_contacto           VARCHAR(20),
    n_paso_ensayo             INT         NOT NULL,
    resistencia_medida        FLOAT       NOT NULL,
    resistencia_cortocircuito FLOAT       NOT NULL DEFAULT -1,
    valor_raw_vain            INT         NOT NULL DEFAULT 0,
    valor_raw_ve              INT         NOT NULL DEFAULT 0,
    resultado                 BOOLEAN     NOT NULL,
    estado_medicion           VARCHAR(20) NOT NULL DEFAULT 'Nok',
    timestamp_medicion        DATETIME    NOT NULL,
    FOREIGN KEY (resultado_id) REFERENCES resultados(id)
);
