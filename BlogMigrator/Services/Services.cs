namespace BlogMigrator
{
    using CookComputing.MetaWeblog;
    using CookComputing.XmlRpc;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Windows;

    /// <summary>
    /// This class handles all of the MetaWebBlog API method calls.
    /// </summary>
    internal class Services
    {
        /// <summary>
        /// Verifies that the destination server can be connected to by
        /// retrieving a single post.
        /// </summary>
        /// <param name="service">The service to connect to.</param>
        /// <param name="blogId">The blog Id to login with.</param>
        /// <param name="username">The username to login with.</param>
        /// <param name="password">The password to login with.</param>
        /// <returns>String with result message.</returns>
        /// <remarks>
        /// No exception handling is performed in the method since the only
        /// thing it could do is bubble it up one step further. Calling method
        ///// must be resposible for checking for an exception.
        /// </remarks>
        /// <history>
        /// Sean Patterson    11/3/2010   [Created]
        /// </history>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public string CheckServerStatus(string serviceUrl, string blogId, string username, string password)
        {
            var proxy = (IMetaWeblog)XmlRpcProxyGen.Create(typeof(IMetaWeblog));
            var cp = (XmlRpcClientProtocol)proxy;
            cp.Url = serviceUrl;

            string results;
            try
            {
                var testPosts = proxy.getRecentPosts(blogId, username, password, 1);

                results = testPosts.Length > 0 ? "Connection successful." : "Connection failed. No posts found.";
            }
            catch (Exception ex)
            {
                results = $"Connection failed: {ex}";
            }

            return results;
        }

        /// <summary>
        /// Retrieves all posts from the blog.
        /// </summary>
        /// <param name="serviceUrl">The service URL to connect to.</param>
        /// <param name="blogId">The blog Id to login with.</param>
        /// <param name="username">The username to login with.</param>
        /// <param name="password">The password to login with.</param>
        /// <returns>List collection of posts.</returns>
        /// <history>Sean Patterson 11/3/2010 [Created]</history>
        public List<Post> GetAllPosts(
            string serviceUrl, 
            string blogId,
            string username, 
            string password)
        {
            var proxy = (IMetaWeblog)XmlRpcProxyGen.Create(typeof(IMetaWeblog));
            var cp = (XmlRpcClientProtocol)proxy;
            cp.Url = serviceUrl;

            var results = new List<Post>();
            try
            {
                var TestPosts = proxy.getRecentPosts(blogId, username, password, 9999999);

                if (TestPosts.Length > 0)
                {
                    results = new List<Post>(TestPosts);
                }
            }
            catch (XmlRpcFaultException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            return results;
        }

        /// <summary>
        /// Retrieves a blog post.
        /// </summary>
        /// <param name="serviceUrl">The service URL to connect to.</param>
        /// <param name="postId">The post Id to retrieve.</param>
        /// <param name="username">The username to login with.</param>
        /// <param name="password">The password to login with.</param>
        /// <returns>List collection of posts.</returns>
        /// <history>Sean Patterson 11/7/2010 [Created]</history>
        public Post GetPost(string serviceUrl, int postId, string username,
                            string password)
        {
            var proxy = (IMetaWeblog)XmlRpcProxyGen.Create(typeof(IMetaWeblog));
            var cp = (XmlRpcClientProtocol)proxy;
            cp.Url = serviceUrl;
            Post results;
            try
            {
                results = proxy.getPost(postId.ToString(), username, password);
            }
            catch (XmlRpcFaultException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            return results;
        }

        /// <summary>
        /// Inserts a post.
        /// </summary>
        /// <param name="serviceUrl">The service URL of the blog.</param>
        /// <param name="blogId">The blog Id.</param>
        /// <param name="username">The blog username.</param>
        /// <param name="password">The blog password.</param>
        /// <param name="title">The post title.</param>
        /// <param name="content">The post content.</param>
        /// <param name="authorid">The post author id.</param>
        /// <param name="dateCreated">The post creation date.</param>
        /// <param name="categories">The post categories.</param>
        /// <returns>Post object that was created by the server.</returns>
        public Post InsertPost(string serviceUrl, string blogId, string username,
                               string password, string title, string content,
                               string authorid, DateTime dateCreated,
                               List<string> categories)
        {
            Post results;
            string postResult;
            Post TestPost;

            var proxy = (IMetaWeblog)XmlRpcProxyGen.Create(typeof(IMetaWeblog));
            var cp = (XmlRpcClientProtocol)proxy;
            cp.Url = serviceUrl;

            try
            {
                TestPost = new Post
                {
                    dateCreated = dateCreated,
                    userid = authorid,
                    title = title,
                    description = content,
                    categories = categories.ToArray()
                };

                postResult = proxy.newPost(blogId, username, password, TestPost, true);

                if (string.IsNullOrEmpty(postResult))
                {
                    throw new Exception("Post not created.");
                }
                else
                {
                    results = proxy.getPost(postResult, username, password);
                }
            }
            catch (XmlRpcFaultException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            return results;
        }

        /// <summary>
        /// Inserts a post.
        /// </summary>
        /// <param name="serviceUrl">The service URL of the blog.</param>
        /// <param name="blogId">The blog Id.</param>
        /// <param name="username">The blog username.</param>
        /// <param name="password">The blog password.</param>
        /// <param name="postItem">The post object.</param>
        /// <returns>Post object that was created by the server.</returns>
        public Post InsertPost(string serviceUrl, string blogId, string username,
                               string password, Post postItem, StreamWriter swLog, bool batchMode)
        {
            Post results;
            Post tempPost;
            string postResult;

            var proxy = (IMetaWeblog)XmlRpcProxyGen.Create(typeof(IMetaWeblog));
            var cp = (XmlRpcClientProtocol)proxy;
            cp.Url = serviceUrl;
            cp.NonStandard = XmlRpcNonStandard.All;
            tempPost = new Post();

            try
            {
                tempPost.dateCreated = postItem.dateCreated;
                tempPost.userid = username;
                tempPost.title = postItem.title;
                tempPost.description = postItem.description;
                tempPost.categories = postItem.categories;

                postResult = proxy.newPost(blogId, username, password, tempPost, true);

                if (string.IsNullOrEmpty(postResult))
                {
                    throw new Exception("Post not created.");
                }
                else
                {
                    results = proxy.getPost(postResult, username, password);
                }
            }
            catch (Exception ex)
            {
                var messageBoxText = "An error occurred migrating blog post:" +
                                     Environment.NewLine + Environment.NewLine +
                                     ex.ToString() +
                                     Environment.NewLine + Environment.NewLine + tempPost.ToString();
                swLog.WriteLine(messageBoxText);
                if (!batchMode)
                {
                    var answer = MessageBox.Show(messageBoxText, "Error Migrating Post",
                        MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    if (answer == MessageBoxResult.Cancel)
                    {
                        throw;
                    }
                }

                return new Post();
            }

            return results;
        }


        /// <summary>
        /// Inserts a test post.
        /// </summary>
        /// <param name="serviceUrl">The service URL to connect to.</param>
        /// <param name="blogId">The blog Id to login with.</param>
        /// <param name="username">The username to login with.</param>
        /// <param name="password">The password to login with.</param>
        /// <returns>Message indicating success or failure.</returns>
        /// <history>Sean Patterson 11/3/2010 [Created]</history>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public string InsertSamplePost(string serviceUrl, string blogId,
                                       string username, string password)
        {
            string Results;
            string postResult;
            Post TestPost;
            Post ReturnPost;

            var proxy = (IMetaWeblog)XmlRpcProxyGen.Create(typeof(IMetaWeblog));
            var cp = (XmlRpcClientProtocol)proxy;
            cp.Url = serviceUrl;

            try
            {
                TestPost = new Post
                {
                    categories = new string[] { "Cool Category" },
                    dateCreated = DateTime.Now,
                    userid = username,
                    title = "Cool new XML-RPC test!",
                    description = "This is the main body of the post. It has lots of cool things here to test the migration I'm about to do."
                };

                postResult = proxy.newPost(blogId, username, password, TestPost, true);

                if (string.IsNullOrEmpty(postResult))
                {
                    Results = "Fail. No new post.";
                }
                else
                {
                    ReturnPost = proxy.getPost(postResult, username, password);

                    Results = $"Success! Post Id = {ReturnPost.postid}{Environment.NewLine}Link to post is: {ReturnPost.link}";
                }
            }
            catch (XmlRpcFaultException fex)
            {
                Results = $"XML-RPC error connecting to server: {fex}";
            }
            catch (Exception ex)
            {
                Results = $"General error connecting to server: {ex}";
            }

            return Results;
        }

        /// <summary>
        /// Updates the post.
        /// </summary>
        /// <param name="serviceUrl">The service URL of the blog.</param>
        /// <param name="username">The blog username.</param>
        /// <param name="password">The blog password.</param>
        /// <param name="postItem">The post object.</param>
        /// <returns>True/False indicating if post was updated.</returns>
        /// <remarks>It is assumed that the Post object already has the updated details in it.</remarks>
        public bool UpdatePost(string serviceUrl, string username, string password, Post postItem)
        {
            var proxy = (IMetaWeblog)XmlRpcProxyGen.Create(typeof(IMetaWeblog));
            var cp = (XmlRpcClientProtocol)proxy;
            cp.Url = serviceUrl;

            object results;
            try
            {
                results = proxy.editPost(postItem.postid.ToString(), username, password,
                                         postItem, true);
            }
            catch (XmlRpcFaultException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            return (bool)results;
        }
    }
}
