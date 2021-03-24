using Microsoft.AspNetCore.Mvc;
using Pure.api.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Pure.api.Controllers
{
    [Route("api/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [HttpGet("usernames")]
        public async Task<IActionResult> SearchUsersByName(string search)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = ModelState.Values
                                      .SelectMany(e => e.Errors.Select(m => m.ErrorMessage))
                    });
                }

                var users = await _searchService.SearchUsersByName(search);

                return Ok(new { Success = true, Users = users });
            }
            catch (ApplicationException e)
            {
                return BadRequest(new { e.Message });
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = "Please contact the IT Department for futher information" });
            }
        }
    }
}
