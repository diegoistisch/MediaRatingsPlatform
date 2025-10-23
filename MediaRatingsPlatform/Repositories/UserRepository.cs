using System.Data;
using MediaRatingsPlatform.Helpers;
using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;
using Npgsql;

namespace MediaRatingsPlatform.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public User? GetByUsername(string username)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT id, username, email, password, created_at, updated_at
                    FROM users
                    WHERE username = @username
                ";

                connection.Open();
                command.AddParameterWithValue("username", DbType.String, username);

                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Email = reader.GetString(2),
                            Password = reader.GetString(3),
                            CreatedAt = reader.GetDateTime(4),
                            UpdatedAt = reader.GetDateTime(5)
                        };
                    }
                }
            }
        }
        return null;
    }

    public User? GetById(int id)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT id, username, email, password, created_at, updated_at
                    FROM users
                    WHERE id = @id
                ";

                connection.Open();
                command.AddParameterWithValue("id", DbType.Int32, id);

                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Email = reader.GetString(2),
                            Password = reader.GetString(3),
                            CreatedAt = reader.GetDateTime(4),
                            UpdatedAt = reader.GetDateTime(5)
                        };
                    }
                }
            }
        }
        return null;
    }

    public void Add(User user)
    {
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    INSERT INTO users (username, email, password, created_at, updated_at)
                    VALUES (@username, @email, @password, @created_at, @updated_at)
                    RETURNING id
                ";

                command.AddParameterWithValue("username", DbType.String, user.Username);
                command.AddParameterWithValue("email", DbType.String, user.Email);
                command.AddParameterWithValue("password", DbType.String, user.Password);
                command.AddParameterWithValue("created_at", DbType.DateTime, DateTime.UtcNow);
                command.AddParameterWithValue("updated_at", DbType.DateTime, DateTime.UtcNow);

                var result = command.ExecuteScalar();
                if (result != null)
                {
                    user.Id = Convert.ToInt32(result);
                    user.CreatedAt = DateTime.UtcNow;
                    user.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }

    public List<User> GetAll()
    {
        List<User> users = new List<User>();
        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            using (IDbCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = @"
                    SELECT id, username, email, password, created_at, updated_at
                    FROM users
                ";

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Email = reader.GetString(2),
                            Password = reader.GetString(3),
                            CreatedAt = reader.GetDateTime(4),
                            UpdatedAt = reader.GetDateTime(5)
                        });
                    }
                }
            }
        }
        return users;
    }
}