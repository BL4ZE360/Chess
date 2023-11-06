using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;

namespace Chess {
	public partial class MainWindow : Window {
		private ChessBoard chessBoard; // Object for board-representation
		private ChessPiece? selectedPiece; // Variable for the piece the user has selected to move
		private List<int> possMoves = new List<int>(); // List of possible moves for selectedPiece to move to
		private Dictionary<Image, ChessPiece> imageToChessPieceMap; // Mapping of each piece image to its respective ChessPiece object
		private ChessPieceColor turn = ChessPieceColor.White; // Tracker for whose turn it is
		private bool whiteCheck = false, blackCheck = false; // Variables to determine if a player is in "Check"

		readonly SolidColorBrush colorDark = new SolidColorBrush(Color.FromRgb(110, 110, 110)); // Dark tile color
		readonly SolidColorBrush colorLight = new SolidColorBrush(Color.FromRgb(210, 210, 210)); // Light tile color
		readonly SolidColorBrush selectedDark = new SolidColorBrush(Color.FromRgb(81, 100, 150)); // selectedPiece-tile color (if light)
		readonly SolidColorBrush selectedLight = new SolidColorBrush(Color.FromRgb(117, 144, 217)); // selectedPiece-tile color (if dark)
		readonly SolidColorBrush maskColorDark = new SolidColorBrush(Color.FromRgb(81, 150, 100)); // Dark tile color if in possMoves
		readonly SolidColorBrush maskColorLight = new SolidColorBrush(Color.FromRgb(117, 217, 144)); // Light tile color if in possMoves


		public MainWindow() {
			InitializeComponent();

			// Create the chessboard
			chessBoard = new ChessBoard();
			chessBoard.ResetBoard();

			// Initialise the mapping and chessboard
			imageToChessPieceMap = new Dictionary<Image, ChessPiece>();
			InitializeChessboard();

			UpdateUI();
		}

		// Creates the grid and the ChessPieces
		private void InitializeChessboard() {
			// Initialise the turn tracker
			turn = ChessPieceColor.White;

			// Clear the existing children of the uniformGrid
			uniformGrid.Children.Clear();

			// Iterate through the grid's rows
			for (int y = 0; y < 8; y++) {
				// Iterate through the grid's columns
				for (int x = 0; x < 8; x++) {
					// Create the grid for each tile
					Grid grid = new Grid();

					// Set the row and column properties
					grid.SetValue(Grid.ColumnProperty, x);
					grid.SetValue(Grid.RowProperty, y);

					// Set the tile color based on the row and column values
					SolidColorBrush tileColor = (x + y) % 2 == 0 ? colorLight : colorDark;

					// Create and add the rectangle to the grid
					Rectangle rectangle = new Rectangle();
					rectangle.Fill = tileColor;
					rectangle.MouseDown += ChessBoardTile_MouseDown;
					grid.Children.Add(rectangle);

					// Check if the grid coordinates correspond to a piece position
					if (HasPiece(x, y)) {
						// Create and add the image for the piece
						Image pieceImage = new Image();
						pieceImage.Source = GetPieceImageSource(x, y);
						pieceImage.MouseDown += ChessPiece_MouseDown;

						// Add the piece image to the grid
						grid.Children.Add(pieceImage);

						// Add the mapping of this piece and its image
						imageToChessPieceMap.Add(pieceImage, chessBoard.GetPiece(x, y));
					}

					// Add the grid to the uniformGrid
					uniformGrid.Children.Add(grid);
				}
			}
		}

		// Checks if tile (x,y) has a piece on it
		private bool HasPiece(int x, int y) {
			// Check if the grid coordinates correspond to a piece position
			return chessBoard.GetPiece(x, y) != null;
		}

		// Gets the ImageSource for the piece at (x,y) (if there is one)
		private ImageSource? GetPieceImageSource(int x, int y) {
			// Return the appropriate image source based on the piece type and color
			if (!HasPiece(x, y)) return null;
			ChessPiece piece = chessBoard.GetPiece(x, y);
			string pieceName = piece.Color.ToNameString() + piece.Type.ToNameString();

			return new BitmapImage(new Uri("pieces/" + pieceName + ".png", UriKind.Relative));
		}

