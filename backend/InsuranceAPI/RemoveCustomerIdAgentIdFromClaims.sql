-- Remove Customer_Id and Agent_Id columns from Claims table
-- These columns are not needed as we have Created_By_User_Id and Processed_By_User_Id

-- Step 1: Drop foreign key constraints
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Claims_Customers_Customer_Id')
    ALTER TABLE [Claims] DROP CONSTRAINT [FK_Claims_Customers_Customer_Id];

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Claims_Agents_Agent_Id')
    ALTER TABLE [Claims] DROP CONSTRAINT [FK_Claims_Agents_Agent_Id];

-- Step 2: Drop indexes
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Claims_Customer_Id' AND object_id = OBJECT_ID('Claims'))
    DROP INDEX [IX_Claims_Customer_Id] ON [Claims];

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Claims_Agent_Id' AND object_id = OBJECT_ID('Claims'))
    DROP INDEX [IX_Claims_Agent_Id] ON [Claims];

-- Step 3: Drop columns
ALTER TABLE [Claims] DROP COLUMN [Customer_Id];
ALTER TABLE [Claims] DROP COLUMN [Agent_Id];
