using System.ComponentModel.DataAnnotations;


namespace Part2_CMCS.Models
{
    public class ClaimDocument
    {
        public int Id { get; set; }
        [Required]
        public string FileName { get; set; }
        [Required]
        public string FilePath { get; set; }


        public int ClaimId { get; set; }
        public Claim Claim { get; set; }
    }
}