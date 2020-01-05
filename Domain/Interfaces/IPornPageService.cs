using System.Threading;
using System.Threading.Tasks;
using PornWebApi.Models;

namespace PornWebApi.Domain.Interfaces
{
    public interface IPornPageService
    {
        Task<PornPage> LoadPornPageByCategory(string category, int page, string m, CancellationToken cancellationToken);
    }
}