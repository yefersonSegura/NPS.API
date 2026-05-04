# NPS — Ejercicio DEV-001

Soy **Yeferson Segura**, desarrollador de software en **móvil y web**. Mi portfolio profesional y formas de contacto están en **[yefersonsegura.com](https://yefersonsegura.com/)**. Este repo concentra la **API del ejercicio DEV-001**.

El **back-end** (.NET 10) incluye JWT con refresh, un voto por usuario y resultados NPS para admin. El **SPA (Omega, Angular)** vive **en otro repo:** **[github.com/yefersonSegura/NPS.FRONT](https://github.com/yefersonSegura/NPS.FRONT)** (nombre local habitual del checkout: `npsApp`). Este árbol es **solo API, scripts SQL y tests**.

---

## Repositorios y código

| Qué | Dónde |
|-----|--------|
| API + scripts SQL + tests | Este repo (`Nps.Api.slnx`) |
| SPA Omega Angular | **[NPS.FRONT](https://github.com/yefersonSegura/NPS.FRONT)** · checkout local típ.: `C:\Proyectos_Yeferson\Angular\npsApp` |

**Capas (.NET)**  
`Domain` → entidades limpias. `Application` → casos de uso (MediatR), validación (FluentValidation), contratos, AutoMapper donde aporta (`NpsResultsProfile` → `NpsResultsDto`). `Infrastructure` → Dapper contra SQL Server, emisión JWT y hashing. `Api` → controladores finos, pipeline, OpenAPI/Swagger en desarrollo.

No uso Entity Framework de propósito: el enunciado pide Dapper y consultas explícitas; los repositorios concentran el SQL.

---

## Base de datos

**Antes que `dotnet run`:** tiene que existir la base con tablas y datos de prueba. Orden típico:

1. Ejecutá **primero** `Scripts/InitialSchema.sql`, **después** `Scripts/SeedData.sql` (en SSMS/Azure Data Studio o `sqlcmd`, contra tu instancia de SQL Server).
2. Usuarios demo: `admin` y `voter01`; las contraseñas están solo en comentarios del seed (no las dejo como documento “vivo” fuera del script).
3. Apuntá la cadena en la API a esa misma base: partí de `src/Api/appsettings.Example.json`, copiá a algo local ignorado por git o usá [`dotnet user-secrets`](https://learn.microsoft.com/aspnet/core/security/app-secrets) para `ConnectionStrings:DefaultConnection`.

Si saltás estos pasos, la API levanta pero fallará al pegarle a usuarios/tablas que no existen.

---

## Arrancar la API

(Solo después de tener **BD aplicada + `DefaultConnection`** configurada.)

```bash
cd src/Api
dotnet run --launch-profile https
```

URLs según `Properties/launchSettings.json`: HTTPS `https://localhost:7070`, HTTP `http://localhost:5140`. Si el proceso queda trabado con DLL bloqueadas, es el mismo `dotnet`/IISExpress de siempre — cerrá el host antes de rebuild.

OpenAPI genera `/openapi/v1.json`; en desarrollo Swagger UI lo monta desde `Program.cs`. CORS viene de `Cors:AllowedOrigins`; por defecto `localhost:4200` http/https. Otro puerto → agregalo ahí.

**JSON**: `PropertyNamingPolicy = CamelCase` en controladores; el mismo contrato espera Angular (véase envolvente `data`, `succeeded`, `message`, `statusCode`).

**JWT** (`JwtSettings`): `ExpiryMinutes` en servidor (5 en demo). El Angular además desloguea por inactividad (~5 min): son reglas distintas; el token puede seguir válido unos segundos si solo mirás el reloj del servidor.

---

## Cliente Omega (Angular)

La guía del **front** (contrato HTTP, rutas SPA, `apiUrl`, carpetas y build) está en el **[README del repo NPS.FRONT](https://github.com/yefersonSegura/NPS.FRONT)**, no acá — una sola fuente de verdad del SPA.

---

## Trazabilidad DEV-001 (requisito → implementación)

| Requisito | Implementación principal |
|-----------|---------------------------|
| Roles admin / votante | Seed + `[Authorize(Roles = "1"|"2")]`; guards Angular |
| Login usuario/contraseña | `Login.cs`, `AuthController` |
| Escala 0–10 y solo votantes | `CreateSurveyResponseCommand` / `SurveyController`; validación FluentValidation |
| SQL Server | Dapper (`SurveyRepository`, `UserRepository`) |
| Un voto por persona | Chequeo en handler + restricción de negocio en repo / BD |
| Admin ve NPS cuando quiera | `GetNpsResultsQuery`, `AdminController` |
| Cierre por inactividad | Timer en SPA; servidor solo expira JWT |
| Bloqueo 3 intentos | `IdentityService` / cuenta bloqueada; mensaje en login sin scripts mágicos de “unlock” en prod |
| Refresh token | `RefreshToken.cs`; persistencia en `Users` |
| Dapper + FV + MediatR + AM | Como arriba; AM en cálculo NPS (`NpsComputationResult` → DTO) |

---

## Tests y CI

```bash
dotnet restore Nps.Api.slnx
dotnet test Nps.Api.slnx
```

Hay pruebas de calculadora NPS, validador del voto y perfil AutoMapper (`tests/NPS.Api.UnitTests`). CI en `.github/workflows/ci.yml` — restore, build, test.

`nuget.config` en raíz fuerza solo **nuget.org** para que la CI no dependa de feeds privados caídos o con 401.

---

## SQL incluido

| Archivo | Contenido |
|---------|-----------|
| `InitialSchema.sql` | BD, tablas, restricciones |
| `SeedData.sql` | `admin`, `voter01` |

---

## Sobre esta documentación

La fui armando sobre el código real: tablas de rutas, capas y scripts reflejan lo que hay en el tree, no un tutorial copiado. Si al ejecutar algo no cierra con lo leído acá, asumí primero diferencia de entorno (.NET SDK, SQL, `apiUrl`) antes de tirar la doc a la basura. Para el resto de mi perfil profesional: [yefersonsegura.com](https://yefersonsegura.com/).
