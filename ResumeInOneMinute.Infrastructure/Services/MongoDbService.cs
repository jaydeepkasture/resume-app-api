using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using ResumeInOneMinute.Domain.Interface;

namespace ResumeInOneMinute.Infrastructure.Services;

/// <summary>
/// Service for managing MongoDB database connections and collections
/// </summary>
public class MongoDbService : IMongoDbService
{
    private readonly IMongoDatabase _database;
    private readonly IMongoClient _client;

    public MongoDbService(IConfiguration configuration)
    {
        var mongoConnectionString = configuration.GetConnectionString("MongoDB")
            ?? throw new InvalidOperationException("MongoDB connection string not configured");

        var mongoUrl = new MongoUrl(mongoConnectionString);
        _client = new MongoClient(mongoUrl);
        _database = _client.GetDatabase(mongoUrl.DatabaseName ?? "resume_documents_dev");
    }

    /// <summary>
    /// Gets a MongoDB collection by name
    /// </summary>
    /// <typeparam name="T">The document type</typeparam>
    /// <param name="collectionName">Name of the collection</param>
    /// <returns>MongoDB collection instance</returns>
    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
        {
            throw new ArgumentException("Collection name cannot be null or empty", nameof(collectionName));
        }

        return _database.GetCollection<T>(collectionName);
    }

    /// <summary>
    /// Gets the MongoDB database instance
    /// </summary>
    /// <returns>MongoDB database instance</returns>
    public IMongoDatabase GetDatabase()
    {
        return _database;
    }
}
