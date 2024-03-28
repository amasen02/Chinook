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
            var favoritePlaylist = _dbContext.Playlists
                .Include(p => p.UserPlaylists)
                .FirstOrDefault(p => p.Name == "My Favorite Tracks" && p.UserPlaylists.Any(up => up.UserId == userId));

            if (favoritePlaylist == null)
            {
                favoritePlaylist = new Models.Playlist
                {
                    Name = "My Favorite Tracks",
                    UserPlaylists = new List<UserPlaylist>
            {
                new UserPlaylist { UserId = userId }
            }
                };
                _dbContext.Playlists.Add(favoritePlaylist);
                _dbContext.SaveChanges();
            }

            var track = _dbContext.Tracks.Find(trackId);
            if (!favoritePlaylist.Tracks.Contains(track))
            {
                favoritePlaylist.Tracks.Add(track);
                _dbContext.SaveChanges();
            }
        }

        public void UnfavoriteTrack(long trackId, string userId)
        {
            var favoritePlaylist = _dbContext.Playlists
                .Include(p => p.Tracks)
                .Include(p => p.UserPlaylists)
                .FirstOrDefault(p => p.Name == "My Favorite Tracks" && p.UserPlaylists.Any(up => up.UserId == userId));

            if (favoritePlaylist != null)
            {
                var track = favoritePlaylist.Tracks.FirstOrDefault(t => t.TrackId == trackId);
                if (track != null)
                {
                    favoritePlaylist.Tracks.Remove(track);
                    _dbContext.SaveChanges();
                }
            }
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
            // First, try to find an existing playlist with the given name and user ID.
            var playlist = _dbContext.Playlists
                .Include(p => p.UserPlaylists)
                .Include(p => p.Tracks) // Ensure Tracks are included for manipulation
                .FirstOrDefault(p => p.Name == playlistName && p.UserPlaylists.Any(up => up.UserId == userId));

            // If the playlist does not exist, create a new one.
            if (playlist == null)
            {
                playlist = new Models.Playlist
                {
                    Name = playlistName,
                    UserPlaylists = new List<UserPlaylist> { new UserPlaylist { UserId = userId } },
                    Tracks = new List<Track>()
                };
                _dbContext.Playlists.Add(playlist);
            }

            // Check if the track is already in the playlist to avoid duplicates.
            if (!playlist.Tracks.Any(t => t.TrackId == trackId))
            {
                // Find the track by ID.
                var trackToAdd = _dbContext.Tracks.Find(trackId);
                if (trackToAdd != null)
                {
                    // Add the track to the playlist.
                    playlist.Tracks.Add(trackToAdd);
                }
                else
                {
                    throw new Exception($"Track with ID {trackId} not found.");
                }
            }

            // Save changes to the database.
            _dbContext.SaveChanges();
        }
    }
}

