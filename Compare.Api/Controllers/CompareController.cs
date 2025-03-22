using Application.IService;
using Application.Models;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Compare.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompareController : ControllerBase
    {
        private readonly IObjectComparer _comparerService;

        public CompareController(IObjectComparer comparerService)
        {
            _comparerService = comparerService;
        }

        [HttpPost("compare")]
        public IActionResult CompareObjects([FromBody] CompareRequest<Employe> request)
        {
            if (request.OldObject == null || request.NewObject == null)
            {
                return BadRequest("Both objects must be provided.");
            }

            string result = _comparerService.CompareAndLogChanges(request.OldObject, request.NewObject);
            return Ok(new { پیام = result });
        }
    }

}
