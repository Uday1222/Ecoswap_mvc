using Ecoswap_mvc.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecoswap_mvc.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        public string Sender { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        //// Backing field for encrypted message
        //private string _encryptedMessage;

        //public string Message
        //{
        //    get => CryptoHelper.Decrypt(_encryptedMessage); // Decrypt when reading
        //    set => _encryptedMessage = CryptoHelper.Encrypt(value); // Encrypt when saving
        //}

        //[Column("Message")]
        //public string EncryptedMessage
        //{
        //    get => _encryptedMessage;
        //    set => _encryptedMessage = value;
        //}

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public int? ItemId { get; set; } // Link to Item if needed
        public int SenderId { get; set; } // ID of the user sending the message
        public int ReceiverId { get; set; } // ID of the user receiving the message
    }
} 