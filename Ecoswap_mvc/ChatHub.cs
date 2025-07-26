using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Ecoswap_mvc.Repository;
using Ecoswap_mvc.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Ecoswap_mvc
{
    public class ChatHub : Hub
    {
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IUserRepository _userRepository;

        public ChatHub(IChatMessageRepository chatMessageRepository, 
                      IItemRepository itemRepository, 
                      IUserRepository userRepository)
        {
            _chatMessageRepository = chatMessageRepository;
            _itemRepository = itemRepository;
            _userRepository = userRepository;
        }

        public async Task JoinItemChat(int itemId, int userId)
        {
            // Add user to the specific item's chat group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Item_{itemId}");
            
            // Load previous messages for this item
            var messages = await _chatMessageRepository.GetMessagesByItemIdAsync(itemId);
            var messageList = messages.ToList();
            
            // Send previous messages to the user
            foreach (var message in messageList)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", message.Sender, message.Message, message.SentAt);
            }
        }

        public async Task SendMessage(string senderName, string message, int itemId, int senderUserId)
        {
            // Verify the item exists
            var item = await _itemRepository.GetItemByIdAsync(itemId);
            if (item == null)
                return;

            // Save message to database
            var chatMessage = new ChatMessage
            {
                Sender = senderName,
                Message = message,
                ItemId = itemId,
                SentAt = System.DateTime.UtcNow
            };
            await _chatMessageRepository.CreateMessageAsync(chatMessage);

            // Broadcast message only to users in this item's chat group
            await Clients.Group($"Item_{itemId}").SendAsync("ReceiveMessage", senderName, message, chatMessage.SentAt);
        }

        public async Task LeaveItemChat(int itemId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Item_{itemId}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Clean up when user disconnects
            await base.OnDisconnectedAsync(exception);
        }
    }
} 