using Cecs475.BoardGames.Model;
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
using System.Windows.Shapes;

namespace Cecs475.BoardGames.Chess.WpfView {
    /// <summary>
    /// Interaction logic for PawnPromote.xaml
    /// </summary>
    public partial class PawnPromote : Window {

        ChessViewModel vMod;
        BoardPosition startPosi;
        BoardPosition endPosi;

        public PawnPromote(ChessViewModel vm, BoardPosition startPos, BoardPosition endPos) {
            InitializeComponent();
            vMod = vm;
            startPosi = startPos;
            endPosi = endPos;
            if (vm.CurrentPlayer == 1) {
                Queen.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resource/White Queen.png", UriKind.Relative));
                Bishop.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resource/White Bishop.png", UriKind.Relative));
                Rook.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resource/White Rook.png", UriKind.Relative));
                Knight.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resource/White Knight.png", UriKind.Relative));
            }

            else if (vm.CurrentPlayer == 2) {
                Queen.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resource/Black Queen.png", UriKind.Relative));
                Bishop.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resource/Black Bishop.png", UriKind.Relative));
                Rook.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resource/Black Rook.png", UriKind.Relative));
                Knight.Source = new BitmapImage(new Uri("/Cecs475.BoardGames.Chess.WpfView;component/Resource/Black Knight.png", UriKind.Relative));
            }
        }

        public void BorderQueen_MouseEnter(object sender, MouseEventArgs e) {
            Border b = sender as Border;
            b.Background = Brushes.LightBlue;
        }
        public void BorderQueen_MouseLeave(object sender, MouseEventArgs e) {
            Border b = sender as Border;
            b.Background = Brushes.Transparent;
        }

        public void BorderRook_MouseEnter(object sender, MouseEventArgs e) {
            Border b = sender as Border;
            b.Background = Brushes.LightGreen;
        }

        public void Border_MouseLeave(object sender, MouseEventArgs e) {
            Border b = sender as Border;
            b.Background = Brushes.Transparent;
        }

        public void BorderBishop_MouseEnter(object sender, MouseEventArgs e) {
            Border b = sender as Border;
            b.Background = Brushes.LightPink;
        }
        public void BorderKnight_MouseEnter(object sender, MouseEventArgs e) {
            Border b = sender as Border;
            b.Background = Brushes.LightSlateGray;
        }

        public async void BorderQueen_MouseUp(object sender, MouseEventArgs e) {
            await vMod.ApplyMove(endPosi, startPosi, "queen");
            Close();
        }

        public async void BorderRook_MouseUp(object sender, MouseEventArgs e) {
            await vMod.ApplyMove(endPosi, startPosi, "rook");
            Close();
        }

        public async void BorderBishop_MouseUp(object sender, MouseEventArgs e) {
            await vMod.ApplyMove(endPosi, startPosi, "bishop");
            Close();
        }

        public async void BorderKnight_MouseUp(object sender, MouseEventArgs e) {
            await vMod.ApplyMove(endPosi, startPosi, "knight");
            Close();
        }

    }
}
