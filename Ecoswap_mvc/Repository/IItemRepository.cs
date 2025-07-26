using EcoSwap.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecoswap_mvc.Repository
{
    public interface IItemRepository
    {
        Task<IEnumerable<Item>> GetAllItemsAsync();
        Task<Item> GetItemByIdAsync(int id);
        Task<Item> CreateItemAsync(Item item);
        Task<Item> UpdateItemAsync(Item item);
        Task<bool> DeleteItemAsync(int id);
        Task<IEnumerable<Item>> GetItemsByUserIdAsync(string userId);
    }
} 