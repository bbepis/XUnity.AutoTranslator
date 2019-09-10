using System;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   class UGUI_HorizontalOverflow : IUGUI_HorizontalOverflow
   {
      private int? _mode;

      public UGUI_HorizontalOverflow( string[] args )
      {
         if( args.Length != 1 ) throw new ArgumentException( "UGUI_HorizontalOverflow requires one argument." );

         _mode = (int)EnumHelper.GetValues( ClrTypes.HorizontalWrapMode, args[ 0 ] );
      }

      public int? GetMode()
      {
         return _mode;
      }
   }
}
