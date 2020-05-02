namespace BlogMigrator
{
    /// <summary>
    /// This class represents a post comment.
    /// </summary>
    internal class Comment
    {
        public string author { get; set; }
        public string email { get; set; }
        public string publishDate { get; set; }
        public string text { get; set; }
        public string url { get; set; }
    }
}
