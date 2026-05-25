using System;
using System.Collections.Generic;
using System.Linq;
using Core.Enums;
using Core.Entities;

namespace Application.Discounts;

public sealed class DiscountFactory
{
    private readonly IEnumerable<IPricingStrategy> _strategies;

    public DiscountFactory(IEnumerable<IPricingStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IPricingStrategy GetStrategy(OrderType type)
    {
        var s = _strategies.FirstOrDefault(x => x.Type == type);
        if (s is null) throw new InvalidOperationException("No pricing strategy registered for type " + type);
        return s;
    }
}
