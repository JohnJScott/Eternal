# Eternal UTF16MustDie 

## Eternal.UTF16MustDie utility for Net6.0
Copyright 2022 Eternal Developments, LLC. All Rights Reserved.

## License

MIT

# Functionality
## UTF16MustDie

This utility does the following
* Finds the local Perforce workspace based on the local folder and P4PORT
 * P4PORT may need to be set for it to find the correct Perforce server
* Iterates only all files that are the filetype UTF-16, syncs to #head, and checks them out to a named changelist
* All pending changes (bar the default) are iterated over and any UTF-16 files are moved to another named changelist
* All files in the above changelists are iterated over
 * If the file is broken, it is fixed
 * The file is converted to UTF-8
 * The file type is changed to UTF-8
* Nothing is submitted by default


# Explanation of the problem

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

# Known Issues

* I have not worked out how to properly interrogate the default changelist so those files are not processed.

# Background

[There's no such thing as plain text](https://www.cqse.eu/en/news/blog/no-such-thing-as-plain-text/)

[The absolute minimum everyone should know](https://www.joelonsoftware.com/2003/10/08/the-absolute-minimum-every-software-developer-absolutely-positively-must-know-about-unicode-and-character-sets-no-excuses/)

[My coding standard mentions this](https://eternaldevelopments.com/Home/CodingStandard)

[Epic's thoughts on the subject](https://docs.unrealengine.com/4.26/en-US/ProgrammingAndScripting/ProgrammingWithCPP/UnrealArchitecture/StringHandling/CharacterEncoding/)

[The solution and best practice](https://utf8everywhere.org/)

# FAQ

Q: What if I require UTF-16 files?

A: Don't use this utility or make it so you don't require UTF-16. Also, see the utility name.

Q: What happens if I find a file that does not fix up properly?

A: Please email me the file (or a minimum representation of the problem) and I'll see what the problem is.

# Notes

Full Doxygen documentation at https://eternaldevelopments.com/docs

All functions have basic unit tests to avoid simple mistakes. I speak English, so the test cases are English based. Everything
should work with East Asian languages.

I have no intention of maintaining backwards compatability, but will endeavor to mention if the code no longer is backwards compatible.

This utility appeals to the most niche aspects of development, but if you feel like making a donation, 
please send DOGE to D5iPmmqhT2niGF6Q9BCb4u7RD4FPR1SFPh
