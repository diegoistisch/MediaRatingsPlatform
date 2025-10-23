using System.Data;
using MediaRatingsPlatform.Helpers;
using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;
using Npgsql;

namespace MediaRatingsPlatform.Repositories;

public class MediaRepository : IMediaRepository
{
    private readonly string _connectionString;

    public MediaRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public MediaEntry Create(MediaEntry mediaEntry)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    INSERT INTO media_entries (creator_id, title, description, type, age_restriction, release_year, genres, average_rating, created_at)
                    VALUES (@creator_id, @title, @description, @type, @age_restriction, @release_year, @genres, @average_rating, @created_at)
                    RETURNING id
                ";

                command.AddParameterWithValue("creator_id", DbType.Int32, mediaEntry.CreatorId);
                command.AddParameterWithValue("title", DbType.String, mediaEntry.Title);
                command.AddParameterWithValue("description", DbType.String, mediaEntry.Description);
                command.AddParameterWithValue("type", DbType.Int32, (int)mediaEntry.Type);
                command.AddParameterWithValue("age_restriction", DbType.Int32, (int)mediaEntry.AgeRestriction);
                command.AddParameterWithValue("release_year", DbType.Int32, mediaEntry.ReleaseYear);
                command.AddParameterWithValue("genres", DbType.String, string.Join(",", mediaEntry.Genres ?? new List<string>()));
                command.AddParameterWithValue("average_rating", DbType.Decimal, 0.0);
                command.AddParameterWithValue("created_at", DbType.DateTime, DateTime.UtcNow);

