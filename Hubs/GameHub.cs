using Microsoft.AspNetCore.SignalR;

namespace ConnectFour.Hubs
{
    public class GameHub : Hub
    {
        private Dictionary<string, HashSet<string>> groups = new Dictionary<string, HashSet<string>>();
        private readonly Random random = new Random();
        private const int maxGroupSize = 2;

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"New connection: " + Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Disconnect: " + Context.ConnectionId);
            foreach (var group in groups)
            {
                if (group.Value.Contains(Context.ConnectionId))
                {
                    foreach (var con in group.Value)
                    {
                        Groups.RemoveFromGroupAsync(con, group.Key);
                    }
                    groups.Remove(group.Key);
                    break;
                }
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task<string> CreateLobby()
        {
            var lobby = Context.ConnectionId.Substring(0, 5);
            await Groups.AddToGroupAsync(Context.ConnectionId, lobby);
            _ = groups.TryAdd(lobby, new HashSet<string>());
            groups[lobby].Add(Context.ConnectionId);
            await Console.Out.WriteLineAsync("Lobby created: " + lobby);
            return lobby;
        }

        public async Task<bool> JoinLobby(string lobby)
        {
            if (groups.TryGetValue(lobby, out HashSet<string> group))
            {
                if (group.Contains(Context.ConnectionId))
                    return true;
            }
                
            if (!groups.ContainsKey(lobby) || groups[lobby].Count >= maxGroupSize)
                return false;

            groups[lobby].Add(Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, lobby);

            await Console.Out.WriteLineAsync($"Client {Context.ConnectionId} connected to lobby {lobby}");
            return true;
        }

        public async Task<bool> StartGame(string lobby)
        {
            if (!groups.ContainsKey(lobby)) 
                return false;

            if (groups[lobby].Count == maxGroupSize)
            {
                byte player1 = (byte)random.Next(2);
                byte player2 = (byte)(1 - player1);

                await Clients.OthersInGroup(lobby).SendAsync("FirstTurnResolve", player1);
                await Clients.Caller.SendAsync("FirstTurnResolve", player2);
            }

            return true;
        }

        public async Task CloseLobby(string lobby)
        {
            await Clients.OthersInGroup(lobby).SendAsync("OpponentDisconnect");
        }

        public async Task PlayPiece(byte col, string lobby)
        {
            await Clients.OthersInGroup(lobby).SendAsync("GetPiece", col);
        }
    }
}