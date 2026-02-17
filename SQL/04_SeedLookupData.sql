-- =============================================
-- Seed Data for Check_Table_Values and Passable_Values
-- =============================================

USE DatabridgeDB;
GO

-- Insert Check Table Values for T144 (Container Requirements)
INSERT INTO dbo.Check_Table_Values (CheckTableName, KeyValue, Description, Language, IsActive, SortOrder)
VALUES
('T144', 'CR01', 'Standard Container', 'EN', 1, 1),
('T144', 'CR02', 'Sealed Container', 'EN', 1, 2),
('T144', 'CR03', 'Refrigerated Container', 'EN', 1, 3),
('T144', 'CR04', 'Hazmat Container', 'EN', 1, 4);

-- Insert Check Table Values for T006 (Units of Measure)
INSERT INTO dbo.Check_Table_Values (CheckTableName, KeyValue, Description, Language, IsActive, SortOrder)
VALUES
('T006', 'KG', 'Kilogram', 'EN', 1, 1),
('T006', 'G', 'Gram', 'EN', 1, 2),
('T006', 'LB', 'Pound', 'EN', 1, 3),
('T006', 'OZ', 'Ounce', 'EN', 1, 4),
('T006', 'M', 'Meter', 'EN', 1, 5),
('T006', 'CM', 'Centimeter', 'EN', 1, 6),
('T006', 'MM', 'Millimeter', 'EN', 1, 7),
('T006', 'IN', 'Inch', 'EN', 1, 8),
('T006', 'FT', 'Foot', 'EN', 1, 9),
('T006', 'L', 'Liter', 'EN', 1, 10),
('T006', 'GAL', 'Gallon', 'EN', 1, 11),
('T006', 'M3', 'Cubic Meter', 'EN', 1, 12),
('T006', 'PC', 'Piece', 'EN', 1, 13),
('T006', 'EA', 'Each', 'EN', 1, 14);

-- Insert Check Table Values for T142 (Storage Conditions)
INSERT INTO dbo.Check_Table_Values (CheckTableName, KeyValue, Description, Language, IsActive, SortOrder)
VALUES
('T142', 'SC01', 'Normal Storage', 'EN', 1, 1),
('T142', 'SC02', 'Climate Controlled', 'EN', 1, 2),
('T142', 'SC03', 'Refrigerated', 'EN', 1, 3),
('T142', 'SC04', 'Frozen', 'EN', 1, 4),
('T142', 'SC05', 'Hazmat Storage', 'EN', 1, 5);

-- Insert Check Table Values for T143 (Temperature Conditions)
INSERT INTO dbo.Check_Table_Values (CheckTableName, KeyValue, Description, Language, IsActive, SortOrder)
VALUES
('T143', 'T01', 'Room Temperature', 'EN', 1, 1),
('T143', 'T02', 'Refrigerated (2-8°C)', 'EN', 1, 2),
('T143', 'T03', 'Frozen (-18°C)', 'EN', 1, 3),
('T143', 'T04', 'Ultra-Cold (-80°C)', 'EN', 1, 4),
('T143', 'T05', 'High Temperature (>50°C)', 'EN', 1, 5);

-- Insert Check Table Values for T023 (Material Groups)
INSERT INTO dbo.Check_Table_Values (CheckTableName, KeyValue, Description, Language, IsActive, SortOrder)
VALUES
('T023', 'MAT001', 'Electronics Components', 'EN', 1, 1),
('T023', 'MAT002', 'Raw Materials - Metals', 'EN', 1, 2),
('T023', 'MAT003', 'Chemicals - Industrial', 'EN', 1, 3),
('T023', 'MAT004', 'Building Materials', 'EN', 1, 4),
('T023', 'MAT005', 'Pharmaceuticals', 'EN', 1, 5),
('T023', 'MAT006', 'Office Supplies', 'EN', 1, 6),
('T023', 'MAT007', 'Automotive Parts', 'EN', 1, 7),
('T023', 'MAT008', 'Textiles', 'EN', 1, 8),
('T023', 'MAT009', 'Food Ingredients', 'EN', 1, 9),
('T023', 'MAT010', 'Energy Equipment', 'EN', 1, 10);

