using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BigBeerData.Shared.Utils
{
	/// <summary>
	/// Temp Dynamic Converter with c# 8
	/// by:tchivs@live.cn
	/// </summary>
	public class DynamicJsonConverter : JsonConverter<dynamic>
	{
		public override dynamic Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)

		=> reader.TokenType switch
		{
			JsonTokenType.True => true,
			JsonTokenType.False => false,
			JsonTokenType.Number => reader.GetString().Contains('.')
					? reader.TryGetDouble(out double d) ? d : 0d : reader.TryGetInt32(out int i) ? i : 0,
			JsonTokenType.String => reader.TryGetDateTime(out DateTime datetime) ? datetime.ToString() : reader.GetString(),
			JsonTokenType.StartObject => ReadObject(JsonDocument.ParseValue(ref reader).RootElement),
			_ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
		};

		private object ReadObject(JsonElement jsonElement)
		{
			IDictionary<string, object> expandoObject = new ExpandoObject();
			foreach (var obj in jsonElement.EnumerateObject())
			{
				var k = obj.Name;
				var value = ReadValue(obj.Value);
				expandoObject[k] = value;
			}
			return expandoObject;
		}

		#nullable enable
		private object? ReadValue(JsonElement jsonElement) => jsonElement.ValueKind switch
		{
			JsonValueKind.Object => ReadObject(jsonElement),
			JsonValueKind.Array => ReadList(jsonElement),
			JsonValueKind.String => jsonElement.ToString(),
			JsonValueKind.Number => ReadNumber(jsonElement),
			JsonValueKind.True => true,
			JsonValueKind.False => false,
			JsonValueKind.Undefined => null,
			JsonValueKind.Null => null,
			_ => throw new ArgumentOutOfRangeException(nameof(jsonElement), jsonElement, "Cannot parse")
		};

		private object? ReadList(JsonElement jsonElement)
		{
			var list = new List<object?>();
			jsonElement.EnumerateArray().ToList().ForEach(j => list.Add(ReadValue(j)));
			return list.Count == 0 ? null : list;
		}

		private static object? ReadNumber(JsonElement jsonElement){
			if (jsonElement.GetRawText().Contains('.')){
				return jsonElement.TryGetDouble(out double d) ? d : 0d ;
			}
			return jsonElement.TryGetInt32(out int i) ? i : 0;
		}

		public override void Write(Utf8JsonWriter writer,
			object value,
			JsonSerializerOptions options)
		{
			// writer.WriteStringValue(value.ToString());
		}
	}

}
