SELECT name
FROM sys.key_constraints
WHERE type = 'PK'
AND parent_object_id = OBJECT_ID('Field_Metadata');

ALTER TABLE dbo.Field_Metadata
DROP CONSTRAINT PK__Field_Me__A88707A7FB8E6F51;