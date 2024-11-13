using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BattleShipClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameClient client;
        private const int gridSize = 10;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGrid(PlayerGrid);
            InitializeGrid(OpponentGrid);
            client = new GameClient();
        }

        // Initialize a 10x10 grid with buttons for interaction
        private void InitializeGrid(UniformGrid grid)
        {
            for (int i = 0; i < gridSize * gridSize; i++)
            {
                Button cell = new Button { Tag = i, Width = 30, Height = 30 };
                cell.Click += Cell_Click;
                grid.Children.Add(cell);
            }
        }

        // Handle shot on opponent grid
        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (!client.IsConnected)
            {
                StatusLabel.Content = "Status: Not connected to server.";
                return;
            }

            Button cell = sender as Button;
            int position = (int)cell.Tag;
            int x = position % gridSize;
            int y = position / gridSize;

            // Send shot coordinates to server
            client.SendShot(x, y);
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            client.Connect("127.0.0.1", 5000); // Adjust IP and port as needed
            StatusLabel.Content = client.IsConnected ? "Status: Connected" : "Status: Failed to connect";
        }
    }
}