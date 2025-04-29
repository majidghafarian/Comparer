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
        public IActionResult CompareProducts([FromBody] CompareRequest<Product> request)
        {
            if (request == null )
            {
                return BadRequest("درخواست نامعتبر است یا کلید مشخص نشده.");
            }

            var changes = _comparerService.CompareObjects(request.OldList, request.NewList );

            return Ok(changes);
        }

    }

}
