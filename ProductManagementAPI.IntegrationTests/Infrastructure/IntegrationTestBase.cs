using Xunit;

namespace ProductManagementAPI.IntegrationTests.Infrastructure;

/// <summary>
/// Bazowa klasa dla testów integracyjnych z automatycznym czyszczeniem bazy.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient(); // Klient HTTP dla wywołań API
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    // Automatyczne czyszczenie bazy po każdym teście
    public virtual async Task DisposeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }
}