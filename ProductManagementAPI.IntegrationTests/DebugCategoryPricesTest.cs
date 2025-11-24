using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProductManagementAPI.IntegrationTests.Infrastructure;
using Xunit;
using ZtpLab1.DataTransferObjects;

namespace ProductManagementAPI.IntegrationTests;

public class DebugCategoryPricesTest : IntegrationTestBase
{
    public DebugCategoryPricesTest(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Debug_TestMinimumPricesForCategories()
    {
        // Test minimalne ceny dla różnych kategorii
        var testCases = new[]
        {
            new { Category = "elektronika", Price = 49.99m },    // Za mało
            new { Category = "elektronika", Price = 50.00m },    // OK
            new { Category = "książki", Price = 0.01m },         // Test minimum
            new { Category = "książki", Price = 5.00m },         
            new { Category = "odzież", Price = 0.01m },          // Test minimum
            new { Category = "odzież", Price = 10.00m },         
        };

        foreach (var testCase in testCases)
        {
            Console.WriteLine($"Testing category: '{testCase.Category}' with price: {testCase.Price}");
            
            var createDto = new CreateProductDto
            {
                Name = $"Test_{Guid.NewGuid().ToString("N")[..6]}",
                Category = testCase.Category,
                Price = testCase.Price,
                Quantity = 1
            };

            var response = await Client.PostAsJsonAsync("/api/v1/products", createDto);
            Console.WriteLine($"Status: {response.StatusCode}");
            
            if (response.StatusCode != HttpStatusCode.Created)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {content}");
            }
            Console.WriteLine("---");
        }
    }
}