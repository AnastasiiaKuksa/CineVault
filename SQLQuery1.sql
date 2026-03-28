IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CacheTable' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[CacheTable] (
        [Id]                         NVARCHAR(449)      NOT NULL,
        [Value]                      VARBINARY(MAX)     NOT NULL,
        [ExpiresAtTime]              DATETIMEOFFSET     NOT NULL,
        [SlidingExpirationInSeconds] BIGINT             NULL,
        [AbsoluteExpiration]         DATETIMEOFFSET     NULL,
        CONSTRAINT [PK_CacheTable] PRIMARY KEY ([Id])
    );
    PRINT 'CacheTable created successfully.';
END
ELSE
BEGIN
    PRINT 'CacheTable already exists.';
END