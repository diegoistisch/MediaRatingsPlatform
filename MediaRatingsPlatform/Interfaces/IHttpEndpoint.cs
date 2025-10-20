using MediaRatingsPlatform.Helpers;

namespace MediaRatingsPlatform.Interfaces;

public interface IHttpEndpoint
{
    void RegisterRoutes(Router router);
}
