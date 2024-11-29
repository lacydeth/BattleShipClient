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
                if (ShipListBox.SelectedItem != null)
                {
                    selectedShip = (Ship)ShipListBox.SelectedItem;

                    // Debugging: Log the selected ship details
                    Console.WriteLine($"Ship selected: {selectedShip.Name}, Size: {selectedShip.Size}");

                    // Check if ship is already placed
                    if (selectedShip.StartX != -1 || selectedShip.StartY != -1)
                    {
                        StatusLabel.Content = $"{selectedShip.Name} has already been placed!";
                        return;
                    }

                    // Update the UI to prompt placement
                    StatusLabel.Content = $"Select a position to place the {selectedShip.Name}.";
                    selectedShipLength = selectedShip.Size; // Set the ship length
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting ship: " + ex.Message);
                Console.WriteLine($"Error selecting ship: {ex.Message}");
            }
        }

        private async Task<bool> PlaceShipAsync(Ship selectedShip)
        {
            try
            {
                // Simulate server communication for placing the ship
                bool success = await ServerPlaceShipAsync(selectedShip);
                if (success)
                {
                    // Update the UI after ship placement
                    UpdateUIAfterShipPlacement(selectedShip);
                    return true;
                }
                else
                {
                    MessageBox.Show("Failed to place the ship.");
                    Console.WriteLine("Ship placement failed.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while placing the ship: " + ex.Message);
                Console.WriteLine($"Error placing ship: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> ServerPlaceShipAsync(Ship selectedShip)
        {
            // Simulate some server communication (use actual network code here)
            await Task.Delay(1000); // Simulating a delay
            Console.WriteLine($"Ship {selectedShip.Name} placed on the server.");
            return true;
        }
        private void UpdateUIAfterShipPlacement(Ship placedShip)
        {
            // Update grid colors to indicate ship placement
            UpdateGameGridWithShip(placedShip);

            // Reset selection
            selectedShip = null;
            selectedShipLength = 0;
        }

        private void UpdateGameGridWithShip(Ship selectedShip)
        {
            // Get the starting point (coordinates) and place the ship horizontally or vertically
            int startX = selectedShip.StartX;
            int startY = selectedShip.StartY;
            int length = selectedShip.Size;

            for (int i = 0; i < length; i++)
            {
                int x = startX;
                int y = startY;

                // Move the ship horizontally or vertically
                if (isHorizontal)
                {
                    x += i;  // Horizontal placement
                }
                else
                {
                    y += i;  // Vertical placement
                }

                // Check if the calculated position is within the grid bounds
                if (x >= gridSize || y >= gridSize)
                {
                    MessageBox.Show($"Invalid placement: Ship goes out of bounds at {x}, {y}");
                    return;
                }

                // Update the grid cell
                Button cell = (Button)PlayerGrid.Children[y * gridSize + x];  // Correct position mapping
                cell.Background = new SolidColorBrush(Colors.Blue); // Set the ship color
            }
            Console.WriteLine($"Placed ship {selectedShip.Name} on the grid at ({startX},{startY})");
        }
        private async void Cell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if the client is connected
                if (!client.IsConnected)
                {
                    StatusLabel.Content = "Status: Not connected.";
                    return;
                }

                // Check if a ship is selected and if the ship size is set
                if (selectedShip == null || selectedShipLength == 0)
                {
                    StatusLabel.Content = "No ship selected for placement.";
                    Console.WriteLine("Error: No ship selected.");
                    return;
                }

                // Debugging: Check the selected ship's properties
                Console.WriteLine($"Selected Ship: {selectedShip.Name}, StartX: {selectedShip.StartX}, StartY: {selectedShip.StartY}");

                Button cell = sender as Button;
                if (cell == null)
                {
                    StatusLabel.Content = "Invalid cell clicked.";
                    return;
                }

                int position = (int)cell.Tag;
                int x = position % gridSize;
                int y = position / gridSize;

                // Debugging: Log the cell position
                Console.WriteLine($"Cell clicked at position: X = {x}, Y = {y}");

                // Check if the current ship has already been placed
                if (selectedShip.StartX != -1 || selectedShip.StartY != -1)
                {
                    StatusLabel.Content = $"{selectedShip.Name} has already been placed.";
                    return;
                }

                // Ensure only one ship of each type is placed
                foreach (var ship in availableShips)
                {
                    if (ship.Name == selectedShip.Name && ship.StartX != -1 && ship.StartY != -1)
                    {
                        StatusLabel.Content = $"{selectedShip.Name} is already placed!";
                        return;
                    }
                }

                // Set the starting position for the ship
                selectedShip.StartX = x;
                selectedShip.StartY = y;

                // Debugging: Log the ship's start position
                Console.WriteLine($"Placing ship at: StartX = {selectedShip.StartX}, StartY = {selectedShip.StartY}");

                // Validate that selectedShip has been placed (non-null)
                if (selectedShip.StartX == -1 || selectedShip.StartY == -1)
                {
                    StatusLabel.Content = "Error: Ship placement failed (invalid coordinates).";
                    return;
                }

                // Update the UI after ship placement
                UpdateUIAfterShipPlacement(selectedShip);

                // Remove the ship from the availableShips list
                availableShips.Remove(selectedShip);

                // Ensure `selectedShip` is not null before trying to access its properties
                if (selectedShip != null)
                {
                    Console.WriteLine($"Removed {selectedShip.Name} from available ships.");
                }
                else
                {
                    Console.WriteLine("Error: selectedShip is null.");
                }


                // Update the ShipListBox by re-binding the ItemsSource
                ShipListBox.ItemsSource = null;
                ShipListBox.ItemsSource = availableShips;

                if (selectedShip != null)
                {
                    StatusLabel.Content = $"{selectedShip.Name} placed successfully!";
                    ShipListBox.Items.Remove(selectedShip);
                }
                else
                {
                    StatusLabel.Content = "Error: No ship selected!";
                    Console.WriteLine("Error: selectedShip is null.");
                }

            }
            catch (Exception ex)
            {
                // Show error message and log the exception details
                MessageBox.Show("Error placing ship: " + ex.Message);
                Console.WriteLine($"Error placing ship: {ex.Message}");
            }
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
        public int StartX { get; set; } = -1;  // Default to unplaced (-1)
        public int StartY { get; set; } = -1;  // Default to unplaced (-1)
    }

}
