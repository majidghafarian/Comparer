using Application.IService;
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
            var compare = new ObjectComparer();
            var changes = new List<string>();

            for (int i = 0; i < request.OldList.Count; i++)
            {
                var oldProduct = request.OldList[i];
                var newProduct = request.NewList.FirstOrDefault(p => p.Id == oldProduct.Id);
                if (newProduct != null)
                {
                    changes.AddRange(compare.CompareObjects(oldProduct, newProduct));
                }
            }
             
            return Ok(changes);
        }

    }

}
