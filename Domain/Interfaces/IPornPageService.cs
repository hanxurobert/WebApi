using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PornWebApi.Models;

namespace PornWebApi.Domain.Interfaces
{
    public interface IPornPageService
    {
        Task<List<PornPageItem>> LoadPornPageByCategory(string category, int page, string m, CancellationToken cancellationToken);
    }
}