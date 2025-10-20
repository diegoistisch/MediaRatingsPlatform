using System.Net;
using MediaRatingsPlatform.Helpers;
using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Endpoints;

public class MediaEndpoints : IHttpEndpoint
{
    private readonly IMediaService _mediaService;
    private readonly IUserService _userService;

    public MediaEndpoints(IMediaService mediaService, IUserService userService)
    {
        _mediaService = mediaService;
        _userService = userService;
    }

    public void RegisterRoutes(Router router)
    {
        router.AddRoute("POST", "/api/media", Create);
        router.AddRoute("GET", "/api/media", GetAll);
        router.AddRoute("GET", "/api/media/{id}", GetById);
        router.AddRoute("PUT", "/api/media/{id}", Update);
        router.AddRoute("DELETE", "/api/media/{id}", Delete);
    }

    private async Task Create(HttpListenerContext context)
    {
        try
        {
            var user = AuthorizationHelper.AuthorizeRequest(context, _userService);
            if (user == null) return;

            var request = await HttpHelper.ReadJsonBody<CreateMediaRequest>(context.Request);

            if (request == null || string.IsNullOrWhiteSpace(request.Title))
            {
                HttpHelper.SendJsonResponse(context.Response, 400, "Title is required");
                return;
            }

            var media = _mediaService.CreateMedia(
                user.Id,
                request.Title,
                request.Description ?? "",
                request.Type,
                request.ReleaseYear,
                request.Genres ?? new List<string>(),
                request.AgeRestriction
            );

            if (media == null)
            {
                HttpHelper.SendJsonResponse(context.Response, 400, "Invalid media data");
                return;
            }

            var response = new MediaResponse
            {
                Id = media.Id,
                CreatorId = media.CreatorId,
                Title = media.Title,
                Description = media.Description,
                Type = media.Type,
                ReleaseYear = media.ReleaseYear,
                Genres = media.Genres,
                AgeRestriction = media.AgeRestriction,
                AverageRating = media.AverageRating,
                CreatedAt = media.CreatedAt
            };

            HttpHelper.SendJsonResponse(context.Response, 201, response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Create media error: {ex.Message}");
            HttpHelper.SendJsonResponse(context.Response, 500, "Internal server error");
        }
    }

    private async Task GetAll(HttpListenerContext context)
    {
        try
        {
            var user = AuthorizationHelper.AuthorizeRequest(context, _userService);
            if (user == null) return;

            var media = _mediaService.GetAllMedia();
            var response = media.Select(m => new MediaResponse
            {
                Id = m.Id,
                CreatorId = m.CreatorId,
                Title = m.Title,
                Description = m.Description,
                Type = m.Type,
                ReleaseYear = m.ReleaseYear,
                Genres = m.Genres,
                AgeRestriction = m.AgeRestriction,
                AverageRating = m.AverageRating,
                CreatedAt = m.CreatedAt
            }).ToList();

            HttpHelper.SendJsonResponse(context.Response, 200, response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetAll media error: {ex.Message}");
            HttpHelper.SendJsonResponse(context.Response, 500, "Internal server error");
        }
    }

    private async Task GetById(HttpListenerContext context)
    {
        try
        {
            var user = AuthorizationHelper.AuthorizeRequest(context, _userService);
            if (user == null) return;

            var path = context.Request.Url?.AbsolutePath ?? "";
            var parameters = HttpHelper.ExtractPathParameters("/api/media/{id}", path);

            if (!parameters.TryGetValue("id", out var idString) || !int.TryParse(idString, out var mediaId))
            {
                HttpHelper.SendJsonResponse(context.Response, 400, "Invalid media ID");
                return;
            }

            var media = _mediaService.GetMediaById(mediaId);

            if (media == null)
            {
                HttpHelper.SendJsonResponse(context.Response, 404, "Media not found");
                return;
            }

            var response = new MediaResponse
            {
                Id = media.Id,
                CreatorId = media.CreatorId,
                Title = media.Title,
                Description = media.Description,
                Type = media.Type,
                ReleaseYear = media.ReleaseYear,
                Genres = media.Genres,
                AgeRestriction = media.AgeRestriction,
                AverageRating = media.AverageRating,
                CreatedAt = media.CreatedAt
            };

            HttpHelper.SendJsonResponse(context.Response, 200, response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetById media error: {ex.Message}");
            HttpHelper.SendJsonResponse(context.Response, 500, "Internal server error");
        }
    }

    private async Task Update(HttpListenerContext context)
    {
        try
        {
            var user = AuthorizationHelper.AuthorizeRequest(context, _userService);
            if (user == null) return;

            var path = context.Request.Url?.AbsolutePath ?? "";
            var parameters = HttpHelper.ExtractPathParameters("/api/media/{id}", path);

            if (!parameters.TryGetValue("id", out var idString) || !int.TryParse(idString, out var mediaId))
            {
                HttpHelper.SendJsonResponse(context.Response, 400, "Invalid media ID");
                return;
            }

            var request = await HttpHelper.ReadJsonBody<UpdateMediaRequest>(context.Request);

            if (request == null || string.IsNullOrWhiteSpace(request.Title))
            {
                HttpHelper.SendJsonResponse(context.Response, 400, "Title is required");
                return;
            }

            var media = _mediaService.UpdateMedia(
                mediaId,
                user.Id,
                request.Title,
                request.Description ?? "",
                request.Type,
                request.ReleaseYear,
                request.Genres ?? new List<string>(),
                request.AgeRestriction
            );

            if (media == null)
            {
                HttpHelper.SendJsonResponse(context.Response, 403, "You can only update your own media");
                return;
            }

            var response = new MediaResponse
            {
                Id = media.Id,
                CreatorId = media.CreatorId,
                Title = media.Title,
                Description = media.Description,
                Type = media.Type,
                ReleaseYear = media.ReleaseYear,
                Genres = media.Genres,
                AgeRestriction = media.AgeRestriction,
                AverageRating = media.AverageRating,
                CreatedAt = media.CreatedAt
            };

            HttpHelper.SendJsonResponse(context.Response, 200, response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update media error: {ex.Message}");
            HttpHelper.SendJsonResponse(context.Response, 500, "Internal server error");
        }
    }

    private async Task Delete(HttpListenerContext context)
    {
        try
        {
            var user = AuthorizationHelper.AuthorizeRequest(context, _userService);
            if (user == null) return;

            var path = context.Request.Url?.AbsolutePath ?? "";
            var parameters = HttpHelper.ExtractPathParameters("/api/media/{id}", path);

            if (!parameters.TryGetValue("id", out var idString) || !int.TryParse(idString, out var mediaId))
            {
                HttpHelper.SendJsonResponse(context.Response, 400, "Invalid media ID");
                return;
            }

            var success = _mediaService.DeleteMedia(mediaId, user.Id);

            if (!success)
            {
                HttpHelper.SendJsonResponse(context.Response, 403, "You can only delete your own media");
                return;
            }

            HttpHelper.SendJsonResponse(context.Response, 200, "Media deleted successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete media error: {ex.Message}");
            HttpHelper.SendJsonResponse(context.Response, 500, "Internal server error");
        }
    }
}

// DTOs
public class CreateMediaRequest
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public Enums.MediaType Type { get; set; }
    public int ReleaseYear { get; set; }
    public List<string> Genres { get; set; } = new List<string>();
    public Enums.AgeRestriction AgeRestriction { get; set; }
}

public class UpdateMediaRequest
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public Enums.MediaType Type { get; set; }
    public int ReleaseYear { get; set; }
    public List<string> Genres { get; set; } = new List<string>();
    public Enums.AgeRestriction AgeRestriction { get; set; }
}

public class MediaResponse
{
    public int Id { get; set; }
    public int CreatorId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public Enums.MediaType Type { get; set; }
    public int ReleaseYear { get; set; }
    public List<string> Genres { get; set; } = new List<string>();
    public Enums.AgeRestriction AgeRestriction { get; set; }
    public double AverageRating { get; set; }
    public DateTime CreatedAt { get; set; }
}
