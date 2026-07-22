-- Redirigimos la ejecucón de los script hacia la base de datos
USE banco_desarrollo;


-- Registramos los tipos de garantía que podrán asociarse a los créditos
INSERT INTO tipo_garantia (tipo_garantia, nombre_garantia)
VALUES
    (1, 'PRENDARIA'),
    (2, 'QUIROGRAFARIA')
ON DUPLICATE KEY UPDATE
    nombre_garantia = VALUES(nombre_garantia);


-- Insertamos varios créditos con diferentes estados y tipos de garantía
INSERT INTO credito (numero_credito, estatus_credito, tipo_garantia, sucursal)
VALUES
    (1001, 'VIGENTE',   1, 1),
    (1002, 'VIGENTE',   1, 1),
    (1003, 'VIGENTE',   2, 1),
    (1004, 'CANCELADO', 1, 1)
ON DUPLICATE KEY UPDATE
    estatus_credito = VALUES(estatus_credito),
    tipo_garantia = VALUES(tipo_garantia);


-- Eliminamos previamente las cuotas de prueba asociadas a estos créditos
DELETE FROM cuota_credito
WHERE (numero_credito, sucursal) IN
(
    (1001, 1),
    (1002, 1),
    (1003, 1),
    (1004, 1)
);


-- Insertamos cuotas que sí cumplen las condiciones establecidas
INSERT INTO cuota_credito
(
    numero_credito,
    numero_cuota,
    fecha_vencimiento,
    capital,
    interes,
    mora,
    pagada,
    sucursal
)
VALUES

    (1001, 1, DATE_SUB(CURDATE(), INTERVAL 15 DAY),
     1000.00, 100.00, 10.00, FALSE, 1),

    (1001, 2, DATE_SUB(CURDATE(), INTERVAL 60 DAY),
     1500.00, 120.00, 20.00, FALSE, 1),

    (1001, 3, DATE_SUB(CURDATE(), INTERVAL 120 DAY),
     2000.00, 150.00, 30.00, FALSE, 1),

    (1002, 1, DATE_SUB(CURDATE(), INTERVAL 240 DAY),
     2500.00, 180.00, 40.00, FALSE, 1),

    (1002, 2, DATE_SUB(CURDATE(), INTERVAL 400 DAY),
     3000.00, 200.00, 50.00, FALSE, 1),

    (1001, 4, DATE_SUB(CURDATE(), INTERVAL 20 DAY),
     9999.00, 0.00, 0.00, TRUE, 1),

    (1003, 1, DATE_SUB(CURDATE(), INTERVAL 45 DAY),
     8888.00, 0.00, 0.00, FALSE, 1),

    (1004, 1, DATE_SUB(CURDATE(), INTERVAL 100 DAY),
     7777.00, 0.00, 0.00, FALSE, 1),

    (1002, 3, DATE_ADD(CURDATE(), INTERVAL 10 DAY),
     6666.00, 0.00, 0.00, FALSE, 1);