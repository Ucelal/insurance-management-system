-- Add missing columns to Documents table to match the application model
-- The current database only has: Id, Customer_Id, Claim_Id, File_Name, File_Url, Uploaded_At
-- We need to add the missing columns that the application expects

-- Add Document_Id column (rename Id to Document_Id)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'Document_Id')
BEGIN
    EXEC sp_rename 'Documents.Id', 'Document_Id', 'COLUMN';
    PRINT 'Renamed Id column to Document_Id successfully.';
END
ELSE
BEGIN
    PRINT 'Document_Id column already exists in Documents table.';
END;

-- Add File_Type column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'File_Type')
BEGIN
    ALTER TABLE Documents
    ADD File_Type NVARCHAR(100) NOT NULL DEFAULT 'application/pdf';
    PRINT 'File_Type column added to Documents table successfully.';
END
ELSE
BEGIN
    PRINT 'File_Type column already exists in Documents table.';
END;

-- Add FileSize column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'FileSize')
BEGIN
    ALTER TABLE Documents
    ADD FileSize BIGINT NOT NULL DEFAULT 0;
    PRINT 'FileSize column added to Documents table successfully.';
END
ELSE
BEGIN
    PRINT 'FileSize column already exists in Documents table.';
END;

-- Add Category column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'Category')
BEGIN
    ALTER TABLE Documents
    ADD Category NVARCHAR(100) NOT NULL DEFAULT 'Document';
    PRINT 'Category column added to Documents table successfully.';
END
ELSE
BEGIN
    PRINT 'Category column already exists in Documents table.';
END;

-- Add Description column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'Description')
BEGIN
    ALTER TABLE Documents
    ADD Description NVARCHAR(500) NULL;
    PRINT 'Description column added to Documents table successfully.';
END
ELSE
BEGIN
    PRINT 'Description column already exists in Documents table.';
END;

-- Add Version column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'Version')
BEGIN
    ALTER TABLE Documents
    ADD Version NVARCHAR(50) NULL;
    PRINT 'Version column added to Documents table successfully.';
END
ELSE
BEGIN
    PRINT 'Version column already exists in Documents table.';
END;

-- Add Status column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'Status')
BEGIN
    ALTER TABLE Documents
    ADD Status NVARCHAR(50) NOT NULL DEFAULT 'Active';
    PRINT 'Status column added to Documents table successfully.';
END
ELSE
BEGIN
    PRINT 'Status column already exists in Documents table.';
END;

-- Add Expires_At column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'Expires_At')
BEGIN
    ALTER TABLE Documents
    ADD Expires_At DATETIME2 NULL;
    PRINT 'Expires_At column added to Documents table successfully.';
END
ELSE
BEGIN
    PRINT 'Expires_At column already exists in Documents table.';
END;

-- Add Updated_At column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'Updated_At')
BEGIN
    ALTER TABLE Documents
    ADD Updated_At DATETIME2 NULL;
    PRINT 'Updated_At column added to Documents table successfully.';
END
ELSE
BEGIN
    PRINT 'Updated_At column already exists in Documents table.';
END;

-- Add Policy_Id column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'Policy_Id')
BEGIN
    ALTER TABLE Documents
    ADD Policy_Id INT NULL;
    PRINT 'Policy_Id column added to Documents table successfully.';
END
ELSE
BEGIN
    PRINT 'Policy_Id column already exists in Documents table.';
END;

-- Add Uploaded_By_User_Id column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'Uploaded_By_User_Id')
BEGIN
    ALTER TABLE Documents
    ADD Uploaded_By_User_Id INT NULL;
    PRINT 'Uploaded_By_User_Id column added to Documents table successfully.';
END
ELSE
BEGIN
    PRINT 'Uploaded_By_User_Id column already exists in Documents table.';
END;

-- Add User_Id column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'User_Id')
BEGIN
    ALTER TABLE Documents
    ADD User_Id INT NULL;
    PRINT 'User_Id column added to Documents table successfully.';
END
ELSE
BEGIN
    PRINT 'User_Id column already exists in Documents table.';
END;

-- Rename File_Name to FileName (remove underscore)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'File_Name')
BEGIN
    EXEC sp_rename 'Documents.File_Name', 'FileName', 'COLUMN';
    PRINT 'Renamed File_Name column to FileName successfully.';
END
ELSE
BEGIN
    PRINT 'FileName column already exists in Documents table.';
END;

PRINT 'Documents table schema update completed successfully.';


