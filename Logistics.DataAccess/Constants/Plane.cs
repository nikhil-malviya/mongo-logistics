namespace Logistics.DataAccess.Constants;

public static class Plane
{
	public const string Id = "_id";

	public const string CurrentLocation = "currentLocation";

	public const string Heading = "heading";

	public const string Route = "route";

	public const string Departed = "departed";

	public const string Landed = "landed";

	public const string LandedOn = "landedOn";

	#region Statistics

	public const string TotalDistanceTravelledInMiles = "statistics.totalDistanceTravelledInMiles";

	public const string DistanceTravelledSinceLastMaintenanceInMiles = "statistics.distanceTravelledSinceLastMaintenanceInMiles";

	public const string MaintenanceRequired = "statistics.maintenanceRequired";

	public const string AirtimeInMinutes = "statistics.airtimeInMinutes";

	#endregion Statistics

	public const string CreatedAt = "createdAt";

	public const string UpdatedAt = "updatedAt";

	#region Metadata

	public const string SchemaType = "metadata.schemaType";

	public const string SchemaVersion = "metadata.schemaVersion";

	#endregion Metadata
}
