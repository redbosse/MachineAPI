using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MachineAPI
{
    public class Machine
    {
        [BsonElement("Id")]
        public string Id { get; set; }

        public string Name { get; set; }

        public EState State { get; set; }
    }
}