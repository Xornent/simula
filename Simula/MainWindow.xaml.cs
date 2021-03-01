using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Simula
{
    public partial class MainWindow : Window
    {
        #region Windows Behavior

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

            this.panelGit.Visibility = Visibility.Hidden;
            this.panelInitGit.Visibility = Visibility.Visible;
            this.Margin = new Thickness();

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
                Margin = new Thickness(7, 7, 7, 7);
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
                MainGrid.Margin = new Thickness(7, 7, 7, 7);
                InstallMainGrid.Margin = new Thickness(7, 7, 7, 7);
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
        private DispatcherFrame dispatcherFrame;
        private FrameworkElement container = null;
        private void DisplayPopup(UserControl control, int width, int height, bool stayOpen = false, bool dialog = false)
        {
            if (!dialog) {
                container = this;
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

                if (stayOpen) {

                }
                
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

        // acknowledgement:
        // [1]: https://www.cnblogs.com/therock/articles/2314460.html

        private void PopDialogHost_Opened(object sender, EventArgs e)
        {
            this.MainGrid.IsHitTestVisible = false;
            // this.GlobalBlur.Radius = 0;

            try {
                ComponentDispatcher.PushModal();
                dispatcherFrame = new DispatcherFrame(true);
                Dispatcher.PushFrame(dispatcherFrame);
            } finally {
                ComponentDispatcher.PopModal();
            }
        }

        private void PopDialogHost_Closed(object sender, EventArgs e)
        {
            if (dispatcherFrame != null) {
                dispatcherFrame.Continue = false;
            }

            ComponentDispatcher.PopModal();

            this.MainGrid.IsHitTestVisible = true;
            // this.GlobalBlur.Radius = 0;
        }

        #endregion

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
            MessageBox.Show("no model");
        }

        private void Menu_ManageExtensions(object sender, EventArgs e)
        {
            
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

        public static event EventHandler DialogCloseCallback;
        public static void InvokeDialogCloseCallback()
        {
            DialogCloseCallback?.Invoke(null, null);
        }

        private void RecursiveDelete(DirectoryInfo directory, bool throwException = false)
        {
            try {
                foreach (var item in directory.GetDirectories()) {
                    RecursiveDelete(item, true);
                    item.Delete();
                }

                foreach (var item in directory.GetFiles()) {
                    item.Delete();
                }
            } catch (Exception ex) {
                if (throwException) throw ex;
                else this.DisplayPopup(new Pages.DeletePermissionBlockDialog(), 450, 183, true);
            }
        }

        //                               Users
        // ---------------------------------------------------------------------
        // users in simula is a role to operate and categorize accessibilities
        // it is also a git authenticator.

        private bool isLoggedin = false;
        private string userFriendlyName = "";
        private string userPassword = "";

        private bool userGitLoggedin = false;
        private string userGitAccount = "";
        private string userGitPassword = "";

        private void Event_User(object sender, EventArgs e)
        {
            if(isLoggedin) {
                DisplayPopup(new Pages.UserDialog(this.userFriendlyName, this.userGitAccount), 450, 163, true);
                return;
            }

            Pages.UserRegisterDialog reg = new Pages.UserRegisterDialog();
            DisplayPopup(reg, 450, 353, true);
            this.isLoggedin = true;
            this.userFriendlyName = reg.FriendlyName;
            this.userPassword = reg.Password;
        }

        private void LogIn()
        {
            if (isLoggedin) return;

            Pages.UserRegisterDialog reg = new Pages.UserRegisterDialog();
            DisplayPopup(reg, 450, 353, true);
            this.isLoggedin = true;
            this.userFriendlyName = reg.FriendlyName;
            this.userPassword = reg.Password;
        }

        //                             Workspaces
        // ---------------------------------------------------------------------
        // the following methods invoke the workspace creation, load from directory
        // and git operations in the workspace explorer and git management panels.

        private string currentWorkspaceLoc = "";
        private DirectoryInfo currentDirectory;
        private bool gitAvailable = false;
        private Scripting.Git.Repository gitRepository;
        private bool hasWorkspace { get { return !string.IsNullOrEmpty(currentWorkspaceLoc); } }

        private void Event_OpenWorkspace(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            if (hasWorkspace) folderBrowser.SelectedPath = currentWorkspaceLoc;

            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                this.currentWorkspaceLoc = folderBrowser.SelectedPath;
                this.currentDirectory = new DirectoryInfo(this.currentWorkspaceLoc);
                this.lblWorkspace.Text = currentDirectory.Name;

                this.btnInitRemote.IsEnabled = true;
                this.btnInitRepo.IsEnabled = true;

                FileSystemWatcher watcher = new FileSystemWatcher(this.currentWorkspaceLoc);
                watcher.EnableRaisingEvents = true;
                watcher.Created += Event_FileSystemChanged;
                watcher.Deleted += Event_FileSystemChanged;
                watcher.Renamed += Event_FileSystemChanged;
                watcher.Changed += Event_FileSystemChanged;
                // trigger the directory changing events.
                this.CheckDirectoryGit();
            }
        }

        private void Event_FileSystemChanged(object sender, FileSystemEventArgs e)
        {
            if (!hasWorkspace) return;
            if (!gitAvailable) return;

            Dispatcher.Invoke(CheckDirectoryGit);
        }

        private void CheckDirectoryGit()
        {
            if (!hasWorkspace) return;

            if (Directory.Exists(currentWorkspaceLoc + @"\.git"))
                gitAvailable = true;

            if (gitAvailable) {
                try {
                    this.gitRepository = new Scripting.Git.Repository(currentWorkspaceLoc + @"\.git");
                    this.panelInitGit.Visibility = Visibility.Hidden;
                    this.panelGit.Visibility = Visibility.Visible;

                    this.treeItemGitBranch.Items.Clear();
                    foreach (var item in this.gitRepository.Branches) {
                        this.treeItemGitBranch.Items.Add(new UI.IconTreeNode(item.FriendlyName + ((item.IsRemote) ? " 远程分支" : ""), "resources/icons/branch-circle.png"));
                    }

                    this.treeItemGitLocal.Items.Clear();
                    foreach (var item in this.gitRepository.Commits) {
                        this.treeItemGitLocal.Items.Add(new UI.IconTreeNode(item.MessageShort, "resources/icons/clock-check.png"));
                    }

                    this.treeItemGitTag.Items.Clear();
                    foreach (var item in this.gitRepository.Tags) {
                        this.treeItemGitTag.Items.Add(new UI.IconTreeNode(item.FriendlyName, "resources/icons/label.png"));
                    }

                    UpdateGitChanges();

                } catch  {

                    this.gitAvailable = false;
                    this.panelInitGit.Visibility = Visibility.Visible;
                    this.panelGit.Visibility = Visibility.Hidden;
                    this.DisplayPopup(new Pages.InvalidGitConfirmationDialog(), 450, 183, true);
                }
            }
        }

        private void Event_InitGitFromDirectory(object sender, EventArgs e)
        {
            if (gitAvailable) return;
            if (!hasWorkspace) return;
            if (Directory.Exists(currentWorkspaceLoc + @"\.git"))
                this.RecursiveDelete(new DirectoryInfo(currentWorkspaceLoc + @"\.git"));

            string root = Scripting.Git.Repository.Init(currentWorkspaceLoc);
            gitAvailable = true;
            CheckDirectoryGit();
        }

        private void Event_InitGitFromRemote(object sender, EventArgs e)
        {
            if (gitAvailable) return;
            if (!hasWorkspace) return;

            GitLogIn();

            Pages.GitCloneRepositoryDialog cloneDlg = new Pages.GitCloneRepositoryDialog();
            this.DisplayPopup(cloneDlg, 450, 183, true);
            Scripting.Git.CloneOptions options = new Scripting.Git.CloneOptions();
            options.CredentialsProvider = (_url, _user, _cred) => new Scripting.Git.UsernamePasswordCredentials 
            {
                Username = this.userGitAccount, 
                Password = this.userGitPassword
            };

            options.OnProgress += GitTransferProgress;

            string root = Scripting.Git.Repository.Clone(cloneDlg.Repository, currentWorkspaceLoc, options);
            gitAvailable = true;
            CheckDirectoryGit();
        }

        private bool GitTransferProgress(string server)
        {
            return true;
        }

        private void GitLogIn()
        {
            if (!userGitLoggedin) {
                Pages.UserAuthenticationDialog auth = new Pages.UserAuthenticationDialog();
                this.DisplayPopup(auth, 450, 283, true);
                this.userGitAccount = auth.Account;
                this.userGitPassword = auth.Password;
            }
        }

        private void UpdateGitChanges()
        {
            var options = new Scripting.Git.StatusOptions();
            var status = this.gitRepository.RetrieveStatus(options);
            this.treeItemChanges.Items.Clear();
            this.treeItemStacked.Items.Clear();

            foreach (var item in status) {
                string[] paths = item.FilePath.Replace("\\", "/").Replace("//", "/").Split('/');

                switch (item.State) {
                    case Scripting.Git.FileStatus.Nonexistent:
                        break;
                    case Scripting.Git.FileStatus.Unaltered:
                        break;
                    case Scripting.Git.FileStatus.NewInIndex:
                        AddToTreeItemRecursive(this.treeItemStacked, paths.ToList(), "resources/icons/data-add.png", item.FilePath);
                        break;
                    case Scripting.Git.FileStatus.ModifiedInIndex:
                        AddToTreeItemRecursive(this.treeItemStacked, paths.ToList(), "resources/icons/data-refresh.png", item.FilePath);
                        break;
                    case Scripting.Git.FileStatus.DeletedFromIndex:
                        AddToTreeItemRecursive(this.treeItemStacked, paths.ToList(), "resources/icons/data-remove.png", item.FilePath);
                        break;
                    case Scripting.Git.FileStatus.RenamedInIndex:
                        AddToTreeItemRecursive(this.treeItemStacked, paths.ToList(), "resources/icons/project-name-edit.png", item.FilePath);
                        break;
                    case Scripting.Git.FileStatus.TypeChangeInIndex:
                        AddToTreeItemRecursive(this.treeItemStacked, paths.ToList(), "resources/icons/project-name-edit.png", item.FilePath);
                        break;
                    case Scripting.Git.FileStatus.NewInWorkdir:
                        AddToTreeItemRecursive(this.treeItemChanges, paths.ToList(), "resources/icons/data-add.png", item.FilePath);
                        break;
                    case Scripting.Git.FileStatus.ModifiedInWorkdir:
                        AddToTreeItemRecursive(this.treeItemChanges, paths.ToList(), "resources/icons/data-refresh.png", item.FilePath);
                        break;
                    case Scripting.Git.FileStatus.DeletedFromWorkdir:
                        AddToTreeItemRecursive(this.treeItemChanges, paths.ToList(), "resources/icons/data-remove.png", item.FilePath);
                        break;
                    case Scripting.Git.FileStatus.TypeChangeInWorkdir:
                        AddToTreeItemRecursive(this.treeItemChanges, paths.ToList(), "resources/icons/project-name-edit.png", item.FilePath);
                        break;
                    case Scripting.Git.FileStatus.RenamedInWorkdir:
                        AddToTreeItemRecursive(this.treeItemChanges, paths.ToList(), "resources/icons/project-name-edit.png", item.FilePath);
                        break;
                    case Scripting.Git.FileStatus.Unreadable:
                        break;
                    case Scripting.Git.FileStatus.Ignored:
                        break;
                    case Scripting.Git.FileStatus.Conflicted:
                        AddToTreeItemRecursive(this.treeItemStacked, paths.ToList(), "resources/icons/data-unavailable.png", item.FilePath);
                        break;
                    default:
                        break;
                }
            }

            if (status.IsDirty) this.treeItemChanges.Header = "更改";
            else this.treeItemChanges.Header = "已全部暂存";
        }

        private void AddToTreeItemRecursive(TreeViewItem item, List<string> paths, string icon = "resources/icons/apple-files.png", string fullPath = "")
        {
            if(paths.Count == 1) {
                item.Items.Add(new UI.IconTreeNode(paths[0], icon));
                return;
            }

            foreach (TreeViewItem searches in item.Items) {
                if(searches.Header.ToString() == paths[0]) {
                    paths.RemoveAt(0);
                    AddToTreeItemRecursive(searches, paths, icon, fullPath);
                    return;
                }
            }

            var tree = new UI.IconTreeNode(paths[0], "resources/icons/apple-files.png");
            tree.Tag = fullPath;
            item.Items.Add(tree);
            paths.RemoveAt(0);
            AddToTreeItemRecursive(tree, paths, icon, fullPath);
        }

        private void Event_GitStashAll(object sender, EventArgs e)
        {
            if (!gitAvailable) return;

            Scripting.Git.StageOptions options = new Scripting.Git.StageOptions();
            Scripting.Git.Commands.Stage(this.gitRepository, "*");

            CheckDirectoryGit();
        }

        private void Event_GitCommit(object sender, EventArgs e)
        {
            if (!gitAvailable) return;
            LogIn();
            GitLogIn();

            Scripting.Git.CommitOptions options = new Scripting.Git.CommitOptions();
            Scripting.Git.Signature committer = new Scripting.Git.Signature(this.userFriendlyName, this.userGitAccount, DateTime.Now);
            this.gitRepository.Commit(this.txtGitCommitMsg.Text, committer, committer, options);

            CheckDirectoryGit();
        }

        private void Event_GitStashAllAndCommit(object sender, EventArgs e)
        {
            this.Event_GitStashAll(sender, e);
            this.Event_GitCommit(sender, e);
        }

        private void Event_GitRemoveUntracked(object sender, EventArgs e)
        {
            Pages.DeleteConfirmationDialog confirm = new Pages.DeleteConfirmationDialog();
            this.DisplayPopup(confirm, 450, 183, true);
            
            if (confirm.Result == Pages.DialogResult.Confirm)
                this.gitRepository.RemoveUntrackedFiles();
        }

        //                         Context Menu Builders
        // ---------------------------------------------------------------------
        // the following code build the context menus used in the application's 
        // contexts.

        private ContextMenu ctxMenuGitFileChange = new ContextMenu();
        private void InitializeComponentCtxGitFileChange()
        {
            ctxMenuGitFileChange = new ContextMenu();

            MenuItem stash = new MenuItem() { Header = "暂存此文件" };
            MenuItem disgardChange = new MenuItem() { Header = "忽略更改" };

            stash.Click += (sender, e) => {
                if(sender is UI.IconTreeNode node) {
                    Scripting.Git.Commands.Stage(this.gitRepository, node.Tag.ToString());
                    e.Handled = true;
                }
            };

            disgardChange.Click += (sender, e) => {
                if (sender is UI.IconTreeNode node) {

                }
            };

            ctxMenuGitFileChange.Items.Add(stash);
        }
    }
}
