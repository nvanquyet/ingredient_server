namespace IngredientServer.Core.Interfaces.Services;

public interface IUserContextService
{
    int GetAuthenticatedUserId(); // Đổi tên cho rõ ràng
    string GetAuthenticatedUsername();
    string GetAuthenticatedUserEmail();
    bool IsAuthenticated();
    bool TryGetAuthenticatedUserId(out int userId); // Safe method
}