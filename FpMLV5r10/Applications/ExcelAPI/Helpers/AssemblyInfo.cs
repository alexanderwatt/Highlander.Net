using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Orion.Util.Helpers;

namespace HLV5r3.Helpers
{
    [Serializable]
    internal class AssemblyInfo
    {
        #region Private Fields

        readonly Assembly _assembly;
        readonly AssemblyName _assemblyName;
        readonly FileVersionInfo _fileVersionInfo;
        List<string> _dictionaryFields;
        IDictionary<string, string> _assemblyDetailsDictionary;

        #endregion

        #region Public Accessors

        /// <summary>
        /// Gets the assembly.
        /// </summary>
        /// <value>The assembly.</value>
        public Assembly Assembly => _assembly;

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name => _assemblyName.Name;

        /// <summary>
        /// Gets the name of the code base.
        /// </summary>
        /// <value>The name of the code base.</value>
        public string CodeBaseName => Path.GetFileName(_assembly.CodeBase);

        /// <summary>
        /// Gets the code base.
        /// </summary>
        /// <value>The code base.</value>
        public string CodeBase
        {
            get 
            {   string c = $@"{CodeBaseLocation}\{CodeBaseName}";
                return c;
            }
        }

        /// <summary>
        /// Gets the code base location.
        /// </summary>
        /// <value>The code base location.</value>
        public string CodeBaseLocation => GetAssemblyCodeBaseLocation(Assembly);

        /// <summary>
        /// Gets the assembly version.
        /// </summary>
        /// <value>The assembly version.</value>
        public string AssemblyVersion => _assemblyName.Version.ToString();

        /// <summary>
        /// Gets the file version.
        /// </summary>
        /// <value>The file version.</value>
        public string FileVersion => _fileVersionInfo.FileVersion;

        /// <summary>
        /// Gets a value indicating whether this instance has strong name.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has strong name; otherwise, <c>false</c>.
        /// </value>
        public Boolean HasStrongName => _assemblyName.GetPublicKey().Length > 0;

        /// <summary>
        /// Gets the public key token.
        /// </summary>
        /// <value>The public key token.</value>
        public string PublicKeyToken => GetPublicKeyToken(_assemblyName);

        /// <summary>
        /// Gets the build date.
        /// </summary>
        /// <value>The build date.</value>
        public DateTime BuildDate => File.GetCreationTime(CodeBaseLocation);

        /// <summary>
        /// Gets the dictionary fields.
        /// </summary>
        /// <value>The dictionary fields.</value>
        public string[] DictionaryFields
        {
            get
            {
                if (_dictionaryFields == null)
                {
                    List<string> fields = new List<string>();
                    foreach (PropertyInfo p in GetType().GetProperties())
                    {
                        if (p.PropertyType.IsPrimitive || p.PropertyType == typeof(string) && p.GetIndexParameters().Length == 0)
                        {
                            fields.Add(p.Name);
                        }
                    }
                    fields.Sort();
                    _dictionaryFields = fields;
                }

                return _dictionaryFields.ToArray();
            }
        }

        #endregion

        #region Public Indexor

        /// <summary>
        /// Gets the <see cref="System.String"/> with the specified dictionary field.
        /// </summary>
        /// <value></value>
        public string this [string dictionaryField]
        {
            get
            {
                string result = null ;
                IDictionary<string, string> items = ToDict();

                List<string> keys = new List<string>(items.Keys);
                string matchedKey = keys.Find(item => String.Compare(item, dictionaryField, StringComparison.OrdinalIgnoreCase) == 0);

                if (!string.IsNullOrEmpty(matchedKey))
                {
                    result = items[matchedKey];
                }
                return result;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyInfo"/> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public AssemblyInfo(Assembly assembly)
        {
            _assembly = assembly;
            _assemblyName = assembly.GetName();
            _fileVersionInfo = FileVersionInfo.GetVersionInfo(CodeBase);
        }

        #endregion

        #region Public Behaviour

        public object[,] ToArray()
        {
            object[,] values = ArrayHelper.ConvertDictionaryTo2DArray(ToDict());
            return values;
        }

        /// <summary>
        /// Toes the array.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public object[,] ToArray(Assembly assembly)
        {
            return ArrayHelper.ConvertDictionaryTo2DArray(ToDict(assembly));
        }

        /// <summary>
        /// Toes the dict.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> ToDict()
        {
            return _assemblyDetailsDictionary ?? (_assemblyDetailsDictionary = AssemblyDetails(this));
        }

        #endregion

        #region Static Helpers

        /// <summary>
        /// Toes the dict.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static IDictionary<string, string> ToDict(Assembly assembly)
        {
            AssemblyInfo info = new AssemblyInfo(assembly);
            return AssemblyDetails(info);
        }

        /// <summary>
        /// Gets the assembly code base location.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static string GetAssemblyCodeBaseLocation(Assembly assembly)
        {
            string assemblyBasePath = Path.GetDirectoryName(assembly.CodeBase);
            assemblyBasePath = assemblyBasePath.Replace(@"file:\", "");
            return assemblyBasePath;
        }

        /// <summary>
        /// Assemblies the details.
        /// </summary>
        /// <param name="assemblyInfo">The assembly info.</param>
        /// <returns></returns>
        public static IDictionary<string, string> AssemblyDetails(AssemblyInfo assemblyInfo)
        {
            IDictionary<string, string> results = new Dictionary<string, string>();
            foreach(string field in assemblyInfo.DictionaryFields)
            {
                results.Add(field, ObjectLookupHelper.GetPropertyValue(assemblyInfo, field).ToString());
            }
            return results;
        }

        /// <summary>
        /// Gets the public key token.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static string GetPublicKeyToken(Assembly assembly)
        {
            return GetPublicKeyToken(assembly.GetName());
        }

        /// <summary>
        /// Gets the public key token.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        public static string GetPublicKeyToken(AssemblyName assemblyName)
        {
            StringBuilder sb = new StringBuilder();
            Byte[] pkt = assemblyName.GetPublicKeyToken();
            foreach (byte t in pkt)
            {
                sb.Append(t.ToString("x"));
            }
            return sb.ToString();
        }

        #endregion

    }
}