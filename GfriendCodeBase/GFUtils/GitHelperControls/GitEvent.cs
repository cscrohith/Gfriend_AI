using System;

namespace HP.GFriend.Utils.Git
{
    public class GitEvent : EventArgs
    {
        public string Message { get; set; }
        public string RepoPath { get; set; }
        public GitEvent(string message)
        {
            Message = message;
        }

        public GitEvent(string message, string repoPath) : this(message)
        {
            RepoPath = repoPath;
        }

    }
}
