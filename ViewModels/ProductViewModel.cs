namespace ShopDongY.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public IFormFile? ImageFile { get; set; }
        public IFormFile? ImageFile1 { get; set; }
        public IFormFile? ImageFile2 { get; set; }
        public IFormFile? ImageFile3 { get; set; }

        public string? Unit { get; set; }
        public int? QuantityPerUnit { get; set; }
        public int Code { get; set; }
        public string? UnitInfo { get; set; }

        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
    }
}
