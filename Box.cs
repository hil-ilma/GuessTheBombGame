using SplashKitSDK;
using System;      
using System.Data.SQLite; 
using System.IO;   

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