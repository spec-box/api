CREATE OR REPLACE PROCEDURE public."BuildTree" ("v_ProjectId" uuid)
LANGUAGE plpgsql
AS $$
BEGIN

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

DROP TABLE IF EXISTS temp_features;
DROP TABLE IF EXISTS temp_path_to_feature;
DROP TABLE IF EXISTS temp_tree_ids;

CREATE TEMPORARY TABLE temp_path_to_feature (
	"TreeId" uuid,
	"AttributeValueId" uuid,
	"Order" int,
	"FeatureId" uuid,
	"Path" TEXT,
	"Key" TEXT
);

CREATE TEMPORARY TABLE temp_tree_ids (
	"Id" uuid,
	"ParentId" uuid,
	"TreeId" uuid,
	"AttributeValueId" uuid,
	"Key" TEXT
);

-- Выбираем все фичи с атрибутами и уровнем группировки
CREATE TEMPORARY TABLE temp_features ON COMMIT DROP as (
	SELECT atgr."TreeId", ftatval."Title", ftatval."Id" "AttributeValueId", atgr."Order", ft."Id" "FeatureId", ft."Title" "FeatureTitle"
	FROM "Feature" ft
		JOIN "FeatureAttributeValue" ftat on ftat."FeatureId" = ft."Id"
		JOIN "AttributeValue" ftatval on ftat."AttributeValueId" = ftatval."Id" 
		JOIN "AttributeGroupOrder" atgr on ftatval."AttributeId" = atgr."AttributeId"
		JOIN "Tree" tree on atgr."TreeId" = tree."Id"
	WHERE ft."ProjectId" = "v_ProjectId" AND tree."ProjectId" = "v_ProjectId"
	UNION ALL
-- Добавляем фичи без требуемого атрибута, это необходимо, так как на каждом уровне потребуется строка
	SELECT atgr."TreeId", null as "Title", null as "AttributeValueId", atgr."Order", ft."Id" "FeatureId", ft."Title" "FeatureTitle"
	FROM "Feature" ft
		CROSS JOIN "AttributeGroupOrder" atgr
			JOIN "Tree" tree on atgr."TreeId" = tree."Id"
	WHERE NOT EXISTS (
		SELECT 1 FROM "FeatureAttributeValue" ftat 
		JOIN "AttributeValue" atval on ftat."AttributeValueId" = atval."Id"
		WHERE ftat."FeatureId" = ft."Id" AND atval."AttributeId" = atgr."AttributeId"
	) AND ft."ProjectId" = "v_ProjectId" AND tree."ProjectId" = "v_ProjectId"
);

-- Строим рекурсивно пути к каждой фиче через склевание AttributeValueId
WITH RECURSIVE path_to_feature AS (
	SELECT 
	tf."TreeId",
	tf."AttributeValueId",
	tf."Order",
	tf."FeatureId",
	COALESCE(MAX(tf."Title"), 'ND') as "Path",
	
	COALESCE(tf."AttributeValueId"::varchar, 'NULL') || '' as "Key"
	
	FROM temp_features as tf
	WHERE "Order" = 1
	GROUP BY "TreeId", "AttributeValueId", "Order", "FeatureId"
	
	UNION ALL
	
	SELECT 
	tf."TreeId",
	tf."AttributeValueId",
	tf."Order",
	tf."FeatureId",
	ptf."Path" || '->' || COALESCE(tf."Title", 'ND') as Path,
	
	ptf."Key" || '.' || COALESCE(tf."AttributeValueId"::varchar, 'NULL') as "Key"
	
	FROM temp_features as tf
		JOIN path_to_feature as ptf ON ptf."TreeId" = tf."TreeId" 
								AND ptf."Order" + 1 = tf."Order" 
								AND ptf."FeatureId" = tf."FeatureId"
)
INSERT INTO temp_path_to_feature
SELECT * FROM path_to_feature;

-- Когда у нас появились уникальные пути к каждой фиче мы можем сгенерировать Id и PrentId для каждого TreeNode
WITH RECURSIVE tree_ids AS (
	SELECT 
	uuid_generate_v4() as "Id",
	NULL::uuid as "ParentId",
	ptf."TreeId",
	ptf."AttributeValueId",
	ptf."Key"
	
	FROM temp_path_to_feature as ptf
	WHERE "Order" = 1
	GROUP BY ptf."TreeId", ptf."AttributeValueId", ptf."Key"
	
	UNION ALL
	
	SELECT 
	uuid_generate_v4() as "Id",
	tids."Id" as "ParentId",
	ptf."TreeId",
	ptf."AttributeValueId",
	ptf."Key"
	
	FROM temp_path_to_feature as ptf 
		JOIN tree_ids tids on tids."Key" = SUBSTRING(ptf."Key" FROM '^(.*)\.')
								AND tids."TreeId" = ptf."TreeId"
	GROUP BY ptf."TreeId", ptf."AttributeValueId", tids."Id", ptf."Key"
)
INSERT INTO temp_tree_ids
SELECT * FROM tree_ids;

DELETE FROM "TreeNodeFeature" WHERE "FeatureId" in (SELECT "Id" FROM "Feature" as ft WHERE ft."ProjectId" = "v_ProjectId");
DELETE FROM "TreeNode" WHERE "TreeId" in (SELECT "Id" FROM "Tree" as tree WHERE tree."ProjectId" = "v_ProjectId");

-- Data to insert to TreeNode, тут выбираем уникальные пары и вставляем, соблюдаем порядок от корня к листьям
INSERT INTO public."TreeNode" ("Id", "ParentId", "AttributeValueId", "TreeId")
SELECT DISTINCT
	tids."Id" as "Id",
	tids."ParentId" as "ParentId",
	tids."AttributeValueId" as "AttributeValueId",
	ptf."TreeId" as "TreeId"
FROM temp_tree_ids as tids
	JOIN temp_path_to_feature as ptf on tids."Key" = ptf."Key"
;

-- Data to insert to TreeNodeFeature, находим самые глубокие узлы связанные с фичами и вставляем
INSERT INTO public."TreeNodeFeature" ("TreeNodeId", "FeatureId")
SELECT DISTINCT ON (ptf."TreeId", ptf."FeatureId") 
	tids."Id" as "TreeNodeId",
	ptf."FeatureId" as "FeatureId"
FROM temp_tree_ids as tids
	   JOIN temp_path_to_feature as ptf on tids."Key" = ptf."Key"
ORDER BY ptf."TreeId", ptf."FeatureId", ptf."Order" DESC;

END;$$