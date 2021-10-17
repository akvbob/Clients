using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;


namespace ClientsApp.Models
{
    public class Log
    {
        [Key]
        public int LogId { get; set; }

        public string Message { get; set; }
        public string MessageTemplate { get; set; }
        public string Level { get; set; }
        
        [Required]
        public  DateTime TimeStamp { get; set; }
 
        public string Exception { get; set; }
        public string Properties { get; set; }
    }
}
