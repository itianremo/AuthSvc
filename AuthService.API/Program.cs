using AuthService.API.Middlewares;
using AuthService.Domain.Configs;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.DependencyInjection;
using AuthService.Application.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.WebHost.UseUrls("http://*:8080"); //Docker compatibility
//builder.WebHost.UseUrls("https://localhost:7065", "http://localhost:5021"); //local development 

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDashboard", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Bind InitConfig from appsettings.json
builder.Services.Configure<InitConfig>(builder.Configuration.GetSection("Init"));

// Authorization policies
var initConfig = builder.Configuration.GetSection("Init").Get<InitConfig>();
builder.Services.AddSingleton(initConfig);

builder.Services.AddAuthorization(options =>
{
    var perms = initConfig.Permissions;

    options.AddPolicy("CanManageAssigns", policy =>
        policy.RequireAssertion(context =>
        {
            var raw = context.User.FindFirst("permissions")?.Value;
            var perms = JsonConvert.DeserializeObject<List<string>>(raw ?? "[]");
            return perms.Contains("SuperAccess") || perms.Contains("ManageAssigns");
        }));

    options.AddPolicy("CanManageApps", policy =>
        policy.RequireAssertion(context =>
        {
            var raw = context.User.FindFirst("permissions")?.Value;
            var perms = JsonConvert.DeserializeObject<List<string>>(raw ?? "[]");
            return perms.Contains("SuperAccess") || perms.Contains("ManageApps");
        }));

    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireAssertion(context =>
        {
            var raw = context.User.FindFirst("permissions")?.Value;
            var perms = JsonConvert.DeserializeObject<List<string>>(raw ?? "[]");
            return perms.Contains("SuperAccess") || perms.Contains("ManageUsers");
        }));

    options.AddPolicy("CanManageRoles", policy =>
        policy.RequireAssertion(context =>
        {
            var raw = context.User.FindFirst("permissions")?.Value;
            var perms = JsonConvert.DeserializeObject<List<string>>(raw ?? "[]");
            return perms.Contains("SuperAccess") || perms.Contains("ManageRoles");
        }));

    options.AddPolicy("CanManagePermissions", policy =>
        policy.RequireAssertion(context =>
        {
            var raw = context.User.FindFirst("permissions")?.Value;
            var perms = JsonConvert.DeserializeObject<List<string>>(raw ?? "[]");
            return perms.Contains("SuperAccess") || perms.Contains("ManagePermissions");
        }));
});

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
});

builder.Services.AddHttpClient();

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repos and application services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Register DbInitializer
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Auth Service APIs", Version = "v1" });
    // Grouped docs
    //c.SwaggerDoc("auth", new() { Title = "Auth Endpoints", Version = "v1" });
    //c.SwaggerDoc("users", new() { Title = "User Endpoints", Version = "v1" });
    //c.SwaggerDoc("apps", new() { Title = "App Endpoints", Version = "v1" });
    //c.SwaggerDoc("roles", new() { Title = "Role Endpoints", Version = "v1" });
    //c.SwaggerDoc("permissions", new() { Title = "Permission Endpoints", Version = "v1" });
    //c.SwaggerDoc("dashboard", new() { Title = "Dashboard Endpoints", Version = "v1" });
    // JWT Auth in Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Dont Enter 'Bearer' followed by your token. Example: eyJhbGciOiJIUzI1NiIs..."
    });

    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactDashboard");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<AccountStatusMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();

// === Run DbInitializer after migrations ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await db.Database.MigrateAsync();

    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.SeedAsync();
}

app.Run();
