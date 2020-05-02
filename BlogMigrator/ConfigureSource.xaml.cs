namespace BlogMigrator
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for ConfigureSource.xaml
    /// </summary>
    public partial class ConfigureSource : Window
    {
        public ConfigureSource() => this.InitializeComponent();

        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <param name="sender">Close button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>Sean Patterson [11/11/2010] Created</history>
        private void CloseButtonClick(object sender, RoutedEventArgs e) => this.Close();

        /// <summary>
        /// Saves the source connection details into the application variable.
        /// </summary>
        /// <param name="sender">Save button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>Sean Patterson [11/11/2010] Created</history>
        private void SaveSourceButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.cmbSourceService.SelectedIndex > -1)
            {
                var itemSource = (ComboBoxItem)this.cmbSourceService.SelectedItem;

                App.sourceBlog = new BlogSource
                {
                    serviceType = itemSource.Tag.ToString()
                };

                switch (itemSource.Tag.ToString())
                {
                    case "SS":
                        App.sourceBlog.serviceUrl = "http://www.squarespace.com/process/service/PostInterceptor";
                        break;

                    case "WP":
                        App.sourceBlog.serviceUrl = $"{this.txtSourceBlogUrl.Text}/xmlrpc.php";
                        break;

                    case "ASPNet":
                        App.sourceBlog.serviceUrl = "http://weblogs.asp.net/metablog.ashx";
                        break;

                    case "OTHER":
                        App.sourceBlog.serviceUrl = this.txtSourceServiceUrl.Text;
                        break;

                    case "FILE":
                        App.sourceBlog.serviceUrl = this.txtSourceServiceUrl.Text;
                        break;
                }

                App.sourceBlog.rootUrl = this.txtSourceBlogUrl.Text;
                App.sourceBlog.blogId = this.txtSourceBlogId.Text;
                App.sourceBlog.username = this.txtSourceUser.Text;
                App.sourceBlog.password = this.txtSourcePassword.Text;
                App.sourceBlog.blogFile = this.txtSourceFile.Text;

                ((MainWindow)this.Owner).btnConfigureSource.Content = itemSource.Tag.ToString() != "FILE"
                    ? $"Configure Source Blog{Environment.NewLine}{Environment.NewLine}{itemSource.Tag} - {this.txtSourceBlogUrl.Text}"
                    : $"Configure Source Blog{Environment.NewLine}{Environment.NewLine}{itemSource.Tag} - {this.txtSourceFile.Text}";

                MessageBox.Show(
                    "Source Configuration Saved.",
                    "Configuration Saved.",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    "Please specify a service type.",
                    "Configuration Not Saved.",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Displays a file dialog window to input source file.
        /// </summary>
        /// <param name="sender">Test Source button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>Sean Patterson [11/11/2010] Created</history>
        private void SourceFileButtonClick(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".xml",
                Filter = "BlogML Files (.xml)|*.xml|All Files|*.*"
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
                this.txtSourceFile.Text = dlg.FileName;
            }
        }

        /// <summary>
        /// Displays the connection help window.
        /// </summary>
        /// <param name="sender">Source Help button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>Sean Patterson [11/11/2010] Created</history>
        private void SourceHelpButtonClick(object sender, RoutedEventArgs e) => new ConnectionHelpWindow().Show();

        /// <summary>
        /// Tests the source connection by attempting to retrieve a post.
        /// </summary>
        /// <param name="sender">Test Source button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>Sean Patterson [11/11/2010] Created</history>
        private void TestSourceButtonClick(object sender, RoutedEventArgs e)
        {
            string status;
            if (this.cmbSourceService.SelectedIndex > -1)
            {
                var itemSource = (ComboBoxItem)this.cmbSourceService.SelectedItem;

                var serviceUrl = string.Empty;
                switch (itemSource.Tag.ToString())
                {
                    case "SS":
                        serviceUrl = "http://www.squarespace.com/process/service/PostInterceptor";
                        break;

                    case "WP":
                        serviceUrl = $"{this.txtSourceBlogUrl.Text}/xmlrpc.php";
                        break;

                    case "ASPNet":
                        serviceUrl = "http://weblogs.asp.net/metablog.ashx";
                        break;

                    case "OTHER":
                        serviceUrl = this.txtSourceServiceUrl.Text;
                        break;
                }

                if (itemSource.Tag.ToString() == "FILE")
                {
                    status = File.Exists(this.txtSourceFile.Text)
                        ? "Connection successful."
                        : "Connection failed. File not found.";
                }
                else
                {
                    status = new Services().CheckServerStatus(
                        serviceUrl,
                        this.txtSourceBlogId.Text,
                        this.txtSourceUser.Text,
                        this.txtSourcePassword.Text);
                }
            }
            else
            {
                status = "Connection failed. No service type specified.";
            }

            MessageBox.Show(
                status,
                "Test Source Connection Result",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
