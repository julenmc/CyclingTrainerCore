using TrainingDatabase.Core.Models;
using Microsoft.Data.Sqlite;

namespace TrainingDatabase.Core.Services
{
    internal static class DatabaseReaderService
    {
        internal static IEnumerable<Cyclist> GetCyclistsFromDb(string path)
        {
            List<Cyclist> cyclists = new List<Cyclist>();
            using (var connection = new SqliteConnection(path))
            {
                string query = @"
                SELECT cyclist_id, name, last_name, birth_date_timestamp
                FROM cyclists";

                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    connection.Open();
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Cyclist cyclist = new Cyclist
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                            LastName = reader.IsDBNull(2) ? null : reader.GetString(2),
                            BirthDate = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(3)).DateTime
                        };
                        cyclists.Add(cyclist);
                    }
                }
            }
            return cyclists;
        }

        internal static IEnumerable<CyclistEvolution> GetCyclistEvolution(string path, int cyclistId)
        {
            List<CyclistEvolution> evolutions = new List<CyclistEvolution>();
            using (var connection = new SqliteConnection(path))
            {
                string query = @$"
                SELECT evolution_id, cyclist_id, date_timestamp, height, weight, vo2_max                
                FROM cyclist_evolution
                WHERE cyclist_id = {cyclistId}
                ORDER BY date_timestamp DESC";
                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    connection.Open();
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CyclistEvolution evolution = new CyclistEvolution
                        {
                            UpdateDate = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(2)).DateTime,
                            Height = reader.GetInt32(3),
                            Weight = reader.GetFloat(4),
                            Vo2Max = reader.GetFloat(5)
                        };
                        evolutions.Add(evolution);
                    }
                }
            }
            return evolutions;
        }

        internal static IEnumerable<Climb> GetClimbsFromDb(string path)
        {
            List<Climb> climbs = new List<Climb>();
            using (var connection = new SqliteConnection(path))
            {
                string query = @"
                SELECT climb_id, name, path, long_init, long_end, 
                lat_init, lat_end, alt_init, alt_end, average_slope, max_slope
                FROM climbs";

                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    connection.Open();
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Climb climb = new Climb
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Path = reader.IsDBNull(2) ? null : reader.GetString(2),
                            LongitudeInit = reader.GetDouble(3),
                            LongitudeEnd = reader.GetDouble(4),
                            LatitudeInit = reader.GetDouble(5),
                            LatitudeEnd = reader.GetDouble(6),
                            AltitudeInit = reader.GetDouble(7),
                            AltitudeEnd = reader.GetDouble(8),
                            AverageSlope = reader.GetDouble(9),
                            MaxSlope = reader.GetDouble(10),
                        };
                        climbs.Add(climb);
                    }
                }
            }
            return climbs;
        }
    }
}