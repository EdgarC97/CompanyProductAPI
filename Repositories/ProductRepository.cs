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
                    SELECT p.Id, p.CompanyId, p.Name, p.Description, p.Price, p.Stock, p.CreatedAt,
                           c.Name as CompanyName, c.Email as CompanyEmail
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

                            // Set the basic company info
                            product.Company = new Company
                            {
                                Id = companyId,
                                Name = reader.GetString(reader.GetOrdinal("CompanyName")),
                                Email = reader.IsDBNull(reader.GetOrdinal("CompanyEmail")) ? null : reader.GetString(reader.GetOrdinal("CompanyEmail"))
                            };

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
                    SELECT p.Id, p.CompanyId, p.Name, p.Description, p.Price, p.Stock, p.CreatedAt,
                           c.Name as CompanyName, c.Email as CompanyEmail
                    FROM Products p
                    INNER JOIN Companies c ON p.CompanyId = c.Id
                    WHERE p.Id = @Id", connection))
                {
                    command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var product = MapProductFromReader(reader);

                            // Set the basic company info
                            product.Company = new Company
                            {
                                Id = product.CompanyId,
                                Name = reader.GetString(reader.GetOrdinal("CompanyName")),
                                Email = reader.IsDBNull(reader.GetOrdinal("CompanyEmail")) ? null : reader.GetString(reader.GetOrdinal("CompanyEmail"))
                            };

                            return product;
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

                // First, check if the company exists
                using (var checkCommand = new SqlCommand("SELECT COUNT(1) FROM Companies WHERE Id = @CompanyId", connection))
                {
                    checkCommand.Parameters.Add(new SqlParameter("@CompanyId", SqlDbType.Int) { Value = product.CompanyId });
                    var count = (int)await checkCommand.ExecuteScalarAsync();
                    if (count == 0)
                    {
                        // Company does not exist
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
                    command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = DateTime.Now });

                    // Execute and get the identity value
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

                // Verify the product exists and belongs to the correct company
                using (var checkCommand = new SqlCommand("SELECT CompanyId FROM Products WHERE Id = @Id", connection))
                {
                    checkCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = product.Id });
                    var existingCompanyId = await checkCommand.ExecuteScalarAsync();

                    if (existingCompanyId == null || existingCompanyId == DBNull.Value)
                    {
                        // Product doesn't exist
                        return false;
                    }

                    // We don't allow changing the CompanyId for a product, so we'll use the existing one
                    // This is a business decision - you could allow changing CompanyId if required
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
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}
