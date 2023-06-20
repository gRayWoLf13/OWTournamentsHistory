using System.Globalization;

namespace OWTournamentsHistory.Common.Utils
{
    public static class NullableExtensions
    {
        public static T? ParseTo<T>(this string value)
          where T : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }
            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }
    }
}
