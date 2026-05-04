-- Crear BD solo si no existe (evita error al repetir el script en local).
IF DB_ID(N'NpsDb') IS NULL
BEGIN
    CREATE DATABASE NpsDb;
END
GO

USE NpsDb;
GO

-- Tablas: pensado para primera instalación. Si ya existían, DROP manual o nueva BD antes de repetir estos CREATE.

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
