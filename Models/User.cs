using System.ComponentModel.DataAnnotations;


namespace Part2_CMCS.Models
{
    public class User
    {
        public int Id { get; set; }


        [Required]
        public string Username { get; set; }


        [Required]
        public string Password { get; set; } // plain-text for prototype only (use hashing in production)


        [Required]
        public string Role { get; set; } // Lecturer, PC, Manager


        public string FullName { get; set; }
    }
}