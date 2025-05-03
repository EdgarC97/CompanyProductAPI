using System.Text.Json.Serialization;

namespace CompanyProductAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        public Company? Company { get; set; }
    }
}
