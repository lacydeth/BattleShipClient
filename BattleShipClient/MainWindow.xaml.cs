using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace BattleShipClient
{
    public partial class MainWindow : Window
    {
        private GameClient client;
        private const int gridSize = 10;
        private int selectedShipLength = 0;
        private bool isHorizontal = true; // Default to horizontal
        private Ship selectedShip; // The currently selected ship
        private List<Ship> availableShips;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGrid(PlayerGrid);
            InitializeGrid(OpponentGrid);
            client = new GameClient();
            InitializeShips();
            ShipListBox.ItemsSource = availableShips; // Bind ListBox to Ship list
        }

        private void InitializeShips()
        {
            availableShips = new List<Ship>
            {
                new Ship { Name = "Aircraft Carrier", Size = 5 },
                new Ship { Name = "Battleship", Size = 4 },
                new Ship { Name = "Submarine", Size = 3 },
                new Ship { Name = "Cruiser", Size = 3 },
                new Ship { Name = "Destroyer", Size = 2 }
            };
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

        private async void ShipListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Ensure a valid selection
                if (ShipListBox.SelectedItem != null)
                {
                    selectedShip = (Ship)ShipListBox.SelectedItem;
                    selectedShip.StartX = -1; // Reset position to indicate no placement yet
                    selectedShip.StartY = -1; // Reset position

                    Console.WriteLine($"Ship selected: {selectedShip.Name}");

                    // Display an appropriate message, allowing the user to click a grid cell to place the ship
                    StatusLabel.Content = $"Select a position to place the {selectedShip.Name}.";
                    selectedShipLength = selectedShip.Size; // Set the ship length
                }
                else
                {
                    Console.WriteLine("No ship selected.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during ship selection: " + ex.Message);
                Console.WriteLine($"Error during ship selection: {ex.Message}");
            }
        }

        private async Task PlaceShipAsync(Ship selectedShip)
        {
            try
            {
                // Simulate server communication for placing the ship
                bool success = await ServerPlaceShipAsync(selectedShip);
                if (success)
                {
                    // Update the UI after ship placement
                    UpdateUIAfterShipPlacement(selectedShip);
                }
                else
                {
                    MessageBox.Show("Failed to place the ship.");
                    Console.WriteLine("Ship placement failed.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while placing the ship: " + ex.Message);
                Console.WriteLine($"Error placing ship: {ex.Message}");
            }
        }

        private async Task<bool> ServerPlaceShipAsync(Ship selectedShip)
        {
            // Simulate some server communication (use actual network code here)
            await Task.Delay(1000); // Simulating a delay
            Console.WriteLine($"Ship {selectedShip.Name} placed on the server.");
            return true;
        }

        private void UpdateUIAfterShipPlacement(Ship selectedShip)
        {
            // Update the selection list to reflect the change
            ShipListBox.Items.Remove(selectedShip);
            UpdateGameGridWithShip(selectedShip);
        }

        private void UpdateGameGridWithShip(Ship selectedShip)
        {
            // Get the starting point (coordinates) and place the ship horizontally or vertically
            int startX = selectedShip.StartX;
            int startY = selectedShip.StartY;
            int length = selectedShip.Size;

            // Update the grid cells based on the ship's position
            for (int i = 0; i < length; i++)
            {
                int x = startX;
                int y = startY;

                if (isHorizontal)
                {
                    x += i;  // Move along the x-axis (horizontal)
                }
                else
                {
                    y += i;  // Move along the y-axis (vertical)
                }

                // Get the corresponding button and change its background color to indicate the ship
                Button cell = (Button)PlayerGrid.Children[y * gridSize + x];  // Assuming grid is 10x10
                cell.Background = new SolidColorBrush(Colors.Blue); // Change color to blue, for example
            }

            Console.WriteLine($"Placed ship {selectedShip.Name} on the game grid.");
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (!client.IsConnected || selectedShipLength == 0)
            {
                StatusLabel.Content = "Status: Not connected or no ship selected.";
                return;
            }

            Button cell = sender as Button;
            int position = (int)cell.Tag;
            int x = position % gridSize;
            int y = position / gridSize;

            // Set the starting position for the ship
            selectedShip.StartX = x;
            selectedShip.StartY = y;

            // Send ship placement to the server
            client.SendShipPlacement(x, y, selectedShipLength, isHorizontal);

            // After placing the ship, update the UI
            UpdateGameGridWithShip(selectedShip);
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            client.Connect("127.0.0.1", 5000);
            StatusLabel.Content = client.IsConnected ? "Status: Connected" : "Status: Failed to connect";
        }
    }

    // Ship class to represent ships
    public class Ship
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public string Type { get; set; }  // Optional: You can add other properties if necessary
        public int StartX { get; set; }  // Starting X position on the grid
        public int StartY { get; set; }  // Starting Y position on the grid
    }
}
