using System.Data;
using MediaRatingsPlatform.Helpers;
using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;
using Npgsql;

namespace MediaRatingsPlatform.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly string _connectionString;

    public FavoriteRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public UserFavorite? GetById(int id)
    {
        return null;
    }

    public IEnumerable<UserFavorite> GetByUserId(int userId)
    {
        List<UserFavorite> favorites = new List<UserFavorite>();
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    SELECT user_id, media_id, created_at
                    FROM user_favorites
                    WHERE user_id = @user_id
                ";

                command.AddParameterWithValue("user_id", DbType.Int32, userId);

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        favorites.Add(new UserFavorite
                        {
                            UserId = reader.GetInt32(0),
                            MediaId = reader.GetInt32(1),
                            CreatedAt = reader.GetDateTime(2)
                        });
                    }
                }
            }
        }
        return favorites;
    }

    public IEnumerable<UserFavorite> GetByMediaId(int mediaId)
    {
        List<UserFavorite> favorites = new List<UserFavorite>();
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    SELECT user_id, media_id, created_at
                    FROM user_favorites
                    WHERE media_id = @media_id
                ";

                command.AddParameterWithValue("media_id", DbType.Int32, mediaId);

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        favorites.Add(new UserFavorite
                        {
                            UserId = reader.GetInt32(0),
                            MediaId = reader.GetInt32(1),
                            CreatedAt = reader.GetDateTime(2)
                        });
                    }
                }
            }
        }
        return favorites;
    }

    public UserFavorite? GetByUserAndMedia(int userId, int mediaId)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT user_id, media_id, created_at
                    FROM user_favorites
                    WHERE user_id = @user_id AND media_id = @media_id
                ";

                connection.Open();
                command.AddParameterWithValue("user_id", DbType.Int32, userId);
                command.AddParameterWithValue("media_id", DbType.Int32, mediaId);

                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new UserFavorite
                        {
                            UserId = reader.GetInt32(0),
                            MediaId = reader.GetInt32(1),
                            CreatedAt = reader.GetDateTime(2)
                        };
                    }
                }
            }
        }
        return null;
    }

    public UserFavorite Create(UserFavorite favorite)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    INSERT INTO user_favorites (user_id, media_id, created_at)
                    VALUES (@user_id, @media_id, @created_at)
                ";

                command.AddParameterWithValue("user_id", DbType.Int32, favorite.UserId);
                command.AddParameterWithValue("media_id", DbType.Int32, favorite.MediaId);
                command.AddParameterWithValue("created_at", DbType.DateTime, DateTime.UtcNow);

                command.ExecuteNonQuery();
                favorite.CreatedAt = DateTime.UtcNow;
            }
        }
        return favorite;
    }

    public bool Delete(int userId, int mediaId)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"DELETE FROM user_favorites WHERE user_id = @user_id AND media_id = @media_id";

                command.AddParameterWithValue("user_id", DbType.Int32, userId);
                command.AddParameterWithValue("media_id", DbType.Int32, mediaId);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }

    public bool IsFavorite(int userId, int mediaId)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"SELECT COUNT(*) FROM user_favorites WHERE user_id = @user_id AND media_id = @media_id";

                command.AddParameterWithValue("user_id", DbType.Int32, userId);
                command.AddParameterWithValue("media_id", DbType.Int32, mediaId);

                var result = command.ExecuteScalar();
                return result != null && Convert.ToInt32(result) > 0;
            }
        }
    }
}
