using System;

namespace gaton.Model;

public class RoomModel
{
    public string? RoomId { get; set; }
    public List<string> Players { get; } = new();
    public bool GameStarted => Players.Count == 2;
    public string CreatedBy { get; set; } = string.Empty;
}
