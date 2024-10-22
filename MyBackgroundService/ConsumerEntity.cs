using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackgroundService;

public class ConsumerEntity
{
    public ConsumerEntity(string uuid)
    {
        Uuid = uuid;
    }

    // Primary Key: Auto-incremented integer
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    // UUID column: indexed, not null
    [Column(TypeName = "text")]
    [Required]
    public string Uuid { get; set; }

    // Created time column: default set to current timestamp
    [Column(TypeName = "timestamp with time zone")]
    [Required]
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
}