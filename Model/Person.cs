using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MachineAPI.Model
{
    public class Person
    {
        public ObjectId Id { get; set; }
        public string Username { get; set; }

        public string Password { get; set; }
        public Role Role { get; set; }
    }

    public enum Role
    {
        defaultUser,
        Admin
    }
}