# NPS — Ejercicio DEV-001

Soy **Yeferson Segura**, desarrollador de software en **móvil y web**. Mi portfolio profesional y formas de contacto están en **[yefersonsegura.com](https://yefersonsegura.com/)**. Este repo concentra la **API del ejercicio DEV-001**.

El back-end (.NET 10) incluye JWT con refresh, un voto por usuario y resultados NPS para admin. El cliente **Omega** (Angular 20) lo desarrollé en otro checkout (`npsApp`). **Omega** también da nombre a la **arquitectura / plantilla** que uso en **Flutter** (repos aparte). La siguiente guía enlaza esta API con la SPA sin asumir nada invisible.

---

## Repositorios y código

| Qué | Dónde |
|-----|--------|
| API + scripts SQL + tests | Este repo (`Nps.Api.slnx`) |
| SPA Omega ([Yeferson Segura](https://yefersonsegura.com/)) | Ruta habitual en mi máquina: `C:\Proyectos_Yeferson\Angular\npsApp` (otro checkout = ajustar solo paths) |

**Más de mi lado (contexto, no vive en este git)**  
Implementé **Omega Architecture para Flutter** como enfoque de estructura de proyecto y flujos Dart. Además trabajo en **[AbeyJS](https://abeyjs-fm.github.io/AbeyJS/)** (documentación oficial), framework todavía **en fase experimental** (interfaces y compatibilidad pueden cambiar hasta que lo estabilice).

**Capas (.NET)**  
`Domain` → entidades limpias. `Application` → casos de uso (MediatR), validación (FluentValidation), contratos, AutoMapper donde aporta (`NpsResultsProfile` → `NpsResultsDto`). `Infrastructure` → Dapper contra SQL Server, emisión JWT y hashing. `Api` → controladores finos, pipeline, OpenAPI/Swagger en desarrollo.

No uso Entity Framework de propósito: el enunciado pide Dapper y consultas explícitas; los repositorios concentran el SQL.

---

## Base de datos

1. Ejecutar en orden `Scripts/InitialSchema.sql` y `Scripts/SeedData.sql`.
2. Usuarios demo: `admin` y `voter01`; las contraseñas están solo en comentarios del seed (no las dejo como documento “vivo” fuera del script).
3. Apuntar la cadena a tu instancia. La cadena con nombre de equipo no va al remoto: partí de `src/Api/appsettings.Example.json` o usá [`dotnet user-secrets`](https://learn.microsoft.com/aspnet/core/security/app-secrets) para `ConnectionStrings:DefaultConnection`.

---

## Arrancar la API

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

La guía del **front** (contrato HTTP, rutas SPA, `apiUrl`, carpetas del código y build) está en el **README del proyecto Angular** (`npsApp`), no en este repo — para no duplicar y mantener una sola fuente de verdad del SPA.

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
