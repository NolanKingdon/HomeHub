using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using HomeHub.SpotifySort;
using HomeHub.SpotifySort.Configuration;
using Moq;
using SpotifyAPI.Web.Models;

namespace HomeHub.Tests.Customizations.Spotify
{
    public class SpotifyApiCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            // Options already defined in SpotifySortOptionsCustomization.cs.
            var options = fixture.Create<SpotifySortOptions>();
            var api = fixture.Freeze<Mock<IApi>>();

            // User Playlists.
            api.Setup( a => a.GetUserPlaylistsAsync(It.IsAny<string>(), options.MaxPlaylistResults))
               .ReturnsAsync(() => fixture.Create<Paging<SimplePlaylist>>());

            // User Profile.
            api.Setup( a => a.GetPrivateProfileAsync())
               .ReturnsAsync(() => fixture.Create<PrivateProfile>());

            // Liked Songs.
            api.Setup( a => a.GetSavedTracksAsync(options.MaxSongResults))
               .ReturnsAsync( () => fixture.Create<Paging<SavedTrack>>());

            // Playlist Descriptions.
            api.Setup( a => a.GetPlaylistAsync(It.IsAny<string>()))
               .ReturnsAsync( () => fixture.Create<FullPlaylist>());

            // Artists.
            api.Setup( a => a.GetArtistAsync(It.IsAny<string>()))
               .ReturnsAsync( () => fixture.Create<FullArtist>());

            // Move songs - Error Unused.
            api.Setup( a => a.AddPlaylistTracksAsync(It.IsAny<string>(), It.IsAny<List<string>>()))
               .ReturnsAsync(() => fixture.Create<ErrorResponse>());

            // Delete Songs - Error Unused.
            api.Setup( a => a.RemoveSavedTracksAsync(It.IsAny<List<string>>()))
               .ReturnsAsync(() => fixture.Create<ErrorResponse>());
        }
    }
}