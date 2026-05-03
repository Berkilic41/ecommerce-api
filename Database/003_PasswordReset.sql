-- Password Reset Tokens
-- Run after 001_Schema.sql and 002_SeedData.sql

CREATE TABLE PasswordResetTokens
(
    Id        INT           IDENTITY(1,1) PRIMARY KEY,
    UserId    INT           NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Token     NVARCHAR(100) NOT NULL UNIQUE,
    ExpiresAt DATETIME2     NOT NULL,
    UsedAt    DATETIME2     NULL,
    CreatedAt DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_PasswordResetTokens_Token  ON PasswordResetTokens(Token);
CREATE INDEX IX_PasswordResetTokens_UserId ON PasswordResetTokens(UserId);
