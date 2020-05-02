namespace BlogMigrator
{
    using CookComputing.MetaWeblog;

    using System;

    /// <summary>
    /// The PostData class is a sortable version of the Post class returned by the XML-RPC service.
    /// An additional boolean field is added to indicate if the post has been selected to migration.
    /// </summary>
    public class PostData
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public PostData()
        {
        }

        /// <summary>
        /// Constructor that copies the members from MetaWeblog post object.
        /// </summary>
        /// <param name="post"></param>
        public PostData(Post post)
        {
            this.dateCreated = post.dateCreated;
            this.description = post.description;
            this.title = post.title;
            this.categories = post.categories;
            this.enclosure = post.enclosure;
            this.link = post.link;
            this.permalink = post.permalink;
            this.postid = Convert.ToInt32(post.postid);
            this.source = post.source;
            this.userid = post.userid;
            this.mt_allow_comments = post.mt_allow_comments;
            this.mt_allow_pings = post.mt_allow_pings;
            this.mt_convert_breaks = post.mt_convert_breaks;
            this.mt_text_more = post.mt_text_more;
            this.mt_excerpt = post.mt_excerpt;
        }

        /// <summary>
        /// Constructor that copies the members from BlogML post object.
        /// </summary>
        /// <param name="post"></param>
        public PostData(BlogML.postType post)
        {
            //dateCreated = post.datecreated;
            //description = post.content.Value;
            this.title = string.Join(" ", post.title.Text);
            //categories = post.categories;
            //enclosure = post.enclosure;
            //link = post.posturl;
            //permalink = "";
            this.postid = Convert.ToInt32(post.id);
            //source = new Source();
            //userid = post.authors.author.ToString();
            //mt_allow_comments = 1;
            //mt_allow_pings = 1;
            //mt_convert_breaks = 0;
            //mt_text_more = "";
            //if (post.excerpt.Value.Length > 0)
            //{
            //    mt_excerpt = String.Join(" ", post.excerpt.Value);
            //}
        }

        public string[] categories { get; set; }
        public DateTime dateCreated { get; set; }
        public string description { get; set; }
        public Enclosure enclosure { get; set; }
        public bool isSelected { get; set; }
        public string link { get; set; }
        public object mt_allow_comments { get; set; }
        public object mt_allow_pings { get; set; }
        public object mt_convert_breaks { get; set; }
        public string mt_excerpt { get; set; }
        public string mt_text_more { get; set; }
        public string permalink { get; set; }
        public int postid { get; set; }
        public Source source { get; set; }
        public string title { get; set; }
        public string userid { get; set; }
    }
}
