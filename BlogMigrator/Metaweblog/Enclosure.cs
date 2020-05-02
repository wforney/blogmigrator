namespace CookComputing.MetaWeblog
{
    using CookComputing.XmlRpc;

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct Enclosure
    {
        public int length;
        public string type;
        public string url;
    }
}
