// Copyright Eternal Developments LLC. All Rights Reserved.

using Eternal.ConsoleUtilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;

namespace Eternal.TheSwizzler
{
	using int16 = System.Int16;
	using int32 = System.Int32;
	using int64 = System.Int64;
	using int8 = System.SByte;
	using uint16 = System.UInt16;
	using uint32 = System.UInt32;
	using uint64 = System.UInt64;
	using uint8 = System.Byte;

	/// <summary>
	/// A class to store the information about the components of an image
	/// </summary>
	public class ImageStats
	{
		public int32 Width = 0;
		public int32 Height = 0;
		public int32 Size = 0;

		public uint8 MinRed = uint8.MaxValue;
		public uint8 MinGreen = uint8.MaxValue;
		public uint8 MinBlue = uint8.MaxValue;
		public uint8 MinAlpha = uint8.MaxValue;

		public uint8 MaxRed = 0;
		public uint8 MaxGreen = 0;
		public uint8 MaxBlue = 0;
		public uint8 MaxAlpha = 0;

		public int64 Red = 0;
		public int64 Green = 0;
		public int64 Blue = 0;
		public int64 Alpha = 0;

		public double Normal = 0;
		public double AverageNormal = 0;
		public bool IsNormalized = false;

		/// <summary>
		/// Print out the collected image stats to the console
		/// </summary>
		public void Print()
		{
			ConsoleLogger.Log( $" .. R: {MinRed} < {Red / Size} < {MaxRed}" );
			ConsoleLogger.Log( $" .. G: {MinGreen} < {Green / Size} < {MaxGreen}" );
			ConsoleLogger.Log( $" .. B: {MinBlue} < {Blue / Size} < {MaxBlue}" );
			ConsoleLogger.Log( $" .. A: {MinAlpha} < {Alpha / Size} < {MaxAlpha}" );

			string normal_description = IsNormalized ? "Normalized" : "NOT normalized";
			ConsoleLogger.Log( $" .. N: {AverageNormal:F3} - {normal_description}" );
		}
	}

	public class TheSwizzler
	{
		/// <summary>
		/// Ensures the swizzle is valid.
		/// Valid swizzle characters are rRgGbBaA01 and must be four characters long.
		/// </summary>
		/// <param name="swizzle">Swizzle argument passed on the command line.</param>
		public static bool ValidateSwizzle( string swizzle )
		{
			if( swizzle.Length != 4 )
			{
				ConsoleLogger.Error( $"Invalid swizzle {swizzle} - it must contain 4 characters" );
				return false;
			}

			foreach( char component in swizzle )
			{
				if( !"rRgGbBaA01N".Contains( component ) )
				{
					ConsoleLogger.Error( $"Invalid component {component} in swizzle {swizzle} - it must only contain rRgGbBaA01N" );
					return false;
				}
			}

			if( swizzle.Count( x => x == 'N' ) > 1 )
			{
				ConsoleLogger.Error( $"Invalid swizzle {swizzle} - only one normal component is allowed." );
				return false;
			}

			return true;
		}

		/// <summary>
		/// Analyze the image on a component wide basis to calculate the image stats
		/// </summary>
		/// <param name="image">The image to analyze</param>
		/// <returns>The stats of the image.</returns>
		public static ImageStats AnalyzeImage( Image<Rgba32> image )
		{
			ImageStats stats = new ImageStats();

			image.ProcessPixelRows( pixelAccessor =>
			{
				stats.Width = pixelAccessor.Width;
				stats.Height = pixelAccessor.Height;
				stats.Size = stats.Width * stats.Height;

				for( int32 row_index = 0; row_index < pixelAccessor.Height; row_index++ )
				{
					Span<Rgba32> row = pixelAccessor.GetRowSpan( row_index );
					foreach( Rgba32 rgba in row )
					{
						stats.MinRed = Math.Min( stats.MinRed, rgba.R );
						stats.MinGreen = Math.Min( stats.MinGreen, rgba.G );
						stats.MinBlue = Math.Min( stats.MinBlue, rgba.B );
						stats.MinAlpha = Math.Min( stats.MinAlpha, rgba.A );

						stats.MaxRed = Math.Max( stats.MaxRed, rgba.R );
						stats.MaxGreen = Math.Max( stats.MaxGreen, rgba.G );
						stats.MaxBlue = Math.Max( stats.MaxBlue, rgba.B );
						stats.MaxAlpha = Math.Max( stats.MaxAlpha, rgba.A );

						stats.Red += rgba.R;
						stats.Green += rgba.G;
						stats.Blue += rgba.B;
						stats.Alpha += rgba.A;

						int32 x = ( rgba.R - 128 ) * ( rgba.R - 128 );
						int32 y = ( rgba.G - 128 ) * ( rgba.G - 128 );
						int32 z = ( rgba.B - 128 ) * ( rgba.B - 128 );
						stats.Normal += Math.Sqrt( x + y + z );
					}
				}

				stats.AverageNormal = stats.Normal / ( stats.Size * 128.0 );
				stats.IsNormalized = ( stats.AverageNormal > .98 && stats.AverageNormal < 1.02 );
			} );

			return stats;
		}

