namespace XUnity.Common.Shims
{
   internal class ManagedTypeCastingHelper : ITypeCastingHelper
   {
      public bool TryCast<TObject>( object obj, out TObject castedObject )
      {
         if( obj is TObject c )
         {
            castedObject = c;
            return true;
         }

         castedObject = default;
         return false;
      }
   }
}
