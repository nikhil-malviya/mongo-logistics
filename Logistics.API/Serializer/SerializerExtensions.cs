using Logistics.API.Responses;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Logistics.API.Serializer;

public static class SerializerExtensions
{

	/// <summary>
	/// Serializes BsonDocument to CityResponse
	/// </summary>
	public static CityResponse ToCityResponse(this BsonDocument? document)
	{
		return BsonSerializer.Deserialize<CityResponse>(document);
	}

	/// <summary>
	/// Serializes IEnumerable<BsonDocument> to IEnumerable<CityResponse>
	/// </summary>
	public static IEnumerable<CityResponse> ToCityResponse(this IEnumerable<BsonDocument> sequence)
	{
		return sequence.Select(x => x.ToCityResponse());
	}

	/// <summary>
	/// Serializes BsonDocument to PlaneResponse
	/// </summary>
	public static PlaneResponse ToPlaneResponse(this BsonDocument? document)
	{
		return BsonSerializer.Deserialize<PlaneResponse>(document);
	}

	/// <summary>
	/// Serializes IEnumerable<BsonDocument> to IEnumerable<PlaneResponse>
	/// </summary>
	public static IEnumerable<PlaneResponse> ToPlaneResponse(this IEnumerable<BsonDocument> sequence)
	{
		return sequence.Select(x => x.ToPlaneResponse());
	}

	/// <summary>
	/// Serializes BsonDocument to CargoResponse
	/// </summary>
	public static CargoResponse ToCargoResponse(this BsonDocument? document)
	{
		return BsonSerializer.Deserialize<CargoResponse>(document);
	}

	/// <summary>
	/// Serializes IEnumerable<BsonDocument> to IEnumerable<CargoResponse>
	/// </summary>
	public static IEnumerable<CargoResponse> ToCargoResponse(this IEnumerable<BsonDocument> document)
	{
		return document.Select(x => x.ToCargoResponse());
	}

	/// <summary>
	/// Serialize BsonDocument to Json
	/// </summary>
	public static string Serialize(this BsonDocument document)
	{
		return document.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson });
	}

	/// <summary>
	/// Serialize IEnumerable<BsonDocument> to Json
	/// </summary>
	public static string Serialize(this IEnumerable<BsonDocument> sequence)
	{
		return sequence.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson });
	}
}