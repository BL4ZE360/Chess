using System;
using System.Collections.Generic;

namespace Chess {
	public enum ChessPieceColor {
		White,
		Black
	}
	public enum ChessPieceType {
		Rook,
		Knight,
		Bishop,
		Queen,
		King,
		Pawn
	}
	public static class EnumExtensions {
		public static string ToNameString(this ChessPieceColor color) {
			string colorName = Enum.GetName(typeof(ChessPieceColor), color);
			return colorName.ToLower();
		}

		public static string ToNameString(this ChessPieceType type) {
			string typeName = Enum.GetName(typeof(ChessPieceType), type);
			return $"{char.ToUpper(typeName[0])}{typeName.Substring(1)}";
		}
	}



	public abstract class ChessPiece {
		public ChessPieceColor Color { get; protected set; }
		public ChessPieceType Type { get; protected set; }
		public int X { get; set; }
		public int Y { get; set; }
		public ChessBoard Board { get; set; }
		public ChessPiece(ChessPieceColor color, ChessPieceType type, int x, int y, ChessBoard board) {
			Color = color;
			Type = type;
			X = x;
			Y = y;
			Board = board;
		}

		public abstract bool IsValidMove(int newX, int newY);
		public abstract List<int> GetPossibleMoves();
		public abstract ChessPiece Clone();
	}

	public class Rook : ChessPiece {
		public Rook(int x, int y, ChessPieceColor color, ChessBoard board) : base(color, ChessPieceType.Rook, x, y, board) {
		}

		public override bool IsValidMove(int newX, int newY) {
			if (!Board.IsValidPosition(newX, newY)) return false;
			int xDir = (newX > X) ? 1 : (newX < X) ? -1 : 0;
			int yDir = (newY > Y) ? 1 : (newY < Y) ? -1 : 0;

			// Rook-specific move validation logic
			if (xDir != 0 && yDir == 0) {
				for (int i = X + xDir; i < newX; i++) {
					if (Board.IsOccupied(i, newY)) return false;
				}
			} else if (xDir == 0 && yDir != 0) {
				for (int i = Y + yDir; i < newY; i++) {
					if (Board.IsOccupied(newX, i)) return false;
				}
			} else { return false; }

			return true;
		}

		public override List<int> GetPossibleMoves() {
			List<int> squares = new List<int>();
			bool xNeg = true, xPos = true, yNeg = true, yPos = true;
			for (int i = 1; i < 8; i++) {
				if (xNeg && i <= X) {
					if (Board.IsOccupied(X - i, Y)) {
						xNeg = false;
						if (Board.GetPiece(X - i, Y).Color != Color) { squares.Add(X - i); squares.Add(Y); }
					} else { squares.Add(X - i); squares.Add(Y); }
				}
				if (xPos && i <= 7 - X) {
					if (Board.IsOccupied(X + i, Y)) {
						xPos = false;
						if (Board.GetPiece(X + i, Y).Color != Color) { squares.Add(X + i); squares.Add(Y); }
					} else { squares.Add(X + i); squares.Add(Y); }
				}
				if (yNeg && i <= Y) {
					if (Board.IsOccupied(X, Y - i)) {
						yNeg = false;
						if (Board.GetPiece(X, Y - i).Color != Color) { squares.Add(X); squares.Add(Y - i); }
					} else { squares.Add(X); squares.Add(Y - i); }
				}
				if (yPos && i <= 7 - Y) {
					if (Board.IsOccupied(X, Y + i)) {
						yPos = false;
						if (Board.GetPiece(X, Y + i).Color != Color) { squares.Add(X); squares.Add(Y + i); }
					} else { squares.Add(X); squares.Add(Y + i); }
				}
			}
			return squares;
		}

		public override ChessPiece Clone() {
			return new Rook(X, Y, Color, Board);
		}
	}

	public class Knight : ChessPiece {
		public Knight(int x, int y, ChessPieceColor color, ChessBoard board) : base(color, ChessPieceType.Knight, x, y, board) {
		}

		public override bool IsValidMove(int newX, int newY) {
			if (!Board.IsValidPosition(newX, newY)) return false;
			int diffX = Math.Abs(newX - X);
			int diffY = Math.Abs(newY - Y);

			// Knight-specific move validation logic
			return (diffX == 2 && diffY == 1) || (diffX == 1 && diffY == 2);
		}

		public override List<int> GetPossibleMoves() {
			List<int> squares = new List<int>();
			int[,] moves = new int[8, 2] {{X - 2, Y - 1}, {X - 2, Y + 1},  {X - 1, Y - 2}, {X - 1, Y + 2},
					{X + 1, Y - 2}, {X + 1, Y + 2}, {X + 2, Y - 1}, {X + 2, Y + 1}};

			for (int i = 0; i < 7; i++) {
				int x = moves[i, 0];
				int y = moves[i, 1];
				if (!Board.IsValidPosition(x, y)) continue;
				if (!Board.IsOccupied(x, y) || Board.GetPiece(x, y).Color != Color) { squares.Add(x); squares.Add(y); }
			}

			return squares;
		}
		public override ChessPiece Clone() {
			return new Knight(X, Y, Color, Board);
		}
	}

