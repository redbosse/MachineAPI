using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Serilog;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;

namespace MachineAPI.Model
{
    public class PersonDataBase : IPersonDataBase
    {
        private IMongoDatabase _database;

        private readonly string userCollectionToken = "MachineApi_Users";

        public PersonDataBase(IMongoDatabase dataBase)
        {
            _database = dataBase;
        }

        private IMongoCollection<BsonDocument> GetMongoCollection()
        {
            return _database.GetCollection<BsonDocument>(userCollectionToken);
        }

        public bool ValidateUser(Person user)
        {
            var collection = GetMongoCollection();

            var filter = new BsonDocument { { "Username", new BsonDocument("$eq", $"{user.Username}") } };

            try
            {
                var userList = collection.Find(filter).ToList();

                if (userList.Count == 0)
                {
                    Log.Error($"User {user.Username} does not exist");

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);

                return false;
            }
        }

        public bool ValidateAdminUser(Person user)
        {
            var collection = GetMongoCollection();

            var filter = new BsonDocument { { "Username", new BsonDocument("$eq", $"{user.Username}") } };

            try
            {
                var userList = collection.Find(filter).ToList();

                if (!userList.Any())
                {
                    Log.Error("User does not exist");

                    return false;
                }

                var person = BsonSerializer.Deserialize<Person>(userList.FirstOrDefault());

                return person.Role == Role.Admin;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);

                return false;
            }
        }

        public bool AddNewUser(Person user)
        {
            var collection = GetMongoCollection();

            var filter = new BsonDocument { { "Username", new BsonDocument("$eq", $"{user.Username}") } };

            try
            {
                var userList = collection.Find(filter).ToList();

                if (userList.Any())
                {
                    Log.Error("There is already a user with that name");

                    return false;
                }

                collection.InsertOne(user.ToBsonDocument());

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);

                return false;
            }
        }

        public bool ValidatePassword(Person user)
        {
            var collection = GetMongoCollection();

            var filter = new BsonDocument { { "Username", new BsonDocument("$eq", $"{user.Username}") } };

            try
            {
                var userList = collection.Find(filter).ToList();

                if (userList.Count == 0)
                {
                    Log.Error($"User {user.Username} does not exist");

                    return false;
                }

                var person = BsonSerializer.Deserialize<Person>(userList.FirstOrDefault());

                Log.Information("Check authorization" + person.ToJson());

                return user.Password == person.Password;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);

                return false;
            }
        }

        public string GetUserlist()
        {
            var collection = GetMongoCollection();

            var persons = collection.Find("{}");

            var personJson = persons.ToList().ToJson();

            Log.Information($"Get Users {personJson}");

            return personJson;
        }
    }
}