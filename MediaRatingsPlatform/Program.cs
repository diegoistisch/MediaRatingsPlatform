using MediaRatingsPlatform.Helpers;
using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Repositories;
using MediaRatingsPlatform.Services;

namespace MediaRatingsPlatform;

class Program
{
    static void Main(string[] args)
    {
        // Initialize services
        IUserRepository userRepository = new UserRepository();
        IUserService userService = new UserService(userRepository);

        // Create HTTP Server with dependencies
        var server = new HttpServer("http://localhost:8080/", userService);

        Console.WriteLine("Starting Media Ratings Platform Server...");
        server.Start();

        Console.WriteLine("Server is running. Press Ctrl+C to stop...");

        var exitEvent = new ManualResetEvent(false);
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            exitEvent.Set();
        };

        exitEvent.WaitOne();
        server.Stop();
    }
}