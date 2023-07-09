using System;

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
	}
}