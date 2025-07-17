using System;

namespace gaton.Model;

public class EntryModel
{
    public string? Action_Type { get; set; }
    public Payload? Value { get; set; }
}

public class Payload
{
    public string? Name { get; set; }
    public string? RoomId { get; set; }
    public string? MoveData { get; set; }
    public bool? RematchAccepted { get; set; }
}
