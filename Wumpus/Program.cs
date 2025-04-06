namespace Wumpus
{
    class Program
    {
        static async Task<GameState> NewGameAsync()
        {
            List<Room> rooms = await AiWrapper.GenerateDungeonAsync();
            


            return new GameState
            {
                Map = rooms,
                Player = new Player(),
                WumpusRoom = rooms.Count-1,
                GameOver = false,
                GameWon = false
            };
        }
        
        

        static async void MovePlayer(string direction, GameState state)
        {
            var currentRoom = state.Map[state.Player.CurrentRoom];
            if (!currentRoom.Exits.TryGetValue(direction, out int newRoomId))
            {
                Console.WriteLine("You can't go that way!");
                return;
            }

            state.Player.CurrentRoom = newRoomId;
            var newRoom = state.Map[newRoomId];

            if (newRoom.Hazard)
            {
                Console.WriteLine(AiWrapper.GenerateHazard(currentRoom.Description).Result);
                state.GameOver = true;
            }

            if (state.Player.CurrentRoom == state.WumpusRoom)
            {
                Console.WriteLine("You walked into the Wumpus! Game over.");
                state.GameOver = true;
            }
        }


        static void ShootArrow(string direction, GameState state)
        {
            if (state.Player.Arrows <= 0)
            {
                Console.WriteLine("You have no arrows left!");
                return;
            }

            var currentRoom = state.Map[state.Player.CurrentRoom];
            if (!currentRoom.Exits.TryGetValue(direction, out int firstRoomId))
            {
                Console.WriteLine("You can't shoot that way!");
                return;
            }

            var arrowPath = new List<int> { firstRoomId };
            while (state.Map[arrowPath[^1]].Exits.TryGetValue(direction, out int nextRoomId))
            {
                arrowPath.Add(nextRoomId);
            }

            if (arrowPath.Contains(state.WumpusRoom))
            {
                Console.WriteLine("You hear a roar! The Wumpus is dead. You win!");
                state.GameOver = true;
                state.GameWon = true;
            }
            else
            {
                Console.WriteLine("The arrow clatters into the distance.");
                state.Player.Arrows--;

                if (new Random().NextDouble() < 0.75)
                {
                    var wumpusRoom = state.Map[state.WumpusRoom];
                    var possibleMoves = new List<int>(wumpusRoom.Exits.Values) { state.WumpusRoom };
                    state.WumpusRoom = possibleMoves[new Random().Next(possibleMoves.Count)];
                    Console.WriteLine("You hear a growl as the Wumpus moves!");
                }
            }
        }

        static void GameLoop(GameState state)
        {
            while (true)
            {
                if (state.GameOver) break;
                
                var currentRoom = state.Map[state.Player.CurrentRoom];
                
                Console.WriteLine($"\n{currentRoom.Description}");
                Console.WriteLine($"Exits: {string.Join(", ", currentRoom.Exits.Keys)}");
                
                Console.WriteLine("\nActions: 1. Move 2. Shoot 3. Save 4. Quit");
                Console.Write("Choose action (1-4): ");
                var choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        Console.Write("Direction: ");
                        var dir = Console.ReadLine()?.ToLower();
                        if (!string.IsNullOrEmpty(dir)) MovePlayer(dir, state);
                        break;
                    case "2":
                        Console.Write("Direction: ");
                        var shootDir = Console.ReadLine()?.ToLower();
                        if (!string.IsNullOrEmpty(shootDir)) ShootArrow(shootDir, state);
                        break;
                    case "3":
                        Console.Write("Filename: ");
                        var file = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(file)) state.Save(file);
                        break;
                    case "4":
                        state.GameOver = true;
                        break;
                    default:
                        Console.WriteLine("Invalid choice");
                        break;
                }
            }

            Console.WriteLine(state.GameWon ? "\nYou won!" : "\nGame Over!");
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to Hunt the Wumpus!");
            Console.Write("New game (n) or load (l)? ");
            var choice = Console.ReadLine()?.ToLower();

            GameState state;
            if (choice == "l")
            {
                Console.Write("Save filename: ");
                try { state = GameState.Load(Console.ReadLine() ?? "save.json"); }
                catch
                {
                    Console.WriteLine("Loading failed. Starting new game.");
                    state = await NewGameAsync();
                }
            }
            else
            {
                state = await NewGameAsync();
            }

            GameLoop(state);
        }
    }
}