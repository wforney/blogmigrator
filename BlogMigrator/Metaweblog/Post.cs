namespace CookComputing.MetaWeblog
{
    using BlogMigrator.Helpers;

    using CookComputing.XmlRpc;

    using System;
    using System.Text;

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct Post
    {
        public string[] categories;

        [XmlRpcMissingMapping(MappingAction.Error)]
        [XmlRpcMember(Description = "Required when posting.")]
        public DateTime dateCreated;

        [XmlRpcMissingMapping(MappingAction.Error)]
        [XmlRpcMember(Description = "Required when posting.")]
        public string description;

        public Enclosure enclosure;

        public string link;

        public object mt_allow_comments;

        public object mt_allow_pings;

        public object mt_convert_breaks;

        public string mt_excerpt;

        public string mt_text_more;

        public string permalink;

        [XmlRpcMember(
            Description = "Not required when posting. Depending on server may be either string or integer. Use Convert.ToInt32(postid) to treat as integer or Convert.ToString(postid) to treat as string")]
        public object postid;

        public Source source;

        [XmlRpcMissingMapping(MappingAction.Error)]
        [XmlRpcMember(Description = "Required when posting.")]
        public string title;

        public string userid;

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());

            sb.Append($" dateCreated={this.dateCreated}");
            sb.Append($" userid={this.userid}");
            sb.Append($" title={this.title}");
            sb.AppendLine($" description={this.description.Truncate(200)}");
            sb.AppendLine($" categories={string.Join(",", this.categories)}");

            return sb.ToString();
        }
    }
}
