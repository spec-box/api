-- Возвращает узлы структуры проекта с учётом фильтров.
--
-- Источник узлов: настроенное дерево ("v_TreeId") или, если дерева нет ("v_TreeId" IS NULL),
-- плоский список фич проекта с подсчётом покрытия.
--
-- Семантика фильтров:
--   * "v_Search" — подстрока названия без учёта регистра; совпадение группирующего узла
--     распространяется на всех его потомков;
--   * "v_FeatureType" — тип фичи, проверяется на самом узле (группы не удовлетворяют);
--   * "v_HasProblems" — наличие проблемных проверок, проверяется на самом узле.
-- Узел попадает в результат, если удовлетворяет всем заданным фильтрам,
-- либо является предком такого узла.
CREATE
OR REPLACE FUNCTION public."FilterTree" (
	"v_ProjectId" uuid,
	"v_TreeId" uuid,
	"v_Search" varchar,
	"v_FeatureType" int,
	"v_HasProblems" boolean
)
RETURNS TABLE (
	"Id" uuid,
	"ParentId" uuid,
	"Title" varchar,
	"Amount" int,
	"AmountAutomated" int,
	"AmountProblem" int,
	"SortOrder" int,
	"FeatureCode" varchar,
	"FeatureType" int
)
LANGUAGE sql STABLE
AS $$
WITH RECURSIVE
-- единый набор узлов: настроенное дерево или плоский список фич
nodes AS (
	SELECT n."Id", n."ParentId", n."Title", n."Amount", n."AmountAutomated", n."AmountProblem",
		n."SortOrder", f."Code" AS "FeatureCode", f."FeatureType"
	FROM "TreeNode" n
		LEFT JOIN "Feature" f ON f."Id" = n."FeatureId"
	WHERE n."TreeId" = "v_TreeId"

	UNION ALL

	SELECT f."Id", NULL::uuid, f."Title"::varchar,
		COUNT(a."Id")::int,
		(COUNT(a."Id") FILTER (WHERE a."AutomationState" = 1))::int,
		(COUNT(a."Id") FILTER (WHERE a."AutomationState" = 2))::int,
		NULL::int, f."Code", f."FeatureType"
	FROM "Feature" f
		LEFT JOIN "AssertionGroup" g ON g."FeatureId" = f."Id"
		LEFT JOIN "Assertion" a ON a."AssertionGroupId" = g."Id"
	WHERE "v_TreeId" IS NULL AND f."ProjectId" = "v_ProjectId"
	GROUP BY f."Id"
),
-- узлы с совпавшим названием и их поддеревья;
-- спецсимволы шаблона в поисковой строке экранируются
title_match AS (
	SELECT ns."Id"
	FROM nodes ns
	WHERE ns."Title" ILIKE
		('%' || replace(replace(replace("v_Search", '\', '\\'), '%', '\%'), '_', '\_') || '%')
		ESCAPE '\'

	UNION

	SELECT ns."Id"
	FROM nodes ns
		JOIN title_match tm ON ns."ParentId" = tm."Id"
),
-- узлы, удовлетворяющие всем заданным фильтрам
matched AS (
	SELECT ns."Id"
	FROM nodes ns
	WHERE ("v_Search" IS NULL OR ns."Id" IN (SELECT tm."Id" FROM title_match tm))
		AND ("v_FeatureType" IS NULL OR ns."FeatureType" = "v_FeatureType")
		AND ("v_HasProblems" IS NULL OR (ns."AmountProblem" > 0) = "v_HasProblems")
),
-- плюс все предки найденных узлов
result AS (
	SELECT m."Id" FROM matched m

	UNION

	SELECT ns."ParentId"
	FROM nodes ns
		JOIN result r ON ns."Id" = r."Id"
	WHERE ns."ParentId" IS NOT NULL
)
SELECT ns."Id", ns."ParentId", ns."Title", ns."Amount", ns."AmountAutomated", ns."AmountProblem",
	ns."SortOrder", ns."FeatureCode", ns."FeatureType"
FROM nodes ns
	JOIN result r ON ns."Id" = r."Id"
$$;
