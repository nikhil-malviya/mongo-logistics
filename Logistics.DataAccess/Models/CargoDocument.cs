using Logistics.DataAccess.Constants;
using Logistics.DataAccess.Utilities;
using MongoDB.Bson;

namespace Logistics.DataAccess.Models;

/// <summary>
/// BsonDocument helper class for 'cargo' document.
/// Provides 'Get' method for flexible schemaless retrieval of value from a BsonDocument.
/// Provides 'Set' method for flexible schemaless assignment of value to a BsonDocument.
/// Has 'get' accessor method for type safe retrieval of value from a BsonDocument.
/// Has 'set' accessor method for type safe assignment of value to a BsonDocument.
/// Doesn't have any backing fields only manipulates underlying BsonDocument.
/// </summary>
public class CargoDocument : BaseDocument
{
	public ObjectId Id
	{
		get => Document.GetValueAsObjectId(Cargo.Id);
		set => Document.Set(Cargo.Id, value);
	}

	public string Courier
	{
		get => Document.GetValueAsString(Cargo.Courier);
		set => Document.Set(Cargo.Courier, value);
	}

	public DateTime Received
	{
		get => Document.GetValueAsDateTime(Cargo.Received);
		set => Document.Set(Cargo.Received, value);
	}

	public string Status
	{
		get => Document.GetValueAsString(Cargo.Status);
		set => Document.Set(Cargo.Status, value);
	}

	public string Location
	{
		get => Document.GetValueAsString(Cargo.Location);
		set => Document.Set(Cargo.Location, value);
	}

	public string Destination
	{
		get => Document.GetValueAsString(Cargo.Destination);
		set => Document.Set(Cargo.Destination, value);
	}

	public CargoDocument(BsonDocument document) : base(document, "cargo", "1.0") { }

	public CargoDocument() : base("cargo", "1.0") { }
}
