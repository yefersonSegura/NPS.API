-- Script para poblar datos iniciales de prueba
-- Contraseñas (BCrypt.Net-Next 4.x, work factor 11):
--   admin   -> Admin123!   (la A es mayúscula)
--   voter01 -> voter123!    (todo en minúsculas para no confundir con Voter123!)
-- Tras ejecutar, comprueba en SSMS: LEN(PasswordHash) debe ser 60 para ambos.
USE NpsDb;
GO

DELETE FROM SurveyResponses;
DELETE FROM Users WHERE Username IN (N'admin', N'voter01');
GO

INSERT INTO Users (Username, PasswordHash, RoleId)
VALUES
    (N'admin', N'$2a$11$huUklf.jgXDQoap.YcorkOoeECBw5ksaQXaWnqotz8oTsv5TLAaXq', 1),
    (N'voter01', N'$2a$11$TzQojqXIC8.LYL8nBGLFH.kCdCdanIzDY6dchzMJN.ZYtHhJiwNU6', 2);
GO

SELECT Username,
       LEN(PasswordHash) AS PasswordHashLength,
       LEFT(PasswordHash, 4) AS HashPrefix -- debe ser $2a$
FROM Users
WHERE Username IN (N'admin', N'voter01');

PRINT N'OK: revisa PasswordHashLength=60 y HashPrefix=$2a$ para ambos.';
GO
