﻿using EuNet.Core;
using Common;
using System;
using System.Threading.Tasks;

namespace StarterServer
{
    public partial class UserSession : IShopRpc
    {
        public Task<int> PurchaseItem(string itemId)
        {
            throw new NotImplementedException();
        }
    }
}
