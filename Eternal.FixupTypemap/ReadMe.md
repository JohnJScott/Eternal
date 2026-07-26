# Eternal FixupTypemap

## Eternal.FixupTypemap NuGet library package
Copyright Eternal Developments, LLC. All Rights Reserved.

## License

MIT

# Functionality

This iterates over a specific line or all lines of the typemap, compares the file type of the matching files in the depot to the file type specified in the typemap. 
If the file type doesn't match the one specified in the type map, it checks out the file and updates the file type.

The command format is: Eternal.FixupTypemap [report | fix | untyped] [filter]

The report command will only report the files that have mismatched file types, while the fix command will actually check out the files and update the file types.
Each change of filetype is marked as Safe, Unsafe, Warning, or Invalid.

Nothing is ever submitted to the depot, so you can run the fix command without worrying about breaking anything. You can then review the changes and submit them when you're ready.

The untyped command will list any files that the typemap does not specify a file type for. It will then list all the extensions of those files sorted by the number of instances.

!!CAVEAT!! Due to a limitation in the Perforce API, this finds missing typemap entries in the current stream, but the rest of the functionality works on the entire depot.

The filter should match the depot path of a typemap line. e.g. '//....png'

# What's this about Safe, Unsafe, Warning, and Invalid file type changes?

Assumption: Changing the file type modifier is always safe e.g. text to text+w

UTF-16 can not be converted to or from any other format (other than binary) as the content of the file would need to change. These are marked as 'Invalid'.

Text, Unicode (ANSI), and UTF8 are all single byte character sets but changing the file type is only strictly valid if they only contain ASCII characters (0x00 to 0x7f). A multibyte UTF8
character will change to multiple text/ANSI characters, and special ANSI characters will not be represented correctly in a UTF8 file. These changes are marked as 'Unsafe'. This is mostly
seen when text is copied from Word or Google Docs where the simple quotes are converted to smart quotes. The copyright and ellipsis symbols are other prime offenders. 

On submit, Perforce will reject the change with the error 'Translation of file content failed near line x'. To fix this, load in to Notepad++, Encoding menu, Convert to UTF-8, Save.

I recommend running another project of mine (UTF16MustDIE) on your repo to fix up most of the above issues. There is also a lot of background information as to why everything should be UTF8 
going forward. 

I work exclusively on Windows (with a smidgeon of Linux), so the 'Apple', 'Resource', and 'Symlink' file types do not appear anywhere in any of my repositories. Any changes to these types is marked as 'Warning'.

I've been working on fixing up my repositories, and I'm going to update UTF16MustDIE to handle the text file type too, as that is proving tedious to fix manually.

# What isn't handled

Case sensitivity. All operations are treated as case insensitive.

A typemap with more than just extensions. The typemap can specify paths based on more than just the file extension (e.g. '//.../ThirdParty/...'), but I've never see this used in practice, so I haven't implemented it. 
If you have a use case for this, please let me know and I can consider adding it.

Files without an extension. This evil Linux paradigm is just ignored as there's no way to determine the proper file type from an extension that doesn't exist.

TODO: Add unit tests.

# Changes 26th July 2026

Fixed untyped file finding by creating a fake client with exclusions. If there's anyone from Perforce reading this, updating GetFileMetaData() to support exclude patterns would be very much appreciated!
Packaged as a tool. 

# Changes 25th May 2026

Initial release.

# Notes

Full Doxygen documentation at https://eternaldevelopments.com/docs

All functions have basic unit tests to avoid simple mistakes.

I have no intention of maintaining backwards compatability, but will endeavor to mention if I make any changes.

This utility appeals to the most niche aspects of development, but if you feel like making a donation, please send DOGE to DFbEt36Qg2s2CVAdk5hZgRJfH8p1g6tW9i or buy a [#programmerlife t-shirt](https://www.bonfire.com/store/programmer-life/)

