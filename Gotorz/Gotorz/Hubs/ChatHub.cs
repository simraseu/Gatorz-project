using Gotorz.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Gotorz.Models;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChatHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    private static HashSet<string> OnlineUsers = new();

    public override async Task OnConnectedAsync()
    {
        var user = Context.User?.Identity?.Name ?? Context.ConnectionId;
        OnlineUsers.Add(user);
        await Clients.All.SendAsync("UpdateUserList", OnlineUsers.ToList());
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = Context.User?.Identity?.Name ?? Context.ConnectionId;
        OnlineUsers.Remove(user);
        await Clients.All.SendAsync("UpdateUserList", OnlineUsers.ToList());
        await base.OnDisconnectedAsync(exception);
    }


    public async Task SendMessage(string user, string message)
    {
        // Gem besked i database
        _context.ChatMessages.Add(new ChatMessage
        {
            SenderUsername = user,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Send til alle klienter
        await Clients.All.SendAsync("ReceiveMessage", user, message, DateTime.UtcNow);
    }
} 