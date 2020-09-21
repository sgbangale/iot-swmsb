using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SWMSB.BAL;
using SWMSB.COMMON;
using SWMSB.WEB.Models;

namespace SWMSB.WEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly IIotHubManagerRepository IotHubManagerRepository;
        public HomeController(IIotHubManagerRepository _iotHubManagerRepository)
        {
            IotHubManagerRepository = _iotHubManagerRepository;
        }
        public async Task<IActionResult> Index()
        {
            var activeData = await IotHubManagerRepository.GetDevicesForPieChart();
            var totaldevices = await IotHubManagerRepository.GetTotalDevicesForPieChart();
            ViewData["activedevices"] = activeData.Count;
            ViewData["inactivedevices"] = totaldevices - activeData.Count;


            return View(activeData);
        }

        public async Task<IActionResult> ActiveMeters()
        {
            var activeData = await IotHubManagerRepository.GetDevicesForPieChart();
            return View(activeData);
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
