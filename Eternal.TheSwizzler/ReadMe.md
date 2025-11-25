# Eternal TheSwizzler

## Eternal.TheSwizzler NuGet library package for Net10.0
Copyright 2025 Eternal Developments, LLC. All Rights Reserved.

## License

MIT

## Direct Dependencies

Eternal.ConsoleUtilities - 1.0.10 - MIT

SixLabors.ImageSharp - 3.1.12 - Apache-2.0

# Functionality

This utility is to analyze the components of an image and report the minimum, maximum, and average of each component. It also reports if the RGB components form a unit vector if each component is treated as a signed 8 bit integer. (This is the case with normal maps.) A common texture compression technique is to store the X and Y components of a normal in a 2 channel image (the shader then derives the Z component at run time.) This tool can re-derive the Z component.

When creating test images for texture compression it can we awkward to create 2 channel images, and this utility will make this easy.

Usage: Eternal.TheSwizzler.exe <image.png> [4 letter swizzle]

The swizzle has to be 4 letters and consist of 'RGBArgba01N', and a new image will be saved as image.swizzle.png

| Swizzle | Action |
|-|---------|
| R | the red component |
| G | the green component |
| B | the blue component |
| A | the alpha component |
| r | 255 minus the red component |
| g | 255 minus the green component |
| b | 255 minus the blue component |
| a | 255 minus the alpha component |
| 0 | 0 |
| 1 | 255 |
| N | the value that will make the components form a unit vector |

Only a single N can be used. It is typically used to reconstruct the blue (Z) channel from an RG (XY) compressed image. In theory, it can do 2 and 4 channel versions too, but I'm not sure of the practical application of that. 0 and 1 components are ignored during this calculation.

| Swizzle | Format |
|-----|-----|
| RGB1 |	RGB |
| RGBA	|RGBA |
| R001 |	L |
| R00A	 |LA |
| RG01	| XY |
| RGB1	| XYZ |

Examples: 

This will invert the alpha channel of the image - Eternal.TheSwizzler.exe image.png RGBa

This will swap the red and blue components - Eternal.TheSwizzler.exe image.png BGRA

This will save an image preserving the R and G channels, but setting the blue channel to 0 and the alpha channel to 255, as image.RG01.png - Eternal.TheSwizzler.exe image.png RG01

This will reconstruct the blue component of the normal map and save as image.RG01.RGN1.png - Eternal.TheSwizzler.exe image.RG01.png RGN1

# Changes 25th November 2025

Updated to .net10. Updated dependencies.

# Changes 26th June 2025

Created

# Notes

Full Doxygen documentation at https://eternaldevelopments.com/docs

There is a basic unit test to validate the environment but more tests could be added.

I have no intention of maintaining backwards compatability, but will endeavor to mention if I make a breaking change. 

This utility appeals to the most niche aspects of development, but if you feel like making a donation, please send DOGE to DFbEt36Qg2s2CVAdk5hZgRJfH8p1g6tW9i or buy a [#programmerlife t-shirt](https://www.bonfire.com/store/programmer-life/)
