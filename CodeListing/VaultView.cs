using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for VaultView.xaml
    /// </summary>
    public partial class VaultView : Page
    {
        MainWindow.Account a = new MainWindow.Account(); //account to be viewed
        int Index = MainWindow.uinfo.Accounts.Count;
        Rectangle Current;
        public VaultView(int AccNum)
        {
            InitializeComponent();

            if (AccNum == -1) //-1 means new account
            {
                a.Site = "New Site";
                a.Username = "User";
                a.Password = "password";
                a.Email = "N/A";
                a.Link = "www.website.com";
                a.Color = Colors.Red;
                a.Icon = "Square";
                MainWindow.uinfo.Accounts.Add(a);
                Update(); //saves a file for password
            }
            else
            {
                Index = AccNum;
                a = MainWindow.uinfo.Accounts[Index];
            }

            txtName.Text = a.Site; //sets text boxes
            txtUsername.Text = a.Username;
            txtPassword.Text = a.Password;
            txtEmail.Text = a.Email;
            txtLink.Text = a.Link;
            clrPicker.SelectedColor = a.Color;

            Current = this.FindName(a.Icon) as Rectangle;
            ChangeIcon(Current);
            Current.Stroke = Brushes.Red;
            Current.IsEnabled = false;
        }

        private void lhover(object sender, MouseEventArgs e) //hover fpr labels
        {
            MainWindow.lblHover(sender as Label);
        }

        private void Return(object sender, MouseButtonEventArgs e) //goes back to vault
        {
            MainWindow mainWindow = App.Current.MainWindow as MainWindow;
            mainWindow.WindowFrame.Navigate(new Vault());
        }

        private void ToggleEdit(object sender, MouseButtonEventArgs e) //toggles between edit and view mode
        {
            TextBox[] boxes = new TextBox[] { txtEmail, txtLink, txtName, txtPassword, txtUsername };
            Rectangle[] rects = new Rectangle[] { Square, Circle, Triangle, Star, Bird, Wrench, Wallet, Book, Key, Smile, Note, Controller };
            bool Readonly = true;
            SolidColorBrush fill = new SolidColorBrush(); //fill brush
            fill.Color = Color.FromArgb(204, 255, 255, 255);
            SolidColorBrush border = Brushes.Black;
            Cursor cursor = Cursors.Arrow;
            if (lblEdit.Content.ToString() == "   Edit   ") //switch to edit mode
            {
                lblEdit.Content = "   View   ";
                fill = Brushes.Black;
                border = Brushes.White;
                Readonly = false;
                cursor = Cursors.IBeam;
                clrPicker.Opacity = 1;
            }
            else
            {
                lblEdit.Content = "   Edit   "; //switch to view mode
                clrPicker.Opacity = 0.7;
            }
            clrPicker.IsEnabled = !Readonly;
            foreach (TextBox box in boxes) //applies style to text boxes
            {
                box.IsReadOnly = Readonly;
                box.Background = fill;
                box.BorderBrush = border;
                box.Foreground = border;
                box.Cursor = cursor;
            }
            foreach (Rectangle rect in rects) rect.IsEnabled = !Readonly; //toggles rects and ensures current stays disabled 
            Current.IsEnabled = false;
        }

        private void Navigate(object sender, MouseButtonEventArgs e) //for navigating to hyperlink
        {
            string Text = txtLink.Text.Trim().ToLower(); ;
            if (!Text.Contains("www.")) Text = Text.Insert(0, "www."); //adds www.
            if (!Text.Contains("https://") && !Text.Contains("http://")) Text = Text.Insert(0, "https://"); //adds https://
            try
            {
                Process.Start(new ProcessStartInfo(new Uri(Text).AbsoluteUri)); //hyperlink navigate
            }
            catch //in case hyperlink bad
            {
                MessageBox.Show("Invalid Hyperlink", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void LinkChange(object sender, TextChangedEventArgs e) //events for changing text boxes
        {
            a.Link = txtLink.Text;
            Update();
        }
        private void EmailChange(object sender, TextChangedEventArgs e)
        {
            a.Email = txtEmail.Text;
            Update();
        }
        private void UsernameChange(object sender, TextChangedEventArgs e)
        {
            a.Username = txtUsername.Text;
            Update();
        }
        private void PasswordChange(object sender, TextChangedEventArgs e)
        {
            a.Password = txtPassword.Text;
            Update();
        }
        private void SiteChange(object sender, TextChangedEventArgs e)
        {
            a.Site = txtName.Text;
            Update();
        }

        private void SiteChange(object sender, TextCompositionEventArgs e) //i cant get rid of this for some reason
        {
        }

        private void Update()
        {
            MainWindow.uinfo.Accounts[Index] = a; //updates 
            MainWindow.Encrypt(MainWindow.uinfo, "Users/" + MainWindow.uinfo.User); //saves file for user
        }

        private void Delete(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult r = MessageBox.Show("Are You Sure that You Would Like to Delete this Account?", "Confirmation Required", MessageBoxButton.YesNo, MessageBoxImage.Warning); //gets confirmation
            if (r == MessageBoxResult.Yes)
            {
                MainWindow.uinfo.Accounts.Remove(MainWindow.uinfo.Accounts[Index]); //deletes item
                Return(sender, e);
            }
        }

        private void ColorChange(object sender, RoutedPropertyChangedEventArgs<Color?> e) //on color change
        {
            MainWindow.PlaySound("1Click");
            a.Color = clrPicker.SelectedColor.Value;
            ChangeColor();
            Update();
        }

        private void ChangeColor()
        {
            SolidColorBrush b = new SolidColorBrush(); //updates color of rect
            b.Color = a.Color;
            rectColor.Fill = b;
        }

        private void ChangeIcon(Rectangle rect)
        {
            a.Icon = rect.Name; //adjusts icon when clicked
            Update();
            rectIcon.Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/" + rect.Name + ".png")));
        }

        private void HoverSound(object sender, MouseEventArgs e)
        {
            MainWindow.PlaySound("0Hover");
        }

        private void RectHover(object sender, MouseEventArgs e) //hover + clicking for icon rects //similar to background code
        {
            MainWindow.PlaySound("0Hover");
            Rectangle rect = sender as Rectangle;
            rect.Stroke = Brushes.Yellow;
            rect.MouseDown += RectClick;
            rect.MouseLeave += RectLeave;
            void RectClick(object s, MouseButtonEventArgs es)
            {
                MainWindow.PlaySound("1Click");

                Select(rect);
                rect.MouseDown -= RectClick;
                rect.MouseLeave -= RectLeave;
            }
            void RectLeave(object s, MouseEventArgs es)
            {
                rect.Stroke = Brushes.White;
                rect.MouseDown -= RectClick;
                rect.MouseLeave -= RectLeave;
            }
        }

        private void Select(Rectangle rect) //selects an icon rect
        {
            ChangeIcon(rect);
            Current.Stroke = Brushes.White;
            Current.IsEnabled = true;
            Current = rect;
            rect.Stroke = Brushes.Red;
            rect.IsEnabled = false;
        }

        private void Randomise(object sender, MouseButtonEventArgs e) //gets random icon + color
        {
            Random r = new Random();
            clrPicker.SelectedColor = Color.FromArgb(255, Convert.ToByte(r.Next(30, 256)), Convert.ToByte(r.Next(30, 256)), Convert.ToByte(r.Next(30, 256))); //gets a randoim color
            Rectangle[] rects = new Rectangle[] { Square, Circle, Triangle, Star, Wallet, Wrench, Book, Bird, Controller, Key, Note, Smile }; //gets all rectangles
            Rectangle rect = rects[r.Next(rects.Length)]; //gets a random rectangle to select
            Select(rect);
        }
    }
}
