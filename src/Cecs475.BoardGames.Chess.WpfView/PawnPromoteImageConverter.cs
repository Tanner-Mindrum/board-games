using Cecs475.BoardGames.Chess.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Cecs475.BoardGames.Chess.WpfView {
    public class PawnPromoteImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			try {
				ChessPiece p = (ChessPiece)value;
				string player = "";
				string pieceType = "";

				if (p.Player == 1)
					player = "White";
				else if (p.Player == 2)
					player = "Black";

				if (p.PieceType == ChessPieceType.Pawn) {
					pieceType = "Pawn";
				} else if (p.PieceType == ChessPieceType.Queen) {
					pieceType = "Queen";
				} else if (p.PieceType == ChessPieceType.King) {
					pieceType = "King";
				} else if (p.PieceType == ChessPieceType.Bishop) {
					pieceType = "Bishop";
				} else if (p.PieceType == ChessPieceType.Knight) {
					pieceType = "Knight";
				} else if (p.PieceType == ChessPieceType.Rook) {
					pieceType = "Rook";
				} else if (p.PieceType == ChessPieceType.Empty) {
					return null;
				}

				string src = player + " " + pieceType;
				return new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resource/" + src + ".png", UriKind.Relative));
			} catch (Exception e) {
				return null;
			}
		}

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