		/// <summary>
		/// Returns the swizzled and/or inverted component based on the swizzle.
		/// </summary>
		/// <param name="sourceRgba">Source R8G8B8A8 pixel color.</param>
		/// <param name="swizzle">The components to rearrange or invert.</param>
		/// <param name="componentIndex">The component index of the swizzle - 0 to 3</param>
		/// <returns>The swizzled and/or inverted component, or 128 for the 0, 1, or N swizzles.</returns>
		private static uint8 GetSimpleComponent( Rgba32 sourceRgba, string swizzle, int32 componentIndex )
		{
			uint8 destination = 0;
			switch( swizzle[componentIndex] )
			{
				case 'R': destination = sourceRgba.R; break;
				case 'G': destination = sourceRgba.G; break;
				case 'B': destination = sourceRgba.B; break;
				case 'A': destination = sourceRgba.A; break;

				case 'r': destination = ( uint8 )( 255 - sourceRgba.R ); break;
				case 'g': destination = ( uint8 )( 255 - sourceRgba.G ); break;
				case 'b': destination = ( uint8 )( 255 - sourceRgba.B ); break;
				case 'a': destination = ( uint8 )( 255 - sourceRgba.A ); break;

				// Special cases - set to signed 0 so they are ignored for the normalization case
				case '0': destination = 128; break;
				case '1': destination = 128; break;
				case 'N': destination = 128; break;
			}

			return destination;
		}

		/// <summary>
		/// Assumes the pixel is a normalized vector, and derives the component to make that so.
		/// If you have a normal map consisting of XYZ1, swizzling as RGN1 will re-derive the Z component to make a unit normal.
		/// The alpha component can be considered in this calculation, but I don't see a practical use for this.
		/// </summary>
		/// <param name="sourceRgba">Source R8G8B8A8 pixel color.</param>
		/// <param name="swizzle">The components to normalize</param>
		/// <returns>The normalized value so that the pixel represents a unit vector.</returns>
		private static uint8 GetNormalizedComponent( Rgba32 sourceRgba, string swizzle )
		{
			int32 r = GetSimpleComponent( sourceRgba, swizzle, 0 ) - 128;
			int32 g = GetSimpleComponent( sourceRgba, swizzle, 1 ) - 128;
			int32 b = GetSimpleComponent( sourceRgba, swizzle, 2 ) - 128;
			int32 a = GetSimpleComponent( sourceRgba, swizzle, 3 ) - 128;

			int32 sum = ( 128 * 128 ) - ( r * r ) - ( g * g ) - ( b * b ) - ( a * a );
			int32 value = Math.Clamp( ( int32 )Math.Sqrt( sum ), 0, 127 );
			return ( uint8 )( value + 128 );
		}

		/// <summary>
		/// Works out the new component value based on the original pixel color and the swizzle
		/// </summary>
		/// <param name="sourceRgba">Source R8G8B8A8 pixel color.</param>
		/// <param name="swizzle">Instructions as to which components to swap</param>
		/// <param name="componentIndex">The index of the swizzle component being worked on.</param>
		/// <returns>The updated component value</returns>
		private static uint8 GetComponent( Rgba32 sourceRgba, string swizzle, int32 componentIndex )
		{
			uint8 destination = GetSimpleComponent( sourceRgba, swizzle, componentIndex );
			if( swizzle[componentIndex] == '0' )
			{
				destination = 0;
			}
			else if( swizzle[componentIndex] == '1' )
			{
				destination = 255;
			}
			else if( swizzle[componentIndex] == 'N' )
			{
				destination = GetNormalizedComponent( sourceRgba, swizzle );
			}

			return destination;
		}

