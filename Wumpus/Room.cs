using System.Text.Json.Serialization;

namespace Wumpus;

public class Room
{
    public required int Id { get; set; }
    public required string Description { get; set; }
    public required Dictionary<string, int> Exits { get; set; }
    public required bool Hazard { get; set; }
}