namespace BlogMigrator
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for ConnectionHelp.xaml
    /// </summary>
    public partial class ConnectionHelpWindow : Window
    {
        public ConnectionHelpWindow() => this.InitializeComponent();

        /// <summary>
        /// Handles the btnClose_Click event.
        /// </summary>
        /// <param name="sender">Close button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>Sean Patterson 11/5/2010 [Created]</history>
        private void CloseButtonClick(object sender, RoutedEventArgs e) => this.Close();
    }
}
