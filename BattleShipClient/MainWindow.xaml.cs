using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace BattleShipClient
{
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

        private void InitializeGrid(UniformGrid grid)
        {
            for (int i = 0; i < gridSize * gridSize; i++)
            {
                Button cell = new Button { Tag = i, Width = 30, Height = 30 };
                cell.Click += Cell_Click;
                grid.Children.Add(cell);
            }
        }

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

            client.SendShot(x, y);
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            client.Connect("127.0.0.1", 5000);
            StatusLabel.Content = client.IsConnected ? "Status: Connected" : "Status: Failed to connect";
        }
    }
}