-- Insert Check Table Values for T134 (Material Types)
INSERT INTO dbo.Check_Table_Values (CheckTableName, KeyValue, Description, Language, IsActive, SortOrder)
VALUES
('T134', 'ROH', 'Raw Material', 'EN', 1, 1),
('T134', 'HALB', 'Semi-Finished Product', 'EN', 1, 2),
('T134', 'FERT', 'Finished Product', 'EN', 1, 3),
('T134', 'HIBE', 'Operating Supplies', 'EN', 1, 4),
('T134', 'VERP', 'Packaging Material', 'EN', 1, 5),
('T134', 'UNBW', 'Non-Valuated Material', 'EN', 1, 6),
('T134', 'NLAG', 'Non-Stock Material', 'EN', 1, 7),
('T134', 'KMAT', 'Configurable Material', 'EN', 1, 8);

-- Insert Check Table Values for T137 (Industry Sectors)
INSERT INTO dbo.Check_Table_Values (CheckTableName, KeyValue, Description, Language, IsActive, SortOrder)
VALUES
('T137', 'M', 'Mechanical Engineering', 'EN', 1, 1),
('T137', 'E', 'Electrical/Electronics', 'EN', 1, 2),
('T137', 'C', 'Chemical Industry', 'EN', 1, 3),
('T137', 'P', 'Pharmaceutical', 'EN', 1, 4),
('T137', 'F', 'Food & Beverage', 'EN', 1, 5),
('T137', 'A', 'Plant Engineering', 'EN', 1, 6),
('T137', 'D', 'Discrete Manufacturing', 'EN', 1, 7),
('T137', 'O', 'Oil Industry', 'EN', 1, 8);

-- Insert Check Table Values for TTGR (Transportation Groups)
INSERT INTO dbo.Check_Table_Values (CheckTableName, KeyValue, Description, Language, IsActive, SortOrder)
VALUES
('TTGR', 'TG01', 'Standard Freight', 'EN', 1, 1),
('TTGR', 'TG02', 'Hazardous Materials', 'EN', 1, 2),
('TTGR', 'TG03', 'Temperature Controlled', 'EN', 1, 3),
('TTGR', 'TG04', 'Fragile Items', 'EN', 1, 4),
('TTGR', 'TG05', 'Oversized Items', 'EN', 1, 5);

-- Insert Check Table Values for T024L (Laboratories)
INSERT INTO dbo.Check_Table_Values (CheckTableName, KeyValue, Description, Language, IsActive, SortOrder)
VALUES
('T024L', 'L01', 'Quality Lab - Electronics', 'EN', 1, 1),
('T024L', 'L02', 'Quality Lab - Chemical', 'EN', 1, 2),
('T024L', 'L03', 'Quality Lab - Materials', 'EN', 1, 3),
('T024L', 'D01', 'Design Office - Mechanical', 'EN', 1, 4),
('T024L', 'D02', 'Design Office - Electrical', 'EN', 1, 5),
('T024L', 'D03', 'Design Office - Software', 'EN', 1, 6);

-- Insert Passable Values for various fields
-- ATTYP - Material Category
INSERT INTO dbo.Passable_Values (FieldName, KeyValue, Description, DisplayOrder, IsActive, CreatedBy)
VALUES
('ATTYP', '00', 'Standard material', 1, 1, 'SYSTEM'),
('ATTYP', '01', 'Variant material', 2, 1, 'SYSTEM'),
('ATTYP', '02', 'Configurable material', 3, 1, 'SYSTEM');

-- CADKZ - CAD Indicator
INSERT INTO dbo.Passable_Values (FieldName, KeyValue, Description, DisplayOrder, IsActive, CreatedBy)
VALUES
('CADKZ', ' ', 'No CAD drawing available', 1, 1, 'SYSTEM'),
('CADKZ', 'X', 'CAD drawing exists', 2, 1, 'SYSTEM');

-- CMETH - Quantity Conversion Method
INSERT INTO dbo.Passable_Values (FieldName, KeyValue, Description, DisplayOrder, IsActive, CreatedBy)
VALUES
('CMETH', ' ', 'Standard conversion method', 1, 1, 'SYSTEM'),
('CMETH', 'A', 'Conversion method A', 2, 1, 'SYSTEM'),
('CMETH', 'B', 'Conversion method B', 3, 1, 'SYSTEM'),
('CMETH', 'C', 'Conversion method C', 4, 1, 'SYSTEM');

