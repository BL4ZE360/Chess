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
		private ChessBoard chessBoard;
		private ChessPiece? selectedPiece;
		private Dictionary<Image, ChessPiece> imageToChessPieceMap;

		SolidColorBrush colorDark = new SolidColorBrush(Color.FromRgb(110, 110, 110));
		SolidColorBrush colorLight = new SolidColorBrush(Color.FromRgb(210, 210, 210));
		SolidColorBrush selectedDark = new SolidColorBrush(Color.FromRgb(81, 100, 150));
		SolidColorBrush selectedLight = new SolidColorBrush(Color.FromRgb(117, 144, 217));
		SolidColorBrush maskColorDark = new SolidColorBrush(Color.FromRgb(81, 150, 100)); // Green translucent mask color
		SolidColorBrush maskColorLight = new SolidColorBrush(Color.FromRgb(117, 217, 144)); // Green translucent mask color
		public MainWindow() {
			InitializeComponent();

			// Create the chessboard
			chessBoard = new ChessBoard();
			chessBoard.ResetBoard();

			imageToChessPieceMap = new Dictionary<Image, ChessPiece>();
			InitializeChessboard();

			UpdateUI();
		}


		private void SetChequers() {
			for (int x = 0; x < 8; x++) {
				for (int y = 0; y < 8; y++) {
					// Find the corresponding Grid element in the UniformGrid based on x and y coordinates
					Grid grid = uniformGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == y && Grid.GetColumn(g) == x);

					if (grid != null) {
						// Find the Rectangle element within the Grid
						Rectangle rectangle = grid.Children.OfType<Rectangle>().FirstOrDefault();

						if (rectangle != null) {
							// Adjust the mask color based on the original tile color
							if (x % 2 == y % 2) rectangle.Fill = colorLight;
							else rectangle.Fill = colorDark;
						}
					}
				}
			}
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
			// Implement your own logic here to determine the image source for the piece at the given position
			// Return the appropriate image source based on the piece type and color
			// Example:
			// return new BitmapImage(new Uri("pieces/whiteRook.png", UriKind.Relative));
			ChessPiece piece = chessBoard.GetPiece(x, y);
			string pieceName = piece.Color.ToNameString() + piece.Type.ToNameString();

			return new BitmapImage(new Uri("pieces/" + pieceName + ".png", UriKind.Relative));
		}


		private void ChessPiece_MouseDown(object sender, MouseButtonEventArgs e) {
			// Get the clicked image
			Image clickedImage = (Image) sender;
			if (selectedPiece != null) ClearSelected();

			imageToChessPieceMap.TryGetValue(clickedImage, out selectedPiece);
			int x = selectedPiece.X, y = selectedPiece.Y;

			Grid grid = uniformGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == y && Grid.GetColumn(g) == x);

			if (grid != null) {
				// Find the Rectangle element within the Grid
				Rectangle rectangle = grid.Children.OfType<Rectangle>().FirstOrDefault();

				if (rectangle != null) {
					// Adjust the mask color based on the original tile color
					if (x % 2 == y % 2) rectangle.Fill = selectedLight;
					else rectangle.Fill = selectedDark;
				}
			}

			// If initial click, show avaible squares to go to
			List<int> possMoves = selectedPiece.GetPossibleMoves();

			for (int i = 0; i < possMoves.Count; i += 2) {
				x = possMoves[i];
				y = possMoves[i + 1];

				// Find the corresponding Grid element in the UniformGrid based on x and y coordinates
				grid = uniformGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == y && Grid.GetColumn(g) == x);

				if (grid != null) {
					// Find the Rectangle element within the Grid
					Rectangle rectangle = grid.Children.OfType<Rectangle>().FirstOrDefault();

					if (rectangle != null) {
						// Adjust the mask color based on the original tile color
						if (x % 2 == y % 2) rectangle.Fill = maskColorLight;
						else rectangle.Fill = maskColorDark;
					}
				}
			}
		}

		private void ClearSelected() {
			if (selectedPiece == null) return;

			SetChequers();
			selectedPiece = null;
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

						Image image = uniformGrid.FindName(pieceName) as Image;
						if (image != null) {
							image.Visibility = Visibility.Visible;
						}
					}
				}
			}
		}
	}
}
