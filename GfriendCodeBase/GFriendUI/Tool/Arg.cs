namespace HP.GFriend.UI.Tool
{
    internal enum ArgType
    {
        DeviceAddress,
        AdminId,
        AdminPassword,
        Port,
        ScriptRoot,
        Custom
    }
    internal class Arg
    {
        public string Name { get; set; }
        public bool Mandatory { get; set; }
        public string Switch { get; set; }
        public ArgType TypeOfArg { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            if(string.IsNullOrEmpty(Value))
            {
                return string.Empty;
            }
            if(string.IsNullOrEmpty(Switch))
            {
                return Value;
            }
            return $"{Switch} {Value}";
        }
    }
}
