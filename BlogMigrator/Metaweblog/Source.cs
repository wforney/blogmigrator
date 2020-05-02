namespace CookComputing.MetaWeblog
{
    using CookComputing.XmlRpc;

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct Source
    {
        public string name;
        public string url;
    }
}
