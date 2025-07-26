using System;
using System.ComponentModel.DataAnnotations;

namespace Ecoswap_mvc.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public int? ItemId { get; set; } // Optional: link to Item if needed
    }
} 