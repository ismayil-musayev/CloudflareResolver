using CloudflareResolver.Models;
using CloudflareResolver.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CloudflareResolver.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResolverController : ControllerBase
    {
        private readonly IResolver _resolver;

        public ResolverController(IResolver resolver)
        {
            _resolver = resolver;
        }

        [HttpPost]
        public async Task<IActionResult> Post(ResolverRequest request)
        {
            var isValid = IsRequestValid(request);

            if (!isValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _resolver.Resolve(request);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, request);
            }
        }

        private bool IsRequestValid(ResolverRequest request)
        {
            if (request == null)
            {
                ModelState.AddModelError(string.Empty, "Request is empty");
                return false;
            }

            if (!IsUrlValid(request.Url))
            {
                ModelState.AddModelError(string.Empty, $"Invalid Url: {request.Url}");
                return false;
            }

            return true;
        }

        private static bool IsUrlValid(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
