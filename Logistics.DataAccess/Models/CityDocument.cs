using Logistics.DataAccess.Constants;
using Logistics.DataAccess.Utilities;
using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Logistics.DataAccess.Models;

/// <summary>
/// BsonDocument helper class for 'city' document.
/// Provides 'Get' method for flexible schemaless retrieval of value from a BsonDocument.
/// Provides 'Set' method for flexible schemaless assignment of value to a BsonDocument.
/// Has 'get' accessor method for type safe retrieval of value from a BsonDocument.
/// Has 'set' accessor method for type safe assignment of value to a BsonDocument.
/// Doesn't have any backing fields only manipulates underlying BsonDocument.
/// </summary>
public class CityDocument : BaseDocument
{
	public string Id
	{
		get => Document.GetValueAsString(City.Id);
		set => Document.Set(City.Id, value);
	}

	public string Country
	{
		get => Document.GetValueAsString(City.Country);
		set => Document.Set(City.Country, value);
	}

	public GeoJson2DGeographicCoordinates Position
	{
		get => Document.GetValueAsCoordinates(City.Position);
		set => Document.Set(City.Position, value.ToBsonArray());
	}

	public DateTime? CreatedAt
	{
		get => Document.GetValueAsDateTime(City.CreatedAt);
		set => Document.Set(City.CreatedAt, value);
	}

	public DateTime? UpdatedAt
	{
		get => Document.GetValueAsDateTime(City.UpdatedAt);
		set => Document.Set(City.UpdatedAt, value);
	}

	#region Metadata

	public string SchemaType
	{
		get => Document.GetValueAsString(City.SchemaType);
		set => Document.Set(City.SchemaType, value);
	}

	public string SchemaVersion
	{
		get => Document.GetValueAsString(City.SchemaVersion);
		set => Document.Set(City.SchemaVersion, value);
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

	private readonly BsonDocument _document;
	public BsonDocument Document => _document;

	public CityDocument(BsonDocument document) : base(document, "city", "1.0")
	{
		_document = document;
	}
}
