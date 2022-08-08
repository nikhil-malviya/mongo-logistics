using MongoDB.Bson;

namespace Logistics.DataAccess.Models;

/// <summary>
/// BaseDocument for BsonDocument helper classes.
/// </summary>
public class BaseDocument
{
	private const string SchemaType = "schemaType";
	private const string SchemaVersion = "schemaVersion";
	private const string Metadata = "metadata";

	public BaseDocument(BsonDocument document, string schemaType, string schemaVersion)
	{
		if (!document.Contains(Metadata))
		{
			document.Set(Metadata, new BsonDocument()
			{
				{SchemaType, schemaType},
				{SchemaVersion, schemaVersion}
			});
		}
	}
}