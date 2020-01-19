using System.Linq;
using System.Security;

namespace HLV5r3.Extensions
{
  public static class ExcelApiExtensions
  {
    public static int CountChar(this string value, char ch)
    {
        return value.Count(t => t == ch);
    }

      public static string EscapeXml(this string value)
    {
      return SecurityElement.Escape(value);
    }

    public static object Default<T>()
    {
      return default(T);
    }

    public static int IndexOf<T>(this T [] values, T value)
    {
      int result = -1;
      for (int idx = 0; idx < values.Length ; idx++)
      {
        if (values[idx].Equals(value))
        {
          result = idx;
          break;
        }
      }
      return result;
    }

    public static int IndexOf(this char[] chars, char[] value)
    {
      int vdx = 0;
      int result = -1;
      int vlast = value.Length - 1;
      int cdx = 0;
      while (cdx < chars.Length)
      {
        if (chars[cdx] == value[vdx])
        {
          if (vdx == 0)
            result = cdx;
          else if (vdx == vlast)
            break;

          vdx++;
          cdx++;
        }
        else if (result != -1)
        {
          result = -1;
          vdx = 0;
        }
        else
        {
          vdx = 0;
          cdx++;
        }
      }

      return cdx == chars.Length ? -1 : result;
    }
  }
}
