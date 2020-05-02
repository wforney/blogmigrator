namespace BlogMigrator
{
    using System.ComponentModel;
    using System.Text;
    using System.Windows;

    public partial class RewriteSourcePosts : Window
    {
        /// <summary>
        /// Runs the rewrite process as a background thread.
        /// </summary>
        /// <param name="sender">Rewrite DoWork dvent.</param>
        /// <param name="e">DoWorkEvent arguments.</param>
        /// <history>Sean Patterson 11/16/2010 [Created]</history>
        private void RewriteWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var myArgs = (WorkerArgs)e.Argument;
            myArgs.status = "Starting rewrite process...";
            this.rewriteWorker.ReportProgress(0, myArgs);

            foreach (var logItem in App.itemsToRewrite)
            {
                var myService = new Services();
                var origPost = myService.GetPost(
                    App.rewriteBlog.serviceUrl,
                    logItem.sourceId,
                    App.rewriteBlog.username,
                    App.rewriteBlog.password);

                myArgs.status = $"Rewriting post: {origPost.title}";
                this.rewriteWorker.ReportProgress(15, myArgs);

                var updatePost = origPost;
                var newUrl = $"<a href='{logItem.destinationUrl}'>{logItem.destinationUrl}</a>";
                var newMessage = App.rewriteMessage.Replace("[URL]", newUrl);
                updatePost.description = newMessage;

                myService.UpdatePost(
                    App.rewriteBlog.serviceUrl,
                    App.rewriteBlog.username,
                    App.rewriteBlog.password,
                    updatePost);
            }
        }

        /// <summary>
        /// Updates log text with progress message.
        /// </summary>
        /// <param name="sender">Rewrite ProgressChanged event.</param>
        /// <param name="e">ProgressChangedEvent arguments.</param>
        /// <history>Sean Patterson 11/16/2010 [Created]</history>
        private void RewriteWorkerProgressChanged(object sender, ProgressChangedEventArgs e) =>
            this.UpdateStatusText(((WorkerArgs)e.UserState).status);

        /// <summary>
        /// Does cleanup work when worker is complete.
        /// </summary>
        /// <param name="sender">Rewrite worker RunWorkerCompleted event.</param>
        /// <param name="e">RunWorkerCompleted event arguments.</param>
        /// <history>Sean Patterson 11/16/2010 [Created]</history>
        private void RewriteWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var progressText = new StringBuilder();

            progressText.AppendLine(this.txtStatus.Text);

            if (e.Cancelled)
            {
                this.UpdateStatusText("Rewrite cancelled.");
            }
            else if (e.Error != null)
            {
                this.UpdateStatusText("Error with rewrite. Process halted.");
            }
            else
            {
                this.UpdateStatusText("Rewrite complete.");
            }
        }
    }
}
