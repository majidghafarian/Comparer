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
       
 
 
        //[HttpPost("compare")]
        //public IActionResult CompareProducts([FromBody] CompareRequest<Product> request)
        //{
        //    if (request == null || string.IsNullOrEmpty(request.KeyName))
        //    {
        //        return BadRequest("درخواست نامعتبر است یا کلید مشخص نشده.");
        //    }

        //    var changes = _comparerService.CompareByKey(request.OldList, request.NewList, request.KeyName);

        //    return Ok(changes);
        //}
        [HttpPost("compare")]
        public IActionResult CompareProducts([FromBody] CompareRequest<TestModel> request)
        {
            
             var changes = ObjectComparer.CompareByKey(request.oldObject, request.newObject);
            //var changes = compare.CompareObjects(request.OldValue, request.NewValue);

            return Ok(changes);
        }


    }

}
