using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Chinook.Services.Interfaces;
using Chinook.ClientModels;
using Chinook.Models;

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

        // Fetches a single track and determines if it is a favorite for the user.
        public ClientModels.PlaylistTrack GetTrackByID(string userId, long trackID)
        {
            return _dbContext.Tracks
                .Where(t => t.TrackId == trackID)
                .Select(t => new ClientModels.PlaylistTrack
                {
                    AlbumTitle = t.Album.Title,
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Any(pt => pt.UserPlaylists.Any(up => up.UserId == userId))
                })
                .FirstOrDefault();
        }

        // Retrieves all tracks for a specific artist, marking those that are favorites.
        public List<ClientModels.PlaylistTrack> GetTracks(long artistId, string userId)
        {
            return _dbContext.Tracks
                .Where(t => t.Album.ArtistId == artistId)
                .Select(t => new ClientModels.PlaylistTrack
                {
                    AlbumTitle = t.Album.Title,
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Any(pt => pt.UserPlaylists.Any(up => up.UserId == userId))
                })
                .ToList();
        }

        // Marks a track as a favorite by adding it to the "My Favorite Tracks" playlist.
        public void FavoriteTrack(long trackId, string userId)
        {
            var playlist = EnsureFavoritePlaylistExists(userId);

            if (!playlist.Tracks.Any(t => t.TrackId == trackId))
            {
                var track = _dbContext.Tracks.Find(trackId);
                if (track != null) playlist.Tracks.Add(track);
                _dbContext.SaveChanges();
            }
        }

        // Removes a track from the user's "My Favorite Tracks" playlist.
        public void UnfavoriteTrack(long trackId, string userId)
        {
            var playlist = _dbContext.Playlists
                .Include(p => p.Tracks)
                .Include(p => p.UserPlaylists)
                .FirstOrDefault(p => p.Name == "My Favorite Tracks" && p.UserPlaylists.Any(up => up.UserId == userId));

            var track = playlist?.Tracks.FirstOrDefault(t => t.TrackId == trackId);
            if (track != null)
            {
                playlist.Tracks.Remove(track);
                _dbContext.SaveChanges();
            }
        }

        // Removes a track from a specific playlist.
        public void RemoveTrack(long trackId, long playlistId)
        {
            var playlist = _dbContext.Playlists
                .Include(p => p.Tracks)
                .FirstOrDefault(p => p.PlaylistId == playlistId);

            if (playlist == null) throw new Exception($"Playlist with ID {playlistId} not found.");

            var track = playlist.Tracks.FirstOrDefault(t => t.TrackId == trackId);
            if (track != null)
            {
                playlist.Tracks.Remove(track);
                _dbContext.SaveChanges();
            }
            else throw new Exception("Track not found in the specified playlist.");
        }

        // Adds a track to a playlist, creating the playlist if it doesn't exist.
        public void AddTrackToPlaylist(long trackId, string playlistName, string userId)
        {
            var playlist = _dbContext.Playlists
                .Include(p => p.UserPlaylists)
                .Include(p => p.Tracks)
                .FirstOrDefault(p => p.Name == playlistName && p.UserPlaylists.Any(up => up.UserId == userId))
                ?? CreatePlaylist(playlistName, userId);

            if (!playlist.Tracks.Any(t => t.TrackId == trackId))
            {
                var track = _dbContext.Tracks.Find(trackId);
                if (track != null) playlist.Tracks.Add(track);
                else throw new Exception($"Track with ID {trackId} not found.");

                _dbContext.SaveChanges();
            }
        }

        private Models.Playlist EnsureFavoritePlaylistExists(string userId)
        {
            var favoritePlaylist = _dbContext.Playlists
                .Include(p => p.UserPlaylists)
                .FirstOrDefault(p => p.Name == "My Favorite Tracks" && p.UserPlaylists.Any(up => up.UserId == userId))
                ?? CreatePlaylist("My Favorite Tracks", userId);

            return favoritePlaylist;
        }

        private Models.Playlist CreatePlaylist(string name, string userId)
        {
            var newPlaylist = new Models.Playlist
            {
                Name = name,
                UserPlaylists = new List<UserPlaylist> { new UserPlaylist { UserId = userId } },
                Tracks = new List<Track>()
            };
            _dbContext.Playlists.Add(newPlaylist);
            _dbContext.SaveChanges();
            return newPlaylist;
        }
    }
}
