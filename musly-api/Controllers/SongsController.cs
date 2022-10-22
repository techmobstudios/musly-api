using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using musly_api.Services;

namespace musly_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SongsController : ControllerBase
    {

        public MuslyService _muslyService { get; set; }

        public SongsController(ILogger<SongsController> logger, MuslyService muslyService)
        {
            _muslyService = muslyService;
        }

        [HttpGet]
        public IEnumerable<String> Get(string query= "bigx")
        {
            return _muslyService.cf.trackFiles.AsEnumerable().Where(x => x.Contains(query, StringComparison.OrdinalIgnoreCase));
        }
    }
}
