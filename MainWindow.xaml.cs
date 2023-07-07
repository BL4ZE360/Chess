using System;
using System.Windows;

namespace Chess {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			ChessBoard chessBoard = new ChessBoard();

			InitializeComponent();
		}

		public class ChessBoard {
			private ChessPiece[,] board;

			public ChessBoard() {
				board = new ChessPiece[8, 8];
				resetBoard();
			}

			public void resetBoard() {
				AddPiece(new Rook(0, 0, "White"));
				AddPiece(new Knight(1, 0, "White"));
				AddPiece(new Bishop(2, 0, "White"));
				AddPiece(new Queen(3, 0, "White"));
				AddPiece(new King(4, 0, "White"));
				AddPiece(new Bishop(5, 0, "White"));
				AddPiece(new Knight(6, 0, "White"));
				AddPiece(new Rook(7, 0, "White"));

				AddPiece(new Rook(0, 7, "Black"));
				AddPiece(new Knight(1, 7, "Black"));
				AddPiece(new Bishop(2, 7, "Black"));
				AddPiece(new Queen(3, 7, "Black"));
				AddPiece(new King(4, 7, "Black"));
				AddPiece(new Bishop(5, 7, "Black"));
				AddPiece(new Knight(6, 7, "Black"));
				AddPiece(new Rook(7, 7, "Black"));

				for (int i = 0; i < 8; i++) {
					AddPiece(new Pawn(i, 1, "White"));
					AddPiece(new Pawn(i, 6, "Black"));
				}
			}

			public void AddPiece(ChessPiece piece) {
				int x = piece.X, y = piece.Y;
				if (IsValidPosition(x, y) && !IsOccupied(x, y)) board[x, y] = piece;
				else throw new ArgumentException("Invalid position.");
			}

			public ChessPiece GetPiece(int x, int y) {
				if (IsValidPosition(x, y)) {
					return board[x, y];
				} else {
					throw new ArgumentException("Invalid position.");
				}
			}

			public bool IsValidPosition(int x, int y) {
				return x >= 0 && x < 8 && y >= 0 && y < 8;
			}

			public bool IsOccupied(int x, int y) {
				ChessPiece piece = GetPiece(x, y);
				if (piece is not null) return true;
				return false;
			}

			public override string ToString() {
				// Return a string representation of the chess board
				// Format it according to your requirements
				string rep = "";
				for (int i = 0; i < 8; i++) {
					if (i > 0) rep += "\n---------------";
					for (int j = 0; j < 8; j++) {
						if (j == 0) rep += "\n";
						else rep += "|";
						ChessPiece piece = board[j, i];
						if (piece is null) rep += " ";
						else rep += piece.Name[0];
					}
				}
				return rep;
			}
		}


		public abstract class ChessPiece {
			public string Name { get; set; }
			public int X { get; set; }
			public int Y { get; set; }
			public string Color { get; set; }

			public ChessPiece(string name, int x, int y, string color) {
				Name = name;
				X = x;
				Y = y;
				Color = color;
			}

			public abstract bool IsValidMove(ChessBoard board, int newX, int newY);
		}

		public class Rook : ChessPiece {
			public Rook(int x, int y, string color) : base("Rook", x, y, color) {
			}

			public override bool IsValidMove(ChessBoard board, int newX, int newY) {
				if (!board.IsValidPosition(newX, newY)) return false;
				int xDir = (newX > X) ? 1 : (newX < X) ? -1 : 0;
				int yDir = (newY > Y) ? 1 : (newY < Y) ? -1 : 0;

				// Rook-specific move validation logic
				if (xDir != 0 && yDir == 0) {
					for (int i = X + xDir; i < newX; i++) {
						if (board.IsOccupied(i, newY)) return false;
					}
				} else if (xDir == 0 && yDir != 0) {
					for (int i = Y + yDir; i < newY; i++) {
						if (board.IsOccupied(newX, i)) return false;
					}
				} else { return false; }

				return true;

			}
		}

		public class Knight : ChessPiece {
			public Knight(int x, int y, string color) : base("Knight", x, y, color) {
			}

			public override bool IsValidMove(ChessBoard board, int newX, int newY) {
				if (!board.IsValidPosition(newX, newY)) return false;
				int diffX = Math.Abs(newX - X);
				int diffY = Math.Abs(newY - Y);

				// Knight-specific move validation logic
				return (diffX == 2 && diffY == 1) || (diffX == 1 && diffY == 2);
			}
		}

		public class Bishop : ChessPiece {
			public Bishop(int x, int y, string color) : base("Bishop", x, y, color) {
			}

			public override bool IsValidMove(ChessBoard board, int newX, int newY) {
				if (!board.IsValidPosition(newX, newY)) return false;
				int diffX = Math.Abs(newX - X);
				int diffY = Math.Abs(newY - Y);

				// Bishop-specific move validation logic
				if (diffX == diffY && diffX * diffY != 0) {
					int xDir = (newX > X) ? 1 : -1;
					int yDir = (newY > Y) ? 1 : -1;

					for (int i = 1; i < diffX; i++) {
						if (board.IsOccupied(X + (i * xDir), Y + (i * yDir))) return false;
					}

					return true;
				}
				return false;
			}
		}

		public class King : ChessPiece {
			public King(int x, int y, string color) : base("King", x, y, color) {
			}

			public override bool IsValidMove(ChessBoard board, int newX, int newY) {
				if (!board.IsValidPosition(newX, newY)) return false;
				int diffX = Math.Abs(newX - X);
				int diffY = Math.Abs(newY - Y);

				// King-specific move validation logic
				return (diffY < 2 && diffX < 2 && diffX + diffY > 0);
			}
		}

		public class Queen : ChessPiece {
			public Queen(int x, int y, string color) : base("Queen", x, y, color) {
			}

			public override bool IsValidMove(ChessBoard board, int newX, int newY) {
				if (!board.IsValidPosition(newX, newY)) return false;
				int diffX = Math.Abs(newX - X);
				int diffY = Math.Abs(newY - Y);

				// Queen-specific move validation logic
				if ((diffX == diffY || diffX * diffY == 0) && (diffX > 0 || diffY > 0)) {
					int xDir = (newX > X) ? 1 : (newX < X) ? -1 : 0;
					int yDir = (newY > Y) ? 1 : (newY < Y) ? -1 : 0;

					for (int i = 1; i < Math.Max(diffX, diffY); i++) {
						if (board.IsOccupied(X + (i * xDir), Y + (i * yDir))) return false;
					}

					return true;
				}

				return false;
			}
		}

		public class Pawn : ChessPiece {
			public Pawn(int x, int y, string color) : base("Pawn", x, y, color) {
			}

			public override bool IsValidMove(ChessBoard board, int newX, int newY) {
				if (!board.IsValidPosition(newX, newY)) return false;
				int yDir = (this.Color == "White") ? 1 : -1;
				int startY = (this.Color == "White") ? 1 : 6;

				// Pawn-specific move validation logic
				if ((newY == Y + yDir && (
						(newX == X && !board.IsOccupied(newX, newY))
						|| (Math.Abs(newX - X) == 1 && board.IsOccupied(newX, newY))))
					|| (newY == Y + 2 * yDir && newX == X && !board.IsOccupied(newX, newY) && !board.IsOccupied(newX, Y + yDir) && Y == startY)) {
					return true;
				} else { return false; }
			}
		}
	}
}
