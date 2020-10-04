using System;
using System.Collections.Generic;
using SpotifyAPI.Web.Models;

namespace HomeHub.SpotifySort
{
    /// <summary>
    /// Because of the hassle getting the tracks, I want to organize this in a way that will let
    /// me easily handle the genres once we call them once. Creating what is essentially a wrapper
    /// class was the best way I had. I had originally intended to inherit from SavedTrack and
    /// use the implicit converison operator to abstract more away, but quickly learned C# doesn't allow
    /// implicit conversion between inherited classes.
    /// </summary>
    public class SavedTrackWithGenre
    {
        public List<string> Genres { get; set; }
        public DateTime AddedAt { get; set; }
        public FullTrack Track { get; set; }

        public SavedTrackWithGenre(SavedTrack track)
        {
            Genres = new List<string>();
            AddedAt = track.AddedAt;
            Track = track.Track;
        }

        public static implicit operator SavedTrackWithGenre(SavedTrack track)
        {
            return new SavedTrackWithGenre(track);
        }
    }
}