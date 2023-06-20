namespace OWTournamentsHistory.Common.Utils
{
    public static class NameExtensions
    {
        public static string GetName(string battleTag) =>
            battleTag.Contains('#') ? battleTag[0..battleTag.IndexOf("#")] : battleTag;

        public static bool EqualsIgnoreCase(string first, string second) =>
            string.Equals(first, second, StringComparison.OrdinalIgnoreCase);

    }
}
