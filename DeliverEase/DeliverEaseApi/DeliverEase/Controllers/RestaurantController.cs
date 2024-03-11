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
    public class RestaurantController : ControllerBase
    {
        private readonly RestaurantService _restaurantService;
        private readonly ReviewService _reviewService;
        

        public RestaurantController(RestaurantService restaurantService, ReviewService reviewService)
        {
            _restaurantService = restaurantService;
            _reviewService = reviewService;
        }

        [HttpGet("GetAllRestaurants")]
        public async Task<ActionResult<List<Restaurant>>> GetAllRestaurants()
        {
            try
            {
                var restaurants = await _restaurantService.GetRestaurantsAsync();
                return Ok(restaurants);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetRestaurantById")]
        public async Task<ActionResult<Restaurant>> GetRestaurantById(string id)
        {
            try
            {
                var restaurant = await _restaurantService.GetRestaurantByIdAsync(id);
                if (restaurant == null)
                {
                    return BadRequest($"Restaurant with ID {id} does not exist.");
                }
                return Ok(restaurant);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateRestaurant")]
        public async Task<ActionResult> CreateRestaurant(Restaurant restaurant)
        {
            try
            {
                if (string.IsNullOrEmpty(restaurant.Address))
                {
                    ModelState.AddModelError("Address", "The Address field is required.");
                    return BadRequest(ModelState);
                }
                else if(string.IsNullOrEmpty(restaurant.Name))
                {
                    ModelState.AddModelError("Name", "The Name field is required.");
                    return BadRequest(ModelState);
                }

                restaurant.Id = null;

                var result = await _restaurantService.CreateRestaurantAsync(restaurant);

               return Ok(result);   

            }
			
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateRestaurant")]
        public async Task<IActionResult> UpdateRestaurant(string id, Restaurant restaurantIn)
        {
            try
            {
                var restaurant = await _restaurantService.GetRestaurantByIdAsync(id);
                if (restaurant == null)
                {
                    return BadRequest($"Restaurant with ID {id} does not exist.");
                }

                restaurantIn.Id = id;
                await _restaurantService.UpdateRestaurantAsync(id, restaurantIn);
               
                return Ok("Restaurant successfully updated.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete("DeleteRestaurant")]
        public async Task<IActionResult> DeleteRestaurant(string id)
        {
            try
            {
                var restaurant = await _restaurantService.GetRestaurantByIdAsync(id);
                if (restaurant == null)
                {
                    return BadRequest($"Restaurant with ID {id} does not exist.");
                }
                await _reviewService.DeleteReviewsForRestaurantAsync(id);

                await _restaurantService.DeleteRestaurantAsync(id);

                return Ok($"Izbrisan je restoran: {restaurant.Id}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("SortByRating")]
        public async Task<ActionResult<List<Restaurant>>> GetRestaurantsSortedByRating(bool ascending = false)
        {
            try
            {
                var restaurants = await _restaurantService.GetRestaurantsSortedByRatingAsync(ascending);
                return Ok(restaurants);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        
        [HttpPost("AddMeal")]
        public async Task<IActionResult> AddMealToRestaurant(string restaurantId, Meal meal)
        {
            try
            {
                meal.Id=null;
                var mealId = await _restaurantService.AddMealToRestaurantAsync(restaurantId, meal);
                return Ok(mealId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteMeal")]
        public async Task<IActionResult> DeleteMealFromRestaurant(string restaurantId, string mealId)
        {
            try
            {
                await _restaurantService.DeleteMealFromRestaurantAsync(restaurantId, mealId);
                return Ok("Meal deleted from restaurant successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

         [HttpGet("GetMealById")]
        public async Task<ActionResult<Meal>> GetMealById(string restaurantId, string mealId)
        {
            try
            {
                var meal = await _restaurantService.GetMealByIdAsync(restaurantId, mealId);
                return Ok(meal);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAllMealsInRestaurant")]
        public async Task<ActionResult<List<Meal>>> GetAllMealsInRestaurant(string restaurantId)
        {
            try
            {
                var meals = await _restaurantService.GetAllMealsInRestaurantAsync(restaurantId);
                return Ok(meals);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        
    }
}
