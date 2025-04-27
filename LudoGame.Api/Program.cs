using LudoGame.Domain;
using Microsoft.OpenApi.Models; // Swagger kræver denne namespace

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// 🌐 Add services to the container
// -----------------------------

builder.Services.AddControllers();

// ✅ Swagger – dokumentation til REST API (kun udvikling)
builder.Services.AddEndpointsApiExplorer(); // Swagger support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ludo API", Version = "v1" });
});

// ✅ CORS – tillader frontend at kalde API
// Vigtigt i udvikling når frontend (7097) kalder backend (7070)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ✅ Dependency Injection – registrér IGameController → GameController
// Bruges til at opnå løse koblinger (SOLID - D)
builder.Services.AddSingleton<IGameController>(sp =>
    new GameController(4) // ← antal spillere
);

var app = builder.Build();

// -----------------------------
// 🚀 Configure the HTTP request pipeline
// -----------------------------

// ✅ Swagger kun i udvikling (Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // genererer swagger.json
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ludo API v1");
    });
}

// ✅ CORS skal stå FØR routing
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
