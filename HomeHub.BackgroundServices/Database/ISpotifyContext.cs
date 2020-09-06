using System.Threading;
using System.Threading.Tasks;
using HomeHub.BackgroundServices.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeHub.BackgroundServices.Database
{
    public interface ISpotifyContext
    {
        DbSet<SpotifyToken> Tokens { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
