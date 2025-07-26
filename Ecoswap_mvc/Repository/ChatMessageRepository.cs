using Ecoswap_mvc.Data;
using Ecoswap_mvc.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecoswap_mvc.Repository
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatMessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ChatMessage>> GetAllMessagesAsync()
        {
            return await _context.ChatMessages
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<ChatMessage> GetMessageByIdAsync(int id)
        {
            return await _context.ChatMessages.FindAsync(id);
        }

        public async Task<ChatMessage> CreateMessageAsync(ChatMessage message)
        {
            try
            {
                Console.WriteLine($"ChatMessageRepository: Creating message - Sender: {message.Sender}, Message: {message.Message}, ItemId: {message.ItemId}, SenderId: {message.SenderId}, ReceiverId: {message.ReceiverId}");
                
                _context.ChatMessages.Add(message);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"ChatMessageRepository: Message created successfully with ID: {message.Id}");
                return message;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChatMessageRepository: Error creating message: {ex.Message}");
                Console.WriteLine($"ChatMessageRepository: Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesByItemIdAsync(int? itemId)
        {
            if (!itemId.HasValue)
                return await GetAllMessagesAsync();

            return await _context.ChatMessages
                .Where(m => m.ItemId == itemId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesBySenderAsync(string sender)
        {
            return await _context.ChatMessages
                .Where(m => m.Sender == sender)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<bool> DeleteMessageAsync(int id)
        {
            var message = await _context.ChatMessages.FindAsync(id);
            if (message == null)
                return false;

            _context.ChatMessages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int count = 50)
        {
            return await _context.ChatMessages
                .OrderByDescending(m => m.SentAt)
                .Take(count)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetConversationBetweenUsersAsync(int itemId, int user1Id, int user2Id)
        {
            return await _context.ChatMessages
                .Where(m => m.ItemId == itemId && 
                           ((m.SenderId == user1Id && m.ReceiverId == user2Id) || 
                            (m.SenderId == user2Id && m.ReceiverId == user1Id)))
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<List<int>> GetUserIdsWhoMessagedAboutItemAsync(int itemId, int ownerId)
        {
            return await _context.ChatMessages
                .Where(m => m.ItemId == itemId && (m.SenderId != ownerId || m.ReceiverId != ownerId))
                .Select(m => m.SenderId == ownerId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();
        }
    }
} 