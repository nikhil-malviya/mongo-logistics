using MongoDB.Bson;
using MongoDB.Driver;

namespace Logistics.DataAccess.MongoDB;

public static class Query
{
	public static async Task<IEnumerable<BsonDocument>> FindAsync(IMongoCollection<BsonDocument> collection, FilterDefinition<BsonDocument> filter, ProjectionDefinition<BsonDocument> projection = default, int limit = default, CancellationToken cancellationToken = default)
	{
		var options = new FindOptions<BsonDocument, BsonDocument>()
		{
			Limit = limit,
			Projection = projection,
		};

		return (await collection.FindAsync<BsonDocument>(filter, options, cancellationToken)).ToEnumerable(CancellationToken.None);
	}

	public static async Task<BsonDocument?> FindOneAsync(IMongoCollection<BsonDocument> collection, FilterDefinition<BsonDocument> filter, ProjectionDefinition<BsonDocument> projection = default, CancellationToken cancellationToken = default)
	{
		var options = new FindOptions<BsonDocument, BsonDocument>()
		{
			Limit = 1,
			Projection = projection,
		};

		return (await collection.FindAsync(filter, options, cancellationToken)).FirstOrDefault(CancellationToken.None);
	}

	public static async Task<IEnumerable<BsonDocument>> GetAllAsync(IMongoCollection<BsonDocument> collection, ProjectionDefinition<BsonDocument> projection = default, CancellationToken cancellationToken = default)
	{
		return await FindAsync(collection, Builders<BsonDocument>.Filter.Empty, projection, default, cancellationToken);
	}

	public static async Task<bool> DocumentExistsAsync(IMongoCollection<BsonDocument> collection, FilterDefinition<BsonDocument> filter, CancellationToken cancellationToken = default)
	{
		return await collection.Find(filter).Limit(1).CountDocumentsAsync(cancellationToken) == 1;
	}
}
