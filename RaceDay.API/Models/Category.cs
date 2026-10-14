using RaceDay.API.Models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class Category
{
    [Key]
    public int CategoryID { get; set; }

    [Required]
    public int EventID { get; set; }
    public Event Event { get; set; } = null!;

    [Required, MaxLength(50)]
    public string Name { get; set; } = null!;

    [MaxLength(200)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string? CategoryType { get; set; } // Age | Distance | Other

    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? Distance { get; set; }
    public decimal? EntryFee { get; set; } = 0;
    public int? Capacity { get; set; }

    public ICollection<EventEnrolment> EventEnrolments { get; set; } = new List<EventEnrolment>();
}