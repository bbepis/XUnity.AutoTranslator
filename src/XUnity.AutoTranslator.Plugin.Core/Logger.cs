using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core
{
   public abstract class Logger
   {
      public static Logger Current;

      public void Error( Exception e, string message )
      {
         Write( $"[XUnity.AutoTranslator][ERROR]: {message}" + Environment.NewLine + e );
      }

      public void Error( string message )
      {
         Write( $"[XUnity.AutoTranslator][ERROR]: {message}" );
      }

      public void Warn( Exception e, string message )
      {
         Write( $"[XUnity.AutoTranslator][WARN]: {message}" + Environment.NewLine + e );
      }

      public void Warn( string message )
      {
         Write( $"[XUnity.AutoTranslator][WARN]: {message}" );
      }

      public void Info( Exception e, string message )
      {
         Write( $"[XUnity.AutoTranslator][INFO]: {message}" + Environment.NewLine + e );
      }

      public void Info( string message )
      {
         Write( $"[XUnity.AutoTranslator][INFO]: {message}" );
      }

      public void Debug( Exception e, string message )
      {
         if( Settings.EnableDebugLogs )
         {
            Write( $"[XUnity.AutoTranslator][DEBUG]: {message}" + Environment.NewLine + e );
         }
      }

      public void Debug( string message )
      {
         if( Settings.EnableDebugLogs )
         {
            Write( $"[XUnity.AutoTranslator][DEBUG]: {message}" );
         }
      }

      protected abstract void Write( string formattedMessage );
   }
}
