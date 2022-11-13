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
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Media;
using System.Runtime.Serialization.Formatters.Binary;

namespace PasswordVault
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [Serializable]
        public struct GInfo //General Application Info
        {
            public int Screen { get; set; } //screen size
            public bool Fullscreen { get; set; } //if screen is full
            public double SoundVol { get; set; } 
            public double MusicVol { get; set; }
            public bool Remember { get; set; } //contains user who wishes to stay logged in
            public string CurrentUser { get; set; } //contains user if they were logged in when application exited
            public string Background { get; set; } //contains current background 
        }

        [Serializable]
        public struct UInfo //User Related Info
        {
            public string User { get; set; }
            public string Password { get; set; }
            public List<Account> Accounts { get; set; }
        }

        public struct Account //Struct Containing induvidual account info
        {
            public string Site { get; set; } //details
            public string Link { get; set; }
            public string Email { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Icon { get; set; } //for icon
            public Color Color { get; set; }
        }

        //PUBLIC VARIABLES THAT I SHOULD NOT BE USING 
        //PROBLEM - I DONT KNOW HOW TO TRANSFER DATA FROM A PAGE BACK TO THE MAIN WINDOW efficiently
        public static GInfo ginfo = new GInfo(); //holds general info
        public static UInfo uinfo; //holds user info
        public static bool Logged = false; //bool to check if user currently logged in

        Rectangle CurrentRect;
        public MainWindow() //when app opened
        {
            CreateFolder("AppInfo/Users"); //ensures all required folders exist to prevent crashing
            CreateFolder("Sounds");
            CreateFolder("Music");

            CreateAsset("Sounds", "_0Hover"); //creates sound files
            CreateAsset("Sounds", "_1Click");
            for (int i = 1; i < 21; i++) CreateAsset("Music", "Music"+i); //creates music files

            ginfo.SoundVol = 0.2; //base values if no file
            ginfo.MusicVol = 0.5;
            ginfo.Background = "PaperA";

            JavaScriptSerializer jss = new JavaScriptSerializer(); //writes in all saved structs from files
            try //in case file missing or corrupted
            {
                string Serialised = DecryptFile("GInfo"); //reads GInfo, uses default values if corrupted
                ginfo = jss.Deserialize<GInfo>(Serialised);
            }
            catch { }

            string Corrupted = "";
            string filetext;
            foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory() + "/AppInfo/Users")) //searches through accounts for any corruptions
            {
                filetext = System.IO.Path.GetFileName(file); //gets file name from path
                filetext = filetext.Remove(filetext.Length - 4, 4); //gets rid of ".txt"
                try 
                {
                    jss.Deserialize<UInfo>(DecryptFile("Users/" + filetext));
                }
                catch //deletes any corrupted files
                {
                    if (ginfo.CurrentUser == filetext) ginfo.CurrentUser = ""; //resets current user if needed
                    Corrupted += "\n\t"+filetext;
                    File.Delete(Directory.GetCurrentDirectory() + "/AppInfo/Users/" + filetext + ".txt"); //deletes corrupted user
                }
            }
            if (Corrupted != "") MessageBox.Show("The Following Accounts were Corrupted and had to be Deleted:" + Corrupted, "Error", MessageBoxButton.OK, MessageBoxImage.Error); //alerts if any files corrupted

            InitializeComponent();

            ChangeVol(ginfo.SoundVol); //sets initial values
            ChangeBackground(ginfo.Background);
            ChangeRes(ginfo.Screen);

            if (ginfo.Remember && ginfo.CurrentUser != "") //if remember pressed and a user logged on exit then user relogged
            {
                try //in case account deleted
                {
                    uinfo = jss.Deserialize<UInfo>(DecryptFile("Users/" + ginfo.CurrentUser));
                    if (ginfo.CurrentUser != uinfo.User) //in case someone starts renaming files
                    {
                        MessageBox.Show("Unfortunately, We Could not Log you back in as there was a Verification Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        ginfo.CurrentUser = "";
                    }
                    else ToggleLogged();
  
                }
                catch
                {
                    MessageBox.Show("Unfortunately, We Could not Log you back in as your Account No Longer Exists","Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ginfo.CurrentUser = "";
                }
            }

            CurrentRect = HomeIcon; //sets current to login
            WindowFrame.NavigationService.Navigate(new System.Uri("MainPg.xaml", UriKind.Relative)); //frame starts on login screen

            if (ginfo.Fullscreen)
            {
                this.WindowState = WindowState.Maximized; //remembers screen state
                MaximiseButton.Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/UnFullscreen.png")));
            }
        }

        private void Close(object sender, EventArgs e) //when app closed
        {
            Encrypt(ginfo, "GInfo"); //writes GInfo
            if (Logged)
            {
                Encrypt(uinfo, "Users/" + uinfo.User); //encrypts and writes UInfo if logged
                if (!ginfo.Remember) ginfo.CurrentUser = "";  //only keeps current user if remember pressed
            }
        }

        public static void Encrypt(object input, string filename) //encrypts a string using a key
        {
            string serialised = Serialise(input);

            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = new byte[] { 55, 34, 133, 34, 54 , 23, 43, 255, 92, 122, 174, 11, 23, 100, 197, 56}; //random key for encryption
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(serialised);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }
            string savetext = Convert.ToBase64String(array);
            File.WriteAllText(Directory.GetCurrentDirectory() + "/AppInfo/" + filename + ".txt", savetext);
        }
        public static string DecryptFile(string filename) //decrypts a string using a key
        {
            string cipherText = File.ReadAllText(Directory.GetCurrentDirectory() + "/AppInfo/" + filename + ".txt");

            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = new byte[] { 55, 34, 133, 34, 54, 23, 43, 255, 92, 122, 174, 11, 23, 100, 197, 56 }; //random key for decryption
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static string Serialise(object input) //serialises data
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            return (jss.Serialize(input));
        }

        static MediaPlayer[] Player = new MediaPlayer[] { new MediaPlayer(), new MediaPlayer() }; //for playing sound effects, always exists as creating a new instance has a very annoying 1-2s delay on sounds
        public static void PlaySound(string Sound) //for playing sound effects in application
        {
            int i = Convert.ToInt16(Sound[0]) - 48;
            MediaPlayer p = Player[i];
            p.Open(new Uri(Directory.GetCurrentDirectory() + "/Sounds/_" + Sound + ".mp3")); //Current Sounds = 0Hover, 1Click
            p.Play();
        }

        public async static void ButtonHover(Rectangle Button) //highlights or unhighlights button
        {
            bool Hover = true;
            int Invert = 0; //changes between 1 and 0 for brushes
            PlaySound("0Hover");

            string ImageName = Button.Name;
            ImageName = ImageName.Remove(ImageName.Length - 1); //removes last number
            ImageBrush[] brushes = new ImageBrush[] { new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/" + ImageName + ".png"))),  new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/" + ImageName + "I.png"))) }; //0 normal 1 Inverts

            Button.MouseLeave += ButtonUnHover;
            Button.MouseDown += UpdateName;

            while (Hover) //flashes image
            {
                Invert = 1 - Invert;
                Button.Fill = brushes[Invert];
                await Task.Delay(500);
            }

            void ButtonUnHover(object sender, MouseEventArgs e) //adds unhover event
            {
                Hover = false;
                Button.Fill = brushes[0];
                Button.MouseLeave -= ButtonUnHover;
                Button.MouseDown -= UpdateName;
            }
            void UpdateName(object sender, MouseButtonEventArgs e) //updates brushes when clicked
            {
                PlaySound("1Click");
                ImageName = Button.Name;
                ImageName = ImageName.Remove(ImageName.Length - 1); 
                brushes = new ImageBrush[] { new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/" + ImageName + ".png"))), new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/" + ImageName + "I.png"))) };
                Button.Fill = brushes[Invert];
            }
        }

        public static void ToggleCheck(Rectangle Button) //changes checkbox state to match bool
        {
            char number = Button.Name[Button.Name.Length - 1]; //gets box number
            ImageBrush b;
            if (Button.Name == "UnCheckbox"+number) //sets rect to true
            {
                Button.Name = "Checkbox"+number; //remove UN from name
                b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Checkbox.png")));
            }
            else //sets rect to unchecked
            {
                Button.Name = "UnCheckbox" + number; //adds UN to name
                b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/UnCheckbox.png")));
            }
            Button.Fill = b;
        }

        public async static void lblHover(Label label) //changes hovered label colour
        {
            bool Hover = true;
            bool Clicked = false;
            void lblUnhover(object sender, MouseEventArgs e) //unhover disables routine
            {
                Hover = false;
            }
            async void lblClick(object sender, MouseButtonEventArgs e) //Flashes button when clicked
            {
                PlaySound("1Click");
                if (label.Background != Brushes.Yellow)
                {
                    Brush b = label.Background;
                    Clicked = true; //stops hover effects
                    label.Foreground = Brushes.Black;
                    label.Background = Brushes.Yellow;
                    await Task.Delay(200);
                    label.Foreground = Brushes.White;
                    label.Background = b;
                    Clicked = false;
                }
            }

            void Disabled(object sender, DependencyPropertyChangedEventArgs e) //stops weird color changes when disabled
            {
                Clicked = true;
                Hover = false;
                label.Foreground = Brushes.DarkGray;
            }

            PlaySound("0Hover");

            label.MouseLeave += lblUnhover; //adds other events
            label.MouseDown += lblClick;
            label.IsEnabledChanged += Disabled;
            while (Hover) //flashes while hovered 
            {
                if (!Clicked)
                {
                    label.Foreground = Brushes.Yellow;
                }
                await Task.Delay(500);
                if (!Clicked)
                {
                    label.Foreground = Brushes.White;
                }
                await Task.Delay(500);
            }
            label.MouseLeave -= lblUnhover; //removes events at end
            label.MouseDown -= lblClick;
            label.IsEnabledChanged -= Disabled;
        }

        public static void Togglelbl(Label label) //for disabling + enabling labelling
        {
            label.IsEnabled = !label.IsEnabled;
            if (label.IsEnabled)
            {
                label.Foreground = Brushes.White;
                label.Opacity = 1;
            }
            else
            {
                label.Foreground = Brushes.DarkGray;
                label.Opacity = 0.7;
            }
        }

        public static void ChangeBackground(string Back) //for changing background //uses a string to set back
        {
            ginfo.Background = Back;
            Window mainWindow = Application.Current.MainWindow;
            mainWindow.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/" + Back + ".jpg")));
        }

        public static void ChangeRes(int Size) //for changing screen size
        {
            ginfo.Screen = Size;
            Window mainWindow = Application.Current.MainWindow;
            mainWindow.Left = 0;
            mainWindow.Top = 0;
            mainWindow.Height = 360 + 180 * Size; //adjusts height and width based on size
            mainWindow.Width = 640 + 320 * Size;
        }

        public static void ChangeVol(double vol)
        {
            ginfo.SoundVol = vol;
            foreach (MediaPlayer player in Player) player.Volume = vol; //changes vol of each player
        }

        public static void ToggleLogged() //toggles logged bool + page access
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            ImageBrush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/VaultIconDisabled.png"))); //disabled icon
            Logged = !Logged;
            if (Logged)
            {
                mainWindow.VaultIcon.IsEnabled = true;
                ginfo.CurrentUser = uinfo.User;
                b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/VaultIcon.png"))); //enabled icon
            }
            else
            {
                mainWindow.VaultIcon.IsEnabled = false;
                ginfo.CurrentUser = "";
                Encrypt(uinfo, "Users/" + uinfo.User); //encrypts and writes UInfo before logout
            }
            mainWindow.VaultIcon.Fill = b;
        }

        private void BHover(object sender, MouseEventArgs e) //for hovering buttons
        {
            Rectangle rectangle = sender as Rectangle;
            string Name = rectangle.Name;
            Brush[] brushes = new Brush[] { new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/" + Name + ".png"))), new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/" + Name + "Hover.png"))), new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/" + Name + "Click.png"))) }; //normal, hover, click brushes
            void BUnhover(object senderS, MouseEventArgs eS) //unhover disables routine
            {
                rectangle.Fill = brushes[0];
                rectangle.MouseLeave -= BUnhover;
                rectangle.MouseDown -= BClick;
            }
            void BClick(object senderS, MouseButtonEventArgs eS) //Flashes button when clicked
            {
                PlaySound("1Click");
                rectangle.Fill = brushes[2];
                rectangle.MouseLeave -= BUnhover;
                rectangle.MouseDown -= BClick;
            }

            PlaySound("0Hover");

            rectangle.MouseLeave += BUnhover;
            rectangle.MouseDown += BClick;

            rectangle.Fill = brushes[1];
        }

        private void BlurFrame(object sender, MouseEventArgs e) //Blurs vault when not focused, *IDEA SHAMELESSLY STOLEN FROM DAVID ADAMS
        {
            DoubleAnimation BA = new DoubleAnimation();
            BA.Duration = new Duration(TimeSpan.FromMilliseconds(250));
            BA.From = 0;
            BA.To =30;
            BA.AccelerationRatio = 1;
            FrameBlur.BeginAnimation(BlurEffect.RadiusProperty, BA);
        }
        private void unBlurFrame(object sender, MouseEventArgs e) //unblurs vault when focused
        {
            DoubleAnimation BA = new DoubleAnimation();
            BA.Duration = new Duration(TimeSpan.FromMilliseconds(250));
            BA.From = 30;
            BA.To = 0;
            BA.AccelerationRatio = 1;
            FrameBlur.BeginAnimation(BlurEffect.RadiusProperty, BA);
        }

        bool MouseHeld = false; //code for dragging form around //uses relative mouse movement to move form
        Point LastMousePos;
        private void MouseBarDown(object sender, MouseButtonEventArgs e) //when mouse clicked on bar
        {
            MouseHeld = true;
            LastMousePos = e.GetPosition(this);
        }
        private void MouseBarUp(object sender, MouseButtonEventArgs e) //when mouse lets go of bar
        {
            MouseHeld = false;
        }
        private void MouseOffBar(object sender, MouseEventArgs e) //when mouse leaves bar
        {
            MouseHeld = false;
        }
        private void TryDrag(object sender, MouseEventArgs e) //allows drag if mouseheld true
        {
            if (MouseHeld)
            {
                this.Left += e.GetPosition(this).X - LastMousePos.X;
                this.Top += e.GetPosition(this).Y - LastMousePos.Y;
            }
        }

        private async void CloseApplication(object sender, MouseButtonEventArgs e) //when close button clicked
        {
            DoubleAnimation CA = new DoubleAnimation(); //funky opacity animation
            CA.Duration = new Duration(TimeSpan.FromMilliseconds(900));
            CA.To = 0;
            CA.AccelerationRatio = 1;
            this.BeginAnimation(OpacityProperty, CA);

            await Task.Delay(1300);
            Application.Current.Shutdown();
        }
        private void Maxinormalise(object sender, MouseButtonEventArgs e) //Maximises / Normalises Screen
        {
            ImageBrush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/UnFullscreen.png")));
            ginfo.Fullscreen = !ginfo.Fullscreen;
            if (this.WindowState == WindowState.Normal) this.WindowState = WindowState.Maximized;
            else
            {
                this.WindowState = WindowState.Normal;
                b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Fullscreen.png")));
            }
            MaximiseButton.Fill = b;
        }
        bool Minimised = false;
        private async void Minimise(object sender, MouseButtonEventArgs e) //minimises screen
        {
            DoubleAnimation CA = new DoubleAnimation(); //funky size animation
            CA.Duration = new Duration(TimeSpan.FromMilliseconds(600));
            CA.From = this.Height;
            CA.To = 0;
            CA.AccelerationRatio = 1;
            this.BeginAnimation(HeightProperty, CA);

            if (this.WindowState != WindowState.Maximized) await Task.Delay(600); //does not play animation when fullscreen so no delay needed
            this.WindowState = WindowState.Minimized;
            Minimised = true;
        }
        private void UnMinimise(object sender, EventArgs e)
        {
            if (Minimised) //plays unmimise animation
            {
                Minimised = false;
                DoubleAnimation CA = new DoubleAnimation();
                CA.Duration = new Duration(TimeSpan.FromMilliseconds(600));
                CA.From = 0;
                CA.To = this.Width / 16 * 9;
                CA.AccelerationRatio = 1;
                this.BeginAnimation(HeightProperty, CA);
            }
        }

        private void Navigate(object sender, MouseButtonEventArgs e) //used to navigate between pages
        {
            Brush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/" + CurrentRect.Name + ".png")));
            CurrentRect.Fill = b;
            CurrentRect.IsEnabled = true; //enables last button

            Rectangle button = sender as Rectangle;
            CurrentRect = button;
            button.IsEnabled = false;
            int PageNum = Convert.ToUInt16(button.MinHeight);
            switch (PageNum) //using minheight of button a key for navigation
            {
                case 0:
                    {
                        WindowFrame.Navigate(new MainPg());
                        break;
                    }
                case 1:
                    {
                        WindowFrame.Navigate(new Accounts());
                        break;
                    }
                case 2:
                    {
                        WindowFrame.Navigate(new Vault());
                        break;
                    }
                case 3:
                    {
                        WindowFrame.Navigate(new Gen());
                        break;
                    }
                case 4:
                    {
                        WindowFrame.Navigate(new Settings());
                        break;
                    }
            }
        }

        private void CreateFolder(string Folder) //makes sure folder exists
        {
            if (!System.IO.Directory.Exists(Directory.GetCurrentDirectory() + "/" + Folder)) //checks if folder exists and creates if not
            {
                System.IO.Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/" + Folder);
            }
        }

        private void CreateAsset(string Folder, string Name) //for creating sound files
        {
            if (!System.IO.Directory.Exists(Directory.GetCurrentDirectory() + "/" + Folder + "/" + Name + ".mp3")) //checks if file exists,
            {
                BinaryFormatter bf = new BinaryFormatter(); //gets resource required
                MemoryStream ms = new MemoryStream();
                object obj;
                Byte[] Res;
                obj = Properties.Resources.ResourceManager.GetObject(Name);
                bf.Serialize(ms, obj);
                Res = ms.ToArray();

                File.WriteAllBytes(Directory.GetCurrentDirectory() + "/" + Folder + "/" + Name + ".mp3", Res); //copies from resources if not
            }
        }
    }
}
