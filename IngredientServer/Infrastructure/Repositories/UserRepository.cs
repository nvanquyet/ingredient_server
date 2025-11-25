using IngredientServer.Core.Entities;
using IngredientServer.Core.Interfaces.Repositories;
using IngredientServer.Core.Interfaces.Services;
using IngredientServer.Core.Services;
using IngredientServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Repositories;

public class UserRepository(ApplicationDbContext context, IUserContextService userContextService, ITimeService timeService)
    : BaseRepository<User>(context, userContextService, timeService), IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await Context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }
    public async Task<User?> ValidateTokenAsync()
    {
        var userId = userContextService.GetAuthenticatedUserId();
        if (userId <= 0)
        {
            return null; // Invalid user ID
        }
        // Fetch the user by ID to validate the token
        return await Context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
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
        user.CreatedAt = TimeService.UtcNow;
        user.UpdatedAt = TimeService.UtcNow;
        user.IsActive = true;
        
        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        return user;
    }
    
    public async Task UpdateForLoginAsync(User user)
    {
        user.UpdatedAt = TimeService.UtcNow;
        Context.Users.Update(user);
        await Context.SaveChangesAsync();
    }
    
    public override async Task<User?> GetByIdAsync(int id)
    {
        // Use the base implementation which already filters by UserId
        return await base.GetByIdAsync(id);
    }
    
    public async Task<User?> GetByIdWithoutAuthAsync(int id)
    {
        return await Context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}