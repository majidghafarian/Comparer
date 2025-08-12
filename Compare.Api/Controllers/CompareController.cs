using Application.Models;
using Domain;
using Infrastructure.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Compare.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompareController : ControllerBase
    {
       
 
 
        [HttpPost("compare")]
        public IActionResult CompareProducts([FromBody] CompareRequest<Product> request)
        {
            
             var changes = ObjectComparer.CompareObjects(request.oldObject, request.newObject);
            

            return Ok(changes);
        }


    }

}
