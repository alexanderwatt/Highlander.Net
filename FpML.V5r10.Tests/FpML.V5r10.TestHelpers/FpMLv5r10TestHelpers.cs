/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Metadata.Common;

#endregion

namespace FpML.V5r10.TestHelpers
{
    /// <summary>
    /// 
    /// </summary>
    public class Counter
    {
        private int _counter;
        public int Count => Interlocked.Add(ref _counter, 0);
        public int Inc() { return Interlocked.Add(ref _counter, 1); }
    }

    /// <summary>
    /// 
    /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
    public class DebugLog : ITestLog
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="testId"></param>
        public void LogInfo(string text, string testId)
        {
            System.Diagnostics.Debug.Print("{2},[{0}] {1}", testId, text, "Info.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="testId"></param>
        /// <param name="fatal"></param>
        public void LogError(string text, string testId, bool fatal)
        {
            System.Diagnostics.Debug.Print("{2},[{0}] {1}", testId, text, fatal ? "Fatal" : "Error");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="excp"></param>
        /// <param name="testId"></param>
        /// <param name="fatal"></param>
        public void LogException(Exception excp, string testId, bool fatal)
        {
            System.Diagnostics.Debug.Print("{2},[{0}] {1}", testId, excp, fatal ? "Fatal" : "Error");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ITestLog
    {
        void LogInfo(string text, string testId);
        void LogError(string text, string testId, bool fatal);
        void LogException(Exception excp, string testId, bool fatal);
    }

    /// <summary>
    /// 
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="testId"></param>
        /// <param name="sourcePath"></param>
        /// <param name="outputPath"></param>
        /// <param name="errorCounter"></param>
        public static void FormatXml(ITestLog log, string testId,
            string sourcePath, string outputPath,
            Counter errorCounter)
        {
            log.LogInfo($"    FormatXml: sourcePath={sourcePath}", testId);
            log.LogInfo($"    FormatXml: outputPath={outputPath}", testId);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="testId"></param>
        /// <param name="xmlDocPath"></param>
        /// <param name="schemaSet"></param>
        /// <param name="errorCounter"></param>
        /// <param name="warningCounter"></param>
        public static void ValidateXml(
            ITestLog log, string testId,
            string xmlDocPath, XmlSchemaSet schemaSet,
            Counter errorCounter, Counter warningCounter)
        {
            log.LogInfo($"    ValidateXml: xmlDocPath={xmlDocPath}", testId);
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
                    log.LogError($"Validation {e.Severity}: {e.Message}", testId, false);
                    log.LogException(e.Exception, testId, false);
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="testId"></param>
        /// <param name="transformer"></param>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <param name="errorCounter"></param>
        public static void TransformXml(ITestLog log, string testId, IXmlTransformer transformer, string sourcePath, string targetPath, Counter errorCounter)
        {
            log.LogInfo($"    TransformXml: sourcePath={sourcePath}", testId);
            log.LogInfo($"    TransformXml: targetPath={targetPath}", testId);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="schemaSet"></param>
        /// <param name="typeDetector"></param>
        /// <param name="pattern"></param>
        /// <param name="transformIncomingViaXsl"></param>
        /// <param name="transformOutgoingViaXsl"></param>
        /// <param name="validateOriginal"></param>
        /// <param name="emitAndCompare"></param>
        /// <param name="validateEmitted"></param>
        /// <returns></returns>
        public static LoadCounters LoadFiles(
            string viewName, XmlSchemaSet schemaSet, Func<string, Type> typeDetector, string pattern,
            IXmlTransformer transformIncomingViaXsl, IXmlTransformer transformOutgoingViaXsl,
            bool validateOriginal, bool emitAndCompare, bool validateEmitted)
        {
            ITestLog testLog = new DebugLog();
            var counters = new LoadCounters();
            string sampleFilesPath = @"..\..\..\..\..\Metadata\FpML.V5r10\fpml.org\samples\5.10\" + viewName;
            // temp files
            string incomingFullPath = Path.GetFullPath(@"..\..\step1incoming.xml");
            string internalFullPath = Path.GetFullPath(@"..\..\step2internal.xml");
            string outgoingFullPath = Path.GetFullPath(@"..\..\step3outgoing.xml");
            string fullPath = Path.GetFullPath(sampleFilesPath);
            var fileNames = Directory.GetFiles(fullPath, pattern, SearchOption.AllDirectories);
            System.Diagnostics.Debug.Print("Loading {0} files from path: {1}", fileNames.Length, fullPath);
            for (int index = 0; index < fileNames.Length; index++)
            {
                string originalFilename = fileNames[index];
                string baseFilename = Path.GetFileName(originalFilename);
                string relativePath = Path.GetDirectoryName(originalFilename.Substring(fullPath.Length));
                string testId = $"File{index}";
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
                // compare internal documents
                CompareXml(testLog, testId, incomingFullPath, internalFullPath, counters.InternalComparisonErrors);
                // outgoing transformation
                TransformXml(testLog, testId, transformOutgoingViaXsl, internalFullPath, outgoingFullPath, counters.OutgoingTransformErrors);
                // compare external documents
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaSet"></param>
        /// <param name="typeDetector"></param>
        /// <param name="transformIncomingViaXsl"></param>
        /// <param name="transformOutgoingViaXsl"></param>
        /// <param name="validateOriginal"></param>
        /// <param name="emitAndCompare"></param>
        /// <param name="validateEmitted"></param>
        /// <returns></returns>
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
            const string sampleFilesPath = @"..\..\..\..\..\Metadata\FpML.V5r10\fpml.org\samples\5.10\Confirmation\products";
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
                string testId = $"File{index}";
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
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="testId"></param>
        /// <param name="typeDetector"></param>
        /// <param name="sourcePath"></param>
        /// <param name="errorCounter"></param>
        /// <returns></returns>
        public static object DeserialiseXml(ITestLog log, string testId, Func<string, Type> typeDetector, string sourcePath, Counter errorCounter)
        {
            log.LogInfo($"    DeserialiseXml: sourcePath={sourcePath}", testId);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="testId"></param>
        /// <param name="fpml"></param>
        /// <param name="outputPath"></param>
        /// <param name="errorCounter"></param>
        public static void SerialiseXml(ITestLog log, string testId, object fpml, string outputPath, Counter errorCounter)
        {
            log.LogInfo($"    SerialiseXml: outputPath={outputPath}", testId);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="testId"></param>
        /// <param name="pathA"></param>
        /// <param name="pathB"></param>
        /// <param name="errorCounter"></param>
        public static void CompareXml(ITestLog log, string testId, string pathA, string pathB, Counter errorCounter)
        {
            log.LogInfo($"    CompareXml: pathA={pathA}", testId);
            log.LogInfo($"    CompareXml: pathB={pathB}", testId);
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
