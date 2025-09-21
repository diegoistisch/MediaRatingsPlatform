using MediaRatingsPlatform;

namespace MediaRatingsPlatform;

class Program
{
    static void Main(string[] args)
    {
        // Services erstellen
        IUserRepository userRepository = new UserRepository();
        IUserService userService = new UserService(userRepository);

        // Test User Registration
        var user1 = userService.Register("testuser", "test@example.com", "password123");
        if (user1 != null)
        {
            Console.WriteLine($"User registered: {user1.Username} (ID: {user1.Id})");
        }
        
        // Test Login
        var token2 = userService.Login("testuser", "password124");
        if (token2 != null)
        {
            Console.WriteLine($"Login successful! Token: {token2}");
        }
        else
        {
            Console.WriteLine($"Login failed!");
        }

        // Test Login
        var token = userService.Login("testuser", "password123");
        if (token != null)
        {
            Console.WriteLine($"Login successful! Token: {token}");
        }

        // Test Profile
        var profile = userService.GetUserProfile("testuser");
        if (profile != null)
        {
            Console.WriteLine($"Profile: {profile.Username}, Email: {profile.Email}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}