// Copyright 2015-2021 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using FloatingPoint = System.Single;
//using FloatingPoint = System.Double;

namespace Mandelbrot
{
	public partial class SimpleWindow : Form
	{
		// Constants to tweak
		private const int Iterations = 20;
		private const int WindowWidth = 1024;
		private const int WindowHeight = 1024;

		// The paletted result
		private byte[] PalettedMandelbrotImage = new byte[WindowWidth * WindowHeight];

		Stopwatch Timer = new Stopwatch();

		// Display the results as the form closes
		private void OnFormClosed( object sender, FormClosingEventArgs e )
		{
			// Show the timing results
			MessageBox.Show( null, "Mandelbrot generation averaged " + Timer.ElapsedMilliseconds / Iterations + " ms", "Mandelbrot C# Test", MessageBoxButtons.OK );
		}

		// A simple click to exit callback
		private void OnMouseClick( object sender, MouseEventArgs e )
		{
			Close();
		}

		// Create a paletted Mandelbot set
		private void CreateMandelbrot()
		{
			FloatingPoint Left = ( FloatingPoint )( -2.1 );
			FloatingPoint Right = ( FloatingPoint )1.0;
			FloatingPoint Top = ( FloatingPoint )( -1.3 );
			FloatingPoint Bottom = ( FloatingPoint )1.3;

			FloatingPoint DeltaX = ( Right - Left ) / WindowWidth;
			FloatingPoint DeltaY = ( Bottom - Top ) / WindowHeight;

			FloatingPoint XCoordinate = Left;
			for( int XIndex = 0; XIndex < WindowWidth; XIndex++ )
			{
				FloatingPoint YCoordinate = Top;
				for( int YIndex = 0; YIndex < WindowHeight; YIndex++ )
				{
					FloatingPoint WorkX = 0;
					FloatingPoint WorkY = 0;

					int Counter = 0;
					while( Counter < 255 && ( ( WorkX * WorkX ) + ( WorkY * WorkY ) ) < 4.0 )
					{
						Counter++;

						FloatingPoint NewWorkX = ( WorkX * WorkX ) - ( WorkY * WorkY ) + XCoordinate;
						WorkY = 2 * WorkX * WorkY + YCoordinate;
						WorkX = NewWorkX;
					}

					PalettedMandelbrotImage[( YIndex * WindowWidth ) + XIndex] = ( byte )Counter;

					YCoordinate += DeltaY;
				}

				XCoordinate += DeltaX;
			}
		}

		// Convert a paletted Mandelbrot set to a bitmap and set that bitmap as the window background
		private void DrawBitmap()
		{
			int[] MandelbrotImage = new int[WindowWidth * WindowHeight];

			for( int Index = 0; Index < WindowWidth * WindowHeight; Index++ )
			{
				int PaletteIndex = PalettedMandelbrotImage[Index];
				MandelbrotImage[Index] = Color.FromArgb( PaletteIndex, PaletteIndex, PaletteIndex ).ToArgb();
			}

			Bitmap NewBitmap = new Bitmap( WindowWidth, WindowHeight );

			Rectangle LockArea = new Rectangle( 0, 0, NewBitmap.Width, NewBitmap.Height );
			BitmapData BitmapData = NewBitmap.LockBits( LockArea, ImageLockMode.WriteOnly, NewBitmap.PixelFormat );
			
			IntPtr StartAddress = BitmapData.Scan0;
			Marshal.Copy( MandelbrotImage, 0, StartAddress, NewBitmap.Width * NewBitmap.Height );

			NewBitmap.UnlockBits( BitmapData );

			BackgroundImage = NewBitmap;
		}

		// Main program logic
		public SimpleWindow()
		{
			InitializeComponent();

			ClientSize = new Size( WindowWidth, WindowHeight );

			// Recalculate a Mandelbrot set several times to get an average
			for( int Iteration = 0; Iteration < Iterations; Iteration++ )
			{
				Timer.Start();
				CreateMandelbrot();
				Timer.Stop();
			}

			// Draw the resultant bitmap to verify the results
			DrawBitmap();
		}
	}
}
