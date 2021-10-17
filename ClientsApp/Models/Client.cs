using System.ComponentModel.DataAnnotations;

namespace ClientsApp.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string Address { get; set; }
        public string PostCode { get; set; }
    }
}
