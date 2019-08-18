using System;
using Xunit;
using XUnity.Common.Extensions;

namespace XUnity.Common.Tests
{
   public class StringExtensionTests
   {
      [Theory( DisplayName = "Can_Make_Path_Relative" )]
      [InlineData(
         @"managed\..\data\levels\scene1.dat",
         @"c:\games\bestGame",
         @"data\levels\scene1.dat"
         )]
      [InlineData(
         @"..\data\levels\scene1.dat",
         @"c:\games\bestGame",
         @"..\data\levels\scene1.dat"
         )]
      [InlineData(
         @"..\..\data\levels\scene1.dat",
         @"c:\games\bestGame",
         @"..\..\data\levels\scene1.dat"
         )]
      [InlineData(
         @"..\..\..\data\levels\scene1.dat",
         @"c:\games\bestGame",
         @"..\..\..\data\levels\scene1.dat"
         )]
      [InlineData(
         @"c:\games\bestGame\managed\..\data\levels\scene1.dat",
         @"c:\games\bestGame",
         @"data\levels\scene1.dat"
         )]
      [InlineData(
         @"c:\games\bestGame\managed\code\..\..\data\levels\scene1.dat",
         @"c:\games\bestGame",
         @"data\levels\scene1.dat"
         )]
      [InlineData(
         @"c:\games\bestGame\data\levels\scene1.dat",
         @"c:\games\a\b\s\d\e\f\g",
         @"..\..\..\..\..\..\..\bestGame\data\levels\scene1.dat"
         )]
      public void Can_Make_Path_Relative( string path, string basePath, string expectedPath )
      {
         var actualPath = path.MakeRelativePath( basePath );

         Assert.Equal( expectedPath, actualPath );
      }
   }
}
