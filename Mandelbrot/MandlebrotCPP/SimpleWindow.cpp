// Copyright 2014-2021 Eternal Developments LLC. All Rights Reserved.

#include <Windows.h>
#include <math.h>

typedef float FloatingPoint;
//typedef double FloatingPoint;

static bool bRunning = true;

static const int Iterations = 20;
static const int WindowWidth = 1024;
static const int WindowHeight = 1024;

static HWND WindowHandle;
static unsigned char PalettedMandelbrotImage[1024 * 1024];
static LARGE_INTEGER Duration;

// The custom WndProc to handle clicking to exit
LRESULT CALLBACK WindowProc( HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam )
{
	switch( uMsg )
	{
	case WM_LBUTTONDOWN:
	case WM_RBUTTONDOWN:
		bRunning = false;
		break;
	}

	return DefWindowProc( hwnd, uMsg, wParam, lParam );
}

// Creates a simple window with a custom WndProc
HWND CreateSimpleWindow( HINSTANCE hInstance )
{
	WNDCLASS WindowClass =
	{
		CS_OWNDC,
		WindowProc,
		0,
		0,
		hInstance,
		NULL,
		NULL,
		NULL,
		NULL,
		L"MandelbrotWindowClass"
	};

	RegisterClass( &WindowClass );

	HWND Handle = CreateWindow( L"MandelbrotWindowClass", L"Mandelbrot", WS_POPUPWINDOW, 0, 0, WindowWidth, WindowHeight, NULL, NULL, NULL, NULL );

	return Handle;
}

// Create a paletted Mandelbot set
void DrawMandelbrot()
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
			while( Counter < 255 && ( WorkX * WorkX ) + ( WorkY * WorkY ) < 4.0 )
			{
				Counter++;

				FloatingPoint NewWorkX = ( WorkX * WorkX ) - ( WorkY * WorkY ) + XCoordinate;
				WorkY = 2 * WorkX * WorkY + YCoordinate;
				WorkX = NewWorkX;
			}

			// Use that number to set the color
			PalettedMandelbrotImage[( YIndex * WindowWidth ) + XIndex] = ( unsigned char )Counter;

			YCoordinate += DeltaY;
		}

		XCoordinate += DeltaX;
	}
}

// Convert a paletted Mandelbrot set to a bitmap and set that bitmap as the window background
void DrawBitmap()
{
	int* MandelbrotImage = new int[1024 * 1024];

	for( int Index = 0; Index < WindowWidth * WindowHeight; Index++ )
	{
		int PaletteIndex = PalettedMandelbrotImage[Index];
		MandelbrotImage[Index] = RGB( PaletteIndex, PaletteIndex, PaletteIndex ) | 0xff000000;
	}

	HDC DestinationDC = GetDC( WindowHandle );
	HDC SourceDC = CreateCompatibleDC( DestinationDC );

	BITMAP NewBitmap = { 0 };
	HBITMAP hBitmap = CreateBitmap( WindowWidth, WindowHeight, 1, 32, MandelbrotImage );
	GetObject( hBitmap, sizeof( BITMAP ), ( LPSTR )&NewBitmap );
	SelectObject( SourceDC, hBitmap );

	BitBlt( DestinationDC, 0, 0, NewBitmap.bmWidth, NewBitmap.bmHeight, SourceDC, 0, 0, SRCCOPY );

	DeleteDC( SourceDC );
	DeleteDC( DestinationDC );
}

// Main program logic
int CALLBACK WinMain( HINSTANCE hInstance, HINSTANCE, LPSTR, int )
{
	WindowHandle = CreateSimpleWindow( hInstance );

	// Recalculate a Mandelbrot set several times to get an average
	Duration.QuadPart = 0;
	for( int Iteration = 0; Iteration < Iterations; Iteration++ )
	{
		LARGE_INTEGER StartTime;
		QueryPerformanceCounter( &StartTime );

		DrawMandelbrot();

		LARGE_INTEGER EndTime;
		QueryPerformanceCounter( &EndTime );

		Duration.QuadPart += EndTime.QuadPart - StartTime.QuadPart;
	}

	ShowWindow( WindowHandle, SW_SHOW );
	DrawBitmap();

	// Tick the input loop waiting for a mouse click
	while( bRunning )
	{
		MSG Message;
		while( PeekMessage( &Message, NULL, 0, 0, PM_NOREMOVE ) )
		{
			GetMessage( &Message, NULL, 0, 0 );
			TranslateMessage( &Message );
			DispatchMessage( &Message );
		}
	}

	// Display the result
	LARGE_INTEGER Frequency;
	QueryPerformanceFrequency( &Frequency );
	int Milliseconds = ( int )( ( Duration.QuadPart * 1000.0 ) / Frequency.QuadPart );

	wchar_t Results[256] = { 0 };
	wsprintf( Results, L"Mandelbrot generation averaged %d ms", Milliseconds / Iterations );
	MessageBox( NULL, Results, L"Mandelbrot C++ Test", MB_OK );

	CloseWindow( WindowHandle );
	return 0;
}
