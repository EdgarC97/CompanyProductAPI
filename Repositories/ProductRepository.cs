using CompanyProductAPI.Data;
using CompanyProductAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CompanyProductAPI.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly DbConnectionFactory _connectionFactory;

        public ProductRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<IEnumerable<Product>> GetAllByCompanyIdAsync(int companyId)
        {
            var products = new List<Product>();

            using (var connection = _connectionFactory.CreateConnection() as SqlConnection)
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(@"
                    SELECT 
                        p.Id, p.CompanyId, p.Name, p.Description, p.Price, p.Stock, p.CreatedAt,
                        c.Id AS CompanyId, c.Name AS CompanyName, c.Address AS CompanyAddress,
                        c.Phone AS CompanyPhone, c.Email AS CompanyEmail, c.WebSite AS CompanyWebSite,
                        c.CreatedAt AS CompanyCreatedAt
                    FROM Products p
                    INNER JOIN Companies c ON p.CompanyId = c.Id
                    WHERE p.CompanyId = @CompanyId", connection))
                {
                    command.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId });

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var product = MapProductFromReader(reader);
                            products.Add(product);
                        }
                    }
                }
            }

            return products;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            using (var connection = _connectionFactory.CreateConnection() as SqlConnection)
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(@"
                    SELECT 
                        p.Id, p.CompanyId, p.Name, p.Description, p.Price, p.Stock, p.CreatedAt,
                        c.Id AS CompanyId, c.Name AS CompanyName, c.Address AS CompanyAddress,
                        c.Phone AS CompanyPhone, c.Email AS CompanyEmail, c.WebSite AS CompanyWebSite,
                        c.CreatedAt AS CompanyCreatedAt
                    FROM Products p
                    INNER JOIN Companies c ON p.CompanyId = c.Id
                    WHERE p.Id = @Id", connection))
                {
                    command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapProductFromReader(reader);
                        }
                        return null;
                    }
                }
            }
        }

        public async Task<int> CreateAsync(Product product)
        {
            using (var connection = _connectionFactory.CreateConnection() as SqlConnection)
            {
                await connection.OpenAsync();

                using (var checkCommand = new SqlCommand("SELECT COUNT(1) FROM Companies WHERE Id = @CompanyId", connection))
                {
                    checkCommand.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.Int) { Value = product.CompanyId });
                    var count = (int)await checkCommand.ExecuteScalarAsync();
                    if (count == 0)
                    {
                        throw new InvalidOperationException($"Cannot create product. Company with ID {product.CompanyId} does not exist.");
                    }
                }

                using (var command = new SqlCommand(@"
                    INSERT INTO Products (CompanyId, Name, Description, Price, Stock, CreatedAt)
                    VALUES (@CompanyId, @Name, @Description, @Price, @Stock, @CreatedAt);
                    SELECT SCOPE_IDENTITY();", connection))
                {
                    command.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.Int) { Value = product.CompanyId });
                    command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = product.Name });
                    command.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 500) { Value = (object)product.Description ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@Price", SqlDbType.Decimal) { Value = product.Price, Precision = 18, Scale = 2 });
                    command.Parameters.Add(new SqlParameter("@Stock", SqlDbType.Int) { Value = product.Stock });
                    command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = DateTime.UtcNow });

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            using (var connection = _connectionFactory.CreateConnection() as SqlConnection)
            {
                await connection.OpenAsync();

                using (var checkCommand = new SqlCommand("SELECT CompanyId FROM Products WHERE Id = @Id", connection))
                {
                    checkCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = product.Id });
                    var existingCompanyId = await checkCommand.ExecuteScalarAsync();

                    if (existingCompanyId == null || existingCompanyId == DBNull.Value)
                    {
                        return false;
                    }

                    product.CompanyId = (int)existingCompanyId;
                }

                using (var command = new SqlCommand(@"
                    UPDATE Products
                    SET Name = @Name, 
                        Description = @Description, 
                        Price = @Price, 
                        Stock = @Stock
                    WHERE Id = @Id", connection))
                {
                    command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = product.Id });
                    command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = product.Name });
                    command.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 500) { Value = (object)product.Description ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@Price", SqlDbType.Decimal) { Value = product.Price, Precision = 18, Scale = 2 });
                    command.Parameters.Add(new SqlParameter("@Stock", SqlDbType.Int) { Value = product.Stock });

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

                using (var command = new SqlCommand("DELETE FROM Products WHERE Id = @Id", connection))
                {
                    command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        private Product MapProductFromReader(SqlDataReader reader)
        {
            return new Product
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                CompanyId = reader.GetInt32(reader.GetOrdinal("CompanyId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                Company = new Company
                {
                    Id = reader.GetInt32(reader.GetOrdinal("CompanyId")),
                    Name = reader.GetString(reader.GetOrdinal("CompanyName")),
                    Address = reader.IsDBNull(reader.GetOrdinal("CompanyAddress")) ? null : reader.GetString(reader.GetOrdinal("CompanyAddress")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("CompanyPhone")) ? null : reader.GetString(reader.GetOrdinal("CompanyPhone")),
                    Email = reader.IsDBNull(reader.GetOrdinal("CompanyEmail")) ? null : reader.GetString(reader.GetOrdinal("CompanyEmail")),
                    WebSite = reader.IsDBNull(reader.GetOrdinal("CompanyWebSite")) ? null : reader.GetString(reader.GetOrdinal("CompanyWebSite")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CompanyCreatedAt"))
                }
            };
        }
    }
}
