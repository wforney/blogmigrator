namespace BlogMigrator
{
    public class LogData
    {
        public LogData()
        {
        }

        public LogData(int SourceId, string SourceUrl, int DestId, string DestUrl)
        {
            this.sourceId = SourceId;
            this.sourceUrl = SourceUrl;
            this.destinationId = DestId;
            this.destinationUrl = DestUrl;
        }

        public int destinationId { get; set; }
        public string destinationUrl { get; set; }
        public int sourceId { get; set; }
        public string sourceUrl { get; set; }
    }
}