		// Function to run when a piece is clicked on (whether to make it the selectedPiece, or to kill it with the selectedPiece)
		private void ChessPiece_MouseDown(object sender, MouseButtonEventArgs e) {
			// Get the clicked image
			Image clickedImage = (Image) sender;

			// If there is a selectedPiece currently, either kill the clicked piece or deselect the selectedPiece
			if (selectedPiece != null) {
				ChessPiece? killedPiece = null;
				imageToChessPieceMap.TryGetValue(clickedImage, out killedPiece);
				// If there is no piece to kill or the piece clicked on is the same color as the player, deselected the selectedPiece
				if (killedPiece == null || killedPiece.Color == selectedPiece.Color) ClearSelected();
				else {
					// Kill the clicked piece and return if it is a valid move to make for the selectedPiece
					for (int i = 0; i < possMoves.Count; i += 2) {
						if (possMoves[i] == killedPiece.X && possMoves[i + 1] == killedPiece.Y) {
							MessageBox.Show("Killing " + killedPiece.Color.ToNameString() + killedPiece.Type.ToNameString());

							MovePiece(selectedPiece, killedPiece.X, killedPiece.Y);
							return;
						}
					}
				}
			}

			// Update selectedPiece to this clicked piece
			imageToChessPieceMap.TryGetValue(clickedImage, out selectedPiece);
			if (selectedPiece == null || selectedPiece.Color != turn) return;

			int x = selectedPiece.X, y = selectedPiece.Y;

			// Get the grid that the selectedPiece is in
			Grid? grid = uniformGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == y && Grid.GetColumn(g) == x);

			if (grid != null) {
				// Find the Rectangle element within the Grid
				Rectangle? rectangle = grid.Children.OfType<Rectangle>().FirstOrDefault();

				if (rectangle != null) {
					// Adjust the mask color based on the original tile color
					if (x % 2 == y % 2) rectangle.Fill = selectedLight;
					else rectangle.Fill = selectedDark;
				}
			}

			// Show the available squares to move selectedPiece to
			possMoves = selectedPiece.GetPossibleMoves();

			// For each possible tile to move to, change its tile color to the maskColors
			for (int i = 0; i < possMoves.Count; i += 2) {
				x = possMoves[i];
				y = possMoves[i + 1];

				// Find the corresponding Grid element in the UniformGrid based on x and y coordinates
				grid = uniformGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == y && Grid.GetColumn(g) == x);

