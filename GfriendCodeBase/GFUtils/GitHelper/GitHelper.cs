using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HP.GFriend.Utils.Git
{
    public class GitHelper
    {
        private Repository _repository;
        private string _repoPath;
        private CredentialsHandler _credentialsHandler;
        private Identity _identify;
        public string Head { get; private set; }


        public static bool IsGitRepo(string repoPath)
        {
            return Repository.IsValid(repoPath);
        }

        public static GitHelper Clone(string cloneURL, string repoPath, string userName, string userEmail, string httpToken)
        {
            CloneOptions options = new CloneOptions();
            UsernamePasswordCredentials credentials = new UsernamePasswordCredentials();
            credentials.Username = httpToken;
            credentials.Password = string.Empty;
            CredentialsHandler credentialsHandler = (_url, _user, _cred) => credentials;
            options.CredentialsProvider = credentialsHandler;
            Repository.Clone(cloneURL, repoPath, options);
            return new GitHelper(repoPath, userName, userEmail);
        }

        public GitHelper(string repoPath, string userName, string userEmail)
        {
            
            _repoPath = repoPath;
            _repository = new Repository(_repoPath);
            _identify = new Identity(userName, userEmail);
            Head = _repository.Head.FriendlyName;
        }

        public GitHelper(string repoPath, string userName, string userEmail, string httpToken) : 
            this(repoPath, userName, userEmail)
        {
            CreateCredentialForRemote(httpToken);
        }

        public void ChangeConfig(string userName, string userEmail, string httpToken)
        {
            _identify = new Identity(userName, userEmail);
            Head = _repository.Head.FriendlyName;
            CreateCredentialForRemote(httpToken);
        }

        public void CreateCredentialForRemote(string httpToken)
        {
            UsernamePasswordCredentials credentials = new UsernamePasswordCredentials();
            credentials.Username = httpToken;
            credentials.Password = string.Empty;
            _credentialsHandler = (_url, _user, _cred) => credentials;
        }
        public bool ChangeExists()
        {
            return _repository.RetrieveStatus().IsDirty;
        }
        public List<Change> GetChanges()
        {
            List<Change> changes = new List<Change>();

            foreach(StatusEntry s in _repository.RetrieveStatus().Untracked)
            {
                Change added = new Change(s.FilePath, Path.GetFileName(s.FilePath), GitState.Added);
                changes.Add(added);
            }

            foreach(StatusEntry s in _repository.RetrieveStatus().Modified)
            {
                Change modified = new Change(s.FilePath, Path.GetFileName(s.FilePath), GitState.Modified);
                changes.Add(modified);
            }

            foreach(StatusEntry s in _repository.RetrieveStatus().Missing)
            {
                Change deleted = new Change(s.FilePath, Path.GetFileName(s.FilePath), GitState.Deleted);
                changes.Add(deleted);
            }

            return changes;
        }

        public List<string> GetRemoteBranches()
        {
            Remote remote = _repository.Network.Remotes["origin"];
            return _repository.Network.ListReferences(remote, _credentialsHandler).Where(elem => elem.IsLocalBranch).Select(elem => elem.CanonicalName.Replace("refs/heads/", "")).ToList();
        }

        public List<string> GetLocalBranches()
        {
            return _repository.Branches.Where(b => b.IsRemote).Select(b => b.CanonicalName.Split('/').Last()).ToList();
        }

        public void Pull()
        {
            Signature signature = new Signature(_identify, DateTimeOffset.Now);
            PullOptions pullOptions = new PullOptions();
            FetchOptions fetchOptions = new FetchOptions();
            fetchOptions.CredentialsProvider = _credentialsHandler;
            pullOptions.FetchOptions = fetchOptions;
            Commands.Pull(_repository, signature, pullOptions);
        }

        public void Checkout(string branchName)
        {
            if(_repository.Branches.Where(b => b.FriendlyName.Equals(branchName)).Count() <1)
            {
                Branch newBranch = _repository.CreateBranch(branchName);
            }

            Commands.Checkout(_repository, branchName);
            Head = _repository.Head.FriendlyName;
        }

        public void Stage(List<Change> targets)
        {
            Commands.Stage(_repository, targets.Select(t=> t.Path).ToList());
        }

        public string Commit(string commitMessage)
        {
            Signature signature = new Signature(_identify, DateTimeOffset.Now);

            Commit commit = _repository.Commit(commitMessage, signature, signature);
            return commit.Id.ToString();
        }

        public void Push()
        {
            Remote remote = _repository.Network.Remotes["origin"];
            _repository.Branches.Update(_repository.Head, b => b.Remote = remote.Name, b => b.UpstreamBranch = _repository.Head.CanonicalName);
            PushOptions option = new PushOptions();
            option.CredentialsProvider = _credentialsHandler;
            _repository.Network.Push(_repository.Head, option);
        }
    }
}
