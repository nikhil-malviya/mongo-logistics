using MongoDB.Driver.GeoJsonObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Logistics.DataAccess.Converters
{
    public class GeoJson2DCoordinatesJsonConverter : JsonConverter<GeoJson2DCoordinates>
    {
        public override GeoJson2DCoordinates? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var coordinates = JsonSerializer.Deserialize<double[]>(ref reader, options);
            return new GeoJson2DCoordinates(coordinates?[0] ?? 0, coordinates?[1] ?? 0);
        }

        public override void Write(Utf8JsonWriter writer, GeoJson2DCoordinates value, JsonSerializerOptions options)
        {
            writer.WriteRawValue($"[{value.X},{value.Y}]");
        }
    }
}