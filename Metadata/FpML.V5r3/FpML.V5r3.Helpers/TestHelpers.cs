using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace FpML.V5r3.Helpers
{
    public class Counter
    {
        private int _counter;
        public int Count { get { return Interlocked.Add(ref _counter, 0); } }
        public int Inc() { return Interlocked.Add(ref _counter, 1); }
    }

    public class LoadCounters
    {
        public readonly Counter FilesProcessed = new Counter();
        public readonly Counter DeserialisationErrors = new Counter();
        public readonly Counter SerialisationErrors = new Counter();
        public readonly Counter OrigValidationErrors = new Counter();
        public readonly Counter OrigValidationWarnings = new Counter();
        public readonly Counter IncomingTransformErrors = new Counter();
        public readonly Counter OutgoingTransformErrors = new Counter();
        public readonly Counter CopyValidationWarnings = new Counter();
        public readonly Counter CopyValidationErrors = new Counter();
        public readonly Counter InternalComparisonErrors = new Counter();
        public readonly Counter ExternalComparisonErrors = new Counter();
    }

    public class DebugLog : ITestLog
    {
        public void LogInfo(string text, string testId)
        {
            System.Diagnostics.Debug.Print("{2},[{0}] {1}", testId, text, "Info.");
        }

        public void LogError(string text, string testId, bool fatal)
        {
            System.Diagnostics.Debug.Print("{2},[{0}] {1}", testId, text, fatal ? "Fatal" : "Error");
        }

        public void LogException(Exception excp, string testId, bool fatal)
        {
            System.Diagnostics.Debug.Print("{2},[{0}] {1}", testId, excp, fatal ? "Fatal" : "Error");
        }
    }

    public interface ITestLog
    {
        void LogInfo(string text, string testId);
        void LogError(string text, string testId, bool fatal);
        void LogException(Exception excp, string testId, bool fatal);
    }

    public static class TestHelper
    {
        public static void FormatXml(ITestLog log, string testId,
            string sourcePath, string outputPath,
            Counter errorCounter)
        {
            log.LogInfo(String.Format("    FormatXml: sourcePath={0}", sourcePath), testId);
            log.LogInfo(String.Format("    FormatXml: outputPath={0}", outputPath), testId);
            try
            {
                var sampleDoc = new XmlDocument();
                using (var xr = new XmlTextReader(sourcePath))
                {
                    sampleDoc.Load(xr);
                }
                var xws = new XmlWriterSettings {Indent = true, Encoding = Encoding.UTF8};
                using (XmlWriter xw = XmlWriter.Create(outputPath, xws))
                {
                    sampleDoc.Save(xw);
                }
            }
            catch (Exception excp)
            {
                errorCounter.Inc();
                log.LogException(excp, testId, false);
            }
        }

        public static void ValidateXml(
            ITestLog log, string testId,
            string xmlDocPath, XmlSchemaSet schemaSet,
            Counter errorCounter, Counter warningCounter)
        {
            log.LogInfo(String.Format("    ValidateXml: xmlDocPath={0}", xmlDocPath), testId);
            //log.LogInfo(String.Format("    ValidateXml: outputPath={0}", outputPath ?? "(null)"), testId);
            using (var stream = new FileStream(xmlDocPath, FileMode.Open, FileAccess.Read))
            {
                var sampleDoc = new XmlDocument();
                sampleDoc.Schemas.Add(schemaSet);
                sampleDoc.Load(stream);
                sampleDoc.Validate((o, e) =>
                {
                    if (e.Severity == XmlSeverityType.Error)
                        errorCounter.Inc();
                    else
                        warningCounter.Inc();
                    log.LogError(String.Format("Validation {0}: {1}", e.Severity, e.Message), testId, false);
                    log.LogException(e.Exception, testId, false);
                });
            }
        }

        public static void TransformXml(ITestLog log, string testId, IXmlTransformer transformer, string sourcePath, string targetPath, Counter errorCounter)
        {
            log.LogInfo(String.Format("    TransformXml: sourcePath={0}", sourcePath), testId);
            log.LogInfo(String.Format("    TransformXml: targetPath={0}", targetPath), testId);
            try
            {
                transformer.Transform(sourcePath, targetPath);
            }
            catch (Exception excp)
            {
                errorCounter.Inc();
                log.LogException(excp, testId, false);
            }
        }

        public static LoadCounters LoadFiles(
            string viewName, XmlSchemaSet schemaSet, Func<string, Type> typeDetector, string pattern,
            IXmlTransformer transformIncomingViaXsl, IXmlTransformer transformOutgoingViaXsl,
            bool validateOriginal, bool emitAndCompare, bool validateEmitted)
        {
            ITestLog testLog = new DebugLog();
            var counters = new LoadCounters();
            
            //string schemaPath = @"..\..\..\nab.QR.FpML.V5r3\" + viewName + ".xsd";
            //string schemaFullPath = Path.GetFullPath(schemaPath);
            //if (!File.Exists(schemaFullPath))
            //    throw new FileNotFoundException("Schema missing!", schemaFullPath);

            string sampleFilesPath = @"..\..\..\..\..\Metadata\FpML.V5r3\fpml.org\samples\5.3\" + viewName;

            // temp files
            string incomingFullPath = Path.GetFullPath(@"..\..\step1incoming.xml");
            string internalFullPath = Path.GetFullPath(@"..\..\step2internal.xml");
            string outgoingFullPath = Path.GetFullPath(@"..\..\step3outgoing.xml");

            string fullPath = Path.GetFullPath(sampleFilesPath);
            var filenames = Directory.GetFiles(fullPath, pattern, SearchOption.AllDirectories);
            System.Diagnostics.Debug.Print("Loading {0} files from path: {1}", filenames.Length, fullPath);
            for (int index = 0; index < filenames.Length; index++)
            {
                string originalFilename = filenames[index];
                string baseFilename = Path.GetFileName(originalFilename);
                string relativePath = Path.GetDirectoryName(originalFilename.Substring(fullPath.Length));
                string testId = String.Format("File{0}", index);

                System.Diagnostics.Debug.Print("[{0}] {1} ({2}) processing...", index, baseFilename, relativePath);
                counters.FilesProcessed.Inc();

                // validation
                if (validateOriginal)
                {
                    ValidateXml(testLog, testId, originalFilename, schemaSet, counters.OrigValidationErrors, counters.OrigValidationWarnings);
                }

                // incoming transformation
                TransformXml(testLog, testId, transformIncomingViaXsl, originalFilename, incomingFullPath, counters.IncomingTransformErrors);

                // deserialise
                object fpml = DeserialiseXml(testLog, testId, typeDetector, incomingFullPath, counters.DeserialisationErrors);

                // serialise
                SerialiseXml(testLog, testId, fpml, internalFullPath, counters.SerialisationErrors);

                // compare internal docuemnts
                CompareXml(testLog, testId, incomingFullPath, internalFullPath, counters.InternalComparisonErrors);

                // outgoing transformation
                TransformXml(testLog, testId, transformOutgoingViaXsl, internalFullPath, outgoingFullPath, counters.OutgoingTransformErrors);

                // compare external docuemnts
                if (emitAndCompare)
                {
                    CompareXml(testLog, testId, originalFilename, outgoingFullPath, counters.ExternalComparisonErrors);
                }

                if (validateEmitted)
                {
                    ValidateXml(testLog, testId, outgoingFullPath, schemaSet, counters.CopyValidationErrors, counters.CopyValidationWarnings);
                }

            }
            return counters;
        }


        public static List<object> LoadAndReturnConfirmationExamples(
            XmlSchemaSet schemaSet, Func<string, Type> typeDetector,
            IXmlTransformer transformIncomingViaXsl, IXmlTransformer transformOutgoingViaXsl,
            bool validateOriginal, bool emitAndCompare, bool validateEmitted)
        {
            ITestLog testLog = new DebugLog();
            var counters = new LoadCounters();
            var result = new List<object>();
            //string schemaPath = @"..\..\..\nab.QR.FpML.V5r3\" + viewName + ".xsd";
            //string schemaFullPath = Path.GetFullPath(schemaPath);
            //if (!File.Exists(schemaFullPath))
            //    throw new FileNotFoundException("Schema missing!", schemaFullPath);
            const string fileSpec = "*.xml";
            const string sampleFilesPath = @"..\..\..\..\..\Metadata\FpML.V5r3\fpml.org\samples\5.3\Confirmation\products";

            // temp files
            string incomingFullPath = Path.GetFullPath(@"..\..\step1incoming.xml");

            string fullPath = Path.GetFullPath(sampleFilesPath);
            var filenames = Directory.GetFiles(fullPath, fileSpec, SearchOption.AllDirectories);
            System.Diagnostics.Debug.Print("Loading {0}", filenames.Length);
            for (int index = 0; index < filenames.Length; index++)
            {
                string originalFilename = filenames[index];
                //string baseFilename = Path.GetFileName(originalFilename);
                //string relativePath = Path.GetDirectoryName(originalFilename.Substring(fullPath.Length));
                string testId = String.Format("File{0}", index);

                //System.Diagnostics.Debug.Print("[{0}] {1} ({2}) processing...", index, baseFilename, relativePath);
                counters.FilesProcessed.Inc();

                // validation
                //if (validateOriginal)
                //{
                //    ValidateXml(testLog, testId, originalFilename, schemaSet, counters.OrigValidationErrors, counters.OrigValidationWarnings);
                //}

                // incoming transformation
                TransformXml(testLog, testId, transformIncomingViaXsl, originalFilename, incomingFullPath, counters.IncomingTransformErrors);

                // deserialise
                object fpml = DeserialiseXml(testLog, testId, typeDetector, incomingFullPath, counters.DeserialisationErrors);
                result.Add(fpml);
            }
            return result;
        }

        
        public static object DeserialiseXml(ITestLog log, string testId, Func<string, Type> typeDetector, string sourcePath, Counter errorCounter)
        {
            log.LogInfo(String.Format("    DeserialiseXml: sourcePath={0}", sourcePath), testId);
            try
            {
                Type autoDetectedType = typeDetector(sourcePath);
                var xs = new XmlSerializer(autoDetectedType);
                using (var xr = new XmlTextReader(sourcePath))
                {
                    return xs.Deserialize(xr);
                }
            }
            catch (Exception excp)
            {
                errorCounter.Inc();
                log.LogException(excp, testId, false);
                return null;
            }
        }

        public static void SerialiseXml(ITestLog log, string testId, object fpml, string outputPath, Counter errorCounter)
        {
            log.LogInfo(String.Format("    SerialiseXml: outputPath={0}", outputPath), testId);
            try
            {
                var xs = new XmlSerializer(fpml.GetType());
                var xws = new XmlWriterSettings {Indent = true, Encoding = Encoding.UTF8};
                using (var xw = XmlWriter.Create(outputPath, xws))
                //using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                //using (var xw = new XmlTextWriter(fs, Encoding.UTF8))
                {
                    xs.Serialize(xw, fpml);
                }
            }
            catch (Exception excp)
            {
                errorCounter.Inc();
                log.LogException(excp, testId, false);
            }
        }

        public static void CompareXml(ITestLog log, string testId, string pathA, string pathB, Counter errorCounter)
        {
            log.LogInfo(String.Format("    CompareXml: pathA={0}", pathA), testId);
            log.LogInfo(String.Format("    CompareXml: pathB={0}", pathB), testId);
            try
            {
                // load original into xml doc
                var docA = new XmlDocument();
                using (var fs = new FileStream(pathA, FileMode.Open, FileAccess.Read))
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    docA.Load(sr);
                }

                // load emitted stream into xml doc
                var docB = new XmlDocument();
                using (var stream = new FileStream(pathB, FileMode.Open, FileAccess.Read))
                {
                    docB.Load(stream);
                }

                // compare
                XmlCompare.CompareXmlDocs(docA, docB);
            }
            catch (Exception excp)
            {
                errorCounter.Inc();
                log.LogException(excp, testId, false);
            }
        }

    }
}
