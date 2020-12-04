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

namespace Installer
{
    public partial class MainWindow : Window
    {
        int stage = 0;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void HideAll()
        {
            start.Visibility = Visibility.Hidden;
            directory.Visibility = Visibility.Hidden;
            selection.Visibility = Visibility.Hidden;
            partial.Visibility = Visibility.Hidden;
            progress.Visibility = Visibility.Hidden;
            finish.Visibility = Visibility.Hidden;
        }

        private void Update()
        {
            switch (stage) {
                case 0:
                    start.Visibility = Visibility.Visible;
                    break;
                case 1:
                    directory.Visibility = Visibility.Visible;
                    break;
                case 2:
                    selection.Visibility = Visibility.Visible;
                    break;
                case 3:
                    partial.Visibility = Visibility.Visible;
                    break;
                case 4:
                    progress.Visibility = Visibility.Visible;
                    break;
                case 5:
                    finish.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void Next(object sender, RoutedEventArgs e)
        {
            HideAll();
            stage++;
            if (stage < 0) stage = 0;
            if (stage > 5) stage = 5;
            Update();
        }

        private void Forward(object sender, RoutedEventArgs e)
        {
            HideAll();
            stage--;
            if (stage < 0) stage = 0;
            if (stage > 5) stage = 5;
            Update();
        }
    }
}
