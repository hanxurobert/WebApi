using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PornWebApi.Domain.Interfaces;
using PornWebApi.Models;

namespace PornWebApi.Controllers
{
    [Route("porn/")]
    [ApiController]
    public class PornController : ControllerBase
    {
        private readonly IPornPageService _service;
        
        public PornController(IPornPageService service)
        {
            _service = service;
        }

        [HttpGet("test")]
        public async Task<string> Test(CancellationToken cancellationToken = default(CancellationToken))
        {
            return "Hello World!!";
        }
        
        [HttpGet("page")]
        public async Task<List<PornPageItem>> Load91PornPageByCategary([FromQuery]string category, [FromQuery]int page, [FromQuery]string m, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _service.LoadPornPageByCategory(category, page, m, cancellationToken);
        }
    }
}