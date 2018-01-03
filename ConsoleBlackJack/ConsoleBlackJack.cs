using System;
using jackel.Cards;

namespace ConsoleBlackJack
{
    class ConsoleGame
    {
        BlackJack game;
        const int houseHand = 0;
        private int numPlayers;

        public ConsoleGame(int players)
        {
            numPlayers = players;
            game = new BlackJack(numPlayers);
        }

        public void AutoGame(int numGames)
        {
            int[,] scoreBoard = new int[numGames, numPlayers + 1];

            for (int numGame = 0; numGame < numGames; numGame++)
            {
                game.Deal(); // collects cards, shuffles, and deals
                for (int i = 1; i <= game.numPlayers; i++)
                {
                    game.AutoHit(i);
                    scoreBoard[numGame, i] = game.GetHandValue(i);
                }
                game.AutoHit(houseHand);
                scoreBoard[numGame, houseHand] = game.GetHandValue(houseHand);
            }

            Console.WriteLine("Scoreboard Results\n==============================================");
            Console.Write("Game     H ");
            for (int j = 1; j <= numPlayers; j++)
                Console.Write($" | P{j}");
            Console.Write("\n");
            for (int i = 0; i < numGames; i++)
            {
                Console.Write($"Game {i,2}: {scoreBoard[i, houseHand],2}");
                for (int j = 1; j <= game.numPlayers; j++)
                {
                    Console.Write($" | {scoreBoard[i, j],2}");
                }
                Console.Write("\n");
            }
        }
        public void InteractiveGame()
        {
            while (true)
            {
                game.Deal();

                Console.WriteLine($"Dealer has card up: {game.Peek(0).ToString()}{Environment.NewLine}");

                ConsoleKeyInfo keyPress;

                for (int i = 1; i <= game.numPlayers; i++)
                {
                    do
                    {
                        game.PrintHand(i);
                        if (game.GetHandValue(i) == BlackJack.maxHandValue) // your turn is over whether you like it or not
                            break;
                        Console.WriteLine($"Player {i}: A=AutoHit, H=Hit, S=Stand, P=PrintHand, L=PileDump, Q=Quit");
                        keyPress = Console.ReadKey(true);
                        Console.WriteLine();
                        switch (char.ToUpper(keyPress.KeyChar))
                        {
                            case 'A':
                                game.AutoHit(i);
                                break;
                            case 'H':
                                game.Hit(i);
                                break;
                            case 'P':
                                game.PrintHand(i);
                                break;
                            case 'L':
                                Deck d = game.ClonePile();
                                d.Sort();
                                Console.Write(d.ToString());
                                break;
                            case 'Q':
                                return; // end the game
                            default:
                                break;
                        }
                    } while ((char.ToUpper(keyPress.KeyChar) != 'S') && (game.GetHandValue(i) <= BlackJack.maxHandValue));
                    if (game.GetHandValue(i) > BlackJack.maxHandValue)
                        Console.WriteLine($"Player {i} (value = {game.GetHandValue(i)}) : BUSTED");
                }
                game.AutoHit(houseHand);
                Console.WriteLine();
                game.PrintHand(houseHand);

                if (game.GetHandValue(houseHand) > BlackJack.maxHandValue)
                {
                    Console.WriteLine("House BUSTS");
                }
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
            Console.ReadLine();
        }
    }

}
