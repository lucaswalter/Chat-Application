using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string userName = loginTextBox.Text.Trim();

            this.Hide();
            MainWindow mainWindow = new MainWindow(userName);
            mainWindow.Show();         
        }
    }
}
