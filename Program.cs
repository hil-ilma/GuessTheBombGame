using SplashKitSDK;    
using System.Data.SQLite; 
   

namespace Bomb
{
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

