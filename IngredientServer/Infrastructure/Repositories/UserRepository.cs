using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Core.Services;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class UserRepository(ApplicationDbContext context, IUserContextService userContextService)
    : BaseRepository<User>(context, userContextService), IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await Context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await Context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> ExistsAsync(string username, string email)
    {
        return await Context.Users
            .AnyAsync(u => u.Username.ToLower() == username.ToLower() || 
                           u.Email.ToLower() == email.ToLower());
    }
    
    public async Task<User> AddForRegistrationAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        user.IsActive = true;
        
        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        return user;
    }
    
    public async Task UpdateForLoginAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        Context.Users.Update(user);
        await Context.SaveChangesAsync();
    }
    
    public async Task<User?> GetByIdAsync(int id)
    {
        var user = await Context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
        Console.WriteLine($"GetByIdAsync - UserId: {id}, User: {(user == null ? "null" : user.Id)}");
        return user;
    }
}