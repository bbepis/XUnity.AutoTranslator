using System.Collections.Generic;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Www
{
   /// <summary>
   /// Class representing the info required to setup a web request with WWW.
   /// </summary>
   public class WwwRequestInfo
   {
      private Dictionary<string, string> _headers;

      /// <summary>
      /// Creates a GET web request.
      /// </summary>
      /// <param name="address"></param>
      public WwwRequestInfo( string address )
      {
         Address = address;
      }

      /// <summary>
      /// Creates a POST web request.
      /// </summary>
      /// <param name="address"></param>
      /// <param name="data"></param>
      public WwwRequestInfo( string address, string data )
      {
         Address = address;
         Data = data;
      }

      /// <summary>
      /// Gets the the address.
      /// </summary>
      public string Address { get; private set; }

      /// <summary>
      /// Gets the data.
      /// </summary>
      public string Data { get; private set; }

      /// <summary>
      /// Gets or sets the headers of the request.
      /// </summary>
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
