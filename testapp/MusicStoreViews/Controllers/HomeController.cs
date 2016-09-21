// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicStoreViews.Models;

namespace MusicStoreViews.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(MusicStoreContext dbContext)
        {
            DbContext = dbContext;
        }

        public MusicStoreContext DbContext { get; }

        // From CheckoutController
        public IActionResult AddressAndPayment()
        {
            return View();
        }

        // From Areas.Admin.Controllers.StoreManagerController
        public IActionResult Create()
        {
            ViewBag.GenreId = new SelectList(DbContext.Genres, "GenreId", "Name");
            ViewBag.ArtistId = new SelectList(DbContext.Artists, "ArtistId", "Name");

            return View();
        }

        // From AccountController
        public IActionResult Register()
        {
            return View();
        }
    }
}
