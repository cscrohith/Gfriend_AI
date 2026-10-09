using HP.GFriend.Keywords;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HP.GFriend.GFLogger;
using System.IO;

namespace HP.GFriend.Keywords
{
    public class XML : IGFLibrary
    {
        private XmlDocument _xmlDoc;
        public void Dispose()
        {
            _xmlDoc = null;
        }

        public bool DutUsed()
        {
            return false;
        }

        public List<string> GetDependencies()
        {
            return null;
        }

        public string GetName()
        {
            return "XML";
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            
        }
       
        private static IDictionary<string, string> GetXmlNamespaces(XmlDocument _xml)
        {
            XPathNavigator ns = _xml.CreateNavigator();
            ns.MoveToFollowing(XPathNodeType.Element);
            return ns.GetNamespacesInScope(XmlNamespaceScope.All);
        }

        [KeywordDescription("Load xml with xml text")]
        [KeywordDisplayName("Load")]
        [KeywordParameters("xml", "xml formatted text")]
        [SampleScript("XML.Load (&lt;testcase>&lt;testcasename name='Testcase1'>Testcase1&lt;/testcasename>&lt;result>Pass&lt;/result>&lt;/testcase>)")]
        public KeywordResult Load(string xml)
        {
            try
            {
                _xmlDoc = new XmlDocument();
                _xmlDoc.LoadXml(xml);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error with xml";
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [KeywordDescription("Load xml from file")]
        [KeywordDisplayName("Load From File")]
        [KeywordParameters("filePath", "path of xml file")]
        [SampleScript("XML.Load From File (C:\\Users\\BaPr519\\Desktop\\Work\\KeywordSampleScripts\\xml\\sample.xml)")]
        public KeywordResult LoadFromFile(string filePath)
        {
            filePath = Support.Utils.GetAbsolutePath(filePath, CommonExecutionInfo.ScriptFolder);

            if(!File.Exists(filePath))
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"File not exist : {filePath}";
                Logger.Error(error.Output);
                return error;
            }

            try
            {
                _xmlDoc = new XmlDocument();
                _xmlDoc.Load(filePath);
                return new KeywordResult(KeywordResults.Pass);
            }
            catch (Exception ex)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = "Error with xml";
                error.AdditionalInfo = ex.ToString();
                Logger.Error(error.Output, ex);
                return error;
            }
        }

        [GetKeyword]
        [KeywordDescription("Get attribute of xml node")]
        [KeywordDisplayName("Get Attribute")]
        [KeywordParameters("saveTo", "name of variable which attribute value will be saved")]
        [KeywordParameters("xPath", "XPath of xml node")]
        [KeywordParameters("attributeName", "name of attribute to get value")]
        [SampleScript("XML.Get Attribute (${attribute},/testcase/testcasename,name)")]
        public KeywordResult GetAttribute(string saveTo, string xPath, string attributeName)
        {
            xPath = Support.Utils.GetVariablevalueIfExist(xPath);
            attributeName = Support.Utils.GetVariablevalueIfExist(attributeName);
            if (_xmlDoc == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"XML is not loaded. Use Load or Load From File before getting values";
                Logger.Error(error.Output);
                return error;
            }

            var namespaceAll = GetXmlNamespaces(_xmlDoc);
            var nsmgr = new XmlNamespaceManager(_xmlDoc.NameTable);

            if (namespaceAll != null)
            {
                foreach (var nss in namespaceAll)
                {
                    nsmgr.AddNamespace(nss.Key, nss.Value);
                }
            }
            XmlNode node = _xmlDoc.SelectSingleNode(xPath, nsmgr);

            if(node == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find xml node with given xpath";
                Logger.Error(fail.Output);
                return fail;
            }

            string value = node.Attributes[attributeName]?.Value ?? null;

            if(value == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find attribute in given node";
                Logger.Error(fail.Output);
                return fail;
            }

            CommonExecutionInfo.SetVariable(saveTo, value);
            KeywordResult pass = new KeywordResult(KeywordResults.Pass);
            pass.Output = value;
            return pass;
        }

        [GetKeyword]
        [KeywordDescription("Get inner text of xml node")]
        [KeywordDisplayName("Get Inner Text")]
        [KeywordParameters("saveTo", "name of variable which attribute value will be saved")]
        [KeywordParameters("xPath", "XPath of xml node")]
        [SampleScript("XML.Get Inner Text (${innerText},/testcase/result)")]
        public KeywordResult GetInnerText(string saveTo, string xPath)
        {
            xPath = Support.Utils.GetVariablevalueIfExist(xPath);
            if (_xmlDoc == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"XML is not loaded. Use Load or Load From File before getting values";
                Logger.Error(error.Output);
                return error;
            }

            var namespaceAll = GetXmlNamespaces(_xmlDoc);
            var nsmgr = new XmlNamespaceManager(_xmlDoc.NameTable);

            if (namespaceAll != null)
            {
                foreach (var nss in namespaceAll)
                {
                    nsmgr.AddNamespace(nss.Key, nss.Value);
                }
            }
            XmlNode node = _xmlDoc.SelectSingleNode(xPath, nsmgr);

            if (node == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find xml node with given xpath";
                Logger.Error(fail.Output);
                return fail;
            }

            string value = node.InnerText ?? null;

            if (value == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find inner text in given node";
                Logger.Error(fail.Output);
                return fail;
            }

            CommonExecutionInfo.SetVariable(saveTo, value);
            KeywordResult pass = new KeywordResult(KeywordResults.Pass);
            pass.Output = value;
            return pass;
        }

