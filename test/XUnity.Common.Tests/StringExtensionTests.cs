using System;
using Xunit;
using XUnity.Common.Extensions;

namespace XUnity.Common.Tests
{
   public class StringExtensionTests
   {
      [Theory( DisplayName = "Can_Make_Path_Relative_If_Not_Full_Path" )]
      [InlineData(
         @"managed\..\data\levels\scene1.dat",
         @"c:\games\bestGame",
         @"data\levels\scene1.dat"
         )]
      public void Can_Make_Path_Relative_If_Not_Full_Path( string path, string basePath, string expectedPath )
      {
         var actualPath = path.MakeRelativePath( basePath );

         Assert.Equal( expectedPath, actualPath );
      }

      [Theory( DisplayName = "Can_Make_Path_Relative1" )]
      [InlineData(
         @"c:\games\bestGame\managed\..\data\levels\scene1.dat",
         @"c:\games\bestGame",
         @"data\levels\scene1.dat"
         )]
      public void Can_Make_Path_Relative1( string path, string basePath, string expectedPath )
      {
         var actualPath = path.MakeRelativePath( basePath );

         Assert.Equal( expectedPath, actualPath );
      }

      [Theory( DisplayName = "Can_Make_Path_Relative2" )]
      [InlineData(
         @"c:\games\bestGame\managed\code\..\..\data\levels\scene1.dat",
         @"c:\games\bestGame",
         @"data\levels\scene1.dat"
         )]
      public void Can_Make_Path_Relative2( string path, string basePath, string expectedPath )
      {
         var actualPath = path.MakeRelativePath( basePath );

         Assert.Equal( expectedPath, actualPath );
      }
   }
}
