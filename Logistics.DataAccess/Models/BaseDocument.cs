using Logistics.DataAccess.Constants;
using Logistics.DataAccess.Utilities;
using MongoDB.Bson;

namespace Logistics.DataAccess.Models;

/// <summary>
/// BaseDocument for BsonDocument helper classes.
/// </summary>
public class BaseDocument
{
	public BsonDocument Document { get; }

	public DateTime? CreatedAt
	{
		get => Document.GetValueAsDateTime(Base.CreatedAt);
		set => Document.Set(Base.CreatedAt, value);
	}

	public DateTime? UpdatedAt
	{
		get => Document.GetValueAsDateTime(Base.UpdatedAt);
		set => Document.Set(Base.UpdatedAt, value);
	}

	#region Metadata

	public string SchemaType
	{
		get => Document.GetValueAsString(string.Join('.', Base.Metadata, Base.SchemaType));
	}

	public string SchemaVersion
	{
		get => Document.GetValueAsString(string.Join('.', Base.Metadata, Base.SchemaVersion));
	}

	#endregion Metadata

	public BsonValue Get(string field)
	{
		return Document.GetValue(field);
	}

	public void Set(string field, BsonValue value)
	{
		Document.Set(field, value);
	}

	public BaseDocument(BsonDocument document, string schemaType, string schemaVersion)
	{
		Document = document;

		if (!Document.Contains(Base.Metadata))
		{
			Document.Set(Base.Metadata, new BsonDocument()
			{
				{Base.SchemaType, schemaType},
				{Base.SchemaVersion, schemaVersion}
			});
		}
	}
}