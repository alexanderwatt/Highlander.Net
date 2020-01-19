/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.VisualStudio.DebuggerVisualizers;

#endregion

namespace Highlander.NamedValueSet.DebuggerVisualizer
{
    public class DebuggerSide : DialogDebuggerVisualizer
    {
        //private const string _assemblyName = "__QRFpML.XmlSerializer.dll";
        
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            var visualizerForm = new NamedValueSetVisualizerForm(objectProvider.GetObject() as Utilities.NamedValues.NamedValueSet);
            windowService.ShowDialog(visualizerForm);
        }

        #region Private methods

        //TODO: revert to this (efficient) implementation later
        //
        private static XmlSerializer GetOrCreateXmlSerializer(Type toSerialize)
        {
            var serializer = new XmlSerializer(toSerialize);
            return serializer;

//            // The idea is to create/load xml serializer autogenerate in the same location next to the visualizer assembly.
//            //
//            string visualizerAssemblyLocation = Assembly.GetExecutingAssembly().Location;
//            string xmlSerializerAssemblyLocation = Path.Combine(Path.GetDirectoryName(visualizerAssemblyLocation), _assemblyName);
//
//            // check if assembly already exists...
//            //
//            Assembly xmlSerializerAssembly;
//
//            // If the serializer assembly exists
//            //
//            if (File.Exists(xmlSerializerAssemblyLocation))
//            {
//                // If it's already exists (been created before) - load it from file.
//                //
//                xmlSerializerAssembly = Assembly.LoadFrom(xmlSerializerAssemblyLocation);
//            }
//            else
//            {
//                // otherwise - create it at the specified location.
//                //
//                Type swapType = typeof(Swap);//any type from QRFpML assembly will do.
//                Assembly fpmlAssembly = swapType.Assembly;
//                Type[] typesDefinedInAssembly = fpmlAssembly.GetTypes();
//
//                xmlSerializerAssembly = GenerateSerializer(typesDefinedInAssembly, xmlSerializerAssemblyLocation);
//            }
//
//            Debug.Assert(null != xmlSerializerAssembly);
//            XmlSerializer xmlSerializer = GetXmlSerializer(xmlSerializerAssembly, toSerialize);
//            Debug.Assert(null != xmlSerializer);
//            
//            return xmlSerializer;
        }

        private static XmlSerializer GetXmlSerializer(Assembly assembly, Type typeToSerialize)
        {
            Debug.Assert(null != assembly);
            Type serializerType = assembly.GetType("Microsoft.Xml.Serialization.GeneratedAssembly.XmlSerializerContract");
            Debug.Assert(null != serializerType);
            var xsi = (XmlSerializerImplementation)Activator.CreateInstance(serializerType);
            Debug.Assert(null != xsi);
            XmlSerializer xmlSerializer = xsi.GetSerializer(typeToSerialize);
            Debug.Assert(null != xmlSerializer);
            return xmlSerializer;
        }

        private static Assembly GenerateSerializer(Type[] types, string outputAssemblyLocation)
        {
            var parameters = new CompilerParameters {GenerateInMemory = false, OutputAssembly = outputAssemblyLocation};
            var importer = new XmlReflectionImporter();
            Converter<Type, XmlMapping> converter = importer.ImportTypeMapping;
            Assembly assembly =  XmlSerializer.GenerateSerializer(types, Array.ConvertAll(types, converter), parameters);
            Debug.Assert(null != assembly);
            return assembly;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectToVisualize"></param>
        public static void TestShowVisualizer(object objectToVisualize)
        {
            var visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(DebuggerSide));
            visualizerHost.ShowVisualizer();
        }
    }
}
