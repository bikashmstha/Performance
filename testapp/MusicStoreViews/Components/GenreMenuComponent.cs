// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MusicStoreViews.Models;

namespace MusicStoreViews.Components
{
    [ViewComponent(Name = "GenreMenu")]
    public class GenreMenuComponent : ViewComponent
    {
        public GenreMenuComponent(MusicStoreContext dbContext)
        {
            DbContext = dbContext;
        }

        private MusicStoreContext DbContext { get; }

        public IViewComponentResult Invoke()
        {
            var genres = DbContext.Genres.Select(g => g.Name).Take(9).ToList();

            return View(genres);
        }
    }
}