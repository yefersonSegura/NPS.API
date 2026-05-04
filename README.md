# NPS — API · DEV-001

Servicio REST en **.NET 10** para el ejercicio **DEV-001**: login con JWT y **refresh token**, roles **Administrador / Votante**, encuesta NPS con **único voto por usuario** (score **0–10**) y lectura agregada de métricas NPS solo para rol admin. Persistencia **SQL Server** mediante **Dapper**; sin Entity Framework Core en esta solución por requisitos de práctica/consultas explícitas.

| Componente | Repositorio / artefacto |
|------------|-------------------------|
| Esta API (+ scripts SQL + tests) | [NPS.API](https://github.com/yefersonSegura/NPS.API) · raíz [`Nps.Api.slnx`](Nps.Api.slnx) |
| Cliente SPA (Omega, Angular 20) | [NPS.FRONT](https://github.com/yefersonSegura/NPS.FRONT) |

**Autoría y contacto:** [Yeferson Segura](https://yefersonsegura.com/)

---

## Arquitectura (.NET)

| Proyecto | Rol |
|----------|-----|
| `Domain` | Entidades y modelo de dominio mínimo. |
| `Application` | Casos de uso (**MediatR**), contratos (`I*` repositories), validación (**FluentValidation**), **AutoMapper** donde une dominio ↔ DTO público (`NpsResultsProfile` → `NpsResultsDto`). |
| `Infrastructure` | **Dapper**, fábrica de conexión, repositorios, **IdentityService** (BCrypt + emisión JWT + refresh persistente). |
| `Api` | Controladores livianos, pipeline HTTP, OpenAPI (**Microsoft.AspNetCore.OpenApi**) + Swagger UI en desarrollo, **CORS** configurable, middleware de errores en JSON (**camelCase** alineado con serialización MVC). |

**MediatR** orquesta comandos/consultas; **FluentValidation** valida payloads antes del handler mediante pipeline behavior estándar.

---

## Solución local

Solución textual **SLNX** incluida como `Nps.Api.slnx` (proyectos bajo `src/` y `tests/`).

```text
src/Api           → Host ASP.NET Core
src/Application
src/Domain
src/Infrastructure
tests/NPS.Api.UnitTests
Scripts/          → InitialSchema.sql, SeedData.sql
```

---

## Base de datos (orden obligatorio antes de ejecutar la API)

1. **`Scripts/InitialSchema.sql`** — creación condicional de `NpsDb` (si no existe), tablas `Roles`, `Users`, `SurveyResponses` y FK/CHK alineadas al código (`UserId` único en respuestas, score **0–10**).
2. **`Scripts/SeedData.sql`** — usuarios de prueba **`admin`** y **`voter01`**; hashes BCrypt documentados solo en comentarios del script.

Ejecutar los scripts contra la **misma instancia** que usará la cadena de la API (SSMS, Azure Data Studio, `sqlcmd`, etc.). Sin estos pasos, el host puede levantar pero fallará cualquier lectura/escritura a datos ausentes.

**Cadena de conexión**

- **`src/Api/appsettings.json`** no debe versionarse con secretos locales: figura en `.gitignore` en entornos de trabajo habitual; modelo de ejemplo en **`appsettings.Example.json`**.
- Configuración recomendada: copia local ignorada por git y/o [`dotnet user-secrets`](https://learn.microsoft.com/aspnet/core/app-secrets) para `ConnectionStrings:DefaultConnection` apuntando a `NpsDb`.

---

## Ejecución local de la API

```bash
cd src/Api
dotnet run --launch-profile https
```

Puertos típicos (`Properties/launchSettings.json`): **HTTPS** `https://localhost:7070`, **HTTP** `http://localhost:5140`. Si el proceso mantiene bloqueadas las DLL, detenga la instancia en ejecución antes de **`dotnet build`**.

En **Development**: documento OpenAPI en **`/openapi/v1.json`**; Swagger UI consumiendo ese recurso (**`Program.cs`**). **CORS:** sección **`Cors:AllowedOrigins`**; por defecto orígenes `localhost:4200` (SPA). Extender lista si cambia puerto/host del cliente.

---

## Seguridad y modelo de autorización

- **JWT** (`JwtSettings`): `Issuer`, `Audience`, **secret simétrica**, tiempo de vida de acceso (p. ej. **5 minutos**). Claims relevantes: `sub` estable numérico = **UserId**, `ClaimTypes.Name`, `ClaimTypes.Role` como **string `"1"` / `"2"`** coherentes con `[Authorize(Roles = "…")]`.
- **Refresh token** aleatorio persistido en `Users`; rotación en operación refresh; cliente Angular documentado en repo **NPS.FRONT**.
- **Lockout:** tras tres intentos fallidos de contraseña, bloqueo temporal con campos estándar en `Users`; al vencer la ventana, el flujo de login restablece contadores donde aplica (`IdentityService`).
- **Políticas de errores**: respuestas envolvente en JSON camelCase tanto en rutas válidas como en excepciones filtradas por middleware.

---

## Endpoints públicos vs protegidos (resumen operativo)

| Área | Rutas relativas típicas | Notas |
|------|--------------------------|-------|
| Anónimas | `POST api/Auth/Login`, `POST api/Auth/refresh` | Emisión o rotación de tokens. |
| Votante (rol 2) | `POST api/Survey/vote` | Body `{ "score": 0–10 }`; usuario deducido de JWT (`ICurrentUserService`). |
| Administrador (rol 1) | `GET api/Admin/results` | Agregados NPS. |

Contrato HTTP detallado y convenciones del cliente: **[README · NPS.FRONT](https://github.com/yefersonSegura/NPS.FRONT)** — evita divergencias entre documentos.

---

## Trazabilidad requisitos DEV-001 ↔ código

| Requisito | Ubicación / notas |
|-----------|---------------------|
| Roles admin / votante | `Roles`/`Users` en SQL; `[Authorize]` en controladores; claim `role` desde JWT |
| Login usuario / contraseña | `Login.cs`, `AuthController`, `IdentityService` |
| Escala **0–10**, solo rol votante en voto | `CreateSurveyResponseCommand`, validador FluentValidation; `SurveyController` |
| SQL Server explícito | `UserRepository`, `SurveyRepository` (Dapper) |
| Un voto por usuario | chequeo aplicación + unicidad **`UserId`** en `SurveyResponses` |
| Admin consulta NPS cuando quiera | `GetNpsResultsQuery`, `AdminController`, `NpsCalculator` |
| Sesión SPA ~**5 min sin actividad** | implementado en el cliente (**NPS.FRONT**); servidor impone TTL JWT |
| Tras **3** fallos, bloqueo de cuenta | `IdentityService.RegisterFailedAttempt` (+ columnas seed) |
| JWT en cada request protegida | Bearer + validación JwtBearer middleware |
| **Refresh token** | `RefreshToken.cs`, tabla `Users` |
| Stack pedido | Dapper · FluentValidation · MediatR · AutoMapper (mapeo NPS DTOs) |

---

## Pruebas y CI local

```bash
dotnet restore Nps.Api.slnx
dotnet test Nps.Api.slnx
```

**`tests/NPS.Api.UnitTests`**: NPS calculator, validación de comando de voto, perfil AutoMapper. **`/.github/workflows/ci.yml`** — restore **nuget.org** vía **`nuget.config`** (reduce fallos CI por feeds privados o HTTP 401).

---

## Artefactos SQL en el repositorio

| Archivo | Propósito |
|---------|-----------|
| `Scripts/InitialSchema.sql` | Esquema e integridad (`NpsDb`) |
| `Scripts/SeedData.sql` | Datos demo y comentarios de contraseña |
