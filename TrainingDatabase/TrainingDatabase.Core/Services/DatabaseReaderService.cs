using CyclingTrainer.Core.Models;
using Microsoft.Data.Sqlite;

namespace CyclingTrainer.TrainingDatabase.Core.Services
{
    internal static class DatabaseReaderService
    {
        internal static async Task<IEnumerable<Cyclist>> GetCyclistsAsync(string path)
        {
            List<Cyclist> cyclists = new List<Cyclist>();
            using (var connection = new SqliteConnection(path))
            {
                string query = @"
                SELECT cyclist_id, name, last_name, birth_date_timestamp
                FROM cyclists";

                using (var command = new SqliteCommand(query, connection))
                {
                    await connection.OpenAsync();
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
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

        internal static async Task<IEnumerable<CyclistFitnessData>> GetCyclistEvolutionAsync(string path, int cyclistId)
        {
            List<CyclistFitnessData> evolutions = new List<CyclistFitnessData>();
            using (var connection = new SqliteConnection(path))
            {
                string query = @$"
                SELECT evolution_id, cyclist_id, date_timestamp, height, weight, vo2_max, power_curve                
                FROM cyclist_evolution
                WHERE cyclist_id = {cyclistId}
                ORDER BY date_timestamp DESC";
                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    await connection.OpenAsync();
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        CyclistFitnessData evolution = new CyclistFitnessData
                        {
                            UpdateDate = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(2)).DateTime,
                            Height = reader.GetInt32(3),
                            Weight = reader.GetFloat(4),
                            Vo2Max = reader.GetFloat(5),
                            MaxPowerCurveRaw = reader.IsDBNull(6) ? null : reader.GetString(6)
                        };
                        evolutions.Add(evolution);
                    }
                }
            }
            return evolutions;
        }

        internal static async Task<IEnumerable<Session>> GetCyclistSessionsAsync(string path, int cyclistId)
        {
            List<Session> sessions = new List<Session>();
            using (var connection = new SqliteConnection(path))
            {
                string query = @$"
                SELECT session_id, cyclist_id, path, start_time, end_time, distance_m, 
                height_diff_m, calories, average_hr, average_power, power_curve, indoor
                FROM training_sessions
                WHERE cyclist_id = {cyclistId}
                ORDER BY start_time DESC";

                using (var command = new SqliteCommand(query, (SqliteConnection)connection))
                {
                    await connection.OpenAsync();
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        Session climb = new Session
                        {
                            Id = reader.GetInt32(0),
                            Path = reader.IsDBNull(2) ? null : reader.GetString(2),
                            StartDate = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(3)).DateTime,
                            EndDate = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(4)).DateTime,
                            Distance = reader.GetDouble(5),
                            HeightDiff = reader.GetDouble(6),
                            AnalyzedData = new AnalyzedData
                            {
                                Calories = reader.GetInt32(7),
                                AverageHr = reader.GetInt32(8),
                                AveragePower = reader.GetInt32(9),
                                PowerCurveRaw = reader.IsDBNull(10) ? null : reader.GetString(10),
                            },
                            IsIndoor = reader.GetBoolean(11),
                        };
                        sessions.Add(climb);
                    }
                }
            }
            return sessions;
        }

        internal static async Task<IEnumerable<Interval>> GetSessionIntervalsAsync(string path, int sessionId)
        {
            List<int> intervalIds = new List<int>();
            List<Interval> intervals = new List<Interval>();
            using (var connection = new SqliteConnection(path))
            {
                string query = @$"
                SELECT session_interval_id, session_id, interval_id
                FROM session_intervals
                WHERE session_id = {sessionId}
                ORDER BY session_id DESC";

                using (var command = new SqliteCommand(query, connection))
                {
                    await connection.OpenAsync();
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int intervalId = reader.GetInt32(2);
                        intervalIds.Add(intervalId);
                    }
                }

                foreach (int intervalId in intervalIds)
                {
                    intervals.Add(await GetIntervalAsync(connection, intervalId));
                }
            }
            return intervals;
        }

        private static async Task<Interval> GetIntervalAsync(SqliteConnection connection, int intervalId)
        {
            Interval interval = new Interval();
            string query = @$"
            SELECT interval_id, start_timestamp, end_timestamp, total_distance_m, average_hr, average_power, average_cadence
            FROM intervals
            WHERE interval_id = {intervalId}
            ORDER BY start_timestamp DESC";

            using (var command = new SqliteCommand(query, (SqliteConnection)connection))
            {
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    interval = new Interval
                    {
                        IntervalId = reader.GetInt32(0),
                        StartTime = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(1)).DateTime,
                        EndTime = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(2)).DateTime,
                        TotalDistance = reader.GetDouble(3),
                        AverageHeartRate = reader.GetInt32(4),
                        AveragePower = reader.GetInt32(5),
                        AverageCadence = reader.GetInt32(6)
                    };
                }
            }
            return interval;
        }

        internal static async Task<IEnumerable<Climb>> GetClimbWithIdAsync(string path, int climbId)
        {
            List<Climb> climbs = new List<Climb>();
            using (var connection = new SqliteConnection(path))
            {
                string query = @"
                SELECT * FROM climbs
                WHERE climb_id = @climb_id";

                using (var command = new SqliteCommand(query, connection))
                {
                    await connection.OpenAsync();
                    command.Parameters.AddWithValue("@climb_id", climbId);
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
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

        internal static async Task<IEnumerable<Climb>> GetClimbsWithCoordsFilterAsync(string path, double longMin, double longMax, double latMin, double latMax)
        {
            List<Climb> climbs = new List<Climb>();
            using (var connection = new SqliteConnection(path))
            {
                string query = @"
                SELECT * FROM climbs
                WHERE long_init > @min_long
                AND long_init < @max_long
                AND lat_init > @min_lat
                AND lat_init < @max_lat";

                using (var command = new SqliteCommand(query, connection))
                {
                    await connection.OpenAsync();
                    command.Parameters.AddWithValue("@min_long", longMin);
                    command.Parameters.AddWithValue("@max_long", longMax);
                    command.Parameters.AddWithValue("@min_lat", latMin);
                    command.Parameters.AddWithValue("@max_lat", latMax);
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
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