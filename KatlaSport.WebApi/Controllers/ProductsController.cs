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
    [RoutePrefix("api/products")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [CustomExceptionFilter]
    [SwaggerResponseRemoveDefaults]
    public class ProductsController : ApiController
    {
        private readonly IProductCatalogueService _productService;

        public ProductsController(IProductCatalogueService productService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a list of products.", Type = typeof(ProductListItem[]))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> GetProductsAsync([FromUri] int start = 0, [FromUri] int amount = 100)
        {
            if (start < 0)
            {
                return BadRequest("start");
            }
            if (amount < 0)
            {
                return BadRequest("end");
            }

            var products = await _productService.GetProductsAsync(start, amount);
            return Ok(products);
        }

        [HttpGet]
        [Route("{id:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a product.", Type = typeof(Product))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> GetProductAsync([FromUri] int id)
        {
            if (id < 1)
            {
                return BadRequest($"Argument {nameof(id)} must be greater than zero.");
            }

            try
            {
                var product = await _productService.GetProductAsync(id);
                return Ok(product);
            }
            catch (RequestedResourceNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Description = "Creates a new product.")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> AddProductAsync([FromBody] UpdateProductRequest createRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var product = await _productService.CreateProductAsync(createRequest);
                var location = string.Format("/api/products/{0}", product.Id);
                return Created<Product>(location, product);
            }
            catch (RequestedResourceHasConflictException)
            {
                return Conflict();
            }
        }

        [HttpPut]
        [Route("{id:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Updates an existed product.")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> UpdateProductAsync([FromUri] int id, [FromBody] UpdateProductRequest updateRequest)
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
                await _productService.UpdateProductAsync(id, updateRequest);
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
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Deletes an existed product.")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> DeleteProductAsync([FromUri] int id)
        {
            if (id < 1)
            {
                return BadRequest($"Argument {nameof(id)} must be greater than zero.");
            }

            try
            {
                await _productService.DeleteProductAsync(id);
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
                await _productService.SetStatusAsync(id, deletedStatus);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
            }
            catch (RequestedResourceNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
