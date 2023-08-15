﻿using System;
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
		private ChessBoard chessBoard;
		private ChessPiece? selectedPiece;
		private List<int> possMoves = new List<int>();
		private Dictionary<Image, ChessPiece> imageToChessPieceMap;
		private ChessPieceColor turn = ChessPieceColor.White;
		private bool whiteCheck = false, blackCheck = false; // whiteCheck shows if white has been checked

		readonly SolidColorBrush colorDark = new SolidColorBrush(Color.FromRgb(110, 110, 110));
		readonly SolidColorBrush colorLight = new SolidColorBrush(Color.FromRgb(210, 210, 210));
		readonly SolidColorBrush selectedDark = new SolidColorBrush(Color.FromRgb(81, 100, 150));
		readonly SolidColorBrush selectedLight = new SolidColorBrush(Color.FromRgb(117, 144, 217));
		readonly SolidColorBrush maskColorDark = new SolidColorBrush(Color.FromRgb(81, 150, 100));
		readonly SolidColorBrush maskColorLight = new SolidColorBrush(Color.FromRgb(117, 217, 144));


		public MainWindow() {
			InitializeComponent();

			// Create the chessboard
			chessBoard = new ChessBoard();
			chessBoard.ResetBoard();

			turn = ChessPieceColor.White;

			imageToChessPieceMap = new Dictionary<Image, ChessPiece>();
			InitializeChessboard();

			UpdateUI();
		}

		private void InitializeChessboard() {
			// Clear the existing children of the uniformGrid
			uniformGrid.Children.Clear();

			for (int y = 0; y < 8; y++) {
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

						imageToChessPieceMap.Add(pieceImage, chessBoard.GetPiece(x, y));
					}

					// Add the grid to the uniformGrid
					uniformGrid.Children.Add(grid);
				}
			}
		}

		private bool HasPiece(int x, int y) {
			// Check if the grid coordinates correspond to a piece position
			return chessBoard.GetPiece(x, y) != null;
		}

		private ImageSource GetPieceImageSource(int x, int y) {
			// Return the appropriate image source based on the piece type and color
			ChessPiece piece = chessBoard.GetPiece(x, y);
			string pieceName = piece.Color.ToNameString() + piece.Type.ToNameString();

			return new BitmapImage(new Uri("pieces/" + pieceName + ".png", UriKind.Relative));
		}


		private void ChessPiece_MouseDown(object sender, MouseButtonEventArgs e) {
			// Get the clicked image
			Image clickedImage = (Image) sender;
			if (selectedPiece != null) {
				ChessPiece? killedPiece = null;
				imageToChessPieceMap.TryGetValue(clickedImage, out killedPiece);
				if (killedPiece == null || killedPiece.Color == selectedPiece.Color) ClearSelected();
				else {
					for (int i = 0; i < possMoves.Count; i += 2) {
						if (possMoves[i] == killedPiece.X && possMoves[i + 1] == killedPiece.Y) {
							MessageBox.Show("Killing " + killedPiece.Color.ToNameString() + killedPiece.Type.ToNameString());

							MovePiece(selectedPiece, killedPiece.X, killedPiece.Y);
							return;
						}
					}
				}
			}

			imageToChessPieceMap.TryGetValue(clickedImage, out selectedPiece);
			int x = selectedPiece.X, y = selectedPiece.Y;
			if (selectedPiece.Color != turn) return;

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

			// If initial click, show avaible squares to go to
			possMoves = selectedPiece.GetPossibleMoves();

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

		private void MovePiece(ChessPiece piece, int x, int y) {
			if (piece.Color != turn || !MoveIsValid(x, y)) return;

			// If in check, make sure they are un-checking themselves, else reject
			ChessBoard sim = chessBoard.Clone();
			ChessPiece simPiece = sim.GetPiece(piece.X, piece.Y);

			// Simulate the movement and check if still in check
			sim.MovePiece(simPiece, x, y);
			sim.turn = turn;
			if (sim.IsThereCheck()) {
				MessageBox.Show("Doing that move leaves you in Check");
				return;
			}

			ClearSelected();

			// Update the piece's position
			ChessPiece? killedPiece = chessBoard.GetPiece(x, y);

			// Get the image of the piece being moved
			Image pieceImage = imageToChessPieceMap.FirstOrDefault(x => x.Value == piece).Key;

			// Get the Grids of the new and old location
			Grid? newGrid = uniformGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == y && Grid.GetColumn(g) == x);
			Grid? oldGrid = uniformGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == piece.Y && Grid.GetColumn(g) == piece.X);

			if (newGrid != null && oldGrid != null) {
				if (killedPiece != null) {
					Image killedImage = imageToChessPieceMap.FirstOrDefault(x => x.Value == killedPiece).Key;

					if (killedImage != null) {
						imageToChessPieceMap.Remove(killedImage);
						newGrid.Children.Remove(killedImage); // Completely remove the killed piece from the UI

						killedImage.Visibility = Visibility.Collapsed;
					}
				}



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

		private void CheckForCheck() {
			whiteCheck = false; blackCheck = false;
			if (turn == ChessPieceColor.White) whiteCheck = chessBoard.IsThereCheck();
			else blackCheck = chessBoard.IsThereCheck();

			if (chessBoard.IsThereCheckMate()) {
				MessageBox.Show("END Of GAME");
				if (turn == ChessPieceColor.White && whiteCheck) MessageBox.Show("CHECKMATE - BLACK WINS!");
				else if (turn == ChessPieceColor.Black && blackCheck) MessageBox.Show("CHECKMATE - WHITE WINS!");
				else MessageBox.Show("STALEMATE - IT'S A DRAW!");
			}
		}

		private void ClearSelected() {
			if (selectedPiece == null) return;

			possMoves.Add(selectedPiece.X);
			possMoves.Add(selectedPiece.Y);

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
					ChessPiece piece = chessBoard.GetPiece(x, y);

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
