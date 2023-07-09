using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Chess {
	public partial class MainWindow : Window {
		private Dictionary<Image, ChessPiece> pieceMap;
		private Image selectedPiece;
		private Point initialPosition;

		public MainWindow() {
			InitializeComponent();
			InitializeChessboard();
		}

		private void InitializeChessboard() {
			pieceMap = new Dictionary<Image, ChessPiece>();

			// Add chess piece images to the chessboard
			AddChessPiece("whiteRook.png", 0, 0, ChessPieceType.Rook, ChessPieceColor.White);
			AddChessPiece("whiteKnight.png", 1, 0, ChessPieceType.Knight, ChessPieceColor.White);
			// Add more chess piece images...

			// Attach event handlers for mouse click events
			foreach (var imagePiece in pieceMap.Keys) {
				imagePiece.MouseLeftButtonDown += ChessPiece_MouseLeftButtonDown;
			}
		}

		private void AddChessPiece(string imagePath, int column, int row, ChessPieceType type, ChessPieceColor color) {
			Image imagePiece = new Image();
			imagePiece.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));

			Grid.SetColumn(imagePiece, column);
			Grid.SetRow(imagePiece, row);

			chessboardGrid.Children.Add(imagePiece);
			pieceMap.Add(imagePiece, new ChessPiece(type, color, column, row));
		}

		private void ChessPiece_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			selectedPiece = (Image) sender;
			initialPosition = e.GetPosition(chessboardGrid);
			// You can perform additional actions when a piece is selected, if needed
		}

		private void chessboardGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
			if (selectedPiece != null) {
				Point currentPosition = e.GetPosition(chessboardGrid);
				double deltaX = currentPosition.X - initialPosition.X;
				double deltaY = currentPosition.Y - initialPosition.Y;

				Canvas.SetLeft(selectedPiece, deltaX);
				Canvas.SetTop(selectedPiece, deltaY);
			}
		}

		private void chessboardGrid_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			if (selectedPiece != null) {
				// Perform move validation and update the piece's position if the move is legal
				// You can access the associated ChessPiece object for the selected piece from the pieceMap dictionary
				// For example: ChessPiece selectedChessPiece = pieceMap[selectedPiece];

				// Reset selectedPiece and initialPosition variables
				selectedPiece = null;
				initialPosition = new Point();
			}
		}
	}

	public enum ChessPieceType {
		Rook,
		Knight,
		Bishop,
		Queen,
		King,
		Pawn
	}

}

