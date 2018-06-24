using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using KatlaSport.Services.ProductManagement;
using KatlaSport.WebApi.CustomFilters;
using Microsoft.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace KatlaSport.WebApi.Controllers
{
    using KatlaSport.Services;

    [ApiVersion("1.0")]
    [RoutePrefix("api/categories")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [CustomExceptionFilter]
    [SwaggerResponseRemoveDefaults]
    public class ProductCategoriesController : ApiController
    {
        private readonly IProductCategoryService _categoryService;
        private readonly IProductCatalogueService _productCatalogueService;

        public ProductCategoriesController(IProductCategoryService categoryService, IProductCatalogueService productCatalogueService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _productCatalogueService = productCatalogueService ?? throw new ArgumentNullException(nameof(productCatalogueService));
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a list of product categories.", Type = typeof(ProductCategoryListItem[]))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> GetProductCategoriesAsync([FromUri] int start = 0, [FromUri] int amount = 100)
        {
            if (start < 0)
            {
                return BadRequest("start");
            }
            if (amount < 0)
            {
                return BadRequest("end");
            }

            var categories = await _categoryService.GetCategoriesAsync(start, amount);
            return Ok(categories);
        }

        [HttpGet]
        [Route("{id:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a product category.", Type = typeof(ProductCategory))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> GetProductCategoryAsync([FromUri] int id)
        {
            if (id < 1)
            {
                return BadRequest($"Argument {nameof(id)} must be greater than zero.");
            }

            try
            {
                var category = await _categoryService.GetCategoryAsync(id);
                return Ok(category);
            }
            catch (RequestedResourceNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Description = "Creates a new product category.")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> AddProductCategoryAsync([FromBody] UpdateProductCategoryRequest createRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var category = await _categoryService.CreateCategoryAsync(createRequest);
                var location = string.Format("/api/categories/{0}", category.Id);
                return Created<ProductCategory>(location, category);
            }
            catch (RequestedResourceHasConflictException)
            {
                return Conflict();
            }
        }

        [HttpPut]
        [Route("{id:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Updates an existed product category.")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> UpdateProductCategoryAsync([FromUri] int id, [FromBody] UpdateProductCategoryRequest updateRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id < 1)
            {
                return BadRequest($"Argument {nameof(id)} must be greater than zero.");
            }

            try
            {
                await _categoryService.UpdateCategoryAsync(id, updateRequest);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
            }
            catch (RequestedResourceHasConflictException)
            {
                return Conflict();
            }
            catch (RequestedResourceNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete]
        [Route("{id:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Deletes an existed product category.")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> DeleteProductCategoryAsync([FromUri] int id)
        {
            if (id < 1)
            {
                return BadRequest($"Argument {nameof(id)} must be greater than zero.");
            }

            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
            }
            catch (RequestedResourceHasConflictException)
            {
                return Conflict();
            }
            catch (RequestedResourceNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut]
        [Route("{id:int:min(1)}/status/{deletedStatus:bool}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Sets deleted status for an existed product category.")]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> SetStatusAsync([FromUri] int id, [FromUri] bool deletedStatus)
        {
            if (id < 1)
            {
                return BadRequest($"Argument {nameof(id)} must be greater than zero.");
            }

            try
            {
                await _categoryService.SetStatusAsync(id, deletedStatus);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
            }
            catch (RequestedResourceNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("{id:int:min(1)}/products")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a list of products for requested product category.", Type = typeof(ProductCategoryProductListItem))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> GetProductsAsync([FromUri] int id)
        {
            if (id < 1)
            {
                return BadRequest($"Argument {nameof(id)} must be greater than zero.");
            }

            try
            {
                var products = await _productCatalogueService.GetCategoryProductsAsync(id);
                return Ok(products);
            }
            catch (RequestedResourceNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
