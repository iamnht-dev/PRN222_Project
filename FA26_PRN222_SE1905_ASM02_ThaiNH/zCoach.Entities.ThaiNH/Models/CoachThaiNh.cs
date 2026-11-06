using System.ComponentModel.DataAnnotations;
namespace zCoach.Entities.ThaiNH.Models;

public partial class CoachThaiNh
{
    public int CoachThaiNhid { get; set; }
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(250)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;
    [StringLength(250)]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email must contain a valid @ and domain (example: coach@example.com).")]
    public string? Email { get; set; }
    [StringLength(20)]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Phone must start with 0 and contain exactly 10 digits.")]
    public string? Phone { get; set; }
    [DataType(DataType.Date)] public DateOnly? DateOfBirth { get; set; }
    [StringLength(10)] public string? Gender { get; set; }
    [StringLength(500)] public string? Address { get; set; }
    [StringLength(1000)] public string? ImageUrl { get; set; }
    [StringLength(500)] public string? Experience { get; set; }
    [StringLength(500)] public string? Certification { get; set; }
    [Range(typeof(decimal), "0", "9999999999999999.99")] public decimal? Salary { get; set; }
    [DataType(DataType.Date)] public DateTime? HireDate { get; set; }
    public int? Status { get; set; }
    [DataType(DataType.Date)] public DateTime? PublishDate { get; set; } = DateTime.Today;
    public DateTime? UpdateAt { get; set; }
    public int? SpecializationThaiNhid { get; set; }
    public bool IsActive { get; set; } = true;
    public virtual SpecializationThaiNh? SpecializationThaiNh { get; set; }
}
