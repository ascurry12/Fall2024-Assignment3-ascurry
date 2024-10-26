using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Fall2024_Assignment3_ascurry.Models;

namespace Fall2024_Assignment3_ascurry.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    // private readonly IConfiguration _config;

    public HomeController(ILogger<HomeController> logger) // , IConfiguration config)
    {
        _logger = logger;
        // _config = config;
    }

    public IActionResult Index()
    {
        //string secret = _config["OpenAI:Secret"] ?? throw new Exception("OpenAI:Secret does not exist in the current Configuration");
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

