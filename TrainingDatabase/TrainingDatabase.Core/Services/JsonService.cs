using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using CyclingTrainer.Core.Models;

namespace CyclingTrainer.TrainingDatabase.Core.Services
{
    internal static class JsonService
    {
        internal static Dictionary<int, PowerCurveData>? LoadCurveFromJson(string json)
        {
            if (json == null) return null;
            Dictionary<string, int>? data = JsonSerializer.Deserialize<Dictionary<string, int>>(json);

            var curve = new Dictionary<int, PowerCurveData>();

            if (data == null) throw new Exception("Invalid raw curve");
            foreach (var kvp in data)
            {
                if (int.TryParse(kvp.Key, out int seconds))
                    curve.Add(seconds, new PowerCurveData { Power = kvp.Value });
            }

            return curve;
        }

        public static string GenerateJsonFromCurve(Dictionary<int, PowerCurveData> powers)
        {
            var data = new Dictionary<string, int>();
            foreach (var kvp in powers)
            {
                data[kvp.Key.ToString()] = (int)kvp.Value.Power;
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(data, options);
        }
    }
}
