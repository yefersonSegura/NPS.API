CREATE DATABASE NpsDb;
GO

USE NpsDb;
GO

CREATE TABLE Roles (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);

INSERT INTO Roles (Id, Name) VALUES (1, 'Admin'), (2, 'Voter');

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    RoleId INT NOT NULL,
    FailedAttempts INT DEFAULT 0,
    LockoutEnd DATETIME NULL,
    RefreshToken NVARCHAR(MAX) NULL,
    RefreshTokenExpiry DATETIME NULL,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

CREATE TABLE SurveyResponses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT UNIQUE NOT NULL, -- Un solo voto por usuario
    Score INT NOT NULL CHECK (Score BETWEEN 0 AND 10),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_SurveyResponses_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
