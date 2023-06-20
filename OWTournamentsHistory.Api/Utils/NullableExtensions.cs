namespace OWTournamentsHistory.Api.Utils
{
    public static class NullableExtensions
    {
        //public static T? ParseTo<T>(string value)
        //{
        //    if (string.IsNullOrEmpty(value))
        //    {
        //        return default;
        //    }

        //    var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        //    var method = targetType?.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string) }, null);
        //    return method == null ? throw new NotSupportedException() : (T?)method.Invoke(null, new[] { value });
        //}

        public static T? ParseTo<T>(this string value)
          where T : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
