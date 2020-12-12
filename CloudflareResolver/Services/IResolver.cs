using CloudflareResolver.Models;
using System.Threading.Tasks;

namespace CloudflareResolver.Services
{
    public interface IResolver
    {
        Task<ResolverResponse> Resolve(ResolverRequest request);
    }
}
