using Microsoft.AspNetCore.SignalR;

namespace TresEnRayaBackend.Service.Hubs;

public class GameHub : Hub
{
    private static int _playerCount = 0;
    private static readonly object _lock = new();

    public override async Task OnConnectedAsync()
    {
        int currentPlayer;

        lock (_lock)
        {
            _playerCount++;
            currentPlayer = _playerCount;
        }

        if (currentPlayer == 1)
        {
            await Clients.Caller.SendAsync("PlayerAssignment", "X");
        }
        else if (currentPlayer == 2)
        {
            await Clients.Caller.SendAsync("PlayerAssignment", "O");
            await Clients.All.SendAsync("StartGame", "X");
        }
        else
        {
            await Clients.Caller.SendAsync("ConnectionRejected", "La partida ya está llena.");
            Context.Abort();
            return;
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        lock (_lock)
        {
            _playerCount--;
            if (_playerCount < 0) _playerCount = 0;
        }

        await Clients.Others.SendAsync("OpponentDisconnected");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task MakeMove(int row, int col, string symbol)
    {
        await Clients.Others.SendAsync("ReceiveMove", row, col, symbol);
    }
}