using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliverEase.Models;
using DeliverEase.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliverEase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("GetAllOrders")]
        public async Task<ActionResult<List<Order>>> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetOrderById")]
        public async Task<ActionResult<Order>> GetOrderById(string id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return BadRequest($"Order with ID {id} does not exist.");
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetOrdersForUser")]
        public async Task<ActionResult<List<Order>>> GetOrdersForUser(string userId)
        {
            try
            {
                var orders = await _orderService.GetOrdersForUserAsync(userId);
                if (orders == null || orders.Count == 0)
                {
                    return BadRequest($"User with ID {userId} does not exist.");
                }
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder(string restaurantId, string userId, List<string> mealIds)
        {
            try
            {
                if (mealIds == null || mealIds.Count == 0)
                {
                    return BadRequest("No meals were added to the order.");
                }

                var order = await _orderService.CreateOrderAsync(restaurantId, userId, mealIds);

                if (order != null)
                {
                    return Ok(order);
                }
                else
                {
                    return BadRequest("Restaurant or user not found, or some meals were not added to the order.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut("UpdateOrder")]
        public async Task<ActionResult> UpdateOrder(string id, List<string> mealIds)
        {
            try
            {
                await _orderService.UpdateOrderAsync(id, mealIds);

                var updatedOrder = await _orderService.GetOrderByIdAsync(id);
                if (updatedOrder == null)
                {
                    return BadRequest();
                }

                return Ok("Order successfully updated.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteOrder")]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return BadRequest($"Order with ID {id} does not exist.");
                }

                await _orderService.DeleteOrderAsync(id);
                return Ok("Order successfully removed.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}
