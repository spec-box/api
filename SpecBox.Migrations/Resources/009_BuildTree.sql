CREATE
OR REPLACE PROCEDURE public."BuildTree" ("v_ProjectId" uuid)
LANGUAGE plpgsql
AS $$
BEGIN

DROP TABLE IF EXISTS temp_features;
DROP TABLE IF EXISTS temp_path_to_feature;
DROP TABLE IF EXISTS temp_tree_ids;
DROP TABLE IF EXISTS temp_unique_path_to_feature;

CREATE
TEMPORARY TABLE temp_path_to_feature (
	"TreeId" uuid,
	"AttributeValueId" uuid,
	"AttributeValue" VARCHAR(400),
	"Order" int,
	"FeatureId" uuid,
	"FeatureTitle" VARCHAR(400),
	"Amount" INT,
	"AmountAutomated" INT,
	"Path" TEXT,
	"Key" TEXT
);

CREATE
TEMPORARY TABLE temp_tree_ids (
	"Id" uuid,
	"ParentId" uuid,
	"TreeId" uuid,
	"AttributeValueId" uuid,
	"Amount" INT,
	"AmountAutomated" INT,
	"Order" INT,
	"Key" TEXT
);

-- Выбираем все фичи с атрибутами и уровнем группировки
CREATE
TEMPORARY TABLE temp_features ON COMMIT DROP
as (
	SELECT 
		tree."Id" as "TreeId", 
		MIN(attrval."Title") as "AttributeValue", 
		attrval."Id" as "AttributeValueId", 
		MIN(atgr."Order") as "Order", 
		ft."Id" as "FeatureId", 
		MIN(ft."Title") as "FeatureTitle",
		COUNT(DISTINCT ass."Id") as "Amount",
		COUNT(DISTINCT case when ass."IsAutomated" = TRUE then ass."Id" else NULL end) as "AmountAutomated"
	FROM "Feature" ft
		JOIN "FeatureAttributeValue" ftat on ftat."FeatureId" = ft."Id"
		JOIN "AttributeValue" attrval on ftat."AttributeValueId" = attrval."Id" 
		JOIN "AttributeGroupOrder" atgr on attrval."AttributeId" = atgr."AttributeId"
		JOIN "Tree" tree on atgr."TreeId" = tree."Id"
		LEFT JOIN "AssertionGroup" assgrp on assgrp."FeatureId" = ft."Id"
			LEFT JOIN "Assertion" ass on ass."AssertionGroupId" = assgrp."Id"
	WHERE ft."ProjectId" = "v_ProjectId" AND tree."ProjectId" = "v_ProjectId"
	GROUP BY ft."Id", atgr."Id", tree."Id", attrval."Id"
	UNION ALL
-- Добавляем фичи без требуемого атрибута, это необходимо, так как на каждом уровне потребуется строка
	SELECT 
		tree."Id" as "TreeId", 
		NULL as "AttributeValue", 
		NULL as "AttributeValueId", 
		MIN(atgr."Order") as "Order", 
		ft."Id" as "FeatureId", 
		MIN(ft."Title") as "FeatureTitle",
		COUNT(DISTINCT ass."Id") as "Amount",
		COUNT(DISTINCT case when ass."IsAutomated" = TRUE then ass."Id" else NULL end) as "AmountAutomated"
	FROM "Feature" ft
		LEFT JOIN "AssertionGroup" assgrp on assgrp."FeatureId" = ft."Id"
			LEFT JOIN "Assertion" ass on ass."AssertionGroupId" = assgrp."Id"
		CROSS JOIN "AttributeGroupOrder" atgr
			JOIN "Tree" tree on atgr."TreeId" = tree."Id"
	WHERE NOT EXISTS (
		SELECT 1 FROM "FeatureAttributeValue" ftat 
		JOIN "AttributeValue" atval on ftat."AttributeValueId" = atval."Id"
		WHERE ftat."FeatureId" = ft."Id" AND atval."AttributeId" = atgr."AttributeId"
	) AND ft."ProjectId" = "v_ProjectId" AND tree."ProjectId" = "v_ProjectId"
	GROUP BY ft."Id", atgr."Id", tree."Id"
);

