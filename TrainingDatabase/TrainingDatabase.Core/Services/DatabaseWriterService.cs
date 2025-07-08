using TrainingDatabase.Core.Models;
using Microsoft.Data.Sqlite;

namespace TrainingDatabase.Core.Services
{
    internal static class DatabaseWriterService
    {
        internal static int AddCyclistToDb(string path, Cyclist cyclist)
        {
            using (var connection = new SqliteConnection(path))
            {
                string query = @"  
                   INSERT INTO cyclists (name, last_name, birth_date_timestamp)  
                   VALUES (@name, @last_name, @birth_date_timestamp)";
                using (var command = new SqliteCommand(query, connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@name", cyclist.Name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@last_name", cyclist.LastName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@birth_date_timestamp", new DateTimeOffset(cyclist.BirthDate).ToUnixTimeSeconds());
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException("No se pudo insertar el ciclista");
                    }
                }

                using (var idCommand = new SqliteCommand("SELECT last_insert_rowid()", connection))
                {
                    object? result = idCommand.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                    {
                        throw new InvalidOperationException("No se pudo obtener el ID del ciclista insertado");
                    }
                    int cyclistId = Convert.ToInt32(result);
                    return cyclistId;
                }
            }
        }

        internal static void AddCyclistEvolutionToDb(string path, int cyclistId, CyclistEvolution evolution)
        {
            using (var connection = new SqliteConnection(path))
            {
                string query = @"  
                   INSERT INTO cyclist_evolution (cyclist_id, date_timestamp, height, weight, vo2_max, power_curve)  
                   VALUES (@cyclist_id, @date_timestamp, @height, @weight, @vo2_max, @power_curve)";
                using (var command = new SqliteCommand(query, connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@cyclist_id", cyclistId);
                    command.Parameters.AddWithValue("@date_timestamp", new DateTimeOffset(evolution.UpdateDate).ToUnixTimeSeconds());
                    command.Parameters.AddWithValue("@height", evolution.Height);
                    command.Parameters.AddWithValue("@weight", evolution.Weight);
                    command.Parameters.AddWithValue("@vo2_max", evolution.Vo2Max);
                    command.Parameters.AddWithValue("@power_curve", evolution.MaxPowerCurveRaw);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException("No se pudo insertar el ciclista");
                    }
                }
            }
        }
    }
}
