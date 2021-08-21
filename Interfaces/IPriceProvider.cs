﻿using BetaMiner.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetaMiner.Interfaces
{
    public interface IPriceProvider
    {
        decimal CoinValue(decimal amount, Coin coin, Currency currency = Currency.USD);
    }
}
