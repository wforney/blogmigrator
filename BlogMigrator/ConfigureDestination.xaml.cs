namespace BlogMigrator
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for ConfigureDestination.xaml
    /// </summary>
    public partial class ConfigureDestination : Window
    {
        public ConfigureDestination() => this.InitializeComponent();

        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <param name="sender">Close button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>Sean Patterson [11/11/2010] Created</history>
        private void CloseButtonClick(object sender, RoutedEventArgs e) => this.Close();

        /// <summary>
        /// Displays the connection help window.
        /// </summary>
        /// <param name="sender">Destination Help button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>Sean Patterson [11/11/2010] Created</history>
        private void DestHelpButtonClick(object sender, RoutedEventArgs e) => new ConnectionHelpWindow().Show();

        /// <summary>
        /// Displays a file dialog window to input source file.
        /// </summary>
        /// <param name="sender">Test Destination button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>Sean Patterson [11/11/2010] Created</history>
        private void DestinationFileButtonClick(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".xml",
                Filter = "WXR Files (.xml)|*.xml|All Files|*.*"
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
                this.txtDestinationFile.Text = dlg.FileName;
            }
        }

        /// <summary>
        /// Inserts a sample post in the destination server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private void InsertSampleButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.cmbDestinationService.SelectedIndex > -1)
            {
                try
                {
                    var serviceUrl = string.Empty;
                    var itemDest = (ComboBoxItem)this.cmbDestinationService.SelectedItem;
                    switch (itemDest.Tag.ToString())
                    {
                        case "SS":
                            serviceUrl = "http://www.squarespace.com/process/service/PostInterceptor";
                            break;

                        case "WP":
                            serviceUrl = $"{this.txtDestinationBlogUrl.Text}/xmlrpc.php";
                            break;

                        case "ASPNet":
                            serviceUrl = "http://weblogs.asp.net/metablog.ashx";
                            break;

                        case "OTHER":
                            serviceUrl = this.txtDestinationServiceUrl.Text;
                            break;
                    }

                    string status;
                    if (itemDest.Tag.ToString() == "FILE")
                    {
                        status = "No test post. Destination is a file.";
                    }
                    else
                    {
                        var myService = new Services();
                        status = myService.InsertSamplePost
                                           (serviceUrl, this.txtDestinationBlogId.Text,
                                            this.txtDestinationUser.Text, this.txtDestinationPassword.Text);
                    }

                    MessageBox.Show(
                        status,
                        "Add Sample Post Result",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error inserting test post:{Environment.NewLine}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}Please check your settings and try again.",
                        "Error adding sample post.",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(
                    "Error inserting test post. No source type specified.",
                    "Error adding sample post.",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Saves the destination connection details into the application variable.
        /// </summary>
        /// <param name="sender">Save button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>Sean Patterson [11/11/2010] Created</history>
        private void SaveSourceButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.cmbDestinationService.SelectedIndex > -1)
            {
                var itemDest = (ComboBoxItem)this.cmbDestinationService.SelectedItem;

                App.destBlog = new BlogSource
                {
                    serviceType = itemDest.Tag.ToString()
                };

                switch (itemDest.Tag.ToString())
                {
                    case "SS":
                        App.destBlog.serviceUrl = "http://www.squarespace.com/process/service/PostInterceptor";
                        break;

                    case "WP":
                        App.destBlog.serviceUrl = $"{this.txtDestinationBlogUrl.Text}/xmlrpc.php";
                        break;

                    case "ASPNet":
                        App.destBlog.serviceUrl = "http://weblogs.asp.net/metablog.ashx";
                        break;

                    case "OTHER":
                        App.destBlog.serviceUrl = this.txtDestinationServiceUrl.Text;
                        break;

                    case "FILE":
                        App.destBlog.serviceUrl = this.txtDestinationServiceUrl.Text;
                        break;
                }

                App.destBlog.rootUrl = this.txtDestinationBlogUrl.Text;
                App.destBlog.blogId = this.txtDestinationBlogId.Text;
                App.destBlog.username = this.txtDestinationUser.Text;
                App.destBlog.password = this.txtDestinationPassword.Text;
                App.destBlog.blogFile = this.txtDestinationFile.Text;

                ((MainWindow)this.Owner).btnConfigureSource.Content = itemDest.Tag.ToString() == "FILE"
                    ? $"Configure Source Blog{Environment.NewLine}{Environment.NewLine}{itemDest.Tag} - {this.txtDestinationFile.Text}"
                    : $"Configure Source Blog{Environment.NewLine}{Environment.NewLine}{itemDest.Tag} - {this.txtDestinationBlogUrl.Text}";
                MessageBox.Show(
                    "Destination Configuration Saved.",
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
        /// Tests the destination connection by attempting to retrieve a post.
        /// </summary>
        /// <param name="sender">Test Source button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>Sean Patterson [11/11/2010] Created</history>
        private void TestDestinationButtonClick(object sender, RoutedEventArgs e)
        {
            string status;
            if (this.cmbDestinationService.SelectedIndex > -1)
            {
                var itemDest = (ComboBoxItem)this.cmbDestinationService.SelectedItem;

                var serviceUrl = string.Empty;
                switch (itemDest.Tag.ToString())
                {
                    case "SS":
                        serviceUrl = "http://www.squarespace.com/process/service/PostInterceptor";
                        break;

                    case "WP":
                        serviceUrl = $"{this.txtDestinationBlogUrl.Text}/xmlrpc.php";
                        break;

                    case "ASPNet":
                        serviceUrl = "http://weblogs.asp.net/metablog.ashx";
                        break;

                    case "OTHER":
                        serviceUrl = this.txtDestinationServiceUrl.Text;
                        break;
                }

                if (itemDest.Tag.ToString() != "FILE")
                {
                    var myService = new Services();
                    status = myService.CheckServerStatus(
                        serviceUrl,
                        this.txtDestinationBlogId.Text,
                        this.txtDestinationUser.Text,
                        this.txtDestinationPassword.Text);
                }
                else
                {
                    // There is no need to test if the destination file exists, only that a location
                    // has been specified.
                    status = string.IsNullOrEmpty(this.txtDestinationFile.Text)
                        ? "Connection failed. No file specified."
                        : "Connection successful.";
                }
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
