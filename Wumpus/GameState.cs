using System.Text.Json;

namespace Wumpus;

public class GameState
{
    public List<Room> Map { get; set; }
    public Player Player { get; set; } = new Player();
    public int WumpusRoom { get; set; }
    public bool GameOver { get; set; }
    public bool GameWon { get; set; }

    public void Save(string filename)
    {
        var options = new JsonSerializerOptions();
        var json = JsonSerializer.Serialize(this, options);
        Console.WriteLine(json);
        File.WriteAllText(filename, json);
    }

    public static GameState Load(string filename)
    {
        var json = File.ReadAllText(filename);
        return JsonSerializer.Deserialize<GameState>(json) 
               ?? throw new InvalidOperationException("Invalid save file");
    }
}