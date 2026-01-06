using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Client;

class TestClient
{
    private static readonly HttpClient client = new HttpClient();
    private static string? authToken = null;
    private static int? currentUserId = null;

    public static async Task Run()
    {
        client.BaseAddress = new Uri("http://localhost:8080");

        Console.WriteLine("=== Media Ratings Platform CLI ===\n");

        while (true)
        {
            Console.WriteLine("\n--------------------------------");
            Console.WriteLine("USER:");
            Console.WriteLine("1 - Register");
            Console.WriteLine("2 - Login");
            Console.WriteLine("3 - Get Profile (Stats)");
            Console.WriteLine("4 - View My Ratings");
            Console.WriteLine("5 - View My Favorites");
            
            Console.WriteLine("\nMEDIA:");
            Console.WriteLine("6 - Search/List Media");
            Console.WriteLine("7 - Create Media");
            
            Console.WriteLine("\nINTERACTIONS:");
            Console.WriteLine("8 - Rate Media");
            Console.WriteLine("9 - Favorite Media");
            Console.WriteLine("10 - Like Rating");
            Console.WriteLine("11 - Confirm Comment (Mod)");
            
            Console.WriteLine("\nDISCOVERY:");
            Console.WriteLine("12 - Leaderboard");
            Console.WriteLine("13 - Recommendations");
            
            Console.WriteLine("\n0 - Exit");
            Console.Write("Wähle: ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1": await Register(); break;
                    case "2": await Login(); break;
                    case "3": await GetProfile(); break;
                    case "4": await GetMyRatings(); break;
                    case "5": await GetMyFavorites(); break;
                    case "6": await SearchMedia(); break;
                    case "7": await CreateMedia(); break;
                    case "8": await RateMedia(); break;
                    case "9": await FavoriteMedia(); break;
                    case "10": await LikeRating(); break;
                    case "11": await ConfirmComment(); break;
                    case "12": await GetLeaderboard(); break;
                    case "13": await GetRecommendations(); break;
                    case "0": return;
                    default: Console.WriteLine("Ungültige Eingabe!"); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler: {ex.Message}");
            }
        }
    }

    private static void AuthorizeRequest(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(authToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        }
    }

    private static async Task<bool> CheckLogin()
    {
        if (string.IsNullOrEmpty(authToken))
        {
            Console.WriteLine("Error: Bitte zuerst einloggen!");
            return false;
        }
        return true;
    }

    // --- USER ---

    static async Task Register()
    {
        Console.Write("Username: ");
        var username = Console.ReadLine();
        Console.Write("Email: ");
        var email = Console.ReadLine();
        Console.Write("Password: ");
        var password = Console.ReadLine();

        var data = new { username, email, password };
        await SendJsonRequest(HttpMethod.Post, "/api/users/register", data);
    }

    static async Task Login()
    {
        Console.Write("Username: ");
        var username = Console.ReadLine();
        Console.Write("Password: ");
        var password = Console.ReadLine();

        var data = new { username, password };
        var jsonDoc = await SendJsonRequest(HttpMethod.Post, "/api/users/login", data);

        if (jsonDoc != null && jsonDoc.RootElement.TryGetProperty("Token", out var tokenElement))
        {
            authToken = tokenElement.GetString();
            if (jsonDoc.RootElement.TryGetProperty("UserId", out var userIdElement))
            {
                currentUserId = userIdElement.GetInt32();
            }
            Console.WriteLine("Login erfolgreich!");
        }
    }

    static async Task GetProfile()
    {
        if (!await CheckLogin()) return;
        await SendRequest(HttpMethod.Get, $"/api/users/{currentUserId}/profile");
    }

    static async Task GetMyRatings()
    {
        if (!await CheckLogin()) return;
        await SendRequest(HttpMethod.Get, $"/api/users/{currentUserId}/ratings");
    }

