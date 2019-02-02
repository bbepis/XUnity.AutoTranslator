using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Utilties
{
   internal class IndentedTextWriter
   {
      private readonly TextWriter _writer;
      private readonly char _indent;

      public IndentedTextWriter( TextWriter writer, char indent )
      {
         _writer = writer;
         _indent = indent;
      }

      public int Indent { get; set; }

      public void WriteLine( string line )
      {
         _writer.Write( new string( _indent, Indent ) );
         _writer.WriteLine( line );
      }
   }
}
