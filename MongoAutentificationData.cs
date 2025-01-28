namespace MachineAPI
{
    public static class MongoAutentificationData
    {
        public static readonly string IP = "172.17.0.3";
        public static readonly string Uri = $"mongodb://{IP}/";
        public static readonly string dataBaseToken = "machines";
        public static readonly string dataBaseCollectionToken = "machine";
    }
}