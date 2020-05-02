namespace BlogMigrator.Helpers
{
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength) =>
            string.IsNullOrEmpty(value) ? value : value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}