-- HAZMAT - Hazardous Material
INSERT INTO dbo.Passable_Values (FieldName, KeyValue, Description, DisplayOrder, IsActive, CreatedBy)
VALUES
('HAZMAT', ' ', 'Not a hazardous material', 1, 1, 'SYSTEM'),
('HAZMAT', 'X', 'Hazardous material', 2, 1, 'SYSTEM');

-- KZKFG - Configurable Material
INSERT INTO dbo.Passable_Values (FieldName, KeyValue, Description, DisplayOrder, IsActive, CreatedBy)
VALUES
('KZKFG', ' ', 'Not configurable', 1, 1, 'SYSTEM'),
('KZKFG', 'X', 'Material is configurable', 2, 1, 'SYSTEM');

-- KZKRI - Critical Part
INSERT INTO dbo.Passable_Values (FieldName, KeyValue, Description, DisplayOrder, IsActive, CreatedBy)
VALUES
('KZKRI', ' ', 'Not a critical part', 1, 1, 'SYSTEM'),
('KZKRI', 'X', 'Critical part', 2, 1, 'SYSTEM');

-- KZKUP - Co-product
INSERT INTO dbo.Passable_Values (FieldName, KeyValue, Description, DisplayOrder, IsActive, CreatedBy)
VALUES
('KZKUP', ' ', 'Cannot be co-product', 1, 1, 'SYSTEM'),
('KZKUP', 'X', 'Can be co-product', 2, 1, 'SYSTEM');

-- LVORM - Deletion Flag
INSERT INTO dbo.Passable_Values (FieldName, KeyValue, Description, DisplayOrder, IsActive, CreatedBy)
VALUES
('LVORM', ' ', 'Material is active and usable', 1, 1, 'SYSTEM'),
('LVORM', 'X', 'Material marked for deletion', 2, 1, 'SYSTEM');

-- MAABC - ABC Indicator  
INSERT INTO dbo.Passable_Values (FieldName, KeyValue, Description, DisplayOrder, IsActive, CreatedBy)
VALUES
('MAABC', 'A', 'High value/high volume', 1, 1, 'SYSTEM'),
('MAABC', 'B', 'Medium value/medium volume', 2, 1, 'SYSTEM'),
('MAABC', 'C', 'Low value/low volume', 3, 1, 'SYSTEM');

-- QMPUR - QM in Procurement
INSERT INTO dbo.Passable_Values (FieldName, KeyValue, Description, DisplayOrder, IsActive, CreatedBy)
VALUES
('QMPUR', ' ', 'Quality management not required', 1, 1, 'SYSTEM'),
('QMPUR', 'X', 'Quality management active in procurement', 2, 1, 'SYSTEM');

-- XCHPF - Batch Management
INSERT INTO dbo.Passable_Values (FieldName, KeyValue, Description, DisplayOrder, IsActive, CreatedBy)
VALUES
('XCHPF', ' ', 'Batch management not required', 1, 1, 'SYSTEM'),
('XCHPF', 'X', 'Batch management required', 2, 1, 'SYSTEM');

-- /VSO/R_STACK_IND - Stackable
INSERT INTO dbo.Passable_Values (FieldName, KeyValue, Description, DisplayOrder, IsActive, CreatedBy)
VALUES
('/VSO/R_STACK_IND', ' ', 'Material may not be stacked', 1, 1, 'SYSTEM'),
('/VSO/R_STACK_IND', 'X', 'Material may be stacked', 2, 1, 'SYSTEM');

-- /VSO/R_TILT_IND - Tiltable
INSERT INTO dbo.Passable_Values (FieldName, KeyValue, Description, DisplayOrder, IsActive, CreatedBy)
VALUES
('/VSO/R_TILT_IND', ' ', 'Material may not be tilted', 1, 1, 'SYSTEM'),
('/VSO/R_TILT_IND', 'X', 'Material may be tilted', 2, 1, 'SYSTEM');

-- AEKLK - Stock Transfer Net Change Costing
INSERT INTO dbo.Passable_Values (FieldName, KeyValue, Description, DisplayOrder, IsActive, CreatedBy)
VALUES
('AEKLK', ' ', 'Not active for net change costing', 1, 1, 'SYSTEM'),
('AEKLK', 'X', 'Active for net change costing', 2, 1, 'SYSTEM');

PRINT 'Sample lookup data inserted successfully.';
GO
