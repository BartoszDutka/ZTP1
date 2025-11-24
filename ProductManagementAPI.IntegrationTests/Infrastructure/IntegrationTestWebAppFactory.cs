using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using DotNet.Testcontainers.Builders;

namespace ProductManagementAPI.IntegrationTests.Infrastructure;

/// <summary>
/// Fabryka testowa do tworzenia aplikacji ASP.NET Core z kontenerem MongoDB.
/// Zapewnia izolowane środowisko testowe z rzeczywistą bazą danych.
/// </summary>
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Kontener MongoDB dla testów - jeden na klasę testową
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder()
        .WithImage("mongo:7.0")
        .WithPortBinding(27017, true)
        .WithEnvironment("MONGO_INITDB_DATABASE", "ProductManagementTestDb")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;
    public IMongoDatabase Database { get; private set; } = null!;

    // Konfiguruje aplikację dla testów - zastępuje produkcyjną konfigurację
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear(); // Usuń produkcyjną konfigurację
            
            // Konfiguracja testowa z kontenerem MongoDB
            var testConfig = new Dictionary<string, string>
            {
                ["MongoDbSettings:ConnectionString"] = ConnectionString,
                ["MongoDbSettings:DatabaseName"] = "ProductManagementTestDb",
                ["Logging:LogLevel:Default"] = "Warning"
            };

            config.AddInMemoryCollection(testConfig!);
        });

        builder.UseEnvironment("Testing");
    }

    // Inicjalizuje kontener MongoDB przed testami
    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
        ConnectionString = _mongoContainer.GetConnectionString();
        
        var mongoClient = new MongoClient(ConnectionString);
        Database = mongoClient.GetDatabase("ProductManagementTestDb");
        
        // Weryfikuj połączenie
        await Database.RunCommandAsync<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1));
    }

    // Czyści zasoby po zakończeniu testów
    public new async Task DisposeAsync()
    {
        await _mongoContainer.StopAsync();
        await _mongoContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    // Resetuje bazę danych między testami dla izolacji
    public async Task ResetDatabaseAsync()
    {
        var collections = await Database.ListCollectionNamesAsync();
        
        await collections.ForEachAsync(async collectionName =>
        {
            await Database.DropCollectionAsync(collectionName);
        });
    }
}