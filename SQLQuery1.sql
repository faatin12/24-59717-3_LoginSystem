

CREATE DATABASE YourID_LoginDB;
GO

USE YourID_LoginDB;
GO

CREATE TABLE dbo.Users (
    UserID        INT IDENTITY(1,1) PRIMARY KEY,
    Username      NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash  NVARCHAR(200) NOT NULL,
    PasswordSalt  NVARCHAR(200) NOT NULL,  -- extra column: per-user salt, on top of the required fields
    Email         NVARCHAR(100),
    FullName      NVARCHAR(100),
    CreatedAt     DATETIME DEFAULT GETDATE()
);
GO

-- Bonus: LoginHistory table with a foreign key back to Users
CREATE TABLE dbo.LoginHistory (
    LoginHistoryID INT IDENTITY(1,1) PRIMARY KEY,
    UserID         INT NOT NULL,
    LoginTime      DATETIME NOT NULL DEFAULT GETDATE(),
    LogoutTime     DATETIME NULL,
    CONSTRAINT FK_LoginHistory_Users FOREIGN KEY (UserID)
        REFERENCES dbo.Users(UserID) ON DELETE CASCADE
);
GO
