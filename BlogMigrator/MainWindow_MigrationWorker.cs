namespace BlogMigrator
{
    using CookComputing.MetaWeblog;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Windows;

    public partial class MainWindow : Window
    {
        /// <summary>
        /// Publishes posts to the destination blog from a BlogML object.
        /// </summary>
        /// <history>Sean Patterson 11/15/2010 [Created]</history>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public void ImportXMLPosts()
        {
            var logFile = $"{App.sourceBlog.serviceType}_{App.destBlog.serviceType}_Migration-{DateTime.Now:yyyy_MM_dd_hhMMss}.csv";

            // Load document.
            var swLog = new StreamWriter(logFile);
            swLog.WriteLine("Source Id, Source Link, Destination Id, Destination Link");
            swLog.Flush();

            try
            {
                var args = new WorkerArgs
                {
                    status = $"Migrating posts from {App.sourceBlog.serviceType} to {App.destBlog.serviceType}"
                };
                this.migrationWorker.ReportProgress(0, args);

                var totalCount = App.sourceBlog.blogData.posts.Length;
                for (var i = 0; i <= totalCount - 1; i++)
                {
                    var currPost = App.sourceBlog.blogData.posts[i];

                    if (App.sourceBlog.postsToMigrate.Contains(Convert.ToInt32(currPost.id)))
                    {
                        args.status = $"Writing Post: {string.Join(" ", currPost.title.Text)}";
                        this.migrationWorker.ReportProgress(decimal.ToInt32(i * 100 / totalCount), args);

                        var newPost = new Post
                        {
                            title = string.Join(" ", currPost.title.Text),
                            dateCreated = currPost.datecreated,
                            userid = App.destBlog.username,
                            postid = currPost.id,
                            description = currPost.content.Value,
                            link = App.sourceBlog.rootUrl + currPost.posturl
                        };

                        // Post Tags/Categories (currently only categories are implemented with BlogML
                        if (currPost.categories != null)
                        {
                            var categoryList = new List<string>();

                            for (var j = 0; j <= currPost.categories.Length - 1; j++)
                            {
                                var currCatRef = currPost.categories[j];

                                // some BlogMl can have currCatRef.@ref  as actual name
                                var categoryName = int.TryParse(currCatRef.@ref, out var categoryId)
                                    ? new Generator().GetCategoryById(App.sourceBlog.blogData, categoryId)
                                    : currCatRef.@ref;

                                categoryList.Add(categoryName);
                            }

                            newPost.categories = categoryList.ToArray();
                        }

                        var blogService = new Services();

                        var resultPost = blogService.InsertPost(
                            App.destBlog.serviceUrl,
                            App.destBlog.blogId,
                            App.destBlog.username,
                            App.destBlog.password,
                            newPost,
                            swLog,
                            App.BatchMode);

                        swLog.WriteLine($"{newPost.postid},{newPost.link},{resultPost.postid},{resultPost.link}");

                        swLog.Flush();

                        // Rewrite posts can still be done "live" even if a BlogML file is being
                        // imported provided the serviceUrl details are provided.
                        if (App.rewritePosts)
                        {
                            var updatePost = newPost;
                            var newUrl = $"<a href='{resultPost.link}'>{resultPost.link}</a>";
                            var newMessage = App.rewriteMessage.Replace("[URL]", newUrl);
                            updatePost.description = newMessage;

                            blogService.UpdatePost(
                                App.sourceBlog.serviceUrl,
                                App.sourceBlog.username,
                                App.sourceBlog.password,
                                updatePost);
                        }
                    }
                }

                swLog.Close();
                args.status = $"Log saved to {logFile}";
                this.migrationWorker.ReportProgress(100, args);
            }
            catch (Exception ex)
            {
                swLog.Flush();
                swLog.Close();
                MessageBox.Show(
                    $"An error occurred migrating blog posts:{Environment.NewLine}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}Please verify your settings and try migrating posts again.",
                    "Error Migrating Posts",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                this.migrationWorker.CancelAsync();
            }
        }


        /// <summary>
        /// Publishes posts to the destination blog.
        /// </summary>
        /// <history>Sean Patterson 11/15/2010 [Created]</history>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public void WriteXMLPosts()
        {
            var logFile = $"{App.sourceBlog.serviceType}_{App.destBlog.serviceType}_Migration-{DateTime.Now:yyyy_MM_dd_hhMMss}.csv";

            // Load document.
            var swLog = new StreamWriter(logFile);
            swLog.WriteLine("Source Id, Source Link, Destination Id, Destination Link");

            swLog.Flush();

            try
            {
                var args = new WorkerArgs
                {
                    status = $"Migrating posts from {App.sourceBlog.serviceType} to {App.destBlog.serviceType}"
                };
                this.migrationWorker.ReportProgress(15, args);

                args.status = $"Rewriting original posts {(App.rewritePosts ? "ENABLED" : "DISABLED")}.";
                this.migrationWorker.ReportProgress(15, args);

                foreach (
                    var blogPost in App.sourceBlog.blogPosts)
                {
                    if (App.sourceBlog.postsToMigrate.Contains(Convert.ToInt32(blogPost.postid)))
                    {
                        args.status = $"Writing Post: {blogPost.title}";
                        this.migrationWorker.ReportProgress(20, args);

                        var blogService = new Services();
                        var resultPost = blogService.InsertPost(
                            App.destBlog.serviceUrl,
                            App.destBlog.blogId,
                            App.destBlog.username,
                            App.destBlog.password,
                            blogPost,
                            swLog,
                            App.BatchMode);

                        swLog.WriteLine($"{blogPost.postid},{blogPost.link},{resultPost.postid},{resultPost.link}");

                        swLog.Flush();

                        if (App.rewritePosts)
                        {
                            var updatePost = blogPost;
                            var newUrl = $"<a href='{resultPost.link}'>{resultPost.link}</a>";
                            var newMessage = App.rewriteMessage.Replace("[URL]", newUrl);
                            updatePost.description = newMessage;

                            blogService.UpdatePost(
                                App.sourceBlog.serviceUrl,
                                App.sourceBlog.username,
                                App.sourceBlog.password,
                                updatePost);
                        }
                    }
                }
                swLog.Close();
            }
            catch (Exception ex)
            {
                swLog.Flush();
                swLog.Close();
                MessageBox.Show(
                    $"An error occurred migrating blog posts:{Environment.NewLine}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}Please verify your settings and try migrating posts again.",
                    "Error Migrating Posts",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                this.migrationWorker.CancelAsync();
            }
        }

        /// <summary>
        /// Determines which method of migration to use based on the source and destination
        /// variables specified.
        /// </summary>
        /// <returns>Migration method code.</returns>
        /// <history>Sean Patterson 11/10/2010 [Created]</history>
        private string DetermineMigrationMethod()
        {
            var results = string.Empty;

            if (App.sourceBlog.serviceType != "FILE" && App.destBlog.serviceType != "FILE")
            {
                results = "XMLtoXML";
            }

            if (App.sourceBlog.serviceType == "FILE" && App.destBlog.serviceType != "FILE")
            {
                results = "FILEtoXML";
            }

            if (App.sourceBlog.serviceType != "FILE" && App.destBlog.serviceType == "FILE")
            {
                results = "XMLtoWXR";
            }

            if (App.sourceBlog.serviceType == "FILE" && App.destBlog.serviceType == "FILE")
            {
                results = "FILEtoWXR";
            }

            return results;
        }

        /// <summary>
        /// Runs the conversion process as a background thread.
        /// </summary>
        /// <param name="sender">Conversion DoWork dvent.</param>
        /// <param name="e">DoWorkEvent arguments.</param>
        /// <history>Sean Patterson 11/1/2010 [Created]</history>
        private void MigrationWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var myArgs = (WorkerArgs)e.Argument;

            // Load document.
            myArgs.status = "Starting migration process...";
            this.migrationWorker.ReportProgress(0, myArgs);


            var myGenerator = new Generator();
            switch (this.DetermineMigrationMethod())
            {
                case "XMLtoXML":
                    this.WriteXMLPosts();
                    break;

                case "FILEtoXML":
                    this.ImportXMLPosts();
                    break;

                case "XMLtoWXR":
                    myGenerator.WriteWXRDocument(
                        App.sourceBlog.blogPosts,
                        App.sourceBlog.postsToMigrate,
                        App.sourceBlog.rootUrl,
                        App.destBlog.blogFile);
                    break;

                case "FILEtoWXR":
                    myGenerator.WriteWXRDocument(
                        App.sourceBlog.blogData,
                        App.sourceBlog.postsToMigrate,
                        App.sourceBlog.rootUrl,
                        App.destBlog.blogFile);
                    break;

                default:
                    throw new Exception("No migration method found.");
            }

            myArgs.status = "Migration process complete.";
            this.migrationWorker.ReportProgress(100, myArgs);
        }

        /// <summary>
        /// Updates log text with progress message.
        /// </summary>
        /// <param name="sender">Conversion ProgressChanged event.</param>
        /// <param name="e">ProgressChangedEvent arguments.</param>
        /// <history>Sean Patterson 11/1/2010 [Created]</history>
        private void MigrationWorkerProgressChanged(object sender, ProgressChangedEventArgs e) =>
            this.UpdateStatusText(((WorkerArgs)e.UserState).status);

        /// <summary>
        /// Does cleanup work when worker is complete.
        /// </summary>
        /// <param name="sender">ConversionWorker RunWorkerCompleted event.</param>
        /// <param name="e">RunWorkerCompleted event arguments.</param>
        /// <history>Sean Patterson 11/1/2010 [Created]</history>
        private void MigrationWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var progressText = new StringBuilder();

            progressText.AppendLine(this.txtStatus.Text);

            if (e.Cancelled)
            {
                this.UpdateStatusText("Migration cancelled.");
                this.UpdateStatusBar("Migration cancelled.");
            }
            else if (e.Error != null)
            {
                this.UpdateStatusText("Error with migration. Process halted.");
                this.UpdateStatusBar("Migration halted.");
            }
            else
            {
                // Use an observable collection to properly bind/update the ListView
                this.PostCollection.Clear();

                if (App.sourceBlog.blogPosts.Count > 0)
                {
                    foreach (var postItem in App.sourceBlog.blogPosts)
                    {
                        var myPost = new PostData(postItem);
                        this.PostCollection.Add(myPost);
                    }
                }
                else
                {
                    for (var i = 0; i <= App.sourceBlog.blogData.posts.Length - 1; i++)
                    {
                        var myPost = new PostData(App.sourceBlog.blogData.posts[i]);
                        this.PostCollection.Add(myPost);
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
