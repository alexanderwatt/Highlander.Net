using System.Diagnostics;
using System.Reflection;

namespace Highlander.Grpc.Session
{
    public partial class V131AssmInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        public V131AssmInfo(Assembly assembly)
        {
            if (assembly.FullName != null)
            {
                string[] parts = assembly.FullName.Split(',');
                assmName_ = parts[0];
                assmNVer_ = parts[1].Split('=')[1];
                //AssmCult = parts[2].Split('=')[1];
                assmPTok_ = parts[3].Split('=')[1];
            }
            assmHash_ = assembly.GetHashCode();
            foreach (object attr in assembly.GetCustomAttributes(true))
            {
                if (attr is AssemblyFileVersionAttribute attribute)
                    assmFVer_ = attribute.Version;
                if (attr is AssemblyInformationalVersionAttribute versionAttribute)
                    assmIVer_ = versionAttribute.InformationalVersion;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="module"></param>
        public V131AssmInfo(ProcessModule module)
        {
            assmName_ = module.ModuleName.Split('.')[0];
            assmNVer_ = "1.0.0.0";
            assmPTok_ = "null";
            assmFVer_ = module.FileVersionInfo.FileVersion;
            assmIVer_ = module.FileVersionInfo.ProductVersion;
        }
    }
}
