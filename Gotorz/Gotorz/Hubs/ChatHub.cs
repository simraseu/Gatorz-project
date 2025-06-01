using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Gotorz.Data;
using Gotorz.Models;

namespace Gotorz.Hubs
{
    [Authorize(Roles = "Customer")]
    public class ChatHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ChatHub(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task JoinDestinationGroup(string destination)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"destination_{destination}");
            await Clients.Group($"destination_{destination}")
                .SendAsync("UserJoined", Context.User.Identity.Name, destination);
        }

        public async Task SendMessageToDestination(string destination, string message)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            var chatMessage = new ChatMessage
            {
                SenderId = user.Id,
                SenderName = $"{user.FirstName} {user.LastName}",
                Message = message,
                Destination = destination,
                Timestamp = DateTime.UtcNow,
                MessageType = ChatMessageType.DestinationGroup
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            await Clients.Group($"destination_{destination}")
                .SendAsync("ReceiveDestinationMessage", chatMessage.SenderName, message, chatMessage.Timestamp);
        }

        public async Task SendPrivateMessage(string recipientUserId, string message)
        {
            var sender = await _userManager.GetUserAsync(Context.User);

            var chatMessage = new ChatMessage
            {
                SenderId = sender.Id,
                SenderName = $"{sender.FirstName} {sender.LastName}",
                RecipientId = recipientUserId,
                Message = message,
                Timestamp = DateTime.UtcNow,
                MessageType = ChatMessageType.Private
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            await Clients.User(recipientUserId)
                .SendAsync("ReceivePrivateMessage", chatMessage.SenderName, message, chatMessage.Timestamp, sender.Id);
        }
    }
}