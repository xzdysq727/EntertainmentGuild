
using System.ComponentModel.DataAnnotations;

namespace EntertainmentGuild.Models
{
    public class DisabledUser
    {
        [Key]
        public string UserId { get; set; }

        public string Email { get; set; }
        public string UserName { get; set; }

        public string OriginalRole { get; set; }
    }
}
