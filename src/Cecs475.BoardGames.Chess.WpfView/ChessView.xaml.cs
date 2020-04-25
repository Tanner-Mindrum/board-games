using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.WpfView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cecs475.BoardGames.Chess.WpfView {
    /// <summary>
    /// Interaction logic for ChessView.xaml
    /// </summary>
    public partial class ChessView : UserControl, IWpfGameView {
        private ChessSquare selectedSquare;

        public ChessView() {
            InitializeComponent();
        }
        private void Border_MouseEnter(object sender, MouseEventArgs e) {
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            var vm = FindResource("vm") as ChessViewModel;

            //square.IsCheck = (vm.IsCheck == true) ? true : false;
          
            if (selectedSquare != null) {
                foreach (ChessMove m in vm.PossibleMoves) {
                    if (m.StartPosition == selectedSquare.Position) {
                        if (m.EndPosition == square.Position) {
                            square.IsHighlighted = true;
                        }
                    }
                }
            } /*else if (vm.IsCheck) {
                square.IsCheck = true;
            }*/ else if (vm.PossibleStartMoves.Contains(square.Position)) {
                square.IsHighlighted = true;
            }
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e) {
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            square.IsHighlighted = false;
        }

        public ChessViewModel ChessViewModel => FindResource("vm") as ChessViewModel;

        public Control ViewControl => this;
        public IGameViewModel ViewModel => ChessViewModel;

        private void Border_MouseUp(object sender, MouseButtonEventArgs e) {
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            var vm = FindResource("vm") as ChessViewModel;
            if (vm.PossibleStartMoves.Contains(square.Position)) {
                if (selectedSquare != null) {  // If it's selected
                    selectedSquare.IsSelected = false;  // Then unselect it
                }
                square.IsHighlighted = false;  // unhighlight current square
                square.IsSelected = true;  // select current square
                
                selectedSquare = square;  // reset field to square we just clicked
            }

            else if (selectedSquare != null) {
                if (square.IsHighlighted) {
                    //if move is pawn promote
                    // open pawn promote window
                    if ((square.Position.Row == 0 || square.Position.Row == 7) && selectedSquare.Player.PieceType == ChessPieceType.Pawn) {
                        var panel = new PawnPromote(vm, selectedSquare.Position, square.Position);
                        panel.Show();
                    } else {
                        vm.ApplyMove(square.Position, selectedSquare.Position, "");
                        selectedSquare.IsSelected = false;
                        selectedSquare = null;
                    }
                }
            }

            if (!vm.PossibleStartMoves.Contains(square.Position)) {
                if (selectedSquare != null) {
                    selectedSquare.IsSelected = false;
                    selectedSquare = null;
                }
            }
        }
    }
}
