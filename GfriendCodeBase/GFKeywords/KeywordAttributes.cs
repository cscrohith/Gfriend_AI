using System;

namespace HP.GFriend.Keywords
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LibraryDescription : Attribute
    {
        public string Description { get; }

        public LibraryDescription(string description)
        {
            Description = description;
        }

        public override string ToString()
        {
            return Description;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class KeywordDescription : Attribute
    {
        public string Description { get; }

        public KeywordDescription(string description)
        {
            Description = description;
        }

        public override string ToString()
        {
            return Description;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class KeywordDisplayName : Attribute
    {
        public string DisplayName { get; }

        public KeywordDisplayName(string displayName)
        {
            DisplayName = displayName;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class KeywordParameters : Attribute
    {
        public string ParamName
        {
            get; private set;
        }
        public string ParamDescription
        {
            get; private set;
        }

        public KeywordParameters(string name, string description)
        {
            ParamName = name;
            ParamDescription = description;
        }

        public override string ToString()
        {
            return $"{ParamName} : {ParamDescription}";
        }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GetKeyword : Attribute
    {
       
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class Deprecated : Attribute
    {
        public string Reason { get; }

        public Deprecated(string reason)
        {
            Reason = reason;
        }

        public override string ToString()
        {
            return Reason;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class SampleScript : Attribute
    {
        public string demoScript { get; }

        public SampleScript(string scriptName)
        {
            demoScript = scriptName;
        }

        public override string ToString()
        {
            return demoScript;
        }
    }
}
