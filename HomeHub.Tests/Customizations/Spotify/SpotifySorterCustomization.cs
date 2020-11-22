using System;
using System.Collections.Generic;
using System.Threading;
using AutoFixture;
using HomeHub.SpotifySort;
using Moq;
using SpotifyAPI.Web.Models;

namespace HomeHub.Tests.Customizations.Spotify
{
    public class SpotifySorterCustomization : ICustomization
    {
        readonly bool active;

        public SpotifySorterCustomization(bool active = true)
        {
            this.active = active;
        }

        public void Customize(IFixture fixture)
        {
            var sorter = fixture.Freeze<Mock<ISpotifySort>>();

            // Active call.
            sorter.Setup( s => s.Active)
                  .Returns(active);

            // Setting up Genre Tracks
            List<SavedTrack> tracks = new List<SavedTrack>()
            {
                new SavedTrack
                {
                    AddedAt = new DateTime(),
                    Track = fixture.Create<FullTrack>()
                }
            };

            sorter.Setup( s => s.GetUserLikedTracksAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new Paging<SavedTrack>()
                  {
                      Items = tracks
                  });

            sorter.Setup( s => s.GetGenreFromSongAsync(It.IsAny<SavedTrackWithGenre>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync( new SavedTrackWithGenre(tracks[0])
                  {
                     Genres = { "LoFi", "Death Metal", "Polka" }
                  });
        }
    }
}
