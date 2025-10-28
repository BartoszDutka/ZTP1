using ZtpLab1.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ZtpLab1 API", Version = "v1" });
});

// Konfiguracja MongoDB i kontekstu bazy danych
var mongoDbSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>()
    ?? throw new InvalidOperationException("MongoDbSettings configuration is missing");
builder.Services.AddSingleton(mongoDbSettings);
builder.Services.AddSingleton<MongoDbContext>();

var app = builder.Build();

// Seed bazy danych przykładowymi produktami
// Ten kod uruchamia się przy starcie aplikacji i automatycznie wypełnia bazę
// przykładowymi produktami, jeśli jeszcze nie istnieją
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    await context.SeedProductsAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ZtpLab1 API V1");
        c.RoutePrefix = string.Empty; // To sprawi, że Swagger UI będzie dostępny na stronie głównej
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); // Mapowanie kontrolerów

app.Run();
