-- SQL Script to remove Type column from Customers table
-- Created: 2025-10-10
-- Purpose: Remove customer type (bireysel/kurumsal) distinction from database

USE InsuranceDB;
GO

-- Step 1: Check if the column exists
IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Customers' 
    AND COLUMN_NAME = 'Type'
)
BEGIN
    PRINT 'Type column found in Customers table. Proceeding with removal...';
    
    -- Step 2: Drop the Type column
    ALTER TABLE Customers
    DROP COLUMN [Type];
    
    PRINT 'Type column successfully removed from Customers table.';
END
ELSE
BEGIN
    PRINT 'Type column does not exist in Customers table. No action needed.';
END
GO

-- Step 3: Verify the change
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Customers'
ORDER BY ORDINAL_POSITION;
GO

PRINT 'Script execution completed successfully.';
GO


