using SplashKitSDK;
using System;      
using System.Data.SQLite; 
using System.IO;    

namespace Bomb
{
    // Represents an individual grid box
    public class Box
    {
        // Properties of the box: position, bomb status, reveal state, and size
        public double X { get; set; }
        public double Y { get; set; }
        
        // Indicates if the box contains a bomb
        public bool HasBomb { get; set; } 
        
        // Indicates if the box has been clicked
        public bool Revealed { get; set; } 
        
        // Fixed size for all boxes
        public static int Size = 100; 

        // Constructor to initialize the box with its position and bomb status
        public Box(double x, double y, bool hasBomb)
        {
            X = x;
            Y = y;
            HasBomb = hasBomb;
            
            // Default state is unrevealed
            Revealed = false; 
        }

        // Draws the box on the game window
        public void Draw()
        {
            // If revealed, color red for bomb or green for safe
            if (Revealed)
            {
                SplashKit.FillRectangle(HasBomb ? Color.Red : Color.Green, X, Y, Size, Size);
            }
            else
            {
                // Default color for unrevealed boxes
                SplashKit.FillRectangle(Color.Gray, X, Y, Size, Size);
            }

            // Draws the border of the box
            SplashKit.DrawRectangle(Color.Black, X, Y, Size, Size);
        }

        // Determines if the box was clicked based on mouse coordinates
        public bool IsClicked(double mouseX, double mouseY)
        {
            return mouseX >= X && mouseX <= X + Size && mouseY >= Y && mouseY <= Y + Size;
        }
    }

    // Manages the game logic and interactions
    public class Game
    {
        // 2D array of boxes representing the grid
        private Box[,] grid; 

        // Size of the grid (e.g., 3x3, 4x4, etc.)
        private int gridSize; 

        // Random generator for bomb placement
        private Random random; 

        // Tracks if the game is over
        private bool gameOver; 

        // Stores the name of the current player
        private string playerName; 

        // Total time for the game in milliseconds
        private const double TimerDuration = 30000; 

        // Name of the SQLite database file
        private string databaseFile = "leaderboard.db"; 

        // Constructor to initialize the game with player name and grid size
        public Game(string playerName, int gridSize)
        {
            // Remove extra spaces from the player name
            this.playerName = playerName.Trim(); 
            
            // Set the grid size
            this.gridSize = gridSize; 
            
            // Initialize the grid
            grid = new Box[gridSize, gridSize]; 
            
            // Initialize the random generator
            random = new Random(); 
            
            // Populate the grid with boxes
            InitializeGrid(); 
            
            // Ensure the database exists and is ready
            InitializeDatabase(); 
        }

