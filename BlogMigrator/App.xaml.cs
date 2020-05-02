namespace BlogMigrator
{
    using System.Collections.Generic;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool BatchMode;
        public static BlogSource destBlog = new BlogSource();
        public static List<LogData> itemsToRewrite = new List<LogData>();
        public static BlogSource rewriteBlog = new BlogSource();
        public static string rewriteMessage;
        public static bool rewritePosts;
        public static BlogSource sourceBlog = new BlogSource();
    }
}
