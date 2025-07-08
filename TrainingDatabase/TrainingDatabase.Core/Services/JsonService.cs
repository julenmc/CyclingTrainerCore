using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrainingDatabase.Core.Services
{
    internal static class JsonService
    {
        internal static Dictionary<int, int>? LoadCurveFromJson(string json)
        {
            if (json == null) return null;
            var data = JsonSerializer.Deserialize<Dictionary<string, int>>(json);

            var curve = new Dictionary<int, int>();

            foreach (var kvp in data)
            {
                if (int.TryParse(kvp.Key, out int seconds))
                    curve.Add(seconds, kvp.Value);
            }

            return curve;
        }

        public static string GenerateJsonFromCurve(Dictionary<int, int> powers)
        {
            var data = new Dictionary<string, int>();
            foreach (var kvp in powers)
            {
                data[kvp.Key.ToString()] = kvp.Value;
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(data, options);
        }
    }
}
