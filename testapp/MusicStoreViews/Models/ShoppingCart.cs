// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace MusicStoreViews.Models
{
    public class ShoppingCart
    {
        private static readonly string _defaultCartId = Guid.NewGuid().ToString();
        private static readonly object _lock = new object();
        private static readonly IList<Album> _albums = new List<Album>
        {
            new Album { Title = "Led Zeppelin I" },
            new Album { Title = "The Cream Of Clapton" },
        };

        private static bool _initialized;

        private readonly MusicStoreContext _dbContext;
        private readonly string _shoppingCartId;

        private ShoppingCart(MusicStoreContext dbContext, string id)
        {
            _dbContext = dbContext;
            _shoppingCartId = id;

            // Place a couple of items into the cart if they're not already included.
            if (!_initialized)
            {
                lock (_lock)
                {
                    if (!_initialized)
                    {
                        for (var i = 0; i < _albums.Count; i++)
                        {
                            _dbContext.CartItems.Add(new CartItem
                            {
                                Album = _albums[i],
                                CartId = _shoppingCartId,
                                CartItemId = i,
                            });
                        }

                        _initialized = true;
                    }
                }
            }
        }

        public static ShoppingCart GetCart(MusicStoreContext db, HttpContext context)
            => GetCart(db, GetCartId(context));

        public static ShoppingCart GetCart(MusicStoreContext db, string cartId)
            => new ShoppingCart(db, cartId);

        public List<string> GetCartAlbumTitles()
        {
            return _dbContext
                .CartItems
                .Where(cart => cart.CartId == _shoppingCartId)
                .Select(c => c.Album.Title)
                .OrderBy(n => n)
                .ToList();
        }

        // We're using HttpContextBase to allow access to sessions.
        private static string GetCartId(HttpContext context)
        {
            return _defaultCartId;
        }
    }
}