        [KeywordDescription("Replace Inner Text of xml node")]
        [KeywordDisplayName("Replace Inner Text")]
        [KeywordParameters("newText", "newText which will replace the oldvalue")]
        [KeywordParameters("xPath", "XPath of xml node")]
        [SampleScript("XML.Replace Inner Text (Fail,/testcase/result)")]
        public KeywordResult ReplaceInnerText(string newText, string xPath)
        {
            xPath = Support.Utils.GetVariablevalueIfExist(xPath);
            if (_xmlDoc == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"XML is not loaded. Use Load or Load From File before getting values";
                Logger.Error(error.Output);
                return error;
            }
            var namespaceAll = GetXmlNamespaces(_xmlDoc);
            var nsmgr = new XmlNamespaceManager(_xmlDoc.NameTable);

            if (namespaceAll != null)
            {
                foreach (var nss in namespaceAll)
                {
                    nsmgr.AddNamespace(nss.Key, nss.Value);
                }
            }
            XmlNode node = _xmlDoc.SelectSingleNode(xPath, nsmgr);
            if (node == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find xml node with given xpath";
                Logger.Error(fail.Output);
                return fail;
            }
            string value = node.InnerText ?? null;
            if (value == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find inner text in given node";
                Logger.Error(fail.Output);
                return fail;
            }
            node.InnerText = newText;
            KeywordResult pass = new KeywordResult(KeywordResults.Pass);
            pass.Output = value;
            return pass;
        }

        [KeywordDescription("Append the value to the inner text of xml node")]
        [KeywordDisplayName("Append To Inner Text")]
        [KeywordParameters("textToAppend", "value of the text which  has to appended")]
        [KeywordParameters("xPath", "XPath of xml node")]
        [SampleScript("XML.Append To Inner Text (not success,/testcase/result)")]
        public KeywordResult AppendToInnerText(string textToAppend, string xPath)
        {
            xPath = Support.Utils.GetVariablevalueIfExist(xPath);
            if (_xmlDoc == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"XML is not loaded. Use Load or Load From File before getting values";
                Logger.Error(error.Output);
                return error;
            }
            var namespaceAll = GetXmlNamespaces(_xmlDoc);
            var nsmgr = new XmlNamespaceManager(_xmlDoc.NameTable);
            if (namespaceAll != null)
            {
                foreach (var nss in namespaceAll)
                {
                    nsmgr.AddNamespace(nss.Key, nss.Value);
                }
            }
            XmlNode node = _xmlDoc.SelectSingleNode(xPath, nsmgr);
            if (node == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find xml node with given xpath";
                Logger.Error(fail.Output);
                return fail;
            }
            string value = node.InnerText ?? null;
            if (value == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find inner text in given node";
                Logger.Error(fail.Output);
                return fail;
            }
            _xmlDoc.SelectSingleNode(xPath, nsmgr).InnerText = value + textToAppend;
            KeywordResult pass = new KeywordResult(KeywordResults.Pass);
            pass.Output = value;
            return pass;
        }

        [KeywordDescription("Append new node at end of xPath ")]
        [KeywordDisplayName("AppendNode")]
        [KeywordParameters("xPath", "xPath")]
        [KeywordParameters("newNode", "Eg :- <NewNode>newtext</NewNode>")]
        [SampleScript("XML.AppendNode (/testcase,<IsSuccess>False</IsSuccess>)")]
        public KeywordResult AppendNode(string xPath, string newNode)
        {
            xPath = Support.Utils.GetVariablevalueIfExist(xPath);
            if (_xmlDoc == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"XML is not loaded. Use Load or Load From File before getting values";
                Logger.Error(error.Output);
                return error;
            }
            var namespaceAll = GetXmlNamespaces(_xmlDoc);
            var nsmgr = new XmlNamespaceManager(_xmlDoc.NameTable);
            if (namespaceAll != null)
            {
                foreach (var nss in namespaceAll)
                {
                    nsmgr.AddNamespace(nss.Key, nss.Value);
                }
            }
            XmlNode node = _xmlDoc.SelectSingleNode(xPath, nsmgr);
            if (node == null)
            {
                KeywordResult fail = new KeywordResult(KeywordResults.Fail);
                fail.Output = $"Can not find xml node with given xpath";
                Logger.Error(fail.Output);
                return fail;
            }
            XmlDocumentFragment xmlDocFragment = _xmlDoc.CreateDocumentFragment();
            xmlDocFragment.InnerXml = newNode;
            _xmlDoc.SelectSingleNode(xPath, nsmgr).AppendChild(xmlDocFragment);
            KeywordResult pass = new KeywordResult(KeywordResults.Pass);
            return pass;
        }

        [KeywordDescription("Save the file")]
        [KeywordDisplayName("Save File")]
        [KeywordParameters("FilePath", "FilePath of xml File")]
        [SampleScript("XML.Save File (C:\\Users\\BaPr519\\Desktop\\Work\\KeywordSampleScripts\\xml\\sample1.xml)")]
        public KeywordResult SaveFile(string FilePath)
        {
            // _xmlDoc already loaded
            if (_xmlDoc == null)
            {
                KeywordResult error = new KeywordResult(KeywordResults.Error);
                error.Output = $"XML is not loaded. Use Load or Load From File before getting values";
                Logger.Error(error.Output);
                return error;
            }
            _xmlDoc.Save(FilePath);
            KeywordResult pass = new KeywordResult(KeywordResults.Pass);
            return pass;
        }
    }
}
