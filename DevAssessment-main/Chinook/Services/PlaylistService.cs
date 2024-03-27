using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Chinook.Services.Interfaces;
using Chinook.ClientModels;
using Chinook.Models;

namespace Chinook.Services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly ChinookContext _dbContext;

        public PlaylistService(IDbContextFactory<ChinookContext> dbFactory)
        {
            _dbContext = dbFactory.CreateDbContext(); // Synchronously create the context
        }

        public ClientModels.Playlist GetPlaylist(long playlistId, string userId)
        {
            var playlistData = _dbContext.Playlists
                .Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
                .Where(p => p.PlaylistId == playlistId)
                .Select(p => new ClientModels.Playlist // Ensure this is the correct Playlist from ClientModels
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

            if (playlistData == null) throw new Exception($"Playlist with ID {playlistId} not found.");

            return playlistData;
        }


        // Implement IDisposable to dispose of the context when the service is done
        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
