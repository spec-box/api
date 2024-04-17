CREATE OR REPLACE PROCEDURE public."MergeExportedData"(exportId uuid)
    LANGUAGE plpgsql
AS
$$
DECLARE
    currentProjectId uuid;
    rowsCount integer;
BEGIN
    -- ## ID ПРОЕКТА
    SELECT "ProjectId"
    INTO currentProjectId
    FROM public."Export"
    WHERE "Id" = exportId;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Export % is not found', exportId;
    END IF;

    -- ## ЭКСПОРТ ФИЧЕЙ
    -- создаем временную таблицу фичей
    CREATE TEMPORARY TABLE tmp_feature
    (
        code        varchar not null,
        title       varchar not null,
        featureType integer,
        description varchar,
        filePath    varchar
    ) ON COMMIT DROP;

    -- заполняем данными временную таблицу фичей
    INSERT INTO tmp_feature (code, title, featureType, description, filePath)
    SELECT "Code", "Title", "FeatureType", "Description", "FilePath"
    FROM public."ExportFeature"
    WHERE "ExportId" = exportId;

    GET DIAGNOSTICS rowsCount = ROW_COUNT;
    RAISE NOTICE 'exported features: %', rowsCount;

    -- обновляем таблицу фичей
    MERGE INTO public."Feature" f
    USING tmp_feature t
    ON f."Code" = t.code AND f."ProjectId" = currentProjectId
    WHEN MATCHED THEN
        UPDATE
        SET "Title"       = t.title,
            "FeatureType" = t.featureType,
            "Description" = t.description,
            "FilePath"    = t.filePath
    WHEN NOT MATCHED THEN
        INSERT ("ProjectId", "Code", "Title", "FeatureType", "Description", "FilePath")
        VALUES (currentProjectId, t.code, t.title, t.featureType, t.description, t.filePath);

    GET DIAGNOSTICS rowsCount = ROW_COUNT;
    RAISE NOTICE 'updated features: %', rowsCount;

    -- ## ЭКСПОРТ ЗАВИСИМОСТЕЙ ФИЧЕЙ
    -- создаем временную таблицу зависимостей
    CREATE TEMPORARY TABLE tmp_dependency
    (
        sourceCode      varchar not null,
        sourceId        uuid    not null,
        dependencyCode  varchar not null,
        dependencyId    uuid    not null
    ) ON COMMIT DROP;

    -- заполняем данными временную таблицу зависимостей
    INSERT INTO tmp_dependency (sourceCode, sourceId, dependencyCode, dependencyId)
    SELECT d."SourceFeatureCode", fs."Id", d."DependencyFeatureCode", fd."Id"
    from public."ExportFeatureDependency" d
             left outer join public."Feature" fs 
                 on fs."Code" = d."SourceFeatureCode" AND fs."ProjectId" = currentProjectId
             left outer join public."Feature" fd
                 on fd."Code" = d."DependencyFeatureCode" AND fd."ProjectId" = currentProjectId
    WHERE "ExportId" = exportId;

    -- добавляем недостающие значения в таблицу зависимостей
    MERGE INTO public."FeatureDependency" fd
    USING tmp_dependency t
    ON fd."SourceFeatureId" = t.sourceId AND fd."DependencyFeatureId" = t.dependencyId
    WHEN NOT MATCHED THEN
        INSERT ("SourceFeatureId", "DependencyFeatureId")
        VALUES (t.sourceId, t.dependencyId);
    
    GET DIAGNOSTICS rowsCount = ROW_COUNT;
    RAISE NOTICE 'added dependencies: %', rowsCount;
    
    -- ## ЭКСПОРТ ГРУПП
    -- создаем временную таблицу групп
    CREATE TEMPORARY TABLE tmp_group
    (
        featureId   uuid    not null,
        featureCode varchar not null,
        title       varchar not null,
        sortOrder   integer
    ) ON COMMIT DROP;

    -- заполняем данными временную таблицу групп
    INSERT INTO tmp_group (featureId, featureCode, title, sortOrder)
    SELECT f."Id", ea."FeatureCode", ea."GroupTitle", MAX(ea."GroupSortOrder")
    FROM public."ExportAssertion" ea
             JOIN public."Feature" f ON ea."FeatureCode" = f."Code" AND f."ProjectId" = currentProjectId
    WHERE ea."ExportId" = exportId
    GROUP BY f."Id", ea."FeatureCode", ea."GroupTitle";

    GET DIAGNOSTICS rowsCount = ROW_COUNT;
    RAISE NOTICE 'exported groups: %', rowsCount;

    -- обновляем таблицу групп
    MERGE INTO public."AssertionGroup" gr
    USING tmp_group t
    ON gr."FeatureId" = t.featureId AND gr."Title" = t.title
    WHEN MATCHED THEN
        UPDATE
            SET "SortOrder" = t.sortOrder
    WHEN NOT MATCHED THEN
        INSERT ("FeatureId", "Title", "SortOrder")
        VALUES (t.featureId, t.title, t.sortOrder);

    GET DIAGNOSTICS rowsCount = ROW_COUNT;
    RAISE NOTICE 'added groups: %', rowsCount;

    -- ## ЭКСПОРТ УТВЕРЖДЕНИЙ
    -- создаем временную таблицу утверждений
    CREATE TEMPORARY TABLE tmp_assertion
    (
        groupId     uuid    not null,
        title       varchar not null,
        description varchar,
        sortOrder   integer,
        automationState integer not null
    ) ON COMMIT DROP;

    -- заполняем данными временную таблицу утверждений
    INSERT INTO tmp_assertion (groupId, title, description, sortOrder, automationState)
    SELECT gr."Id", ea."Title", ea."Description", ea."SortOrder", ea."AutomationState"
    FROM public."ExportAssertion" ea
             JOIN public."AssertionGroup" gr ON gr."Title" = ea."GroupTitle"
             JOIN public."Feature" f
                  ON gr."FeatureId" = f."Id" AND f."Code" = ea."FeatureCode" AND f."ProjectId" = currentProjectId
    WHERE ea."ExportId" = exportId;

    GET DIAGNOSTICS rowsCount = ROW_COUNT;
    RAISE NOTICE 'exported assertions: %', rowsCount;

    -- обновляем таблицу утверждений
    MERGE INTO public."Assertion" a
    USING tmp_assertion t
    ON a."Title" = t.title AND a."AssertionGroupId" = t.groupId
    WHEN MATCHED THEN
        UPDATE
        SET "Description" = t.description,
            "AutomationState" = t.automationState,
            "SortOrder" = t.sortOrder
    WHEN NOT MATCHED THEN
        INSERT ("AssertionGroupId", "Title", "Description", "SortOrder", "AutomationState")
        VALUES (t.groupId, t.title, t.description, t.sortOrder, t.automationState);

    GET DIAGNOSTICS rowsCount = ROW_COUNT;
    RAISE NOTICE 'updated assertions: %', rowsCount;

    -- ## ЭКСПОРТ АТРИБУТОВ ФИЧЕЙ
    -- создаем временную таблицу атрибутов фичей
    CREATE TEMPORARY TABLE tmp_feature_attribute
    (
        featureId          uuid    not null,
        attributeId        uuid    not null,
        attributeValueCode varchar not null
    ) ON COMMIT DROP;

    -- заполняем данными временную таблицу атрибутов фичей
    INSERT INTO tmp_feature_attribute (featureId, attributeId, attributeValueCode)
    SELECT f."Id" as featureId, a."Id" attributeId, fa."AttributeValueCode" as attributeValueCode
    FROM public."ExportFeatureAttribute" fa
             JOIN public."Feature" f ON f."Code" = fa."FeatureCode" AND f."ProjectId" = currentProjectId
             JOIN public."Attribute" a ON a."Code" = fa."AttributeCode" AND a."ProjectId" = currentProjectId
    WHERE fa."ExportId" = exportId;

    GET DIAGNOSTICS rowsCount = ROW_COUNT;
    RAISE NOTICE 'exported feature attribute values: %', rowsCount;

    -- добавляем недостающие значения в таблицу значений атрибутов
    MERGE INTO public."AttributeValue" av
    USING (select attributeId, attributeValueCode
           from tmp_feature_attribute
           group by attributeId, attributeValueCode) t
    ON av."AttributeId" = t.attributeId AND av."Code" = t.attributeValueCode
    WHEN NOT MATCHED THEN
        INSERT ("AttributeId", "Code", "Title")
        VALUES (t.attributeId, t.attributeValueCode, t.attributeValueCode);

    GET DIAGNOSTICS rowsCount = ROW_COUNT;
    RAISE NOTICE 'added attribute values: %', rowsCount;

    -- добавляем недостающие связи значений атрибутов с фичами
    MERGE INTO public."FeatureAttributeValue" fav
    USING (select featureId, av."Id" attributeValueId
           from tmp_feature_attribute as x
                    join public."AttributeValue" av
                         on av."AttributeId" = x.attributeId and av."Code" = x.attributeValueCode) t
    ON fav."FeatureId" = t.featureId AND fav."AttributeValueId" = t.attributeValueId
    WHEN NOT MATCHED THEN
        INSERT ("FeatureId", "AttributeValueId")
        VALUES (t.featureId, t.attributeValueId);

    GET DIAGNOSTICS rowsCount = ROW_COUNT;
    RAISE NOTICE 'added feature attribute values: %', rowsCount;

    -- ## УДАЛЯЕМ НЕАКТУАЛЬНЫЕ ДАННЫЕ

    DELETE
    FROM public."TreeNode" tn
        USING public."Feature" f
    WHERE tn."FeatureId" = f."Id"
      AND f."ProjectId" = currentProjectId;

    -- удаляем неактуальные связи фичей со значениями атрибутов
    DELETE
    FROM public."FeatureAttributeValue"
        USING public."FeatureAttributeValue" fav
            INNER JOIN public."Feature" f
            ON fav."FeatureId" = f."Id" AND f."ProjectId" = currentProjectId
            JOIN public."AttributeValue" av ON fav."AttributeValueId" = av."Id"
            LEFT OUTER JOIN tmp_feature_attribute t
            ON fav."FeatureId" = t.featureId AND av."AttributeId" = t.attributeId AND av."Code" = t.attributeValueCode
    WHERE public."FeatureAttributeValue"."FeatureId" = fav."FeatureId"
      AND public."FeatureAttributeValue"."AttributeValueId" = av."Id"
      AND t.featureId IS NULL;

    GET DIAGNOSTICS rowsCount = ROW_COUNT;
    RAISE NOTICE 'deleted feature attribute values: %', rowsCount;

    -- удаляем неактуальные значения атрибутов
    DELETE
    FROM public."AttributeValue"
        USING public."AttributeValue" av
            INNER JOIN public."Attribute" a ON av."AttributeId" = a."Id" AND a."ProjectId" = currentProjectId
            LEFT OUTER JOIN public."FeatureAttributeValue" fav ON av."Id" = fav."AttributeValueId"
    WHERE public."AttributeValue"."Id" = av."Id"
      AND fav."AttributeValueId" IS NULL;

    GET DIAGNOSTICS rowsCount = ROW_COUNT;
    RAISE NOTICE 'deleted attribute values: %', rowsCount;

    -- удаляем неактуальные утверждения
    DELETE
    FROM public."Assertion"
        USING public."Assertion" a
            INNER JOIN public."AssertionGroup" gr
            ON a."AssertionGroupId" = gr."Id"
            INNER JOIN public."Feature" f
            ON gr."FeatureId" = f."Id" AND f."ProjectId" = currentProjectId
            LEFT OUTER JOIN tmp_assertion t
            ON a."AssertionGroupId" = t.groupId AND a."Title" = t.title
    WHERE public."Assertion"."Id" = a."Id"
      AND t.groupId IS NULL;

    GET DIAGNOSTICS rowsCount = ROW_COUNT;
    RAISE NOTICE 'deleted assertions: %', rowsCount;

    -- удаляем неактуальные группы
    DELETE
    FROM public."AssertionGroup"
        USING public."AssertionGroup" gr
            INNER JOIN public."Feature" f
            ON gr."FeatureId" = f."Id" AND f."ProjectId" = currentProjectId
            LEFT OUTER JOIN tmp_group t
            ON gr."FeatureId" = t.featureId AND gr."Title" = t.title
    WHERE public."AssertionGroup"."Id" = gr."Id"
      AND t.featureId IS NULL;

    GET DIAGNOSTICS rowsCount = ROW_COUNT;
    RAISE NOTICE 'deleted groups: %', rowsCount;

    -- удаляем неактуальные зависимости
    DELETE
    FROM public."FeatureDependency"
        USING public."FeatureDependency" fd
            INNER JOIN public."Feature" f
            ON fd."SourceFeatureId" = f."Id" AND f."ProjectId" = currentProjectId
            LEFT OUTER JOIN tmp_dependency d
            ON fd."SourceFeatureId" = d.sourceId AND fd."DependencyFeatureId" = d.dependencyId
    WHERE public."FeatureDependency"."SourceFeatureId" = fd."SourceFeatureId"
      AND public."FeatureDependency"."DependencyFeatureId" = fd."DependencyFeatureId"
      AND d.sourceId IS NULL;

    GET DIAGNOSTICS rowsCount = ROW_COUNT;
    RAISE NOTICE 'deleted dependencies: %', rowsCount;

    -- удаляем неактуальные фичи
    DELETE
    FROM public."Feature"
        USING public."Feature" f
            LEFT OUTER JOIN tmp_feature t
            ON f."Code" = t.code
    WHERE public."Feature"."Id" = f."Id"
      AND f."ProjectId" = currentProjectId
      AND t.code IS NULL;

    GET DIAGNOSTICS rowsCount = ROW_COUNT;
    RAISE NOTICE 'deleted features: %', rowsCount;
END;
$$;
