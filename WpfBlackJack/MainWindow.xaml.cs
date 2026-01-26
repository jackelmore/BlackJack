using System;
using System.Collections.Generic;
using System.Linq;
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

namespace WpfBlackJack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const double leftBuffer = 10, topBuffer = 10;
        readonly double fontSize = 16;
        readonly double cardHeight = Properties.Resources.B.Height;
        readonly double cardWidth = Properties.Resources.B.Width;
        int currentPlayer;
        Border border;

        private Grid[] grids;
        private TextBlock[] txtBlocks;

        BlackJack game;

        public MainWindow()
        {
            InitializeComponent();
            game = new BlackJack(((App)Application.Current).numPlayers);
            //          game = new BlackJack(8);
            MakeGrids();
            StartOver();
        }
        public void MakeGrids()
        {
            grids = new Grid[game.numPlayers + 1];
            txtBlocks = new TextBlock[game.numPlayers + 1];

            for (int i = 0; i <= game.numPlayers; i++)
            {
                grids[i] = new Grid();
                txtBlocks[i] = new TextBlock
                {
                    Text = i == 0 ? "House" : "Player " + i,
                    Width = grids[i].Width = 3 * cardWidth,
                    HorizontalAlignment = grids[i].HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = grids[i].VerticalAlignment = VerticalAlignment.Top,
                    FontSize = fontSize,
                    Margin = new Thickness(leftBuffer, cardHeight + topBuffer, 0, topBuffer)
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

        private void StartOver()
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
        private void SetImage(Image i, PlayingCard p)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            if (p.faceUp)
                bi.UriSource = new Uri($"pack://application:,,,/resources/{p.ShortName}.gif");
            else
                bi.UriSource = new Uri($"pack://application:,,,/resources/b.gif");
            bi.EndInit();
            i.Source = bi;
        }
        private void DisplayCards(int player)
        {
            bool showValue = true;
            int xPosition = 0;
            ClearGrid(grids[player]);
            foreach (PlayingCard p in game.GetEnumerableHand(player))
            {
                if (p.faceUp == false)
                    showValue = false;
                Image i = new Image
                {
                    Stretch = Stretch.None,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                SetImage(i, p);
                i.Margin = (new Thickness(leftBuffer + xPosition, topBuffer, 0, 0));
                grids[player].Children.Add(i);
                xPosition += (int)i.Source.Width / 5;
            }
            if (showValue)
                txtBlocks[player].Text = $"{grids[player].Name}: Value is {game.GetHandValue(player)}";
            else
                txtBlocks[player].Text = $"{grids[player].Name}";
        }
        private void ClearGrid(Grid g)
        {
            UIElement[] elements = new UIElement[g.Children.Count];
            g.Children.CopyTo(elements, 0);
            foreach (UIElement element in elements) // only remove Image elements
                if (element is Image)
                    g.Children.Remove(element);
        }
        private bool SetCurrentPlayer(int player)
        {
            if (player > game.numPlayers)
                return false;
            currentPlayer = player;
            if (border == null)
            {
                border = new Border
                {
                    BorderBrush = Brushes.Red,
                    BorderThickness = new Thickness(2)
                };
            }
            else
            {
                Grid g = (Grid)border.Parent;
                g.Children.Remove(border);
            }
            grids[currentPlayer].Children.Add(border);
            game.ShowHand(currentPlayer);
            DisplayCards(currentPlayer);
            if (game.GetHandValue(currentPlayer) >= BlackJack.maxHandValue) // if player has 21 already then skip them
                NextPlayer();
            return true;
        }
        private void NextPlayer()
        {
            if (currentPlayer < game.numPlayers)
                SetCurrentPlayer(currentPlayer + 1);
            else
            { // Players are complete, process house hand
                btnStand.IsEnabled = false;
                btnHitMe.IsEnabled = false;
                SetCurrentPlayer(BlackJack.houseHand);
                game.AutoHit(BlackJack.houseHand);
                DisplayCards(BlackJack.houseHand);

                // Iterate thru each player and update winner status
                if (game.GetHandValue(BlackJack.houseHand) > BlackJack.maxHandValue)
                {
                    txtBlocks[BlackJack.houseHand].Text = $"{grids[BlackJack.houseHand].Name} ({game.GetHandValue(BlackJack.houseHand)}): BUST!";
                }
                for (currentPlayer = 1; currentPlayer <= game.numPlayers; currentPlayer++)
                {
                    UpdateWinLose(currentPlayer);
                }
            }
        }
        private void UpdateWinLose(int player)
        {
            switch (game.WinLoseOrBust(player))
            {
                case HandResult.Bust:
                    txtBlocks[player].Text = $"{grids[player].Name} ({game.GetHandValue(player)}): BUST!";
                    txtBlocks[player].Foreground = Brushes.Red;
                    break;
                case HandResult.Win:
                    txtBlocks[player].Text = $"{grids[player].Name} ({game.GetHandValue(player)}): WINNER!";
                    txtBlocks[player].Foreground = Brushes.Green;
                    break;
                case HandResult.Lose:
                    txtBlocks[player].Text = $"{grids[player].Name} ({game.GetHandValue(player)}): LOSER!";
                    txtBlocks[player].Foreground = Brushes.Red;
                    break;
                case HandResult.Push:
                    txtBlocks[player].Text = $"{grids[player].Name} ({game.GetHandValue(player)}): PUSH!";
                    txtBlocks[player].Foreground = Brushes.Purple;
                    break;
            }
        }
        private void BtnHitMe_Click(object sender, RoutedEventArgs e)
        {
            PlayingCard p = game.Hit(currentPlayer);
            if (p != null)
                DisplayCards(currentPlayer);
            if (game.GetHandValue(currentPlayer) >= BlackJack.maxHandValue)
                NextPlayer();
        }

        private void BtnStartOver_Click(object sender, RoutedEventArgs e)
        {
            StartOver();
        }

        private void BtnStand_Click(object sender, RoutedEventArgs e)
        {
            NextPlayer();
        }
    }
}
