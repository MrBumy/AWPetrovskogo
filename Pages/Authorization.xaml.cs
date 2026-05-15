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
using AWPetrovskogo.Data;

namespace AWPetrovskogo.Pages
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Page
    {
        public Authorization()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var user = ConnectObject.GetConnect().Users.AsNoTracking().FirstOrDefault(u => u.Login == TBLogin.Text);

            if (user == null)
            {
                MessageBox.Show("Пользователя с таким логином не существует!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(TBLogin.Text) && string.IsNullOrEmpty(PBPassword.Password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(TBLogin.Text))
            {
                MessageBox.Show("Введите логин!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                TBLogin.Focus();
                return;
            }

            if (string.IsNullOrEmpty(PBPassword.Password))
            {
                MessageBox.Show("Введите пароль!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                PBPassword.Focus();
                return;
            }

            if (user.IsBlocked == true)
            {
                MessageBox.Show("Вы заблокированы! Обратитесь к администратору.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return ;
            }

            if (user.Login == TBLogin.Text)
            {
                if (user.IsBlocked == true)
                {
                    MessageBox.Show("Вы заблокированы! Обратитесь к администратору.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    btnLogin.IsEnabled = false;
                    return;
                }
                if (user.Login == TBLogin.Text && user.Password != PBPassword.Password)
                {
                    MessageBox.Show("Неправильно введен пароль!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    user.AmountOfMistackes++;
                    ConnectObject.GetConnect().SaveChanges();
                    if (user.AmountOfMistackes >= 3)
                    {
                        user.IsBlocked = true;
                        user.AmountOfMistackes = 0;
                        ConnectObject.GetConnect().SaveChanges();
                    }
                    MessageBox.Show($"Осталось {3 - user.AmountOfMistackes} попыток входа", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                else
                {
                    MessageBox.Show("Вы успешно авторизовались!", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);

                    switch (user.RoleID)
                    {
                        case 1:
                            FrameObject.frameMain.Navigate(new AdministratorPage());
                            break;
                    }
                }
            }
        }
    }
}
