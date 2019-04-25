using System.Linq;
using AspNet.Security.OpenIdConnect.Primitives;
using FP.OAuth.ResourceServer.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FP.OAuth.ResourceServer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ValueController : ControllerBase
    {
        private readonly IValueRepository _valueRepository;

        public ValueController(IValueRepository valueRepository)
        {
            _valueRepository = valueRepository;
        }

        [HttpPut]
        public ActionResult<ValueItem> Add([FromBody] AddValue value)
        {
            var user = HttpContext.User.Claims.First(x => x.Type == OpenIdConnectConstants.Claims.Name).Value;
            var item = _valueRepository.Add(user, value.value);
            return item;
        }

        public class AddValue
        {
            public int value;
        }
    }
}
