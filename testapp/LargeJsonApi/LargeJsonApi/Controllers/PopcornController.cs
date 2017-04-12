// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using LargeJsonApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace LargeJsonApi.Controllers
{
    [Route("/[controller]")]
    public class PopcornController : ControllerBase
    {
        [HttpGet("[action]/{id}", Name = "movie")]
        public IActionResult Movie(int id)
        {
            //validate
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id < 0 || id >= DataFactory.Movies.Count)
            {
                return BadRequest($"The movie id { id } is outside of range, must be 0 to { DataFactory.Movies.Count - 1 }");
            }

            //obtain
            var movie = DataFactory.Movies[id];
            return Ok(movie);
        }

        [HttpPost("[action]/{id}", Name = "movie")]
        public IActionResult Movie([FromBody] Model.Movies.RootObject movie)
        {
            //validate
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (movie == null)
            {
                return BadRequest($"Expecting a request body containing a movie description");
            }

            //acknowledge
            return Ok(movie.Products[0].LocalizedProperties[0].ProductTitle);
        }

        [HttpGet("[action]/{id}", Name = "series")]
        public IActionResult Series(int id)
        {
            //validate
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id < 0 || id >= DataFactory.Series.Count)
            {
                return BadRequest($"The series id { id } is outside of range, must be 0 to { DataFactory.Series.Count - 1 }");
            }

            //obtain
            var series = DataFactory.Series[id];
            return Ok(series);
        }

        [HttpPost("[action]/{id}", Name = "series")]
        public IActionResult Series([FromBody] Model.Series.RootObject series)
        {
            //validate
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (series == null)
            {
                return BadRequest("Expecting a request body containing a series description");
            }

            //acknowledge
            return Ok(series.DisplayProductSearchResult.Products[0].LocalizedProperties[0].Parents[0].SeriesTitle);
        }

        [HttpGet("[action]/{id}", Name = "season")]
        public IActionResult Season(int id)
        {
            //validate
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id < 0 || id >= DataFactory.Seasons.Count)
            {
                return BadRequest($"The season id { id } is outside of range, must be 0 to { DataFactory.Seasons.Count - 1 }");
            }

            //obtain
            var season = DataFactory.Seasons[id];
            return Ok(season);
        }

        [HttpPost("[action]/{id}", Name = "season")]
        public IActionResult Season([FromBody] Model.Seasons.RootObject season)
        {
            //validate
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (season == null)
            {
                return BadRequest("Expecting a request body containing a season description");
            }

            //acknowledge
            var md = season.DisplayProductSearchResult.Products[0].LocalizedProperties[0].Parents[0];
            return Ok($"{md.SeriesTitle},{md.SeasonPosition}");
        }
    }
}
