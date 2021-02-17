using System;
using System.Windows;
using System.Windows.Threading;

namespace Simula
{

    public partial class Splash : Window
    {
        private readonly DispatcherTimer timer = new DispatcherTimer();
        public Splash()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Start();
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Me.Instance.Show();
            Hide();
            timer.Stop();
        }
    }

    public static class Me
    {
        public static MainWindow Instance = new MainWindow();
    }
}
