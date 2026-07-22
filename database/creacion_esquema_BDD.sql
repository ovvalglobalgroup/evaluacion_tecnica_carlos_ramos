-- Creación de la base de datos
CREATE DATABASE IF NOT EXISTS banco_codesarrollo
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE banco_codesarrollo;


-- Creación de la tabla de tipos de garantía
CREATE TABLE IF NOT EXISTS tipo_garantia
(
    tipo_garantia TINYINT UNSIGNED NOT NULL,
    nombre_garantia VARCHAR(30) NOT NULL,

    CONSTRAINT pk_tipo_garantia
        PRIMARY KEY (tipo_garantia),

    CONSTRAINT uq_tipo_garantia_nombre
        UNIQUE (nombre_garantia)
) ENGINE = InnoDB;


-- Creación de la tabla de créditos bancarios
CREATE TABLE IF NOT EXISTS credito
(
    numero_credito BIGINT UNSIGNED NOT NULL,
    estatus_credito ENUM('VIGENTE', 'CANCELADO') NOT NULL,
    tipo_garantia TINYINT UNSIGNED NOT NULL,
    sucursal INT UNSIGNED NOT NULL,

    CONSTRAINT pk_credito
        PRIMARY KEY (numero_credito, sucursal),

    CONSTRAINT fk_credito_tipo_garantia
        FOREIGN KEY (tipo_garantia)
        REFERENCES tipo_garantia (tipo_garantia)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
) ENGINE = InnoDB;


-- Creación de la tabla de cuotas de los créditos
CREATE TABLE IF NOT EXISTS cuota_credito
(
    numero_credito BIGINT UNSIGNED NOT NULL,
    numero_cuota INT UNSIGNED NOT NULL,
    fecha_vencimiento DATE NOT NULL,

    -- UNSIGNED impide almacenar valores negativos.
    capital DECIMAL(14, 2) UNSIGNED NOT NULL,
    interes DECIMAL(14, 2) UNSIGNED NOT NULL DEFAULT 0.00,
    mora DECIMAL(14, 2) UNSIGNED NOT NULL DEFAULT 0.00,

    pagada BOOLEAN NOT NULL DEFAULT FALSE,
    sucursal INT UNSIGNED NOT NULL,

    CONSTRAINT pk_cuota_credito
        PRIMARY KEY (numero_credito, numero_cuota, sucursal),

    CONSTRAINT fk_cuota_credito_credito
        FOREIGN KEY (numero_credito, sucursal)
        REFERENCES credito (numero_credito, sucursal)
        ON UPDATE CASCADE
        ON DELETE RESTRICT

) ENGINE = InnoDB;