using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApplicationExample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SampleController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { Message = "GET request received" });

        [HttpPost]
        public IActionResult Post() => Ok(new { Message = "POST request received" });

        [HttpPatch]
        public IActionResult Patch() => Ok(new { Message = "PATCH request received" });

        [HttpPut]
        public IActionResult Put() => Ok(new { Message = "PUT request received" });

        [HttpDelete]
        public IActionResult Delete() => Ok(new { Message = "DELETE request received" });
    }
}
