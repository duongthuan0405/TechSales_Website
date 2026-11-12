using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonModule.CustomAttributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthModule.Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        [HttpGet("test")]
        public async Task<ActionResult<string>> GetTest()
        {
            return StatusCode(StatusCodes.Status200OK, "Ok from Auth");
        }
    }
}