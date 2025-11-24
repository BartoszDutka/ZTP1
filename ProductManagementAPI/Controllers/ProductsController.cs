using Microsoft.AspNetCore.Mvc;
using ZtpLab1.DataTransferObjects;
using ZtpLab1.Services;

namespace ZtpLab1.Controllers
{
    /// <summary>
    /// Kontroler obsługujący operacje CRUD na produktach.
    /// Implementuje REST API zgodnie z konwencją i wzorcami N-Layer Architecture.
    /// </summary>
    [ApiController]
    [Route("api/v1/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        /// <summary>
        /// Konstruktor wykorzystujący Dependency Injection do wstrzyknięcia serwisu produktów.
        /// Kontroler pełni rolę koordynatora między API a warstwą biznesową.
        /// </summary>
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// GET /api/v1/products
        /// Pobiera listę wszystkich produktów (tylko Id i Name).
        /// </summary>
        /// <returns>Lista produktów z podstawowymi informacjami</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<Models.Product>>>> GetProducts()
        {
            var result = await _productService.GetAllBasicAsync();
            
            if (!result.Success)
            {
                return StatusCode(result.ErrorCode ?? 500, result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// GET /api/v1/products/{id}
        /// Pobiera szczegółowe informacje o pojedynczym produkcie.
        /// </summary>
        /// <param name="id">ID produktu</param>
        /// <returns>Szczegóły produktu lub 404 Not Found</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Models.Product>>> GetProduct(string id)
        {
            var result = await _productService.GetByIdAsync(id);
            
            if (!result.Success)
            {
                return StatusCode(result.ErrorCode ?? 500, result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// POST /api/v1/products
        /// Tworzy nowy produkt z walidacją domenową przy użyciu wzorca Specification.
        /// </summary>
        /// <param name="createDto">Dane nowego produktu</param>
        /// <returns>201 Created z utworzonym produktem lub odpowiedni kod błędu</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Models.Product>>> CreateProduct(CreateProductDto createDto)
        {
            var result = await _productService.CreateAsync(createDto);
            
            if (!result.Success)
            {
                return StatusCode(result.ErrorCode ?? 500, result);
            }
            
            // Zwracamy 201 Created z lokalizacją nowego zasobu
            return CreatedAtAction(
                nameof(GetProduct),
                new { id = result.Data!.Id },
                result
            );
        }

        /// <summary>
        /// PUT /api/v1/products/{id}
        /// Aktualizuje istniejący produkt z pełną walidacją domenową przy użyciu wzorca Specification.
        /// </summary>
        /// <param name="id">ID produktu do aktualizacji</param>
        /// <param name="updateDto">Nowe dane produktu</param>
        /// <returns>200 OK z zaktualizowanym produktem lub odpowiedni kod błędu</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<Models.Product>>> UpdateProduct(string id, UpdateProductDto updateDto)
        {
            var result = await _productService.UpdateAsync(id, updateDto);
            
            if (!result.Success)
            {
                return StatusCode(result.ErrorCode ?? 500, result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// DELETE /api/v1/products/{id}
        /// Usuwa produkt z bazy danych.
        /// </summary>
        /// <param name="id">ID produktu do usunięcia</param>
        /// <returns>200 OK z komunikatem sukcesu lub odpowiedni kod błędu</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteProduct(string id)
        {
            var result = await _productService.DeleteAsync(id);
            
            if (!result.Success)
            {
                return StatusCode(result.ErrorCode ?? 500, result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// GET /api/v1/products/history
        /// Pobiera całą historię zmian wszystkich produktów w systemie.
        /// Endpoint wyświetla wszystkie operacje CREATE, UPDATE i DELETE wykonane na produktach
        /// wraz z informacjami o poprzednim i aktualnym stanie oraz datą modyfikacji.
        /// </summary>
        /// <returns>Lista wszystkich wpisów historii produktów w systemie</returns>
        /// <response code="200">Pomyślnie pobrano historię wszystkich produktów</response>
        /// <response code="500">Błąd serwera podczas pobierania historii</response>
        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Models.ProductHistory>>>> GetAllHistory([FromServices] IProductHistoryService historyService)
        {
            var result = await historyService.GetAllHistoryAsync();
            
            if (!result.Success)
            {
                return StatusCode(result.ErrorCode ?? 500, result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// GET /api/v1/products/{id}/history
        /// Pobiera historię zmian dla określonego produktu.
        /// </summary>
        /// <param name="id">ID produktu</param>
        /// <returns>Historia zmian produktu</returns>
        [HttpGet("{id}/history")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Models.ProductHistory>>>> GetProductHistory(string id, [FromServices] IProductHistoryService historyService)
        {
            var result = await historyService.GetProductHistoryAsync(id);
            
            if (!result.Success)
            {
                return StatusCode(result.ErrorCode ?? 500, result);
            }
            
            return Ok(result);
        }
    }
}