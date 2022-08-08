using Logistics.DataAccess.Constants;
using Logistics.DataAccess.Utilities;
using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Logistics.DataAccess.Models;

/// <summary>
/// BsonDocument helper class for 'plane' document.
/// Provides 'Get' method for flexible schemaless retrieval of value from a BsonDocument.
/// Provides 'Set' method for flexible schemaless assignment of value to a BsonDocument.
/// Has 'get' accessor method for type safe retrieval of value from a BsonDocument.
/// Has 'set' accessor method for type safe assignment of value to a BsonDocument.
/// Doesn't have any backing fields only manipulates underlying BsonDocument.
/// </summary>
public class PlaneDocument : BaseDocument
{
	public string Id
	{
		get => Document.GetValueAsString(Plane.Id);
		set => Document.Set(Plane.Id, value);
	}

	public GeoJson2DGeographicCoordinates CurrentLocation
	{
		get => Document.GetValueAsCoordinates(Plane.CurrentLocation);
		set => Document.Set(Plane.CurrentLocation, value.ToBsonArray());
	}

	public double Heading
	{
		get => Document.GetValueAsDouble(Plane.Heading);
		set => Document.Set(Plane.Heading, value);
	}

	public string[] Route
	{
		get => Document.GetValueAsStringArray(Plane.Route);
		set => Document.Set(Plane.Route, value.ToBsonArray());
	}

	public string Departed
	{
		get => Document.GetValueAsString(Plane.Departed);
		set => Document.Set(Plane.Departed, value);
	}

	public string Landed
	{
		get => Document.GetValueAsString(Plane.Landed);
		set => Document.Set(Plane.Landed, value);
	}

	public DateTime? LandedOn
	{
		get => Document.GetValueAsDateTime(Plane.LandedOn);
		set => Document.Set(Plane.LandedOn, value);
	}

	#region Statistics

	public double TotalDistanceTravelledInMiles
	{
		get => Document.GetValueAsDouble(Plane.TotalDistanceTravelledInMiles);
		set => Document.Set(Plane.TotalDistanceTravelledInMiles, value);
	}

	public double DistanceTravelledSinceLastMaintenanceInMiles
	{
		get => Document.GetValueAsDouble(Plane.DistanceTravelledSinceLastMaintenanceInMiles);
		set => Document.Set(Plane.DistanceTravelledSinceLastMaintenanceInMiles, value);
	}

	public bool MaintenanceRequired
	{
		get => Document.GetValueAsBoolean(Plane.MaintenanceRequired);
		set => Document.Set(Plane.MaintenanceRequired, value);
	}

	public double AirtimeInMinutes
	{
		get => Document.GetValueAsDouble(Plane.AirtimeInMinutes);
		set => Document.Set(Plane.AirtimeInMinutes, value);
	}

	#endregion Statistics

	public DateTime? CreatedAt
	{
		get => Document.GetValueAsDateTime(Plane.CreatedAt);
		set => Document.Set(Plane.CreatedAt, value);
	}

	public DateTime? UpdatedAt
	{
		get => Document.GetValueAsDateTime(Plane.UpdatedAt);
		set => Document.Set(Plane.UpdatedAt, value);
	}

	#region Metadata

	public string SchemaType
	{
		get => Document.GetValueAsString(Plane.SchemaType);
		set => Document.Set(Plane.SchemaType, value);
	}

	public string SchemaVersion
	{
		get => Document.GetValueAsString(Plane.SchemaVersion);
		set => Document.Set(Plane.SchemaVersion, value);
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

	public PlaneDocument(BsonDocument document) : base(document, "plane", "1.0")
	{
		_document = document;
	}
}
