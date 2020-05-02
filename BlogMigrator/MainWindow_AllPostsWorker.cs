namespace BlogMigrator
{
    using CookComputing.MetaWeblog;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Windows;
    using System.Xml.Serialization;

    public partial class MainWindow : Window
    {
        /// <summary>
        /// Runs the get all posts process as a background thread.
        /// </summary>
        /// <param name="sender">DoWork event.</param>
        /// <param name="e">DoWorkEvent arguments.</param>
        /// <history>Sean Patterson 11/1/2010 [Created]</history>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private void AllPostsWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var myArgs = (WorkerArgs)e.Argument;

            var myService = new Services();
            var myBlog = App.sourceBlog;

            myArgs.status = "Retrieving post from source...";
            this.allPostsWorker.ReportProgress(0, myArgs);

            try
            {
                // Clear out any old blog entries.
                App.sourceBlog.blogData = new BlogML.blogType();
                App.sourceBlog.blogPosts = new List<Post>();
                App.sourceBlog.postsToMigrate = new List<int>();

                // Retrieve data via XML-RPC unless a source file has been specified.
                if (string.IsNullOrEmpty(myBlog.blogFile))
                {
                    myArgs.status = "Connecting to source...";
                    this.allPostsWorker.ReportProgress(10, myArgs);
                    myBlog.blogPosts = myService.GetAllPosts(
                        myBlog.serviceUrl, myBlog.blogId, myBlog.username, myBlog.password);
                }
                else
                {
                    myArgs.status = "Parsing file for posts...";
                    this.allPostsWorker.ReportProgress(10, myArgs);

                    var serializer = new XmlSerializer(typeof(BlogML.blogType));
                    TextReader reader = new StreamReader(myBlog.blogFile);

                    myBlog.blogData = (BlogML.blogType)serializer.Deserialize(reader);
                    reader.Close();

                    myArgs.status = "Posts retrieved.";
                    this.allPostsWorker.ReportProgress(100, myArgs);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred retrieving blog posts:{Environment.NewLine}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}Please verify your settings and try retrieving posts again.",
                    "Error Retrieving Posts",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                this.allPostsWorker.CancelAsync();
            }
        }

        /// <summary>
        /// Updates log text with progress message.
        /// </summary>
        /// <param name="sender">ProgressChanged event.</param>
        /// <param name="e">ProgressChangedEvent arguments.</param>
        /// <history>Sean Patterson 11/1/2010 [Created]</history>
        private void AllPostsWorkerProgressChanged(object sender, ProgressChangedEventArgs e) =>
            this.UpdateStatusText(((WorkerArgs)e.UserState).status);

        /// <summary>
        /// Does cleanup work when worker is complete.
        /// </summary>
        /// <param name="sender">RunWorkerCompleted event.</param>
        /// <param name="e">RunWorkerCompleted event arguments.</param>
        /// <history>Sean Patterson 11/1/2010 [Created]</history>
        private void AllPostsWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var ProgressText = new StringBuilder();

            ProgressText.AppendLine(this.txtStatus.Text);

            if (e.Cancelled)
            {
                this.UpdateStatusText("Process cancelled.");
                this.UpdateStatusBar("Process cancelled.");
            }
            else if (e.Error != null)
            {
                this.UpdateStatusText("Error with process. Process halted.");
                this.UpdateStatusBar("Process halted.");
            }
            else
            {
                // Use an observable collection to properly bind/update the ListView
                this.PostCollection.Clear();

                if (App.sourceBlog.blogPosts.Count > 0 || (App.sourceBlog.blogData != null))
                {
                    if (App.sourceBlog.blogPosts.Count > 0)
                    {
                        foreach (var postItem in App.sourceBlog.blogPosts)
                        {
                            var myPost = new PostData(postItem);
                            this.PostCollection.Add(myPost);
                        }

                        this.btnMigrate.IsEnabled = true;
                        this.btnSelectAllPosts.IsEnabled = true;
                    }
                    else
                    {
                        for (var i = 0; i <= App.sourceBlog.blogData.posts.Length - 1; i++)
                        {
                            var myPost = new PostData(App.sourceBlog.blogData.posts[i]);
                            this.PostCollection.Add(myPost);
                        }

                        this.btnMigrate.IsEnabled = true;
                        this.btnSelectAllPosts.IsEnabled = true;
                    }
                }

                this.lsvAllPosts.ItemsSource = this.PostCollection;
                this.lblEntriesCount.Content = $"[{this.PostCollection.Count} Total]";

                this.UpdateStatusText("Process complete.");
                this.UpdateStatusBar("Process complete.");
            }
        }
    }
}
