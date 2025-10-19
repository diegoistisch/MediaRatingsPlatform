using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Client;

class TestClient
{
    private static readonly HttpClient client = new HttpClient();
    private static string? authToken = null;

    public static async Task Run()
    {
        client.BaseAddress = new Uri("http://localhost:8080");

        Console.WriteLine("=== Media Ratings Platform ===\n");

        while (true)
        {
            Console.WriteLine("Befehle:");
            Console.WriteLine("1 - Register");
            Console.WriteLine("2 - Login");
            Console.WriteLine("3 - Get Profile");
            Console.WriteLine("4 - Exit");
            Console.WriteLine("Wähle: ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await Register();
                        break;
                    case "2":
                        await Login();
                        break;
                    case "3":
                        await GetProfile();
                        break;
                    case "4":
                        return;
                    default:
                        Console.WriteLine("Ungültige Eingabe!");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler: {ex.Message}");
            }
        }
    }

    static async Task Register()
    {
        Console.Write("Username: ");
        var username = Console.ReadLine();

        Console.Write("Email: ");
        var email = Console.ReadLine();

        Console.Write("Password: ");
        var password = Console.ReadLine();

        var data = new { username, email, password };
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/users/register", content);
        var result = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"\nStatus: {response.StatusCode}");
        Console.WriteLine($"Response: {result}");
    }

    static async Task Login()
    {
        Console.Write("Username: ");
        var username = Console.ReadLine();

        Console.Write("Password: ");
        var password = Console.ReadLine();

        var data = new { username, password };
        var json = JsonSerializer.Serialize(data);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/users/login", content);
        var result = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine($"Response: {result}");

        if (response.IsSuccessStatusCode)
        {
            var jsonDoc = JsonDocument.Parse(result);

            if (jsonDoc.RootElement.TryGetProperty("Token", out var tokenElement))
            {
                authToken = tokenElement.GetString();
                Console.WriteLine("Login erfolgreich!");
            }
        }
    }

    static async Task GetProfile()
    {
        Console.Write("Username: ");
        var username = Console.ReadLine();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{username}/profile");

        if (!string.IsNullOrEmpty(authToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        }

        var response = await client.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine($"Response: {result}");
    }
}
