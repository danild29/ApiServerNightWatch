
namespace DataAccess.Extensions;

public static partial class EnumerbleExt
{
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
    {
        return source ?? Enumerable.Empty<T>();
    }
}