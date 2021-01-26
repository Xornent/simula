using Odyssey.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Simula
{
    public partial class MainWindow : Window
    {

        /*                    Consructors and Window Features
        * -------------------------------------------------------------------------
        * this region defines and implements basic windows feature (close, application,
        * minimize, maximize, move and resize). this should remain stable.
        */

        public MainWindow()
        {
            InitializeComponent();
            StateChanged += Window_StateChanged;
            SizeChanged += MainWindow_SizeChanged;
            SizeChangeEnquiry.Interval = TimeSpan.FromMilliseconds(500);
            SizeChangeEnquiry.Tick += SizeChangeEnquiry_Tick;
            SizeChangeEnquiry.Start();
            popDialogHost.Closed += PopDialogHost_Closed;
            popDialogHost.Opened += PopDialogHost_Opened;

            // this application supports only left-handed operation. otherwise, it will 
            // naturally occur many layout problems

            var isleft = SystemParameters.MenuDropAlignment;
            if (isleft) {
                var sys = typeof(SystemParameters);
                var alignment = sys.GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
                alignment.SetValue(null, false);
            }

            DialogCloseCallback += (sender, args) => {
                popDialogHost.IsOpen = false;
            };
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                Margin = new Thickness(8, 7, 8, 8);
            else
                Margin = new Thickness(0);
        }

        private void Application_Close(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Application_Minimize(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Application_Maximize(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized) {
                WindowState = WindowState.Normal;
                MainGrid.Margin = new Thickness(0, 0, 0, 0);
                InstallMainGrid.Margin = new Thickness(0, 0, 0, 0);
            } else {
                WindowState = WindowState.Maximized;
                MainGrid.Margin = new Thickness(8, 7, 8, 8);
                InstallMainGrid.Margin = new Thickness(8, 7, 8, 8);
            }
        }

        private void Window_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        /*                               Tab Pages
         * -------------------------------------------------------------------------
         * this include the creation, disposal and management of tabs, views of tabs,
         * content and pages of tabs and so on.
         */

        public static double GetStringWidth(TextBlock label)
        {
            return GetStringWidth(label.Text.ToString(), label.FontFamily,
                label.FontStyle, label.FontWeight, label.FontStretch, label.FontSize);
        }

#pragma warning disable CS0618
        public static double GetStringWidth(string str, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double FontSize)
        {
            var formattedText = new FormattedText(
                      str,
                      CultureInfo.CurrentUICulture,
                      FlowDirection.LeftToRight,
                      new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                      FontSize,
                      Brushes.Black
                      );
            Size size = new Size(formattedText.Width, formattedText.Height);
            return size.Width;
        }

        private readonly List<Grid> TabWindows = new List<Grid>();
        private readonly List<UserControl> TabPages = new List<UserControl>();
        private readonly List<Image> TabImages = new List<Image>();
        private readonly List<TextBlock> TabSymbolImages = new List<TextBlock>();
        private readonly List<TextBlock> TabTexts = new List<TextBlock>();
        private readonly List<string> TabIndices = new List<string>();
        private readonly DispatcherTimer SizeChangeEnquiry = new DispatcherTimer();
        private int _selectedTabIndex = 0;

        private int SelectedTabIndex {
            get => _selectedTabIndex;
            set {
                _selectedTabIndex = value;
                for (int c = 0; c < TabWindows.Count; c++) {
                    string i = TabIndices[c];
                    var rect = (Rectangle)(FindName("rectWindow_" + i));
                    if (c == value) {
                        rect.Fill = (LinearGradientBrush)FindResource("tabPanel");
                    } else {
                        rect.Fill = new SolidColorBrush(Color.FromArgb(255, 189, 189, 189));
                    }
                }
            }
        }

        public Grid CreateTabWindow(int width, string id, ImageSource src = null, string text = "")
        {
            Grid container = new Grid();
            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 168, 168, 168));
            border.BorderThickness = new Thickness(1);
            Rectangle rect = new Rectangle();
            rect.Fill = new SolidColorBrush(Color.FromArgb(255, 189, 189, 189));
            rect.Width = width;
            rect.Effect = new System.Windows.Media.Effects.DropShadowEffect()
            {
                Direction = 180,
                Opacity = 0.08,
                Color = Colors.Gray,
                BlurRadius = 5
            };

            Image titleImg = new Image();
            titleImg.Width = 16;
            titleImg.Height = 16;
            titleImg.Margin = new Thickness(5);
            TextBlock blockImg = new TextBlock();
            blockImg.Text = "\ue132";
            blockImg.Width = 20;
            blockImg.FontSize = 12;
            blockImg.VerticalAlignment = VerticalAlignment.Center;
            blockImg.TextAlignment = TextAlignment.Center;
            blockImg.FontFamily = new FontFamily("Segoe MDL2 Assets");
            TextBlock block = new TextBlock();
            block.FontFamily = new FontFamily("PingFang SC");
            block.FontWeight = FontWeights.Bold;
            block.TextAlignment = TextAlignment.Center;
            block.VerticalAlignment = VerticalAlignment.Center;
            block.Text = text;
            block.FontSize = 12;
            block.Width = Math.Max(1, Math.Min(GetStringWidth(block) + 15, width - 40));
            block.TextTrimming = TextTrimming.CharacterEllipsis;

            StackPanel dockinner = new StackPanel();
            dockinner.Orientation = Orientation.Horizontal;
            dockinner.HorizontalAlignment = HorizontalAlignment.Center;
            dockinner.VerticalAlignment = VerticalAlignment.Center;
            if (src != null)
                dockinner.Children.Add(titleImg);
            else
                dockinner.Children.Add(blockImg);
            dockinner.Children.Add(block);
            dockinner.IsHitTestVisible = false;
            Grid inner = new Grid();
            inner.Children.Add(rect);
            inner.Children.Add(dockinner);
            border.Child = inner;

            container.Children.Add(border);
            RegisterName("window_" + id, container);
            RegisterName("border_" + id, border);
            RegisterName("rectWindow_" + id, rect);
            TabSymbolImages.Add(blockImg);
            TabImages.Add(titleImg);
            TabTexts.Add(block);
            rect.Name = "N" + id;
            rect.MouseLeftButtonDown += Tab_Click;
            rect.MouseRightButtonDown += Tab_Close;
            return container;
        }

        private void Tab_Close(object sender, MouseButtonEventArgs e)
        {
            int removeId = TabIndices.FindIndex((guid) => { if ((sender as Rectangle).Name == "N" + guid) return true; else return false; });
            TabContainer.Children.Remove(TabWindows[removeId]);
            UnregisterName("window_" + TabIndices[removeId]);
            UnregisterName("border_" + TabIndices[removeId]);
            UnregisterName("rectWindow_" + TabIndices[removeId]);
            if (TabPages[removeId] is Pages.IBrowserPage)
                (TabPages[removeId] as Pages.IBrowserPage).WindowDispose();
            TabWindows.RemoveAt(removeId);
            TabImages.RemoveAt(removeId);
            TabSymbolImages.RemoveAt(removeId);
            TabTexts.RemoveAt(removeId);
            TabIndices.RemoveAt(removeId);
            TabPages.RemoveAt(removeId);

            double totalWidths = ((ActualWidth - Margin.Left - Margin.Right - 60 - 300) / TabWindows.Count) - 2;
            for (int c = 0; c < TabWindows.Count; c++) {
                string i = TabIndices[c].ToString();
                Storyboard story = new Storyboard();
                story.AutoReverse = false;
                DoubleAnimation xAnimation = new DoubleAnimation();

                TabTexts[c].Width = Math.Max(1, Math.Min(GetStringWidth(TabTexts[c]), totalWidths - 40));
                var rect = (Rectangle)(FindName("rectWindow_" + i));
                xAnimation.From = rect.Width;
                xAnimation.To = totalWidths;
                xAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(150));

                DependencyProperty[] propertyChain = new DependencyProperty[]
                {
                    Rectangle.WidthProperty
                };

                story.Children.Add(xAnimation);
                Storyboard.SetTargetName(xAnimation, "rectWindow_" + i);
                Storyboard.SetTargetProperty(xAnimation, new PropertyPath("(0)", propertyChain));
                story.Begin(this);
            }
            if (TabWindows.Count == 0) {
                SelectedTabIndex = 0;
                Host.Children.Clear();
                GC.Collect();
            } else {
                if (removeId < SelectedTabIndex) {
                    SelectedTabIndex--;
                } else if (removeId > SelectedTabIndex) {
                } else {
                    SelectedTabIndex = Math.Max(0, SelectedTabIndex - 1);
                }
            }
        }

        private void Tab_Click(object sender, MouseButtonEventArgs e)
        {
            try {
                SelectedTabIndex = TabIndices.FindIndex((guid) => { if ((sender as Rectangle).Name == "N" + guid) return true; else return false; });
                Host.Children.Clear();
                Host.Children.Add(TabPages[SelectedTabIndex]);
                for (int i = 0; i < TabPages.Count; i++) {
                    if (i == SelectedTabIndex) {
                        if (TabPages[i] is Pages.IBrowserPage)
                            (TabPages[i] as Pages.IBrowserPage).Signal("refreshable", true);
                    } else {
                        if (TabPages[i] is Pages.IBrowserPage)
                            (TabPages[i] as Pages.IBrowserPage).Signal("refreshable", false);
                    }
                }
            } catch { SelectedTabIndex = 0; }
        }

        private void NewTab_Click(object sender, MouseButtonEventArgs e)
        {
            UserControl pg = new Pages.SourceEditor();
            TabPages.Add(pg);
            var id = Guid.NewGuid();
            TabIndices.Add(id.ToString().Replace("-", "_"));
            var newtab = CreateTabWindow(0, id.ToString().Replace("-", "_"), null, " Simula Workspace   ");
            TabContainer.Children.Add(newtab);
            TabWindows.Add(newtab);

            // the extra 2 represents the additional border thickness. 1 on each side.
            double totalWidths = ((ActualWidth - Margin.Left - Margin.Right - 60 - 300) / TabWindows.Count) - 2;
            for (int c = 0; c < TabWindows.Count; c++) {
                string i = TabIndices[c].ToString();
                Storyboard story = new Storyboard();
                story.AutoReverse = false;
                DoubleAnimation xAnimation = new DoubleAnimation();

                TabTexts[c].Width = Math.Max(1, Math.Min(GetStringWidth(TabTexts[c]), totalWidths - 40));
                var rect = (Rectangle)(FindName("rectWindow_" + i));
                xAnimation.From = rect.Width;
                xAnimation.To = totalWidths;
                xAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(150));

                DependencyProperty[] propertyChain = new DependencyProperty[]
                {
                    Rectangle.WidthProperty
                };

                story.Children.Add(xAnimation);
                Storyboard.SetTargetName(xAnimation, "rectWindow_" + i);
                Storyboard.SetTargetProperty(xAnimation, new PropertyPath("(0)", propertyChain));
                story.Begin(this);
            }
            SelectedTabIndex = TabWindows.Count - 1;
            Host.Children.Clear();
            Host.Children.Add(TabPages[SelectedTabIndex]);
        }

        private bool requireSizeChange = false;
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            requireSizeChange = true;
        }

        private void SizeChangeEnquiry_Tick(object sender, EventArgs e)
        {
            if (requireSizeChange) {
                double totalWidths = ((ActualWidth - Margin.Left - Margin.Right - 60 - 300) / TabWindows.Count) - 2;
                for (int c = 0; c < TabWindows.Count; c++) {
                    string i = TabIndices[c].ToString();
                    Storyboard story = new Storyboard();
                    story.AutoReverse = false;
                    DoubleAnimation xAnimation = new DoubleAnimation();

                    TabTexts[c].Width = Math.Max(1, Math.Min(GetStringWidth(TabTexts[c]), totalWidths - 40));
                    var rect = (Rectangle)(FindName("rectWindow_" + i));
                    xAnimation.From = rect.Width;
                    xAnimation.To = totalWidths;
                    xAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(150));

                    DependencyProperty[] propertyChain = new DependencyProperty[]
                    {
                    Rectangle.WidthProperty
                    };

                    story.Children.Add(xAnimation);
                    Storyboard.SetTargetName(xAnimation, "rectWindow_" + i);
                    Storyboard.SetTargetProperty(xAnimation, new PropertyPath("(0)", propertyChain));
                    story.Begin(this);
                }

                requireSizeChange = false;
            }
        }

        private void DisplayPopup(UserControl control, int width, int height, bool stayOpen = false, bool dialog = false)
        {
            if (!dialog) {
                popDialogGrid.Width = width + 40;
                popDialogGrid.Height = height + 20;
                popRectangle.Margin = new Thickness(20, 0, 20, 20);
                popRectangle.Width = width;
                popRectangle.Height = height;
                popDialogHost.Width = width + 40;
                popDialogHost.Height = height + 20;
                if (popDialogGrid.Children.Count > 1)
                    popDialogGrid.Children.RemoveRange(1, popDialogGrid.Children.Count - 1);
                popDialogGrid.Children.Add(control);

                if (WindowState != WindowState.Maximized)
                    popDialogHost.VerticalOffset = Top + Margin.Top + 32;
                else
                    popDialogHost.VerticalOffset = 32;

                if (WindowState != WindowState.Maximized)
                    popDialogHost.HorizontalOffset = Left + Margin.Left + (ActualWidth - width) / 2 - 20;
                else
                    popDialogHost.HorizontalOffset = (ActualWidth - width) / 2 - 20;
                popDialogHost.StaysOpen = stayOpen;
                popDialogHost.IsOpen = true;
                
            } else {
                Window window = new Window();
                window.Icon = new BitmapImage(new Uri("pack://siteoforigin:,,,/app.icon.ico"));
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                Grid grid = new Grid();
                control.Width = width; control.Height = height;
                grid.Width = width; grid.Height = height;
                grid.VerticalAlignment = VerticalAlignment.Top;
                grid.HorizontalAlignment = HorizontalAlignment.Center;
                grid.Children.Add(control);
                window.Content = grid;
                window.Width = width + 22;
                window.Height = height + 55;
                window.ResizeMode = ResizeMode.NoResize;
                window.ShowDialog();
            }
        }

        private void PopDialogHost_Opened(object sender, EventArgs e)
        {
            body.IsHitTestVisible = false;
        }

        private void PopDialogHost_Closed(object sender, EventArgs e)
        {
            body.IsHitTestVisible = true;
        }

        private void Menu_About(object sender, EventArgs e)
        {
            var startup = new Pages.Startup();
            startup.Width = 500;
            startup.Height = 500;
            DisplayPopup(startup, 500, 500, false);
        }

        private void CallSaveFileDialog(object sender, EventArgs e)
        {
            var startup = new Pages.SaveConfirmationDialog();
            startup.Width = 450;
            startup.Height = 183;
            DisplayPopup(startup, 450, 183, true);
        }

        private void Menu_ManageExtensions(object sender, EventArgs e)
        {
            new Scripting.Packaging.PackageExplorer().ShowDialog();
        }

        private void Menu_CheckUpdate(object sender, EventArgs e)
        {
            MainGrid.Visibility = Visibility.Hidden;
            InstallMainGrid.Visibility = Visibility.Visible;
        }

        private void Installer_Cancel(object sender, EventArgs e)
        {
            InstallMainGrid.Visibility = Visibility.Hidden;
            MainGrid.Visibility = Visibility.Visible;
        }

        
        // A BreadcrumbItem needs to populate it's Items. This can be due to the fact that a new BreadcrumbItem is selected, and thus
        // it's Items must be populated to determine whether this BreadcrumbItem show a dropdown button,
        // or when the Path property of the BreadcrumbBar is changed and therefore the Breadcrumbs must be populated from the new path.
      
        private void BreadcrumbBar_PopulateItems(object sender, Odyssey.Controls.BreadcrumbItemEventArgs e)
        {
            BreadcrumbItem item = e.Item;
            if (item.Items.Count == 0) {
                PopulateFolders(item);
                e.Handled = true;
            }
        }

        // Populate the Items of the specified BreadcrumbItem with the sub folders if necassary.

        private static void PopulateFolders(BreadcrumbItem item)
        {
            BreadcrumbBar bar = item.BreadcrumbBar;
            string path = bar.PathFromBreadcrumbItem(item);
            string trace = item.TraceValue;
            if (trace.Equals("工作区资源管理器")) {
                string[] dirs = System.IO.Directory.GetLogicalDrives();
                foreach (string s in dirs) {
                    string dir = s;
                    if (s.EndsWith(bar.SeparatorString)) dir = s.Remove(s.Length - bar.SeparatorString.Length, bar.SeparatorString.Length);
                    FolderItem fi = new FolderItem();
                    fi.Folder = dir;

                    item.Items.Add(fi);
                }
            } else {
                try {
                    string[] paths = System.IO.Directory.GetDirectories(path + "\\");
                    foreach (string s in paths) {
                        DirectoryInfo info = new DirectoryInfo(s);
                        if (!info.Attributes.HasFlag(FileAttributes.Hidden)) {
                            string file = System.IO.Path.GetFileName(s);
                            FolderItem fi = new FolderItem();
                            fi.Folder = file;
                            item.Items.Add(fi);
                        }
                    }
                } catch { }
            }
        }

        private void BreadcrumbBar_PathConversion(object sender, PathConversionEventArgs e)
        {
            if (e.Mode == PathConversionEventArgs.ConversionMode.DisplayToEdit) {
                if (e.DisplayPath.StartsWith(@"工作区资源管理器\", StringComparison.OrdinalIgnoreCase)) {
                    e.EditPath = e.DisplayPath.Remove(0, 9);
                } 
            } else {
                if (e.EditPath.StartsWith("c:", StringComparison.OrdinalIgnoreCase)) {
                    e.DisplayPath = @"Desktop\Computer\" + e.EditPath;
                } else if (e.EditPath.StartsWith(@"\\")) {
                    e.DisplayPath = @"Desktop\Network\" + e.EditPath.Remove(0, 2).Replace('/', '\\');
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation da = new DoubleAnimation(100, new Duration(new TimeSpan(0, 0, 2)));
            da.FillBehavior = FillBehavior.Stop;
            bar.BeginAnimation(BreadcrumbBar.ProgressValueProperty, da);
        }

        // The dropdown menu of a BreadcrumbItem was pressed, so delete the current folders, and repopulate the folders
        // to ensure actual data.

        private void bar_BreadcrumbItemDropDownOpened(object sender, BreadcrumbItemEventArgs e)
        {
            BreadcrumbItem item = e.Item;

            // only repopulate, if the BreadcrumbItem is dynamically generated which means, item.Data is a  pointer to itself:
            if (!(item.Data is BreadcrumbItem)) {
                item.Items.Clear();
                PopulateFolders(item);
            }
        }

        public static event EventHandler DialogCloseCallback;
        public static void InvokeDialogCloseCallback()
        {
            DialogCloseCallback?.Invoke(null, null);
        }
    }
}
