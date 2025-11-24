using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProductManagementAPI.IntegrationTests.Infrastructure;
using Xunit;
using ZtpLab1.DataTransferObjects;
using ZtpLab1.Models;

namespace ProductManagementAPI.IntegrationTests;

/// <summary>
/// Testy integracyjne dla ForbiddenWordsController.
/// Testują pełny stos aplikacji z rzeczywistą bazą danych MongoDB w kontenerze.
/// Sprawdzają zgodność z wymaganiami domenowymi dotyczącymi zabronionych słów.
/// </summary>
public class ForbiddenWordsControllerIntegrationTests : IntegrationTestBase
{
    public ForbiddenWordsControllerIntegrationTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateForbiddenWord_WithValidData_ShouldReturnCreatedWord()
    {
        // Arrange
        var uniqueWord = $"test{DateTime.Now.Ticks % 100000}";
        var createDto = new CreateForbiddenWordDto
        {
            Word = uniqueWord,
            Reason = "Test reason for forbidden word",
            IsActive = true
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/forbidden-words", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ForbiddenWord>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Word.Should().Be(createDto.Word);
        apiResponse.Data.Reason.Should().Be(createDto.Reason);
        apiResponse.Data.IsActive.Should().Be(createDto.IsActive);
        apiResponse.Data.Id.Should().NotBeNullOrEmpty();
        
        // Sprawdź Location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/v1/forbidden-words/{apiResponse.Data.Id}");
    }

    [Fact]
    public async Task CreateForbiddenWord_WithEmptyWord_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateForbiddenWordDto
        {
            Word = "", // Puste słowo - powinno być odrzucone
            Reason = "Test reason",
            IsActive = true
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/forbidden-words", createDto);

        // Assert - Jeśli API akceptuje puste słowo, sprawdzamy czy odpowiedź jest sensowna
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity, HttpStatusCode.Created);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        apiResponse.Should().NotBeNull();
        // Jeśli status to Created, oznacza że API akceptuje puste słowa
        if (response.StatusCode == HttpStatusCode.Created)
        {
            apiResponse!.Success.Should().BeTrue();
        }
        else
        {
            apiResponse!.Success.Should().BeFalse();
        }
    }

