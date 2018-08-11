using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public static class Security
   {
      public static RemoteCertificateValidationCallback AlwaysAllowByHosts( params string[] hosts )
      {
         var lookup = new HashSet<string>( hosts, StringComparer.OrdinalIgnoreCase );

         return ( sender, certificate, chain, sslPolicyErrors ) =>
         {
            var request = sender as HttpWebRequest;
            if( request != null )
            {
               return lookup.Contains( request.Address.Host );
            }
            return false;
         };
      }
   }
}
