namespace Chess {
	public class ChessBoard {
		private ChessPiece[,] board;

		public ChessBoard() {
			board = new ChessPiece[8, 8];
		}

		public void resetBoard() {
			AddPiece(new Rook(0, 0, ChessPieceColor.White, this));
			AddPiece(new Knight(1, 0, ChessPieceColor.White, this));
			AddPiece(new Bishop(2, 0, ChessPieceColor.White, this));
			AddPiece(new Queen(3, 0, ChessPieceColor.White, this));
			AddPiece(new King(4, 0, ChessPieceColor.White, this));
			AddPiece(new Bishop(5, 0, ChessPieceColor.White, this));
			AddPiece(new Knight(6, 0, ChessPieceColor.White, this));
			AddPiece(new Rook(7, 0, ChessPieceColor.White, this));

			AddPiece(new Rook(0, 7, ChessPieceColor.Black, this));
			AddPiece(new Knight(1, 7, ChessPieceColor.Black, this));
			AddPiece(new Bishop(2, 7, ChessPieceColor.Black, this));
			AddPiece(new Queen(3, 7, ChessPieceColor.Black, this));
			AddPiece(new King(4, 7, ChessPieceColor.Black, this));
			AddPiece(new Bishop(5, 7, ChessPieceColor.Black, this));
			AddPiece(new Knight(6, 7, ChessPieceColor.Black, this));
			AddPiece(new Rook(7, 7, ChessPieceColor.Black, this));

			for (int i = 0; i < 8; i++) {
				AddPiece(new Pawn(i, 1, ChessPieceColor.White, this));
				AddPiece(new Pawn(i, 6, ChessPieceColor.Black, this));
			}
		}

		public ChessPiece GetPiece(int x, int y) {
			return board[x, y];
		}

		public void AddPiece(ChessPiece piece) {
			board[piece.X, piece.Y] = piece;
		}

		public bool IsOccupied(int x, int y) {
			if (!IsValidPosition(x, y)) return true;

			return board[x, y] != null;
		}

		public bool IsValidPosition(int x, int y) {
			return (x < 0 || x >= 8 || y < 0 || y >= 8);
		}
	}
}
