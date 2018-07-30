using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public abstract class KnownEndpoint
   {
      public KnownEndpoint( string identifier )
      {
         Identifier = identifier;
      }

      public string Identifier { get; }

      public abstract void ConfigureServicePointManager();

      public abstract string GetServiceUrl( string untranslatedText, string from, string to );

      public abstract void ApplyHeaders( WebHeaderCollection headers );

      public abstract bool TryExtractTranslated( string result, out string translated );

      public virtual void WriteCookies( HttpWebResponse response )
      {

      }

      public virtual CookieContainer ReadCookies()
      {
         return null;
      }

      public virtual bool Fallback()
      {
         return false;
      }

      public virtual IEnumerator Setup( int translationCount )
      {
         return null;
      }
   }
}
