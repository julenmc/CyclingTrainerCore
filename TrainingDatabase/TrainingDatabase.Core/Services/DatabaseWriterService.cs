using TrainingDatabase.Core.Models;
using Microsoft.Data.Sqlite;

namespace TrainingDatabase.Core.Services
{
    internal static class DatabaseWriterService
    {
        internal static async Task<int> AddCyclistAsync(string path, Cyclist cyclist)
        {
            using (var connection = new SqliteConnection(path))
            {
                string query = @"  
                   INSERT INTO cyclists (name, last_name, birth_date_timestamp)  
                   VALUES (@name, @last_name, @birth_date_timestamp)";
                using (var command = new SqliteCommand(query, connection))
                {
                    await connection.OpenAsync();
                    command.Parameters.AddWithValue("@name", cyclist.Name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@last_name", cyclist.LastName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@birth_date_timestamp", new DateTimeOffset(cyclist.BirthDate).ToUnixTimeSeconds());
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException("No se pudo insertar el ciclista");
                    }
                }

                using (var idCommand = new SqliteCommand("SELECT last_insert_rowid()", connection))
                {
                    object? result = await idCommand.ExecuteScalarAsync();
                    if (result == null || result == DBNull.Value)
                    {
                        throw new InvalidOperationException("No se pudo obtener el ID del ciclista insertado");
                    }
                    int cyclistId = Convert.ToInt32(result);
                    return cyclistId;
                }
            }
        }

