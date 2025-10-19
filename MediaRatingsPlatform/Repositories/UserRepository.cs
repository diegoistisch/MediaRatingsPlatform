using MediaRatingsPlatform.Interfaces;
using MediaRatingsPlatform.Models;

namespace MediaRatingsPlatform.Repositories;

public class UserRepository : IUserRepository
{
    private readonly List<User> users = new List<User>();
    private int nextId = 1;

    public User? GetByUsername(string username)
    {
        for (int i = 0; i < users.Count; i++)
        {
            if (users[i].Username == username)
            {
                return users[i];
            }
        }
        return null;
    }

    public User? GetById(int id)
    {
        for (int i = 0; i < users.Count; i++)
        {
            if (users[i].Id == id)
            {
                return users[i];
            }
        }
        return null;
    }

    public void Add(User user)
    {
        user.Id = nextId++;
        user.CreatedAt = DateTime.Now;
        user.UpdatedAt = DateTime.Now;
        users.Add(user);
    }

    public List<User> GetAll()
    {
        return users;
    }
}