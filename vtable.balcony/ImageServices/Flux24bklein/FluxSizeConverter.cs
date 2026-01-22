using System;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vtable.Balcony.ImageServices.Flux24bklein
{
    public class FluxSizeJsonConverter : JsonConverter<Size>
    {
        public override Size Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int width = 0;
            int height = 0;

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return new Size(width, height);

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string prop = reader.GetString();
                    reader.Read();

                    switch (prop.ToLowerInvariant())
                    {
                        case "width":
                            width = reader.GetInt32();
                            break;
                        case "height":
                            height = reader.GetInt32();
                            break;
                    }
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Size value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("width", value.Width);
            writer.WriteNumber("height", value.Height);
            writer.WriteEndObject();
        }
    }
}
