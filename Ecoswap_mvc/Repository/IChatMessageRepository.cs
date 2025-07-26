using Ecoswap_mvc.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecoswap_mvc.Repository
{
    public interface IChatMessageRepository
    {
        Task<IEnumerable<ChatMessage>> GetAllMessagesAsync();
        Task<ChatMessage> GetMessageByIdAsync(int id);
        Task<ChatMessage> CreateMessageAsync(ChatMessage message);
        Task<IEnumerable<ChatMessage>> GetMessagesByItemIdAsync(int? itemId);
        Task<IEnumerable<ChatMessage>> GetMessagesBySenderAsync(string sender);
        Task<bool> DeleteMessageAsync(int id);
        Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int count = 50);
        Task<IEnumerable<ChatMessage>> GetConversationBetweenUsersAsync(int itemId, int user1Id, int user2Id);
        Task<List<int>> GetUserIdsWhoMessagedAboutItemAsync(int itemId, int ownerId);
    }
} 