using System.Threading.Tasks;
using HomeHub.SpotifySort.Configuration;
using HomeHub.SpotifySort.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web.Models;

namespace HomeHub.SpotifySort.Database
{
    public class SpotifyContext : DbContext, ISpotifyContext
    {
        public DbSet<SpotifyToken> Tokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=SpotifyDB;Trusted_Connection=True;");
        }
    }
}
