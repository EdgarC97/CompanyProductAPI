using CompanyProductAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CompanyProductAPI.Data
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

        public async Task<Company> GetByIdWithProductsAsync(int id)
        {
            Company company = null;
            var products = new List<Product>();

            using (var connection = _connectionFactory.CreateConnection() as SqlConnection)
            {
                await connection.OpenAsync();

                // Get company
                using (var command = new SqlCommand(@"
                    SELECT c.Id, c.Name, c.Address, c.Phone, c.Email, c.WebSite, c.CreatedAt,
                           p.Id as ProductId, p.Name as ProductName, p.Description, p.Price, p.Stock, p.CreatedAt as ProductCreatedAt
                    FROM Companies c
                    LEFT JOIN Products p ON c.Id = p.CompanyId
                    WHERE c.Id = @Id", connection))
                {
                    command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (company == null)
                            {
                                company = new Company
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                                    WebSite = reader.IsDBNull(reader.GetOrdinal("WebSite")) ? null : reader.GetString(reader.GetOrdinal("WebSite")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    Products = new List<Product>()
                                };
                            }

                            // Check if we have a product (could be null if company has no products)
                            if (!reader.IsDBNull(reader.GetOrdinal("ProductId")))
                            {
                                var product = new Product
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                    CompanyId = id,
                                    Name = reader.GetString(reader.GetOrdinal("ProductName")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("ProductCreatedAt")),
                                    Company = company
                                };

                                company.Products.Add(product);
                            }
                        }
                    }
                }
            }

            return company;
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
                    command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = DateTime.Now });

                    // Execute and get the identity value
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

                // First check if company has products
                using (var checkCommand = new SqlCommand("SELECT COUNT(1) FROM Products WHERE CompanyId = @Id", connection))
                {
                    checkCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
                    var count = (int)await checkCommand.ExecuteScalarAsync();
                    if (count > 0)
                    {
                        // Company has products, can't delete
                        return false;
                    }
                }

                // If no products, proceed with deletion
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
