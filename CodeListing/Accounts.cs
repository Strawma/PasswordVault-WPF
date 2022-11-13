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
using System.IO;

namespace PasswordVault
{
    /// <summary>
    /// Interaction logic for Accounts.xaml
    /// </summary>
    public partial class Accounts : Page
    {
        List<string> accs = new List<string>();
        int DNum = 0;
        public Accounts()
        {
            InitializeComponent();
            string text;
            foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory() + "/AppInfo/Users/")) //gets accounts and puts them in a list
            {
                text = file.Remove(file.Length - 4, 4).Remove(0, 85); //removes start and end from file path
                accs.Add(text);
            }
            DisplayAccs(DNum);

            if (MainWindow.Logged) //checks if user logged for account features to enable //
            {
                txtPassword.IsEnabled = true;
                txtPassword.Cursor = Cursors.IBeam;
                txtPassword.Opacity = 1;
                lblChangeP.IsEnabled = true;
                lblChangeP.Opacity = 1;
                lblChangeP.Foreground = Brushes.White;
                lblDelete.Foreground = Brushes.White;
                lblDelete.IsEnabled = true;
                lblDelete.Opacity = 1;
            }
        }

        private void DisplayAccs(int Start)
        {
            lblNext.Visibility = Visibility.Visible; //shows bottom buttons at start
            lblBack.Visibility = Visibility.Visible;
            if (Start == 0) lblBack.Visibility = Visibility.Hidden; //hides back button if at start
            Label[] labels = new Label[] { lblUser0, lblUser1, lblUser2, lblUser3, lblUser4, lblUser5 }; //gets a list of 6 display labels
            foreach (Label label in labels) label.Visibility = Visibility.Hidden; //initially hides all labels
            int Limit = 6;
            Start *= Limit; //num is starting position of display //can display 6 nums at a time
            if (accs.Count != 0) //no loop if list empty
            {
                if (accs.Count - Start <= Limit)//if less than 6 doesn't show 6
                {
                    Limit = accs.Count - Start;
                    lblNext.Visibility = Visibility.Hidden;
                }
                for (int i = 0; i < Limit; i++)
                {
                    labels[i].Visibility = Visibility.Visible; //shows and fills in needed display labels
                    labels[i].Content = accs[Start + i];
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

        private void PasswordChange(object sender, MouseButtonEventArgs e)
        {
            string Password = txtPassword.Text.Trim();
            txtPassword.Text = "";
            string[] illegal = new string[] { @"\", "/", @"'", "*", "|", "<", ">", "\"", "?" }; //llist of illegal characters that shouldn't be in username / password
            if (Password == "") MessageBox.Show("Please Enter a Password First", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); //chechs that details have been filled
            else if (illegal.Any(Password.Contains)) MessageBox.Show("Please Make Sure that your Password does not Contain these Characters:\n\t\\ \n\t/ \n\t\' \n\t* \n\t| \n\t< \n\t> \n\t? ", "Illegal Characters Detected", MessageBoxButton.OK, MessageBoxImage.Warning); //checks for illegal characters
            else
            {
                MessageBox.Show("Password Successfully Changed to \"" + Password + "\"", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.uinfo.Password = Password;
            }
        }

        private void Delete(object sender, MouseButtonEventArgs e) //when deleting user
        {
            MessageBoxResult r = MessageBox.Show("Are You Sure that You Would Like to Delete this Account? This will also Delete Your Saved Passwords", "Confirmation Required", MessageBoxButton.YesNo, MessageBoxImage.Warning); //gets confirmation
            if (r == MessageBoxResult.Yes)
            {
                MainWindow.ToggleLogged(); //logs out
                File.Delete(Directory.GetCurrentDirectory() + "/AppInfo/Users/" + MainWindow.uinfo.User + ".txt"); //deletes user
                MainWindow mainWindow = App.Current.MainWindow as MainWindow; //resets page
                mainWindow.WindowFrame.Navigate(new Accounts());
            }
        }
    }
}
