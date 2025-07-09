using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json;

namespace ShopDongY.Models
{
    public class WarehouseModel
    {
        [Key, ForeignKey("Product")] // Khóa chính đồng thời là khóa ngoại
        public int ProductId { get; set; }
        [JsonIgnore]
        public virtual ProductModel? Product { get; set; }

        [Required]
        public int TotalImported { get; set; }

        [Required]
        public int TotalSold { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        [NotMapped]
        public int QuantityInStock => TotalImported - TotalSold;
    }

}
