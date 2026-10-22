using System.ComponentModel.DataAnnotations;
namespace zCoach.Entities.ThaiNH.Models;

public partial class SpecializationThaiNh
{
    public int SpecializationThaiNhid { get; set; }
    [Required, StringLength(200)] public string SpecializationName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Status { get; set; }
    public DateTime? PublishDate { get; set; }
    public virtual ICollection<CoachThaiNh> CoachThaiNhs { get; set; } = new List<CoachThaiNh>();
}