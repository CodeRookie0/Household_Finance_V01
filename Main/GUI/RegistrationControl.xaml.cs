using Main.Logic;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using System.Windows.Shapes;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for RegistrationControl.xaml
    /// </summary>
    public partial class RegistrationControl : Window
    {
        public RegistrationControl()
        {
            InitializeComponent();
        }

        private void CompleteRegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UserNameInputTextBox.Text.Trim();
            string email = EmailInputTextBox.Text.Trim();
            string password = PasswordInputBox.Text.Trim();
            string confirmPassword = ConfirmPasswordInputBox.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Proszę wypełnić wszystkie pola.", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Hasła nie pasują do siebie.", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Service.IsEmailExists(email))
            {
                MessageBox.Show("Ten adres e-mail jest już zarejestrowany.", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Service.ValidateEmail(email))
            {
                MessageBox.Show("Wprowadzono nieprawidłowy format adresu e-mail. Proszę wprowadzić poprawny adres e-mail.", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Service.ValidatePassword(password))
            {
                return;
            }

            try
            {
                if (Service.AddUser(username, email, password))
                {
                    MessageBox.Show("Rejestracja zakończona pomyślnie!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    UserNameInputTextBox.Clear();
                    EmailInputTextBox.Clear();
                    PasswordInputBox.Clear();
                    ConfirmPasswordInputBox.Clear();
                }
                else
                {
                    MessageBox.Show("Wystąpił błąd podczas rejestracji. Spróbuj ponownie.", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas rejestracji: {ex.Message}", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginControl loginWindow = new LoginControl();
            loginWindow.Show();
            this.Hide();
        }

        private void CloseImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void RegistrationContainerBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CompleteRegistrationButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ColorAnimation colorAnimation = new ColorAnimation
            {
                To = Colors.White,
                Duration = TimeSpan.FromSeconds(0.2)
            };
            ColorAnimation foregroundAnimation = new ColorAnimation
            {
                To = (Color)ColorConverter.ConvertFromString("#3aa9ad"),
                Duration = TimeSpan.FromSeconds(0.2)
            };

            SolidColorBrush backgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3aa9ad"));
            SolidColorBrush foregroundBrush = new SolidColorBrush(Colors.White);
            CompleteRegistrationButton.Background = backgroundBrush;
            CompleteRegistrationButton.Foreground = foregroundBrush;
            backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            foregroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, foregroundAnimation);
        }

        private void CompleteRegistrationButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ColorAnimation colorAnimation = new ColorAnimation
            {
                To = Color.FromRgb(58, 169, 173),
                Duration = TimeSpan.FromSeconds(0.2)
            };
            ColorAnimation foregroundAnimation = new ColorAnimation
            {
                To = Colors.White,
                Duration = TimeSpan.FromSeconds(0.2)
            };

            SolidColorBrush backgroundBrush = new SolidColorBrush(Colors.White);
            SolidColorBrush foregroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3aa9ad"));
            CompleteRegistrationButton.Background = backgroundBrush;
            CompleteRegistrationButton.Foreground = foregroundBrush;
            backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            foregroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, foregroundAnimation);
        }

        private void UserNameInputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UserNameInputTextBox.Text == "Nazwa użytkownika")
            {
                UserNameInputTextBox.Text = string.Empty;
                UserNameInputTextBox.Foreground = Brushes.Black;
            }
        }

        private void UserNameInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserNameInputTextBox.Text))
            {
                UserNameInputTextBox.Text = "Nazwa użytkownika";
                UserNameInputTextBox.Foreground = Brushes.Gray;
            }
        }

        private void EmailInputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EmailInputTextBox.Text == "E-mail")
            {
                EmailInputTextBox.Text = string.Empty;
                EmailInputTextBox.Foreground = Brushes.Black;
            }
        }

        private void EmailInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailInputTextBox.Text))
            {
                EmailInputTextBox.Text = "E-mail";
                EmailInputTextBox.Foreground = Brushes.Gray;
            }
        }

        private void PasswordInputBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordInputBox.Text == "Hasło")
            {
                PasswordInputBox.Text = string.Empty;
                PasswordInputBox.Foreground = Brushes.Black;
            }
        }

        private void PasswordInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordInputBox.Text))
            {
                PasswordInputBox.Text = "Hasło";
                PasswordInputBox.Foreground = Brushes.Gray;
            }
        }

        private void ConfirmPasswordInputBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ConfirmPasswordInputBox.Text == "Potwierdź hasło")
            {
                ConfirmPasswordInputBox.Text = string.Empty;
                ConfirmPasswordInputBox.Foreground = Brushes.Black;
            }
        }

        private void ConfirmPasswordInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ConfirmPasswordInputBox.Text))
            {
                ConfirmPasswordInputBox.Text = "Potwierdź hasło";
                ConfirmPasswordInputBox.Foreground = Brushes.Gray;
            }
        }
    }
}