-- -- Строим рекурсивно пути к каждой фиче через склевание AttributeValueId
WITH RECURSIVE path_to_feature AS (SELECT tf."TreeId"                                      as "TreeId",
                                          tf."AttributeValueId"                            as "AttributeValueId",
                                          MAX(tf."AttributeValue")                         as "AttributeValue",
                                          tf."Order"                                       as "Order",
                                          tf."FeatureId"                                   as "FeatureId",
                                          MAX(tf."FeatureTitle")                           as "FeatureTitle",
                                          SUM(tf."Amount")                                 as "Amount",
                                          SUM(tf."AmountAutomated")                        as "AmountAutomated",
                                          COALESCE(MAX(tf."AttributeValue"), 'ND')         as "Path",
                                          COALESCE(tf."AttributeValueId"::varchar, 'NULL') as "Key"

                                   FROM temp_features as tf
                                   WHERE "Order" = 1
                                   GROUP BY "TreeId", "AttributeValueId", "Order", "FeatureId"

                                   UNION ALL

                                   SELECT tf."TreeId"                                                          as "TreeId",
                                          tf."AttributeValueId"                                                as "AttributeValueId",
                                          tf."AttributeValue"                                                  as "AttributeValue",
                                          tf."Order"                                                           as "Order",
                                          tf."FeatureId"                                                       as "FeatureId",
                                          tf."FeatureTitle"                                                    as "FeatureTitle",
                                          tf."Amount"                                                          as "Amount",
                                          tf."AmountAutomated"                                                 as "AmountAutomated",
                                          ptf."Path" || '->' || COALESCE(tf."AttributeValue", 'ND')            as Path,
                                          ptf."Key" || '.' || COALESCE(tf."AttributeValueId"::varchar, 'NULL') as "Key"

                                   FROM temp_features as tf
                                            JOIN path_to_feature as ptf ON ptf."TreeId" = tf."TreeId"
                                       AND ptf."Order" + 1 = tf."Order"
                                       AND ptf."FeatureId" = tf."FeatureId")
INSERT
INTO temp_path_to_feature
SELECT *
FROM path_to_feature;

-- Когда у нас появились уникальные пути к каждой фиче мы можем сгенерировать Id и PrentId для каждого TreeNode
WITH RECURSIVE tree_ids AS (SELECT gen_random_uuid()      as "Id",
                                   NULL::uuid as "ParentId", ptf."TreeId" as "TreeId",
                                   ptf."AttributeValueId" as "AttributeValueId",
                                   ptf."Key"              as "Key"
                            FROM temp_path_to_feature as ptf
                            WHERE "Order" = 1
                            GROUP BY ptf."TreeId", ptf."AttributeValueId", ptf."Key"

                            UNION ALL

                            SELECT gen_random_uuid()      as "Id",
                                   tids."Id"              as "ParentId",
                                   ptf."TreeId",
                                   ptf."AttributeValueId" as "AttributeValueId",
                                   ptf."Key"              as "Key"
                            FROM temp_path_to_feature as ptf
                                     JOIN tree_ids tids on tids."Key" = SUBSTRING(ptf."Key" FROM '^(.*)\.')
                                AND tids."TreeId" = ptf."TreeId"
                            GROUP BY ptf."TreeId", ptf."AttributeValueId", tids."Id", ptf."Key")
INSERT
INTO temp_tree_ids
SELECT tids."Id",
       tids."ParentId",
       tids."TreeId",
       tids."AttributeValueId",
       SUM(ptf."Amount")          as "Amount",
       SUM(ptf."AmountAutomated") as "AmountAutomated",
       MAX(ptf."Order"),
       tids."Key"
FROM tree_ids as tids
         JOIN temp_path_to_feature ptf on tids."Key" = ptf."Key"
GROUP BY tids."Id", tids."ParentId", tids."TreeId", tids."AttributeValueId", tids."Key";

DELETE
FROM "TreeNode"
WHERE "TreeId" in (SELECT "Id" FROM "Tree" as tree WHERE tree."ProjectId" = "v_ProjectId");

-- Data to insert to TreeNode, тут выбираем уникальные пары и вставляем, соблюдаем порядок от корня к листьям
INSERT INTO public."TreeNode" ("Id", "ParentId", "TreeId", "Title", "Amount", "AmountAutomated", "SortOrder")
SELECT DISTINCT tids."Id"              as "Id",
                tids."ParentId"        as "ParentId",
                tids."TreeId"          as "TreeId",
                aval."Title"           as "Title",
                tids."Amount"          as "Amount",
                tids."AmountAutomated" as "AmountAutomated",
                aval."SortOrder"       as "SortOrder"
FROM temp_tree_ids as tids
         JOIN "AttributeValue" as aval on tids."AttributeValueId" = aval."Id";

-- Data to insert to TreeNode, находим самые глубокие узлы связанные с фичами и вставляем
INSERT INTO public."TreeNode" ("ParentId", "FeatureId", "TreeId", "Title", "Amount", "AmountAutomated")
SELECT DISTINCT
ON (ptf."TreeId", ptf."FeatureId")
    tids."Id" as "ParentId",
    ptf."FeatureId" as "FeatureId",
    ptf."TreeId" as "TreeId",
    ptf."FeatureTitle" as "Title",
    ptf."Amount" as "Amount",
    ptf."AmountAutomated" as "AmountAutomated"
FROM temp_tree_ids as tids
    JOIN (
    SELECT
    MAX ("Key") as "Key", "TreeId", "FeatureId", MAX ("FeatureTitle") as "FeatureTitle", MAX ("Amount") as "Amount", MAX ("AmountAutomated") as "AmountAutomated", MAX ("Order") as "MaxOrder"
    FROM temp_path_to_feature
    GROUP BY "TreeId", "FeatureId"
    ) as ptf
on tids."Key" = ptf."Key" AND tids."Order" = ptf."MaxOrder";

END;$$
