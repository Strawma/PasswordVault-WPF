using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
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
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPg : Page
    {
        public MainPg()
        {
            InitializeComponent();
            if (MainWindow.ginfo.Remember) MainWindow.ToggleCheck(UnCheckbox2); //if remember toggled //
            if (MainWindow.Logged) //logs user if logged //
            {
                txtUsername.Text = MainWindow.uinfo.User;
                txtPassword.Text = MainWindow.uinfo.Password;
                LoginProtocol(); //runs login Protocol
            }
        }

        private void Hover(object sender, MouseEventArgs e)
        {
            MainWindow.ButtonHover(sender as Rectangle);
        }

        private void ToggleRemember(object sender, MouseButtonEventArgs e)
        {
            MainWindow.ginfo.Remember = !MainWindow.ginfo.Remember; //toggles remember //
            MainWindow.ToggleCheck(sender as Rectangle);
        }

        private async void CreateAcc(object sender, MouseButtonEventArgs e) //creates an account
        {
            await Task.Delay(210); //allows effect to occur
            string Username = @txtUsername.Text.Trim(); //removes spaces from ends
            string Password = @txtPassword.Text.Trim();
            if (TextCheck(Username, Password))
            {
            }
            else if (File.Exists(Directory.GetCurrentDirectory() + "/AppInfo/Users/" + Username + ".txt"))
            {
                txtPassword.Text = "";
                MessageBox.Show("User already Exists", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); //if user already exists
            }
            else
            {
                MainWindow.uinfo = new MainWindow.UInfo(); //sets user info when account made //
                MainWindow.uinfo.User = Username[0].ToString().ToUpper() + Username.Substring(1).ToLower(); //capitalises first letter of string //
                MainWindow.uinfo.Password = Password; //
                MainWindow.uinfo.Accounts = new List<MainWindow.Account>(); //
                MainWindow.ToggleLogged(); //logs in

                MainWindow.Encrypt(MainWindow.uinfo, "Users/" + MainWindow.uinfo.User); //writes a file for user

                LoginProtocol();
                MessageBox.Show("Account Created Successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information); //notifies user of creation
            }
        }

        private void TogglePassword(object sender, MouseButtonEventArgs e) //switches password fonts to show / unshow it
        {
            if (txtPassword.FontFamily.ToString() == "Consolas") txtPassword.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./resources/#Password");
            else txtPassword.FontFamily = new FontFamily("Consolas");
            MainWindow.ToggleCheck(sender as Rectangle);
        }

        private void lhover(object sender, MouseEventArgs e) //hover fpr labels
        {
            MainWindow.lblHover(sender as Label);
        }

        private async void Login(object sender, MouseButtonEventArgs e) //allows user to login
        {
            await Task.Delay(210); //allows effect to occur
            string Username = @txtUsername.Text.Trim(); //removes spaces from ends
            string Password = @txtPassword.Text.Trim();

            if (TextCheck(Username, Password))
            {
            }
            else
            {
                if (!File.Exists(Directory.GetCurrentDirectory() + "/AppInfo/Users/" + Username + ".txt"))
                {
                    txtPassword.Text = "";
                    MessageBox.Show("User Does not Exist", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); //if user does not exist
                }
                else
                {
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    MainWindow.UInfo tempuser = jss.Deserialize<MainWindow.UInfo>(MainWindow.DecryptFile("Users/" + Username)); //decrypts username to check
                    if (tempuser.Password != Password)
                    {
                        txtPassword.Text = "";
                        MessageBox.Show("Incorrect Password", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); //if password incorrect
                    }
                    else
                    {
                        MainWindow.uinfo = tempuser; //
                        MainWindow.ToggleLogged(); //logs in //
                        LoginProtocol();
                    }
                }
            }
        }

        private void LoginProtocol()
        {
            SolidColorBrush fill = Brushes.White;
            SolidColorBrush border = Brushes.Black;
            TextBox[] boxes = new TextBox[] { txtUsername, txtPassword };
            foreach (TextBox box in boxes) //disables textboxes + buttons
            {
                box.Focusable = false;
                box.Cursor = Cursors.Arrow;
                box.Background = fill;
                box.Foreground = border;
                box.BorderBrush = border;
            }
            MainWindow.Togglelbl(lblLogin);
            MainWindow.Togglelbl(lblCreateAcc);
            MainWindow.Togglelbl(lblLogout);
        }

        private async void LogOut(object sender, MouseButtonEventArgs e)
        {
            MainWindow.ToggleLogged(); ; //logs out

            await Task.Delay(210); //allows effect to occur
            SolidColorBrush fill = Brushes.Black;
            SolidColorBrush border = Brushes.White;
            TextBox[] boxes = new TextBox[] { txtUsername, txtPassword };
            foreach (TextBox box in boxes) //disables textboxes + buttons
            {
                box.Focusable = true;
                box.Cursor = Cursors.IBeam;
                box.Background = fill;
                box.Foreground = border;
                box.BorderBrush = border;
            }
            txtPassword.Text = "";
            MainWindow.Togglelbl(lblLogin);
            MainWindow.Togglelbl(lblCreateAcc);
            MainWindow.Togglelbl(lblLogout);
        }

        private bool TextCheck(string U, string P) //checks input is safe
        {
            string[] illegal = new string[] { @"\", "/", @"'", "*", "|", "<", ">", "\"", "?" }; //llist of illegal characters that shouldn't be in username / password
            if (U == "" || P == "")
            {
                MessageBox.Show("Please Fill in Account Details", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); //chechs that details have been filled
                return true;
            }
            if (illegal.Any((U + P).Contains)) //checks for illegal characters
            {
                txtUsername.Text = "";
                txtPassword.Text = "";
                MessageBox.Show("Please Make Sure that your Username + Password do not Contain these Characters:\n\t\\ \n\t/ \n\t\' \n\t* \n\t| \n\t< \n\t> \n\t? ", "Illegal Characters Detected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return true;
            }
            else return false;
        }
    }
}
