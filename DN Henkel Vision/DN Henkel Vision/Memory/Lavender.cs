using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using Windows.Storage;

namespace DN_Henkel_Vision.Memory
{
    public static class Lavender
    {
        private static string s_local = "";
        private static SqliteConnection Lavenderbase;

        public static void Invalidate()
        {
            Lavenderbase?.Close();
            Lavenderbase = null;
        }

        public static SqliteConnection GetConnection()
        {
            if (Lavenderbase == null)
            {
                s_local = Path.Combine(ApplicationData.Current.LocalFolder.Path, "DN Henkel Vision Data.db");
                Lavenderbase = new SqliteConnection($"Filename={s_local}");
                Lavenderbase.Open();

                string[] tables = new string[] {
                    "CREATE TABLE IF NOT EXISTS orders (id INTEGER PRIMARY KEY, number INTEGER)",
                    "CREATE TABLE IF NOT EXISTS faults (id INTEGER PRIMARY KEY, number INTEGER, component TEXT, placement TEXT, description TEXT, cause TEXT, classification TEXT, type TEXT, user TEXT, machine TEXT, status TEXT)",
                    "CREATE TABLE IF NOT EXISTS users (id INTEGER PRIMARY KEY, username TEXT, fullname TEXT, password TEXT, role TEXT)",
                    "CREATE TABLE IF NOT EXISTS graph (id INTEGER PRIMARY KEY, datafrom DATE, userservice TIME, machineservice TIME, userexports TIME, machineexports TIME)",
                    "CREATE TABLE IF NOT EXISTS settings (id INTEGER PRIMARY KEY, name TEXT, value TEXT)"
                };

                for (int i = 0; i < tables.Length; i++)
                {
                    SqliteCommand createTable = new SqliteCommand(tables[i], Lavenderbase);
                    createTable.ExecuteReader();
                }
            }

            return Lavenderbase;
        }

        public static void Validate()
        {
            GetConnection();
        }

        public static List<string> LoadRegistry()
        {
            List<string> registry = new List<string>();

            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();
                SqliteCommand selectCommand = new SqliteCommand("SELECT * from orders", Lavenderbase);
                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    registry.Add(query.GetString(1));
                }

                Lavenderbase.Close();
            }

            return registry;
        }

        public static List<Fault> LoadFaults(string orderNumber)
        {
            List<Fault> faults = new List<Fault>();

            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();
                SqliteCommand selectCommand = new SqliteCommand($"SELECT * from faults WHERE number='{orderNumber.Replace(" ", "")}'", Lavenderbase);
                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    Fault fault = new(query.GetString(4))
                    {
                        Index = uint.Parse(query.GetString(0)),
                        Component = query.GetString(2),
                        Placement = query.GetString(3)
                    };

                    fault.ClassIndexes[0] = query.GetInt32(5);
                    fault.ClassIndexes[1] = query.GetInt32(6);
                    fault.ClassIndexes[2] = query.GetInt32(7);

                    fault.Cause = Memory.Classification.LocalCauses[fault.ClassIndexes[0]];
                    fault.Classification = Memory.Classification.LocalClassifications[fault.ClassIndexes[0]][fault.ClassIndexes[1]];
                    fault.Type = Memory.Classification.LocalTypes[Memory.Classification.ClassificationsPointers[fault.ClassIndexes[0]][fault.ClassIndexes[1]]][fault.ClassIndexes[2]];
                
                    faults.Add(fault);
                }

                Lavenderbase.Close();
            }

            return faults.Reverse<Fault>().ToList();
        }
    
        public static int AppendFault(Fault fault, string orderNumber)
        {
            int id = 0;

            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();

                SqliteCommand insertCommand = new SqliteCommand($"INSERT INTO faults (number, component, placement, description, cause, classification, type, user, machine, status) VALUES ('{orderNumber.Replace(" ", "")}', '{fault.Component}', '{fault.Placement}', '{fault.Description}', '{fault.ClassIndexes[0]}', '{fault.ClassIndexes[1]}', '{fault.ClassIndexes[2]}', '{fault.UserTime}', '{fault.MachineTime}', 'complete')", Lavenderbase);
                insertCommand.ExecuteNonQuery();

                SqliteCommand selectCommand = new SqliteCommand("SELECT * FROM faults WHERE id = (SELECT MAX(id) FROM faults)", Lavenderbase);
                SqliteDataReader query = selectCommand.ExecuteReader();

                query.Read();
                id = query.GetInt32(0);
                Lavenderbase.Close();
            }

            return id;
        }

        public static void DeleteFault(uint index)
        {
            using (SqliteConnection Lavenderbase = GetConnection())
            {
                Lavenderbase.Open();
                SqliteCommand deleteCommand = new SqliteCommand($"DELETE FROM faults WHERE id='{index}'", Lavenderbase);
                deleteCommand.ExecuteReader();
                Lavenderbase.Close();
            }
        }
    }
}
