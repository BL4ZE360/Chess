﻿using System.Collections.Generic;

namespace Chess {
	public class ChessBoard {
		private ChessPiece?[,] board;
		public ChessPieceColor turn = ChessPieceColor.White;

		public ChessBoard() {
			board = new ChessPiece[8, 8];
			turn = ChessPieceColor.White;
		}

		public ChessBoard Clone() {
			ChessBoard sim = new ChessBoard();
			sim.turn = this.turn;
			for (int row = 0; row < 8; row++) {
				for (int col = 0; col < 8; col++) {
					if (this.board[row, col] != null) {
						switch (this.board[row, col].Type) {
							case ChessPieceType.Rook: sim.AddPiece(new Rook(row, col, this.board[row, col].Color, sim)); break;
							case ChessPieceType.Knight: sim.AddPiece(new Knight(row, col, this.board[row, col].Color, sim)); break;
							case ChessPieceType.Bishop: sim.AddPiece(new Bishop(row, col, this.board[row, col].Color, sim)); break;
							case ChessPieceType.Queen: sim.AddPiece(new Queen(row, col, this.board[row, col].Color, sim)); break;
							case ChessPieceType.King: sim.AddPiece(new King(row, col, this.board[row, col].Color, sim)); break;
							case ChessPieceType.Pawn: sim.AddPiece(new Pawn(row, col, this.board[row, col].Color, sim)); break;
						}
					}
				}
			}
			return sim;
		}

		public void ResetBoard() {
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

			turn = ChessPieceColor.White;
		}

		public ChessPiece? GetPiece(int x, int y) {
			return board[x, y];
		}

		public void AddPiece(ChessPiece piece) {
			board[piece.X, piece.Y] = piece;
		}

		public void MovePiece(ChessPiece piece, int x, int y) {
			// Move piece across the board
			board[piece.X, piece.Y] = null;
			piece.X = x; piece.Y = y;
			board[x, y] = piece;

			if (turn == ChessPieceColor.White) turn = ChessPieceColor.Black;
			else turn = ChessPieceColor.White;
		}

		public bool IsOccupied(int x, int y) {
			if (!IsValidPosition(x, y)) return true;

			return board[x, y] != null;
		}

		public bool IsValidPosition(int x, int y) {
			return (x >= 0 && x < 8 && y >= 0 && y < 8);
		}

		public bool IsThereCheck() {
			// Get this King's position
			int[] kingCoords = new int[2];
			for (int row = 0; row < 8; row++) {
				for (int col = 0; col < 8; col++) {
					if (board[row, col] != null && board[row, col].Color == turn && board[row, col].Type == ChessPieceType.King) {
						kingCoords[0] = row; kingCoords[1] = col;
						break;
					}
				}
			}

			// Go through all simulations of re-moving the color just gone and see if opponents king can be taken
			for (int row = 0; row < 8; row++) {
				for (int col = 0; col < 8; col++) {
					if (board[row, col] != null && board[row, col].Color != turn) {
						List<int> moves = board[row, col].GetPossibleMoves();
						for (int i = 0; i < moves.Count; i += 2) {
							if (moves[i] == kingCoords[0] && moves[i + 1] == kingCoords[1]) return true;
						}
					}
				}
			}
			return false;
		}

		public bool IsThereCheckMate() {
			// Simulate moving each piece and see if all simulations result in being in check
			for (int row = 0; row < 8; row++) {
				for (int col = 0; col < 8; col++) {
					if (board[row, col] != null && board[row, col].Color == turn) {
						List<int> moves = board[row, col].GetPossibleMoves();
						for (int i = 0; i < moves.Count; i += 2) {
							ChessBoard sim = this.Clone();
							ChessPiece piece = sim.GetPiece(row, col);

							sim.MovePiece(piece, moves[i], moves[i + 1]);
							if (!sim.IsThereCheck()) return false;
						}
					}
				}
			}
			return true;
		}
	}
}
