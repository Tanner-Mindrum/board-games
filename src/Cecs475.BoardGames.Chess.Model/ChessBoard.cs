using System;
using System.Collections.Generic;
using System.Text;
using Cecs475.BoardGames.Model;
using System.Linq;

namespace Cecs475.BoardGames.Chess.Model {
	/// <summary>
	/// Represents the board state of a game of chess. Tracks which squares of the 8x8 board are occupied
	/// by which player's pieces.
	/// </summary>
	public class ChessBoard : IGameBoard {
		#region Member fields.
		// The history of moves applied to the board.
		private List<ChessMove> mMoveHistory = new List<ChessMove>();

		public const int BoardSize = 8;

		// TODO: create a field for the board position array. You can hand-initialize
		// the starting entries of the array, or set them in the constructor.
		private byte[] mBoard;

		// TODO: Add a means of tracking miscellaneous board state, like captured pieces and the 50-move rule.
		private int drawCount = 0;

		// TODO: add a field for tracking the current player and the board advantage.		
		private int currentPlayer = 1;
		private int advantage;
		private List<GameState> gameStateList = new List<GameState>();
		private GameState gameState;
		private bool leftWRook = true;
		private bool rightWRook = true;
		private bool leftBRook = true;
		private bool rightBRook = true;
		private bool pawnMovedTwoSpaces = false;
		private BoardPosition pawnMovedTwoSpacesPos = new BoardPosition(0, 0);
		private ChessPiece pawnPromote = new ChessPiece();
		private bool whiteKingHasMoved = false;
		private bool blackKingHasMoved = false;

		#endregion

		#region Properties.
		// TODO: implement these properties.
		// You can choose to use auto properties, computed properties, or normal properties 
		// using a private field to back the property.

		// You can add set bodies if you think that is appropriate, as long as you justify
		// the access level (public, private).

		public bool IsFinished {
			get {
				return IsCheckmate || IsStalemate || IsDraw;
				//return GetPossibleMoves().Any() || IsDraw;
			}
		}

		public int CurrentPlayer { get { return currentPlayer; } }

		public GameAdvantage CurrentAdvantage {
			get {
				if (advantage < 0) {
					return new GameAdvantage(2, advantage * -1);
				}
				else if (advantage > 0) {
					return new GameAdvantage(1, advantage);
				}
				else {
					return new GameAdvantage(0, 0);
				}
			}
		}

		public IReadOnlyList<ChessMove> MoveHistory => mMoveHistory;

		// TODO: implement IsCheck, IsCheckmate, IsStalemate
		public bool IsCheck {
			get {
				if (IsCheckmate) {
					return false;
				}

				var positions = BoardPosition.GetRectangularPositions(8, 8);
				foreach (BoardPosition p in positions) {
					ChessPiece piece = GetPieceAtPosition(p);
					if (piece.PieceType == ChessPieceType.King) {
						if (piece.Player == 1) {
							if (PositionIsAttacked(p, 2)) {
								return true;
							}
						}
						else if (piece.Player == 2) {
							if (PositionIsAttacked(p, 1)) {
								return true;
							}
						}
					}
				}

				return false;
			}
		}

		public bool IsCheckmate {
			get {
				IEnumerable<ChessMove> possMoves = GetPossibleMoves();
				var positions = BoardPosition.GetRectangularPositions(8, 8);
				bool inCheck = false;
				foreach (BoardPosition p in positions) {
					//if (!inCheck) {
					ChessPiece piece = GetPieceAtPosition(p);

					ChessPiece testPiece = GetPieceAtPosition(new BoardPosition(7, 3));

					if (testPiece.PieceType == ChessPieceType.Queen && testPiece.Player == 2) { }

					if (piece.PieceType == ChessPieceType.King) {
						if (piece.Player == 1) {
							if (PositionIsAttacked(p, 2)) {
								inCheck = true;
							}
						}
						else if (piece.Player == 2) {
							if (PositionIsAttacked(p, 1)) {
								inCheck = true;
							}
						}
					}

					if (inCheck) {
						if (!possMoves.Any()) {
							return true;
						}
						foreach (ChessMove m in possMoves) {
							ApplyMove(m);
							if (!PositionIsAttacked(m.EndPosition, currentPlayer)) {
								UndoLastMove();
								return false;
							}
							UndoLastMove();
						}
					}
					//}
					inCheck = false;
				}
				return inCheck;
			}
		}

		public bool IsStalemate {
			get {
				bool inCheck = false;
				var positions = BoardPosition.GetRectangularPositions(8, 8);
				foreach (BoardPosition p in positions) {
					if (!inCheck) {
						ChessPiece piece = GetPieceAtPosition(p);
						if (piece.PieceType == ChessPieceType.King) {
							if (piece.Player == 1) {
								if (PositionIsAttacked(p, 2)) {
									inCheck = true;
								}
							}
							else if (piece.Player == 2) {
								if (PositionIsAttacked(p, 1)) {
									inCheck = true;
								}
							}
						}
					}
				}
				if (!inCheck) {
					return !GetPossibleMoves().Any();
				}
				else {
					return false;
				}
			}
		}

		public bool IsDraw {
			get { return drawCount >= 100; }
		}

		private struct GameState {
			public ChessPiece pieceTaken;
			public ChessMove move;
			public bool WhiteCanCastle;
			public bool BlackCanCastle;
			public int drawCounter;
			public bool pawnMovedTwoSpaces;
			public BoardPosition pawnMovedTwoSpacesPos;
			public ChessPiece pawnPromote;
			public bool leftWRook;
			public bool rightWRook;
			public bool leftBRook;
			public bool rightBRook;
			public bool whiteKingHasMoved;
			public bool blackKingHasMoved;
			public int advantage;
		}


		/// <summary>
		/// Tracks the current draw counter, which goes up by 1 for each non-capturing, non-pawn move, and resets to 0
		/// for other moves. If the counter reaches 100 (50 full turns), the game is a draw.
		/// </summary>
		public int DrawCounter {
			get { return drawCount; }
		}
		#endregion


