using Microsoft.Data.SqlClient;

namespace LoginSystem.InjectionDemo
{
    public static class VulnerableLoginDemo
    {

        public static bool VulnerableLogin(SqlConnection con, string username, string password)
        {
            string sql = "SELECT COUNT(*) FROM Users WHERE Username='" + username +
                         "' AND PasswordHash='" + password + "'";

            using (var cmd = new SqlCommand(sql, con))
            {
                con.Open();
                int count = (int)cmd.ExecuteScalar();
                con.Close();
                return count > 0;
            }
        }

        public static bool FixedLogin(SqlConnection con, string username, string passwordHash)
        {
            string sql = "SELECT COUNT(*) FROM Users WHERE Username=@username AND PasswordHash=@hash";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@hash", passwordHash);
                con.Open();
                int count = (int)cmd.ExecuteScalar();
                con.Close();
                return count > 0;
            }
        }
    }
}