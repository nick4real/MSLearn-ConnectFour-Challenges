using Microsoft.AspNetCore.SignalR;
using System.Text;

namespace ConnectFour.Hubs
{
    public class GameHub : Hub
    {
        private Dictionary<string, HashSet<string>> groups = new Dictionary<string, HashSet<string>>();
        private readonly Random random = new Random();
        private const int maxGroupSize = 2;

        private readonly char[] characters = new char[62];
        private readonly Random _rnd = new(); 

        public GameHub() : base()
        {
            int index = 0;

            for (char c = 'A'; c <= 'Z'; c++) characters[index++] = c;
            for (char c = 'a'; c <= 'z'; c++) characters[index++] = c;
            for (char c = '0'; c <= '9'; c++) characters[index++] = c;
        }

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
            var lobby = await GenerateCode();
            while (groups.ContainsKey(lobby))
                lobby = await GenerateCode();

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

        private async Task<string> GenerateCode(int length = 5)
        {
            StringBuilder res = new();

            for (int i = 0; i < length; i++)
            {
                res.Append(characters[_rnd.Next(62)]);
            }

            return res.ToString();
        }
    }
}