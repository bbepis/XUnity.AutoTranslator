using System;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   class UGUI_VerticalOverflow : IUGUI_VerticalOverflow
   {
      private int? _mode;

      public UGUI_VerticalOverflow( string[] args )
      {
         if( args.Length != 1 ) throw new ArgumentException( "UGUI_VerticalOverflow requires one argument." );

         _mode = (int)EnumHelper.GetValues( ClrTypes.VerticalWrapMode, args[ 0 ] );
      }

      public int? GetMode()
      {
         return _mode;
      }
   }
}
