namespace HP.GFriend.Utils.Git
{
    public class Change
    {
        public string Path { get; set; }
        public string FileName { get; set; }
        public GitState State { get; set; }

        public Change(string path, string fileName, GitState state)
        {
            Path = path;
            FileName = fileName;
            State = state;
        }

        public override string ToString()
        {
            return $"{Path} ({State})";
        }
    }
}
