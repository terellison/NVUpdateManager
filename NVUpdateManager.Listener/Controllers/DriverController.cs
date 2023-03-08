using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NVUpdateManager.Listener.Data;
using NVUpdateManager.Core;

namespace NVUpdateManager.Listener.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class DriverController : Controller
    {
        private readonly IDriverService _driverService;

        public DriverController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        [HttpGet]
        public async Task<DriverInfo> Driver()
        {
           return await _driverService.GetDriverInfoAsync();
        }
    }
}
