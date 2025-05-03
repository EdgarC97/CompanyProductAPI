using CompanyProductAPI.Models;
using CompanyProductAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CompanyProductAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        /// <summary>
        /// Gets all products for a company
        /// </summary>
        /// <param name="companyId">The id of the company</param>
        /// <returns>A list of products</returns>
        [HttpGet("companies/{companyId}/products")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCompany(int companyId)
        {
            try
            {
                var products = await _productService.GetProductsByCompanyIdAsync(companyId);
                return Ok(products);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Company with ID {companyId} not found");
            }
        }

        /// <summary>
        /// Gets a specific product by id
        /// </summary>
        /// <param name="id">The id of the product</param>
        /// <returns>The product if found</returns>
        [HttpGet("products/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        /// <summary>
        /// Creates a new product for a company
        /// </summary>
        /// <param name="companyId">The id of the company</param>
        /// <param name="product">The product to create</param>
        /// <returns>The created product</returns>
        [HttpPost("companies/{companyId}/products")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Product>> CreateProduct(int companyId, Product product)
        {
            // Ensure the companyId in the URL matches the one in the product
            product.CompanyId = companyId;

            try
            {
                var id = await _productService.CreateProductAsync(product);
                product.Id = id;

                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing product
        /// </summary>
        /// <param name="id">The id of the product to update</param>
        /// <param name="product">The updated product data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("products/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            try
            {
                var result = await _productService.UpdateProductAsync(product);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a product
        /// </summary>
        /// <param name="id">The id of the product to delete</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("products/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
