using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver.Core.Configuration;

namespace DAL
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(string connectionString, string dbname)
        {
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.Credential = MongoCredential.CreateCredential("admin","swenlly152", "0LYyHg8NpDLxK2AV");
            var client = new MongoClient(settings);
            _database = client.GetDatabase(dbname);

            CreateCollection();
            Console.WriteLine("connected to mongoDB!!");


        }
        public void CreateCollection()
        { //יירקתי כי כבר התחבר ונוספו הטבלאות..שלא יוסיף בכל הרצה נתונים שכבר קיימים
            ////usercollection
            //var userCollection = _database.GetCollection<BsonDocument>("Users");
            //var userDoc = new BsonDocument
            //{
            //    {"UserId","1415" },
            //    {"UaerName","Lali noy" },
            //    {"email","demo@gmail.com" },
            //    {"phone","0548557896" }

            //};
            //userCollection.InsertOne(userDoc);
            ////sitecollections
            //var websitesCollection = _database.GetCollection<BsonDocument>("websites");
            //var websitedoc = new BsonDocument
            //{
            //    { "SiteId", "2221" },
            //    { "name", "mongoDB" },
            //    { "baseAddress", "https://www.mongodb.com/" }
            //};
            //websitesCollection.InsertOne(websitedoc);
            ////passwordcollection
            //var passworsCollection = _database.GetCollection<BsonDocument>("Passwords");
            //var PasswordDoc = new BsonDocument
            //{
            //    {"PasswordId","3548" },
            //    {"UserId" , "1414" },
            //    {"SiteId","2221" },
            //    {"DateReg","27/03/25" },
            //    {"LastDateUse","27/03/25" }
            //};
            //passworsCollection.InsertOne(PasswordDoc);

        }

       
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
        public async Task AddDocumentAsync<T>(string collectionName, T document)
        {
            var collection = GetCollection<T>(collectionName);
            await collection.InsertOneAsync(document);
        }

        public async Task<T> GetDocumentByIdAsync<T>(string collectionName, string id)
        {
            var collection = GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task UpdateDocumentAsync<T>(string collectionName, string id, T document)
        {
            var collection = GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);
            await collection.ReplaceOneAsync(filter, document);
        }

        public async Task DeleteDocumentAsync<T>(string collectionName, string id)
        {
            var collection = GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);
            await collection.DeleteOneAsync(filter);
        }

    }
}
