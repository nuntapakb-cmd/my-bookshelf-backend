using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyBookshelf.Api.Data;
using MyBookshelf.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB (SQLite)
var conn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=my_bookshelf.db";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(conn));

// register TokenService
builder.Services.AddScoped<TokenService>();

// CORS - allow local Angular dev at :4200
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocal", policy =>
    {
        policy.WithOrigins(
                    "http://localhost:4200"
                    "https://mybookshelf-client.netlify.app"
                )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT Auth
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSection["Key"] ?? throw new Exception("JWT key missing"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})

.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true, // Check token time
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger (เปิดทั้ง dev + production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyBookshelf.Api v1");
});


app.UseHttpsRedirection();

app.UseCors("AllowLocal");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

app.Run();
