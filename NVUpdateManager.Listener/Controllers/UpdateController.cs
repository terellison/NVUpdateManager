using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NVUpdateManager.Listener.Data;

namespace NVUpdateManager.Listener.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class UpdateController : ControllerBase
    {
        private readonly IUpdateService _updateService;

        public UpdateController(IUpdateService service)
        {
            _updateService = service;
        }

        [HttpPost]
        public async Task<HttpStatusCode> InstallUpdate([Required]string downloadLink)
        {
            await _updateService.InstallUpdate(downloadLink);

            return HttpStatusCode.OK;
        }
    }
}
