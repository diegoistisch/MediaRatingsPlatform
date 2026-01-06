using MediaRatingsPlatform.Helpers;
using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Repositories;
using MediaRatingsPlatform.Services;

namespace MediaRatingsPlatform;

class Program
{
    // Connection string for PostgreSQL running in Docker
    private const string CONNECTION_STRING = "Host=localhost;Port=5432;Username=postgres;Password=mysecretpassword;Database=mediaratingdb";

    static void Main(string[] args)
    {
        Console.WriteLine("Initializing database...");
        try
        {
            // Initialize database and create tables
            DatabaseInitializer.InitDatabase(CONNECTION_STRING);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize database: {ex.Message}");
            Console.WriteLine("Make sure PostgreSQL is running in Docker.");
            return;
        }

        // Initialize repositories with connection string
        IUserRepository userRepository = new UserRepository(CONNECTION_STRING);
        IMediaRepository mediaRepository = new MediaRepository(CONNECTION_STRING);
        IRatingRepository ratingRepository = new RatingRepository(CONNECTION_STRING);
        IFavoriteRepository favoriteRepository = new FavoriteRepository(CONNECTION_STRING);
        ILikeRepository likeRepository = new LikeRepository(CONNECTION_STRING);

        // Initialize services
        IUserService userService = new UserService(userRepository);
        IMediaService mediaService = new MediaService(mediaRepository);
        IRatingService ratingService = new RatingService(ratingRepository, mediaRepository);
        IFavoriteService favoriteService = new FavoriteService(favoriteRepository, mediaRepository);
        ILikeService likeService = new LikeService(likeRepository, ratingRepository);
        ILeaderboardService leaderboardService = new LeaderboardService(ratingRepository, userRepository);
        IRecommendationService recommendationService = new RecommendationService(ratingRepository, mediaRepository);
        IStatisticsService statisticsService = new StatisticsService(ratingRepository, favoriteRepository, mediaRepository);

        // Create HTTP Server with dependencies
        var server = new HttpServer("http://localhost:8080/", userService, mediaService, ratingService, favoriteService, likeService, leaderboardService, recommendationService, statisticsService);

        Console.WriteLine("Starting Media Ratings Platform Server...");
        server.Start();

        Console.WriteLine("Server is running on http://localhost:8080/");
        Console.WriteLine("Press Ctrl+C to stop...");

        var exitEvent = new ManualResetEvent(false);
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            exitEvent.Set();
        };

        exitEvent.WaitOne();
        server.Stop();
        Console.WriteLine("Server stopped.");
    }
}