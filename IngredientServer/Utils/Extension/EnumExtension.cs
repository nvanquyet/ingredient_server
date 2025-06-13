namespace IngredientServer.Utils.Extension;

public class EnumExtension
{
    public static T ConvertStringToMealType<T>(string value) where T : struct, Enum
    {
        if (!Enum.TryParse<T>(value, true, out var result))
        {
            throw new ArgumentException($"Invalid enum value: {value}");
        }
        return result;
    }
}