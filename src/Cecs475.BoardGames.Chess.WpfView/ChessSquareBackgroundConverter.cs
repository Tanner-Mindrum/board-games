using Cecs475.BoardGames.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Cecs475.BoardGames.Chess.WpfView {
	public class ChessSquareBackgroundConverter : IMultiValueConverter {
		private static SolidColorBrush DEFAULT_LIGHT_BRUSH = Brushes.LightGoldenrodYellow;
		private static SolidColorBrush DEFAULT_DARK_BRUSH = Brushes.LightSalmon;
		private static SolidColorBrush HIGHLIGHT_BRUSH = Brushes.LightGreen;
		private static SolidColorBrush SELECTED_BRUSH = Brushes.Red;
		private static SolidColorBrush CHECK_BRUSH = Brushes.Yellow;

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			BoardPosition pos = (BoardPosition)values[0];
			bool isHighlighted = (bool)values[1];
			bool isSelected = (bool)values[2];
			bool isCheck = (bool)values[3];
			 
			if (isHighlighted) return HIGHLIGHT_BRUSH;
			if (isSelected) return SELECTED_BRUSH;
			if (isCheck) return CHECK_BRUSH;

			// Alternating square light/dark color
			if (pos.Row % 2 == 0 && pos.Col % 2 == 0)
				return DEFAULT_LIGHT_BRUSH;
			else if (pos.Row % 2 == 0 && pos.Col % 2 == 1)
				return DEFAULT_DARK_BRUSH;
			else if (pos.Row % 2 == 1 && pos.Col % 2 == 0)
				return DEFAULT_DARK_BRUSH;
			else
				return DEFAULT_LIGHT_BRUSH;
		}
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}