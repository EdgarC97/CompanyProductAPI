using CompanyProductAPI.Models;
using CompanyProductAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CompanyProductAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
        }

        /// <summary>
        /// Gets all companies
        /// </summary>
        /// <returns>A list of companies</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Company>>> GetAllCompanies()
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            return Ok(companies);
        }

        /// <summary>
        /// Gets a specific company by id
        /// </summary>
        /// <param name="id">The id of the company</param>
        /// <returns>The company if found</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Company>> GetCompanyById(int id)
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            return Ok(company);
        }

        /// <summary>
        /// Gets a company with its products
        /// </summary>
        /// <param name="id">The id of the company</param>
        /// <returns>The company with products if found</returns>
        [HttpGet("{id}/with-products")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Company>> GetCompanyWithProducts(int id)
        {
            var company = await _companyService.GetCompanyWithProductsAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            return Ok(company);
        }

        /// <summary>
        /// Creates a new company
        /// </summary>
        /// <param name="company">The company to create</param>
        /// <returns>The created company</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Company>> CreateCompany(Company company)
        {
            try
            {
                var id = await _companyService.CreateCompanyAsync(company);
                company.Id = id;

                // Return a 201 Created response with the location of the created resource
                return CreatedAtAction(nameof(GetCompanyById), new { id }, company);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing company
        /// </summary>
        /// <param name="id">The id of the company to update</param>
        /// <param name="company">The updated company data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCompany(int id, Company company)
        {
            if (id != company.Id)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            try
            {
                var result = await _companyService.UpdateCompanyAsync(company);
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
        /// Deletes a company
        /// </summary>
        /// <param name="id">The id of the company to delete</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            try
            {
                var result = await _companyService.DeleteCompanyAsync(id);
                if (!result)
                {
                    // Check if the company exists but couldn't be deleted due to having products
                    var company = await _companyService.GetCompanyByIdAsync(id);
                    if (company != null)
                    {
                        return Conflict("Cannot delete company because it has products");
                    }
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
