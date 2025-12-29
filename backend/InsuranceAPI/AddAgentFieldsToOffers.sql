-- Add missing Agent_Id and Reviewed_By columns to Offers table

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Offers') AND name = 'Agent_Id')
BEGIN
    ALTER TABLE Offers
    ADD Agent_Id INT NULL;
    PRINT 'Agent_Id column added to Offers table successfully.';
END
ELSE
BEGIN
    PRINT 'Agent_Id column already exists in Offers table.';
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Offers') AND name = 'Reviewed_By')
BEGIN
    ALTER TABLE Offers
    ADD Reviewed_By INT NULL;
    PRINT 'Reviewed_By column added to Offers table successfully.';
END
ELSE
BEGIN
    PRINT 'Reviewed_By column already exists in Offers table.';
END;

-- Update existing offers to set Agent_Id to 7 (admin's agent ID)
UPDATE Offers 
SET Agent_Id = 7, Reviewed_By = 7
WHERE Agent_Id IS NULL AND Reviewed_By IS NULL;

PRINT 'Updated existing offers with Agent_Id = 7 and Reviewed_By = 7.';

-- Add foreign key constraints after columns are added
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Offers') AND name = 'Agent_Id')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Offers_Agents_Agent_Id')
    BEGIN
        ALTER TABLE Offers
        ADD CONSTRAINT FK_Offers_Agents_Agent_Id
        FOREIGN KEY (Agent_Id) REFERENCES Agents(Agent_Id);
        PRINT 'Foreign key constraint FK_Offers_Agents_Agent_Id added successfully.';
    END
    ELSE
    BEGIN
        PRINT 'Foreign key constraint FK_Offers_Agents_Agent_Id already exists.';
    END;
END;
