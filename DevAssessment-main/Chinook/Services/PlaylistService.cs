using Microsoft.EntityFrameworkCore;
using System.Linq;
using Chinook.Services.Interfaces;
using Chinook.ClientModels;
using Chinook.Models;

namespace Chinook.Services
{
    public class PlaylistService : IPlaylistService, IDisposable
    {
        private readonly ChinookContext _dbContext;

        public PlaylistService(IDbContextFactory<ChinookContext> dbFactory)
        {
            // DbContext is created directly from the factory without asynchronous operation due to constructor limitation.
            _dbContext = dbFactory.CreateDbContext();
        }

        public ClientModels.Playlist GetPlaylist(long playlistId, string userId)
        {
            // Fetches a playlist including its tracks, and related albums and artists, also determines if each track is a favorite for the given user.
            var playlistData = _dbContext.Playlists
                .Include(p => p.Tracks)
                    .ThenInclude(t => t.Album)
                        .ThenInclude(a => a.Artist)
                .Where(p => p.PlaylistId == playlistId)
                .Select(p => new ClientModels.Playlist
                {
                    Name = p.Name,
                    Tracks = p.Tracks.Select(t => new PlaylistTrack
                    {
                        AlbumTitle = t.Album.Title,
                        ArtistName = t.Album.Artist.Name,
                        TrackId = t.TrackId,
                        TrackName = t.Name,
                        IsFavorite = t.Playlists.Any(pl => pl.UserPlaylists.Any(up => up.UserId == userId && up.Playlist.Name == "Favorites"))
                    }).ToList()
                })
                .FirstOrDefault();

            if (playlistData == null)
            {
                throw new Exception($"Playlist with ID {playlistId} not found.");
            }

            return playlistData;
        }

        public Models.Playlist GetFavoritePlaylist(string userId)
        {
            // Retrieves a user's favorite playlist by a predefined naming convention, if it exists.
            const string favoritePlaylistName = "My Favorite Tracks";
            var playlist = _dbContext.Playlists
                .FirstOrDefault(p => p.Name == favoritePlaylistName && p.UserPlaylists.Any(up => up.UserId == userId));

            return playlist;
        }

        public void Dispose()
        {
            // Ensures the DbContext is properly disposed of when the service is no longer in use.
            _dbContext?.Dispose();
        }
    }
}
