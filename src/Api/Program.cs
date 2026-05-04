using System.Text;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NPS.Api.Application.Common.Interfaces;
using NPS.Api.Infrastructure.Persistence;
using NPS.Api.Infrastructure.Identity;
using NPS.Api.Api.Services;
using NPS.Api.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// MediatR + validadores + repos Dapper; assembly application como ancla para registrar todo junto.
var applicationAssembly = typeof(NPS.Api.Application.Services.Auth.LoginCommand).Assembly;
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
builder.Services.AddAutoMapper(_ => { }, applicationAssembly);
builder.Services.AddValidatorsFromAssembly(applicationAssembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(NPS.Api.Application.Common.Behaviors.ValidationBehavior<,>));

builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// JWT: mismos issuer/audience que emite IdentityService; ClockSkew en cero para no regalar segundos gratis.
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings.GetValue<string>("Secret")!;

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
        ValidAudience = jwtSettings.GetValue<string>("Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            if (HttpMethods.Options.Equals(context.HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
                context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                  ?.Where(static o => !string.IsNullOrWhiteSpace(o)).Select(static o => o.TrimEnd('/')).Distinct().ToArray()
                  ?? [];
if (corsOrigins.Length == 0)
    corsOrigins = ["http://localhost:4200", "https://localhost:4200"];

builder.Services.AddCors(o =>
{
    o.AddPolicy("Angular", p =>
    {
        p.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Documento OpenAPI generado; la UI de Swagger es sólo comodidad en Development.
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info.Title = "DEV-001 · NPS Survey API";
        document.Info.Version = "v1";

        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Introduce el token JWT: {token}"
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes.Add(JwtBearerDefaults.AuthenticationScheme, scheme);

        document.Security ??= new List<OpenApiSecurityRequirement>();
        var requirement = new OpenApiSecurityRequirement();
        var schemeRef = new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document, null);
        
        requirement.Add(schemeRef, new List<string>());
        document.Security.Add(requirement);

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Orden habitual: CORS antes de auth; errores no atrapados salen como JSON en el middleware.
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("Angular");
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // El paquete oficial no trae UI integrada; Swagger apunta al JSON estático.
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "DEV-001 NPS v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
