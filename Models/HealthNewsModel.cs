using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class HealthNewsModel
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
    public string Content { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [NotMapped]
    public IFormFile? ImageFile { get; set; }
}
