using Cecs475.BoardGames.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cecs475.BoardGames.ComputerOpponent {
	internal struct MinimaxBestMove {
		public long Weight { get; set; }
		public IGameMove Move { get; set; }
	}

	public class MinimaxAi : IGameAi {
		private int mMaxDepth;
		public MinimaxAi(int maxDepth) {
			mMaxDepth = maxDepth;
		}

		public IGameMove FindBestMove(IGameBoard b) {
			/*return FindBestMove(b,
				true ? long.MinValue : long.MaxValue,
				true ? long.MaxValue : long.MinValue,
				mMaxDepth).Move;*/
			return FindBestMove(b,
			mMaxDepth,
			b.CurrentPlayer == 1).Move;
		}

		private static MinimaxBestMove FindBestMove(IGameBoard b, long depth, bool isMaximizing) {
			if (depth == 0 || b.IsFinished)
				return new MinimaxBestMove {
					Weight = b.BoardWeight,
					Move = null
				};

			long bestWeight;
			bestWeight = (isMaximizing) ? long.MinValue : long.MaxValue;
			IGameMove bestMove = null;
			
			IEnumerable<IGameMove> possMoves = b.GetPossibleMoves();
			foreach (IGameMove move in possMoves) {
				b.ApplyMove(move);
				long w = (FindBestMove(b, depth - 1, !isMaximizing)).Weight;
				b.UndoLastMove();
				if (isMaximizing && w > bestWeight) {
					bestWeight = w;
					bestMove = move;
				} else if (!isMaximizing && w < bestWeight) {
					bestWeight = w;
					bestMove = move;
				}
			}
			return new MinimaxBestMove {
				Weight = bestWeight,
				Move = bestMove
			};
		}
	}
}
