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
/// Debug test to understand API responses
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
        // Arrange - Test with unique name and valid category
        var uniqueName = $"TestProduct{DateTime.Now.Ticks}";
        var createDto = new CreateProductDto
        {
            Name = uniqueName,
            Category = "elektronika", // Valid category
            Price = 99.99m,
            Quantity = 10
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/products", createDto);

        // Debug output
        _output.WriteLine($"Status Code: {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Response Content: {content}");

        // Try to read as ApiResponse
        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            try
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                _output.WriteLine($"Error Message: {errorResponse?.Message}");
                if (errorResponse?.ValidationErrors != null)
                {
                    foreach (var error in errorResponse.ValidationErrors)
                    {
                        _output.WriteLine($"Validation Error - Field: {error.Field}, Message: {error.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Failed to parse error response: {ex.Message}");
            }
        }
    }
}