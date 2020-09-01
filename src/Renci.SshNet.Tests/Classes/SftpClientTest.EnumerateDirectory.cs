﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;
using System.Diagnostics;
using System.Linq;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    public partial class SftpClientTest : TestBase
    {
        [TestMethod]
        [TestCategory("Sftp")]
        [ExpectedException(typeof(SshConnectionException))]
        public void Test_Sftp_EnumerateDirectory_Without_Connecting()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                var files = sftp.EnumerateDirectory(".");
                foreach (var file in files)
                {
                    Debug.WriteLine(file.FullName);
                }
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        [ExpectedException(typeof(SftpPermissionDeniedException))]
        public void Test_Sftp_EnumerateDirectory_Permission_Denied()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                var files = sftp.EnumerateDirectory("/root");
                foreach (var file in files)
                {
                    Debug.WriteLine(file.FullName);
                }

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        [ExpectedException(typeof(SftpPathNotFoundException))]
        public void Test_Sftp_EnumerateDirectory_Not_Exists()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                var files = sftp.EnumerateDirectory("/asdfgh");
                foreach (var file in files)
                {
                    Debug.WriteLine(file.FullName);
                }

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        public void Test_Sftp_EnumerateDirectory_Current()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                var files = sftp.EnumerateDirectory(".");

                Assert.IsTrue(files.Count() > 0);

                foreach (var file in files)
                {
                    Debug.WriteLine(file.FullName);
                }

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        public void Test_Sftp_EnumerateDirectory_Empty()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                var files = sftp.EnumerateDirectory(string.Empty);

                Assert.IsTrue(files.Count() > 0);

                foreach (var file in files)
                {
                    Debug.WriteLine(file.FullName);
                }

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        [Description("Test passing null to EnumerateDirectory.")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_Sftp_EnumerateDirectory_Null()
        {
            using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                sftp.Connect();

                var files = sftp.EnumerateDirectory(null);

                Assert.IsTrue(files.Count() > 0);

                foreach (var file in files)
                {
                    Debug.WriteLine(file.FullName);
                }

                sftp.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        public void Test_Sftp_EnumerateDirectory_HugeDirectory()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
                {
                    sftp.Connect();
                    sftp.ChangeDirectory("/home/" + Resources.USERNAME);

                    var count = 10000;
                    //  Create 10000 directory items
                    for (int i = 0; i < count; i++)
                    {
                        sftp.CreateDirectory(string.Format("test_{0}", i));
                    }
                    Debug.WriteLine("Created {0} directories within {1} seconds", count, stopwatch.Elapsed.TotalSeconds);

                    stopwatch.Restart();
                    var files = sftp.EnumerateDirectory(".");
                    Debug.WriteLine("Listed {0} directories within {1} seconds", count, stopwatch.Elapsed.TotalSeconds);

                    //  Ensure that directory has at least 10000 items
                    stopwatch.Restart();
                    var actualCount = files.Count();
                    Assert.IsTrue(actualCount >= 10000);
                    Debug.WriteLine("Used {0} items within {1} seconds", actualCount, stopwatch.Elapsed.TotalSeconds);

                    sftp.Disconnect();
                }
            }
            finally
            {
                stopwatch.Restart();
                RemoveAllFiles();
                stopwatch.Stop();
                Debug.WriteLine("Removed all files within {0} seconds", stopwatch.Elapsed.TotalSeconds);
            }
        }

        [TestMethod]
        [TestCategory("Sftp")]
        [TestCategory("integration")]
        [ExpectedException(typeof(SshConnectionException))]
        public void Test_Sftp_EnumerateDirectory_After_Disconnected()
        {
            try {
                using (var sftp = new SftpClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
                {
                    sftp.Connect();

                    sftp.CreateDirectory("test_at_dsiposed");

                    var files = sftp.EnumerateDirectory(".").Take(1);

                    sftp.Disconnect();

                    // Must fail on disconnected session.
                    var count = files.Count();
                }
            }
            finally
            {
                RemoveAllFiles();
            }
        }
    }
}