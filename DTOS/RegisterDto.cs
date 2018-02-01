using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.DTOS
{
    public class RegisterDto
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [StringLength(8, MinimumLength=3, ErrorMessage="Spec password")]
        public string Password { get; set; }
    }
}