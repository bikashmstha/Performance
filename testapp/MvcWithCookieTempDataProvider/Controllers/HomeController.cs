// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using MvcWithCookieTempDataProvider.Models;

namespace MvcWithCookieTempDataProvider.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // [HttpPost]
        // public IActionResult AddCustomer(Customer customer)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }

        //     TempData["foo"] = "bar";

        //     return RedirectToAction("AddCustomerCompleted");
        // }

        // [HttpGetAttribute]
        // public IActionResult AddCustomerCompleted()
        // {
        //     return Content("Content: " + TempData["foo"]?.ToString());
        // }

        [HttpGet]
        public IActionResult SetTempData()
        {
            TempData["foo"] = "bar";
            return Ok();
        }
    }
}
