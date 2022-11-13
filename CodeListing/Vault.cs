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

namespace PasswordVault
{
    /// <summary>
    /// Interaction logic for Vault.xaml
    /// </summary>
    public partial class Vault : Page
    {
        int DNum = 0;
        public Vault()
        {
            MainWindow.uinfo.Accounts.Sort((s1, s2) => s1.Site.CompareTo(s2.Site)); //sorts list alphabetically
            InitializeComponent();
            DisplayAccs(DNum);
        }

        private void DisplayAccs(int Start)
        {
            lblNext.Visibility = Visibility.Visible; //shows bottom buttons at start
            lblBack.Visibility = Visibility.Visible;
            if (Start == 0) lblBack.Visibility = Visibility.Hidden; //hides back button if at start
            Label[] labels = new Label[] { lblAcc0, lblAcc1, lblAcc2, lblAcc3, lblAcc4, lblAcc5, lblAcc6, lblAcc7, lblAcc8, lblAcc9 }; //gets a list of labels, icons and view buttons
            Label[] buttons = new Label[] { lblView0, lblView1, lblView2, lblView3, lblView4, lblView5, lblView6, lblView7, lblView8, lblView9 };
            Rectangle[] icons = new Rectangle[] { rectIcon0, rectIcon1, rectIcon2, rectIcon3, rectIcon4, rectIcon5, rectIcon6, rectIcon7, rectIcon8, rectIcon9 };
            Rectangle[] colors = new Rectangle[] { rectColor0, rectColor1, rectColor2, rectColor3, rectColor4, rectColor5, rectColor6, rectColor7, rectColor8, rectColor9 };
            foreach (Label label in labels) label.Visibility = Visibility.Hidden; //initially hides all labels, icons and buttons
            foreach (Label button in buttons) button.Visibility = Visibility.Hidden;
            foreach (Rectangle icon in icons) icon.Visibility = Visibility.Hidden;
            foreach (Rectangle color in colors) color.Visibility = Visibility.Hidden;
            int Limit = 10;
            Start *= Limit; //num is starting position of display //can display 10 nums at a time
            if (MainWindow.uinfo.Accounts.Count != 0) //no loop if list empty
            {
                if (MainWindow.uinfo.Accounts.Count - Start <= Limit)//if less than limit doesn't show all
                {
                    Limit = MainWindow.uinfo.Accounts.Count - Start; 
                    lblNext.Visibility = Visibility.Hidden;
                }
                for (int i = 0; i < Limit; i++)
                {
                    labels[i].Visibility = Visibility.Visible; //shows and fills in needed display labels
                    buttons[i].Visibility = Visibility.Visible;
                    icons[i].Visibility = Visibility.Visible;
                    colors[i].Visibility = Visibility.Visible;
                    labels[i].Content = MainWindow.uinfo.Accounts[Start + i].Site;
                    labels[i].BorderBrush = Brushes.Black; //in case label highlighted
                    SolidColorBrush c = new SolidColorBrush(); //gets icon color
                    c.Color = MainWindow.uinfo.Accounts[Start + i].Color;
                    colors[i].Fill = c;
                    ImageBrush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/" + MainWindow.uinfo.Accounts[Start + i].Icon + ".png")));
                    icons[i].Fill = b;
                }
            }
            else lblNext.Visibility = Visibility.Hidden;
        }

        private void lhover(object sender, MouseEventArgs e) //hover fpr labels
        {
            MainWindow.lblHover(sender as Label);
        }

        private void NextDisplay(object sender, MouseButtonEventArgs e) //adjusts display using button minwidth
        {
            DNum += 1;
            DisplayAccs(DNum);
        }
        private void BackDisplay(object sender, MouseButtonEventArgs e) //adjusts display using button minwidth
        {
            DNum -= 1;
            DisplayAccs(DNum);
        }

        private void Create(object sender, MouseButtonEventArgs e) //for creating a vault
        {
            MainWindow mainWindow = App.Current.MainWindow as MainWindow;
            mainWindow.WindowFrame.Navigate(new VaultView(-1));
        }

        private void View(object sender, MouseButtonEventArgs e) //for viewing a vault
        {
            Label label = sender as Label;
            int Index = DNum * 10 + int.Parse(""+label.Name[label.Name.Length - 1]);
            MainWindow mainWindow = App.Current.MainWindow as MainWindow;
            mainWindow.WindowFrame.Navigate(new VaultView(Index));
        }

        private void Search(object sender, TextChangedEventArgs e) //searches for an entry or similar entries
        {
            string Search = txtSearch.Text.Trim().ToLower(); //adds search term to temp list of names

            if (Search != "") //does search if box not empty
            {
                List<string> SiteNames = new List<string>();
                foreach (MainWindow.Account account in MainWindow.uinfo.Accounts) SiteNames.Add(account.Site);
                SiteNames.Add(Search);
                SiteNames.Sort(); //alphabetically sorts name list
                DNum = SiteNames.IndexOf(Search) / 10; //Index DIV 10 for page Number
                DisplayAccs(DNum);

                Label[] labels = new Label[] { lblAcc0, lblAcc1, lblAcc2, lblAcc3, lblAcc4, lblAcc5, lblAcc6, lblAcc7, lblAcc8, lblAcc9 }; //gets a list of labels
                foreach (Label label in labels) if (label.Content.ToString().ToLower().Contains(Search)) label.BorderBrush = Brushes.Yellow; //highlights any labels containing search
            }
            else DisplayAccs(DNum);
        }
    }
}
