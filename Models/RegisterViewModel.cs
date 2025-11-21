using System.ComponentModel.DataAnnotations;

namespace Part2_CMCS.Models
{
    public class RegisterViewModel
    {
        [Required]
        public string Username { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        [Required]
        public string FullName { get; set; } = "";

        [Required]
        public string Role { get; set; } = "";
    }
}
