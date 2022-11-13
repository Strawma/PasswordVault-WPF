using System;
using System.Collections.Generic;
using System.IO;
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

namespace PasswordVault
{
    /// <summary>
    /// Interaction logic for MusicDashboard.xaml
    /// </summary>
    public partial class MusicDashboard : Page
    {
        private struct MInfo //Music Related Info
        {
            public MInfo(bool randomise, bool loop, double vol, int track, bool playing) //info for player
            {
                Randomise = randomise;
                Loop = loop;
                Track = track;

                Playing = playing;
            }

            public bool Randomise { get; set; }
            public bool Loop { get; set; }
            public int Track { get; set; }

            public bool Playing { get; set; }
        }

        MediaPlayer MusicPlayer = new MediaPlayer();
        MInfo Info = new MInfo(true, false, 0.6, 1, true);
        List<int> PrevTracks = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //stores 10 latest tracks

        public MusicDashboard()
        {
            InitializeComponent();
            sldVol.Value = MainWindow.ginfo.MusicVol; //remembers volume
            MusicPlayer.Volume = sldVol.Value;
            sldVol.ValueChanged += this.VolChange; //event added after to stop initial overwrite
            Random r = new Random();
            Info.Track = r.Next(1, 21); //random track at start

            MusicPlayer.MediaEnded += MusicSkip;
            MusicPlayer.Open(new Uri(Directory.GetCurrentDirectory() + "/Music/Music" + Info.Track + ".mp3"));
            MusicPlayer.Play();
        }

        private void MusicPlay(object sender, MouseButtonEventArgs e) //alternates between playing / stopping music
        {
            ImageBrush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Stop.png")));
            if (Info.Playing)
            {
                MusicPlayer.Pause();
                b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Play.png")));
            }
            else MusicPlayer.Play();
            PlayButton.Fill = b;
            Info.Playing = !Info.Playing;
        }

        private void MusicSkip(object sender, EventArgs e)
        {
            if (Info.Randomise)
            {
                Random r = new Random();
                int Current = Info.Track;
                PrevTracks.Insert(0,Info.Track); //adds song to PrevTracks
                PrevTracks.RemoveAt(10); //removes last song
                while (Info.Track == PrevTracks[0]) Info.Track = r.Next(1, 21); //randomises song to DIFFERENT SONG
            }
            else //adds 1 if linear order
            {
                Info.Track += 1;
                if (Info.Track == 21) Info.Track = 1; //loops track around
            }

            ImageBrush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Stop.png")));
            PlayButton.Fill = b;
            MusicPlayer.Open(new Uri(Directory.GetCurrentDirectory() + "/Music/Music" + Info.Track + ".mp3"));
            Info.Playing = true;
            MusicPlayer.Play();
        }

        private void MusicBack(object sender, MouseButtonEventArgs e)
        {
            if (Info.Randomise) //randomise will move back to last song played
            {
                if (PrevTracks[0] == 0)
                {
                    Random r = new Random(); //if no song stored go to a random one
                    Info.Track = r.Next(1, 21);
                }
                else Info.Track = PrevTracks[0]; ;
                PrevTracks.RemoveAt(0);
                PrevTracks.Add(0);
            }
            else //minus 1 if linear order
            {
                Info.Track -= 1;
                if (Info.Track == 0) Info.Track = 20; //loops track around
            }

            MusicPlayer.Open(new Uri(Directory.GetCurrentDirectory() + "/Music/Music" + Info.Track + ".mp3"));
            Info.Playing = true;
            ImageBrush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Stop.png")));
            PlayButton.Fill = b;
            MusicPlayer.Play();
        }

        private void ToggleShuffle(object sender, MouseButtonEventArgs e)
        {
            Info.Randomise = !Info.Randomise; //shuffle toggled
            ImageBrush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Shuffle.png")));
            if (Info.Randomise) b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Shuffling.png")));
            ShuffleButton.Fill = b;
        }

        private void VolChange(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MusicPlayer.Volume = sldVol.Value; //vol change
            MainWindow.ginfo.MusicVol = sldVol.Value;
        }

        private void ButtonHover(object sender, MouseEventArgs e)
        {
            Rectangle rect = sender as Rectangle; //make button bigger when hovered
            ScaleTransform scaleup = new ScaleTransform();
            scaleup.ScaleX = 1.05;
            scaleup.ScaleY = 1.05;
            rect.RenderTransform = scaleup;

            rect.MouseLeave += ButtonLeave;
            void ButtonLeave(object senderB, MouseEventArgs eB) //make button small again when leave
            {
                rect.MouseLeave -= ButtonLeave;
                scaleup.ScaleX = 1;
                scaleup.ScaleY = 1;
            }
        }
    }
}