    static async Task GetMyFavorites()
    {
        if (!await CheckLogin()) return;
        await SendRequest(HttpMethod.Get, $"/api/users/{currentUserId}/favorites");
    }

    // --- MEDIA ---

    static async Task SearchMedia()
    {
        Console.Write("Search term (Enter for all): ");
        var term = Console.ReadLine();
        
        string queryString = "";
        if (!string.IsNullOrWhiteSpace(term))
        {
             queryString = $"?search={Uri.EscapeDataString(term)}";
        }
        
        await SendRequest(HttpMethod.Get, $"/api/media{queryString}");
    }

    static async Task CreateMedia()
    {
        if (!await CheckLogin()) return;

        Console.Write("Title: ");
        var title = Console.ReadLine();
        Console.Write("Description: ");
        var description = Console.ReadLine();
        
        Console.WriteLine("Type (0=Movie, 1=Series, 2=Game): ");
        int type = int.Parse(Console.ReadLine() ?? "0");
        
        Console.Write("Release Year: ");
        int year = int.Parse(Console.ReadLine() ?? "2024");

        Console.Write("Age Restriction: ");
        int age = int.Parse(Console.ReadLine() ?? "0");

        var data = new 
        { 
            title, 
            description, 
            type, 
            releaseYear = year, 
            ageRestriction = age,
            genres = new[] { "Action", "Demo" } // Simplified for CLI
        };
        
        await SendJsonRequest(HttpMethod.Post, "/api/media", data);
    }

    // --- INTERACTIONS ---

    static async Task RateMedia()
    {
        if (!await CheckLogin()) return;

        Console.Write("Media ID: ");
        var mediaId = Console.ReadLine();

        Console.Write("Stars (1-5): ");
        int stars = int.Parse(Console.ReadLine() ?? "5");

        Console.Write("Comment: ");
        var comment = Console.ReadLine();

        var data = new { stars, comment };
        await SendJsonRequest(HttpMethod.Post, $"/api/media/{mediaId}/ratings", data);
    }

    static async Task FavoriteMedia()
    {
        if (!await CheckLogin()) return;
        Console.Write("Media ID: ");
        var mediaId = Console.ReadLine();
        await SendRequest(HttpMethod.Post, $"/api/media/{mediaId}/favorite");
    }

    static async Task LikeRating()
    {
        if (!await CheckLogin()) return;
        Console.Write("Rating ID: ");
        var ratingId = Console.ReadLine();
        await SendRequest(HttpMethod.Post, $"/api/ratings/{ratingId}/like");
    }

    static async Task ConfirmComment()
    {
        if (!await CheckLogin()) return;
        Console.Write("Rating ID to confirm: ");
        var ratingId = Console.ReadLine();
        await SendRequest(HttpMethod.Post, $"/api/ratings/{ratingId}/confirm");
    }

    // --- DISCOVERY ---

    static async Task GetLeaderboard()
    {
        await SendRequest(HttpMethod.Get, "/api/leaderboard");
    }

    static async Task GetRecommendations()
    {
        if (!await CheckLogin()) return;
        await SendRequest(HttpMethod.Get, $"/api/users/{currentUserId}/recommendations");
    }

    // --- HELPERS ---

    static async Task SendRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        AuthorizeRequest(request);
        
        var response = await client.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();
        
        Console.WriteLine($"\nStatus: {response.StatusCode}");
        Console.WriteLine($"Response: {FormatJson(result)}");
    }

    static async Task<JsonDocument?> SendJsonRequest(HttpMethod method, string url, object data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(method, url);
        request.Content = content;
        AuthorizeRequest(request);

        var response = await client.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"\nStatus: {response.StatusCode}");
        var formatted = FormatJson(result);
        Console.WriteLine($"Response: {formatted}");

        if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(result))
        {
            try { return JsonDocument.Parse(result); } catch { return null; }
        }
        return null;
    }

    static string FormatJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return json;
        }
    }
}
