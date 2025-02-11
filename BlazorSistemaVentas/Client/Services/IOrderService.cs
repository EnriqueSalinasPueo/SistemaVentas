﻿using BlazorSistemaVentas.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorSistemaVentas.Client.Services
{
    public interface IOrderService
    {
        Task SaveOrder(Order order);
        Task<int> GetNextNumber();
        Task<IEnumerable<Order>> GetAll();
        Task<Order> GetDetails(int id);
        Task DeleteOrder(int id);
    }
}
