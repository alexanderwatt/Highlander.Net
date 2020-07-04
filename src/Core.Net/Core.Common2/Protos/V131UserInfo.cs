using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Highlander.Core.Common;
using System;

namespace Highlander.Grpc.Session
{
    public partial class V131UserInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userIdentityName"></param>
        /// <param name="userFullName"></param>
        public V131UserInfo(string userIdentityName, string userFullName)
        {
            if (userIdentityName == null)
                throw new ArgumentNullException(nameof(userIdentityName));
            if (userIdentityName.Split('\\').Length != 2)
                throw new ArgumentException("userIdentityName not in domain\\loginid format!");

            UserIdentityName = userIdentityName;
            UserFullName = userFullName;
        }
    }
}
