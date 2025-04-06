using System.Text.Json;
using Microsoft.Extensions.AI;

namespace Wumpus;

static class AiWrapper
{
    private static readonly OllamaChatClient ChatClient = new(
        new Uri("http://localhost:11434"), 
        "llama3",
        new HttpClient()
        
    );

    private static readonly List<ChatMessage> ChatHistory = new();

    public static async Task<string> PromptAI(string prompt)
    {
        ChatHistory.Add(new ChatMessage(ChatRole.User, prompt));
        var result = await ChatClient.GetResponseAsync(ChatHistory);
        ChatHistory.Add(new ChatMessage(ChatRole.Assistant, result.Text));
        return result.Text;
    }
        
    public static async Task<List<Room>> GenerateDungeonAsync()
    {
        Console.WriteLine("ai is generating rooms, this might take multiple tries");

        while (true)
        {
            try
            {
                const string prompt = @"Generate a dungeon layout for a Wumpus game with 4-6 rooms in JSON format. Do not provide any output except the JSON object. 
            Each room should have:
            - id (unique number)
            - description of the room, providing a hit of where to go or not to go (provide a specific direction with the hint)
            - exits (dictionary of direction to room ID)
            - Hazard - boolean determining whether there is a hazard or not
            
            Example response:
            {
                ""Rooms"": [
                    {
                        ""Id"": 0,
                        ""Description"": ""A dank cave with glowing fungi"",
                        ""Exits"": { ""north"": 2, ""east"": 3 },
                        ""Hazard"": false
                    },
                    {
                        ""Id"": 1,
                        ""Description"": ""A narrow ledge overlooking a chasm"",
                        ""Exits"": { ""south"": 1 },
                        ""Hazard"": true
                    }
                ]
            }";

                var response = await ChatClient.GetResponseAsync(new[]
                {
                    new ChatMessage(ChatRole.User, prompt)
                });

                var result = JsonSerializer.Deserialize<DungeonResponse>(
                    response.Text,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );


                if (result != null)
                {
                    foreach (var room in result.Rooms)
                    {
                        if (room.Exits == null || room.Id == null || room.Description == null || room.Hazard == null)
                        {
                            throw new Exception();
                        }
                    }

                    return result.Rooms;
                }
            }
            catch
            {
                Console.WriteLine("ai failed to generate rooms, retrying");
            }
        }

    }

    public static async Task<string> GenerateHazard(string description)
    {
        string prompt = $@"Generate a hazard for a room described as: {description}. give me only a description of the hazard that the player has just walked into";
                
        string response = await PromptAI(prompt);

        return response;
    
    }
}

class DungeonResponse
{
    public List<Room> Rooms { get; set; }
}