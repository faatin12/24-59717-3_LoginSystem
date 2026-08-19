using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace LoginSystem
{
    public static class DatabaseHelper
    {
        private static string ConnString =>
            ConfigurationManager.ConnectionStrings["LoginDB"].ConnectionString;

        public static bool TestConnection(out string errorMessage)
        {
            errorMessage = null;
            try
            {
                using (var con = new SqlConnection(ConnString))
                {
                    con.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public static bool UsernameExists(string username)
        {
            using (var con = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.Users WHERE Username = @username", con))
            {
                cmd.Parameters.AddWithValue("@username", username);
                con.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public static void RegisterUser(string username, string password, string email, string fullName)
        {
            string salt = PasswordHelper.GenerateSalt();
            string hash = PasswordHelper.HashPassword(password, salt);

            using (var con = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO dbo.Users (Username, PasswordHash, PasswordSalt, Email, FullName)
                  VALUES (@username, @hash, @salt, @email, @fullName)", con))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@hash", hash);
                cmd.Parameters.AddWithValue("@salt", salt);
                cmd.Parameters.AddWithValue("@email", (object)email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@fullName", (object)fullName ?? DBNull.Value);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static (int UserID, string FullName)? ValidateLogin(string username, string password)
        {
            using (var con = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(
                "SELECT UserID, PasswordHash, PasswordSalt, FullName FROM dbo.Users WHERE Username = @username", con))
            {
                cmd.Parameters.AddWithValue("@username", username);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    string storedHash = reader["PasswordHash"].ToString();
                    string salt = reader["PasswordSalt"].ToString();
                    int userId = (int)reader["UserID"];
                    string fullName = reader["FullName"] == DBNull.Value ? "" : reader["FullName"].ToString();

                    string attemptHash = PasswordHelper.HashPassword(password, salt);
                    if (attemptHash == storedHash)
                        return (userId, fullName);

                    return null;
                }
            }
        }

        public static int RecordLogin(int userId)
        {
            using (var con = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO dbo.LoginHistory (UserID, LoginTime) OUTPUT INSERTED.LoginHistoryID
                  VALUES (@userId, GETDATE())", con))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public static void RecordLogout(int loginHistoryId)
        {
            using (var con = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(
                "UPDATE dbo.LoginHistory SET LogoutTime = GETDATE() WHERE LoginHistoryID = @id AND LogoutTime IS NULL", con))
            {
                cmd.Parameters.AddWithValue("@id", loginHistoryId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static DataTable GetUsersTable()
        {
            var table = new DataTable();
            using (var con = new SqlConnection(ConnString))
            using (var adapter = new SqlDataAdapter(
                "SELECT UserID, Username, Email, FullName, CreatedAt FROM dbo.Users ORDER BY UserID", con))
            {
                adapter.Fill(table);
            }
            return table;
        }

        public static DataTable SearchUsers(string term)
        {
            var table = new DataTable();
            using (var con = new SqlConnection(ConnString))
            using (var adapter = new SqlDataAdapter(
                "SELECT UserID, Username, Email, FullName, CreatedAt FROM dbo.Users WHERE Username LIKE @term ORDER BY UserID", con))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@term", "%" + term + "%");
                adapter.Fill(table);
            }
            return table;
        }

        public static void DeleteUser(int userId)
        {
            using (var con = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("DELETE FROM dbo.Users WHERE UserID = @id", con))
            {
                cmd.Parameters.AddWithValue("@id", userId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}