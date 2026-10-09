using Newtonsoft.Json.Linq;

namespace HP.GFriend.Client
{
    public class Script : IGFServerData
    {
        private string _storedPath;
        public int Id { get; set; }
        public string ScriptName { get; set; }
        public string Author { get; set; }
        public int TcCount { get; set; }
        public string Description { get; set; }
        public string StoredPath
        {
            get
            {
                return _storedPath;
            }
            set
            {
                _storedPath = value.Replace('\\','/');
            }
        }
        public string AbsolutePath { get; set; }

        public Script()
        {
            Id = 0;
        }
        public Script(JObject json)
        {
            Id = int.Parse(json["id"].ToString());
            ScriptName = json["script_name"].ToString();
            Author = json["author"].ToString();
            TcCount = int.Parse(json["tc_count"].ToString());
            Description = json["description"].ToString();
            StoredPath = json["stored_path"].ToString();
        }

        public string ToJson()
        {
            JObject json = new JObject();
            json.Add("id", Id);
            json.Add("script_name", ScriptName);
            json.Add("author", Author);
            json.Add("tc_count", TcCount);
            json.Add("description", Description);
            json.Add("stored_path", StoredPath);

            return json.ToString();
        }

        public override string ToString()
        {
            string ret = $"[{ScriptName}] (Author : {Author}) - {TcCount} TC(s)\r\n";
            ret += Description;

            return ret;

        }

    }
}
