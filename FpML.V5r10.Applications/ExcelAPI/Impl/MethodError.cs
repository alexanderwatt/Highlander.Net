using System;

namespace HLV5r3.Impl
{
  [Serializable]
  public class MethodError
  {
    public bool AsWarning = false;
    public string DomainName;
    public string Message;
    public string StackTrace;
    public string Tag;
  }
}
