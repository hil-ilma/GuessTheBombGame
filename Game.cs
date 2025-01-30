using SplashKitSDK;
using System;      
using System.Data.SQLite; 
using System.IO; 
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