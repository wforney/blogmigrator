namespace BlogMigrator
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for RewriteSourcePosts.xaml
    /// </summary>
    public partial class RewriteSourcePosts : Window
    {
        /// <summary>
        /// The log collection
        /// </summary>
        public ObservableCollection<LogData> LogCollection = new ObservableCollection<LogData>();

        private readonly BackgroundWorker rewriteWorker = new BackgroundWorker();

        /// <summary>
        /// Initializes a new instance of the <see cref="RewriteSourcePosts"/> class.
        /// </summary>
        public RewriteSourcePosts()
        {
            this.InitializeComponent();

            this.rewriteWorker.WorkerReportsProgress = true;
            this.rewriteWorker.WorkerSupportsCancellation = true;
            this.rewriteWorker.DoWork += this.RewriteWorkerDoWork;
            this.rewriteWorker.ProgressChanged += this.RewriteWorkerProgressChanged;
            this.rewriteWorker.RunWorkerCompleted += this.RewriteWorkerRunWorkerCompleted;
        }

        /// <summary>
        /// Updates the status TextBox.
        /// </summary>
        /// <param name="Message">The message to add.</param>
        /// ///
        /// <history>Sean Patterson 11/6/2010 [Created]</history>
        public void UpdateStatusText(string message)
        {
            var ProgressText = new StringBuilder();
            ProgressText.AppendLine(this.txtStatus.Text);
            ProgressText.AppendLine(message);
            this.txtStatus.Text = ProgressText.ToString();
            this.txtStatus.ScrollToLine(this.txtStatus.LineCount - 1);
        }

        /// <summary>
        /// Handles the Click event of the btnClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CloseButtonClick(object sender, RoutedEventArgs e) => this.Close();

        /// <summary>
        /// Handles the Click event of the btnLoadLog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void LoadLogButtonClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists(this.txtRewriteFile.Text))
            {
                this.LogCollection.Clear();

                using (var sr = new StreamReader(this.txtRewriteFile.Text))
                {
                    string line;
                    // Read and display lines from the file until the end of the file is reached.
                    var firstLine = true;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!firstLine)
                        {
                            var logValues = line.Split(char.Parse(","));
                            var logItem = new LogData
                              (Convert.ToInt32(logValues[0]), logValues[1],
                               Convert.ToInt32(logValues[2]), logValues[3]);
                            this.LogCollection.Add(logItem);
                        }
                        else
                        {
                            firstLine = false;
                        }
                    }
                }

                this.lblEntriesCount.Content = $"[{this.LogCollection.Count} Total]";
                this.lsvLogEntries.ItemsSource = this.LogCollection;

                if (this.LogCollection.Count > 0)
                {
                    this.btnRewrite.IsEnabled = true;
                    this.btnSelectAllEntries.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRewrite control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void RewriteButtonClick(object sender, RoutedEventArgs e)
        {
            var myArgs = new WorkerArgs();

            if (this.cmbRewriteService.SelectedIndex > -1)
            {
                var itemDest = (ComboBoxItem)this.cmbRewriteService.SelectedItem;

                switch (itemDest.Tag.ToString())
                {
                    case "SS":
                        App.rewriteBlog.serviceUrl = "http://www.squarespace.com/process/service/PostInterceptor";
                        break;

                    case "WP":
                        App.rewriteBlog.serviceUrl = $"{this.txtRewriteBlogUrl.Text}/xmlrpc.php";
                        break;

                    case "ASPNet":
                        App.rewriteBlog.serviceUrl = "http://weblogs.asp.net/metablog.ashx";
                        break;

                    case "OTHER":
                        App.rewriteBlog.serviceUrl = this.txtRewriteServiceUrl.Text;
                        break;
                }

                App.rewriteBlog.blogId = this.txtRewriteBlogId.Text;
                App.rewriteBlog.rootUrl = this.txtRewriteBlogUrl.Text;
                App.rewriteBlog.username = this.txtRewriteUser.Text;
                App.rewriteBlog.password = this.txtRewritePassword.Text;
                App.rewriteBlog.blogFile = this.txtRewriteFile.Text;

                App.itemsToRewrite.Clear();
                foreach (LogData logItem in this.lsvLogEntries.SelectedItems)
                {
                    App.itemsToRewrite.Add(logItem);
                }

                App.rewriteMessage = this.txtUpdateSource.Text;

                myArgs.processToRun = "rewrite";
                myArgs.status = "Starting rewrite process...";
                this.rewriteWorker.RunWorkerAsync(myArgs);
            }
            else
            {
                MessageBox.Show("Please specify a service type.",
                                "Rewrite Sources Error.",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRewriteFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void RewriteFileButtonClick(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".csv",
                Filter = "Import Log Files (.csv)|*.csv|All Files|*.*"
            };

            var result = dlg.ShowDialog();

            if (result == true)
            {
                this.txtRewriteFile.Text = dlg.FileName;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRewriteHelp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void RewriteHelpButtonClick(object sender, RoutedEventArgs e) => new ConnectionHelpWindow().Show();

        /// <summary>
        /// Selects all entries in the ListView.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SelectAllEntriesButtonClick(object sender, RoutedEventArgs e) => this.lsvLogEntries.SelectAll();

        /// <summary>
        /// Handles the Click event of the btnTestRewrite control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TestRewriteButtonClick(object sender, RoutedEventArgs e)
        {
            string status;
            if (this.cmbRewriteService.SelectedIndex > -1)
            {
                var itemDest = (ComboBoxItem)this.cmbRewriteService.SelectedItem;

                var serviceUrl = string.Empty;
                switch (itemDest.Tag.ToString())
                {
                    case "SS":
                        serviceUrl = "http://www.squarespace.com/process/service/PostInterceptor";
                        break;

                    case "WP":
                        serviceUrl = this.txtRewriteBlogUrl.Text + "/xmlrpc.php";
                        break;

                    case "ASPNet":
                        serviceUrl = "http://weblogs.asp.net/metablog.ashx";
                        break;

                    case "OTHER":
                        serviceUrl = this.txtRewriteServiceUrl.Text;
                        break;
                }

                status = new Services().CheckServerStatus(
                    serviceUrl, this.txtRewriteBlogId.Text, this.txtRewriteUser.Text, this.txtRewritePassword.Text);
            }
            else
            {
                status = "Connection failed. No service type specified.";
            }

            MessageBox.Show(
                status,
                "Test Destination Connection Result",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
