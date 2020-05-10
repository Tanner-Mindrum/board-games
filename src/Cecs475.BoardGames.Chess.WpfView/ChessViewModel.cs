using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;
using Cecs475.BoardGames.WpfView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cecs475.BoardGames.ComputerOpponent;
using System.Windows;

namespace Cecs475.BoardGames.Chess.WpfView {
	public class ChessSquare : INotifyPropertyChanged {

		private int mPlayerTurn;
		/// <summary>
		/// The player that has a piece in the given square, or 0 if empty.
		/// </summary>
		public int PlayerTurn {
			get { return mPlayerTurn; }
			set {
				if (value != mPlayerTurn) {
					mPlayerTurn = value;
					OnPropertyChanged(nameof(PlayerTurn));
				}
			}
		}

		/// <summary>
		/// The position of the square.
		/// </summary>
		public BoardPosition Position {
			get; set;
		}

		public ChessMove Move {
			get; set;
		}

		private ChessPiece mPlayer;
		public ChessPiece Player {
			get { return mPlayer; }
			set {
				if (value.PieceType != mPlayer.PieceType || value.Player != mPlayer.Player) {
					mPlayer = value;
					OnPropertyChanged(nameof(Player));
				}
			}
		}

		private bool mIsHighlighted;
		private bool mIsSelected;
		private bool mIsCheck;
		/// <summary>
		/// Whether the square should be highlighted because of a user action.
		/// </summary>
		public bool IsHighlighted {
			get { return mIsHighlighted; }
			set {
				if (value != mIsHighlighted) {
					mIsHighlighted = value;
					OnPropertyChanged(nameof(IsHighlighted));
				}
			}
		}

		public bool IsSelected {
			get { return mIsSelected; }
			set {
				if (value != mIsSelected) {
					mIsSelected = value;
					OnPropertyChanged(nameof(IsSelected));
				}
			}
		}

