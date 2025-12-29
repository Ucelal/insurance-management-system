-- Remove unused columns from Claims table
-- Title, Priority, Estimated_Resolution_Date, Claim_Amount, Reported_Date

-- Check if columns exist before dropping
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Claims') AND name = 'Title')
BEGIN
    ALTER TABLE Claims DROP COLUMN Title;
    PRINT 'Column Title dropped successfully';
END
ELSE
BEGIN
    PRINT 'Column Title does not exist';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Claims') AND name = 'Priority')
BEGIN
    ALTER TABLE Claims DROP COLUMN Priority;
    PRINT 'Column Priority dropped successfully';
END
ELSE
BEGIN
    PRINT 'Column Priority does not exist';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Claims') AND name = 'Estimated_Resolution_Date')
BEGIN
    ALTER TABLE Claims DROP COLUMN Estimated_Resolution_Date;
    PRINT 'Column Estimated_Resolution_Date dropped successfully';
END
ELSE
BEGIN
    PRINT 'Column Estimated_Resolution_Date does not exist';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Claims') AND name = 'Claim_Amount')
BEGIN
    ALTER TABLE Claims DROP COLUMN Claim_Amount;
    PRINT 'Column Claim_Amount dropped successfully';
END
ELSE
BEGIN
    PRINT 'Column Claim_Amount does not exist';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Claims') AND name = 'Reported_Date')
BEGIN
    ALTER TABLE Claims DROP COLUMN Reported_Date;
    PRINT 'Column Reported_Date dropped successfully';
END
ELSE
BEGIN
    PRINT 'Column Reported_Date does not exist';
END

PRINT 'Migration completed successfully';

