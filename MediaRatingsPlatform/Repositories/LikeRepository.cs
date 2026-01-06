using System.Data;
using MediaRatingsPlatform.Helpers;
using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;
using Npgsql;

namespace MediaRatingsPlatform.Repositories;

public class LikeRepository : ILikeRepository
{
    private readonly string _connectionString;

    public LikeRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public UserLike? GetById(int id)
    {
        return null;
    }

    public IEnumerable<UserLike> GetByUserId(int userId)
    {
        List<UserLike> likes = new List<UserLike>();
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    SELECT user_id, rating_id, created_at
                    FROM user_likes
                    WHERE user_id = @user_id
                ";

                command.AddParameterWithValue("user_id", DbType.Int32, userId);

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        likes.Add(new UserLike
                        {
                            UserId = reader.GetInt32(0),
                            RatingId = reader.GetInt32(1),
                            CreatedAt = reader.GetDateTime(2)
                        });
                    }
                }
            }
        }
        return likes;
    }

    public IEnumerable<UserLike> GetByRatingId(int ratingId)
    {
        List<UserLike> likes = new List<UserLike>();
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    SELECT user_id, rating_id, created_at
                    FROM user_likes
                    WHERE rating_id = @rating_id
                ";

                command.AddParameterWithValue("rating_id", DbType.Int32, ratingId);

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        likes.Add(new UserLike
                        {
                            UserId = reader.GetInt32(0),
                            RatingId = reader.GetInt32(1),
                            CreatedAt = reader.GetDateTime(2)
                        });
                    }
                }
            }
        }
        return likes;
    }

    public UserLike? GetByUserAndRating(int userId, int ratingId)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT user_id, rating_id, created_at
                    FROM user_likes
                    WHERE user_id = @user_id AND rating_id = @rating_id
                ";

                connection.Open();
                command.AddParameterWithValue("user_id", DbType.Int32, userId);
                command.AddParameterWithValue("rating_id", DbType.Int32, ratingId);

                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new UserLike
                        {
                            UserId = reader.GetInt32(0),
                            RatingId = reader.GetInt32(1),
                            CreatedAt = reader.GetDateTime(2)
                        };
                    }
                }
            }
        }
        return null;
    }

    public UserLike Create(UserLike like)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    INSERT INTO user_likes (user_id, rating_id, created_at)
                    VALUES (@user_id, @rating_id, @created_at)
                ";

                command.AddParameterWithValue("user_id", DbType.Int32, like.UserId);
                command.AddParameterWithValue("rating_id", DbType.Int32, like.RatingId);
                command.AddParameterWithValue("created_at", DbType.DateTime, DateTime.UtcNow);

                command.ExecuteNonQuery();
                like.CreatedAt = DateTime.UtcNow;
            }
        }
        return like;
    }

    public bool Delete(int userId, int ratingId)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"DELETE FROM user_likes WHERE user_id = @user_id AND rating_id = @rating_id";

                command.AddParameterWithValue("user_id", DbType.Int32, userId);
                command.AddParameterWithValue("rating_id", DbType.Int32, ratingId);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }

    public bool HasUserLikedRating(int userId, int ratingId)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"SELECT COUNT(*) FROM user_likes WHERE user_id = @user_id AND rating_id = @rating_id";

                command.AddParameterWithValue("user_id", DbType.Int32, userId);
                command.AddParameterWithValue("rating_id", DbType.Int32, ratingId);

                var result = command.ExecuteScalar();
                return result != null && Convert.ToInt32(result) > 0;
            }
        }
    }

    public int GetLikeCount(int ratingId)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"SELECT COUNT(*) FROM user_likes WHERE rating_id = @rating_id";

                command.AddParameterWithValue("rating_id", DbType.Int32, ratingId);

                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
    }
}