                var result = command.ExecuteScalar();
                if (result != null)
                {
                    mediaEntry.Id = Convert.ToInt32(result);
                    mediaEntry.CreatedAt = DateTime.UtcNow;
                    mediaEntry.AverageRating = 0;
                    mediaEntry.RatingsList = new List<Rating>();
                }
            }
        }
        return mediaEntry;
    }

    public MediaEntry? GetById(int id)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT id, creator_id, title, description, type, age_restriction, release_year, genres, average_rating, created_at
                    FROM media_entries
                    WHERE id = @id
                ";

                connection.Open();
                command.AddParameterWithValue("id", DbType.Int32, id);

                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapMediaEntry(reader);
                    }
                }
            }
        }
        return null;
    }

    public IEnumerable<MediaEntry> GetAll()
    {
        List<MediaEntry> mediaEntries = new List<MediaEntry>();
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    SELECT id, creator_id, title, description, type, age_restriction, release_year, genres, average_rating, created_at
                    FROM media_entries
                ";

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        mediaEntries.Add(MapMediaEntry(reader));
                    }
                }
            }
        }
        return mediaEntries;
    }

    public IEnumerable<MediaEntry> GetByCreatorId(int creatorId)
    {
        List<MediaEntry> mediaEntries = new List<MediaEntry>();
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    SELECT id, creator_id, title, description, type, age_restriction, release_year, genres, average_rating, created_at
                    FROM media_entries
                    WHERE creator_id = @creator_id
                ";

                command.AddParameterWithValue("creator_id", DbType.Int32, creatorId);

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        mediaEntries.Add(MapMediaEntry(reader));
                    }
                }
            }
        }
        return mediaEntries;
    }

    public IEnumerable<MediaEntry> SearchByTitle(string title)
    {
        List<MediaEntry> mediaEntries = new List<MediaEntry>();
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    SELECT id, creator_id, title, description, type, age_restriction, release_year, genres, average_rating, created_at
                    FROM media_entries
                    WHERE LOWER(title) LIKE LOWER(@title)
                ";

                command.AddParameterWithValue("title", DbType.String, $"%{title}%");

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        mediaEntries.Add(MapMediaEntry(reader));
                    }
                }
            }
        }
        return mediaEntries;
    }

    public IEnumerable<MediaEntry> FilterByGenre(string genre)
    {
        List<MediaEntry> mediaEntries = new List<MediaEntry>();
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    SELECT id, creator_id, title, description, type, age_restriction, release_year, genres, average_rating, created_at
                    FROM media_entries
                    WHERE LOWER(genres) LIKE LOWER(@genre)
                ";

                command.AddParameterWithValue("genre", DbType.String, $"%{genre}%");

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        mediaEntries.Add(MapMediaEntry(reader));
                    }
                }
            }
        }
        return mediaEntries;
    }

    public IEnumerable<MediaEntry> FilterByType(Enums.MediaType type)
    {
        List<MediaEntry> mediaEntries = new List<MediaEntry>();
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    SELECT id, creator_id, title, description, type, age_restriction, release_year, genres, average_rating, created_at
                    FROM media_entries
                    WHERE type = @type
                ";

                command.AddParameterWithValue("type", DbType.Int32, (int)type);

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        mediaEntries.Add(MapMediaEntry(reader));
                    }
                }
            }
        }
        return mediaEntries;
    }

    public IEnumerable<MediaEntry> FilterByYear(int year)
    {
        List<MediaEntry> mediaEntries = new List<MediaEntry>();
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    SELECT id, creator_id, title, description, type, age_restriction, release_year, genres, average_rating, created_at
                    FROM media_entries
                    WHERE release_year = @year
                ";

                command.AddParameterWithValue("year", DbType.Int32, year);

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        mediaEntries.Add(MapMediaEntry(reader));
                    }
                }
            }
        }
        return mediaEntries;
    }

    public IEnumerable<MediaEntry> FilterByAgeRestriction(Enums.AgeRestriction ageRestriction)
    {
        List<MediaEntry> mediaEntries = new List<MediaEntry>();
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    SELECT id, creator_id, title, description, type, age_restriction, release_year, genres, average_rating, created_at
                    FROM media_entries
                    WHERE age_restriction = @age_restriction
                ";

                command.AddParameterWithValue("age_restriction", DbType.Int32, (int)ageRestriction);

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        mediaEntries.Add(MapMediaEntry(reader));
                    }
                }
            }
        }
        return mediaEntries;
    }

    public MediaEntry? Update(MediaEntry mediaEntry)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    UPDATE media_entries
                    SET title = @title, description = @description, type = @type,
                        age_restriction = @age_restriction, release_year = @release_year, genres = @genres
                    WHERE id = @id
                ";

                command.AddParameterWithValue("id", DbType.Int32, mediaEntry.Id);
                command.AddParameterWithValue("title", DbType.String, mediaEntry.Title);
                command.AddParameterWithValue("description", DbType.String, mediaEntry.Description);
                command.AddParameterWithValue("type", DbType.Int32, (int)mediaEntry.Type);
                command.AddParameterWithValue("age_restriction", DbType.Int32, (int)mediaEntry.AgeRestriction);
                command.AddParameterWithValue("release_year", DbType.Int32, mediaEntry.ReleaseYear);
                command.AddParameterWithValue("genres", DbType.String, string.Join(",", mediaEntry.Genres ?? new List<string>()));

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    return GetById(mediaEntry.Id);
                }
            }
        }
        return null;
    }

    public bool Delete(int id)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"DELETE FROM media_entries WHERE id = @id";

                command.AddParameterWithValue("id", DbType.Int32, id);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }

    public bool Exists(int id)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"SELECT COUNT(*) FROM media_entries WHERE id = @id";

                command.AddParameterWithValue("id", DbType.Int32, id);

                var result = command.ExecuteScalar();
                return result != null && Convert.ToInt32(result) > 0;
            }
        }
    }

    private MediaEntry MapMediaEntry(IDataReader reader)
    {
        var genresString = reader.GetString(7);
        var genres = string.IsNullOrWhiteSpace(genresString)
            ? new List<string>()
            : genresString.Split(',').ToList();

        return new MediaEntry
        {
            Id = reader.GetInt32(0),
            CreatorId = reader.GetInt32(1),
            Title = reader.GetString(2),
            Description = reader.GetString(3),
            Type = (Enums.MediaType)reader.GetInt32(4),
            AgeRestriction = (Enums.AgeRestriction)reader.GetInt32(5),
            ReleaseYear = reader.GetInt32(6),
            Genres = genres,
            AverageRating = Convert.ToDouble(reader.GetDecimal(8)),
            CreatedAt = reader.GetDateTime(9),
            RatingsList = new List<Rating>()
        };
    }
}