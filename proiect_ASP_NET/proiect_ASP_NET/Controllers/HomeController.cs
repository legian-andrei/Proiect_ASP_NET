﻿using Microsoft.AspNetCore.Mvc;
using proiect_ASP_NET.Models;
using System.Diagnostics;

namespace proiect_ASP_NET.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public IActionResult Index()
		{
			SetAccessRights();
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

		private void SetAccessRights()
		{
			ViewBag.AfisareButoane = false;
			if (User.IsInRole("Admin"))
			{
				ViewBag.AfisareButoane = true;
			}
		}
	}
}