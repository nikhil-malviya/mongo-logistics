using MongoDB.Bson;
using MongoDB.Driver;

namespace Logistics.DataAccess.MongoDB;

public static class Command
{
	private const string UpdatedAt = "updatedAt";
	private const string CreatedAt = "createdAt";

	public static async Task<UpdateResult> UpdateOneAsync(IMongoCollection<BsonDocument> collection, FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update, CancellationToken cancellationToken = default)
	{
		update.CurrentDate(UpdatedAt);
		return await collection.UpdateOneAsync(filter, update, default, cancellationToken);
	}

	public static async Task<BsonDocument> FindOneAndUpdateAsync(IMongoCollection<BsonDocument> collection, FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update, CancellationToken cancellationToken = default)
	{
		update.CurrentDate(UpdatedAt);
		return await collection.FindOneAndUpdateAsync(filter, update, default, cancellationToken);
	}

	public static async Task InsertOneAsync(IMongoCollection<BsonDocument> collection, BsonDocument document, CancellationToken cancellationToken = default)
	{
		document.Set(CreatedAt, DateTime.UtcNow);
		await collection.InsertOneAsync(document, default, cancellationToken);
	}
}
