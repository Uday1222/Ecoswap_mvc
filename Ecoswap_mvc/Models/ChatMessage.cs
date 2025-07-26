using System;
using System.ComponentModel.DataAnnotations;

namespace Ecoswap_mvc.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        public string Sender { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public int? ItemId { get; set; } // Link to Item if needed
        public int SenderId { get; set; } // ID of the user sending the message
        public int ReceiverId { get; set; } // ID of the user receiving the message
    }
} 