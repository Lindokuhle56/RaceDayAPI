using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;   

namespace RaceDayAPI.Models
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("EventId")]
        public Event Event { get; set; } = null!;
    }
}