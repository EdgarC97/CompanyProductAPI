using Microsoft.Data.SqlClient;
using System.Data;

namespace CompanyProductAPI.Data
{
    public class DbConnectionFactory
    {
        public IDbConnection CreateConnection()
        {
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("DB_CONNECTION_STRING not found in environment variables.");
            }

            return new SqlConnection(connectionString);
        }
    }
}
