using System.Collections.Generic;
using System.Xml;
using System.Linq;

namespace HP.GFriend.Utils.XPath
{
    public class XPathBuilder
    {
        private List<string> _blackListAttributes;
        private List<string> _firstPriorityAttributes;
        public XPathBuilder()
        {
            _blackListAttributes = new List<string>();
            _firstPriorityAttributes = new List<string>();
        }

        public void ClearBlackListAttribute()
        {
            _blackListAttributes = new List<string>();
        }

        public void AddBlackListAttribute(string attribute)
        {
            _blackListAttributes.Add(attribute);
        }

        public void AddBlackListAttribute(List<string> attributes)
        {
            _blackListAttributes.AddRange(attributes);
        }

        public void ClearFirstPriorityAttribute()
        {
            _firstPriorityAttributes = new List<string>();
        }

        public void AddFirstPriorityAttribute(string attribute)
        {
            _firstPriorityAttributes.Add(attribute);
        }

        public void AddFirstPriorityAttribute(List<string> attributes)
        {
            _firstPriorityAttributes.AddRange(attributes);
        }


        public string GetXPath(XmlNode node, XmlDocument document)
        {
            string xPath;
            if(node.ParentNode == null)
            {
                return $"/{node.Name}";
            }
            if(node.Attributes.Count >0)
            {
                // First check with id or identifier attribute
                foreach(XmlAttribute attribute in node.Attributes)
                {
                    if (!string.IsNullOrEmpty(attribute.Value))
                    {
                        if (_firstPriorityAttributes.Contains(attribute.Name))
                        {
                            // apostrophe handling
                            if(attribute.Value.Contains("'"))
                            {
                                xPath = $"//{node.Name}[contains(@{attribute.Name},\"{attribute.Value.Replace("'", @"\'")}\")]";
                            }

                            // Line Feed handling
                            else if(attribute.Value.Contains("\n"))
                            {
                                string value = attribute.Value.Replace("\r", string.Empty);
                                string[] splitted = value.Split('\n');
                                value = splitted.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur);
                                value = value.Replace("'", @"\'").Trim();
                                xPath = $"//{node.Name}[contains(@{attribute.Name},\"{value}\")]";
                                int idx = 1;
                                foreach (XmlNode n in document.SelectNodes(xPath))
                                {
                                    if (n.Equals(node))
                                    {
                                        return $"{xPath}[{idx}]";
                                    }
                                    idx++;
                                }
                            }

                            else
                            {
                                xPath = $"//{node.Name}[@{attribute.Name}='{attribute.Value}']";
                            }
                            
                            if (IsUniqueXPath(xPath, document))
                            {
                                return xPath;
                            }
                        }
                    }
                }

                // Check for other attributes
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    if (!string.IsNullOrEmpty(attribute.Value) && !_blackListAttributes.Contains(attribute.Name) && !_firstPriorityAttributes.Contains(attribute.Name))
                    {
                        // apostrophe handling
                        if (attribute.Value.Contains("'"))
                        {
                            xPath = $"//{node.Name}[contains(@{attribute.Name},\"{attribute.Value.Replace("'", @"\'")}\")]";
                        }

                        // Line Feed handling
                        else if (attribute.Value.Contains("\n"))
                        {
                            string value = attribute.Value.Replace("\r", string.Empty);
                            string[] splitted = value.Split('\n');
                            value = splitted.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur);
                            value = value.Replace("'", @"\'").Trim();
                            xPath = $"//{node.Name}[contains(@{attribute.Name},\"{value}\")]";
                            int idx = 1;
                            foreach (XmlNode n in document.SelectNodes(xPath))
                            {
                                if (n.Equals(node))
                                {
                                    return $"{xPath}[{idx}]";
                                }
                                idx++;
                            }
                        }

                        else
                        {
                            xPath = $"//{node.Name}[@{attribute.Name}='{attribute.Value}']";
                        }
                        if (IsUniqueXPath(xPath, document))
                        {
                            return xPath;
                        }
                    }
                }
            }
            
            // Search parent
            string parentXpath = GetXPath(node.ParentNode, document);
            int index = 1;

            foreach(XmlNode child in node.ParentNode.ChildNodes)
            {
                if(child.Name.Equals(node.Name))
                {
                    if(child.Equals(node))
                    {
                        return $"{parentXpath}/{node.Name}[{index}]";
                    }
                    else
                    {
                        index++;
                    }
                }
            }
            
            return null;
        }

        private bool IsUniqueXPath(string xPath, XmlDocument document)
        {
            return document.SelectNodes(xPath).Count > 1 ? false : true;
        }
    }
}
