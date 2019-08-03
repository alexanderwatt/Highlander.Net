/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using FpML.V5r3.Codelist;
using Orion.Util.Helpers;
using Orion.Util.Serialisation;

#endregion

namespace FpMLCodeListTool
{
    class Program
    {
        private static string GetClassDefName(string schemeName)
        {
            string result;
            if (schemeName.EndsWith("Scheme"))
                result = schemeName.Substring(0, (schemeName.Length - 6));
            else
                throw new ApplicationException(
                    $"Cannot derive class def name from '{schemeName}'.");
            result = result[0].ToString(CultureInfo.InvariantCulture).ToUpper() + result.Substring(1);
            return result;
        }
        static void Main(string[] args)
        {
            Console.WriteLine("FpMLCodeListTool: Starting...");
            var outerIterator = new TemplateIterator(1);
            outerIterator.Iterations[0].Tokens.Set("NameSpace", "FpML.V5r3.Codes");
            outerIterator.Iterations[0].Tokens.Set("ToolTitle", "TemplateProcessor");
            outerIterator.Iterations[0].Tokens.Set("ToolVersion", "1.1.0.0");
            Console.WriteLine("FpMLCodeListTool: Parsing code lists...");
            string[] xmlFiles = Directory.GetFiles(@"..\..\..\..\FpML.V5r3\fpml.org\download\codelist", "*.xml", SearchOption.AllDirectories);
            var classIterator = new TemplateIterator(xmlFiles.Length);
            outerIterator.Iterations[0].SubIterators["ClassDef"] = classIterator;
            int fileNum = 0;
            foreach (string xmlFile in xmlFiles)
            {
                try
                {
                    //Console.WriteLine("FpMLCodeListTool:   Parsing '{0}' ...", xmlFile);
                    var data = XmlSerializerHelper.DeserializeFromFile<CodeListDocument>(xmlFile);
                    string classDefName = GetClassDefName(data.Identification.ShortName);
                    // determine primary key
                    string primaryKey = null;
                    foreach (Key key in data.ColumnSet.Key)
                    {
                        if (key.Id == "PrimaryKey")
                        {
                            if (primaryKey != null)
                                throw new ApplicationException("PrimaryKey defined more than once!");
                            primaryKey = key.ColumnRef[0].Ref;
                        }
                    }
                    if (primaryKey == null)
                        throw new ApplicationException("PrimaryKey is not defined!");

                    classIterator.Iterations[fileNum].Tokens.Set("ClassDef", classDefName);
                    classIterator.Iterations[fileNum].Tokens.Set("PrimaryKey", primaryKey);
                    classIterator.Iterations[fileNum].Tokens.Set("ClassDefDataFile", Path.GetFileNameWithoutExtension(xmlFile));
                    // subiterator - column (field) definitions
                    var fieldIterator = new TemplateIterator(data.ColumnSet.Column.Length);
                    classIterator.Iterations[fileNum].SubIterators["FieldDef"] = fieldIterator;
                    int colNum = 0;
                    foreach (Column col in data.ColumnSet.Column)
                    {
                        fieldIterator.Iterations[colNum].Tokens.Set("ColumnNum", colNum);
                        fieldIterator.Iterations[colNum].Tokens.Set("ColumnName", col.ShortName);
                        fieldIterator.Iterations[colNum].Tokens.Set("XSDataType", col.Data.Type);
                        // next column
                        colNum++;
                    }
                    // subiterator - row (primary key/enum) definitions
                    var valueIterator = new TemplateIterator(data.SimpleCodeList.Row.Length);
                    classIterator.Iterations[fileNum].SubIterators["ValueDef"] = valueIterator;
                    int rowNum = 0;
                    foreach (Row row in data.SimpleCodeList.Row)
                    {
                        valueIterator.Iterations[rowNum].Tokens.Set("RowNum", rowNum + 1); // note: 0 = Undefined
                        string value = row.Value[0].SimpleValue.Value;
                        valueIterator.Iterations[rowNum].Tokens.Set("RowName", value);
                        string enumName = EnumHelper.ToEnumId(value);
                        valueIterator.Iterations[rowNum].Tokens.Set("RowEnum", enumName);
                        //valueIterator.Iterations[rowNum].Tokens.Set("XSDataType", row.Data.Type);
                        // next row
                        rowNum++;
                    }
                }
                catch (Exception excp)
                {
                    Console.Error.WriteLine("FpMLCodeListTool:   Parse '{0}' failed: {1}", xmlFile, excp);
                }
                // next file
                fileNum++;
            }
            // write XML schema definitions
            Console.WriteLine("FpMLCodeListTool: Writing schema definition...");
            {
                const string templateFileName = @"TemplateCodeList.txt";
                outerIterator.Iterations[0].Tokens.Set("TemplateFile", templateFileName);
                string text = ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), templateFileName);
                string[] input = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                var tp = new TemplateProcessor {CommentPrefix = "<!--", CommentSuffix = "-->"};
                string[] output = tp.ProcessTemplate(input, outerIterator);
                using (StreamWriter f = File.CreateText("FpMLCodes.xsd"))
                {
                    foreach (string line in output)
                    {
                        f.WriteLine(line);
                    }
                    f.Flush();
                }
            }
            // write class extensions
            Console.WriteLine("FpMLCodeListTool: Writing class extensions...");
            {
                const string templateFileName = @"TemplateExtensions.txt";
                outerIterator.Iterations[0].Tokens.Set("TemplateFile", templateFileName);
                string text = ResourceHelper.GetResourceWithPartialName(Assembly.GetExecutingAssembly(), templateFileName);
                string[] input = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var tp = new TemplateProcessor {CommentPrefix = "//", CommentSuffix = null};
                string[] output = tp.ProcessTemplate(input, outerIterator);
                using (StreamWriter f = File.CreateText("FpMLCodesExt.cs"))
                {
                    foreach (string line in output)
                    {
                        f.WriteLine(line);
                    }
                    f.Flush();
                }
            }
            Console.WriteLine("FpMLCodeListTool: Complete.");
        }
    }
}
