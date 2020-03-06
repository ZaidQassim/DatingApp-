using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string userName { get; set; }


        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "You moust specify password between  4 and 8 characters")]
        public string password { get; set; }
    }
}