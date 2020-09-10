
#if HAVE_ADO_NET
using System;
using System.Data;
using Simula.Scripting.Json.Serialization;

namespace Simula.Scripting.Json.Converters
{
    public class DataSetConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            DataSet dataSet = (DataSet)value;
            DefaultContractResolver? resolver = serializer.ContractResolver as DefaultContractResolver;

            DataTableConverter converter = new DataTableConverter();

            writer.WriteStartObject();

            foreach (DataTable table in dataSet.Tables)
            {
                writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(table.TableName) : table.TableName);

                converter.WriteJson(writer, table, serializer);
            }

            writer.WriteEndObject();
        }
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            DataSet ds = (objectType == typeof(DataSet))
                ? new DataSet()
                : (DataSet)Activator.CreateInstance(objectType);

            DataTableConverter converter = new DataTableConverter();

            reader.ReadAndAssert();

            while (reader.TokenType == JsonToken.PropertyName)
            {
                DataTable dt = ds.Tables[(string)reader.Value!];
                bool exists = (dt != null);

                dt = (DataTable)converter.ReadJson(reader, typeof(DataTable), dt, serializer)!;

                if (!exists)
                {
                    ds.Tables.Add(dt);
                }

                reader.ReadAndAssert();
            }

            return ds;
        }
        public override bool CanConvert(Type valueType)
        {
            return typeof(DataSet).IsAssignableFrom(valueType);
        }
    }
}

#endif