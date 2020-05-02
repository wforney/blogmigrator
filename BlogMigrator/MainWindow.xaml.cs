namespace BlogMigrator
{
    using CookComputing.XmlRpc;

    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The post collection
        /// </summary>
        public ObservableCollection<PostData> PostCollection = new ObservableCollection<PostData>();

        private readonly BackgroundWorker allPostsWorker = new BackgroundWorker();
        private readonly BackgroundWorker migrationWorker = new BackgroundWorker();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow" /> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            this.allPostsWorker.WorkerReportsProgress = true;
            this.allPostsWorker.WorkerSupportsCancellation = true;
            this.allPostsWorker.DoWork += this.AllPostsWorkerDoWork;
            this.allPostsWorker.ProgressChanged += this.AllPostsWorkerProgressChanged;
            this.allPostsWorker.RunWorkerCompleted += this.AllPostsWorkerRunWorkerCompleted;

            this.migrationWorker.WorkerReportsProgress = true;
            this.migrationWorker.WorkerSupportsCancellation = true;
            this.migrationWorker.DoWork += this.MigrationWorkerDoWork;
            this.migrationWorker.ProgressChanged += this.MigrationWorkerProgressChanged;
            this.migrationWorker.RunWorkerCompleted += this.MigrationWorkerRunWorkerCompleted;
        }

        /// <summary>
        /// Updates the status bar.
        /// </summary>
        /// <param name="Message">The message to add.</param>
        /// ///
        /// <history>Sean Patterson 11/6/2010 [Created]</history>
        public void UpdateStatusBar(string message) => this.StatusBarMessage.Content = message;

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
        /// Displays the configure destination window.
        /// </summary>
        /// <param name="sender">Configure source button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>Sean Patterson [11/11/2010] Created</history>
        private void ConfigureDestinationButtonClick(object sender, RoutedEventArgs e) =>
            new ConfigureDestination
            {
                Owner = this
            }.ShowDialog();

        /// <summary>
        /// Displays the configure source window.
        /// </summary>
        /// <param name="sender">Configure source button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>Sean Patterson [11/11/2010] Created</history>
        private void ConfigureSourceButtonClick(object sender, RoutedEventArgs e) =>
            new ConfigureSource
            {
                Owner = this
            }.ShowDialog();

        /// <summary>
        /// Exits the application.
        /// </summary>
        /// <param name="sender">Quit menu click event.</param>
        /// <param name="e">Click event arguments.</param>
        /// <history>Sean Patterson 11/4/2010 [Created]</history>
        private void FileQuitMenuClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        /// <summary>
        /// Handles btnGetAllPosts click event.
        /// </summary>
        /// <param name="sender">Get All Posts button click event.</param>
        /// <param name="e">Click event arguments.</param>
        /// <history>Sean Patterson 11/4/2010 [Created]</history>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private void GetAllPostsButtonClick(object sender, RoutedEventArgs e)
        {
            var myArgs = new WorkerArgs
            {
                processToRun = "getallposts",
                status = "Get all posts action selected."
            };

            try
            {
                this.allPostsWorker.RunWorkerAsync(myArgs);
            }
            catch (XmlRpcFaultException fex)
            {
                // Flush out old records to prevent accidental writes.
                if (App.sourceBlog.blogPosts.Count > 0)
                {
                    App.sourceBlog.blogPosts.Clear();
                }

                App.sourceBlog.blogData = null;

                if (this.PostCollection.Count > 0)
                {
                    this.PostCollection.Clear();
                }

                this.lblEntriesCount.Content = "[0 Total]";

                MessageBox.Show(
                    $"XML-RPC error migrating posts: {Environment.NewLine}{Environment.NewLine}{fex}Please check your settings and try again.",
                    "Migration Result",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // Flush out old records to prevent accidental writes.
                if (App.sourceBlog.blogPosts.Count > 0)
                {
                    App.sourceBlog.blogPosts.Clear();
                }

                App.sourceBlog.blogData = null;

                if (this.PostCollection.Count > 0)
                {
                    this.PostCollection.Clear();
                }

                this.lblEntriesCount.Content = "[0 Total]";

                MessageBox.Show(
                    $"General error migrating posts: {Environment.NewLine}{Environment.NewLine}{ex}Please check your settings and try again.",
                    "Migration Result",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Displays the about window.
        /// </summary>
        /// <param name="sender">About menu click event.</param>
        /// <param name="e">Click event arguments.</param>
        /// <history>Sean Patterson 11/4/2010 [Created]</history>
        private void HelpAboutMenuClick(object sender, RoutedEventArgs e) => new AboutWindow().ShowDialog();

        /// <summary>
        /// Migrates the posts from the source to destination server.
        /// </summary>
        /// <param name="sender">Migrate button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>Sean Patterson 11/4/2010 [Created]</history>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private void MigrateButtonClick(object sender, RoutedEventArgs e)
        {
            var myArgs = new WorkerArgs
            {
                processToRun = "migrateposts",
                status = "Migrate posts action selected."
            };

            try
            {
                App.sourceBlog.postsToMigrate.Clear();

                foreach (PostData item in this.lsvAllPosts.SelectedItems)
                {
                    App.sourceBlog.postsToMigrate.Add(item.postid);
                }

                if (this.chkUpdateSource.IsChecked == true)
                {
                    App.rewritePosts = true;
                    App.rewriteMessage = this.txtUpdateSource.Text;
                }
                else
                {
                    App.rewritePosts = false;
                    App.rewriteMessage = null;
                }
                App.BatchMode = this.IsBatch.IsChecked.Value;
                if (App.sourceBlog.postsToMigrate.Count > 0)
                {
                    this.migrationWorker.RunWorkerAsync(myArgs);
                }
                else
                {
                    MessageBox.Show(
                        "Please specify at least one post to migrate.",
                        "No Posts Specified.",
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                }
            }
            catch (XmlRpcFaultException fex)
            {
                MessageBox.Show(
                    $"XML-RPC error migrating posts: {Environment.NewLine}{Environment.NewLine}{fex}Please check your settings and try again.",
                    "Migration Result",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"General error migrating posts: {Environment.NewLine}{Environment.NewLine}{ex}Please check your settings and try again.",
                    "Migration Result",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Selects all posts in the ListView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAllPostsButtonClick(object sender, RoutedEventArgs e) => this.lsvAllPosts.SelectAll();

        /// <summary>
        /// Handles the Click event of the mnuToolsRewrite control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void ToolsRewriteMenuClick(object sender, RoutedEventArgs e) =>
                    new RewriteSourcePosts
                    {
                        Owner = this
                    }.ShowDialog();
    }
}
