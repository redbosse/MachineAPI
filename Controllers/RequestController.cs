using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Serilog;
using Microsoft.AspNetCore.Authorization;
using MachineAPI.Model;

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
                    Log.Error(logError);

                    return logError;
                }

                try
                {
                    await collection.InsertOneAsync(machine.ToBsonDocument());

                    Log.Information(logSucces);

                    return logSucces;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);

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
                var agregates = (await machineListBson.ToListAsync()).ToJson();

                Log.Information($"Get Agregates {agregates}");

                return agregates;
            }
        }

        [HttpPost("application/RenameAgregat/")]
        public async Task<ActionResult<string>> Rename(string sourceName, string newName)
        {
            var succesLog = $"The name \'{sourceName}\' has been replaced by \'{newName}\'";
            var errorLog = $"Name '{sourceName}' was not found in the database";

            var collection = GetMongoCollection();

            var filter = Builders<BsonDocument>.Filter.Eq("Name", sourceName);
            var filterForNew = Builders<BsonDocument>.Filter.Eq("Name", newName);

            var updateDefinition = Builders<BsonDocument>.Update.Set("Name", newName);

            var check = (await (await collection.FindAsync(filterForNew)).ToListAsync()).Any();

            if (check)
            {
                Log.Error($"Name '{newName}' was found in the database");

                return $"Name '{newName}' was found in the database";
            }

            var result = await collection.UpdateOneAsync(filter, updateDefinition);

            if (result == null)
            {
                Log.Error(errorLog);

                return errorLog;
            }

            Log.Information(succesLog);

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
                    Log.Error(logError);

                    return logError;
                }

                var mashine = machineList.First();

                if (mashine == null)
                {
                    Log.Error(logError);

                    return logError;
                }

                EState state = new EState();

                state = BsonSerializer.Deserialize<Machine>(mashine).State;

                var result = new BsonDocument("State", state).ToJson();

                Log.Information(result);

                return result;
            }
        }

        [HttpPost("application/ChangeStateByName/")]
        public async Task<ActionResult<string>> ChangeState(string name, EState state)
        {
            var succesLog = $"The state by name \'{name}\' has been replaced to \'{state}\'";
            var errorLog = $"Name \'{name}\' was not found in the database";

            var collection = GetMongoCollection();

            var filter = Builders<BsonDocument>.Filter.Eq("Name", name);

            var updateDefinition = Builders<BsonDocument>.Update.Set("State", state);

            var result = await collection.UpdateOneAsync(filter, updateDefinition);

            if (result == null)
            {
                Log.Error(errorLog);

                return errorLog;
            }

            Log.Information(succesLog);

            return succesLog;
        }
    }
}