using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Simula.Pages
{
    public partial class UserRegisterDialog : UserControl
    {
        public UserRegisterDialog()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.InvokeDialogCloseCallback();
        }

        public string FriendlyName { get => this.inputFriendlyName.Text; }
        public string Password { get => this.inputPassword.Password; }
    }
}
