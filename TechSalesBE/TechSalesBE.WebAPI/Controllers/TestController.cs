using Microsoft.AspNetCore.Mvc;

namespace TechSalesBE.WebAPI.Controllers
{
    [ApiController]
    [Route("/api/test")]
    public class TestController : ControllerBase
    {
        IWebHostEnvironment host;
        public TestController(IWebHostEnvironment _host)
        {
            host = _host;

        }
        [HttpGet]
        public async Task<ActionResult<string>> GetTest()
        {
            return StatusCode(StatusCodes.Status200OK, host.ContentRootPath + "\n" + Directory.GetCurrentDirectory());
        }
    }
}
