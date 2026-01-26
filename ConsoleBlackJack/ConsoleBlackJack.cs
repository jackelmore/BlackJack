using System;
using jackel.Cards;

namespace ConsoleBlackJack
{
    /// <summary>
    /// Enum representing player menu options in the interactive game.
    /// </summary>
    enum PlayerOption
    {
        AutoHit = 'A',
        Hit = 'H',
        Stand = 'S',
        PrintHand = 'P',
        PileDump = 'L',
        Quit = 'Q'
    }

    /// <summary>
    /// Manages the console-based BlackJack game experience.
    /// </summary>
    class ConsoleGame
    {
        BlackJack game;
        const int HOUSE_HAND = 0;
        const string PLAYER_MENU = "A=AutoHit, H=Hit, S=Stand, P=PrintHand, L=PileDump, Q=Quit";
        const string SCOREBOARD_HEADER = "Scoreboard Results\n==============================================";
        
        private int numPlayers;

        /// <summary>
        /// Initializes a new instance of the ConsoleGame class.
        /// </summary>
        /// <param name="players">The number of players for the game.</param>
        public ConsoleGame(int players)
        {
            numPlayers = players;
            game = new BlackJack(numPlayers);
        }

        /// <summary>
        /// Runs the specified number of automatic games without player interaction.
        /// </summary>
        /// <param name="numGames">The number of games to play automatically.</param>
        public void AutoGame(int numGames)
        {
            try
            {
                int[,] scoreBoard = new int[numGames, numPlayers + 1];

                for (int numGame = 0; numGame < numGames; numGame++)
                {
                    game.Deal();
                    for (int i = 1; i <= game.numPlayers; i++)
                    {
                        game.AutoHit(i);
                        scoreBoard[numGame, i] = game.GetHandValue(i);
                    }
                    game.AutoHit(HOUSE_HAND);
                    scoreBoard[numGame, HOUSE_HAND] = game.GetHandValue(HOUSE_HAND);
                }

                DisplayScoreboard(scoreBoard, numGames);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during auto game: {ex.Message}");
            }
        }

        /// <summary>
        /// Displays the scoreboard results in a formatted table.
        /// </summary>
        private void DisplayScoreboard(int[,] scoreBoard, int numGames)
        {
            Console.WriteLine(SCOREBOARD_HEADER);
            Console.Write("Game     H ");
            for (int j = 1; j <= numPlayers; j++)
                Console.Write($" | P{j}");
            Console.Write("\n");
            
            for (int i = 0; i < numGames; i++)
            {
                Console.Write($"Game {i,2}: {scoreBoard[i, HOUSE_HAND],2}");
                for (int j = 1; j <= game.numPlayers; j++)
                {
                    Console.Write($" | {scoreBoard[i, j],2}");
                }
                Console.Write("\n");
            }
        }

        /// <summary>
        /// Runs an interactive BlackJack game where players can make decisions.
        /// </summary>
        public void InteractiveGame()
        {
            try
            {
                while (true)
                {
                    game.Deal();

                    Console.WriteLine($"Dealer has card up: {game.Peek(0).ToString()}{Environment.NewLine}");

                    ConsoleKeyInfo keyPress = default;
                    bool validInput = false;

                    for (int i = 1; i <= game.numPlayers; i++)
                    {
                        do
                        {
                            game.PrintHand(i);
                            if (game.GetHandValue(i) == BlackJack.maxHandValue)
                                break;
                            
                            Console.WriteLine($"Player {i}: {PLAYER_MENU}");
                            keyPress = Console.ReadKey(true);
                            validInput = IsValidMenuOption(keyPress.KeyChar);
                            
                            if (!validInput)
                            {
                                Console.WriteLine("Invalid input. Please try again.");
                                continue;
                            }

                            Console.WriteLine();
                            HandlePlayerInput(keyPress, i);
                        } while (ShouldContinueTurn(keyPress, i));
                        
                        if (game.GetHandValue(i) > BlackJack.maxHandValue)
                            Console.WriteLine($"Player {i} (value = {game.GetHandValue(i)}) : BUSTED");
                    }

                    game.AutoHit(HOUSE_HAND);
                    Console.WriteLine();
                    game.PrintHand(HOUSE_HAND);

                    if (game.GetHandValue(HOUSE_HAND) > BlackJack.maxHandValue)
                    {
                        Console.WriteLine("House BUSTS");
                    }

                    DisplayRoundResults();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during interactive game: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the player's menu input.
        /// </summary>
        private void HandlePlayerInput(ConsoleKeyInfo keyPress, int playerNumber)
        {
            PlayerOption option = (PlayerOption)char.ToUpper(keyPress.KeyChar);

            switch (option)
            {
                case PlayerOption.AutoHit:
                    game.AutoHit(playerNumber);
                    break;
                case PlayerOption.Hit:
                    game.Hit(playerNumber);
                    break;
                case PlayerOption.PrintHand:
                    game.PrintHand(playerNumber);
                    break;
                case PlayerOption.PileDump:
                    Deck d = game.ClonePile();
                    d.Sort();
                    Console.Write(d.ToString());
                    break;
                case PlayerOption.Quit:
                    throw new OperationCanceledException("Player quit the game.");
                default:
                    break;
            }
        }

        /// <summary>
        /// Determines if the given character is a valid menu option.
        /// </summary>
        private bool IsValidMenuOption(char keyChar)
        {
            return char.ToUpper(keyChar) == 'A' || char.ToUpper(keyChar) == 'H' ||
                   char.ToUpper(keyChar) == 'S' || char.ToUpper(keyChar) == 'P' ||
                   char.ToUpper(keyChar) == 'L' || char.ToUpper(keyChar) == 'Q';
        }

        /// <summary>
        /// Displays the results for all players after the round.
        /// </summary>
        private void DisplayRoundResults()
        {
            for (int i = 1; i <= game.numPlayers; i++)
            {
                switch (game.WinLoseOrBust(i))
                {
                    case HandResult.Bust:
                        Console.WriteLine($"Player {i} (value = {game.GetHandValue(i)}): BUST");
                        break;
                    case HandResult.Lose:
                        Console.WriteLine($"Player {i} (value = {game.GetHandValue(i)}): LOSER");
                        break;
                    case HandResult.Win:
                        Console.WriteLine($"Player {i} (value = {game.GetHandValue(i)}): WINNER!");
                        break;
                    case HandResult.Push:
                        Console.WriteLine($"Player {i} (value = {game.GetHandValue(i)}): PUSH");
                        break;
                }
            }
        }

        /// <summary>
        /// Determines if the current player's turn should continue.
        /// </summary>
        private bool ShouldContinueTurn(ConsoleKeyInfo keyPress, int playerNumber)
        {
            char input = char.ToUpper(keyPress.KeyChar);
            int handValue = game.GetHandValue(playerNumber);

            return input != 'S' && handValue <= BlackJack.maxHandValue;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int numPlayers = 5;
            if (args.Length > 0 && int.TryParse(args[0], out int players))
            {
                numPlayers = players;
            }
            
            ConsoleGame cg = new ConsoleGame(numPlayers);
            cg.InteractiveGame();
            cg.AutoGame(5);
#if DEBUG
            Console.ReadLine();
#endif
        }
    }
}