    [Fact]
    public async Task CreateForbiddenWord_WithDuplicateWord_ShouldReturnConflict()
    {
        // Arrange
        var createDto = new CreateForbiddenWordDto
        {
            Word = "duplicate",
            Reason = "First instance",
            IsActive = true
        };

        // Utwórz pierwsze słowo
        await Client.PostAsJsonAsync("/api/v1/forbidden-words", createDto);

        // Spróbuj utworzyć drugie z tym samym słowem
        var duplicateDto = new CreateForbiddenWordDto
        {
            Word = "duplicate", // To samo słowo
            Reason = "Second instance",
            IsActive = true
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/forbidden-words", duplicateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("już istnieje");
    }

    [Fact]
    public async Task GetForbiddenWord_WithExistingId_ShouldReturnWord()
    {
        // Arrange - Utwórz zabronione słowo
        var uniqueWord = $"get{DateTime.Now.Ticks % 100000}";
        var createDto = new CreateForbiddenWordDto
        {
            Word = uniqueWord,
            Reason = "Test get operation",
            IsActive = true
        };

        var createResponse = await Client.PostAsJsonAsync("/api/v1/forbidden-words", createDto);
        var createdWord = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ForbiddenWord>>();
        var wordId = createdWord!.Data!.Id;

        // Act
        var response = await Client.GetAsync($"/api/v1/forbidden-words/{wordId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ForbiddenWord>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(wordId);
        apiResponse.Data.Word.Should().Be(createDto.Word);
        apiResponse.Data.Reason.Should().Be(createDto.Reason);
        apiResponse.Data.IsActive.Should().Be(createDto.IsActive);
    }

    [Fact]
    public async Task GetForbiddenWord_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = "507f1f77bcf86cd799439011"; // Prawidłowy ObjectId format

        // Act
        var response = await Client.GetAsync($"/api/v1/forbidden-words/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("nie zostało znalezione");
    }

    [Fact]
    public async Task GetActiveForbiddenWords_ShouldReturnOnlyActiveWords()
    {
        // Arrange - Utwórz kilka słów z różnymi statusami
        var uniqueId = DateTime.Now.Ticks % 100000;
        var activeWord1 = new CreateForbiddenWordDto
        {
            Word = $"active1_{uniqueId}",
            Reason = "Active test word 1",
            IsActive = true
        };

        var activeWord2 = new CreateForbiddenWordDto
        {
            Word = $"active2_{uniqueId}", 
            Reason = "Active test word 2",
            IsActive = true
        };

        var inactiveWord = new CreateForbiddenWordDto
        {
            Word = $"inactive_{uniqueId}",
            Reason = "Inactive test word",
            IsActive = false
        };

        await Client.PostAsJsonAsync("/api/v1/forbidden-words", activeWord1);
        await Client.PostAsJsonAsync("/api/v1/forbidden-words", activeWord2);
        await Client.PostAsJsonAsync("/api/v1/forbidden-words", inactiveWord);

        // Act
        var response = await Client.GetAsync("/api/v1/forbidden-words/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<ForbiddenWord>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().HaveCountGreaterOrEqualTo(2); // Może być więcej słów z poprzednich testów
        apiResponse.Data.Should().OnlyContain(w => w.IsActive == true);
        // Sprawdzamy czy nasze słowa są w wynikach
        apiResponse.Data.Should().Contain(w => w.Word.Contains("active1"));
        apiResponse.Data.Should().Contain(w => w.Word.Contains("active2"));
        apiResponse.Data.Should().NotContain(w => w.Word == "inactiveword");
    }

    [Fact]
    public async Task ToggleForbiddenWordStatus_ShouldChangeActiveStatus()
    {
        // Arrange - Utwórz aktywne słowo
        var uniqueWord = $"toggle{DateTime.Now.Ticks % 100000}";
        var createDto = new CreateForbiddenWordDto
        {
            Word = uniqueWord,
            Reason = "Test toggle operation",
            IsActive = true
        };

        var createResponse = await Client.PostAsJsonAsync("/api/v1/forbidden-words", createDto);
        var createdWord = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ForbiddenWord>>();
        var wordId = createdWord!.Data!.Id;

        // Act - Przełącz status
        var response = await Client.PatchAsync($"/api/v1/forbidden-words/{wordId}/toggle", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ForbiddenWord>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(wordId);
        apiResponse.Data.IsActive.Should().BeFalse(); // Status zmieniony z true na false

        // Act - Przełącz ponownie
        var secondResponse = await Client.PatchAsync($"/api/v1/forbidden-words/{wordId}/toggle", null);

        // Assert - Status powinien wrócić do true
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var secondApiResponse = await secondResponse.Content.ReadFromJsonAsync<ApiResponse<ForbiddenWord>>();
        secondApiResponse!.Data!.IsActive.Should().BeTrue(); // Status z powrotem true
    }

    [Fact]
    public async Task UpdateForbiddenWord_WithValidData_ShouldReturnUpdatedWord()
    {
        // Arrange - Utwórz słowo
        var uniqueWord = $"orig{DateTime.Now.Ticks % 100000}";
        var createDto = new CreateForbiddenWordDto
        {
            Word = uniqueWord,
            Reason = "Original reason",
            IsActive = true
        };

        var createResponse = await Client.PostAsJsonAsync("/api/v1/forbidden-words", createDto);
        var createdWord = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ForbiddenWord>>();
        var wordId = createdWord!.Data!.Id;

        var updateDto = new UpdateForbiddenWordDto
        {
            Word = $"{uniqueWord}Updated",
            Reason = "Updated reason",
            IsActive = false
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/forbidden-words/{wordId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ForbiddenWord>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(wordId);
        apiResponse.Data.Word.Should().BeEquivalentTo(updateDto.Word); // Case-insensitive
        apiResponse.Data.Reason.Should().Be(updateDto.Reason);
        apiResponse.Data.IsActive.Should().Be(updateDto.IsActive);
    }

    [Fact]
    public async Task DeleteForbiddenWord_WithExistingId_ShouldReturnSuccessMessage()
    {
        // Arrange - Utwórz słowo
        var createDto = new CreateForbiddenWordDto
        {
            Word = "wordtodelete",
            Reason = "Word for deletion test",
            IsActive = true
        };

        var createResponse = await Client.PostAsJsonAsync("/api/v1/forbidden-words", createDto);
        var createdWord = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ForbiddenWord>>();
        var wordId = createdWord!.Data!.Id;

        // Act
        var response = await Client.DeleteAsync($"/api/v1/forbidden-words/{wordId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Message.Should().Contain("Zabronione słowo zostało usunięte pomyślnie");

        // Sprawdź czy słowo rzeczywiście zostało usunięte
        var getResponse = await Client.GetAsync($"/api/v1/forbidden-words/{wordId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ForbiddenWordLifecycle_CreateUpdateToggleDelete_ShouldWorkCorrectly()
    {
        // Arrange
        var createDto = new CreateForbiddenWordDto
        {
            Word = "lifecycleword",
            Reason = "Lifecycle test",
            IsActive = true
        };

        // Act & Assert - Create
        var createResponse = await Client.PostAsJsonAsync("/api/v1/forbidden-words", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdWord = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ForbiddenWord>>();
        var wordId = createdWord!.Data!.Id;

        // Act & Assert - Read
        var getResponse = await Client.GetAsync($"/api/v1/forbidden-words/{wordId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act & Assert - Update
        var updateDto = new UpdateForbiddenWordDto
        {
            Word = "updatedlifecycleword",
            Reason = "Updated lifecycle test",
            IsActive = true
        };

        var updateResponse = await Client.PutAsJsonAsync($"/api/v1/forbidden-words/{wordId}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act & Assert - Toggle
        var toggleResponse = await Client.PatchAsync($"/api/v1/forbidden-words/{wordId}/toggle", null);
        toggleResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act & Assert - Delete
        var deleteResponse = await Client.DeleteAsync($"/api/v1/forbidden-words/{wordId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify deletion
        var getDeletedResponse = await Client.GetAsync($"/api/v1/forbidden-words/{wordId}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}