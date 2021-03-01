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
    public partial class UserDialog : UserControl
    {
        public UserDialog(string friendly, string email)
        {
            InitializeComponent();
            this.lblName.Content = "用户 " + friendly + " 已登入";
            this.lblAddress.Text = email;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.InvokeDialogCloseCallback();
        }
    }
}
