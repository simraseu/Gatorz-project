using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Gotorz.Data;
using Gotorz.Models;

namespace Gotorz.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ChatHub(ILogger<ChatHub> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                Console.WriteLine($"User connected: {user.Identity.Name}");
            }
            else
            {
                Console.WriteLine("Unauthenticated user tried to connect.");
            }
            await base.OnConnectedAsync();
        }


        public async Task JoinDestinationGroup(string destination)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"destination_{destination}");
            await Clients.Group($"destination_{destination}")
                .SendAsync("UserJoined", Context.User.Identity.Name, destination);
        }

        [Authorize]
        public async Task SendMessageToDestination(string destination, string message)
        {
            var user = await _userManager.GetUserAsync(Context.User);

            if (user == null)
            {
                _logger.LogWarning("SendMessageToDestination: Authenticated user not found. Context User Identity: {UserIdentity}, Claims: {Claims}",
                    Context.User?.Identity?.Name ?? "null",
                    Context.User?.Claims.Select(c => $"{c.Type}={c.Value}").ToList() ?? new List<string>());

                throw new InvalidOperationException("Authenticated user not found.");
            }

            _logger.LogInformation("SendMessageToDestination: User {UserName} ({UserId}) sending message to destination '{Destination}'. Message: {Message}",
                user.UserName, user.Id, destination, message);

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