		#region Public methods.
		public IEnumerable<ChessMove> GetPossibleMoves() {
			/*if (mMoves != null) return mMoves*/
			ISet<ChessMove> possibleMoves = new HashSet<ChessMove>();
			var positions = BoardPosition.GetRectangularPositions(8, 8);
			HashSet<BoardPosition> attackPositions = new HashSet<BoardPosition>();
			ISet<BoardPosition> possiblePositions = new HashSet<BoardPosition>();
			BoardPosition kingPos = new BoardPosition();

			var piece2 = GetPieceAtPosition(new BoardPosition(0, 6));
			if (piece2.PieceType == ChessPieceType.Empty) { }

			attackPositions.UnionWith(GetAttackedPositions(currentPlayer));

			foreach (BoardPosition p in attackPositions) {
				ChessPiece piece = GetPieceAtPosition(p);
				if (piece.Player != currentPlayer || piece.PieceType == ChessPieceType.Empty) {
					possiblePositions.Add(p);
				}
			}

			IEnumerable<BoardPosition> validMovesPerPiece = new HashSet<BoardPosition>();
			foreach (BoardPosition p in positions) {
				ChessPiece piece = GetPieceAtPosition(p);
				if (piece.PieceType == ChessPieceType.Knight && piece.Player == currentPlayer) {
					validMovesPerPiece = possiblePositions.Intersect(KnightAttackPos(p));
					foreach (BoardPosition v in validMovesPerPiece) {
						possibleMoves.Add(new ChessMove(p, v));
					}
				}
				else if (piece.PieceType == ChessPieceType.Queen && piece.Player == currentPlayer) {
					validMovesPerPiece = possiblePositions.Intersect(QueenAttackPos(p));
					foreach (BoardPosition v in validMovesPerPiece) {
						possibleMoves.Add(new ChessMove(p, v));
					}
				}
				else if (piece.PieceType == ChessPieceType.Bishop && piece.Player == currentPlayer) {
					validMovesPerPiece = possiblePositions.Intersect(BishopAttackPos(p));
					foreach (BoardPosition v in validMovesPerPiece) {
						possibleMoves.Add(new ChessMove(p, v));
					}
				}
				else if (piece.PieceType == ChessPieceType.King && piece.Player == currentPlayer) {
					kingPos = p;

					int emptyCount = 0;
					// White castle :P
					if (!whiteKingHasMoved && rightWRook && currentPlayer == 1
						&& GetPieceAtPosition(p).Player == currentPlayer
						&& GetPieceAtPosition(new BoardPosition(7, 4)).PieceType == ChessPieceType.King
						&& GetPieceAtPosition(new BoardPosition(7, 7)).PieceType == ChessPieceType.Rook) {
						for (int col = p.Col + 1; col <= 6; col++) {
							ChessPiece checkPiece = GetPieceAtPosition(new BoardPosition(p.Row, col));
							if (checkPiece.PieceType == ChessPieceType.Empty && !PositionIsAttacked(new BoardPosition(p.Row, col), 2)) {
								emptyCount++;
							}
						}
						if (emptyCount == 2) {
							possibleMoves.Add(new ChessMove(p, new BoardPosition(p.Row, p.Col + 2), ChessMoveType.CastleKingSide));
						}
					}
					if (!whiteKingHasMoved && leftWRook && currentPlayer == 1 && GetPieceAtPosition(p).Player == currentPlayer
						&& GetPieceAtPosition(new BoardPosition(7, 4)).PieceType == ChessPieceType.King
						&& GetPieceAtPosition(new BoardPosition(7, 0)).PieceType == ChessPieceType.Rook) {
						emptyCount = 0;
						for (int col = p.Col - 1; col >= 1; col--) {
							ChessPiece checkPiece = GetPieceAtPosition(new BoardPosition(p.Row, col));
							if (col != 1) {
								if (checkPiece.PieceType == ChessPieceType.Empty && !PositionIsAttacked(new BoardPosition(p.Row, col), 2)) {
									emptyCount++;
								}
							} else {
								if (checkPiece.PieceType == ChessPieceType.Empty) {
									emptyCount++;
								}
							}
						}
						if (emptyCount == 3) {
							possibleMoves.Add(new ChessMove(p, new BoardPosition(p.Row, p.Col - 2), ChessMoveType.CastleQueenSide));
						}
					}
					// Black
					if (!blackKingHasMoved && rightBRook && currentPlayer == 2
						&& GetPieceAtPosition(p).Player == 2
						&& GetPieceAtPosition(new BoardPosition(0, 4)).PieceType == ChessPieceType.King
						&& GetPieceAtPosition(new BoardPosition(0, 7)).PieceType == ChessPieceType.Rook) {
						emptyCount = 0;
						for (int col = p.Col + 1; col <= 6; col++) {
							ChessPiece checkPiece = GetPieceAtPosition(new BoardPosition(p.Row, col));
							if (checkPiece.PieceType == ChessPieceType.Empty && !PositionIsAttacked(new BoardPosition(p.Row, col), 1)) {
								emptyCount++;
							}
						}
						if (emptyCount == 2) {
							possibleMoves.Add(new ChessMove(p, new BoardPosition(p.Row, p.Col + 2), ChessMoveType.CastleKingSide));
						}
					}
					if (!blackKingHasMoved && leftBRook && currentPlayer == 2
						  && GetPieceAtPosition(p).Player == 2
						  && GetPieceAtPosition(new BoardPosition(0, 4)).PieceType == ChessPieceType.King
						  && GetPieceAtPosition(new BoardPosition(0, 0)).PieceType == ChessPieceType.Rook) {
						emptyCount = 0;
						for (int col = p.Col - 1; col >= 1; col--) {
							ChessPiece checkPiece = GetPieceAtPosition(new BoardPosition(p.Row, col));
							if (col != 1) {
								if (checkPiece.PieceType == ChessPieceType.Empty && !PositionIsAttacked(new BoardPosition(p.Row, col), 1)) {
									emptyCount++;
								}
							} else {
								if (checkPiece.PieceType == ChessPieceType.Empty) {
									emptyCount++;
								}
							}
						}
						if (emptyCount == 3) {
							possibleMoves.Add(new ChessMove(p, new BoardPosition(p.Row, p.Col - 2), ChessMoveType.CastleQueenSide));
						}
					}

					validMovesPerPiece = possiblePositions.Intersect(KingAttackPos(p));
					foreach (BoardPosition v in validMovesPerPiece) {
						possibleMoves.Add(new ChessMove(p, v));
					}
				}
				else if (piece.PieceType == ChessPieceType.Rook && piece.Player == currentPlayer) {
					validMovesPerPiece = possiblePositions.Intersect(RookAttackPos(p));
					foreach (BoardPosition v in validMovesPerPiece) {
						possibleMoves.Add(new ChessMove(p, v));
					}
				}
				else if (piece.PieceType == ChessPieceType.Pawn && piece.Player == currentPlayer) {
					HashSet<BoardPosition> pawnMoves = new HashSet<BoardPosition>();
					if (currentPlayer == 1 && p.Row == 6 || currentPlayer == 2 && p.Row == 1) {
						if (currentPlayer == 1) {
							if (GetPieceAtPosition(new BoardPosition(p.Row - 2, p.Col)).PieceType == ChessPieceType.Empty
								&& PositionInBounds(new BoardPosition(p.Row - 2, p.Col))
								&& GetPieceAtPosition(new BoardPosition(p.Row - 1, p.Col)).PieceType == ChessPieceType.Empty) {
								pawnMoves.Add(new BoardPosition(p.Row - 2, p.Col));
							}
						}
						else {
							if (GetPieceAtPosition(new BoardPosition(p.Row + 2, p.Col)).PieceType == ChessPieceType.Empty
								&& PositionInBounds(new BoardPosition(p.Row + 2, p.Col))
								&& GetPieceAtPosition(new BoardPosition(p.Row + 1, p.Col)).PieceType == ChessPieceType.Empty) {
								pawnMoves.Add(new BoardPosition(p.Row + 2, p.Col));
							}
						}
					}
					if (currentPlayer == 1 && GetPieceAtPosition(p).Player == currentPlayer) {
						BoardPosition pos = p;
						pos += new BoardDirection(-1, 1);
						if (PositionInBounds(pos) && PositionIsEnemy(pos, currentPlayer) && !PositionIsEmpty(pos)) {
							if (pos.Row == 0) {
								possibleMoves.Add(new ChessMove(p, pos, ChessPieceType.Queen, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, pos, ChessPieceType.Knight, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, pos, ChessPieceType.Rook, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, pos, ChessPieceType.Bishop, ChessMoveType.PawnPromote));
							}
							else {
								pawnMoves.Add(pos);
							}
						}
						pos = p;
						pos += new BoardDirection(-1, -1);
						if (PositionInBounds(pos) && PositionIsEnemy(pos, currentPlayer) && !PositionIsEmpty(pos)) {
							if (pos.Row == 0) {
								possibleMoves.Add(new ChessMove(p, pos, ChessPieceType.Queen, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, pos, ChessPieceType.Knight, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, pos, ChessPieceType.Rook, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, pos, ChessPieceType.Bishop, ChessMoveType.PawnPromote));
							}
							else {
								pawnMoves.Add(pos);
							}
						}
						if (PositionIsEmpty(new BoardPosition(p.Row - 1, p.Col))
							&& PositionInBounds(new BoardPosition(p.Row - 1, p.Col))) {
							// Pawn promote
							if (p.Row - 1 == 0) {
								possibleMoves.Add(new ChessMove(p, new BoardPosition(p.Row - 1, p.Col), ChessPieceType.Queen, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, new BoardPosition(p.Row - 1, p.Col), ChessPieceType.Knight, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, new BoardPosition(p.Row - 1, p.Col), ChessPieceType.Rook, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, new BoardPosition(p.Row - 1, p.Col), ChessPieceType.Bishop, ChessMoveType.PawnPromote));
							}
							else {
								pawnMoves.Add(new BoardPosition(p.Row - 1, p.Col));
							}
						}
					}
					else if (currentPlayer == 2 && GetPieceAtPosition(p).Player == currentPlayer) {
						BoardPosition pos = p;
						pos += new BoardDirection(1, 1);
						if (PositionInBounds(pos) && PositionIsEnemy(pos, currentPlayer) && !PositionIsEmpty(pos)) {
							if (pos.Row == 7) {
								possibleMoves.Add(new ChessMove(p, pos, ChessPieceType.Queen, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, pos, ChessPieceType.Knight, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, pos, ChessPieceType.Rook, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, pos, ChessPieceType.Bishop, ChessMoveType.PawnPromote));
							}
							else {
								pawnMoves.Add(pos);
							}
						}
						pos = p;
						pos += new BoardDirection(1, -1);
						if (PositionInBounds(pos) && PositionIsEnemy(pos, currentPlayer) && !PositionIsEmpty(pos)) {
							if (pos.Row == 7) {
								possibleMoves.Add(new ChessMove(p, pos, ChessPieceType.Queen, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, pos, ChessPieceType.Knight, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, pos, ChessPieceType.Rook, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, pos, ChessPieceType.Bishop, ChessMoveType.PawnPromote));
							}
							else {
								pawnMoves.Add(pos);
							}
						}
						if (PositionIsEmpty(new BoardPosition(p.Row + 1, p.Col))
							&& PositionInBounds(new BoardPosition(p.Row + 1, p.Col))) {
							if (p.Row + 1 == 7) {
								possibleMoves.Add(new ChessMove(p, new BoardPosition(p.Row + 1, p.Col), ChessPieceType.Queen, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, new BoardPosition(p.Row + 1, p.Col), ChessPieceType.Knight, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, new BoardPosition(p.Row + 1, p.Col), ChessPieceType.Rook, ChessMoveType.PawnPromote));
								possibleMoves.Add(new ChessMove(p, new BoardPosition(p.Row + 1, p.Col), ChessPieceType.Bishop, ChessMoveType.PawnPromote));
							}
							else {
								pawnMoves.Add(new BoardPosition(p.Row + 1, p.Col));
							}
						}
					}

					// En passant
					if (currentPlayer == 1 && GetPieceAtPosition(p).Player == currentPlayer) {
						BoardPosition pos = p;
						if (pawnMovedTwoSpaces && pawnMovedTwoSpacesPos == new BoardPosition(p.Row, p.Col - 1) &&
							GetPieceAtPosition(new BoardPosition(p.Row, p.Col - 1)).PieceType == ChessPieceType.Pawn
							&& PositionIsEnemy(new BoardPosition(p.Row, p.Col - 1), currentPlayer)
							&& PositionInBounds(new BoardPosition(p.Row, p.Col - 1))) {
							pos += new BoardDirection(-1, -1);
							if (PositionInBounds(pos)) {
								possibleMoves.Add(new ChessMove(p, pos, ChessMoveType.EnPassant));
							}
						}
						else if (pawnMovedTwoSpaces && pawnMovedTwoSpacesPos == new BoardPosition(p.Row, p.Col + 1) &&
						  GetPieceAtPosition(new BoardPosition(p.Row, p.Col + 1)).PieceType == ChessPieceType.Pawn
						  && PositionIsEnemy(new BoardPosition(p.Row, p.Col + 1), currentPlayer)
						  && PositionInBounds(new BoardPosition(p.Row, p.Col + 1))) {
							pos = p;
							pos += new BoardDirection(-1, 1);
							if (PositionInBounds(pos)) {
								possibleMoves.Add(new ChessMove(p, pos, ChessMoveType.EnPassant));
							}
						}
					}
					else if (currentPlayer == 2 && GetPieceAtPosition(p).Player == currentPlayer) {
						BoardPosition pos = p;
						if (pawnMovedTwoSpaces && pawnMovedTwoSpacesPos == new BoardPosition(p.Row, p.Col - 1) &&
							GetPieceAtPosition(new BoardPosition(p.Row, p.Col - 1)).PieceType == ChessPieceType.Pawn
							&& PositionIsEnemy(new BoardPosition(p.Row, p.Col - 1), currentPlayer)
							&& PositionInBounds(new BoardPosition(p.Row, p.Col - 1))) {
							pos += new BoardDirection(1, -1);
							if (PositionInBounds(pos)) {
								possibleMoves.Add(new ChessMove(p, pos, ChessMoveType.EnPassant));
							}
						}
						else if (pawnMovedTwoSpaces && pawnMovedTwoSpacesPos == new BoardPosition(p.Row, p.Col + 1) &&
						  GetPieceAtPosition(new BoardPosition(p.Row, p.Col + 1)).PieceType == ChessPieceType.Pawn
						  && PositionIsEnemy(new BoardPosition(p.Row, p.Col + 1), currentPlayer)
						  && PositionInBounds(new BoardPosition(p.Row, p.Col + 1))) {
							pos = p;
							pos += new BoardDirection(1, 1);
							if (PositionInBounds(pos)) {
								possibleMoves.Add(new ChessMove(p, pos, ChessMoveType.EnPassant));
							}
						}
					}
					// End en passant

					foreach (BoardPosition v in pawnMoves) {
						possibleMoves.Add(new ChessMove(p, v));
					}
				}
			}

			List<ChessMove> validMoves = new List<ChessMove>();
			BoardPosition newKingPos = new BoardPosition();
			foreach (ChessMove m in possibleMoves) {
				if (m.MoveType == ChessMoveType.CastleKingSide || m.MoveType == ChessMoveType.CastleQueenSide) {
					if (PositionIsAttacked(kingPos, (currentPlayer == 1) ? 2 : 1)) {
						continue;
					}
				}

				if (GetPieceAtPosition(m.StartPosition).PieceType == ChessPieceType.King) {
					ApplyMove(m);
					newKingPos = m.EndPosition;
					if (!PositionIsAttacked(newKingPos, currentPlayer)) {
						validMoves.Add(m);
					}
					UndoLastMove();
				}
				else {
					ApplyMove(m);
					if (!PositionIsAttacked(kingPos, currentPlayer)) {
						validMoves.Add(m);
					}
					UndoLastMove();
				}
			}

			return validMoves;
			//return mMoves = moves;
		}

