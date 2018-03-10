using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.DTOS
{
    public class RegisterDto
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 3, ErrorMessage = "Spec password")]
        public string Password { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string KnownAs { get; set; }
        
        [Required]
        public string City { get; set; }
      
        [Required]
        public string Country { get; set; }

        public DateTime DateOfBirth { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }

        public RegisterDto()
        {
            Created = DateTime.Now;
            LastActive = DateTime.Now;
        }
    }
}