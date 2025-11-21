using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Part2_CMCS.Models
{
    public enum ClaimStatus { Pending, ApprovedByPC, RejectedByPC, ApprovedByManager, RejectedByManager }


    public class Claim
    {
        [Key]
        public int Id { get; set; }


        [Required]
        public required string LecturerUsername { get; set; } // tie to User.Username


        [Required]
        public required string LecturerName { get; set; }


        [Required]
        // map DateSubmitted to the existing DB column (likely "SubmittedAt") so EF queries the correct column
        [Column("SubmittedAt")]
        public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;

        // Backwards-compatible alias used by controllers/views that expect SubmittedAt.
        [NotMapped]
        public DateTime SubmittedAt
        {
            get => DateSubmitted;
            set => DateSubmitted = value;
        }


        [Required]
        public decimal HoursWorked { get; set; }


        [Required]
        public decimal HourlyRate { get; set; }


        // Computed total used by views/controllers (not persisted).
        [NotMapped]
        public decimal TotalAmount => HoursWorked * HourlyRate;


        public string? Notes { get; set; }


        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;


        public DateTime? LastStatusChanged { get; set; }


        public string? LastChangedBy { get; set; }


        public List<ClaimDocument>? Documents { get; set; }
    }
}