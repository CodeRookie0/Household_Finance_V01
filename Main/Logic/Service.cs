using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Main.Logic
{
    public static class Service
    {
        // Database Connection
        private static SqlConnection conn;
        public static SqlConnection GetDBConnection()
        {
            //string connection_String = Properties.Settings.Default.connection_String;
            //conn = new SqlConnection(connection_String);
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();
            return conn;
        }
        public static void CloseDBConnection()
        {
            if (conn != null && conn.State == ConnectionState.Open)
            {
                conn.Close();
                conn.Dispose();
                conn = null;
            }
        }
        public static DataTable GetDataTables(string SQLText)
        {
            DataTable table = new DataTable();
            using (conn = GetDBConnection())
            {
                SqlDataAdapter adapter = new SqlDataAdapter(SQLText, conn);
                adapter.Fill(table);
            }
            return table;
        }

        // Execute SQL Command
        public static void ExecuteSQL(string SQLText)
        {
            using (conn = GetDBConnection())
            {
                SqlCommand sqlCommand = new SqlCommand(SQLText, conn);
                sqlCommand.ExecuteNonQuery();
            }
        }

        public static void ExecuteSQL(string SQLText, SqlParameter[] parameters = null)
        {
            using (var connection = GetDBConnection())
            {
                using (SqlCommand sqlCommand = new SqlCommand(SQLText, connection))
                {
                    if (parameters != null)
                    {
                        sqlCommand.Parameters.AddRange(parameters);
                    }
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        // Validation Methods
        public static bool IsEmailExists(string email)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            using (var connection = GetDBConnection())
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            return Regex.IsMatch(email, pattern);
        }

        public static bool ValidatePassword(string password)
        {
            string summaryMessage = "Hasło";
            bool passedValidation = true;

            if (password.Length < 8 || password.Length > 15)
            {
                summaryMessage = summaryMessage + "\n- długość musi wynosić od 8 do 15 znaków. ";
                passedValidation = false;
            }

            if (!password.Any(char.IsUpper))
            {
                summaryMessage = summaryMessage + "\n- musi zawierać co najmniej jedną wielką literę. ";
                passedValidation = false;
            }

            if (!password.Any(char.IsLower))
            {
                summaryMessage = summaryMessage + "\n- musi zawierać co najmniej jedną małą literę. ";
                passedValidation = false;
            }

            if (!password.Any(char.IsDigit))
            {
                summaryMessage = summaryMessage + "\n- musi zawierać co najmniej jedną cyfrę. ";
                passedValidation = false;
            }

            string specialCharacters = "-_!*#$&";
            if (!password.Any(c => specialCharacters.Contains(c)))
            {
                summaryMessage = summaryMessage + "\n- musi zawierać co najmniej jeden znak specjalny (-, _, !, *, #, $, &). ";
                passedValidation = false;
            }

            if (!passedValidation)
            {
                MessageBox.Show(summaryMessage, "Błąd walidacji hasła", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return passedValidation;
        }

        // Add Methods
        public static bool AddUser(string username, string email, string password)
        {

            string insertQuery = "INSERT INTO Users (UserName, Email, Password, RoleID) VALUES (@UserName, @Email, @Password, @RoleID)";
            var parameters = new[]
            {
                new SqlParameter("@UserName", username),
                new SqlParameter("@Email", email),
                new SqlParameter("@Password", password),
                new SqlParameter("@RoleID", 1)
            };

            try
            {
                ExecuteSQL(insertQuery, parameters);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        // Update Methods


        // Delete Methods
    }

    //Interfejs do bazy danych Sqlite
    public class DbSqlite
    {
        private readonly string _connectionString;
        private SqliteConnection _connection;

        public DbSqlite()
        {
            _connectionString = "Data Source=FinanceManagerDataBase.db"; //Default DataBase 
        }
        public DbSqlite(string connectionString)
        {
            _connectionString = $"Data Source={connectionString}";

        }


        public DataTable ExecuteQuery(string query, params SqliteParameter[] parameters)
        {
            DataTable resultTable = new DataTable();

            using (_connection = new SqliteConnection(_connectionString))
            {
                _connection.Open();

                using (SqliteCommand command = new SqliteCommand(query, _connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        resultTable.Load(reader);
                    }
                }
            }

            return resultTable;
        }

        public void ExecuteNonQuery(string query, params SqliteParameter[] parameters)
        {
            using (_connection = new SqliteConnection(_connectionString))
            {
                _connection.Open();
                using (SqliteCommand command = new SqliteCommand(query, _connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    command.ExecuteNonQuery();
                }
            }

        }

        public bool LoginUser(string UserName, string Password) //Poprawić generowanie hasha i soli [WSTĘPNA WERSJA]
        {
            string selectQuery = "SELECT PasswordHash,Salt FROM Users WHERE Email=@InputUserName";
            var result = ExecuteQuery(selectQuery, new Microsoft.Data.Sqlite.SqliteParameter("@InputUserName", UserName));
            if (result.Rows.Count > 0)
            {
                var tmp = result.Rows[0];

                string hashPassword = HashPassword(Encoding.UTF8.GetBytes(Password), Encoding.UTF8.GetBytes(tmp["Salt"].ToString()));

                if (hashPassword == tmp["PasswordHash"].ToString())
                {
                    return true;
                }
            }

            return false;

        }


        public bool RegisterUser(string name, string email, string password,string Salt)
        {
            try
            {
                string insertQuery = "INSERT INTO Users (Name, Email,PasswordHash,Salt,CreatedAt) VALUES (@name, @email, @password,@Salt,@createdAt)";
                ExecuteNonQuery(insertQuery,
                    new SqliteParameter("@name", name),
                    new SqliteParameter("@email", email),
                    new SqliteParameter("@password", password),
                    new SqliteParameter("@Salt",Salt),
                    new SqliteParameter("@createdAt", DateTime.Now.ToString()));
                return true;
            }
            catch (SqliteException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }


        public static bool IsEmailExistis(string email)
        {
            DbSqlite tmp = new DbSqlite();
            string selectQuery = "SELECT COUNT(*) FROM Users WHERE Email=@Email";
            var result = tmp.ExecuteQuery(selectQuery, new SqliteParameter("@Email", email));
            return result.Rows.Count > 0 && Convert.ToInt32(result.Rows[0][0]) > 0;

        }

        static public string HashPassword(byte[] bytesToHash, byte[] salt)
        {
            var byteResult = new Rfc2898DeriveBytes(bytesToHash, salt, 1000);
            return Convert.ToBase64String(byteResult.GetBytes(24));
        }

        static public string GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[16];
                rng.GetBytes(salt);
                return Convert.ToBase64String(salt);
            }
        }


        public static bool ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            return Regex.IsMatch(email, pattern);
        }


        public static bool ValidatePassword(string password)
        {
            string summaryMessage = "Hasło";
            bool passedValidation = true;

            if (password.Length < 8 || password.Length > 15)
            {
                summaryMessage = summaryMessage + "\n- długość musi wynosić od 8 do 15 znaków. ";
                passedValidation = false;
            }

            if (!password.Any(char.IsUpper))
            {
                summaryMessage = summaryMessage + "\n- musi zawierać co najmniej jedną wielką literę. ";
                passedValidation = false;
            }

            if (!password.Any(char.IsLower))
            {
                summaryMessage = summaryMessage + "\n- musi zawierać co najmniej jedną małą literę. ";
                passedValidation = false;
            }

            if (!password.Any(char.IsDigit))
            {
                summaryMessage = summaryMessage + "\n- musi zawierać co najmniej jedną cyfrę. ";
                passedValidation = false;
            }

            string specialCharacters = "-_!*#$&";
            if (!password.Any(c => specialCharacters.Contains(c)))
            {
                summaryMessage = summaryMessage + "\n- musi zawierać co najmniej jeden znak specjalny (-, _, !, *, #, $, &). ";
                passedValidation = false;
            }

            if (!passedValidation)
            {
                MessageBox.Show(summaryMessage, "Błąd walidacji hasła", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return passedValidation;

        }



    };
}
