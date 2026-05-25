using System;
using System.Collections.Generic;
using System.Linq;
using Core.Enums;
using Core.Entities;

namespace Application.Discounts;

public class DiscountFactory
{
    private readonly IEnumerable<IDiscountStrategy> _strategies;

    public DiscountFactory(IEnumerable<IDiscountStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IDiscountStrategy GetStrategy(OrderType type)
    {
        var s = _strategies.FirstOrDefault(x => x.Type == type);
        if (s is null) throw new InvalidOperationException("No discount strategy registered for type " + type);
        return s;
    }
}
