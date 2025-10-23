using System.Data;
using Npgsql;

namespace MediaRatingsPlatform.Helpers;

public static class DatabaseInitializer
{
    public static void InitDatabase(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        string dbName = builder.Database ?? "mediaratingdb";

        // Connect without database to create it if needed
        builder.Remove("Database");
        string cs = builder.ToString();

        using (IDbConnection connection = new NpgsqlConnection(cs))
        {
            connection.Open();

            using (IDbCommand cmd = connection.CreateCommand())
            {
                // Check if database exists, if not create it
                cmd.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{dbName}'";
                var result = cmd.ExecuteScalar();

                if (result == null)
                {
                    cmd.CommandText = $"CREATE DATABASE {dbName}";
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"Database '{dbName}' created successfully.");
                }
            }
        }

        // Now connect to the actual database to create tables
        using (IDbConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            CreateTables(connection);
            Console.WriteLine("All tables created successfully.");
        }
    }

    private static void CreateTables(IDbConnection connection)
    {
        // Create Users table
        using (IDbCommand cmd = connection.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS users (
                    id SERIAL PRIMARY KEY,
                    username VARCHAR(50) UNIQUE NOT NULL,
                    email VARCHAR(100) NOT NULL,
                    password VARCHAR(255) NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )
            ";
            cmd.ExecuteNonQuery();
        }

        // Create MediaEntries table
        using (IDbCommand cmd = connection.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS media_entries (
                    id SERIAL PRIMARY KEY,
                    creator_id INT NOT NULL,
                    title VARCHAR(255) NOT NULL,
                    description TEXT NOT NULL,
                    type INT NOT NULL,
                    age_restriction INT NOT NULL,
                    release_year INT NOT NULL,
                    genres TEXT NOT NULL,
                    average_rating DECIMAL(3,2) DEFAULT 0.0,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (creator_id) REFERENCES users(id) ON DELETE CASCADE
                )
            ";
            cmd.ExecuteNonQuery();
        }

        // Create Ratings table
        using (IDbCommand cmd = connection.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS ratings (
                    id SERIAL PRIMARY KEY,
                    user_id INT NOT NULL,
                    media_id INT NOT NULL,
                    stars INT NOT NULL CHECK (stars >= 1 AND stars <= 5),
                    comment TEXT,
                    is_comment_confirmed BOOLEAN DEFAULT FALSE,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                    FOREIGN KEY (media_id) REFERENCES media_entries(id) ON DELETE CASCADE,
                    UNIQUE(user_id, media_id)
                )
            ";
            cmd.ExecuteNonQuery();
        }

        // Create UserFavorites table
        using (IDbCommand cmd = connection.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS user_favorites (
                    user_id INT NOT NULL,
                    media_id INT NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY (user_id, media_id),
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                    FOREIGN KEY (media_id) REFERENCES media_entries(id) ON DELETE CASCADE
                )
            ";
            cmd.ExecuteNonQuery();
        }

        // Create UserLikes table
        using (IDbCommand cmd = connection.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS user_likes (
                    user_id INT NOT NULL,
                    rating_id INT NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY (user_id, rating_id),
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
                    FOREIGN KEY (rating_id) REFERENCES ratings(id) ON DELETE CASCADE
                )
            ";
            cmd.ExecuteNonQuery();
        }

        // Create index for faster queries
        using (IDbCommand cmd = connection.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE INDEX IF NOT EXISTS idx_media_creator ON media_entries(creator_id);
                CREATE INDEX IF NOT EXISTS idx_ratings_media ON ratings(media_id);
                CREATE INDEX IF NOT EXISTS idx_ratings_user ON ratings(user_id);
            ";
            cmd.ExecuteNonQuery();
        }
    }
}
