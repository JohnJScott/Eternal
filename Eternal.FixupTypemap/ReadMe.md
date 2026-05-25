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

Nothing is ever submitted to the depot, so you can run the fix command without worrying about breaking anything. You can then review the changes and submit them when you're ready.

The untyped command will attempt to find any files that the typemap does not specify a file type for. However, the filespec exclusion does not seem to work currently, and I'm working on a fix.

The filter should match the depot path of a typemap line. e.g. '//....png'

# What isn't handled

Case sensitivity. All operations are treated as case insensitive.

More than just extensions. The typemap can specify paths based on more than just the file extension (e.g. '//.../ThirdParty/...'), but I've never see this used in practice, so I haven't implemented it. If you have a use case for this, please let me know and I can consider adding it.

# Changes 25th May 2026

Initial release.

# Notes

Full Doxygen documentation at https://eternaldevelopments.com/docs

All functions have basic unit tests to avoid simple mistakes.

I have no intention of maintaining backwards compatability, but will endeavor to mention if I make any changes.

This utility appeals to the most niche aspects of development, but if you feel like making a donation, please send DOGE to DFbEt36Qg2s2CVAdk5hZgRJfH8p1g6tW9i or buy a [#programmerlife t-shirt](https://www.bonfire.com/store/programmer-life/)

