using System.Data;
using MediaRatingsPlatform.Helpers;
using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;
using Npgsql;

namespace MediaRatingsPlatform.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly string _connectionString;

    public RatingRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Rating? GetById(int id)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT id, user_id, media_id, stars, comment, is_comment_confirmed, created_at
                    FROM ratings
                    WHERE id = @id
                ";

                connection.Open();
                command.AddParameterWithValue("id", DbType.Int32, id);

                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapRating(reader);
                    }
                }
            }
        }
        return null;
    }

    public IEnumerable<Rating> GetByMediaId(int mediaId)
    {
        List<Rating> ratings = new List<Rating>();
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    SELECT id, user_id, media_id, stars, comment, is_comment_confirmed, created_at
                    FROM ratings
                    WHERE media_id = @media_id
                ";

                command.AddParameterWithValue("media_id", DbType.Int32, mediaId);

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ratings.Add(MapRating(reader));
                    }
                }
            }
        }
        return ratings;
    }

    public IEnumerable<Rating> GetByUserId(int userId)
    {
        List<Rating> ratings = new List<Rating>();
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    SELECT id, user_id, media_id, stars, comment, is_comment_confirmed, created_at
                    FROM ratings
                    WHERE user_id = @user_id
                ";

                command.AddParameterWithValue("user_id", DbType.Int32, userId);

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ratings.Add(MapRating(reader));
                    }
                }
            }
        }
        return ratings;
    }

    public Rating? GetByUserAndMedia(int userId, int mediaId)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT id, user_id, media_id, stars, comment, is_comment_confirmed, created_at
                    FROM ratings
                    WHERE user_id = @user_id AND media_id = @media_id
                ";

                connection.Open();
                command.AddParameterWithValue("user_id", DbType.Int32, userId);
                command.AddParameterWithValue("media_id", DbType.Int32, mediaId);

                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapRating(reader);
                    }
                }
            }
        }
        return null;
    }

    public Rating Create(Rating rating)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    INSERT INTO ratings (user_id, media_id, stars, comment, is_comment_confirmed, created_at)
                    VALUES (@user_id, @media_id, @stars, @comment, @is_comment_confirmed, @created_at)
                    RETURNING id
                ";

                command.AddParameterWithValue("user_id", DbType.Int32, rating.UserId);
                command.AddParameterWithValue("media_id", DbType.Int32, rating.MediaId);
                command.AddParameterWithValue("stars", DbType.Int32, rating.Stars);
                command.AddParameterWithValue("comment", DbType.String, rating.Comment ?? (object)DBNull.Value);
                command.AddParameterWithValue("is_comment_confirmed", DbType.Boolean, rating.IsCommentConfirmed);
                command.AddParameterWithValue("created_at", DbType.DateTime, DateTime.UtcNow);

                var result = command.ExecuteScalar();
                if (result != null)
                {
                    rating.Id = Convert.ToInt32(result);
                    rating.CreatedAt = DateTime.UtcNow;
                }
            }
        }
        return rating;
    }

    public Rating? Update(Rating rating)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    UPDATE ratings
                    SET stars = @stars, comment = @comment, is_comment_confirmed = @is_comment_confirmed
                    WHERE id = @id
                ";

                command.AddParameterWithValue("id", DbType.Int32, rating.Id);
                command.AddParameterWithValue("stars", DbType.Int32, rating.Stars);
                command.AddParameterWithValue("comment", DbType.String, rating.Comment ?? (object)DBNull.Value);
                command.AddParameterWithValue("is_comment_confirmed", DbType.Boolean, rating.IsCommentConfirmed);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    return GetById(rating.Id);
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
                command.CommandText = @"DELETE FROM ratings WHERE id = @id";

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
                command.CommandText = @"SELECT COUNT(*) FROM ratings WHERE id = @id";

                command.AddParameterWithValue("id", DbType.Int32, id);

                var result = command.ExecuteScalar();
                return result != null && Convert.ToInt32(result) > 0;
            }
        }
    }

    public bool UserHasRated(int userId, int mediaId)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"SELECT COUNT(*) FROM ratings WHERE user_id = @user_id AND media_id = @media_id";

                command.AddParameterWithValue("user_id", DbType.Int32, userId);
                command.AddParameterWithValue("media_id", DbType.Int32, mediaId);

                var result = command.ExecuteScalar();
                return result != null && Convert.ToInt32(result) > 0;
            }
        }
    }

    public double CalculateAverageRating(int mediaId)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"SELECT AVG(stars) FROM ratings WHERE media_id = @media_id";
                command.AddParameterWithValue("media_id", DbType.Int32, mediaId);

                var result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToDouble(result);
                }
                return 0.0;
            }
        }
    }

    public Dictionary<int, int> GetTopActiveUsers(int limit)
    {
        var result = new Dictionary<int, int>();
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    SELECT user_id, COUNT(*) as count 
                    FROM ratings 
                    GROUP BY user_id 
                    ORDER BY count DESC 
                    LIMIT @limit
                ";
                command.AddParameterWithValue("limit", DbType.Int32, limit);

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetInt32(0), reader.ConvertToInt(1)); // Helper needed for Int64->Int32 usually COUNT returns long
                    }
                }
            }
        }
        return result;
    }

    private Rating MapRating(IDataReader reader)
    {
        return new Rating
        {
            Id = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            MediaId = reader.GetInt32(2),
            Stars = reader.GetInt32(3),
            Comment = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
            IsCommentConfirmed = reader.GetBoolean(5),
            CreatedAt = reader.GetDateTime(6)
        };
    }
}

// Extension method helper for reader
public static class ReaderExtensions
{
    public static int ConvertToInt(this IDataReader reader, int index)
    {
         var val = reader.GetValue(index);
         return Convert.ToInt32(val);
    }
}
