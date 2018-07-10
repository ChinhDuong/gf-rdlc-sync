using System;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.Reporting.WinForms;

namespace OxfordValley_SureMedPlusRDLC
{
    public class ReportGenerator
    {
        private static void PrepareXmlDocument(XmlDocument doc)
        {
            // Create an XmlNamespaceManager to resolve the default namespace.
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("nm", "http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition");
            nsmgr.AddNamespace("rd", "http://schemas.microsoft.com/SQLServer/reporting/reportdesigner");

            //Go through the nodes of XML document and localize the text of nodes Value, ToolTip, Label. 
            foreach (var nodeName in new[] { "Value", "ToolTip", "Label" })
                foreach (XmlNode node in doc.DocumentElement.SelectNodes(string.Format("//nm:{0}[@rd:LocID]", nodeName),
                    nsmgr))
                {
                    var nodeValue = node.InnerText;
                    if (string.IsNullOrEmpty(nodeValue) || !nodeValue.StartsWith("="))
                        try
                        {
                            var localizedValue = Linguist.Phrase(node.Attributes["rd:LocID"].Value);
                            if (!string.IsNullOrEmpty(localizedValue))
                                node.InnerText = localizedValue;
                        }
                        catch (InvalidCastException)
                        {
                            // if the specified resource is not a String
                        }
                }
        }

        public static void GenerateReportByLanguageCode(LocalReport report, string[] subReportPath)
        {
            var doc = new XmlDocument();

            doc.Load(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("OxfordValley_SureMedPlusRDLC.Rdlc.SureMedReport2ColsView.rdlc"));

            PrepareXmlDocument(doc);

            report.ReportPath = string.Empty;

            using (var rdlcOutputStream = new StringReader(doc.DocumentElement.OuterXml))
            {
                report.LoadReportDefinition(rdlcOutputStream);
            }

            foreach (var sub in subReportPath)
                if (sub != "")
                {
                    var docSubReport = new XmlDocument();
                    var subPath = "OxfordValley_SureMedPlusRDLC.Rdlc." + sub + ".rdlc";
                    docSubReport.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream(subPath));

                    PrepareXmlDocument(docSubReport);

                    using (var rdlcSubReportOutputStream = new StringReader(docSubReport.DocumentElement.OuterXml))
                    {
                        report.LoadSubreportDefinition(sub, rdlcSubReportOutputStream);
                    }
                }
        }
    }
}