	public class Bishop : ChessPiece {
		public Bishop(int x, int y, ChessPieceColor color, ChessBoard board) : base(color, ChessPieceType.Bishop, x, y, board) {
		}

		public override bool IsValidMove(int newX, int newY) {
			if (!Board.IsValidPosition(newX, newY)) return false;
			int diffX = Math.Abs(newX - X);
			int diffY = Math.Abs(newY - Y);

			// Bishop-specific move validation logic
			if (diffX == diffY && diffX * diffY != 0) {
				int xDir = (newX > X) ? 1 : -1;
				int yDir = (newY > Y) ? 1 : -1;

				for (int i = 1; i < diffX; i++) {
					if (Board.IsOccupied(X + (i * xDir), Y + (i * yDir))) return false;
				}

				return true;
			}
			return false;
		}

		public override List<int> GetPossibleMoves() {
			List<int> squares = new List<int>();
			bool topLeft = true, bottomLeft = true, topRight = true, bottomRight = true;
			for (int i = 1; i < 8; i++) {
				if (topLeft && i <= Math.Min(X, Y)) {
					if (Board.IsOccupied(X - i, Y - i)) {
						topLeft = false;
						if (Board.GetPiece(X - i, Y - i).Color != Color) { squares.Add(X - i); squares.Add(Y - i); }
					} else { squares.Add(X - i); squares.Add(Y - i); }
				}
				if (bottomLeft && i <= Math.Min(7 - X, 7 - Y)) {
					if (Board.IsOccupied(X + i, Y + i)) {
						bottomLeft = false;
						if (Board.GetPiece(X + i, Y + i).Color != Color) { squares.Add(X + i); squares.Add(Y + i); }
					} else { squares.Add(X + i); squares.Add(Y + i); }
				}
				if (topRight && i <= Math.Min(7 - X, Y)) {
					if (Board.IsOccupied(X + i, Y - i)) {
						topRight = false;
						if (Board.GetPiece(X + i, Y - i).Color != Color) { squares.Add(X + i); squares.Add(Y - i); }
					} else { squares.Add(X + i); squares.Add(Y - i); }
				}
				if (bottomRight && i <= Math.Min(X, 7 - Y)) {
					if (Board.IsOccupied(X - i, Y + i)) {
						bottomRight = false;
						if (Board.GetPiece(X - i, Y + i).Color != Color) { squares.Add(X - i); squares.Add(Y + i); }
					} else { squares.Add(X - i); squares.Add(Y + i); }
				}
			}
			return squares;
		}
		public override ChessPiece Clone() {
			return new Bishop(X, Y, Color, Board);
		}
	}

	public class King : ChessPiece {
		public King(int x, int y, ChessPieceColor color, ChessBoard board) : base(color, ChessPieceType.King, x, y, board) {
		}

		public override bool IsValidMove(int newX, int newY) {
			if (!Board.IsValidPosition(newX, newY)) return false;
			int diffX = Math.Abs(newX - X);
			int diffY = Math.Abs(newY - Y);

			// King-specific move validation logic
			return (diffY < 2 && diffX < 2 && diffX + diffY > 0);
		}

		public override List<int> GetPossibleMoves() {
			List<int> squares = new List<int>();
			int[,] moves = new int[8, 2] {{X - 1, Y - 1}, {X - 1, Y},  {X - 1, Y + 1}, {X, Y - 1},
					{X, Y + 1}, {X + 1, Y - 1}, {X + 1, Y}, {X + 1, Y + 1}};

			for (int i = 0; i < 7; i++) {
				int x = moves[i, 0];
				int y = moves[i, 1];
				if (!Board.IsValidPosition(x, y)) continue;
				if (!Board.IsOccupied(x, y) || Board.GetPiece(x, y).Color != Color) { squares.Add(x); squares.Add(y); }
			}

			return squares;
		}
		public override ChessPiece Clone() {
			return new King(X, Y, Color, Board);
		}
	}

	public class Queen : ChessPiece {
		public Queen(int x, int y, ChessPieceColor color, ChessBoard board) : base(color, ChessPieceType.Queen, x, y, board) {
		}

		public override bool IsValidMove(int newX, int newY) {
			if (!Board.IsValidPosition(newX, newY)) return false;
			int diffX = Math.Abs(newX - X);
			int diffY = Math.Abs(newY - Y);

			// Queen-specific move validation logic
			if ((diffX == diffY || diffX * diffY == 0) && (diffX > 0 || diffY > 0)) {
				int xDir = (newX > X) ? 1 : (newX < X) ? -1 : 0;
				int yDir = (newY > Y) ? 1 : (newY < Y) ? -1 : 0;

				for (int i = 1; i < Math.Max(diffX, diffY); i++) {
					if (Board.IsOccupied(X + (i * xDir), Y + (i * yDir))) return false;
				}

				return true;
			}

			return false;
		}

