using System.Collections.Generic;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Www
{
   public class WwwRequestInfo
   {
      private Dictionary<string, string> _headers;

      public WwwRequestInfo( string address )
      {
         Address = address;
      }

      public WwwRequestInfo( string address, string data )
      {
         Address = address;
         Data = data;
      }

      public string Address { get; set; }

      public string Data { get; set; }

      public Dictionary<string, string> Headers
      {
         get
         {
            if( _headers == null ) _headers = new Dictionary<string, string>();

            return _headers;
         }
         set
         {
            _headers = value;
         }
      }
   }
}
