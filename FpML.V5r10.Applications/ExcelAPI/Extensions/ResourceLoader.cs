using System;
using System.IO;
using System.Reflection;

namespace HLV5r3.Extensions
{
  public static class ResourceLoader
  {
    public static string LoadString(Assembly asm, Type nameScope, string resourceName)
    {
      string result;
      string [] names = resourceName.Split(',');
      if (names.Length == 1)
      {
        if (string.Compare(nameScope.Namespace, 0, resourceName, 0, nameScope.Namespace.Length, true) == 0)
          result = LoadEmbeddedString(asm, resourceName);
        else
          result = LoadEmbeddedString(asm, nameScope.Namespace + "." + resourceName);
      }
      else
      {
        names[0] = names[0].Trim();
        names[1] = names[1].Trim();
        if (string.Compare(nameScope.Namespace, 0, names[0], 0, nameScope.Namespace.Length, true) == 0)
          result = LoadResourceString(asm, names[0], names[1]);
        else
          result = LoadResourceString(asm, nameScope.Namespace + "." + names[0], names[1]);
      }
      return result;
    }


    private static string LoadEmbeddedString(Assembly asm, string resourceName)
    {
      string result = "";
      using (Stream stream = asm.GetManifestResourceStream(resourceName))
      {
        if (stream != null)
        {
          using (TextReader reader = new StreamReader(asm.GetManifestResourceStream(resourceName)))
          {
            result = reader.ReadToEnd();
          }
        }
      }
      return result;
    }

    private static string LoadResourceString(Assembly asm, string resourceName, string itemName)
    {
      var m = new System.Resources.ResourceManager(resourceName, asm);
      return m.GetString(itemName);
    }
  }
}
