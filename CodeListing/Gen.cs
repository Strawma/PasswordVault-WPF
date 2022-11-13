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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PasswordVault
{
    /// <summary>
    /// Interaction logic for Gen.xaml
    /// </summary>
    public partial class Gen : Page
    {
        public Gen()
        {
            InitializeComponent();
        }

        private void lhover(object sender, MouseEventArgs e) //hover fpr labels
        {
            MainWindow.lblHover(sender as Label);
        }

        private void Generate(object sender, MouseButtonEventArgs e) //generates random password using text document of words
        {
            string[] words = Properties.Resources.Words.Replace("\r","").Split('\n');
            string[] symbols = new string[] { "@", "!", "$", "&", ",", "%", "^" };
            Random r = new Random();
            string Password = words[r.Next(words.Length)] + words[r.Next(words.Length)] + r.Next(100, 999) + symbols[r.Next(symbols.Length)];
            lblGenPass.Content = Password;

        }

        private void Copy(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(lblGenPass.Content.ToString());
        }

        private void StrengthCheck(object sender, TextChangedEventArgs e) //judges password based on a series of hoops
        {
            string p = txtCheckPass.Text.Replace(" ","");
            int score = p.Length *4;

            if (p.Length != 0)
            {
                bool Upper = p.Any(char.IsUpper); //checks for upper + lowercase 
                bool Lower = p.Any(char.IsLower);
                if (Upper && Lower && (p.Any(char.IsDigit) || p.Any(char.IsSymbol)) && p.Length >= 12) score += p.Length * 2; //bonus for meeting safe requirements
                int Repeat = 0; //tracks repeat values
                char last = ' '; //tracks last character
                for (int i = 0; i < p.Length; i++) //goes through every digit of password
                {
                    int MidBonus = 0; //bonus for symbols + letters in middle of character
                    if (i != 0 && i != p.Length - 1) MidBonus = 2; //checks for middle
                    char l = p[i];
                    if (l == last) Repeat += 1; //counts consecutive repeats
                    if ((char.IsLetter(l) && char.IsLetter(last)) || (char.IsDigit(l) && char.IsDigit(last))) score -= 2; //deduction for consecutives
                    if (char.IsSymbol(l) || char.IsPunctuation(l)) score += 6 + MidBonus; //points for symbols + punct
                    else if (char.IsDigit(l)) score += 4 + MidBonus; //points for numbers
                    if (Upper && !char.IsUpper(l)) score += 2; //if lower / upper exists, adds bonus for every other character
                    else if (Lower && !char.IsLower(l)) score += 2;
                    last = l; //new last character
                }
                score -= Repeat * Repeat / p.Length * 10; //deductions for consecutive repeats
                if (p.All(char.IsDigit)) score -= p.Length; //checks if all numbers in password
                if (p.All(char.IsLetter)) score -= p.Length; //checks if all Letters in password
            }

            SolidColorBrush b = Brushes.DarkRed; //different score brackets for password
            string Display = "Very Weak";

            if (score < 0) score = 0; //minimum value of score to 0
            if (score >= 130)
            {
                b = Brushes.LightBlue;
                Display = "Very Strong";
            }
            else if (score >= 100)
            {
                b = Brushes.Green;
                Display = "Strong";
            }
            else if (score >= 70)
            {
                b = Brushes.Orange;
                Display = "Good";
            }
            else if (score >= 30)
            {
                b = Brushes.Red;
                Display = "Weak";
            }

            lblRating.Content = Display;
            barStrength.Foreground = b;

            DoubleAnimation bmove = new DoubleAnimation(); //animates bar movement
            bmove.From = barStrength.Value;
            bmove.To = score;
            bmove.Duration = TimeSpan.FromMilliseconds(800);
            barStrength.BeginAnimation(ProgressBar.ValueProperty, bmove);
        }
    }
}
