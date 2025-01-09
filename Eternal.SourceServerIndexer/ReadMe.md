# Eternal Source Server Indexer

## Eternal.SourceServerIndexer
Copyright 2024 Eternal Developments, LLC. All Rights Reserved.

## License

MIT

# Functionality
## SourceServerIndexer

This takes the gnarly decades old Perl script and reimplements as C# Net8.0. This allows logging and debugging of the process. It also automatically
detects the workspace and Perforce settings based on P4PORT and the working directory. The log will also show the connection information it finds making
debugging the process much easier.

Background: https://docs.microsoft.com/en-us/windows/win32/debug/source-server-and-source-indexing

The Debugging Tools for Windows are part of the Windows SDK, but not installed by default. Go to Settings -> Apps and Features -> Search for Windows Software Development Kit -> Select change from 
the drop down.

Microsoft documentation for installing Debugging Tools for Windows: https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/debugger-download-tools

The jist is that given a source server indexed pdb, the Visual Studio debugger will automatically sync up the correct source file from Perforce. When debugging minidumps (say from WER), if you know the change
the binary that caused the minidump was built from, you can sync up the entire stream. This can be very time comsuming for large codebases. If the pdb file source server indexed, the debugger will lazily 
retrieve only the required source files.

Using a Symbol Server: https://docs.microsoft.com/en-us/windows/win32/debug/using-symstore?redirectedfrom=MSDN

To create a symbol store: symstore add /s d:\\SymStore /f D:\\Symbols\\SymbolFile.pdb /t ProjectName

To test, enable source server support and diagnostics in Visual Studio in Options -> Debugging -> General. With the above enabled, you should see lines like this:

``` SRCSRV:  p4.exe -p server:1666 print -o "C:\\Users\\john.scott\\AppData\\Local\\SourceServer\\REPOSITORY\\Folder\\Folder\\FileName.cpp\\3\\FileName.cpp" -q "//stream/tasks/branch/Folder/Folder/FileName.cpp#3" ```

# Notes

Full Doxygen documentation at https://eternaldevelopments.com/docs

Epic has added Source Server support to their build suite (for a comparison) - check out \\Engine\\Source\\Programs\\AutomationTool\\Win\\Tasks\\SrcSrvTask.cs

There is a basic unit test to validate the environment but more tests could be added.

I have no intention of maintaining backwards compatability, but will endeavor to mention if I do. 

This utility appeals to the most niche aspects of development, but if you feel like making a donation, please send DOGE to DFbEt36Qg2s2CVAdk5hZgRJfH8p1g6tW9i or buy a [#programmerlife t-shirt](https://www.bonfire.com/store/programmer-life/)


