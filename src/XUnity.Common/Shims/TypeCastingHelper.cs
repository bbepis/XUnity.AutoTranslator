using XUnity.Common.Utilities;

namespace XUnity.Common.Shims
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class TypeCastingHelper
   {
      private static ITypeCastingHelper _instance;

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      public static ITypeCastingHelper Instance
      {
         get
         {
            if( _instance == null )
            {
               _instance = ActivationHelper.Create<ITypeCastingHelper>(
                  typeof( TypeCastingHelper ).Assembly,
                  "XUnity.Common.Managed.dll",
                  "XUnity.Common.IL2CPP.dll" );
            }
            return _instance;
         }
      }
   }

   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public interface ITypeCastingHelper
   {
      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      bool TryCastTo<TObject>( object obj, out TObject castedObject );
   }
}
