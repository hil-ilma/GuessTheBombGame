Guess the Bomb Game

Introduction
Welcome to the "Guess the Bomb" game! This interactive game is built using 
SplashKit SDK and demonstrates fundamental Object-Oriented Programming (OOP) concepts. 
The game combines grid-based gameplay with database integration for a persistent leaderboard, 
making it a fun and educational resource for students learning C#.

Objective
The goal of the game is to avoid clicking on the hidden bomb while selecting safe boxes within a time limit. 
The game includes features like dynamic grid sizes, a real-time timer, and a leaderboard to track the best players.
Video tutorial on detailed datbase integration with splashkit program
https://deakin.au.panopto.com/Panopto/Pages/Viewer.aspx?id=9fafdea1-3df0-4e22-933f-b27500490830
Setup Instructions
01. Install SplashKit
1. Visit the [SplashKit website](https://splashkit.io/).
2. Follow the instructions to install SplashKit for your operating system.
3. Ensure your development environment (e.g., Visual Studio or Visual Studio Code) is configured for C# and SplashKit.

02. Clone the Repository
1. Clone this repository to your local machine:
   ```bash
   git clone https://github.com/yourusername/GuessTheBombGame.git
   cd GuessTheBombGame
   ```
2. Open the project in your preferred IDE.

03. Database Setup (SQLite)
1. The game automatically creates a `leaderboard.db` file in the project directory if it doesn't exist.
2. No additional setup is required for the database.
3. To inspect or modify the database, use tools like **DB Browser for SQLite**.

04. Run the Game
1. Build and run the project in your IDE.
2. Follow the on-screen instructions to play the game or view the leaderboard.

Features
1. Dynamic Grid Size:
   - Choose a grid size between 3x3 and 10x10 for varying difficulty.
2. Real-Time Timer:
   - A 30-second timer challenges players to act quickly.
3. Leaderboard:
   - Tracks the top 10 players based on time taken and displays their names, grid sizes, and results.
4. Main Menu:
   - Options to play the game, view the leaderboard, or quit.
5. Game Over Messages:
   - Displays dynamic messages based on whether the timer runs out or the bomb is clicked.
6. Database Integration:
   - Saves player results to a persistent SQLite database.

Code Walkthrough
1. `Box` Class
- Represents an individual grid box.
- Key Properties:
  - `X` and `Y`: Position of the box on the screen.
  - `HasBomb`: Indicates if the box contains a bomb.
  - `Revealed`: Tracks whether the box has been clicked.
- Key Methods:
  - `Draw()`: Colors the box based on its state (safe, bomb, or unrevealed).
  - `IsClicked()`: Determines if a box was clicked by the player.

2. `Game` Class
- Manages the game logic and interactions.
- Key Methods:
  - `InitializeGrid()`: Sets up the grid with one randomly placed bomb.
  - `SaveResult()`: Saves the player's result to the SQLite database.
  - `Run()`: The main game loop, which handles timer logic, player interactions, and rendering.

3. Database Integration
- The SQLite database (`leaderboard.db`) is used to store player names, grid sizes, time taken, and results.
- Key Queries:
  - Create Table:
    ```sql
    CREATE TABLE Leaderboard (
        ID INTEGER PRIMARY KEY AUTOINCREMENT,
        PlayerName TEXT NOT NULL,
        GridSize INTEGER NOT NULL,
        TimeTaken REAL NOT NULL,
        Result TEXT NOT NULL
    );
    ```
  - Insert Results:
    ```sql
    INSERT INTO Leaderboard (PlayerName, GridSize, TimeTaken, Result) 
    VALUES (@Name, @Grid, @Time, @Result);
    ```
  - Retrieve Top Scores:
    ```sql
    SELECT PlayerName, GridSize, TimeTaken, Result 
    FROM Leaderboard 
    ORDER BY TimeTaken ASC 
    LIMIT 10;
    ```

OOP Concepts
1. Encapsulation:
   - The `Box` and `Game` classes encapsulate their data and behavior.
   - Grid initialization, result saving, and gameplay logic are encapsulated in `Game`.
2. Composition:
   - The `Game` class uses a 2D array of `Box` objects to represent the grid.
3. Polymorphism:
   - The `Draw()` method in the `Box` class changes its behavior based on the state of the box.
4. Flow of Control:
   - The `while` loop in the `Run()` method manages the game flow, with conditionals checking for game-over scenarios.

Special Features
1. Timer Logic:
   - Tracks elapsed time using SplashKit's `Timer` object and displays the remaining time on the screen.
2. Database Integration:
   - Results are saved in SQLite, enabling a persistent leaderboard that is accessible across game sessions.
3. Graphical User Interface:
   - Interactive main menu and leaderboard windows built using SplashKit's graphics capabilities.
4. Dynamic Gameplay:
   - Players can customize the difficulty by choosing the grid size.

Screenshots
![Game window](https://github.com/user-attachments/assets/0a4b829c-81cf-4a16-bbd1-03e8f9829590)
![Leaderbaord](https://github.com/user-attachments/assets/715ba1c5-6ae7-4a0b-9cda-7f95c1c570d3)

Future Improvements
- Add multiple bombs for higher difficulty levels.
- Implement a scoring system based on the number of safe boxes revealed.
- Add animations or sound effects for a more engaging experience.
- Explore online multiplayer functionality (splashkit networking).

License
This project is open-source.

