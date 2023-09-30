CREATE OR REPLACE PROCEDURE public."MergeExportedData"(exportId uuid)
    LANGUAGE plpgsql
AS
$$
DECLARE
    projectId uuid;
BEGIN
    -- id проекта
    SELECT "ProjectId"
    INTO projectId
    FROM public."Export"
    WHERE "Id" = exportId;

-- создаем временную таблицу фичей
    CREATE TEMPORARY TABLE tmp_feature
    (
        code        varchar not null,
        title       varchar not null,
        description varchar,
        filePath    varchar
    ) ON COMMIT DROP;

    -- заполняем данными
    INSERT INTO tmp_feature (code, title, description, filePath)
    SELECT "Code", "Title", "Description", "FilePath"
    FROM public."ExportFeature"
    WHERE "ExportId" = exportId;

-- обновляем таблицу фичей
    MERGE INTO public."Feature" f
    USING tmp_feature t
    ON f."Code" = t.code AND f."ProjectId" = projectId
    WHEN MATCHED THEN
        UPDATE
        SET "Title"       = t.title,
            "Description" = t.description,
            "FilePath"    = t.filePath
    WHEN NOT MATCHED THEN
        INSERT ("ProjectId", "Code", "Title", "Description", "FilePath")
        VALUES (projectId, t.code, t.title, t.description, t.filePath);

-- удаляем неактуальные данные
    DELETE
    FROM public."Assertion" a
        USING public."AssertionGroup" gr, public."Feature" f
    WHERE a."AssertionGroupId" = gr."Id"
      AND gr."FeatureId" = f."Id"
      AND f."ProjectId" = projectId;

    DELETE
    FROM public."AssertionGroup" gr
        USING public."Feature" f
    WHERE gr."FeatureId" = f."Id"
      AND f."ProjectId" = projectId;

    DELETE
    FROM public."TreeNode" tn
        USING public."Feature" f
    WHERE tn."FeatureId" = f."Id"
      AND f."ProjectId" = projectId;

    DELETE
    FROM public."Feature"
        USING public."Feature" f
            LEFT OUTER JOIN tmp_feature t
            ON f."ProjectId" = projectId AND f."Code" = t.code
    WHERE public."Feature"."Id" = f."Id"
      AND t.code IS NULL;
END;
$$;

