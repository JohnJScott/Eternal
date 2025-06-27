// Copyright Eternal Developments LLC. All Rights Reserved.

global using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Eternal.TheSwizzlerTest
{
	[TestClass]
	public class TheSwizzlerTest
	{
		[TestMethod("Validate the swizzle validation")]
		public void TestSwizzleValidation()
		{
			Assert.IsTrue( TheSwizzler.TheSwizzler.ValidateSwizzle( "RGBA" ), "RGBA is a valid swizzle" );
			Assert.IsTrue( TheSwizzler.TheSwizzler.ValidateSwizzle( "rgba" ), "rgba is a valid swizzle" );
			Assert.IsTrue( TheSwizzler.TheSwizzler.ValidateSwizzle( "0101" ), "0101 is a valid swizzle" );

			Assert.IsFalse( TheSwizzler.TheSwizzler.ValidateSwizzle( "01010" ), "01010 is an invalid swizzle" );
			Assert.IsFalse( TheSwizzler.TheSwizzler.ValidateSwizzle( "010" ), "010 is an invalid swizzle" );
			Assert.IsFalse( TheSwizzler.TheSwizzler.ValidateSwizzle( "xyzw" ), "xyzw is an invalid swizzle" );
			Assert.IsFalse( TheSwizzler.TheSwizzler.ValidateSwizzle( "XYZW" ), "XYZW is an invalid swizzle" );
			Assert.IsFalse( TheSwizzler.TheSwizzler.ValidateSwizzle( "abcd" ), "abcd is an invalid swizzle" );

			Assert.IsFalse( TheSwizzler.TheSwizzler.ValidateSwizzle( "rgNN" ), "rgNN is an invalid swizzle" );
		}

		[TestMethod( "Test primary components" )]
		public void TestPrimaryComponents()
		{
			string image_name = "TestData/test.png";
			string image_full_path = Path.GetFullPath( Path.Combine( "..", "..", "..", "Eternal.TheSwizzlerTest", image_name ) );

			using( Image<Rgba32> source_image = Image.Load<Rgba32>( image_full_path ) )
			{
				TheSwizzler.ImageStats source_stats = TheSwizzler.TheSwizzler.AnalyzeImage( source_image );

				Assert.IsTrue( source_stats.MinRed == 0, "Minimum red value should be 0" );
				Assert.IsTrue( source_stats.MinGreen == 0, "Minimum green value should be 0" );
				Assert.IsTrue( source_stats.MinBlue == 0, "Minimum blue value should be 0" );
				Assert.IsTrue( source_stats.MinAlpha == 0, "Minimum alpha value should be 0" );

				Assert.IsTrue( source_stats.MaxRed == 255, "Maximum red value should be 0" );
				Assert.IsTrue( source_stats.MaxGreen == 255, "Maximum green value should be 0" );
				Assert.IsTrue( source_stats.MaxBlue == 255, "Maximum blue value should be 0" );
				Assert.IsTrue( source_stats.MaxAlpha == 255, "Maximum alpha value should be 0" );
			}
		}

		[TestMethod( "Test red filter" )]
		public void TestRedFilter()
		{
			string image_name = "TestData/test.png";
			string image_full_path = Path.GetFullPath( Path.Combine( "..", "..", "..", "Eternal.TheSwizzlerTest", image_name ) );

			using( Image<Rgba32> source_image = Image.Load<Rgba32>( image_full_path ) )
			{
				using( Image<Rgba32> destination_image = TheSwizzler.TheSwizzler.SwizzleImage( source_image, "G001" ) )
				{
					TheSwizzler.ImageStats destination_stats = TheSwizzler.TheSwizzler.AnalyzeImage( destination_image );

					Assert.IsTrue( destination_stats.MinGreen == 0, "Minimum green value should be 0" );
					Assert.IsTrue( destination_stats.MinBlue == 0, "Minimum blue value should be 0" );
					Assert.IsTrue( destination_stats.MaxGreen == 0, "Maximum green value should be 0" );
					Assert.IsTrue( destination_stats.MaxBlue == 0, "Maximum blue value should be 0" );
				}
			}
		}

		[TestMethod( "Test green filter" )]
		public void TestGreenFilter()
		{
			string image_name = "TestData/test.png";
			string image_full_path = Path.GetFullPath( Path.Combine( "..", "..", "..", "Eternal.TheSwizzlerTest", image_name ) );

			using( Image<Rgba32> source_image = Image.Load<Rgba32>( image_full_path ) )
			{
				using( Image<Rgba32> destination_image = TheSwizzler.TheSwizzler.SwizzleImage( source_image, "0B01" ) )
				{
					TheSwizzler.ImageStats destination_stats = TheSwizzler.TheSwizzler.AnalyzeImage( destination_image );

					Assert.IsTrue( destination_stats.MinRed == 0, "Minimum red value should be 0" );
					Assert.IsTrue( destination_stats.MinBlue == 0, "Minimum blue value should be 0" );
					Assert.IsTrue( destination_stats.MaxRed == 0, "Maximum red value should be 0" );
					Assert.IsTrue( destination_stats.MaxBlue == 0, "Maximum blue value should be 0" );
				}
			}
		}

		[TestMethod( "Test blue filter" )]
		public void TestBlueFilter()
		{
			string image_name = "TestData/test.png";
			string image_full_path = Path.GetFullPath( Path.Combine( "..", "..", "..", "Eternal.TheSwizzlerTest", image_name ) );

			using( Image<Rgba32> source_image = Image.Load<Rgba32>( image_full_path ) )
			{
				using( Image<Rgba32> destination_image = TheSwizzler.TheSwizzler.SwizzleImage( source_image, "00R1" ) )
				{
					TheSwizzler.ImageStats destination_stats = TheSwizzler.TheSwizzler.AnalyzeImage( destination_image );

					Assert.IsTrue( destination_stats.MinRed == 0, "Minimum red value should be 0" );
					Assert.IsTrue( destination_stats.MinGreen == 0, "Minimum green value should be 0" );
					Assert.IsTrue( destination_stats.MaxRed == 0, "Maximum red value should be 0" );
					Assert.IsTrue( destination_stats.MaxGreen == 0, "Maximum green value should be 0" );
				}
			}
		}

		[TestMethod( "Test alpha filter" )]
		public void TestAlphaFilter()
		{
			string image_name = "TestData/test.png";
			string image_full_path = Path.GetFullPath( Path.Combine( "..", "..", "..", "Eternal.TheSwizzlerTest", image_name ) );

			using( Image<Rgba32> source_image = Image.Load<Rgba32>( image_full_path ) )
			{
				using( Image<Rgba32> destination_image = TheSwizzler.TheSwizzler.SwizzleImage( source_image, "000A" ) )
				{
					TheSwizzler.ImageStats destination_stats = TheSwizzler.TheSwizzler.AnalyzeImage( destination_image );

					Assert.IsTrue( destination_stats.MaxAlpha != 0, "Minimum alpha value should be 0" );
					
					Assert.IsTrue( destination_stats.MinRed == 0, "Minimum red value should be 0" );
					Assert.IsTrue( destination_stats.MinGreen == 0, "Minimum green value should be 0" );
					Assert.IsTrue( destination_stats.MinBlue == 0, "Minimum blue value should be 0" );
					Assert.IsTrue( destination_stats.MaxRed == 0, "Maximum red value should be 0" );
					Assert.IsTrue( destination_stats.MaxGreen == 0, "Maximum green value should be 0" );
					Assert.IsTrue( destination_stats.MaxBlue == 0, "Maximum blue value should be 0" );
				}
			}
		}

		[TestMethod( "Test negative component" )]
		public void TestMegativeFilter()
		{
			string image_name = "TestData/lena.png";
			string image_full_path = Path.GetFullPath( Path.Combine( "..", "..", "..", "Eternal.TheSwizzlerTest", image_name ) );

			using( Image<Rgba32> source_image = Image.Load<Rgba32>( image_full_path ) )
			{
				TheSwizzler.ImageStats source_stats = TheSwizzler.TheSwizzler.AnalyzeImage( source_image );

				using( Image<Rgba32> destination_image = TheSwizzler.TheSwizzler.SwizzleImage( source_image, "rgba" ) )
				{
					TheSwizzler.ImageStats destination_stats = TheSwizzler.TheSwizzler.AnalyzeImage( destination_image );

					Assert.IsTrue( destination_stats.MaxAlpha == 255 - source_stats.MinAlpha, "Minimum red value should be 0" );

					Assert.IsTrue( destination_stats.MinRed == 255 - source_stats.MaxRed, "Minimum red value should be inverted" );
					Assert.IsTrue( destination_stats.MinGreen == 255 - source_stats.MaxGreen, "Minimum green value should be be inverted" );
					Assert.IsTrue( destination_stats.MinBlue == 255 - source_stats.MaxBlue, "Minimum blue value should be be inverted" );
					Assert.IsTrue( destination_stats.MaxRed == 255 - source_stats.MinRed, "Maximum red value should be be inverted" );
					Assert.IsTrue( destination_stats.MaxGreen == 255 - source_stats.MinGreen, "Maximum green value should be be inverted" );
					Assert.IsTrue( destination_stats.MaxBlue == 255 - source_stats.MinBlue, "Maximum blue value should be be inverted" );
				}
			}
		}

		[TestMethod( "Test normalization" )]
		public void TestNormalization()
		{
			string image_name = "TestData/fabric_normal.png";
			string image_full_path = Path.GetFullPath( Path.Combine( "..", "..", "..", "Eternal.TheSwizzlerTest", image_name ) );

			using( Image<Rgba32> source_image = Image.Load<Rgba32>( image_full_path ) )
			{
				TheSwizzler.ImageStats source_stats = TheSwizzler.TheSwizzler.AnalyzeImage( source_image );
				Assert.IsTrue( source_stats.IsNormalized, "Image should be a normalized normal map" );

				using( Image<Rgba32> destination_image = TheSwizzler.TheSwizzler.SwizzleImage( source_image, "RGN1" ) )
				{
					TheSwizzler.ImageStats destination_stats = TheSwizzler.TheSwizzler.AnalyzeImage( destination_image );
					Assert.IsTrue( source_stats.IsNormalized, "Image should be a normalized normal map" );

					Assert.IsTrue( destination_stats.MinRed == source_stats.MinRed, "Minimum red value should be untouched" );
					Assert.IsTrue( destination_stats.MinGreen == source_stats.MinGreen, "Minimum green value should be be untouched" );
					Assert.IsTrue( destination_stats.MaxRed == source_stats.MaxRed, "Maximum red value should be be untouched" );
					Assert.IsTrue( destination_stats.MaxGreen == source_stats.MaxGreen, "Maximum green value should be be untouched" );

					Assert.IsFalse( destination_stats.MinBlue == source_stats.MinBlue, "Minimum blue value should be be different" );
				}
			}
		}
	}
}