		/// <summary>
		/// Swap the components of a pixel based on a swizzle pattern
		/// </summary>
		/// <param name="sourceImage">The unmodified source image</param>
		/// <param name="swizzle">Instructions as to which components to swap</param>
		/// <returns>The image after the components have been swapped.</returns>
		public static Image<Rgba32> SwizzleImage( Image<Rgba32> sourceImage, string swizzle )
		{
			Image<Rgba32> destination_image = new Image<Rgba32>( sourceImage.Width, sourceImage.Height );

			sourceImage.ProcessPixelRows( destination_image,
				( sourceAccessor, destinationAccessor ) =>
				{
					for( int32 row_index = 0; row_index < sourceAccessor.Height; row_index++ )
					{
						Span<Rgba32> source_row = sourceAccessor.GetRowSpan( row_index );
						Span<Rgba32> destination_row = destinationAccessor.GetRowSpan( row_index );

						for( int32 column_index = 0; column_index < sourceAccessor.Width; column_index++ )
						{
							Rgba32 source_rgba = source_row[column_index];

							Rgba32 destination_rgba;
							destination_rgba.R = GetComponent( source_rgba, swizzle, 0 );
							destination_rgba.G = GetComponent( source_rgba, swizzle, 1 );
							destination_rgba.B = GetComponent( source_rgba, swizzle, 2 );
							destination_rgba.A = GetComponent( source_rgba, swizzle, 3 );

							destination_row[column_index] = destination_rgba;
						}
					}
				} );

			return destination_image;
		}

		/// <summary>
		/// Swizzles to create test images, especially for those that don't map directly to an RGBA image.
		/// </summary>
		/// rgba01N
		/// rgb1 - rgb
		/// rgba - rgba
		/// r001 - l
		/// r00a - la
		/// rg01 - xy
		/// rgb1 - xyz
		/// <param name="args">The parameters passed on the command line.</param>
		/// <returns></returns>
		private static int32 Main( string[] args )
		{
			ConsoleLogger.Title( "TheSwizzler - Copyright Eternal Developments, LLC. All Rights Reserved." );
			if( args.Length < 1 )
			{
				ConsoleLogger.Log( "Usage: TheSwizzler.exe <image_name> [swizzle]" );
				ConsoleLogger.Log( " The swizzle must be 4 characters and consist of the following characters 'RGBArgba01N'" );
				ConsoleLogger.Log( " e.g. 'TheSwizzler.exe image.png GrB1' will load image.png and write out image.GrB1.png." );
				ConsoleLogger.Log( " The destination image will have" );
				ConsoleLogger.Log( " .. the original green component in the red channel," );
				ConsoleLogger.Log( " .. 255 - the original red component in the green channel," );
				ConsoleLogger.Log( " .. the original blue component in the blue channel," );
				ConsoleLogger.Log( " .. and 255 in the alpha channel." );
				return -1;
			}

			string image_name = args[0];
			if( !File.Exists( image_name ) )
			{
				ConsoleLogger.Error( $"Image '{image_name}' does not exist" );
				return -2;
			}

			using( Image<Rgba32> source_image = Image.Load<Rgba32>( image_name ) )
			{
				ConsoleLogger.Log( "Analyzing..." );
				ConsoleLogger.Log( $" .. loaded '{args[0]}'" );
				ConsoleLogger.Log( $" .. dimensions {source_image.Width} x {source_image.Height} as R8G8B8A8");
				ImageStats source_stats = AnalyzeImage( source_image );
				source_stats.Print();

				if( args.Length > 1 )
				{
					if( ValidateSwizzle( args[1] ) )
					{
						ConsoleLogger.Log( "Swizzling..." );
						Image<Rgba32> destination_image = SwizzleImage( source_image, args[1] );

						ConsoleLogger.Log( "Analyzing swizzled..." );
						ImageStats swizzled_stats = AnalyzeImage( destination_image );
						swizzled_stats.Print();

						image_name = Path.ChangeExtension( image_name, args[1] + ".png" );
						destination_image.SaveAsPng( image_name );

						ConsoleLogger.Log( $" .. saved '{image_name}'" );
					}
				}
			}

			ConsoleLogger.Success( "Processing complete!" );
			return 0;
		}
	}
}