        // Initializes the grid and randomly places the bomb
        private void InitializeGrid()
        {
            // Random x-coordinate for the bomb
            int bombX = random.Next(gridSize); 
            
            // Random y-coordinate for the bomb
            int bombY = random.Next(gridSize); 

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    // Place bomb in one random box
                    bool hasBomb = (i == bombX && j == bombY); 
                    grid[i, j] = new Box(i * Box.Size + 50, j * Box.Size + 50, hasBomb);
                }
            }
        }

        // Initializes the SQLite database and creates the leaderboard table if it doesn't exist
        private void InitializeDatabase()
        {
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={databaseFile}"))
            {
                conn.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name='Leaderboard';";
                SQLiteCommand command = new SQLiteCommand(query, conn);
                
                // Check if the table exists
                var result = command.ExecuteScalar(); 
                if (result == null)
                {
                    // Create the leaderboard table
                    string createQuery = @"CREATE TABLE Leaderboard (
                                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                                            PlayerName TEXT NOT NULL,
                                            GridSize INTEGER NOT NULL,
                                            TimeTaken REAL NOT NULL,
                                            Result TEXT NOT NULL
                                          )";
                    SQLiteCommand createCommand = new SQLiteCommand(createQuery, conn);
                    createCommand.ExecuteNonQuery();
                }
            }
        }

        // Saves the player's result to the leaderboard
        public void SaveResult(double timeTaken, string result)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection($"Data Source={databaseFile}"))
                {
                    conn.Open();
                    string query = "INSERT INTO Leaderboard (PlayerName, GridSize, TimeTaken, Result) VALUES (@Name, @Grid, @Time, @Result)";
                    SQLiteCommand command = new SQLiteCommand(query, conn);

                    // Parameterized query to prevent SQL injection
                    command.Parameters.AddWithValue("@Name", playerName);
                    command.Parameters.AddWithValue("@Grid", gridSize);
                    command.Parameters.AddWithValue("@Time", timeTaken / 1000); 
                    command.Parameters.AddWithValue("@Result", result);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
            }
        }

        // Runs the game loop
        public void Run()
        {
            // Load a font for text rendering
            Font font = SplashKit.LoadFont("Arial", "arial.ttf"); 
            
            // Adjust window width based on grid size
            int windowWidth = Math.Max(400, gridSize * Box.Size + 100); 
            
            // Adjust window height based on grid size
            int windowHeight = Math.Max(400, gridSize * Box.Size + 100);
            Window window = new Window($"Guess the Bomb - Player: {playerName}", windowWidth, windowHeight);
            
            // Create a timer
            SplashKitSDK.Timer gameTimer = new SplashKitSDK.Timer("GameTimer"); 
            
            // Start the timer
            gameTimer.Start(); 
            
            // Main game loop
            while (!window.CloseRequested && !gameOver) 
            {
                // Process user input
                SplashKit.ProcessEvents(); 
                
                // Clear the screen for the next frame
                SplashKit.ClearScreen(Color.White); 
                
                // Get elapsed time
                double elapsedTime = gameTimer.Ticks; 
                
                // Calculate time left
                double timeLeft = Math.Max(0, (TimerDuration - elapsedTime) / 1000); 

                // Handle timer expiration
                if (elapsedTime >= TimerDuration && !gameOver)
                {
                    gameOver = true;
                    RenderGameOver(window, $"Time's Up! Game Over!\nTime Taken: {elapsedTime / 1000:F2} seconds", font);
                    SaveResult(TimerDuration, "Loss (Time Up)");
                }

                // Draw the timer on the screen
                SplashKit.DrawText($"Time Left: {timeLeft:F2}s", Color.Black, font, 20, 10, 10);

                // Draw the grid of boxes
                foreach (Box box in grid)
                {
                    box.Draw();
                }

                // Handle mouse clicks on the grid
                if (SplashKit.MouseClicked(MouseButton.LeftButton))
                {
                    double mouseX = SplashKit.MouseX();
                    double mouseY = SplashKit.MouseY();

                    foreach (Box box in grid)
                    {
                        if (box.IsClicked(mouseX, mouseY) && !box.Revealed)
                        {
                            // Mark the box as revealed
                            box.Revealed = true; 
                            
                            // Update the display for the box
                            box.Draw(); 

                            // Check if the box has a bomb
                            if (box.HasBomb) 
                            {
                                gameOver = true;
                                RenderGameOver(window, $"Bomb Found! Game Over! Time Taken: {elapsedTime / 1000:F2} seconds", font);
                                SaveResult(elapsedTime, "Loss (Bomb Found)");
                            }
                        }
                    }
                }

                SplashKit.RefreshScreen(60); 
            }

            // Cleanup resources after the game
            PerformFinalCleanup(window, gameTimer); 
        }

        // Displays a game-over message
        private void RenderGameOver(Window window, string message, Font font)
        {
            // Clear the screen
            SplashKit.ClearScreen(Color.White); 
            
            // Display the message
            SplashKit.DrawText(message, Color.Red, font, 12, 50, 50); 
            
            // Refresh the screen
            SplashKit.RefreshScreen(60); 
            
            // Pause for 3 seconds
            SplashKit.Delay(3000); 
        }

        // Cleanup after the game ends
        private void PerformFinalCleanup(Window window, SplashKitSDK.Timer gameTimer)
        {
            SplashKit.ClearScreen(Color.White);
            SplashKit.RefreshScreen(60);
            
            // Brief pause before closing
            SplashKit.Delay(500); 
            gameTimer.Stop();
            
            // Close the game window
            window.Close(); 
        }
    }

    // Main entry point for the program
    public class Program
    {
        public static void Main()
        {
            while (true)
            {
                // Main Menu for the game
                Window menuWindow = new Window("Main Menu", 400, 400);

                while (!menuWindow.CloseRequested)
                {
                    SplashKit.ProcessEvents();
                    SplashKit.ClearScreen(Color.White);

                    // Draw main menu options
                    SplashKit.DrawText("Main Menu", Color.Black, "Arial", 24, 140, 50);
                    SplashKit.DrawText("Press 'P' to Play Game", Color.Black, "Arial", 16, 50, 120);
                    SplashKit.DrawText("Press 'L' to View Leaderboard", Color.Black, "Arial", 16, 50, 150);
                    SplashKit.DrawText("Press 'Q' to Quit", Color.Black, "Arial", 16, 50, 180);

                    if (SplashKit.KeyTyped(KeyCode.PKey))
                    {
                        // Start the game
                        string playerName = TextInput(menuWindow, "Enter Player Name:", 20);
                        string gridSizeInput = TextInput(menuWindow, "Enter Grid Size (3-10):", 2);
                        if (int.TryParse(gridSizeInput, out int gridSize) && gridSize >= 3 && gridSize <= 10)
                        {
                            menuWindow.Close();
                            Game game = new Game(playerName, gridSize);
                            game.Run();
                        }
                        else
                        {
                            SplashKit.DrawText("Invalid Grid Size. Try Again.", Color.Red, "Arial", 14, 50, 200);
                            SplashKit.Delay(2000);
                        }
                    }
                    else if (SplashKit.KeyTyped(KeyCode.LKey))
                    {
                        menuWindow.Close();
                        
                        // Check if returning to the main menu
                        if (ShowLeaderboard()) 
                        {
                            // Break out of this loop and restart main menu
                            break; 
                        }
                    }
                    else if (SplashKit.KeyTyped(KeyCode.QKey))
                    {
                        Console.WriteLine("Goodbye!");
                        
                        // Exit the program
                        return; 
                    }

                    SplashKit.RefreshScreen(60);
                }

                // Ensure the menu window closes
                menuWindow.Close(); 
            }
        }


        // Helper method to capture text input from the player
        private static string TextInput(Window window, string prompt, int maxLength)
        {
            // Initialize input string
            string input = ""; 

            while (!window.CloseRequested)
            {
                SplashKit.ProcessEvents();
                SplashKit.ClearScreen(Color.White);

                // Display the prompt and current input
                SplashKit.DrawText(prompt, Color.Black, "Arial", 16, 10, 10);
                SplashKit.DrawText(input, Color.Blue, "Arial", 16, 10, 40);
                
                // Finish input on Enter
                if (SplashKit.KeyTyped(KeyCode.ReturnKey)) break; 
                if (SplashKit.KeyTyped(KeyCode.BackspaceKey) && input.Length > 0)
                {
                    // Remove the last character
                    input = input.Substring(0, input.Length - 1); 
                }

                foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                {
                    if (SplashKit.KeyTyped(key))
                    {
                        if (key >= KeyCode.AKey && key <= KeyCode.ZKey) // Check for alphabet keys
                        {
                            char keyChar = (char)('A' + (key - KeyCode.AKey));
                            if (SplashKit.KeyDown(KeyCode.LeftShiftKey) || SplashKit.KeyDown(KeyCode.RightShiftKey))
                            {
                                // Add uppercase letter
                                input += keyChar; 
                            }
                            else
                            {
                                // Add lowercase letter
                                input += char.ToLower(keyChar); 
                            }
                        }
                        
                        // Check for number keys
                        else if (key >= KeyCode.Num0Key && key <= KeyCode.Num9Key) 
                        {
                            char keyChar = (char)('0' + (key - KeyCode.Num0Key));
                            input += keyChar;
                        }
                        else if (key == KeyCode.SpaceKey && input.Length < maxLength)
                        {
                            input += ' '; 
                        }
                    }
                }

                SplashKit.RefreshScreen(60); 
            }
            
            // Return the trimmed input
            return input.Trim(); 
        }

        // Displays the leaderboard in a new window
        private static bool ShowLeaderboard()
        {
            Window leaderboardWindow = new Window("Leaderboard", 500, 500);
            
            // Load font for displaying leaderboard
            Font font = SplashKit.LoadFont("Arial", "arial.ttf"); 
            
            // Flag to return to the main menu
            bool backToMenu = false; 

            while (!leaderboardWindow.CloseRequested && !backToMenu)
            {
                SplashKit.ProcessEvents();
                SplashKit.ClearScreen(Color.White);

                // Draw the leaderboard title
                SplashKit.DrawText("Leaderboard", Color.Black, font, 24, 10, 10);
                
                // Vertical offset for entries
                int yOffset = 50; 

                using (SQLiteConnection conn = new SQLiteConnection("Data Source=leaderboard.db"))
                {
                    conn.Open();
                    string query = "SELECT PlayerName, GridSize, TimeTaken, Result FROM Leaderboard ORDER BY TimeTaken ASC LIMIT 10";

                    SQLiteCommand command = new SQLiteCommand(query, conn);
                    SQLiteDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        // Retrieve and format leaderboard data
                        string? playerName = reader["PlayerName"].ToString();
                        int gridSize = Convert.ToInt32(reader["GridSize"]);
                        double timeTaken = Convert.ToDouble(reader["TimeTaken"]);
                        string? result = reader["Result"].ToString();

                        string entry = $"{playerName} | Grid: {gridSize} | Time: {timeTaken:F2}s | {result}";

                        // Draw each leaderboard entry
                        SplashKit.DrawText(entry, Color.Black, font, 14, 10, yOffset);
                        
                        // Increment vertical offset
                        yOffset += 30; 
                    }
                }

                // Draw the "Back to Main Menu" button
                int buttonX = 150; 
                int buttonY = 450; 
                int buttonWidth = 200; 
                int buttonHeight = 40; 

                SplashKit.FillRectangle(Color.LightGray, buttonX, buttonY, buttonWidth, buttonHeight); // Button background
                SplashKit.DrawRectangle(Color.Black, buttonX, buttonY, buttonWidth, buttonHeight); // Button border
                SplashKit.DrawText("Back to Main Menu", Color.Black, font, 16, buttonX + 20, buttonY + 10); // Button text

                // Check if the button is clicked
                if (SplashKit.MouseClicked(MouseButton.LeftButton))
                {
                    double mouseX = SplashKit.MouseX();
                    double mouseY = SplashKit.MouseY();

                    // Detect if the click is inside the button
                    if (mouseX >= buttonX && mouseX <= buttonX + buttonWidth &&
                        mouseY >= buttonY && mouseY <= buttonY + buttonHeight)
                    {
                        backToMenu = true;
                    }
                }

                SplashKit.RefreshScreen(60); 
            }
            // Close the leaderboard window
            leaderboardWindow.Close(); 
            
            // Return to indicate whether to go back to the main menu
            return backToMenu; 
        }
    }
}
