using System;
using System.Threading;
using System.Threading.Tasks;
using HomeHub.SpotifySort.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeHub.SpotifySort.Database
{
    public interface ISpotifyContext : IDisposable
    {
        DbSet<SpotifyToken> Tokens { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
