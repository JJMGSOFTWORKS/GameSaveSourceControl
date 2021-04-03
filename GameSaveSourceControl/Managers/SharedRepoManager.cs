using GameSaveSourceControl.Model;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace GameSaveSourceControl.Managers
{
    //TODO Warn Can only currently pull/push from a branch named master
    public class SharedRepoManager
    {
        public SharedRepoManager()
        {

        }

        public string CloneRepo(string repoFolderPath)
        {
            try
            {
                var co = new CloneOptions();
                co.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = ConfigurationManager.AppSettings["GitEmail"], Password = ConfigurationManager.AppSettings["GitPassword"] };
                Repository.Clone(ConfigurationManager.AppSettings["GitRemoteUrl"], repoFolderPath, co);
                return repoFolderPath;
            }
            catch (LibGit2SharpException e)
            {
                if (e.Message.Contains("exists"))
                {
                    Console.WriteLine("log :: check seems to indicate we have already cloned the repo");
                    return repoFolderPath;
                }
                throw;
            }
        }

        public string PullWithStatus(string repoFolderPath)
        {
            using (var repo = new Repository(repoFolderPath))
            {
                // Credential information to fetch
                PullOptions options = new PullOptions();
                options.FetchOptions = new FetchOptions();
                options.FetchOptions.CredentialsProvider = new CredentialsHandler(
                    (url, usernameFromUrl, types) =>
                        new UsernamePasswordCredentials()
                        {
                            Username = ConfigurationManager.AppSettings["GitEmail"],
                            Password = ConfigurationManager.AppSettings["GitPassword"]
                        });

                Signature author = new Signature(ConfigurationManager.AppSettings["GitUser"], ConfigurationManager.AppSettings["GitEmail"], DateTime.Now);

                // Pull
                return Commands.Pull(repo, author, options).Status.ToString();
            }
        }

        public string PushWithStatus(List<LocalMapping> fileMappings, string repoFolderPath)
        {
            try
            {
                SyncFilesToRepo(fileMappings, repoFolderPath);

                using (var repo = new Repository(repoFolderPath))
                {
                    foreach (var item in repo.RetrieveStatus(new LibGit2Sharp.StatusOptions()))
                        Console.WriteLine(item.FilePath);

                    var changes = repo.RetrieveStatus(new LibGit2Sharp.StatusOptions());

                    if (!changes.ToList().Any())
                        return "no changes to push";

                    Commands.Stage(repo, "*");
                    repo.Index.Write();

                    // Create the committer's signature and commit
                    Signature author = new Signature(ConfigurationManager.AppSettings["GitUser"], ConfigurationManager.AppSettings["GitEmail"], DateTime.Now);
                    Signature committer = author;

                    // Commit to the repository
                    Commit commit = repo.Commit($"Saves Update {DateTime.Now.ToShortDateString()}", author, committer);

                    PushOptions options = new PushOptions();
                    options.CredentialsProvider = new CredentialsHandler(
                        (url, usernameFromUrl, types) =>
                            new UsernamePasswordCredentials()
                            {
                                Username = ConfigurationManager.AppSettings["GitEmail"],
                                Password = ConfigurationManager.AppSettings["GitPassword"]
                            });
                    repo.Network.Push(repo.Branches["master"], options);

                    return "successfully pushed changes to the shared repo";
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error has occurred. {e.Message}");
                return "failed to push changes";
            }
        }

        public void TransforLocalToShared(string localFilePath, string sharedLocation, string saveName)
        {
            var sharedSubFolder = $"{ sharedLocation }\\{ Constants.BaseFolderName}\\{saveName}\\{localFilePath.Split('\\').Last()}";
            try
            {
                TransforFilesfromSourceToTarget(localFilePath, sharedSubFolder);
            }
            catch (Exception e)
            {
                Console.WriteLine($"The game data for {saveName} failed to be pulled");
                Console.WriteLine($"log :: {e.Message}");
            }
        }

        public void TransforSharedToLocal(string sharedLocation, string localFilePath, string saveName)
        {
            var sharedSubFolder = $"{ sharedLocation }\\{ Constants.BaseFolderName}\\{saveName}\\{localFilePath.Split('\\').Last()}";
            try
            {
                TransforFilesfromSourceToTarget(sharedSubFolder, localFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"The game date for {saveName} failed to be pulled");
                Console.WriteLine($"log :: {e.Message}");
            }
        }


        void TransforFilesfromSourceToTarget(string sourcePath, string targetPath)
        {
            DirectoryInfo source = new DirectoryInfo(sourcePath);
            DirectoryInfo target = new DirectoryInfo(targetPath);

            TransferDirectorySourceToTarget(source, target);
        }

        void SyncFilesToRepo(List<LocalMapping> fileMappings, string sharedLocation)
        {
            foreach (var item in fileMappings)
                TransforLocalToShared(item.FilePathToTrack, sharedLocation, item.FileName);
        }

        void TransferDirectorySourceToTarget(DirectoryInfo source, DirectoryInfo target)
        {
            if (source.FullName.ToLower() == target.FullName.ToLower())
                return;

            if (!Directory.Exists(Path.GetDirectoryName(source.FullName)))
                throw new FileNotFoundException($"The source file {source.FullName} was not found");

            // Check if the target directory exists, if not, create it.
            if (!Directory.Exists(Path.GetDirectoryName(target.FullName)))
                Directory.CreateDirectory(Path.GetDirectoryName(target.FullName));

            try
            {
                // get the file attributes for file or directory
                FileAttributes attr = File.GetAttributes(source.FullName);

                bool isFolder = attr.HasFlag(FileAttributes.Directory);

                if (!isFolder)
                    File.Copy(source.FullName, target.FullName, true);
                else
                    FileSystem.CopyDirectory(source.FullName, target.FullName, true);
            }
            catch (Exception e)
            {
                Console.WriteLine("Log :: Could not retrieve a source file");
                throw;
            }
        }
    }
}

