using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
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

namespace Cecs475.BoardGames.WpfApp {
    /// <summary>
    /// Interaction logic for LoadScreen.xaml
    /// </summary>
    public partial class LoadScreen : Window {
        public LoadScreen() {
            InitializeComponent();
        }

        public class BoardGames {
            public Game[] Games { get; set; }
        }

        public class Game {
            public string Name { get; set; }
            public GameDetails[] Files { get; set; }
        }

        public class GameDetails {
            public string FileName { get; set; }
            public string Url { get; set; }
            public string PublicKey { get; set; }
            public string Version { get; set; }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            var client = new RestClient("https://cecs475-boardamges.herokuapp.com/api/games");
            var request = new RestRequest("https://cecs475-boardamges.herokuapp.com/api/games", Method.GET);
            var response = await client.ExecuteAsync(request);
            var loaded = JsonConvert.DeserializeObject<List<Game>>(response.Content);

            foreach (Game game in loaded) {
                WebClient webClient = new WebClient();
                foreach (GameDetails details in game.Files) {
                   await webClient.DownloadFileTaskAsync(details.Url, @"games\" + details.Url.Split('/').Last());
                }
            }

            new GameChoiceWindow().Show();
            Close();
        }
    }
}
