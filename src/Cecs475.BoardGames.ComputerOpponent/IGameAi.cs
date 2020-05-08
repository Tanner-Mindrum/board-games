using Cecs475.BoardGames.Model;
using System;

namespace Cecs475.BoardGames.ComputerOpponent {
	public interface IGameAi {
		IGameMove FindBestMoveAsync(IGameBoard b);
	}
}