		public override List<int> GetPossibleMoves() {
			List<int> squares = new List<int>();
			bool xNeg = true, xPos = true, yNeg = true, yPos = true;
			bool topLeft = true, bottomLeft = true, topRight = true, bottomRight = true;
			for (int i = 1; i < 8; i++) {
				if (xNeg && i <= X) {
					if (Board.IsOccupied(X - i, Y)) {
						xNeg = false;
						if (Board.GetPiece(X - i, Y).Color != Color) { squares.Add(X - i); squares.Add(Y); }
					} else { squares.Add(X - i); squares.Add(Y); }
				}
				if (xPos && i <= 7 - X) {
					if (Board.IsOccupied(X + i, Y)) {
						xPos = false;
						if (Board.GetPiece(X + i, Y).Color != Color) { squares.Add(X + i); squares.Add(Y); }
					} else { squares.Add(X + i); squares.Add(Y); }
				}
				if (yNeg && i <= Y) {
					if (Board.IsOccupied(X, Y - i)) {
						yNeg = false;
						if (Board.GetPiece(X, Y - i).Color != Color) { squares.Add(X); squares.Add(Y - i); }
					} else { squares.Add(X); squares.Add(Y - i); }
				}
				if (yPos && i <= 7 - Y) {
					if (Board.IsOccupied(X, Y + i)) {
						yPos = false;
						if (Board.GetPiece(X, Y + i).Color != Color) { squares.Add(X); squares.Add(Y + i); }
					} else { squares.Add(X); squares.Add(Y + i); }
				}
				if (topLeft && i <= Math.Min(X, Y)) {
					if (Board.IsOccupied(X - i, Y - i)) {
						topLeft = false;
						if (Board.GetPiece(X - i, Y - i).Color != Color) { squares.Add(X - i); squares.Add(Y - i); }
					} else { squares.Add(X - i); squares.Add(Y - i); }
				}
				if (bottomLeft && i <= Math.Min(7 - X, 7 - Y)) {
					if (Board.IsOccupied(X + i, Y + i)) {
						bottomLeft = false;
						if (Board.GetPiece(X + i, Y + i).Color != Color) { squares.Add(X + i); squares.Add(Y + i); }
					} else { squares.Add(X + i); squares.Add(Y + i); }
				}
				if (topRight && i <= Math.Min(7 - X, Y)) {
					if (Board.IsOccupied(X + i, Y - i)) {
						topRight = false;
						if (Board.GetPiece(X + i, Y - i).Color != Color) { squares.Add(X + i); squares.Add(Y - i); }
					} else { squares.Add(X + i); squares.Add(Y - i); }
				}
				if (bottomRight && i <= Math.Min(X, 7 - Y)) {
					if (Board.IsOccupied(X - i, Y + i)) {
						bottomRight = false;
						if (Board.GetPiece(X - i, Y + i).Color != Color) { squares.Add(X - i); squares.Add(Y + i); }
					} else { squares.Add(X - i); squares.Add(Y + i); }
				}
			}
			return squares;
		}
		public override ChessPiece Clone() {
			return new Queen(X, Y, Color, Board);
		}
	}

	public class Pawn : ChessPiece {
		public Pawn(int x, int y, ChessPieceColor color, ChessBoard board) : base(color, ChessPieceType.Pawn, x, y, board) {
		}

		public override bool IsValidMove(int newX, int newY) {
			if (!Board.IsValidPosition(newX, newY)) return false;
			int yDir = (Color == ChessPieceColor.White) ? 1 : -1;
			int startY = (Color == ChessPieceColor.White) ? 1 : 6;

			// Pawn-specific move validation logic
			if ((newY == Y + yDir && (
					(newX == X && !Board.IsOccupied(newX, newY))
					|| (Math.Abs(newX - X) == 1 && Board.IsOccupied(newX, newY))))
				|| (newY == Y + 2 * yDir && newX == X && !Board.IsOccupied(newX, newY) && !Board.IsOccupied(newX, Y + yDir) && Y == startY)) {
				return true;
			} else { return false; }
		}

		public override List<int> GetPossibleMoves() {
			List<int> squares = new List<int>();
			int[,] moves = new int[8, 2] {{X - 1, Y + 1}, {X - 1, Y - 1}, {X, Y + 1}, {X, Y + 2},
					{X, Y - 1}, {X, Y - 2}, {X + 1, Y + 1}, {X + 1, Y - 1}};

			for (int i = 0; i < 7; i++) {
				int x = moves[i, 0];
				int y = moves[i, 1];
				if (IsValidMove(x, y)) { squares.Add(x); squares.Add(y); }
			}

			return squares;
		}
		public override ChessPiece Clone() {
			return new Pawn(X, Y, Color, Board);
		}
	}
}