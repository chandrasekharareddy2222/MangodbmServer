USE [DatabridgeDB]
GO

/****** Object:  Table [dbo].[Field_Metadata]    Script Date: 09-03-2026 16:34:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Field_Metadata](
	[FieldName] [nvarchar](100) NOT NULL,
	[DataElement] [nvarchar](100) NULL,
	[Description] [nvarchar](500) NULL,
	[KeyField] [nvarchar](1) NULL,
	[CheckTable] [nvarchar](100) NULL,
	[DataType] [nvarchar](50) NULL,
	[FieldLength] [int] NULL,
	[Decimals] [int] NULL,
	[ValidationType]  AS (case when [CheckTable] IS NOT NULL AND [CheckTable]<>'' then 'LOOKUP' when [DataType]='DATS' then 'DATE' when [DataType]='NUMC' then 'NUMERIC' else 'TEXT' end) PERSISTED NOT NULL,
	[HasDropdown] [nvarchar](1) NULL,
	[IsMandatory]  AS (case when [KeyField]='X' then (1) else (0) end) PERSISTED NOT NULL,
	[UIAssignmentBlock] [nvarchar](100) NULL,
	[UIControlType]  AS (case when [HasDropdown]='X' then 'DROPDOWN' when [DataType]='DATS' then 'DATEPICKER' when [FieldLength]>(255) then 'TEXTAREA' else 'TEXTBOX' end) PERSISTED NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[Subject] [nvarchar](100) NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Field_Metadata] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[Field_Metadata] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[Field_Metadata]
ADD [Coordinate] NVARCHAR(100) NULL;
