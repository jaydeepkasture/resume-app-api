using MongoDB.Driver;

namespace ResumeInOneMinute.Domain.Interface;

/// <summary>
/// Interface for MongoDB database service
/// </summary>
public interface IMongoDbService
{
    /// <summary>
    /// Gets a MongoDB collection by name
    /// </summary>
    /// <typeparam name="T">The document type</typeparam>
    /// <param name="collectionName">Name of the collection</param>
    /// <returns>MongoDB collection instance</returns>
    IMongoCollection<T> GetCollection<T>(string collectionName);
    
    /// <summary>
    /// Gets the MongoDB database instance
    /// </summary>
    /// <returns>MongoDB database instance</returns>
    IMongoDatabase GetDatabase();
}
