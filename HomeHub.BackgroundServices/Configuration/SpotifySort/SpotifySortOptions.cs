using System;

namespace HomeHub.BackgroundServices.Configuration.SpotifySort
{
    public class SpotifySortOptions
    {
        /// <summary>
        /// Interval in ms.
        /// </summary>
        public int Interval { get; set; }
        public int MaxPlaylistResults { get; set; }
        public int MaxSongResults { get; set; }
        public int MaxConcurrentThreads { get; set; }
    }
}