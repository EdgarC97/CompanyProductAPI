using CompanyProductAPI.Data;
using CompanyProductAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;

namespace CompanyProductAPI.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public CompanyRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<IEnumerable<Company>> GetAllAsync()
        {
            var companies = new List<Company>();

            using (var connection = _connectionFactory.CreateConnection() as SqlConnection)
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("SELECT Id, Name, Address, Phone, Email, WebSite, CreatedAt FROM Companies", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        companies.Add(MapCompanyFromReader(reader));
                    }
                }
            }

            return companies;
        }

        public async Task<Company> GetByIdAsync(int id)
        {
            using (var connection = _connectionFactory.CreateConnection() as SqlConnection)
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("SELECT Id, Name, Address, Phone, Email, WebSite, CreatedAt FROM Companies WHERE Id = @Id", connection))
                {
                    command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapCompanyFromReader(reader);
                        }
                        return null;
                    }
                }
            }
        }

        public async Task<int> CreateAsync(Company company)
        {
            using (var connection = _connectionFactory.CreateConnection() as SqlConnection)
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(@"
                    INSERT INTO Companies (Name, Address, Phone, Email, WebSite, CreatedAt)
                    VALUES (@Name, @Address, @Phone, @Email, @WebSite, @CreatedAt);
                    SELECT SCOPE_IDENTITY();", connection))
                {
                    command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = company.Name });
                    command.Parameters.Add(new SqlParameter("@Address", SqlDbType.NVarChar, 200) { Value = (object)company.Address ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 20) { Value = (object)company.Phone ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = (object)company.Email ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@WebSite", SqlDbType.NVarChar, 100) { Value = (object)company.WebSite ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = DateTime.UtcNow });

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<bool> UpdateAsync(Company company)
        {
            using (var connection = _connectionFactory.CreateConnection() as SqlConnection)
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(@"
                    UPDATE Companies
                    SET Name = @Name, 
                        Address = @Address, 
                        Phone = @Phone, 
                        Email = @Email, 
                        WebSite = @WebSite
                    WHERE Id = @Id", connection))
                {
                    command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = company.Id });
                    command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = company.Name });
                    command.Parameters.Add(new SqlParameter("@Address", SqlDbType.NVarChar, 200) { Value = (object)company.Address ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 20) { Value = (object)company.Phone ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = (object)company.Email ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@WebSite", SqlDbType.NVarChar, 100) { Value = (object)company.WebSite ?? DBNull.Value });

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = _connectionFactory.CreateConnection() as SqlConnection)
            {
                await connection.OpenAsync();

                using (var checkCommand = new SqlCommand("SELECT COUNT(1) FROM Products WHERE CompanyId = @Id", connection))
                {
                    checkCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
                    var count = (int)await checkCommand.ExecuteScalarAsync();
                    if (count > 0)
                    {
                        return false; // no eliminar si tiene productos
                    }
                }

                using (var command = new SqlCommand("DELETE FROM Companies WHERE Id = @Id", connection))
                {
                    command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        private Company MapCompanyFromReader(SqlDataReader reader)
        {
            return new Company
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                WebSite = reader.IsDBNull(reader.GetOrdinal("WebSite")) ? null : reader.GetString(reader.GetOrdinal("WebSite")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}
