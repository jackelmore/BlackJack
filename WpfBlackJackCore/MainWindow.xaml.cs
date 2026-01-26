using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using jackel.Cards;

namespace WpfBlackJackCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double LEFT_BUFFER = 10;
        private const double TOP_BUFFER = 10;
        private const double FONT_SIZE = 16;
        private const double CARD_SPACING_DIVISOR = 5;
        private const int GRID_WIDTH_MULTIPLIER = 3;
        private const string RESOURCE_BASE_PATH = "pack://application:,,,/resources/";
        private const string CARD_BACK_RESOURCE = "b.gif";
        private const string INITIAL_CARD_RESOURCE = "C2.gif";
        private const int BORDER_THICKNESS = 2;
        
        private readonly Dictionary<string, BitmapImage> cardImageCache = new Dictionary<string, BitmapImage>();
        private readonly double cardHeight;
        private readonly double cardWidth;

        private int currentPlayer;
        private Border currentPlayerBorder;
        private Grid[] grids;
        private TextBlock[] txtBlocks;
        private BlackJack game;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                BitmapImage initialCard = LoadCardImageFromCache(INITIAL_CARD_RESOURCE);
                cardHeight = initialCard.PixelHeight;
                cardWidth = initialCard.PixelWidth;

                game = new BlackJack(((App)Application.Current).numPlayers);
                MakeGrids();
                StartOver();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing game: {ex.Message}", "Initialization Error");
            }
        }

        /// <summary>
        /// Creates the grids and text blocks for the house and all players.
        /// </summary>
        public void MakeGrids()
        {
            try
            {
                grids = new Grid[game.numPlayers + 1];
                txtBlocks = new TextBlock[game.numPlayers + 1];

                for (int i = 0; i <= game.numPlayers; i++)
                {
                    grids[i] = new Grid();
                    txtBlocks[i] = new TextBlock
                    {
                        Text = i == 0 ? "House" : "Player " + i,
                        Width = grids[i].Width = GRID_WIDTH_MULTIPLIER * cardWidth,
                        HorizontalAlignment = grids[i].HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = grids[i].VerticalAlignment = VerticalAlignment.Top,
                        FontSize = FONT_SIZE,
                        Margin = new Thickness(LEFT_BUFFER, cardHeight + TOP_BUFFER, 0, TOP_BUFFER)
                    };
                }

                grids[0].Name = "House";
                grids[0].Children.Add(txtBlocks[BlackJack.houseHand]);
                MainGrid.Children.Add(grids[BlackJack.houseHand]);

                for (int i = 1; i <= game.numPlayers; i++)
                {
                    grids[i].Name = "Player" + i;
                    grids[i].Children.Add(txtBlocks[i]);
                    wpPlayers.Children.Add(grids[i]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating grids: {ex.Message}", "Grid Creation Error");
            }
        }

        /// <summary>
        /// Starts a new game, dealing cards and resetting the UI.
        /// </summary>
        private void StartOver()
        {
            try
            {
                game.Deal();
                for (int i = 0; i <= game.numPlayers; i++)
                {
                    DisplayCards(i);
                    txtBlocks[i].Foreground = Brushes.Black;
                }
                SetCurrentPlayer(1);
                btnHitMe.IsEnabled = true;
                btnStand.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting new game: {ex.Message}", "Game Start Error");
            }
        }

        /// <summary>
        /// Loads a card image from the resource cache, or creates it if not cached.
        /// </summary>
        /// <param name="resourceName">The name of the resource (without path or extension).</param>
        /// <returns>A BitmapImage for the specified card.</returns>
        private BitmapImage LoadCardImageFromCache(string resourceName)
        {
            if (cardImageCache.TryGetValue(resourceName, out BitmapImage cachedImage))
            {
                return cachedImage;
            }

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            string extension = resourceName.EndsWith(".gif") ? "" : ".gif";
            bi.UriSource = new Uri($"{RESOURCE_BASE_PATH}{resourceName}{extension}");
            bi.EndInit();
            bi.Freeze();
            cardImageCache[resourceName] = bi;
            return bi;
        }

        /// <summary>
        /// Sets the image source for a card display element.
        /// </summary>
        /// <param name="imageControl">The Image control to update.</param>
        /// <param name="card">The playing card to display.</param>
        private void SetImage(Image imageControl, PlayingCard card)
        {
            try
            {
                string resourceName = card.faceUp ? card.ShortName : CARD_BACK_RESOURCE;
                imageControl.Source = LoadCardImageFromCache(resourceName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading card image: {ex.Message}", "Image Loading Error");
            }
        }

        /// <summary>
        /// Displays all cards for the specified player.
        /// </summary>
        /// <param name="playerNumber">The player number (0 for house).</param>
        private void DisplayCards(int playerNumber)
        {
            try
            {
                ValidatePlayerNumber(playerNumber);
                
                bool showValue = true;
                int xPosition = 0;
                ClearGrid(grids[playerNumber]);

                foreach (PlayingCard card in game.GetEnumerableHand(playerNumber))
                {
                    if (!card.faceUp)
                        showValue = false;

                    Image cardImage = new Image
                    {
                        Stretch = Stretch.None,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    
                    SetImage(cardImage, card);
                    cardImage.Margin = new Thickness(LEFT_BUFFER + xPosition, TOP_BUFFER, 0, 0);
                    grids[playerNumber].Children.Add(cardImage);
                    xPosition += (int)cardImage.Source.Width / (int)CARD_SPACING_DIVISOR;
                }

                UpdatePlayerStatusText(playerNumber, showValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying cards: {ex.Message}", "Display Error");
            }
        }

        /// <summary>
        /// Updates the status text for a player, showing their hand value if visible.
        /// </summary>
        /// <param name="playerNumber">The player number.</param>
        /// <param name="showValue">Whether to display the hand value.</param>
        private void UpdatePlayerStatusText(int playerNumber, bool showValue)
        {
            if (showValue)
                txtBlocks[playerNumber].Text = $"{grids[playerNumber].Name}: Value is {game.GetHandValue(playerNumber)}";
            else
                txtBlocks[playerNumber].Text = $"{grids[playerNumber].Name}";
        }

        /// <summary>
        /// Clears image elements from a grid while preserving other UI elements.
        /// </summary>
        /// <param name="grid">The grid to clear.</param>
        private void ClearGrid(Grid grid)
        {
            try
            {
                UIElement[] elements = new UIElement[grid.Children.Count];
                grid.Children.CopyTo(elements, 0);
                foreach (UIElement element in elements)
                {
                    if (element is Image)
                        grid.Children.Remove(element);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing grid: {ex.Message}", "Clear Error");
            }
        }

        /// <summary>
        /// Sets the current player and displays their hand with a border highlight.
        /// </summary>
        /// <param name="playerNumber">The player number to set as current.</param>
        /// <returns>True if the player was set successfully; otherwise, false.</returns>
        private bool SetCurrentPlayer(int playerNumber)
        {
            try
            {
                ValidatePlayerNumber(playerNumber);

                currentPlayer = playerNumber;
                
                if (currentPlayerBorder == null)
                {
                    currentPlayerBorder = new Border
                    {
                        BorderBrush = Brushes.Red,
                        BorderThickness = new Thickness(BORDER_THICKNESS)
                    };
                }
                else
                {
                    Grid previousGrid = (Grid)currentPlayerBorder.Parent;
                    if (previousGrid != null)
                        previousGrid.Children.Remove(currentPlayerBorder);
                }

                grids[currentPlayer].Children.Add(currentPlayerBorder);
                game.ShowHand(currentPlayer);
                DisplayCards(currentPlayer);

                if (game.GetHandValue(currentPlayer) >= BlackJack.maxHandValue)
                    NextPlayer();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting current player: {ex.Message}", "Player Selection Error");
                return false;
            }
        }

        /// <summary>
        /// Advances to the next player, or processes the house hand if all players have completed.
        /// </summary>
        private void NextPlayer()
        {
            try
            {
                if (currentPlayer < game.numPlayers)
                {
                    SetCurrentPlayer(currentPlayer + 1);
                }
                else
                {
                    ProcessGameEnd();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error advancing to next player: {ex.Message}", "Next Player Error");
            }
        }

        /// <summary>
        /// Processes the end of game, revealing house hand and determining winners.
        /// </summary>
        private void ProcessGameEnd()
        {
            btnStand.IsEnabled = false;
            btnHitMe.IsEnabled = false;
            
            SetCurrentPlayer(BlackJack.houseHand);
            game.AutoHit(BlackJack.houseHand);
            DisplayCards(BlackJack.houseHand);

            if (game.GetHandValue(BlackJack.houseHand) > BlackJack.maxHandValue)
            {
                txtBlocks[BlackJack.houseHand].Text = FormatBustMessage(BlackJack.houseHand);
                txtBlocks[BlackJack.houseHand].Foreground = GetStatusColor(HandResult.Bust);
            }

            for (currentPlayer = 1; currentPlayer <= game.numPlayers; currentPlayer++)
            {
                UpdateWinLose(currentPlayer);
            }
        }

        /// <summary>
        /// Updates the win/loss/bust status for a player.
        /// </summary>
        /// <param name="playerNumber">The player number.</param>
        private void UpdateWinLose(int playerNumber)
        {
            try
            {
                ValidatePlayerNumber(playerNumber);
                HandResult result = game.WinLoseOrBust(playerNumber);
                int handValue = game.GetHandValue(playerNumber);

                string statusText = result switch
                {
                    HandResult.Bust => FormatBustMessage(playerNumber),
                    HandResult.Win => $"{grids[playerNumber].Name} ({handValue}): WINNER!",
                    HandResult.Lose => $"{grids[playerNumber].Name} ({handValue}): LOSER!",
                    HandResult.Push => $"{grids[playerNumber].Name} ({handValue}): PUSH!",
                    _ => grids[playerNumber].Name
                };

                txtBlocks[playerNumber].Text = statusText;
                txtBlocks[playerNumber].Foreground = GetStatusColor(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating player status: {ex.Message}", "Status Update Error");
            }
        }

        /// <summary>
        /// Gets the brush color for a given game result.
        /// </summary>
        /// <param name="result">The hand result.</param>
        /// <returns>A brush color corresponding to the result.</returns>
        private Brush GetStatusColor(HandResult result)
        {
            return result switch
            {
                HandResult.Bust => Brushes.Red,
                HandResult.Win => Brushes.Green,
                HandResult.Lose => Brushes.Red,
                HandResult.Push => Brushes.Purple,
                _ => Brushes.Black
            };
        }

        /// <summary>
        /// Formats a bust message for a player.
        /// </summary>
        /// <param name="playerNumber">The player number.</param>
        /// <returns>A formatted bust message.</returns>
        private string FormatBustMessage(int playerNumber)
        {
            return $"{grids[playerNumber].Name} ({game.GetHandValue(playerNumber)}): BUST!";
        }

        /// <summary>
        /// Validates that a player number is within valid range.
        /// </summary>
        /// <param name="playerNumber">The player number to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when player number is invalid.</exception>
        private void ValidatePlayerNumber(int playerNumber)
        {
            if (playerNumber < 0 || playerNumber > game.numPlayers)
                throw new ArgumentOutOfRangeException(nameof(playerNumber), $"Player number must be between 0 and {game.numPlayers}");
        }

        /// <summary>
        /// Handles the Hit button click event.
        /// </summary>
        private void BtnHitMe_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayingCard card = game.Hit(currentPlayer);
                if (card != null)
                    DisplayCards(currentPlayer);

                if (game.GetHandValue(currentPlayer) >= BlackJack.maxHandValue)
                {
                    if (game.GetHandValue(currentPlayer) > BlackJack.maxHandValue)
                    {
                        txtBlocks[currentPlayer].Text = FormatBustMessage(currentPlayer);
                    }
                    NextPlayer();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing hit: {ex.Message}", "Hit Error");
            }
        }

        /// <summary>
        /// Handles the Start Over button click event.
        /// </summary>
        private void BtnStartOver_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cardImageCache.Clear();
                StartOver();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting new game: {ex.Message}", "Start Over Error");
            }
        }

        /// <summary>
        /// Handles the Stand button click event.
        /// </summary>
        private void BtnStand_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NextPlayer();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing stand: {ex.Message}", "Stand Error");
            }
        }
    }
}
