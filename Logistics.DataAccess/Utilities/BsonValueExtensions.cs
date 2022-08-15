using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Logistics.DataAccess.Utilities;

public static class BsonValueExtensions
{
	/// <summary>
	/// Get BsonValue as ObjectId
	/// </summary>
	/// <param name="document"></param>
	/// <param name="field"></param>
	/// <returns></returns>
	public static ObjectId GetValueAsObjectId(this BsonDocument document, string field)
	{
		try
		{
			return document.TryGetValue(field).AsObjectId;
		}
		catch (KeyNotFoundException)
		{
			return default;
		}
	}

	/// <summary>
	/// Get BsonValue as string
	/// </summary>
	/// <param name="document"></param>
	/// <param name="field"></param>
	/// <returns></returns>
	public static string GetValueAsString(this BsonDocument document, string field)
	{
		try
		{
			return document.TryGetValue(field).AsString;
		}
		catch (KeyNotFoundException)
		{
			return default;
		}
	}

	/// <summary>
	/// Get BsonValue as string array
	/// </summary>
	/// <param name="document"></param>
	/// <param name="field"></param>
	/// <returns></returns>
	public static string[] GetValueAsStringArray(this BsonDocument document, string field)
	{
		try
		{
			var array = document.TryGetValue(field).AsBsonArray;
			return array.Select(ele => ele.AsString).ToArray();
		}
		catch (KeyNotFoundException)
		{
			return default;
		}
	}

	/// <summary>
	/// Get BsonValue as double
	/// </summary>
	/// <param name="document"></param>
	/// <param name="field"></param>
	/// <returns></returns>
	public static double GetValueAsDouble(this BsonDocument document, string field)
	{
		try
		{
			return document.TryGetValue(field).AsDouble;
		}
		catch (KeyNotFoundException)
		{
			return default;
		}
	}

	/// <summary>
	/// Get BsonValue as DateTime
	/// </summary>
	/// <param name="document"></param>
	/// <param name="field"></param>
	/// <returns></returns>
	public static DateTime GetValueAsDateTime(this BsonDocument document, string field)
	{
		try
		{
			return document.TryGetValue(field).ToUniversalTime();
		}
		catch (KeyNotFoundException)
		{
			return default;
		}
	}

	/// <summary>
	/// Get BsonValue as bool
	/// </summary>
	/// <param name="document"></param>
	/// <param name="field"></param>
	/// <returns></returns>
	public static bool GetValueAsBoolean(this BsonDocument document, string field)
	{
		try
		{
			return document.TryGetValue(field).AsBoolean;
		}
		catch (KeyNotFoundException)
		{
			return default;
		}
	}

	/// <summary>
	/// Convert string array to BsonArray
	/// </summary>
	/// <param name="document"></param>
	/// <param name="field"></param>
	/// <returns></returns>
	public static BsonArray ToBsonArray(this string[] sequence)
	{
		try
		{
			var array = new BsonArray();
			foreach (var item in sequence)
			{
				array.Add(item);
			}

			return array;
		}
		catch (KeyNotFoundException)
		{
			return default;
		}
	}

	#region Coordinates

	/// <summary>
	/// Convert GeoJson2DGeographicCoordinates to BsonArray
	/// </summary>
	/// <param name="document"></param>
	/// <param name="field"></param>
	/// <returns></returns>
	public static BsonArray ToBsonArray(this GeoJson2DGeographicCoordinates coordinates)
	{
		return new BsonArray() { coordinates.Latitude, coordinates.Longitude };
	}

	/// <summary>
	/// Get BsonValue as GeoJson2DGeographicCoordinates
	/// </summary>
	/// <param name="document"></param>
	/// <param name="field"></param>
	/// <returns></returns>
	public static GeoJson2DGeographicCoordinates GetValueAsCoordinates(this BsonDocument document, string field)
	{
		try
		{
			var coordinates = document.TryGetValue(field).AsBsonArray;
			var longitude = coordinates[0].AsDouble;
			var latitude = coordinates[1].AsDouble;
			return new GeoJson2DGeographicCoordinates(latitude: latitude, longitude: longitude);
		}
		catch (KeyNotFoundException)
		{
			return default;
		}
	}

	/// <summary>
	/// Try parse location string to GeoJson2DGeographicCoordinates
	/// </summary>
	/// <param name="location"></param>
	/// <returns></returns>
	public static (bool, GeoJson2DGeographicCoordinates) TryParseCoordinates(this string location)
	{
		var tokens = location.Split(',');
		if (tokens.Length != 2 || !double.TryParse(tokens[0], out double latitude) || !double.TryParse(tokens[1], out double longitude))
		{
			return (false, new GeoJson2DGeographicCoordinates(0, 0));
		}

		return (true, new GeoJson2DGeographicCoordinates(latitude: latitude, longitude: longitude));
	}

	#endregion Coordinates

	// Parse field names with dot notation
	public static BsonValue TryGetValue(this BsonDocument document, string field)
	{
		var segments = field.Split('.', StringSplitOptions.RemoveEmptyEntries);
		if (segments.Length == 1)
		{
			return document.GetValue(field);
		}
		else
		{
			var current = document;
			for (int i = 0; i < segments.Length; i++)
			{
				var segment = segments[i];
				if (i == segments.Length - 1)
				{
					return current.GetValue(segment);
				}
				else
				{
					current = current.GetValue(segment).AsBsonDocument;
				}
			}

			return current;
		}
	}
}
