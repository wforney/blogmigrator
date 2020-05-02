namespace BlogMigrator
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow() => this.InitializeComponent();

        /// <summary>
        /// Handles the btnClose click event.
        /// </summary>
        /// <param name="sender">Close button click event.</param>
        /// <param name="e">Button click event arguments.</param>
        /// <history>Sean Patterson 11/5/2010 [Created]</history>
        private void CloseButtonClick(object sender, RoutedEventArgs e) => this.Close();
    }
}
