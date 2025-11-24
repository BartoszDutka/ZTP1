using ZtpLab1.Data;
using ZtpLab1.Repositories;
using ZtpLab1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Konfiguracja CORS dla komunikacji z frontendem
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "System Zarządzania Produktami - Laboratorium ZTP", 
        Version = "v1"
    });
    
    // Grupowanie endpoint-ów
    c.TagActionsBy(api =>
    {
        if (api.ActionDescriptor.RouteValues["controller"]?.ToString() == "Products")
        {
            // Sprawdzanie czy to endpoint historii produktów
            var actionName = api.ActionDescriptor.RouteValues["action"]?.ToString();
            if (actionName == "GetProductHistory" || actionName == "GetAllHistory")
                return new[] { "📊 Historia Produktów (Ocena 4.5)" };
            return new[] { "🛍️ Produkty (Ocena 3.0)" };
        }
        if (api.ActionDescriptor.RouteValues["controller"]?.ToString() == "ForbiddenWords")
            return new[] { "🚫 Zabronione Słowa (Ocena 3.5)" };
        return new[] { "Inne" };
    });
    
    c.DocInclusionPredicate((name, api) => true);
});

// Konfiguracja MongoDB i kontekstu bazy danych
var mongoDbSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>()
    ?? throw new InvalidOperationException("MongoDbSettings configuration is missing");
builder.Services.AddSingleton(mongoDbSettings);
builder.Services.AddSingleton<MongoDbContext>();

// Konfiguracja N-Layer Architecture z Dependency Injection
// Repository Layer - abstrakcja dostępu do danych
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IForbiddenWordRepository, ForbiddenWordRepository>();
builder.Services.AddScoped<IProductHistoryRepository, ProductHistoryRepository>();

// Service Layer - logika biznesowa
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IForbiddenWordService, ForbiddenWordService>();
builder.Services.AddScoped<IProductHistoryService, ProductHistoryService>();

var app = builder.Build();

// Seed bazy danych przykładowymi produktami i zabronionymi słowami
// Ten kod uruchamia się przy starcie aplikacji i automatycznie wypełnia bazę
// przykładowymi danymi, jeśli jeszcze nie istnieją
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    await context.SeedDataAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "System Zarządzania Produktami V1");
        c.RoutePrefix = string.Empty; // Swagger UI na stronie głównej
        c.DocumentTitle = "ZTP Lab 1 - API Documentation";
        c.DefaultModelsExpandDepth(-1); // Zwijanie modeli dla przejrzystości
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        c.EnableDeepLinking();
        c.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers(); // Mapowanie kontrolerów

app.Run();
