using DemoExam2.Data;
using DemoExam2.Helpers;
using DemoExam2.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Input;

namespace DemoExam2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var login = LoginEnter.Text;
            var password = PasswordEnter.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageHelper.ShowError("Не введено имя пользователя или пароль");
                return;
            }

            User? user;
            try
            {
                using var dbContext = new AppDbContext();
                user = dbContext.Users
                    .Include(u => u.IdRoleNavigation)
                    .FirstOrDefault(u => u.Login == login && u.Password == password);
            }
            catch (Exception error)
            {
                MessageHelper.ShowError("Не удалось выполнить запрос к базе данных:\n" + error.GetBaseException().Message);
                return;
            }

            if (user == null)
            {
                MessageHelper.ShowError("Пользователь с указанным логином и паролем не найден");
                return;
            }

            new CatalogWindow(user).Show();
            Close();
        }

        private void GuestLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            new CatalogWindow().Show();
            Close();
        }
    }
}
