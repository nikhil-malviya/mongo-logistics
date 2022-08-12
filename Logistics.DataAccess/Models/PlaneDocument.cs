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

	public PlaneDocument(BsonDocument document) : base(document, "plane", "1.0") { }
}
