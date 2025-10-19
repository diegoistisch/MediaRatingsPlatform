namespace MediaRatingsPlatform;

public interface IUserRepository
{
    User? GetByUsername(string username);
    User? GetById(int id);
    void Add(User user);
    List<User> GetAll();
}