		public void ApplyMove(ChessMove m) {
			ChessPiece playerPiece = GetPieceAtPosition(m.StartPosition);
			ChessPiece enemyPiece = GetPieceAtPosition(m.EndPosition);

			var initialPosition = m.StartPosition;
			var WhiteRookLeftPosition = new BoardPosition(7, 0);
			var WhiteRookRightPosition = new BoardPosition(7, 7);
			var BlackRookLeftPosition = new BoardPosition(0, 0);
			var BlackRookRightPosition = new BoardPosition(0, 7);

			gameState.drawCounter = drawCount;
			gameState.pawnMovedTwoSpaces = pawnMovedTwoSpaces;
			gameState.pawnMovedTwoSpacesPos = pawnMovedTwoSpacesPos;
			gameState.pawnPromote = playerPiece;
			gameState.whiteKingHasMoved = whiteKingHasMoved;
			gameState.blackKingHasMoved = blackKingHasMoved;
			gameState.leftWRook = leftWRook;
			gameState.leftBRook = leftBRook;
			gameState.rightBRook = rightBRook;
			gameState.rightWRook = rightWRook;
			gameState.advantage = advantage;

			m.Player = currentPlayer;

			SetPieceAtPosition(m.EndPosition, playerPiece);
			if (m.MoveType == ChessMoveType.EnPassant) {
				int rowChange = 0;
				if (currentPlayer == 1) {
					rowChange = 1;
				}
				else if (currentPlayer == 2) {
					rowChange = -1;
				}
				BoardPosition capturedPos = new BoardPosition(m.EndPosition.Row + rowChange, m.EndPosition.Col);
				enemyPiece = GetPieceAtPosition(capturedPos);
				gameState.pieceTaken = enemyPiece;
				SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
				SetPieceAtPosition(capturedPos, new ChessPiece(ChessPieceType.Empty, 0));
			}
			else if (m.MoveType == ChessMoveType.PawnPromote && m.ChessPiece == ChessPieceType.Queen) {
				advantage = (currentPlayer == 1) ? advantage + 8 : advantage - 8;
				SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Queen, currentPlayer));
				SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
			}
			else if (m.MoveType == ChessMoveType.PawnPromote && m.ChessPiece == ChessPieceType.Knight) {
				advantage = (currentPlayer == 1) ? advantage + 2 : advantage - 2;
				SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Knight, currentPlayer));
				SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
			}
			else if (m.MoveType == ChessMoveType.PawnPromote && m.ChessPiece == ChessPieceType.Rook) {
				advantage = (currentPlayer == 1) ? advantage + 4 : advantage - 4;
				SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Rook, currentPlayer));
				SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
			}
			else if (m.MoveType == ChessMoveType.PawnPromote && m.ChessPiece == ChessPieceType.Bishop) {
				advantage = (currentPlayer == 1) ? advantage + 2 : advantage - 2;
				SetPieceAtPosition(m.EndPosition, new ChessPiece(ChessPieceType.Bishop, currentPlayer));
				SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
			}


			if (m.MoveType == ChessMoveType.CastleKingSide && currentPlayer == 1) {
				SetPieceAtPosition(new BoardPosition(7, 6), GetPieceAtPosition(new BoardPosition(7, 4)));
				SetPieceAtPosition(new BoardPosition(7, 4), new ChessPiece(ChessPieceType.Empty, 0));
				SetPieceAtPosition(new BoardPosition(7, 5), GetPieceAtPosition(new BoardPosition(7, 7)));
				SetPieceAtPosition(new BoardPosition(7, 7), new ChessPiece(ChessPieceType.Empty, 0));
			}
			else if (m.MoveType == ChessMoveType.CastleKingSide && currentPlayer == 2) {
				SetPieceAtPosition(new BoardPosition(0, 6), GetPieceAtPosition(new BoardPosition(0, 4)));
				SetPieceAtPosition(new BoardPosition(0, 4), new ChessPiece(ChessPieceType.Empty, 0));
				SetPieceAtPosition(new BoardPosition(0, 5), GetPieceAtPosition(new BoardPosition(0, 7)));
				SetPieceAtPosition(new BoardPosition(0, 7), new ChessPiece(ChessPieceType.Empty, 0));
			}
			else if (m.MoveType == ChessMoveType.CastleQueenSide && currentPlayer == 1) {
				SetPieceAtPosition(new BoardPosition(7, 2), GetPieceAtPosition(new BoardPosition(7, 4)));
				SetPieceAtPosition(new BoardPosition(7, 4), new ChessPiece(ChessPieceType.Empty, 0));
				SetPieceAtPosition(new BoardPosition(7, 3), GetPieceAtPosition(new BoardPosition(7, 0)));
				SetPieceAtPosition(new BoardPosition(7, 0), new ChessPiece(ChessPieceType.Empty, 0));
			}
			else if (m.MoveType == ChessMoveType.CastleQueenSide && currentPlayer == 2) {
				SetPieceAtPosition(new BoardPosition(0, 2), GetPieceAtPosition(new BoardPosition(0, 4)));
				SetPieceAtPosition(new BoardPosition(0, 4), new ChessPiece(ChessPieceType.Empty, 0));
				SetPieceAtPosition(new BoardPosition(0, 3), GetPieceAtPosition(new BoardPosition(0, 0)));
				SetPieceAtPosition(new BoardPosition(0, 0), new ChessPiece(ChessPieceType.Empty, 0));
			}
			else {
				SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
			}
			//Checking for castling on the left side of the white pieces


			if (playerPiece.PieceType == ChessPieceType.King) {
				if (currentPlayer == 1) {
					whiteKingHasMoved = true;
				}
				else {
					blackKingHasMoved = true;
				}
			}

			if (playerPiece.PieceType == ChessPieceType.Pawn
				&& Math.Abs(m.EndPosition.Row - m.StartPosition.Row) == 2) {
				pawnMovedTwoSpaces = true;
				pawnMovedTwoSpacesPos = m.EndPosition;
			}
			else {
				pawnMovedTwoSpaces = false;
			}

			if (GetPieceAtPosition(WhiteRookLeftPosition).PieceType != ChessPieceType.Rook) {
				leftWRook = false;
			}
			else if (GetPieceAtPosition(WhiteRookRightPosition).PieceType != ChessPieceType.Rook) {
				rightWRook = false;
			}
			else if (GetPieceAtPosition(BlackRookLeftPosition).PieceType != ChessPieceType.Rook) {
				leftBRook = false;
			}
			else if (GetPieceAtPosition(BlackRookRightPosition).PieceType != ChessPieceType.Rook) {
				rightBRook = false;
			}

			// Calc advantage
			if (enemyPiece.PieceType == ChessPieceType.Pawn) {
				advantage = (currentPlayer == 1) ? advantage + 1 : advantage - 1;
			}
			else if (enemyPiece.PieceType == ChessPieceType.Knight || enemyPiece.PieceType == ChessPieceType.Bishop) {
				advantage = (currentPlayer == 1) ? advantage + 3 : advantage - 3;
			}
			else if (enemyPiece.PieceType == ChessPieceType.Rook) {
				advantage = (currentPlayer == 1) ? advantage + 5 : advantage - 5;
			}
			else if (enemyPiece.PieceType == ChessPieceType.Queen) {
				advantage = (currentPlayer == 1) ? advantage + 9 : advantage - 9;
			}

			if (enemyPiece.PieceType == ChessPieceType.Pawn) {
				if (m.MoveType != ChessMoveType.EnPassant) {
					gameState.pieceTaken = enemyPiece;
				}
			}
			else if (enemyPiece.PieceType == ChessPieceType.Knight || enemyPiece.PieceType == ChessPieceType.Bishop) {
				gameState.pieceTaken = enemyPiece;
			}
			else if (enemyPiece.PieceType == ChessPieceType.Rook) {
				gameState.pieceTaken = enemyPiece;
			}
			else if (enemyPiece.PieceType == ChessPieceType.Queen) {
				gameState.pieceTaken = enemyPiece;
			}
			else if (enemyPiece.PieceType == ChessPieceType.Empty) {
				gameState.pieceTaken = enemyPiece;
			}
			else if (enemyPiece.PieceType == ChessPieceType.King) {
				gameState.pieceTaken = enemyPiece;
			}

			if (playerPiece.PieceType == ChessPieceType.Pawn) {
				drawCount = 0;

			}
			else if (enemyPiece.PieceType != ChessPieceType.Empty) {
				drawCount = 0;
			}
			else {
				drawCount++;
			}

			gameState.move = m;
			currentPlayer = (currentPlayer == 1) ? 2 : 1;
			gameStateList.Add(gameState);
			mMoveHistory.Add(m);
			//mMoves = null
		}




		public void UndoLastMove() {
			//If the currentplayer is 1, when undoing the move it will move back
			//To the previous player
			if (gameStateList.Count > 0) {
				currentPlayer = (currentPlayer == 1) ? 2 : 1;
			}


			GameState lastMove = gameStateList.Last();

			var startPosition = lastMove.move.StartPosition;
			var endPosition = lastMove.move.EndPosition;
			var lastPiece = lastMove.pieceTaken;
			var undoDrawCounter = lastMove.drawCounter;
			var undoPawnMovedTwo = lastMove.pawnMovedTwoSpaces;
			var UndoPawnMovedTwoPos = lastMove.pawnMovedTwoSpacesPos;
			var undoPawnPromote = lastMove.pawnPromote;
			var undoWhiteKingHasMoved = lastMove.whiteKingHasMoved;
			var undoBlackKingHasMoved = lastMove.blackKingHasMoved;
			var undoLeftWRook = lastMove.leftWRook;
			var undoRightWRook = lastMove.rightWRook;
			var undoLeftBRook = lastMove.leftBRook;
			var undoRightBRook = lastMove.rightBRook;
			var undoAdvantage = lastMove.advantage;

			if (lastMove.move.MoveType != ChessMoveType.PawnPromote) {
				SetPieceAtPosition(startPosition, GetPieceAtPosition(endPosition));
			}

			if (lastMove.move.MoveType == ChessMoveType.EnPassant) {
				if (currentPlayer == 1) { //black 
					SetPieceAtPosition(new BoardPosition(endPosition.Row + 1, endPosition.Col), lastPiece);
				}
				else if (currentPlayer == 2) {
					SetPieceAtPosition(new BoardPosition(endPosition.Row - 1, endPosition.Col), lastPiece);
				}
				SetPieceAtPosition(endPosition, new ChessPiece(ChessPieceType.Empty, 0));
			}
			else if (GetPieceAtPosition(endPosition).PieceType == ChessPieceType.Queen && lastMove.move.MoveType == ChessMoveType.PawnPromote && lastMove.move.ChessPiece == ChessPieceType.Queen) {
				SetPieceAtPosition(endPosition, lastPiece);
				SetPieceAtPosition(startPosition, new ChessPiece(ChessPieceType.Pawn, currentPlayer));
			}
			else if (GetPieceAtPosition(endPosition).PieceType == ChessPieceType.Knight && lastMove.move.MoveType == ChessMoveType.PawnPromote && lastMove.move.ChessPiece == ChessPieceType.Knight) {
				SetPieceAtPosition(endPosition, lastPiece);
				SetPieceAtPosition(startPosition, new ChessPiece(ChessPieceType.Pawn, currentPlayer));
			}
			else if (GetPieceAtPosition(endPosition).PieceType == ChessPieceType.Rook && lastMove.move.MoveType == ChessMoveType.PawnPromote && lastMove.move.ChessPiece == ChessPieceType.Rook) {
				SetPieceAtPosition(endPosition, lastPiece);
				SetPieceAtPosition(startPosition, new ChessPiece(ChessPieceType.Pawn, currentPlayer));
			}
			else if (GetPieceAtPosition(endPosition).PieceType == ChessPieceType.Bishop && lastMove.move.MoveType == ChessMoveType.PawnPromote && lastMove.move.ChessPiece == ChessPieceType.Bishop) {
				SetPieceAtPosition(endPosition, lastPiece);
				SetPieceAtPosition(startPosition, new ChessPiece(ChessPieceType.Pawn, currentPlayer));
			}
			else if (lastMove.move.MoveType == ChessMoveType.CastleKingSide && currentPlayer == 1) {
				SetPieceAtPosition(new BoardPosition(7, 6), new ChessPiece(ChessPieceType.Empty, 0));
				SetPieceAtPosition(new BoardPosition(7, 5), new ChessPiece(ChessPieceType.Empty, 0));
				SetPieceAtPosition(new BoardPosition(7, 4), new ChessPiece(ChessPieceType.King, 1));
				SetPieceAtPosition(new BoardPosition(7, 7), new ChessPiece(ChessPieceType.Rook, 1));
			}
			else if (lastMove.move.MoveType == ChessMoveType.CastleKingSide && currentPlayer == 2) {
				SetPieceAtPosition(new BoardPosition(0, 6), new ChessPiece(ChessPieceType.Empty, 0));
				SetPieceAtPosition(new BoardPosition(0, 5), new ChessPiece(ChessPieceType.Empty, 0));
				SetPieceAtPosition(new BoardPosition(0, 4), new ChessPiece(ChessPieceType.King, 2));
				SetPieceAtPosition(new BoardPosition(0, 7), new ChessPiece(ChessPieceType.Rook, 2));
			}
			else if (lastMove.move.MoveType == ChessMoveType.CastleQueenSide && currentPlayer == 1) {
				SetPieceAtPosition(new BoardPosition(7, 2), new ChessPiece(ChessPieceType.Empty, 0));
				SetPieceAtPosition(new BoardPosition(7, 3), new ChessPiece(ChessPieceType.Empty, 0));
				SetPieceAtPosition(new BoardPosition(7, 4), new ChessPiece(ChessPieceType.King, 1));
				SetPieceAtPosition(new BoardPosition(7, 0), new ChessPiece(ChessPieceType.Rook, 1));
			}
			else if (lastMove.move.MoveType == ChessMoveType.CastleQueenSide && currentPlayer == 2) {
				SetPieceAtPosition(new BoardPosition(0, 2), new ChessPiece(ChessPieceType.Empty, 0));
				SetPieceAtPosition(new BoardPosition(0, 3), new ChessPiece(ChessPieceType.Empty, 0));
				SetPieceAtPosition(new BoardPosition(0, 4), new ChessPiece(ChessPieceType.King, 2));
				SetPieceAtPosition(new BoardPosition(0, 0), new ChessPiece(ChessPieceType.Rook, 2));
			}
			else {
				SetPieceAtPosition(endPosition, lastPiece);
			}

			//currentPlayer = (currentPlayer == 1) ? 2 : 1;


			//I need to keep track of the advantage
			if (lastPiece.PieceType == ChessPieceType.Pawn) {
				advantage = (currentPlayer == 1) ? advantage - 1 : advantage + 1;
			}
			else if (lastPiece.PieceType == ChessPieceType.Knight || lastPiece.PieceType == ChessPieceType.Bishop) {
				advantage = (currentPlayer == 1) ? advantage - 3 : advantage + 3;
			}
			else if (lastPiece.PieceType == ChessPieceType.Rook) {
				advantage = (currentPlayer == 1) ? advantage - 5 : advantage + 5;
			}
			else if (lastPiece.PieceType == ChessPieceType.Queen) {
				advantage = (currentPlayer == 1) ? advantage - 9 : advantage + 9;
			}

			drawCount = undoDrawCounter;
			advantage = undoAdvantage;
			pawnMovedTwoSpaces = undoPawnMovedTwo;
			pawnMovedTwoSpacesPos = UndoPawnMovedTwoPos;
			pawnPromote = undoPawnPromote;
			whiteKingHasMoved = undoWhiteKingHasMoved;
			blackKingHasMoved = undoBlackKingHasMoved;
			leftWRook = undoLeftWRook;
			leftBRook = undoLeftBRook;
			rightBRook = undoRightBRook;
			rightWRook = undoRightWRook;

			gameStateList.RemoveAt(gameStateList.Count - 1);
			mMoveHistory.RemoveAt(mMoveHistory.Count - 1);
			//mMoves = null;
		}

		/// <summary>
		/// Returns whatever chess piece is occupying the given position.
		/// </summary>
		public ChessPiece GetPieceAtPosition(BoardPosition position) {
			byte currPiece, currPieceType, playerBit;

			// Determine if requested position is a left or right square. Find index in the byte array and bit mask
			// current piece to obtain only left piece. Bit mask current piece to obtain the piece type and player; then, shift
			if (position.Col % 2 == 0) {
				currPiece = (byte)(mBoard[position.Row * 4 + position.Col / 2] & 0b_11110000);
				currPieceType = (byte)((currPiece & 0b_01110000) >> 4);
				playerBit = (byte)((currPiece & 0b_10000000) >> 7);
			}
			else {
				currPiece = (byte)(mBoard[position.Row * 4 + position.Col / 2] & 0b_00001111);
				currPieceType = (byte)(currPiece & 0b_00000111);
				playerBit = (byte)((currPiece & 0b_00001000) >> 3);
			}

			if (currPieceType == 0) {
				return new ChessPiece((ChessPieceType)currPieceType, 0);
			}
			else {
				return new ChessPiece((ChessPieceType)currPieceType, playerBit + 1);
			}
		}

		/// <summary>
		/// Returns whatever player is occupying the given position.
		/// </summary>
		public int GetPlayerAtPosition(BoardPosition pos) {
			// Get the piece at the specified position and determine who's piece it is using the player property
			return GetPieceAtPosition(pos).Player;
		}

		/// <summary>
		/// Returns true if the given position on the board is empty.
		/// </summary>
		/// <remarks>returns false if the position is not in bounds</remarks>
		public bool PositionIsEmpty(BoardPosition pos) {
			return GetPieceAtPosition(pos).PieceType == ChessPieceType.Empty;
		}

		/// <summary>
		/// Returns true if the given position contains a piece that is the enemy of the given player.
		/// </summary>
		/// <remarks>returns false if the position is not in bounds</remarks>
		public bool PositionIsEnemy(BoardPosition pos, int player) {
			return GetPieceAtPosition(pos).Player != player && GetPieceAtPosition(pos).PieceType != ChessPieceType.Empty;
		}

		/// <summary>
		/// Returns true if the given position is in the bounds of the board.
		/// </summary>
		public static bool PositionInBounds(BoardPosition pos) {
			return ((pos.Row >= 0 && pos.Row <= 7) && (pos.Col >= 0 && pos.Col <= 7));
		}

		/// <summary>
		/// Returns all board positions where the given piece can be found.
		/// </summary>
		public IEnumerable<BoardPosition> GetPositionsOfPiece(ChessPieceType piece, int player) {
			var positions = BoardPosition.GetRectangularPositions(8, 8);
			ISet<BoardPosition> piecePositions = new HashSet<BoardPosition>();
			foreach (BoardPosition p in positions) {
				ChessPiece pAtPos = GetPieceAtPosition(p);
				if (pAtPos.PieceType == piece && pAtPos.Player == player) {
					piecePositions.Add(p);
				}
			}
			return piecePositions;
		}

		/// <summary>
		/// Returns true if the given player's pieces are attacking the given position.
		/// </summary>
		public bool PositionIsAttacked(BoardPosition position, int byPlayer) {
			ISet<BoardPosition> attackedPositions = new HashSet<BoardPosition>();
			attackedPositions.UnionWith(GetAttackedPositions(byPlayer));
			return attackedPositions.Contains(position);
		}

		/// <summary>
		/// Returns a set of all BoardPositions that are attacked by the given player.
		/// </summary>
		public ISet<BoardPosition> GetAttackedPositions(int byPlayer) {
			ISet<BoardPosition> attackedPositions = new HashSet<BoardPosition>();
			var positions = BoardPosition.GetRectangularPositions(8, 8);

			foreach (BoardPosition p in positions) {
				var piece = GetPieceAtPosition(p);
				if (byPlayer == piece.Player) {
					if (piece.PieceType == ChessPieceType.Rook) {
						attackedPositions.UnionWith(RookAttackPos(p));
					}
					else if (piece.PieceType == ChessPieceType.Bishop) {
						attackedPositions.UnionWith(BishopAttackPos(p));
					}
					else if (piece.PieceType == ChessPieceType.Queen) {
						attackedPositions.UnionWith(QueenAttackPos(p));
					}
					else if (piece.PieceType == ChessPieceType.King) {
						attackedPositions.UnionWith(KingAttackPos(p));
					}
					else if (piece.PieceType == ChessPieceType.Pawn) {
						attackedPositions.UnionWith(PawnAttackPos(p, byPlayer));
					}
					else if (piece.PieceType == ChessPieceType.Knight) {
						attackedPositions.UnionWith(KnightAttackPos(p));
					}
				}
			}
			return attackedPositions;
		}
		#endregion

		#region Private methods.
		/// <summary>
		/// Mutates the board state so that the given piece is at the given position.
		/// </summary>
		private void SetPieceAtPosition(BoardPosition position, ChessPiece piece) {
			// Find the two potential squares the piece will be set at, and
			// determine piece to set using the bitwise-OR of piece type and player
			int indx = position.Row * 4 + position.Col / 2;
			byte pieceToSet = 0;
			if (piece.Player == 2) {
				pieceToSet = (byte)((byte)piece.PieceType | (byte)piece.Player << 2);
			}
			else if (piece.Player == 1) {
				pieceToSet = (byte)((byte)piece.PieceType | (byte)piece.Player & 0b_0000);
			}
			// Find which square to set piece at then shift accordingly (if needed), so that
			// the piece to be set can be ORed with the piece currently at the position 
			mBoard[indx] = position.Col % 2 == 0 ? (byte)((mBoard[indx] & 0b_00001111) | (pieceToSet << 4)) :
				(byte)(mBoard[indx] & 0b_11110000 | pieceToSet);
		}

		private int calculateInitialAdvantage() {
			int pawnValue = 1;
			int knightValue, bishopValue = 3;
			int rookValue = 5;
			int queenValue = 9;

			var whitePawnValue = GetPositionsOfPiece(ChessPieceType.Pawn, 1).Count();
			var whiteKnightValue = GetPositionsOfPiece(ChessPieceType.Knight, 1).Count() * 3;
			var whiteBishopValue = GetPositionsOfPiece(ChessPieceType.Bishop, 1).Count() * 3;
			var whiteRookValue = GetPositionsOfPiece(ChessPieceType.Rook, 1).Count() * 5;
			var whiteQueenValue = GetPositionsOfPiece(ChessPieceType.Queen, 9).Count() * 9;

			var blackPawnValue = GetPositionsOfPiece(ChessPieceType.Pawn, 2).Count();
			var blackKnightValue = GetPositionsOfPiece(ChessPieceType.Knight, 2).Count() * 3;
			var blackBishopValue = GetPositionsOfPiece(ChessPieceType.Bishop, 2).Count() * 3;
			var blackRookValue = GetPositionsOfPiece(ChessPieceType.Rook, 2).Count() * 5;
			var blackQueenValue = GetPositionsOfPiece(ChessPieceType.Queen, 2).Count() * 9;

			var blackAdvantage = blackPawnValue + blackKnightValue + blackBishopValue + blackRookValue + blackQueenValue;
			var whiteAdvantage = whitePawnValue + whiteKnightValue + whiteBishopValue + blackRookValue + blackQueenValue;

			return whiteAdvantage - blackAdvantage;
		}

		#endregion

		#region Explicit IGameBoard implementations.
		IEnumerable<IGameMove> IGameBoard.GetPossibleMoves() {
			return GetPossibleMoves();
		}
		void IGameBoard.ApplyMove(IGameMove m) {
			ApplyMove(m as ChessMove);
		}
		IReadOnlyList<IGameMove> IGameBoard.MoveHistory => mMoveHistory;

		/*		public long BoardWeight {
					get {
						if (currentPlayer == 1) {
							long longWeight = advantage;
							return longWeight;
						} else {
							long longWeight = advantage * -1;
							return longWeight;
						}
					} 
				}*/

		public long BoardWeight {
			get; private set;
		}
		#endregion

		// You may or may not need to add code to this constructor.
		public ChessBoard() {
			mBoard = new byte[32] {
								   171, 205, 236, 186,
								   153, 153, 153, 153,
									0, 0, 0, 0,
									0, 0, 0, 0,
									0, 0, 0, 0,
									0, 0, 0, 0,
									17, 17, 17, 17,
									35, 69, 100, 50};

			gameState = new GameState();
			gameState.BlackCanCastle = true;
			gameState.WhiteCanCastle = true;
			int advantage = calculateInitialAdvantage();
		}

		public ChessBoard(IEnumerable<Tuple<BoardPosition, ChessPiece>> startingPositions)
			: this() {
			var king1 = startingPositions.Where(t => t.Item2.Player == 1 && t.Item2.PieceType == ChessPieceType.King);
			var king2 = startingPositions.Where(t => t.Item2.Player == 2 && t.Item2.PieceType == ChessPieceType.King);
			if (king1.Count() != 1 || king2.Count() != 1) {
				throw new ArgumentException("A chess board must have a single king for each player");
			}

			foreach (var position in BoardPosition.GetRectangularPositions(8, 8)) {
				SetPieceAtPosition(position, ChessPiece.Empty);
			}

			int[] values = { 0, 0 };
			foreach (var pos in startingPositions) {
				SetPieceAtPosition(pos.Item1, pos.Item2);
				// TODO: you must calculate the overall advantage for this board, in terms of the pieces
				// that the board has started with. "pos.Item2" will give you the chess piece being placed
				// on this particular position.
				if (pos.Item2.PieceType == ChessPieceType.Pawn) {
					advantage = (pos.Item2.Player == 1) ? advantage + 1 : advantage - 1;
				}
				else if (pos.Item2.PieceType == ChessPieceType.Knight || pos.Item2.PieceType == ChessPieceType.Bishop) {
					advantage = (pos.Item2.Player == 1) ? advantage + 3 : advantage - 3;
				}
				else if (pos.Item2.PieceType == ChessPieceType.Rook) {
					advantage = (pos.Item2.Player == 1) ? advantage + 5 : advantage - 5;
				}
				else if (pos.Item2.PieceType == ChessPieceType.Queen) {
					advantage = (pos.Item2.Player == 1) ? advantage + 9 : advantage - 9;
				}
			}
		}

		public ISet<BoardPosition> RookAttackPos(BoardPosition p) {
			ISet<BoardPosition> rookPositions = new HashSet<BoardPosition>();
			var directions = BoardDirection.CardinalDirections;
			int count = 0;
			var initialPos = p;

			foreach (BoardDirection bd in directions) {
				if (count == 1 || count == 3 || count == 4 || count == 6) {
					p += bd;
					while ((p.Row >= 0 && p.Row <= 7) && (p.Col >= 0 && p.Col <= 7)) {
						rookPositions.Add(p);
						if (GetPieceAtPosition(p).PieceType != ChessPieceType.Empty) {
							break;
						}
						p += bd;
					}
				}
				count++;
				p = initialPos;
			}
			return rookPositions;
		}

		public ISet<BoardPosition> KnightAttackPos(BoardPosition p) {
			ISet<BoardPosition> knightPos = new HashSet<BoardPosition>();
			var directions = BoardDirection.CardinalDirections;
			var initialPos = p;
			int count = 0;
			foreach (BoardDirection bd in directions) {
				p += bd;
				p += bd;
				if ((p.Row >= 0 && p.Row <= 7) && (p.Col >= 0 && p.Col <= 7)) {
					if (count == 1 || count == 3) {
						if (count == 1) {
							int colPlus = p.Col + 1;
							int colMinus = p.Col - 1;
							if ((p.Row >= 0 && p.Row <= 7) && (colPlus >= 0 && colPlus <= 7)) {
								knightPos.Add(new BoardPosition(p.Row, colPlus));
							}
							if ((p.Row >= 0 && p.Row <= 7) && (colMinus >= 0 && colMinus <= 7)) {
								knightPos.Add(new BoardPosition(p.Row, colMinus));
							}
						}
						else if (count == 3) {
							int rowPlus = p.Row + 1;
							int rowMinus = p.Row - 1;
							if ((rowPlus >= 0 && rowPlus <= 7) && (p.Col >= 0 && p.Col <= 7)) {
								knightPos.Add(new BoardPosition(rowPlus, p.Col));
							}
							if ((rowMinus >= 0 && rowMinus <= 7) && (p.Col >= 0 && p.Col <= 7)) {
								knightPos.Add(new BoardPosition(rowMinus, p.Col));
							}
						}

					}
					else if (count == 4 || count == 6) {
						if (count == 4) {
							int rowPlus = p.Row + 1;
							int rowMinus = p.Row - 1;
							if ((rowPlus >= 0 && rowPlus <= 7) && (p.Col >= 0 && p.Col <= 7)) {
								knightPos.Add(new BoardPosition(rowPlus, p.Col));
							}
							if ((rowMinus >= 0 && rowMinus <= 7) && (p.Col >= 0 && p.Col <= 7)) {
								knightPos.Add(new BoardPosition(rowMinus, p.Col));
							}
						}
						else if (count == 6) {
							int colPlus = p.Col + 1;
							int colMinus = p.Col - 1;
							if ((p.Row >= 0 && p.Row <= 7) && (colPlus >= 0 && colPlus <= 7)) {
								knightPos.Add(new BoardPosition(p.Row, colPlus));
							}
							if ((p.Row >= 0 && p.Row <= 7) && (colMinus >= 0 && colMinus <= 7)) {
								knightPos.Add(new BoardPosition(p.Row, colMinus));
							}
						}
					}
				}
				count++;
				p = initialPos;
			}
			return knightPos;
		}

		public ISet<BoardPosition> BishopAttackPos(BoardPosition p) {
			ISet<BoardPosition> bishopPos = new HashSet<BoardPosition>();
			var directions = BoardDirection.CardinalDirections;
			var initialPos = p;
			int count = 0;
			foreach (BoardDirection bd in directions) {
				if (count == 0 || count == 2 || count == 5 || count == 7) {
					p += bd;
					while ((p.Row >= 0 && p.Row <= 7) && (p.Col >= 0 && p.Col <= 7)) {
						bishopPos.Add(p);
						if (GetPieceAtPosition(p).PieceType != ChessPieceType.Empty) {
							break;
						}
						p += bd;
					}
				}
				count++;
				p = initialPos;
			}
			return bishopPos;
		}

		public ISet<BoardPosition> QueenAttackPos(BoardPosition p) {
			ISet<BoardPosition> queenPositions = new HashSet<BoardPosition>();
			var direction = BoardDirection.CardinalDirections;
			var startingPosition = p;

			foreach (BoardDirection bd in direction) {
				var checkBounds = p + bd;
				p += bd;
				while ((p.Row >= 0 && p.Row <= 7) && (p.Col >= 0 && p.Col <= 7)) {
					queenPositions.Add(p);
					if (GetPieceAtPosition(p).PieceType != ChessPieceType.Empty) {
						break;
					}
					p += bd;

				}
				p = startingPosition;
			}
			return queenPositions;
		}

		public ISet<BoardPosition> KingAttackPos(BoardPosition p) {
			ISet<BoardPosition> kingPositions = new HashSet<BoardPosition>();
			var direction = BoardDirection.CardinalDirections;
			var startingPosition = p;

			foreach (BoardDirection bd in direction) {
				p += bd;
				if ((p.Col >= 0 && p.Col <= 7) && (p.Row <= 7 && p.Row >= 0)) {
					kingPositions.Add(p);
				}
				p = startingPosition;
			}
			return kingPositions;
		}

		public ISet<BoardPosition> PawnAttackPos(BoardPosition p, int player) {
			ISet<BoardPosition> pawnPos = new HashSet<BoardPosition>();
			var directions = new BoardDirection[] { new BoardDirection(-1, -1), new BoardDirection(-1, 1),
			new BoardDirection(1, -1), new BoardDirection(1, 1)};
			var initialPos = p;

			foreach (BoardDirection bd in directions) {
				if (player == 1 && (bd == directions[0] || bd == directions[1])) {
					p += bd;
					if ((p.Row >= 0 && p.Row <= 7) && (p.Col >= 0 && p.Col <= 7)) {
						pawnPos.Add(p);
					}
				}
				else if (player == 2 && (bd == directions[2] || bd == directions[3])) {
					p += bd;
					if ((p.Row >= 0 && p.Row <= 7) && (p.Col >= 0 && p.Col <= 7)) {
						pawnPos.Add(p);
					}
				}
				p = initialPos;
			}

			return pawnPos;
		}
	}
}
