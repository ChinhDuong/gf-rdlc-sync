using BJRX_v4_basic_SureMedPlusRdlc.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;

namespace BJRX_v4_basic_SureMedPlusRdlc
{
    public class Linguist
    {
        private static readonly Dictionary<string, XmlDocument> languageCache = new Dictionary<string, XmlDocument>();

        public static string Phrase(string keyName)
        {
            var value = keyName;
            string languageId = "";            
            string language = RDLCReportView.languageCode;
            if (language != "" && language.Length > 2) languageId = language.Substring(0, 2);
            if (keyName.Trim() == "") return value;

            try
            {
                var xmldoc = new XmlDocument();

                if (languageCache.ContainsKey(languageId))
                {
                    xmldoc = languageCache[languageId];
                }
                else
                {
                    var xmlFilePath = "BJRX_v4_basic_SureMedPlusRdlc.Languages." + languageId + ".xml";
                    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(xmlFilePath);
                    if (stream != null)
                    {
                        xmldoc.Load(stream);
                        languageCache.Add(languageId, xmldoc);
                    }
                    else
                    {
                        var xmlFilePath1 = "BJRX_v4_basic_SureMedPlusRdlc.Languages.en.xml";
                        var stream1 = Assembly.GetExecutingAssembly().GetManifestResourceStream(xmlFilePath1);
                        if (stream1 != null)
                        {
                            xmldoc.Load(stream1);
                            languageCache.Add(languageId, xmldoc);
                        }
                        else
                        {
                            return keyName;
                        }
                    }
                }

                var xpathExpression = new StringBuilder();
                xpathExpression.Append("//phrases//phrase[@id='");
                xpathExpression.Append(keyName);
                xpathExpression.Append("']");

                var node = xmldoc.SelectSingleNode(xpathExpression.ToString());
                if (node != null)
                    value = node.InnerText;

                if (value == "") value = keyName;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return value;
        }
    }
}