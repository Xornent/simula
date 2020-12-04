using Simula.Scripting.Packaging.Spkg;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Simula.Scripting.Packaging
{

    public partial class PackageExplorer : Window
    {

        public PackageSearchResult SearchResult { get; set; } = new PackageSearchResult();
        public Package SelectedPackage { get; set; } = new Package();
        public Module SelectedModule { get; set; } = new Module();

        private static string Searcher = "Simula.Scripting";
        private static int Page = 1;
        private readonly System.ComponentModel.BackgroundWorker Worker = new System.ComponentModel.BackgroundWorker();

        public PackageExplorer()
        {
            InitializeComponent();
            Package.Initialize();
            DataContext = this;
            Worker.DoWork += HandleDownloadInformation;
            Worker.RunWorkerCompleted += HandleRunFinished;
        }

        private void HandleRunFinished(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            load.Visibility = Visibility.Hidden;
            content.Visibility = Visibility.Visible;

            packages.ItemsSource = SearchResult.Packages;
        }

        private void HandleDownloadInformation(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            SearchResult = Package.Search(Searcher, Page - 1, true);
        }

        private void HandleMouseDown(object sender, EventArgs e)
        {
            content.Visibility = Visibility.Hidden;
            load.Visibility = Visibility.Visible;
            Searcher = searchBox.Text;
            Page = 1;
            Worker.RunWorkerAsync();
        }

        private void HandlePackageSelection(object sender, MouseButtonEventArgs e)
        {
            string name = (sender as TreeViewItem).Header.ToString();
            int i = 0; int selected = -1;
            foreach (var item in SearchResult.Packages) {
                if (item.Name.ToLower().Trim() == name.ToLower().Trim()) {
                    selected = i;
                    break;
                }
                i++;
            }
            if (selected != -1)
                SelectedPackage = SearchResult.Packages[selected];
            modules.ItemsSource = SelectedPackage.Modules;
        }

        private void packages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string name = "";
            if (e.AddedItems != null)
                if (e.AddedItems.Count >= 1)
                    if (e.AddedItems[0] != null)
                        name = (e.AddedItems[0] as Package).Name;
            int i = 0; int selected = -1;
            foreach (var item in SearchResult.Packages) {
                if (item.Name.ToLower().Trim() == name.ToLower().Trim()) {
                    selected = i;
                    break;
                }
                i++;
            }
            if (selected != -1)
                SelectedPackage = SearchResult.Packages[selected];
            modules.ItemsSource = SelectedPackage.Modules;
        }

        private void modules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null)
                if (e.AddedItems.Count >= 1)
                    if (e.AddedItems[0] != null) {
                        var module = e.AddedItems[0] as Module;

                        _author.Text = module.Author;
                        _description.Text = module.Description;
                        if (!string.IsNullOrWhiteSpace(module.Summary))
                            _description.Text += ("\n\n" + module.Summary);
                        _lic.Text = module.License;
                        _licurl.Text = module.LicenseUrl;
                        _project.Text = module.ProjectUrl;
                        _id.Text = module.Name;

                        string deps = "";
                        foreach (var item in module.Dependency) {
                            deps += ("包 " + item.Id + " 版本 ");
                            if (item.Minimal.Major == 0 &&
                                item.Minimal.Minor == 0 &&
                                item.Minimal.Build == 0) { } else {
                                if (item.Maximum.Major == 0 &&
                                item.Maximum.Minor == 0 &&
                                item.Maximum.Build == 0) {
                                    deps += (item.Minimal.ToString() + " +");
                                } else {
                                    deps += (item.Minimal.ToString() + " - " + item.Maximum.ToString());
                                }
                            }
                            deps += "\n";
                        }
                        _dependency.Text = deps;
                        if (!string.IsNullOrWhiteSpace(module.IconUrl))
                            img.Source = new BitmapImage(new Uri(module.IconUrl));
                        else img.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/app.icon.ico"));
                    }
        }
    }
}
