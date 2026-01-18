using DigitalBank.Api.Middlewares;
using DigitalBank.Api.Services;
using DigitalBank.Application.Interfaces;
using DigitalBank.Application.Options;
using DigitalBank.Persistence.PersistenceServiceRegistration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Debugging;
using Stripe;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog self-log (DIQQET: System.IO.File yaziriq ki conflict olmasin)
SelfLog.Enable(msg =>
{
    Debug.WriteLine(msg);
    System.IO.File.AppendAllText("serilog-selflog.txt", msg + Environment.NewLine);
});

// Serilog main
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// Layers
builder.Services.AddPersistenceServiceRegistration(builder.Configuration);

// Stripe global api key
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// HttpContext based stuff (API layer)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
builder.Services.AddScoped<IAuthCookieService, AuthCookieService>();

// JWT Bearer validate
var jwt = builder.Configuration.GetSection("JwtOptions").Get<JwtOption>()
          ?? throw new InvalidOperationException("JwtOptions config is missing.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddAuthorization();

// SignalR (1 defe kifayetdir)
builder.Services.AddSignalR();

builder.Services.AddScoped<INotificationPushService, DigitalBank.Api.Realtime.NotificationPushService>();
builder.Services.AddScoped<IChatPushService, DigitalBank.Api.Realtime.ChatPushService>();

// CORS (cookie refresh istifadə edirsənsə mütləq AllowCredentials olmalıdır)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173"
            // buraya real frontend domainini də əlavə edəcəksən
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger + JWT
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DigitalBank API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Enter JWT Bearer token only"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Middleware order (sənin kimi: correlation + global exception)
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

// SignalR hubs
app.MapHub<DigitalBank.Api.Hubs.NotificationHub>("/hubs/notifications");
app.MapHub<DigitalBank.Api.Hubs.ChatHub>("/hubs/chat");

app.MapControllers();

// Role seed (sadə variant)
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

app.Run();
