using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MachineAPI.Model
{
    public class Machine
    {
        [BsonElement("Id")]
        public string Id { get; set; }

        public string Name { get; set; }

        public EState State { get; set; }
    }
}