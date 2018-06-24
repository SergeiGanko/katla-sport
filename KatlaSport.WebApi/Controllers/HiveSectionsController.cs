using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using KatlaSport.Services.HiveManagement;
using KatlaSport.WebApi.CustomFilters;
using Microsoft.Web.Http;
using Swashbuckle.Swagger.Annotations;
using KatlaSport.Services;

namespace KatlaSport.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [RoutePrefix("api/sections")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [CustomExceptionFilter]
    [SwaggerResponseRemoveDefaults]
    public class HiveSectionsController : ApiController
    {
        private readonly IHiveSectionService _hiveSectionService;

        public HiveSectionsController(IHiveSectionService hiveSectionService)
        {
            _hiveSectionService = hiveSectionService ?? throw new ArgumentNullException(nameof(hiveSectionService));
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a list of hive sections.", Type = typeof(HiveSectionListItem[]))]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> GetHiveSectionsAsync()
        {
            var hives = await _hiveSectionService.GetHiveSectionsAsync();
            return Ok(hives);
        }

        [HttpGet]
        [Route("{hiveSectionId:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Returns a hive section.", Type = typeof(HiveSection))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> GetHiveSectionAsync(int hiveSectionId)
        {
            if (hiveSectionId < 1)
            {
                return BadRequest($"Invalid argument {nameof(hiveSectionId)}");
            }

            try
            {
                var hive = await _hiveSectionService.GetHiveSectionAsync(hiveSectionId);
                return Ok(hive);
            }
            catch (RequestedResourceNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut]
        [Route("{hiveSectionId:int:min(1)}/status/{deletedStatus:bool}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Sets deleted status for an existed hive section.")]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> SetStatusAsync([FromUri] int hiveSectionId, [FromUri] bool deletedStatus)
        {
            if (hiveSectionId < 1)
            {
                return BadRequest($"Invalid argument {nameof(hiveSectionId)}");
            }

            try
            {
                await _hiveSectionService.SetStatusAsync(hiveSectionId, deletedStatus);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NoContent));
            }
            catch (RequestedResourceNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Description = "Creates a new hive section.")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> AddHiveSectionAsync([FromBody] UpdateHiveSectionRequest createRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var hiveSection = await _hiveSectionService.CreateHiveSectionAsync(createRequest);
                var location = $"/api/sections/{hiveSection.Id}";
                return Created<HiveSection>(location, hiveSection);
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
        [Route("{id:int:min(1)}")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Updates an existed hive section.")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> UpdateHiveSectionAsync([FromUri] int id,[FromBody] UpdateHiveSectionRequest updateRequest)
        {
            if (id < 1)
            {
                return BadRequest($"Argument {nameof(id)} must be greater than zero.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _hiveSectionService.UpdateHiveSectionAsync(id, updateRequest);
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
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Deletes an existed hive section.")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
        public async Task<IHttpActionResult> DeleteHiveSectionAsync([FromUri] int id)
        {
            if (id < 1)
            {
                return BadRequest($"Argument {nameof(id)} must be greater than zero.");
            }

            try
            {
                await _hiveSectionService.DeleteHiveSectionAsync(id);
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
    }
}
