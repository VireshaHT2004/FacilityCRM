using Microsoft.AspNetCore.Mvc;

namespace CRM_Project1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Swagger is working!";
        }
    }
}