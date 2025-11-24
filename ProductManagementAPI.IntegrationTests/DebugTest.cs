using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProductManagementAPI.IntegrationTests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using ZtpLab1.DataTransferObjects;
using ZtpLab1.Models;

namespace ProductManagementAPI.IntegrationTests;

/// <summary>
/// Test debugowania do analizy odpowiedzi API
/// </summary>
public class DebugTest : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;

    public DebugTest(IntegrationTestWebAppFactory factory, ITestOutputHelper output) : base(factory)
    {
        _output = output;
    }

    [Fact]
    public async Task DebugCreateProduct()
    {

        var uniqueName = $"TestProduct_{Guid.NewGuid().ToString("N")[..8]}";
        var createDto = new CreateProductDto
        {
            Name = uniqueName,
            Category = "elektronika", // Prawidłowa kategoria
            Price = 99.99m,
            Quantity = 10
        };

        // Wykonanie
        var response = await Client.PostAsJsonAsync("/api/v1/products", createDto);

        // Wynik debugowania
        _output.WriteLine($"Kod statusu: {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Zawartość odpowiedzi: {content}");

        // Próba odczytu jako ApiResponse
        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            try
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                _output.WriteLine($"Komunikat błędu: {errorResponse?.Message}");
                if (errorResponse?.ValidationErrors != null)
                {
                    foreach (var error in errorResponse.ValidationErrors)
                    {
                        _output.WriteLine($"Błąd walidacji - Pole: {error.Field}, Komunikat: {error.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Niepowodzenie parsowania odpowiedzi błędu: {ex.Message}");
            }
        }
    }
}