				if (grid != null) {
					// Find the Rectangle element within the Grid
					Rectangle? rectangle = grid.Children.OfType<Rectangle>().FirstOrDefault();

					if (rectangle != null) {
						// Adjust the mask color based on the original tile color
						if (x % 2 == y % 2) rectangle.Fill = maskColorLight;
						else rectangle.Fill = maskColorDark;
					}
				}
			}
		}

		// When a tile with no piece is clicked on, move the selectedPiece to this tile (if valid)
		private void ChessBoardTile_MouseDown(object sender, MouseButtonEventArgs e) {
			Rectangle clickedRectangle = (Rectangle) sender;

			Grid? parentGrid = VisualTreeHelper.GetParent(clickedRectangle) as Grid;

			if (parentGrid != null) {
				// Get the row and column of the clicked rectangle
				int x = (int) parentGrid.GetValue(Grid.ColumnProperty);
				int y = (int) parentGrid.GetValue(Grid.RowProperty);

				if (selectedPiece != null && MoveIsValid(x, y)) MovePiece(selectedPiece, x, y);
			}
		}

		// Move piece to (x,y)
		private void MovePiece(ChessPiece piece, int x, int y) {
			if (piece.Color != turn || !MoveIsValid(x, y)) return;

			// If in check, make sure they are un-checking themselves, else reject
			ChessBoard sim = chessBoard.Clone();
			ChessPiece? simPiece = sim.GetPiece(piece.X, piece.Y);
			if (simPiece == null) return;

			// Simulate the movement and check if still in check
			sim.MovePiece(simPiece, x, y);
			sim.turn = turn;
			// If the user will still be in "Check" after this mvoe, don't allow the user to make this move
			if (sim.IsThereCheck()) {
				MessageBox.Show("Doing that move leaves you in Check");
				return;
			}

			// Deselect the selectedPiece
			ClearSelected();

			// Update the piece's position
			ChessPiece? killedPiece = chessBoard.GetPiece(x, y);

			// Get the image of the piece being moved
			Image pieceImage = imageToChessPieceMap.FirstOrDefault(x => x.Value == piece).Key;

			// Get the Grids of the new and old location
			Grid? newGrid = uniformGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == y && Grid.GetColumn(g) == x);
			Grid? oldGrid = uniformGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == piece.Y && Grid.GetColumn(g) == piece.X);

			if (newGrid != null && oldGrid != null) {
				// If there is a piece to be killed, remove it from the board
				if (killedPiece != null) {
					Image killedImage = imageToChessPieceMap.FirstOrDefault(x => x.Value == killedPiece).Key;

					if (killedImage != null) {
						imageToChessPieceMap.Remove(killedImage);
						newGrid.Children.Remove(killedImage); // Completely remove the killed piece from the UI

						killedImage.Visibility = Visibility.Collapsed;
					}
				}

				// Move the piece within the Board object
				chessBoard.MovePiece(piece, x, y);

				// Remove the pieceImage from the oldGrid
				oldGrid.Children.Remove(pieceImage);

				// Create a new Image with the same source
				Image newPieceImage = new Image {
					Source = pieceImage.Source,
					Stretch = Stretch.Uniform,
					Margin = new Thickness(2),
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center,
				};
				newPieceImage.MouseDown += ChessPiece_MouseDown;

				// Update the image position in the UI
				newPieceImage.SetValue(Grid.RowProperty, y);
				newPieceImage.SetValue(Grid.ColumnProperty, x);

				// Update the mapping in the imageToChessPieceMap dictionary
				imageToChessPieceMap.Remove(pieceImage);
				imageToChessPieceMap[newPieceImage] = piece;
				newGrid.Children.Add(newPieceImage);

				pieceImage.Visibility = Visibility.Collapsed;
			}

			// Switch turn color
			if (turn == ChessPieceColor.White) turn = ChessPieceColor.Black;
			else turn = ChessPieceColor.White;

			CheckForCheck();

		}

		// Check for whether a player is in "Check"
		private void CheckForCheck() {
			whiteCheck = false; blackCheck = false;
			if (turn == ChessPieceColor.White) whiteCheck = chessBoard.IsThereCheck();
			else blackCheck = chessBoard.IsThereCheck();

			// If there is checkmate, print the resepective message and end the game
			if (chessBoard.IsThereCheckMate()) {
				MessageBox.Show("END Of GAME");
				if (turn == ChessPieceColor.White && whiteCheck) MessageBox.Show("CHECKMATE - BLACK WINS!");
				else if (turn == ChessPieceColor.Black && blackCheck) MessageBox.Show("CHECKMATE - WHITE WINS!");
				else MessageBox.Show("STALEMATE - IT'S A DRAW!");
			}
		}

		// Deselect the selectedPiece and remove its possMoves
		private void ClearSelected() {
			if (selectedPiece == null) return;

			// Add the selectedPiece's co-ords to possMoves for resetting the tile colors
			possMoves.Add(selectedPiece.X);
			possMoves.Add(selectedPiece.Y);

			// Iterate through the tiles in possMoves and reset their tile colors
			for (int i = 0; i < possMoves.Count; i += 2) {
				Grid? grid = uniformGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == possMoves[i + 1] && Grid.GetColumn(g) == possMoves[i]);

				if (grid != null) {
					// Find the Rectangle element within the Grid
					Rectangle? rectangle = grid.Children.OfType<Rectangle>().FirstOrDefault();

					if (rectangle != null) {
						// Adjust the mask color based on the original tile color
						if (possMoves[i] % 2 == possMoves[i + 1] % 2) rectangle.Fill = colorLight;
						else rectangle.Fill = colorDark;
					}
				}
			}
			// Reset selectedPiece and possMoves
			selectedPiece = null;
			possMoves = new List<int>();
		}

		// Returns whether moving the selected piece to [x,y] is valid (i.e. is in possMoves)
		private bool MoveIsValid(int x, int y) {
			if (selectedPiece == null || possMoves == null) return false;

			for (int i = 0; i < possMoves.Count; i += 2) {
				if (possMoves[i] == x && possMoves[i + 1] == y) return true;
			}
			return false;
		}

		private void UpdateUI() {
			// Clear all image visibility
			foreach (UIElement child in uniformGrid.Children) {
				if (child is Image image) {
					image.Visibility = Visibility.Collapsed;
				}
			}

			// Show image for each chess piece
			for (int x = 0; x < 8; x++) {
				for (int y = 0; y < 8; y++) {
					ChessPiece? piece = chessBoard.GetPiece(x, y);

					if (piece != null) {
						// Construct the image name dynamically based on piece type and color
						string pieceName = $"{piece.Color.ToString().ToLower()}{piece.Type.ToString().ToLower()}";
						if (piece.Type == ChessPieceType.Pawn) {
							pieceName += (y + 1).ToString(); // Append the pawn number (1 to 8)
						} else {
							pieceName += (x * 8 + y).ToString(); // Append index based on row and column indices
						}

						Image? image = uniformGrid.FindName(pieceName) as Image;
						if (image != null) image.Visibility = Visibility.Visible;

					}
				}
			}
		}
	}
}
