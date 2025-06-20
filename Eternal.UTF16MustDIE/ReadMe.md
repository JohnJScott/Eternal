# Eternal UTF16MustDie 

## Eternal.UTF16MustDie utility for Net9.0
Copyright 2025 Eternal Developments, LLC. All Rights Reserved.

## License

MIT

# Functionality
## UTF16MustDie (and ANSI Must Die now too)

* Command line options
	* -h - displays help information
	* -v - enables verbose logging
	* -skipUtf16 - skips processing files of type utf16
	* -skipANSI - skips processing files of type unicode

### Connects to the Perforce server.
* Finds the local Perforce workspace based on the local folder and P4PORT
   * P4PORT and P4USER may need to be set for it to find the correct Perforce server
   
### Fixes all UTF-16 files

* Iterates only all files that are the filetype UTF-16, syncs to \#head, and checks them out to a named changelist
* All pending changes (bar the default) are iterated over and any UTF-16 files are moved to another named changelist
* All files in the above changelists are iterated over
    * If the file is broken, it is fixed
    * The file is converted to UTF-8
    * The file type is changed to UTF-8
* Nothing is submitted

### Fixes all ANSI files (files of type Unicode)

* Iterates only all files that are the filetype Unicode, syncs to \#head, and checks them out to a named changelist
* All pending changes (bar the default) are iterated over and any Unicode files are moved to another named changelist
* All files in the above changelists are iterated over
    * The file is converted to UTF-8
    * The file type is changed to UTF-8
* Nothing is submitted

# Perforce filetypes

Based on [Base filetypes](https://www.perforce.com/manuals/cmdref/Content/CmdRef/file.types.synopsis.base.html) and my research.

* text - Text file
    * This is an arbitrary sequence of bytes. Tested by writing out 4096 random bytes as a string.
	* Line endings are converted to the local line endings on sync.
* binary - Non-text file
    * Used for files that cannot be text merged, such as images or audio files.
* symlink - Symbolic link
    * Causes issues on Windows machines - please do not use. 
* unicode - Unicode file - only available on Unicode enabled servers
    * The connection has a 'CharacterSetName' property which defines how files of type Unicode are stored on the client.
		* If this is UTF-8 (which I recommend), then all files of type Unicode are stored as UTF-8.
			* This means the app doesn't need to detect the encoding, it just needs to change the file type.
			* All of these files are stored as UTF-8 on the server.
		* If this is not UTF-8 (such as Windows-1252) then the client will try to process as ASCII plus the 'charsetname' high ASCII character set. 
			* It is checked against the P4CHARSET for having available characters. 
			* I would call this ANSI, and it may fail to translate some files (such as Shift-JIS).
			* I use UTFUnknown to detect the encoding in this case.
	* Line endings are converted to the local line endings on sync.
	* I would recommend disabling Unicode on your Perforce server.
* utf16 - Unicode file
    * UTF-16 files - either 2 bytes or 4 bytes. Not supporting the 4 byte version of UTF-16 means you are supporting UCS-2.
	* Line endings are converted to the local line endings on sync.
* utf8 - Unicode file
    * The one true encoding to rule them all, which is used to store all text based files on the server. 
	* Line endings are converted to the local line endings on sync.

I would like an ASCII filetype which would be characters 0x0 to 0x7f. That would be preferred for most code, and only use
UTF-8 for localized items. ASCII is a subset of UTF-8. Most of the Unicode code files I've found are could easily be ASCII but have smart quotes or use the copyright
symbol '©' rather than '(c)'.

# Explanation of the UTF-16 problem

Occasionally, you'll see UTF-16 files in Perforce that alternate between the correct English and Chinese (most likely random glyphs).
This is because when Perforce syncs a text file (such as code) from the server, it converts the line ending from the common server ending (0xa) to the local
system line ending (0xd 0xa for Windows). For a UTF-16 file, this means a single byte is inserted every line ending, hence effectively swapping the 
endianness of the file for each line. 

This is especially nefarious as you can check in a broken file, but your local version will be OK as there is no line end conversion done. Other
people syncing that same file will see the corruption.

Here's an example from DotNetZip:

```
using System.Reflection;
਍甀猀椀渀最 匀礀猀琀攀洀⸀刀甀渀琀椀洀攀⸀䌀漀洀瀀椀氀攀爀匀攀爀瘀椀挀攀猀㬀ഀഀ

਍⼀⼀ 䜀攀渀攀爀愀氀 䤀渀昀漀爀洀愀琀椀漀渀 愀戀漀甀琀 愀渀 愀猀猀攀洀戀氀礀 椀猀 挀漀渀琀爀漀氀氀攀搀 琀栀爀漀甀最栀 琀栀攀 昀漀氀氀漀眀椀渀最ഀഀ
// set of attributes. Change these attribute values to modify the information
਍⼀⼀ 愀猀猀漀挀椀愀琀攀搀 眀椀琀栀 愀渀 愀猀猀攀洀戀氀礀⸀ഀഀ

਍嬀愀猀猀攀洀戀氀礀㨀 䄀猀猀攀洀戀氀礀吀椀琀氀攀⠀∀䤀漀渀椀挀✀猀 䴀愀渀愀最攀搀 䈀娀椀瀀㈀ 昀漀爀 䌀漀洀瀀愀挀琀 䘀爀愀洀攀眀漀爀欀∀⤀崀ഀഀ
[assembly: AssemblyDescription("library for BZip2 compression, for Compact Framework. http://www.codeplex.com/DotNetZip")]
਍嬀愀猀猀攀洀戀氀礀㨀 䄀猀猀攀洀戀氀礀䌀漀渀昀椀最甀爀愀琀椀漀渀⠀∀∀⤀崀ഀഀ
```

# Changes 20th June 2025

Updated to .net9. Updated dependencies.

# Known Issues

* I have not worked out how to properly interrogate the default changelist so those files are not processed.

# Background

[There's no such thing as plain text](https://www.cqse.eu/en/news/blog/no-such-thing-as-plain-text/)

[The absolute minimum everyone should know](https://www.joelonsoftware.com/2003/10/08/the-absolute-minimum-every-software-developer-absolutely-positively-must-know-about-unicode-and-character-sets-no-excuses/)

[My coding standard mentions this](https://eternaldevelopments.com/Home/CodingStandard)

[Epic's thoughts on the subject](https://docs.unrealengine.com/4.26/en-US/ProgrammingAndScripting/ProgrammingWithCPP/UnrealArchitecture/StringHandling/CharacterEncoding/)

[The solution and best practice](https://utf8everywhere.org/)

[Perforce I18n Notes](https://www.perforce.com/perforce/doc.current/user/i18nnotes.txt)

# FAQ

Q: What if I require UTF-16 files?

A: Don't use this utility or make it so you don't require UTF-16. Also, see the utility name.

Q: What happens if I find a file that does not fix up properly?

A: Please email me the file (or a minimum representation of the problem) and I'll see what the problem is.

# Notes

Full Doxygen documentation at https://eternaldevelopments.com/docs

All functions have basic unit tests to avoid simple mistakes. I speak English, so the test cases are English based. Everything
should work with East Asian languages.

Due to the nature of this tool, the unit tests only work when this is checked into a Perforce server.

I have no intention of maintaining backwards compatability, but will endeavor to mention if the code no longer is backwards compatible.

This utility appeals to the most niche aspects of development, but if you feel like making a donation, please send DOGE to DFbEt36Qg2s2CVAdk5hZgRJfH8p1g6tW9i or buy a [#programmerlife t-shirt](https://www.bonfire.com/store/programmer-life/)
