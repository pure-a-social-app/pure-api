using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pure.api.Domain.Contracts;
using Pure.api.Domain.Models.Shopping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Pure.api.Controllers
{
    [Route("api/shopping")]
    [ApiController]
    public class ShoppingController : ControllerBase
    {
        private readonly IShoppingService _shoppingService;

        public ShoppingController(IShoppingService shoppingService)
        {
            _shoppingService = shoppingService;
        }

        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost("createNewShop")]
        public async Task<IActionResult> CreateNewShop([FromBody] CreateNewShopViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(
                        new
                        {
                            Message = ModelState.Values.SelectMany(e => e.Errors.Select(m => m.ErrorMessage))
                        });
                }

                var result = await _shoppingService.CreateNewShop(vm.LoginId, vm.ShopAddress);

                if (!result)
                {
                    return BadRequest(new { Message = "User already has a shop." });
                }

                return Ok(new { Success = true });
            }
            catch (ApplicationException e)
            {
                return BadRequest(new { Message = "Catch block: Cannot create new item" });
            }
        }

        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpPost("addItem")]
        public async Task<IActionResult> CreateShoppingItem([FromForm] IList<IFormFile> photos, 
            [FromForm] string itemDetails)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(
                        new 
                        {
                            Message = ModelState.Values.SelectMany(e => e.Errors.Select(m => m.ErrorMessage))
                        });
                }

                var item = await _shoppingService.AddItem(photos, itemDetails);

                if (item == null)
                {
                    return BadRequest(new { Message = "Cannot add item" });
                }

                return Ok(new { Item = item });
            }
            catch (ApplicationException e)
            {
                return BadRequest(new { Message = "Catch block: Cannot create new item" });
            }
        }

        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpGet("getAllShoppingItems")]
        public async Task<IActionResult> GetAllItems()
        {
            try
            {   
                var items = await _shoppingService.GetAllFiles(Common.Contracts.FileType.ShoppingItem);

                return Ok(new { Success = true, Items = items });
            }
            catch (ApplicationException e)
            {
                return BadRequest(new { Message = e.Message });
            }
        }
    }
}
