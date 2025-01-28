using Microsoft.AspNetCore.Mvc;
using MachineAPI;
using MongoDB.Driver;
using MongoDB.Bson;

using MongoDB.Bson.Serialization;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.X86;

namespace MachineAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly ILogger<RequestController> _logger;

        private IMongoDatabase _database;

        public RequestController(ILogger<RequestController> logger, IMongoDatabase database)
        {
            _logger = logger;
            _database = database;
        }

        private IMongoCollection<BsonDocument> GetMongoCollection()
        {
            return _database.GetCollection<BsonDocument>(MongoAutentificationData.dataBaseCollectionToken);
        }

        [HttpPut("application/AppendAgregate/")]
        public async Task<ActionResult<string>> AppendToDataBase([FromForm] Machine machine)
        {
            var collection = GetMongoCollection();

            string logError = $"The item is already in the database";
            string logSucces = $"The item \'{machine.Name}\' append to the database";

            var filter = Builders<BsonDocument>.Filter.Eq("Name", machine.Name);
            var filter2 = Builders<BsonDocument>.Filter.Eq("Id", machine.Id);

            using (var duplicates = await collection.FindAsync(filter))
            {
                bool isContainsName = (await duplicates.ToListAsync()).Any();

                if (isContainsName)
                {
                    _logger.LogError(logError);

                    return logError;
                }

                try
                {
                    await collection.InsertOneAsync(machine.ToBsonDocument());

                    _logger.LogDebug(logSucces);

                    return logSucces;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);

                    return ex.Message;
                }
            }
        }

        [HttpGet("application/GetAgregateList/")]
        public async Task<ActionResult<string>> GetAgregateList()
        {
            var collection = GetMongoCollection();

            using (var machineListBson = await collection.FindAsync("{}"))
            {
                return (await machineListBson.ToListAsync()).ToJson();
            }
        }

        [HttpPost("application/RenameAgregat/")]
        public async Task<ActionResult<string>> Rename(string sourceName, string newName)
        {
            var succesLog = $"The name \'{sourceName}\' has been replaced by \'{sourceName}\'";
            var errorLog = $"Name {sourceName} was not found in the database";

            var collection = GetMongoCollection();

            var filter = Builders<BsonDocument>.Filter.Eq("Name", sourceName);

            var updateDefinition = Builders<BsonDocument>.Update.Set("Name", newName);

            var result = await collection.UpdateOneAsync(filter, updateDefinition);

            if (result == null)
            {
                _logger.LogError(errorLog);

                return errorLog;
            }

            _logger.LogDebug(succesLog);

            return succesLog;
        }

        [HttpGet("application/GetAgregateStateByName/")]
        public async Task<ActionResult<string>> GetAgregateStateByName(string Name)
        {
            string logError = $"Name \'{Name}\' was not found in the database";

            var filter = new BsonDocument { { "Name", new BsonDocument("$eq", $"{Name}") } };

            var collection = GetMongoCollection();

            using (var bsonMachine = await collection.FindAsync(filter))
            {
                var machineList = bsonMachine.ToList();

                if (machineList.Count < 1)
                {
                    _logger.LogError(logError);
                    return logError;
                }

                var mashine = machineList.First();

                if (mashine == null)
                {
                    _logger.LogError(logError);
                    return logError;
                }

                EState state = new EState();

                state = BsonSerializer.Deserialize<Machine>(mashine).State;

                var result = new BsonDocument("State", state).ToJson();

                _logger.LogDebug(result);

                return result;
            }
        }
    }
}