IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916164253_InitialCreate'
)
BEGIN
    CREATE TABLE [Location_Details] (
        [Id] int NOT NULL IDENTITY,
        [Location_Code] nvarchar(100) NOT NULL,
        [Location_Name] nvarchar(200) NOT NULL,
        [Created_At] datetime2 NOT NULL,
        [Updated_At] datetime2 NOT NULL,
        CONSTRAINT [PK_Location_Details] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916164253_InitialCreate'
)
BEGIN
    CREATE TABLE [Purchase_Bill] (
        [Id] int NOT NULL IDENTITY,
        [Created_By_Username] nvarchar(256) NOT NULL,
        [Created_At] datetime2 NOT NULL,
        [Total_Items] int NOT NULL,
        [Total_Quantity] decimal(18,2) NOT NULL,
        [Total_Cost] decimal(18,2) NOT NULL,
        [Total_Selling] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Purchase_Bill] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916164253_InitialCreate'
)
BEGIN
    CREATE TABLE [Purchase_Bill_Item] (
        [Id] int NOT NULL IDENTITY,
        [PurchaseBillId] int NOT NULL,
        [Item_Name] nvarchar(100) NOT NULL,
        [Batch_Location_Code] nvarchar(100) NOT NULL,
        [Batch_Location_Name] nvarchar(200) NOT NULL,
        [Standard_Cost] decimal(18,2) NOT NULL,
        [Standard_Price] decimal(18,2) NOT NULL,
        [Margin] decimal(18,2) NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [Free_Quantity] decimal(18,2) NOT NULL,
        [Discount_Percent] decimal(5,2) NOT NULL,
        [Total_Cost] decimal(18,2) NOT NULL,
        [Total_Selling] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Purchase_Bill_Item] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Purchase_Bill_Item_Purchase_Bill_PurchaseBillId] FOREIGN KEY ([PurchaseBillId]) REFERENCES [Purchase_Bill] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916164253_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Location_Details_Location_Code] ON [Location_Details] ([Location_Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916164253_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Purchase_Bill_Item_PurchaseBillId] ON [Purchase_Bill_Item] ([PurchaseBillId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916164253_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260916164253_InitialCreate', N'8.0.10');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924090000_AddPoNumberAndDashboardIndexes'
)
BEGIN
    ALTER TABLE [Purchase_Bill] ADD [Po_Number] nvarchar(40) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924090000_AddPoNumberAndDashboardIndexes'
)
BEGIN
    EXEC(N'UPDATE [Purchase_Bill] SET [Po_Number] = ''PO-'' + RIGHT(''000000'' + CAST([Id] AS varchar(10)), 6)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924090000_AddPoNumberAndDashboardIndexes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Purchase_Bill_Po_Number] ON [Purchase_Bill] ([Po_Number]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924090000_AddPoNumberAndDashboardIndexes'
)
BEGIN
    CREATE INDEX [IX_Purchase_Bill_Created_At] ON [Purchase_Bill] ([Created_At]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924090000_AddPoNumberAndDashboardIndexes'
)
BEGIN
    CREATE INDEX [IX_Purchase_Bill_Item_Item_Name] ON [Purchase_Bill_Item] ([Item_Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924090000_AddPoNumberAndDashboardIndexes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924090000_AddPoNumberAndDashboardIndexes', N'8.0.10');
END;
GO

COMMIT;
GO

