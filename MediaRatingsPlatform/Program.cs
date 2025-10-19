using MediaRatingsPlatform;

namespace MediaRatingsPlatform;

class Program
{
    static void Main(string[] args)
    {
        // Create HTTP Server
        var server = new HttpServer("http://localhost:8080/");

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