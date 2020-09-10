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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Simula {
   
    public partial class Splash : Window {
        DispatcherTimer timer = new DispatcherTimer();
        public Splash() {
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Start();
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e) {
            Me.Instance.Show();
            this.Hide();
            timer.Stop();
        }
    }

    public static class Me {
        public static MainWindow Instance = new MainWindow();
    }
}
