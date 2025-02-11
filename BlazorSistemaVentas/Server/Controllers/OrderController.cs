﻿using BlazorSistemaVentas.Shared;
using BlazorSistemaVentas_Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace BlazorSistemaVentas.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _iOrderRepository;
        private readonly IOrderProductRepository _iOrderProductRepository;

        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderRepository iOrderRepository, IOrderProductRepository iOrderProductRepository, ILogger<OrderController> logger)
        {
            _iOrderRepository = iOrderRepository;
            _iOrderProductRepository = iOrderProductRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order order)
        {
            _logger.LogInformation($"INICIO - Post order: {order}");

            if (order == null)
            {
                return BadRequest();
            }
            if (order.OrderNumber == 0)
            {
                ModelState.AddModelError("OrderNumber", "Order numbar can`t be empty");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Usando el objeto TransactionScope si algo va mal dentro del using se hara automaticamente un rollback
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                order.Id = await _iOrderRepository.GetNextId();

                var request = await _iOrderRepository.InserOrder(order);

                if (order.Products != null)
                {
                    foreach (var prd in order.Products)
                    {
                        await _iOrderProductRepository.InserOrderProduct(order.Id, prd);
                    }
                }

                scope.Complete();

                _logger.LogInformation($"FIN - Post request: {request}");
            }

            return NoContent();

        }

        [HttpGet("GetNextNumber")]
        public async Task<int> GetNextNumber()
        {
            _logger.LogInformation($"INICIO - GetNextNumber");

            var request = await _iOrderRepository.GetNextNumber();

            _logger.LogInformation($"FIN - GetNextNumber request: {request}");

            return request;

        }

        [HttpGet]
        public async Task<IEnumerable<Order>> GetAll()
        {
            _logger.LogInformation($"INICIO - GetAll");

            var request = await _iOrderRepository.GetAll();

            foreach (var item in request)
            {
                item.Products =(List<Product>) await _iOrderProductRepository.GetByOrder(item.Id);
            }

            _logger.LogInformation($"FIN - GetAll request: {request}");

            return request;

        }


        [HttpGet("{id}")]
        public async Task<Order> GetDetailsId(int id)
        {
            _logger.LogInformation($"INICIO - GetDetailsId");

            var order = await _iOrderRepository.GetDetailsId(id);

            var products = await _iOrderProductRepository.GetByOrder(id);

            if (order != null)
            {
                order.Products = products.ToList();
            }

            _logger.LogInformation($"FIN - GetDetailsId request: {order}");

            return order;

        }

        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            _logger.LogInformation($"INICIO - Delete");

            await _iOrderRepository.DeleteOrder(id);

            _logger.LogInformation($"FIN - Delete");
        }


        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Order order)
        {
            _logger.LogInformation($"INICIO - Put order: {order}");

            if (order == null)
            {
                return BadRequest();
            }
            if (order.OrderNumber == 0)
            {
                ModelState.AddModelError("OrderNumber", "Order numbar can`t be empty");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Usando el objeto TransactionScope si algo va mal dentro del using se hara automaticamente un rollback
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var request = await _iOrderRepository.UpdateOrder(order);

                await _iOrderProductRepository.DeleteOrderProductByOrder(order.Id);

                if (order.Products != null)
                {
                    foreach (var prd in order.Products)
                    {
                        await _iOrderProductRepository.InserOrderProduct(order.Id, prd);
                    }
                }

                scope.Complete();

                _logger.LogInformation($"FIN - Put request: {request}");
            }
            return NoContent();
        }
    }
}
