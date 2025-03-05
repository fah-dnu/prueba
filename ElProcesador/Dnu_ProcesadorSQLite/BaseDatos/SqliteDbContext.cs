using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu_ProcesadorSQLite.BaseDatos
{
    public static class SqliteDbContext
    {
        private const string DBName = @"{0}\SamsBenefits{1}.db";
        //private const string SQLScript = @"DB\init.sql";

        private static bool IsDbRecentlyCreated = false;

        public static String Up(string claveEstado, String path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            // Crea la base de datos y registra usuario solo una vez

            if (File.Exists(Path.GetFullPath(String.Format(DBName, path, claveEstado))))
            {
                File.Delete(Path.GetFullPath(String.Format(DBName, path, claveEstado)));
            }

            if (!File.Exists(Path.GetFullPath(String.Format(DBName, path, claveEstado))))
            {
                SQLiteConnection.CreateFile(String.Format(DBName, path, claveEstado));
                IsDbRecentlyCreated = true;
            }


            byte[] byteArray = Encoding.ASCII.GetBytes(Dnu_ProcesadorSQLite.Properties.Resources.init);
            MemoryStream stream = new MemoryStream(byteArray);

            using (var ctx = GetInstance(claveEstado, path))
            {
                if (IsDbRecentlyCreated)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var query = "";
                        var line = "";
                        while ((line = reader.ReadLine()) != null)
                        {
                            query += line;
                        }

                        using (var command = new SQLiteCommand(query, ctx))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }

            return String.Format(DBName, path, claveEstado);
        }

        public static SQLiteConnection GetInstance(String claveEstado,string path)
        {
            var db = new SQLiteConnection(
                string.Format("Data Source={0};Version=3;", String.Format(DBName, path, claveEstado))
            );

            db.Open();

            return db;
        }
    }
}
