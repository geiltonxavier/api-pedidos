using System;
using System.Linq;
using System.Threading.RateLimiting;
using Api.Filters;
using Api.Middlewares;
using Application.Discounts;
using Application.Interfaces;
using Application.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext());

// Add services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// TODO: Em produção, adicionar autenticação JWT Bearer com validação via Identity Provider externo (ex: Microsoft Entra ID, Keycloak).
// Não implementado neste PoC para manter o foco nas regras de negócio e evitar auth fake com chave simétrica hardcoded.
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.Authority = "https://<identity-provider>";
//         options.Audience = "orders-api";
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true
//         };
//     });
// builder.Services.AddAuthorization();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new Asp.Versioning.UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Application.Validators.CreateOrderDtoValidator>();

builder.Services.AddDbContext<OrdersDbContext>(opt => opt.UseInMemoryDatabase("OrdersDb"));

// register pricing strategies
builder.Services.AddSingleton<IPricingStrategy, StandardDiscount>();
builder.Services.AddSingleton<IPricingStrategy, ExpressDiscount>();
builder.Services.AddSingleton<IPricingStrategy, SubscriptionDiscount>();
builder.Services.AddSingleton<DiscountFactory>();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IdempotencyFilter>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("fixed", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Orders API");
    });
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseRouting();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

