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
            // Get the item to find the owner
            var item = await _itemRepository.GetItemByIdAsync(itemId);
            if (item == null || !item.UserId.HasValue)
                return;

            int itemOwnerId = item.UserId.Value;
            
            // Add user to their personal group for this item
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Item_{itemId}_User_{userId}");
            
            // If user is not the owner, also add them to the owner's group
            if (userId != itemOwnerId)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Item_{itemId}_User_{itemOwnerId}");
            }
            
            // Load conversation between these two users for this item
            var messages = await _chatMessageRepository.GetConversationBetweenUsersAsync(itemId, userId, itemOwnerId);
            var messageList = messages.ToList();
            
            // Send previous messages to the user
            foreach (var message in messageList)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", message.Sender, message.Message, message.SentAt);
            }
        }

        public async Task JoinOwnerChat(int itemId, int ownerId, int selectedUserId)
        {
            // Get the item to verify ownership
            var item = await _itemRepository.GetItemByIdAsync(itemId);
            if (item == null || !item.UserId.HasValue || item.UserId.Value != ownerId)
                return;

            // Add owner to both their own group and the selected user's group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Item_{itemId}_User_{ownerId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Item_{itemId}_User_{selectedUserId}");
            
            // Load conversation between owner and selected user for this item
            var messages = await _chatMessageRepository.GetConversationBetweenUsersAsync(itemId, ownerId, selectedUserId);
            var messageList = messages.ToList();
            
            // Send previous messages to the owner
            foreach (var message in messageList)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", message.Sender, message.Message, message.SentAt);
            }
        }

        public async Task SendMessage(string senderName, string message, int itemId, int senderUserId, int? explicitReceiverId = null)
        {
            try
            {
                Console.WriteLine($"SendMessage called: senderName={senderName}, message={message}, itemId={itemId}, senderUserId={senderUserId}");
                
                if (senderUserId == 0 || itemId == 0)
                {
                    Console.WriteLine($"Invalid senderUserId ({senderUserId}) or itemId ({itemId})");
                    return;
                }
                
                Console.WriteLine($"Getting item with ID: {itemId}");
                var item = await _itemRepository.GetItemByIdAsync(itemId);
                if (item == null || !item.UserId.HasValue)
                {
                    Console.WriteLine($"Item not found or item.UserId is null for itemId={itemId}");
                    return;
                }
                
                int itemOwnerId = item.UserId.Value;
                Console.WriteLine($"Item owner ID: {itemOwnerId}");
                
                int receiverId;

                if (explicitReceiverId.HasValue)
                {
                    receiverId = explicitReceiverId.Value;
                    Console.WriteLine($"Using explicit receiver ID: {receiverId}");
                }
                else if (senderUserId == itemOwnerId)
                {
                    // Owner must specify a user to reply to
                    Console.WriteLine($"Owner must specify a user to reply to.");
                    return;
                }
                else
                {
                    receiverId = itemOwnerId;
                    Console.WriteLine($"Using item owner as receiver ID: {receiverId}");
                }
                
                Console.WriteLine($"Creating chat message with SenderId={senderUserId}, ReceiverId={receiverId}");
                var chatMessage = new ChatMessage
                {
                    Sender = senderName,
                    Message = message,
                    ItemId = itemId,
                    SenderId = senderUserId,
                    ReceiverId = receiverId,
                    SentAt = System.DateTime.UtcNow
                };
                
                Console.WriteLine($"Saving message to database...");
                await _chatMessageRepository.CreateMessageAsync(chatMessage);
                Console.WriteLine($"Message saved successfully with ID: {chatMessage.Id}");
                
                // Send to sender only once
                await Clients.Caller.SendAsync("ReceiveMessage", senderName, message, chatMessage.SentAt);
                
                if (senderUserId != receiverId)
                {
                    Console.WriteLine($"Sending to receiver group: Item_{itemId}_User_{receiverId}");
                    await Clients.Group($"Item_{itemId}_User_{receiverId}").SendAsync("ReceiveMessage", senderName, message, chatMessage.SentAt);
                }
                
                Console.WriteLine($"SendMessage completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendMessage: {ex}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task LeaveItemChat(int itemId, int userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Item_{itemId}_User_{userId}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Clean up when user disconnects
            await base.OnDisconnectedAsync(exception);
        }
    }
} 