        internal static async Task AddCyclistEvolutionAsync(string path, int cyclistId, CyclistEvolution evolution)
        {
            using (var connection = new SqliteConnection(path))
            {
                await connection.OpenAsync();
                // Opcional: Verificar que el ciclista existe antes del insert
                string checkQuery = "SELECT COUNT(*) FROM cyclists WHERE cyclist_id = @cyclist_id";
                using (var checkCommand = new SqliteCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@cyclist_id", cyclistId);
                    var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

                    if (!exists)
                    {
                        throw new InvalidOperationException($"El ciclista con ID {cyclistId} no existe");
                    }
                }

                string query = @"  
                   INSERT INTO cyclist_evolution (cyclist_id, date_timestamp, height, weight, vo2_max, power_curve)  
                   VALUES (@cyclist_id, @date_timestamp, @height, @weight, @vo2_max, @power_curve)";
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@cyclist_id", cyclistId);
                    command.Parameters.AddWithValue("@date_timestamp", new DateTimeOffset(evolution.UpdateDate).ToUnixTimeSeconds());
                    command.Parameters.AddWithValue("@height", evolution.Height);
                    command.Parameters.AddWithValue("@weight", evolution.Weight);
                    command.Parameters.AddWithValue("@vo2_max", evolution.Vo2Max);
                    command.Parameters.AddWithValue("@power_curve", evolution.MaxPowerCurveRaw);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException("No se pudo insertar el ciclista");
                    }
                }
            }
        }

        internal static async Task<int> AddSessionAsync(string path, int cyclistId, Session session)
        {
            using (var connection = new SqliteConnection(path))
            {
                await connection.OpenAsync();
                // Opcional: Verificar que el ciclista existe antes del insert
                string checkQuery = "SELECT COUNT(*) FROM cyclists WHERE cyclist_id = @cyclist_id";
                using (var checkCommand = new SqliteCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@cyclist_id", cyclistId);
                    var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

                    if (!exists)
                    {
                        throw new InvalidOperationException($"El ciclista con ID {cyclistId} no existe");
                    }
                }

                string query = @"  
                   INSERT INTO training_sessions (cyclist_id, path, start_time, end_time, distance_m, height_diff_m, 
                                                  calories, average_hr, average_power, power_curve, indoor)  
                   VALUES (@cyclist_id, @path, @start_time, @end_time, @distance_m, @height_diff_m, @calories, @average_hr, @average_power, @power_curve, @indoor)";
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@cyclist_id", cyclistId);
                    command.Parameters.AddWithValue("@path", session.Path);
                    command.Parameters.AddWithValue("@start_time", new DateTimeOffset(session.StartDate).ToUnixTimeSeconds());
                    command.Parameters.AddWithValue("@end_time", new DateTimeOffset(session.EndDate).ToUnixTimeSeconds());
                    command.Parameters.AddWithValue("@distance_m", session.DistanceM);
                    command.Parameters.AddWithValue("@height_diff_m", session.HeightDiff);
                    command.Parameters.AddWithValue("@calories", session.Calories);
                    command.Parameters.AddWithValue("@average_hr", session.AverageHr);
                    command.Parameters.AddWithValue("@average_power", session.AveragePower);
                    command.Parameters.AddWithValue("@power_curve", session.PowerCurveRaw ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@indoor", session.IsIndoor);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException("No se pudo insertar la sesion");
                    }
                }

                using (var idCommand = new SqliteCommand("SELECT last_insert_rowid()", connection))
                {
                    object? result = await idCommand.ExecuteScalarAsync();
                    if (result == null || result == DBNull.Value)
                    {
                        throw new InvalidOperationException("No se pudo obtener el ID de la sesion insertada");
                    }
                    int sessionId = Convert.ToInt32(result);
                    return sessionId;
                }
            }
        }

        internal static async Task<int> AddIntervalAsync(string path, int sessionId, Interval interval)
        {
            using (var connection = new SqliteConnection(path))
            {
                await connection.OpenAsync();

                int intervalId = await AddIntervalAsync(connection, interval);
                await AddSessionIntervalAsync(connection, sessionId, intervalId);
                return intervalId;
            }
        }

        internal static async Task<int> AddClimbIntervalAsync(string path, int sessionId, int climbId, Interval interval)
        {
            using (var connection = new SqliteConnection(path))
            {
                await connection.OpenAsync();

                int intervalId = await AddIntervalAsync(connection, interval);
                await AddClimbIntervalAsync(connection, sessionId, climbId, intervalId);
                return intervalId;
            }
        }

        private static async Task<int> AddIntervalAsync(SqliteConnection connection, Interval interval)
        {
            string query = @"  
                   INSERT INTO intervals (start_timestamp, end_timestamp, total_distance_m, average_hr, average_power, average_cadence)
                   VALUES (@start_timestamp, @end_timestamp, @total_distance_m, @average_hr, @average_power, @average_cadence)";
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@start_timestamp", new DateTimeOffset(interval.StartTime).ToUnixTimeSeconds());
                command.Parameters.AddWithValue("@end_timestamp", new DateTimeOffset(interval.EndTime).ToUnixTimeSeconds());
                command.Parameters.AddWithValue("@total_distance_m", interval.TotalDistance);
                command.Parameters.AddWithValue("@average_hr", interval.AverageHeartRate);
                command.Parameters.AddWithValue("@average_power", interval.AveragePower);
                command.Parameters.AddWithValue("@average_cadence", interval.AverageCadence);
                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException("No se pudo insertar el intervalo");
                }
            }

            using (var idCommand = new SqliteCommand("SELECT last_insert_rowid()", connection))
            {
                object? result = await idCommand.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value)
                {
                    throw new InvalidOperationException("No se pudo obtener el ID del intervalo insertado");
                }
                int intervalId = Convert.ToInt32(result);
                return intervalId;
            }
        }

        private static async Task AddSessionIntervalAsync(SqliteConnection connection, int sessionId, int intervalId)
        {
            // Opcional: Verificar que la sesion existe antes del insert
            string checkQuery = "SELECT COUNT(*) FROM training_sessions WHERE session_id = @session_id";
            using (var checkCommand = new SqliteCommand(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@session_id", sessionId);
                var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

                if (!exists)
                {
                    throw new InvalidOperationException($"La sesion con ID {sessionId} no existe");
                }
            }
            // Opcional: Verificar que el intervalo existe antes del insert
            checkQuery = "SELECT COUNT(*) FROM intervals WHERE interval_id = @interval_id";
            using (var checkCommand = new SqliteCommand(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@interval_id", intervalId);
                var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

                if (!exists)
                {
                    throw new InvalidOperationException($"El intervalo con ID {intervalId} no existe");
                }
            }

            string query = @"  
                   INSERT INTO session_intervals (session_id, interval_id)
                   VALUES (@session_id, @interval_id)";
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@session_id", sessionId);
                command.Parameters.AddWithValue("@interval_id", intervalId);
                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException("No se pudo insertar el intervalo");
                }
            }
        }

        private static async Task AddClimbIntervalAsync(SqliteConnection connection, int sessionId, int climbId, int intervalId)
        {
            // Opcional: Verificar que la sesion existe antes del insert
            string checkQuery = "SELECT COUNT(*) FROM training_sessions WHERE session_id = @session_id";
            using (var checkCommand = new SqliteCommand(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@session_id", sessionId);
                var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

                if (!exists)
                {
                    throw new InvalidOperationException($"La sesion con ID {sessionId} no existe");
                }
            }
            // Opcional: Verificar que el intervalo existe antes del insert
            checkQuery = "SELECT COUNT(*) FROM intervals WHERE interval_id = @interval_id";
            using (var checkCommand = new SqliteCommand(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@interval_id", intervalId);
                var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

                if (!exists)
                {
                    throw new InvalidOperationException($"El intervalo con ID {intervalId} no existe");
                }
            }
            // Opcional: Verificar que la subida existe antes del insert
            checkQuery = "SELECT COUNT(*) FROM climbs WHERE climb_id = @climb_id";
            using (var checkCommand = new SqliteCommand(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@climb_id", climbId);
                var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;

                if (!exists)
                {
                    throw new InvalidOperationException($"La subida con ID {climbId} no existe");
                }
            }

            string query = @"  
                   INSERT INTO session_climbs (session_id, climb_id, interval_id)
                   VALUES (@session_id, @climb_id, @interval_id)";
            using (var command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@session_id", sessionId);
                command.Parameters.AddWithValue("@interval_id", intervalId);
                command.Parameters.AddWithValue("@climb_id", climbId);
                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException("No se pudo insertar el intervalo");
                }
            }
        }

        internal static async Task<int> AddClimbAsync(string path, Climb climb)
        {
            using (var connection = new SqliteConnection(path))
            {
                await connection.OpenAsync();

                string query = @"  
                   INSERT INTO climbs (name, path, long_init, long_end, lat_init, lat_end, alt_init, alt_end, distance, average_slope, max_slope)
                   VALUES (@name, @path, @long_init, @long_end, @lat_init, @lat_end, @alt_init, @alt_end, @distance, @average_slope, @max_slope)";
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", climb.Name);
                    command.Parameters.AddWithValue("@path", climb.Path);
                    command.Parameters.AddWithValue("@long_init", climb.LongitudeInit);
                    command.Parameters.AddWithValue("@long_end", climb.LongitudeEnd);
                    command.Parameters.AddWithValue("@lat_init", climb.LatitudeInit);
                    command.Parameters.AddWithValue("@lat_end", climb.LatitudeEnd);
                    command.Parameters.AddWithValue("@alt_init", climb.AltitudeInit);
                    command.Parameters.AddWithValue("@alt_end", climb.AltitudeEnd);
                    command.Parameters.AddWithValue("@distance", climb.Distance);
                    command.Parameters.AddWithValue("@average_slope", climb.AverageSlope);
                    command.Parameters.AddWithValue("@max_slope", climb.MaxSlope);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException("No se pudo insertar el puerto");
                    }
                }

                using (var idCommand = new SqliteCommand("SELECT last_insert_rowid()", connection))
                {
                    object? result = await idCommand.ExecuteScalarAsync();
                    if (result == null || result == DBNull.Value)
                    {
                        throw new InvalidOperationException("No se pudo obtener el ID del puerto insertado");
                    }
                    int climbId = Convert.ToInt32(result);
                    return climbId;
                }
            }
        }
    }
}
