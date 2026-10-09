namespace HP.GFriend.Keywords
{
    public class KeywordResult
    {
        public KeywordResults Result { get; set; }
        public string Output { get; set; }
        public byte[] ScreenShot { get; set; }
        public string AdditionalInfo { get; set; }

        public KeywordResult (KeywordResults result)
        {
            Result = result;
        }

        public KeywordResult(KeywordResults result, string output) : this(result)
        {
            Output = output;
        }

    }
}
