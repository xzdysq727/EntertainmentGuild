using System.ComponentModel.DataAnnotations.Schema;

namespace EntertainmentGuild.Models
{
    public class CreditCard
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public string CardHolder { get; set; }
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string CardType { get; set; }

        [NotMapped]
        public string Last4Digits => CardNumber?.Length >= 4 ? CardNumber[^4..] : "";
    }
}
