using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Chess {
	public partial class MainWindow : Window {
		private ChessBoard chessBoard;
		private ChessPiece selectedPiece;

		public MainWindow() {
			InitializeComponent();

			// Create the chessboard
			chessBoard = new ChessBoard();
			chessBoard.ResetBoard();

			UpdateUI();
		}

		private void ChessPiece_MouseDown(object sender, MouseButtonEventArgs e) {
			// Get the clicked image
			Image clickedImage = (Image) sender;

			// Extract the piece type from the image name
			string imageName = clickedImage.Name;

			// Determine the piece color and type from the image name
			string colorString = imageName.Contains("white") ? "White" : "Black";
			string pieceTypeName = imageName.Replace(colorString.ToLower(), "");
			string pieceTypeString = new string(pieceTypeName.Where(c => !Char.IsDigit(c)).ToArray());


			// Convert the piece type string to the ChessPieceType enum value
			ChessPieceType pieceType;
			if (!Enum.TryParse(pieceTypeString, out pieceType)) {
				// Invalid piece type
				MessageBox.Show("Invalid piece type - ", pieceTypeString);
				return;
			}

			// Determine the piece color based on the image name
			ChessPieceColor pieceColor = colorString == "White" ? ChessPieceColor.White : ChessPieceColor.Black;

			// Handle the piece type and color accordingly
			switch (pieceType) {
				case ChessPieceType.Pawn:
					// Handle pawn logic
					break;
				case ChessPieceType.Rook:
					// Handle rook logic
					break;
					// Handle other piece types
			}
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
