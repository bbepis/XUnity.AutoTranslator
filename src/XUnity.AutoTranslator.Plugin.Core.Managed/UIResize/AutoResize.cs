using System;

namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   class AutoResize : IFontAutoResizeCommand
   {
      private bool _shouldAutoResize;

      public AutoResize( string[] args )
      {
         if( args.Length != 1 ) throw new ArgumentException( "ChangeFontSize requires one argument." );

         _shouldAutoResize = bool.Parse( args[ 0 ] );
      }

      public bool ShouldAutoResize()
      {
         return _shouldAutoResize;
      }
   }
}
