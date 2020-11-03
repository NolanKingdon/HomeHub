# Spotify Sort Library

Periodically checks against Spotify using given credentials to access and sort liked songs. Service looks at public, user created playlists, and will sort accordingly based on the description. Once sorted, the song will be removed from the 'liked' section via unliking.

Database created using [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/).
Made using [SpotifyAPI-Net](https://johnnycrazy.github.io/SpotifyAPI-NET/).
Leverages [Spotify Web Api](https://developer.spotify.com/documentation/web-api/).

#### Playlist Creation

In order for a playlist to be considered, two conditions must be met:

- The playlist must be publicly accessible.
- The playlist's description must include any genres to include, delimited by spaces.

If these conditions are met, liked songs will be sorted into them.

##### Notes

- Because of the frequent, non-essential occurence of the service, request retry libraries like Polly were not used.
- Will require options outlined in secretsTemplate.json to be provided.
- First attempt to run will require you to authenticate. A link will be provided in the console for permissions.