		public bool IsCheck {
			get { return mIsCheck; }
			set {
				if (value != mIsCheck) {
					mIsCheck = value;
					OnPropertyChanged(nameof(IsCheck));
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string name) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}

	public class ChessViewModel : INotifyPropertyChanged, IGameViewModel {
		private ChessBoard mBoard;
		private ObservableCollection<ChessSquare> mSquares;
		public event EventHandler GameFinished;
		private const int MAX_AI_DEPTH = 4;
		private IGameAi mGameAi = new MinimaxAi(MAX_AI_DEPTH);

		public ChessViewModel() {
			mBoard = new ChessBoard();
			// Initialize the squares objects based on the board's initial state.
			mSquares = new ObservableCollection<ChessSquare>(
				BoardPosition.GetRectangularPositions(8, 8)
				.Select(pos => new ChessSquare() {
					Position = pos,
					Player = mBoard.GetPieceAtPosition(pos),
					PlayerTurn = mBoard.GetPlayerAtPosition(pos)
				})
			);

			PossibleEndMoves = new HashSet<BoardPosition>(
				from ChessMove m in mBoard.GetPossibleMoves()
				select m.EndPosition
			);

			PossibleStartMoves = new HashSet<BoardPosition>(
				from ChessMove m in mBoard.GetPossibleMoves()
				select m.StartPosition
			);

			PossibleMoves = mBoard.GetPossibleMoves();

		}

		/// <summary>
		/// Applies a move for the current player at the given position.
		/// </summary>
		public async Task ApplyMove(BoardPosition endPosition, BoardPosition startPosition, string promote) {
			var possMoves = mBoard.GetPossibleMoves() as IEnumerable<ChessMove>;
			// Validate the move as possible.
			foreach (var move in possMoves) {
				if (mBoard.GetPieceAtPosition(move.StartPosition).PieceType.Equals(
					mBoard.GetPieceAtPosition(startPosition).PieceType) && move.StartPosition.Equals(startPosition) &&
					move.EndPosition.Equals(endPosition)) {

					if (move.MoveType == ChessMoveType.PawnPromote) {
						if (move.ChessPiece == ChessPieceType.Queen && promote == "queen") {
							mBoard.ApplyMove(move);
							break;
						} else if (move.ChessPiece == ChessPieceType.Rook && promote == "rook") {
							mBoard.ApplyMove(move);
							break;
						} else if (move.ChessPiece == ChessPieceType.Bishop && promote == "bishop") {
							mBoard.ApplyMove(move);
							break;
						} else if (move.ChessPiece == ChessPieceType.Knight && promote == "knight") {
							mBoard.ApplyMove(move);
							break;
						}
					}
					else {
						mBoard.ApplyMove(move);
						break;
					}
				}
			}
			RebindState();

			if (Players == NumberOfPlayers.One && !mBoard.IsFinished) {
				var bestMoveResult = await Task.Run(() => mGameAi.FindBestMove(mBoard));
				if (bestMoveResult != null)
					mBoard.ApplyMove(bestMoveResult as ChessMove);
				RebindState();
			}

			if (mBoard.IsFinished) {
				GameFinished?.Invoke(this, new EventArgs());
			}

			//MessageBoxResult result = MessageBox.Show(mBoard.BoardWeight.ToString());
		}

		private void RebindState() {
			// Rebind the possible moves, now that the board has changed.
			PossibleStartMoves = new HashSet<BoardPosition>(
				from ChessMove m in mBoard.GetPossibleMoves()
				select m.StartPosition
			);

			PossibleEndMoves = new HashSet<BoardPosition>(
				from ChessMove m in mBoard.GetPossibleMoves()
				select m.EndPosition
			);

			PossibleMoves = mBoard.GetPossibleMoves();

			// Update the collection of squares by examining the new board state.
			var newSquares = BoardPosition.GetRectangularPositions(8, 8);
			int i = 0;
			foreach (var pos in newSquares) {
				mSquares[i].Player = mBoard.GetPieceAtPosition(pos);
				mSquares[i].PlayerTurn = mBoard.GetPlayerAtPosition(pos);
				if (mSquares[i].Player.PieceType == ChessPieceType.King && mBoard.IsCheck && mSquares[i].PlayerTurn == CurrentPlayer) {
					mSquares[i].IsCheck = true;
				} else {
					mSquares[i].IsCheck = false;
				}
				i++;
			}
			OnPropertyChanged(nameof(PossibleStartMoves));
			OnPropertyChanged(nameof(PossibleEndMoves));
			OnPropertyChanged(nameof(PossibleMoves));
			OnPropertyChanged(nameof(BoardAdvantage));
			OnPropertyChanged(nameof(CurrentPlayer));
			OnPropertyChanged(nameof(CanUndo));
		}

		/// <summary>
		/// A collection of 64 ChessSquare objects representing the state of the 
		/// game board.
		/// </summary>
		public ObservableCollection<ChessSquare> Squares {
			get { return mSquares; }
		}

		/// <summary>
		/// A set of board positions where the current player can move.
		/// </summary>
		public HashSet<BoardPosition> PossibleEndMoves {
			get; private set;
		}

		public HashSet<BoardPosition> PossibleStartMoves {
			get; private set;
		}

		public IEnumerable<ChessMove> PossibleMoves {
			get; private set;
		}

		/// <summary>
		/// The player whose turn it currently is.
		/// </summary>
		public int CurrentPlayer {
			get { return mBoard.CurrentPlayer; }
		}

		public GameAdvantage BoardAdvantage => mBoard.CurrentAdvantage;

		public bool CanUndo => mBoard.MoveHistory.Any();

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string name) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public NumberOfPlayers Players { get; set; }

		public void UndoMove() {
			if (CanUndo) {
				mBoard.UndoLastMove();
				// In one-player mode, Undo has to remove an additional move to return to the
				// human player's turn.
				if (Players == NumberOfPlayers.One && CanUndo) {
					mBoard.UndoLastMove();
				}
				RebindState();
			}
		}
	}
}
