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

using System.Xml.Serialization;

namespace Highlander.Orc.Messages
{
    public abstract class Security : SendMessage
    {
        protected Security(MessageTypes messageType)
            : base(messageType)
        {
        }

        protected Security(MessageTypes messageType, string userName, string password)
            : base(messageType)
        {
            UserName = userName;
            Password = password;
        }

        [XmlElement(ElementName = "login_id")]
        public string UserName { get; set; }

        [XmlElement(ElementName = "password")]
        public string Password { get; set; }

        public string SubsID
        {
            set => Header.PrivateKey = value;
        }

    }

    [XmlRootAttribute(ElementName = "orc_message")]
    public class Login : Security
    {

        public Login()
            : base(MessageTypes.login)
        {

        }

        public Login(string userName, string password) : base(MessageTypes.login)
        {
            UserName = userName;
            Password = password;  
        
        }

        public new string SubsID
        {
            set => Header.PrivateKey = value;
        }
            
        //MessageHeader _header = new MessageHeader(MessageTypes.login);

        //[XmlElement(ElementName = "message_info")]
        //public MessageHeader Header
        //{
        //    get { return _header; }
        //    set { _header = value; }
        //}

        [XmlElement(ElementName = "login_id")]
        public new string UserName { get; set; }

        [XmlElement(ElementName = "password")]
        public new string Password { get; set; }
    }

    [XmlRootAttribute(ElementName = "orc_message")]
    public class Logout : Security
    {

        public Logout()
            : base(MessageTypes.logout)
        {

        }
    
        public Logout(string userName) :base(MessageTypes.logout)
        {
            UserName = userName;
        }

        [XmlElement(ElementName = "message_info")]
        public new MessageHeader Header { get; set; } = new MessageHeader(MessageTypes.logout);


        [XmlElement(ElementName = "login_id")]
        public new string UserName { get; set; }
    }

    public class LoginResult : ReplyMessage
    {
        #region Declarations

        #endregion

        #region Properties

        [XmlElement(ElementName = "login_id")]
        public string UserName { get; set; }

        [XmlElement(ElementName = "version")]
        public string Version { get; set; }

        [XmlElement(ElementName = "utc_offset")]
        public int UTCOffset { get; set; }

        #endregion

        #region Constructors and Initialise
        public LoginResult(string version, int utcOffset)
            : base(MessageTypes.LOGIN_REPLY)
        {
            Version = version;
            UTCOffset = utcOffset;
        }

        public LoginResult()
            : base(MessageTypes.LOGIN_REPLY)
        {
        }

        #endregion
    }
}
