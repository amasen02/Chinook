using Chinook.ClientModels;
using Chinook.Models;
using Chinook.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
 
namespace Chinook.Services
{
    public class TrackService : ITrackService
    {
        private readonly IDbContextFactory<ChinookContext> _dbFactory;
        private ChinookContext _dbContext;

        public TrackService(IDbContextFactory<ChinookContext> dbFactory)
        {
            _dbFactory = dbFactory;
            _dbContext = _dbFactory.CreateDbContext();
        }

        public ClientModels.PlaylistTrack GetTrackByID(string userId, long trackID)
        {
            var track = _dbContext.Tracks
                .Where(t => t.TrackId == trackID)
                .Select(t => new ClientModels.PlaylistTrack
                {
                    AlbumTitle = t.Album.Title,
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Any(pt => pt.UserPlaylists.Any(up => up.UserId == userId))
                }).FirstOrDefault();
            return track;
        }
        public List<ClientModels.PlaylistTrack> GetTracks(long artistId, string userId)
        {
            var tracks = _dbContext.Tracks
                .Where(t => t.Album.ArtistId == artistId)
                .Select(t => new ClientModels.PlaylistTrack
                {
                    AlbumTitle = t.Album.Title,
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Any(pt => pt.UserPlaylists.Any(up => up.UserId == userId))
                })
                .ToList();
            return tracks;
        }
        public void FavoriteTrack(long trackId, string userId)
        {
            // Synchronously favorite a track using a predetermined method (this is a placeholder)
        }

        public void UnfavoriteTrack(long trackId, string userId)
        {
            // Synchronously unfavorite a track using a predetermined method (this is a placeholder)
        }

        public void RemoveTrack(long trackId, long playlistId)
        {
            var playlist = _dbContext.Playlists
                .Include(p => p.Tracks)
                .FirstOrDefault(p => p.PlaylistId == playlistId);

            if (playlist == null) throw new Exception($"Playlist with ID {playlistId} not found.");

            var track = _dbContext.Tracks.Find(trackId);
            if (track != null && playlist.Tracks.Contains(track))
            {
                playlist.Tracks.Remove(track);
                _dbContext.SaveChanges(); // Synchronously save changes
            }
            else
            {
                throw new Exception("Track not found in the specified playlist.");
            }
        }

        public void AddTrackToPlaylist(long trackId, string playlistName, string userId)
        {
            // Synchronous implementation for adding a track to a playlist
        }
    }
}

