using Microsoft.Data.SqlClient;
using System.Data;

namespace CompanyProductAPI.Data
{
    public class DbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IDbConnection CreateConnection()
        {
            var connectionString = _configuration.GetConnectionString("DB_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("DB_CONNECTION_STRING not found in environment variables.");
            }

            return new SqlConnection(connectionString);
        }
    }
}
