using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProductManagementAPI.IntegrationTests.Infrastructure;
using Xunit;
using ZtpLab1.DataTransferObjects;

namespace ProductManagementAPI.IntegrationTests;

public class DebugCreateTest : IntegrationTestBase
{
    public DebugCreateTest(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Debug_CreateProduct_InvestigateValidationRules()
    {
        // Test różnych nazw produktów
        var testNames = new[]
        {
            $"Get{DateTime.Now.Ticks % 1000000}",  // Oryginalna nazwa z błędem
            $"Test{DateTime.Now.Ticks % 100000}",  // Krótsza wersja
            $"Prod{DateTime.Now.Ticks % 10000}",   // Jeszcze krótsza
            "Simple123",                            // Prosta nazwa
            "X123",                                 // Bardzo krótka
        };

        foreach (var name in testNames)
        {
            Console.WriteLine($"Testing product name: '{name}' (length: {name.Length})");
            
            var createDto = new CreateProductDto
            {
                Name = name,
                Category = "elektronika",
                Price = 55.00m,
                Quantity = 3
            };

            var response = await Client.PostAsJsonAsync("/api/v1/products", createDto);
            Console.WriteLine($"Status: {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {content}");
            Console.WriteLine("---");
        }
    }
}