using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   internal class HttpSecurity
   {
      public readonly HashSet<string> _hosts = new HashSet<string>();

      public void EnableSslFor( params string[] hosts )
      {
         foreach( var host in hosts )
         {
            _hosts.Add( host );
         }
      }

      internal RemoteCertificateValidationCallback GetCertificateValidationCheck()
      {
         if( _hosts.Count == 0 ) return null;

         return ( sender, certificate, chain, sslPolicyErrors ) =>
         {
            var request = sender as HttpWebRequest;
            if( request != null )
            {
               return _hosts.Contains( request.Address.Host );
            }
            return false;
         };
      }
   }
}
