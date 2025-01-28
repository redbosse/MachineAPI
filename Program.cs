using MongoDB.Bson;
using MongoDB.Driver;
using MachineAPI;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

var database = new MongoClient(MongoAutentificationData.Uri).GetDatabase(MongoAutentificationData.dataBaseToken);

var targetCollection = database.GetCollection<BsonDocument>(MongoAutentificationData.dataBaseCollectionToken);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(database);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();