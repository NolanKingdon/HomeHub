using HomeHub.BackgroundServices.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeHub.BackgroundServices.Database
{
    public interface ISpotifyContext
    {
        DbSet<SpotifyToken> Tokens { get; set; }

        int SaveChanges();
    }
}