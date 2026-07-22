USE banco_codesarrollo;


WITH cuotas_filtradas AS
(
    SELECT
        cc.capital,
        DATEDIFF(CURDATE(), cc.fecha_vencimiento) AS dias_vencidos
    FROM cuota_credito AS cc
    INNER JOIN credito AS c
        ON c.numero_credito = cc.numero_credito
       AND c.sucursal = cc.sucursal
    INNER JOIN tipo_garantia AS tg
        ON tg.tipo_garantia = c.tipo_garantia
    WHERE cc.pagada = FALSE
      AND c.estatus_credito = 'VIGENTE'
      AND UPPER(tg.nombre_garantia) = 'PRENDARIA'
      AND cc.fecha_vencimiento < CURDATE()
)
SELECT 1 AS orden,
       '1 a 30 días' AS banda,
       COALESCE(SUM(CASE WHEN dias_vencidos BETWEEN 1 AND 30 THEN capital ELSE 0 END), 0) AS capital_total
FROM cuotas_filtradas
UNION ALL
SELECT 2,
       '31 a 90 días',
       COALESCE(SUM(CASE WHEN dias_vencidos BETWEEN 31 AND 90 THEN capital ELSE 0 END), 0)
FROM cuotas_filtradas
UNION ALL
SELECT 3,
       '91 a 180 días',
       COALESCE(SUM(CASE WHEN dias_vencidos BETWEEN 91 AND 180 THEN capital ELSE 0 END), 0)
FROM cuotas_filtradas
UNION ALL
SELECT 4,
       '181 a 360 días',
       COALESCE(SUM(CASE WHEN dias_vencidos BETWEEN 181 AND 360 THEN capital ELSE 0 END), 0)
FROM cuotas_filtradas
UNION ALL
SELECT 5,
       'Más de 360 días',
       COALESCE(SUM(CASE WHEN dias_vencidos > 360 THEN capital ELSE 0 END), 0)
FROM cuotas_filtradas
ORDER BY orden;
