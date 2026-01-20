namespace UtilityBillingSystem.Infrastructure.Db;

public static class SchemaSql
{
    public const string Value = @"
IF OBJECT_ID('dbo.Communities','U') IS NULL
BEGIN
    CREATE TABLE dbo.Communities (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Communities PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL CONSTRAINT UQ_Communities_Name UNIQUE,
        Address NVARCHAR(300) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Communities_CreatedAt DEFAULT SYSUTCDATETIME()
    );
END

IF OBJECT_ID('dbo.Users','U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        Username NVARCHAR(120) NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
        PasswordHash NVARCHAR(400) NOT NULL,
        Role NVARCHAR(20) NOT NULL,
        CommunityId INT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_Users_Role CHECK (Role IN ('ADMIN','MANAGER','RESIDENT'))
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_Communities')
BEGIN
    ALTER TABLE dbo.Users
    ADD CONSTRAINT FK_Users_Communities
    FOREIGN KEY (CommunityId) REFERENCES dbo.Communities(Id);
END

IF OBJECT_ID('dbo.Services','U') IS NULL
BEGIN
    CREATE TABLE dbo.Services (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Services PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL CONSTRAINT UQ_Services_Name UNIQUE,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Services_IsActive DEFAULT 1
    );
END

IF OBJECT_ID('dbo.CommunityServices','U') IS NULL
BEGIN
    CREATE TABLE dbo.CommunityServices (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CommunityServices PRIMARY KEY,
        CommunityId INT NOT NULL,
        ServiceId INT NOT NULL,
        AssignedAt DATETIME2 NOT NULL CONSTRAINT DF_CommunityServices_AssignedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_CommunityServices UNIQUE (CommunityId, ServiceId)
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CommunityServices_Communities')
BEGIN
    ALTER TABLE dbo.CommunityServices
    ADD CONSTRAINT FK_CommunityServices_Communities
    FOREIGN KEY (CommunityId) REFERENCES dbo.Communities(Id) ON DELETE CASCADE;
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CommunityServices_Services')
BEGIN
    ALTER TABLE dbo.CommunityServices
    ADD CONSTRAINT FK_CommunityServices_Services
    FOREIGN KEY (ServiceId) REFERENCES dbo.Services(Id);
END

IF OBJECT_ID('dbo.Prices','U') IS NULL
BEGIN
    CREATE TABLE dbo.Prices (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Prices PRIMARY KEY,
        CommunityServiceId INT NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        Currency NVARCHAR(10) NOT NULL CONSTRAINT DF_Prices_Currency DEFAULT 'EUR',
        EffectiveFrom DATE NOT NULL,
        EffectiveTo DATE NULL,
        CONSTRAINT CK_Prices_Price CHECK (Price >= 0)
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Prices_CommunityServices')
BEGIN
    ALTER TABLE dbo.Prices
    ADD CONSTRAINT FK_Prices_CommunityServices
    FOREIGN KEY (CommunityServiceId) REFERENCES dbo.CommunityServices(Id) ON DELETE CASCADE;
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Prices_Current' AND object_id = OBJECT_ID('dbo.Prices'))
BEGIN
    CREATE UNIQUE INDEX UX_Prices_Current
    ON dbo.Prices(CommunityServiceId)
    WHERE EffectiveTo IS NULL;
END
";
}
