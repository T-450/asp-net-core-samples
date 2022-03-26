// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using NotificationPatternExample.Business.Interfaces;
using NotificationPatternExample.Business.Models;
using NotificationPatternExample.Business.Models.Validations;
using DbContext = NotificationPatternExample.Data.DbContext;

namespace NotificationPatternExample.Business;

public class ProductService
{
    private readonly DbContext _context;
    private readonly INotificator _notificator;

    public ProductService(DbContext context, INotificator notificator)
    {
        (_context, _notificator) = (context, notificator);
    }

    public async Task<IEnumerable<Product>> List()
    {
        var products = await _context.Products.AsNoTracking().ToListAsync().ConfigureAwait(false);
        return products;
    }

    public async Task Add(Product product)
    {
        var result = new ProductValidation().Validate(product);
        if (!result.isValid)
        {
            _notificator.Handle(result.errors);
            return;
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);
    }
}
