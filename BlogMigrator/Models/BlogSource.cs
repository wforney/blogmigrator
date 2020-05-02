namespace BlogMigrator
{
    using CookComputing.MetaWeblog;

    using System.Collections.Generic;

    public class BlogSource
    {
        public BlogML.blogType blogData { get; set; }
        public string blogFile { get; set; }
        public string blogId { get; set; }
        public List<Post> blogPosts { get; set; } = new List<Post>();
        public string password { get; set; }
        public List<int> postsToMigrate { get; set; } = new List<int>();
        public string rootUrl { get; set; }
        public string serviceType { get; set; }
        public string serviceUrl { get; set; }
        public string username { get; set; }
    }
}
