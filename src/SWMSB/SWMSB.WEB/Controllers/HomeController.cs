using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SWMSB.BAL;
using SWMSB.COMMON;
using SWMSB.PROVIDERS;
using SWMSB.WEB.Models;

namespace SWMSB.WEB.Controllers
{


    public class HomeController : Controller
    {
        readonly string[] Months = {
            "January",
        "February",
        "March",
        "April",
        "May",
        "June",
        "July",
        "August",
        "September",
        "October",
        "November",
        "December"
        };
        private readonly int cacheRefreshRateInMinutes = 3;
        private readonly IBackendRepository backendRepository;
        private readonly IIotHubManagerRepository iotHubManagerRepository;
        public HomeController(IIotHubManagerRepository _iotHubManagerRepository, IBackendRepository _backendRepository)
        {
            backendRepository = _backendRepository;
            iotHubManagerRepository = _iotHubManagerRepository;
        }
        public async Task<IActionResult> Index()
        {
            var activeData = await iotHubManagerRepository.GetDevicesForPieChart(cacheRefreshRateInMinutes);
            var totaldevices = await iotHubManagerRepository.GetTotalDevicesForPieChart(cacheRefreshRateInMinutes);
            ViewData["activedevices"] = activeData.Count;
            ViewData["inactivedevices"] = totaldevices - activeData.Count;


            return View(activeData);
        }

        public async Task<IActionResult> Meters()
        {
            var activeData = await iotHubManagerRepository.GetAllDevices(cacheRefreshRateInMinutes);
            return View(activeData);
        }

        public async Task<IActionResult> WaterUsage(string id, int year)
        {

            ViewData["Title"] = $"Water Usage - {id}";
            var result = await backendRepository.GetWaterUsageAsync(id, cacheRefreshRateInMinutes);
            List<MonthlyData> yrdata = new List<MonthlyData>();
            var currentyr = year == 0 ? DateTime.UtcNow.Year : year;
            if (result?.Any() ?? false)
            {
                foreach (var item in Months)
                {
                    var key = $"{item}-{currentyr}";
                    var data = result.Where(x => x.RowKey.StartsWith(key)).ToList();
                    yrdata.Add(new MonthlyData {
                        DeviceId = id,
                        MonthYr = key, Data = data,
                        TotalWaterUsage = data.Any() ? data.Sum(x => x.DayWaterUsage) : 0,
                        AvgWaterUsage = data.Any() ? data.Average(x => x.DayWaterUsage) : 0
                    });

                }

            }
            return View(yrdata);
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
