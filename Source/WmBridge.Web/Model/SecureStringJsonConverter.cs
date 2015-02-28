//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Runtime.InteropServices;
using System.Security;

using Newtonsoft.Json;


namespace WmBridge.Web.Model
{
    public class SecureStringJsonConverter : JsonConverter
    {
        // Source: https://github.com/cloudfoundry-incubator/if_warden/blob/master/IronFoundry.Warden.Shared/Messaging/SecureStringJsonConverter.cs

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SecureString);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;

            var value = reader.Value.ToString();
            return StringToSecureString(value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                SecureString secureValue = (SecureString)value;
                writer.WriteValue(SecureStringToString(secureValue));
            }
        }

        static string SecureStringToString(System.Security.SecureString secureString)
        {
            if (secureString == null)
                throw new ArgumentNullException("secureString");

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        static SecureString StringToSecureString(string unsecuredString)
        {
            var securedString = new SecureString();
            foreach (var c in unsecuredString)
            {
                securedString.AppendChar(c);
            }

            securedString.MakeReadOnly();

            return securedString;
        }
    }
}
