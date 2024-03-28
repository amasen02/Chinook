using Chinook.ClientModels;
using Chinook.Models;
using Chinook.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chinook.Services
{
    public class ArtistService : IArtistService, IDisposable
    {
        private readonly IDbContextFactory<ChinookContext> _dbFactory;
        private ChinookContext _dbContext;

        public ArtistService(IDbContextFactory<ChinookContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _dbContext = _dbFactory.CreateDbContext();
        }

        // Retrieves an artist and their albums by artist ID.
        public Artist GetArtist(long artistId)
        {
            var artist = _dbContext.Artists
                        .Include(a => a.Albums)
                        .FirstOrDefault(a => a.ArtistId == artistId);

            if (artist == null)
            {
                throw new Exception($"Artist with ID {artistId} not found.");
            }
            return artist;
        }

        // Fetches tracks associated with an artist, indicating if each track is a user's favorite.
        public List<PlaylistTrack> GetTracks(long artistId, string userId)
        {
            var tracks = _dbContext.Tracks
                        .Where(t => t.Album.ArtistId == artistId)
                        .Select(t => new PlaylistTrack
                        {
                            AlbumTitle = t.Album.Title,
                            TrackId = t.TrackId,
                            TrackName = t.Name,
                            // Determine if the track is marked as favorite by the user.
                            IsFavorite = t.Playlists.Any(pt => pt.UserPlaylists.Any(up => up.UserId == userId))
                        })
                        .ToList();
            return tracks;
        }

        // Asynchronously retrieves all artists including their albums.
        public async Task<List<Artist>> GetArtists()
        {
            return await _dbContext.Artists
                .Include(a => a.Albums)
                .ToListAsync();
        }

        // Asynchronously finds artists by name, including their albums. The search is case-insensitive.
        public async Task<List<Artist>> GetArtistsByName(string name)
        {
            return await _dbContext.Artists
                .Where(a => EF.Functions.Like(a.Name, $"%{name}%"))
                .Include(a => a.Albums)
                .ToListAsync();
        }

        // Asynchronously gets albums for a specific artist. Note: Updated to async pattern.
        public async Task<List<Album>> GetAlbumsForArtist(int artistId)
        {
            // This method was updated to use async-await pattern for consistency.
            return await _dbContext.Albums.Where(a => a.ArtistId == artistId).ToListAsync();
        }

        // Disposes the DbContext to free up resources.
        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
