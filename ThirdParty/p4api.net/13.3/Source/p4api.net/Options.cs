/*******************************************************************************

Copyright (c) 2011, Perforce Software, Inc.  All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1.  Redistributions of source code must retain the above copyright
	notice, this list of conditions and the following disclaimer.

2.  Redistributions in binary form must reproduce the above copyright
	notice, this list of conditions and the following disclaimer in the
	documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL PERFORCE SOFTWARE, INC. BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*******************************************************************************/

/*******************************************************************************
 * Name		: Options.cs
 *
 * Author(s)	: wjb, dbb
 *
 * Description	: Classes used to define command options
 *
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// A generic list of command options and values.
	/// </summary>
	public partial class Options : Dictionary<string, string>
	{
		/// <summary>
		/// Construct an blank Options object
		/// </summary>
		public Options() : base() { }

		public override string ToString()
		{
			StringBuilder buf = new StringBuilder(this.Count * 256);

			foreach (string key in this.Keys)
			{
				buf.Append(key);
				if (String.IsNullOrEmpty(this[key]) == false)
				{
					buf.Append(' ');
					buf.Append(this[key]);
				}
				buf.Append(' ');
			}
			return buf.ToString();
		}

		public static implicit operator StringList(Options o)
		{
			if (o == null)
				return null;

			StringList buf = new StringList();

			foreach (string key in o.Keys)
			{
				buf.Add(key);
				if (String.IsNullOrEmpty(o[key]) == false)
				{
					buf.Add(o[key]);
				}
			}
			return buf;
		}

		public StringList ToStringList()
		{
			StringList list = (StringList) this;
			return list;
		}
	}

	/// <summary>
	/// Flags for the add command.
	/// </summary>
	[Flags]
	public enum AddFilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// As a shortcut to reverting and re-adding, you can use the -d
		/// flag to reopen currently-open files for add (downgrade) under
		/// the following circumstances:
		///  
		///     A file that is 'opened for edit' and is synced to the head
		///     revision, and the head revision has been deleted (or moved).
		/// 
		///     A file that is 'opened for move/add' can be downgraded to add,
		///     which is useful when the source of the move has been deleted
		///     or moved.  Typically, under these circumstances, your only
		///     alternative is to revert.  In this case, breaking the move
		///     connection enables you to preserve any content changes in the
		///     new file and safely revert the source file (of the move).
		/// </summary>    
		Downgrade = 0x0001,
		/// <summary>
		/// To add files with filenames that contain wildcard characters, specify
		/// the -f flag. Filenames that contain the special characters '@', '#',
		/// '%' or '*' are reformatted to encode the characters using ASCII
		/// hexadecimal representation.
		/// </summary>
		KeepWildcards = 0x0002,
		/// <summary>
		/// The -I flag informs the client that it should not perform any ignore
		/// checking configured by P4IGNORE.
		/// </summary>
		NoP4Ignore = 0x0004,
		/// <summary>
		/// The -n flag, displays a preview of the specified add operation without
		/// changing any files or metadata.
		/// </summary>
		PreviewOnly = 0x00084
	};

	public partial class Options
	{
		/// <summary>
		///  Options for the Add command.
		/// </summary>
		/// <param name="flags">Flags for the command</param>
		/// <param name="changeList">Optional changelist for the fies</param>
		/// <param name="fileType">Optional file type for the files</param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help add</b>
		/// <br/> 
		/// <br/>     add -- Open a new file to add it to the depot
		/// <br/> 
		/// <br/>     p4 add [-c changelist#] [-d -f -n] [-t filetype] file ...
		/// <br/> 
		/// <br/> 	Open a file for adding to the depot.  If the file exists on the
		/// <br/> 	client, it is read to determine if it is text or binary. If it does
		/// <br/> 	not exist, it is assumed to be text.  To be added, the file must not
		/// <br/> 	already reside in the depot, or it must be deleted at the current
		/// <br/> 	head revision.  Files can be deleted and re-added.
		/// <br/> 
		/// <br/> 	To associate the open files with a specific pending changelist, use
		/// <br/> 	the -c flag; if you omit the -c flag, the open files are associated
		/// <br/> 	with the default changelist.  If file is already open, it is moved
		/// <br/> 	into the specified pending changelist.  You cannot reopen a file for
		/// <br/> 	add unless it is already open for add.
		/// <br/> 
		/// <br/> 	As a shortcut to reverting and re-adding, you can use the -d
		/// <br/> 	flag to reopen currently-open files for add (downgrade) under
		/// <br/> 	the following circumstances:
		/// <br/> 
		/// <br/> 	    A file that is 'opened for edit' and is synced to the head
		/// <br/> 	    revision, and the head revision has been deleted (or moved).
		/// <br/> 
		/// <br/> 	    A file that is 'opened for move/add' can be downgraded to add,
		/// <br/> 	    which is useful when the source of the move has been deleted
		/// <br/> 	    or moved.  Typically, under these circumstances, your only
		/// <br/> 	    alternative is to revert.  In this case, breaking the move
		/// <br/> 	    connection enables you to preserve any content changes in the
		/// <br/> 	    new file and safely revert the source file (of the move).
		/// <br/> 
		/// <br/> 	To specify file type, use the -t flag.  By default, 'p4 add'
		/// <br/> 	determines file type using the name-to-type mapping table managed
		/// <br/> 	by 'p4 typemap' and by examining the file's contents and execute
		/// <br/> 	permission bit. If the file type specified by -t or configured in
		/// <br/> 	the typemap table is a partial filetype, the resulting modifier is
		/// <br/> 	applied to the file type that is determined by 'p4 add'. For more
		/// <br/> 	details, see 'p4 help filetypes'.
		/// <br/> 
		/// <br/> 	To add files with filenames that contain wildcard characters, specify
		/// <br/> 	the -f flag. Filenames that contain the special characters '@', '#',
		/// <br/> 	'%' or '*' are reformatted to encode the characters using ASCII
		/// <br/> 	hexadecimal representation.  After the files are added, you must
		/// <br/> 	refer to them using the reformatted file name, because Perforce
		/// <br/> 	does not recognize the local filesystem name.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of the specified add operation without
		/// <br/> 	changing any files or metadata.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(AddFilesCmdFlags flags, int changeList, FileType fileType)
		{
			if (changeList >= 0)
			{
				this["-c"] = changeList.ToString();
			}
			if (flags == AddFilesCmdFlags.Downgrade)
			{
				this["-d"] = null;
			}
			if (flags == AddFilesCmdFlags.KeepWildcards)
			{
				this["-f"] = null;
			}
			if (flags == AddFilesCmdFlags.NoP4Ignore)
			{
				this["-I"] = null;
			}
			if (flags == AddFilesCmdFlags.PreviewOnly)
			{
				this["-n"] = null;
			}
			if (fileType != null)
			{
				this["-t"] = fileType.ToString();
			}
		}
	}
	/// <summary>
	/// Add command options
	/// </summary>
	public class AddFilesCmdOptions : Options
	{
		/// <summary>
		///  Options for the Add command.
		/// </summary>
		/// <param name="flags">Flags for the command</param>
		/// <param name="changeList">Optional changelist for the fies</param>
		/// <param name="fileType">Optional file type for the files</param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help add</b>
		/// <br/> 
		/// <br/>     add -- Open a new file to add it to the depot
		/// <br/> 
		/// <br/>     p4 add [-c changelist#] [-d -f -n] [-t filetype] file ...
		/// <br/> 
		/// <br/> 	Open a file for adding to the depot.  If the file exists on the
		/// <br/> 	client, it is read to determine if it is text or binary. If it does
		/// <br/> 	not exist, it is assumed to be text.  To be added, the file must not
		/// <br/> 	already reside in the depot, or it must be deleted at the current
		/// <br/> 	head revision.  Files can be deleted and re-added.
		/// <br/> 
		/// <br/> 	To associate the open files with a specific pending changelist, use
		/// <br/> 	the -c flag; if you omit the -c flag, the open files are associated
		/// <br/> 	with the default changelist.  If file is already open, it is moved
		/// <br/> 	into the specified pending changelist.  You cannot reopen a file for
		/// <br/> 	add unless it is already open for add.
		/// <br/> 
		/// <br/> 	As a shortcut to reverting and re-adding, you can use the -d
		/// <br/> 	flag to reopen currently-open files for add (downgrade) under
		/// <br/> 	the following circumstances:
		/// <br/> 
		/// <br/> 	    A file that is 'opened for edit' and is synced to the head
		/// <br/> 	    revision, and the head revision has been deleted (or moved).
		/// <br/> 
		/// <br/> 	    A file that is 'opened for move/add' can be downgraded to add,
		/// <br/> 	    which is useful when the source of the move has been deleted
		/// <br/> 	    or moved.  Typically, under these circumstances, your only
		/// <br/> 	    alternative is to revert.  In this case, breaking the move
		/// <br/> 	    connection enables you to preserve any content changes in the
		/// <br/> 	    new file and safely revert the source file (of the move).
		/// <br/> 
		/// <br/> 	To specify file type, use the -t flag.  By default, 'p4 add'
		/// <br/> 	determines file type using the name-to-type mapping table managed
		/// <br/> 	by 'p4 typemap' and by examining the file's contents and execute
		/// <br/> 	permission bit. If the file type specified by -t or configured in
		/// <br/> 	the typemap table is a partial filetype, the resulting modifier is
		/// <br/> 	applied to the file type that is determined by 'p4 add'. For more
		/// <br/> 	details, see 'p4 help filetypes'.
		/// <br/> 
		/// <br/> 	To add files with filenames that contain wildcard characters, specify
		/// <br/> 	the -f flag. Filenames that contain the special characters '@', '#',
		/// <br/> 	'%' or '*' are reformatted to encode the characters using ASCII
		/// <br/> 	hexadecimal representation.  After the files are added, you must
		/// <br/> 	refer to them using the reformatted file name, because Perforce
		/// <br/> 	does not recognize the local filesystem name.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of the specified add operation without
		/// <br/> 	changing any files or metadata.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public AddFilesCmdOptions(AddFilesCmdFlags flags, int changeList, FileType fileType)
			: base(flags, changeList, fileType) { }
	}
	
	/// <summary>
	/// Flags for the delete command.
	/// </summary>
	[Flags]
	public enum DeleteFilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// The -n flag, displays a preview of the operation without changing any
		/// files or metadata.
		/// </summary>
		PreviewOnly = 0x0001,
		/// <summary>
		/// The -v flag, enables you to delete files that are not synced to the
		/// client workspace.
		/// </summary>
		DeleteUnsynced = 0x0002,
		/// <summary>
		/// The -k flag performs the delete on the server without modifying
		/// client files.  Use with caution, as an incorrect delete can cause
		/// discrepancies between the state of the client and the corresponding
		/// server metadata.
		/// </summary>
		ServerOnly = 0x004
	};

	public partial class Options
	{
		/// <summary>
		/// Options for the delete command.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changeList"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help delete</b>
		/// <br/> 
		/// <br/>     delete -- Open an existing file for deletion from the depot
		/// <br/> 
		/// <br/>     p4 delete [-c changelist#] [-n -v -k] file ...
		/// <br/> 
		/// <br/> 	Opens a depot file for deletion.
		/// <br/> 	If the file is synced in the client workspace, it is removed.  If a
		/// <br/> 	pending changelist number is specified using with the -c flag, the
		/// <br/> 	file is opened for delete in that changelist. Otherwise, it is opened
		/// <br/> 	in the default pending changelist.
		/// <br/> 
		/// <br/> 	Files that are deleted generally do not appear on the have list.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of the operation without changing any
		/// <br/> 	files or metadata.
		/// <br/> 
		/// <br/> 	The -v flag enables you to delete files that are not synced to the
		/// <br/> 	client workspace.
		/// <br/> 
		/// <br/> 	The -k flag performs the delete on the server without modifying
		/// <br/> 	client files.  Use with caution, as an incorrect delete can cause
		/// <br/> 	discrepancies between the state of the client and the corresponding
		/// <br/> 	server metadata.
		/// <br/> 
		/// </remarks>
		public Options(DeleteFilesCmdFlags flags, int changeList)
		{
			if (changeList >= 0)
			{
				this["-c"] = changeList.ToString();
			}
			if (flags == DeleteFilesCmdFlags.PreviewOnly)
			{
				this["-n"] = null;
			}
			if (flags == DeleteFilesCmdFlags.DeleteUnsynced)
			{
				this["-v"] = null;
			}
			if (flags == DeleteFilesCmdFlags.ServerOnly)
			{
				this["-k"] = null;
			}
		}
	}
	/// <summary>
	/// delete command options
	/// </summary>
	public class DeleteFilesCmdOptions : Options
	{
		/// <summary>
		/// Options for the delete command.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changeList"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help delete</b>
		/// <br/> 
		/// <br/>     delete -- Open an existing file for deletion from the depot
		/// <br/> 
		/// <br/>     p4 delete [-c changelist#] [-n -v -k] file ...
		/// <br/> 
		/// <br/> 	Opens a depot file for deletion.
		/// <br/> 	If the file is synced in the client workspace, it is removed.  If a
		/// <br/> 	pending changelist number is specified using with the -c flag, the
		/// <br/> 	file is opened for delete in that changelist. Otherwise, it is opened
		/// <br/> 	in the default pending changelist.
		/// <br/> 
		/// <br/> 	Files that are deleted generally do not appear on the have list.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of the operation without changing any
		/// <br/> 	files or metadata.
		/// <br/> 
		/// <br/> 	The -v flag enables you to delete files that are not synced to the
		/// <br/> 	client workspace.
		/// <br/> 
		/// <br/> 	The -k flag performs the delete on the server without modifying
		/// <br/> 	client files.  Use with caution, as an incorrect delete can cause
		/// <br/> 	discrepancies between the state of the client and the corresponding
		/// <br/> 	server metadata.
		/// <br/> 
		/// </remarks>
		public DeleteFilesCmdOptions(DeleteFilesCmdFlags flags, int changeList)
			: base(flags, changeList) { }
	}

	/// <summary>
	/// Flags for the edit command.
	/// </summary>
	[Flags]
	public enum EditFilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// The -n flag, previews the operation without changing any files or
		/// metadata.
		/// </summary>
		PreviewOnly = 0x0001,
		/// <summary>
		/// The -k flag, updates metadata without transferring files to the
		/// workspace.
		/// 
		/// </summary>
		ServerOnly = 0x0002,
	};

	public partial class Options
	{
		/// <summary>
		/// Options for the edit command
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changeList"></param>
		/// <param name="fileType"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help edit</b>
		/// <br/> 
		/// <br/>     edit -- Open an existing file for edit
		/// <br/> 
		/// <br/>     p4 edit [-c changelist#] [-k -n] [-t filetype] file ...
		/// <br/> 
		/// <br/> 	Open an existing file for edit.  The server records the fact that
		/// <br/> 	the current user has opened the file in the current workspace, and
		/// <br/> 	changes the file permission from read-only to read/write.
		/// <br/> 
		/// <br/> 	If -c changelist# is included, the file opened in the specified
		/// <br/> 	pending changelist.  If changelist number is omitted, the file is
		/// <br/> 	opened in the 'default' changelist.
		/// <br/> 
		/// <br/> 	If -t filetype is specified, the file is assigned that Perforce
		/// <br/> 	filetype. Otherwise, the filetype of the previous revision is reused.
		/// <br/> 	If a partial filetype is specified, it is combined with the current
		/// <br/> 	filetype.For details, see 'p4 help filetypes'.
		/// <br/> 	Using a filetype of 'auto' will cause the filetype to be choosen
		/// <br/> 	as if the file were being added, that is the typemap will be
		/// <br/> 	considered and the file contents may be examined.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without changing any files or
		/// <br/> 	metadata.
		/// <br/> 
		/// <br/> 	The -k flag updates metadata without transferring files to the
		/// <br/> 	workspace. This option can be used to tell the server that files in
		/// <br/> 	a client workspace are already editable, even if they are not in the
		/// <br/> 	client view. Typically this flag is used to correct the Perforce
		/// <br/> 	server when it is wrong about the state of files in the client
		/// <br/> 	workspace, but incorrect use of this option can result in inaccurate
		/// <br/> 	file status information.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(EditFilesCmdFlags flags, int changeList, FileType fileType)
		{
			if (changeList > 0)
			{
				this["-c"] = changeList.ToString();
			}
			if (flags == EditFilesCmdFlags.ServerOnly)
			{
				this["-k"] = null;
			}
			if (flags == EditFilesCmdFlags.PreviewOnly)
			{
				this["-n"] = null;
			}
			if (fileType != null)
			{
				this["-t"] = fileType.ToString();
			}
		}
	}
	/// <summary>
	/// Options for the edit command
	/// </summary>
	public class EditCmdOptions : Options
	{
		/// <summary>
		/// Options for the edit command
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changeList"></param>
		/// <param name="fileType"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help edit</b>
		/// <br/> 
		/// <br/>     edit -- Open an existing file for edit
		/// <br/> 
		/// <br/>     p4 edit [-c changelist#] [-k -n] [-t filetype] file ...
		/// <br/> 
		/// <br/> 	Open an existing file for edit.  The server records the fact that
		/// <br/> 	the current user has opened the file in the current workspace, and
		/// <br/> 	changes the file permission from read-only to read/write.
		/// <br/> 
		/// <br/> 	If -c changelist# is included, the file opened in the specified
		/// <br/> 	pending changelist.  If changelist number is omitted, the file is
		/// <br/> 	opened in the 'default' changelist.
		/// <br/> 
		/// <br/> 	If -t filetype is specified, the file is assigned that Perforce
		/// <br/> 	filetype. Otherwise, the filetype of the previous revision is reused.
		/// <br/> 	If a partial filetype is specified, it is combined with the current
		/// <br/> 	filetype.For details, see 'p4 help filetypes'.
		/// <br/> 	Using a filetype of 'auto' will cause the filetype to be choosen
		/// <br/> 	as if the file were being added, that is the typemap will be
		/// <br/> 	considered and the file contents may be examined.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without changing any files or
		/// <br/> 	metadata.
		/// <br/> 
		/// <br/> 	The -k flag updates metadata without transferring files to the
		/// <br/> 	workspace. This option can be used to tell the server that files in
		/// <br/> 	a client workspace are already editable, even if they are not in the
		/// <br/> 	client view. Typically this flag is used to correct the Perforce
		/// <br/> 	server when it is wrong about the state of files in the client
		/// <br/> 	workspace, but incorrect use of this option can result in inaccurate
		/// <br/> 	file status information.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public EditCmdOptions(EditFilesCmdFlags flags, int changeList, FileType fileType)
			:base(flags, changeList, fileType) {}
	}

	/// <summary>
	/// Flags for the integrate command.
	/// </summary>
	[Flags]
	public enum IntegrateFilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// The -f flag forces integrate to ignore previous integration history.
		/// <br/><b>p4 help integrate</b>
		/// <br/> 
		/// <br/>     integrate -- Integrate one set of files into another
		/// <br/> 
		/// <br/>     p4 integrate [options] fromFile[revRange] toFile
		/// <br/>     p4 integrate [options] -b branch [-r] [toFile[revRange] ...]
		/// <br/>     p4 integrate [options] -b branch -s fromFile[revRange] [toFile ...]
		/// <br/>     p4 integrate [options] -S stream [-r] [-P parent] [file[revRange] ...]
		/// <br/> 
		/// <br/> 	options: -c changelist# -d -f -h -i -o -n -m max -t -v
		/// <br/> 	         -D&lt;flags&gt; -R&lt;flags&gt;
		/// <br/> 
		/// <br/> 	'p4 integrate' integrates one set of files (the 'source') into
		/// <br/> 	another (the 'target'). 
		/// <br/> 
		/// <br/> 	(See also 'p4 merge' and 'p4 copy', variants of 'p4 integrate' that
		/// <br/> 	may be easier and more effective for the task at hand.) 
		/// <br/> 
		/// <br/> 	Using the client workspace as a staging area, 'p4 integrate' adds and
		/// <br/> 	deletes target files per changes in the source, and schedules all
		/// <br/> 	other affected target files to be resolved.  Target files outside of
		/// <br/> 	the current client view are not affected. Source files need not be
		/// <br/> 	within the client view.
		/// <br/> 
		/// <br/> 	'p4 resolve' must be used to merge file content, and to resolve
		/// <br/> 	filename and filetype changes. 'p4 submit' commits integrated files
		/// <br/> 	to the depot.  Unresolved files may not be submitted.  Integrations
		/// <br/> 	can be shelved with 'p4 shelve' and abandoned with 'p4 revert'.  The
		/// <br/> 	commands 'p4 integrated' and 'p4 filelog' display integration history.
		/// <br/> 
		/// <br/> 	When 'p4 integrate' schedules a workspace file to be resolved, it
		/// <br/> 	leaves it read-only. 'p4 resolve' can operate on a read-only file.
		/// <br/> 	For other pre-submit changes, 'p4 edit' must be used to make the
		/// <br/> 	file writable.
		/// <br/> 
		/// <br/> 	Source and target files can be specified either on the 'p4 integrate'
		/// <br/> 	command line or through a branch view. On the command line, fromFile
		/// <br/> 	is the source file set and toFile is the target file set.  With a
		/// <br/> 	branch view, one or more toFile arguments can be given to limit the
		/// <br/> 	scope of the target file set.
		/// <br/> 
		/// <br/> 	revRange is a revision or a revision range that limits the span of
		/// <br/> 	source history to be probed for unintegrated revisions.  revRange
		/// <br/> 	can be used on fromFile, or on toFile, but not on both.  When used on
		/// <br/> 	toFile, it refers to source revisions, not to target revisions.  For
		/// <br/> 	details about revision specifiers, see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -S flag makes 'p4 integrate' use a stream's branch view.  (See
		/// <br/> 	'p4 help stream'.) The source is the stream itself, and the target is
		/// <br/> 	the stream's parent. With -r, the direction is reversed.  -P can be
		/// <br/> 	used to specify a parent stream other than the stream's actual parent.
		/// <br/> 	Note that to submit integrated stream files, the current client must
		/// <br/> 	be dedicated to the target stream. (See 'p4 help client'.)
		/// <br/> 
		/// <br/> 	The -b flag makes 'p4 integrate' use a user-defined branch view.
		/// <br/> 	(See 'p4 help branch'.) The source is the left side of the branch view
		/// <br/> 	and the target is the right side. With -r, the direction is reversed.
		/// <br/> 
		/// <br/> 	The -s flag can be used with -b to cause fromFile to be treated as
		/// <br/> 	the source, and both sides of the branch view to be treated as the
		/// <br/> 	target, per the branch view mapping.  Optional toFile arguments may
		/// <br/> 	be given to further restrict the scope of the target file set.  The
		/// <br/> 	-r flag is ignored when -s is used.
		/// <br/> 
		/// <br/> 	Note that 'p4 integrate' automatically adusts source-to-target
		/// <br/> 	mappings for moved and renamed files.  (Adjustment occurs only if
		/// <br/> 	the 'p4 move' command was used to move/rename files.) The scope of
		/// <br/> 	source and target file sets must include both the old-named and the
		/// <br/> 	new-named files for mappings to be adjusted.  Moved source files may
		/// <br/> 	cause target files to be scheduled for filename resolves.
		/// <br/> 
		/// <br/> 	The -f flag forces integrate to ignore integration history and treat
		/// <br/> 	all source revisions as unintegrated. It is meant to be used with
		/// <br/> 	revRange to force reintegration of specific, previously integrated
		/// <br/> 	revisions. 
		/// <br/> 
		/// <br/> 	The -i flag enables merging between files that have no prior
		/// <br/> 	integration history.  By default, 'p4 integrate' requires a prior
		/// <br/> 	integration in order to identify a base for merging.  The -i flag
		/// <br/> 	allows the integration, and schedules the target file to be resolved
		/// <br/> 	using the first source revision as the merge base.
		/// <br/> 
		/// <br/> 	The -o flag causes more merge information to be output.  For each
		/// <br/> 	target file scheduled to be resolved, the base file revision and the
		/// <br/> 	source file revision are shown. (After running 'p4 integrate', the
		/// <br/> 	same information is available from 'p4 resolve -o'.)
		/// <br/> 
		/// <br/> 	The -R flags modify the way resolves are scheduled:
		/// <br/> 
		/// <br/> 		-Rb	Schedules 'branch resolves' instead of branching new
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rd	Schedules 'delete resolves' instead of deleting
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rs	Skips cherry-picked revisions already integrated.
		/// <br/> 			This can improve merge results, but can also cause
		/// <br/> 			multiple resolves per file to be scheduled.
		/// <br/> 
		/// <br/> 	The -D flags modify the way deleted files are treated:
		/// <br/> 
		/// <br/> 		-Dt     If the target file has been deleted and the source
		/// <br/> 			file has changed, re-branch the source file on top
		/// <br/> 			of the target file instead of scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Ds     If the source file has been deleted and the target
		/// <br/> 			file has changed, delete the target file instead of
		/// <br/> 			scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Di	If the source file has been deleted and re-added,
		/// <br/> 			probe revisions that precede the deletion to find
		/// <br/> 			unintegrated revisions. By default, 'p4 integrate'
		/// <br/> 			starts probing at the last re-added revision.
		/// <br/> 
		/// <br/> 	The -d flag is a shorthand for all -D flags used together.
		/// <br/> 
		/// <br/> 	The -h flag leaves the target files at the revision currently synced
		/// <br/> 	to the client (the '#have' revision). By default, target files are
		/// <br/> 	automatically synced to the head revision by 'p4 integrate'.
		/// <br/> 
		/// <br/> 	The -t flag propagates source filetypes instead of scheduling
		/// <br/> 	filetype conflicts to be resolved.
		/// <br/> 
		/// <br/> 	The -m flag limits integration to the first 'max' number of files.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of integration, without actually
		/// <br/> 	doing anything.
		/// <br/> 
		/// <br/> 	If -c changelist# is specified, the files are opened in the
		/// <br/> 	designated numbered pending changelist instead of the 'default'
		/// <br/> 	changelist.
		/// <br/> 
		/// <br/> 	The -v flag causes a 'virtual' integration that does not modify
		/// <br/> 	client workspace files unless target files need to be resolved.
		/// <br/> 	After submitting a virtual integration, 'p4 sync' can be used to
		/// <br/> 	update the workspace.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		Force = 0x0001,
		/// <summary>
		/// -Dt     If the target file has been deleted and the source
		///         file has changed, re-branch the source file on top
		///         of the target file.
		/// </summary>
		BranchIfTargetDeleted = 0x0002,
		/// <summary>
		/// -Ds     If the source file has been deleted and the target
		///         file has changed, delete the target file.
		/// </summary>
		DeleteIfSourceDeleted = 0x0004,
		/// <summary>
		/// -Di     If the source file has been deleted and re-added,
		///         attempt to integrate all outstanding revisions
		///         of the file, including revisions prior to the
		///         delete. By default, 'p4 integrate' only considers
		///         revisions since the last add.
		/// </summary>
		IntegrateAllIfSourceDeleted = 0x0008,
		/// <summary>
		/// The -h flag leaves the target files at the revision currently synced
		/// to the client (the '#have' revision). By default, target files are
		/// automatically synced to the head revision by 'p4 integrate'.
		/// </summary>
		LeaveHaveVersion = 0x0010,
		/// <summary>
		/// The -i flag enables integration between files that have no integration
		/// <br/><b>p4 help integrate</b>
		/// <br/> 
		/// <br/>     integrate -- Integrate one set of files into another
		/// <br/> 
		/// <br/>     p4 integrate [options] fromFile[revRange] toFile
		/// <br/>     p4 integrate [options] -b branch [-r] [toFile[revRange] ...]
		/// <br/>     p4 integrate [options] -b branch -s fromFile[revRange] [toFile ...]
		/// <br/>     p4 integrate [options] -S stream [-r] [-P parent] [file[revRange] ...]
		/// <br/> 
		/// <br/> 	options: -c changelist# -d -f -h -i -o -n -m max -t -v
		/// <br/> 	         -D&lt;flags&gt; -R&lt;flags&gt;
		/// <br/> 
		/// <br/> 	'p4 integrate' integrates one set of files (the 'source') into
		/// <br/> 	another (the 'target'). 
		/// <br/> 
		/// <br/> 	(See also 'p4 merge' and 'p4 copy', variants of 'p4 integrate' that
		/// <br/> 	may be easier and more effective for the task at hand.) 
		/// <br/> 
		/// <br/> 	Using the client workspace as a staging area, 'p4 integrate' adds and
		/// <br/> 	deletes target files per changes in the source, and schedules all
		/// <br/> 	other affected target files to be resolved.  Target files outside of
		/// <br/> 	the current client view are not affected. Source files need not be
		/// <br/> 	within the client view.
		/// <br/> 
		/// <br/> 	'p4 resolve' must be used to merge file content, and to resolve
		/// <br/> 	filename and filetype changes. 'p4 submit' commits integrated files
		/// <br/> 	to the depot.  Unresolved files may not be submitted.  Integrations
		/// <br/> 	can be shelved with 'p4 shelve' and abandoned with 'p4 revert'.  The
		/// <br/> 	commands 'p4 integrated' and 'p4 filelog' display integration history.
		/// <br/> 
		/// <br/> 	When 'p4 integrate' schedules a workspace file to be resolved, it
		/// <br/> 	leaves it read-only. 'p4 resolve' can operate on a read-only file.
		/// <br/> 	For other pre-submit changes, 'p4 edit' must be used to make the
		/// <br/> 	file writable.
		/// <br/> 
		/// <br/> 	Source and target files can be specified either on the 'p4 integrate'
		/// <br/> 	command line or through a branch view. On the command line, fromFile
		/// <br/> 	is the source file set and toFile is the target file set.  With a
		/// <br/> 	branch view, one or more toFile arguments can be given to limit the
		/// <br/> 	scope of the target file set.
		/// <br/> 
		/// <br/> 	revRange is a revision or a revision range that limits the span of
		/// <br/> 	source history to be probed for unintegrated revisions.  revRange
		/// <br/> 	can be used on fromFile, or on toFile, but not on both.  When used on
		/// <br/> 	toFile, it refers to source revisions, not to target revisions.  For
		/// <br/> 	details about revision specifiers, see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -S flag makes 'p4 integrate' use a stream's branch view.  (See
		/// <br/> 	'p4 help stream'.) The source is the stream itself, and the target is
		/// <br/> 	the stream's parent. With -r, the direction is reversed.  -P can be
		/// <br/> 	used to specify a parent stream other than the stream's actual parent.
		/// <br/> 	Note that to submit integrated stream files, the current client must
		/// <br/> 	be dedicated to the target stream. (See 'p4 help client'.)
		/// <br/> 
		/// <br/> 	The -b flag makes 'p4 integrate' use a user-defined branch view.
		/// <br/> 	(See 'p4 help branch'.) The source is the left side of the branch view
		/// <br/> 	and the target is the right side. With -r, the direction is reversed.
		/// <br/> 
		/// <br/> 	The -s flag can be used with -b to cause fromFile to be treated as
		/// <br/> 	the source, and both sides of the branch view to be treated as the
		/// <br/> 	target, per the branch view mapping.  Optional toFile arguments may
		/// <br/> 	be given to further restrict the scope of the target file set.  The
		/// <br/> 	-r flag is ignored when -s is used.
		/// <br/> 
		/// <br/> 	Note that 'p4 integrate' automatically adusts source-to-target
		/// <br/> 	mappings for moved and renamed files.  (Adjustment occurs only if
		/// <br/> 	the 'p4 move' command was used to move/rename files.) The scope of
		/// <br/> 	source and target file sets must include both the old-named and the
		/// <br/> 	new-named files for mappings to be adjusted.  Moved source files may
		/// <br/> 	cause target files to be scheduled for filename resolves.
		/// <br/> 
		/// <br/> 	The -f flag forces integrate to ignore integration history and treat
		/// <br/> 	all source revisions as unintegrated. It is meant to be used with
		/// <br/> 	revRange to force reintegration of specific, previously integrated
		/// <br/> 	revisions. 
		/// <br/> 
		/// <br/> 	The -i flag enables merging between files that have no prior
		/// <br/> 	integration history.  By default, 'p4 integrate' requires a prior
		/// <br/> 	integration in order to identify a base for merging.  The -i flag
		/// <br/> 	allows the integration, and schedules the target file to be resolved
		/// <br/> 	using the first source revision as the merge base.
		/// <br/> 
		/// <br/> 	The -o flag causes more merge information to be output.  For each
		/// <br/> 	target file scheduled to be resolved, the base file revision and the
		/// <br/> 	source file revision are shown. (After running 'p4 integrate', the
		/// <br/> 	same information is available from 'p4 resolve -o'.)
		/// <br/> 
		/// <br/> 	The -R flags modify the way resolves are scheduled:
		/// <br/> 
		/// <br/> 		-Rb	Schedules 'branch resolves' instead of branching new
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rd	Schedules 'delete resolves' instead of deleting
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rs	Skips cherry-picked revisions already integrated.
		/// <br/> 			This can improve merge results, but can also cause
		/// <br/> 			multiple resolves per file to be scheduled.
		/// <br/> 
		/// <br/> 	The -D flags modify the way deleted files are treated:
		/// <br/> 
		/// <br/> 		-Dt     If the target file has been deleted and the source
		/// <br/> 			file has changed, re-branch the source file on top
		/// <br/> 			of the target file instead of scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Ds     If the source file has been deleted and the target
		/// <br/> 			file has changed, delete the target file instead of
		/// <br/> 			scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Di	If the source file has been deleted and re-added,
		/// <br/> 			probe revisions that precede the deletion to find
		/// <br/> 			unintegrated revisions. By default, 'p4 integrate'
		/// <br/> 			starts probing at the last re-added revision.
		/// <br/> 
		/// <br/> 	The -d flag is a shorthand for all -D flags used together.
		/// <br/> 
		/// <br/> 	The -h flag leaves the target files at the revision currently synced
		/// <br/> 	to the client (the '#have' revision). By default, target files are
		/// <br/> 	automatically synced to the head revision by 'p4 integrate'.
		/// <br/> 
		/// <br/> 	The -t flag propagates source filetypes instead of scheduling
		/// <br/> 	filetype conflicts to be resolved.
		/// <br/> 
		/// <br/> 	The -m flag limits integration to the first 'max' number of files.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of integration, without actually
		/// <br/> 	doing anything.
		/// <br/> 
		/// <br/> 	If -c changelist# is specified, the files are opened in the
		/// <br/> 	designated numbered pending changelist instead of the 'default'
		/// <br/> 	changelist.
		/// <br/> 
		/// <br/> 	The -v flag causes a 'virtual' integration that does not modify
		/// <br/> 	client workspace files unless target files need to be resolved.
		/// <br/> 	After submitting a virtual integration, 'p4 sync' can be used to
		/// <br/> 	update the workspace.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		IntegrateUnrelated = 0x0020,
		/// <summary>
		/// The -o flag displays the base file name and revision that will be
		/// used in subsequent resolves if a resolve is needed.
		/// </summary>
		DisplayBaseFile = 0x0040,
		/// <summary>
		/// The -n flag displays a preview of required integrations.
		/// </summary>
		PreviewIntegrationsOnly = 0x0080,
		/// <summary>
		/// The -r flag reverses the mappings in the branch view, with the
		/// target files and source files exchanging place.  The -b branch
		/// flag is required.
		/// </summary>
		SwapSourceAndTarget = 0x0100,
		/// <summary>
		/// The -s fromFile[revRange] flag causes the branch view to work
		/// <br/><b>p4 help integrate</b>
		/// <br/> 
		/// <br/>     integrate -- Integrate one set of files into another
		/// <br/> 
		/// <br/>     p4 integrate [options] fromFile[revRange] toFile
		/// <br/>     p4 integrate [options] -b branch [-r] [toFile[revRange] ...]
		/// <br/>     p4 integrate [options] -b branch -s fromFile[revRange] [toFile ...]
		/// <br/>     p4 integrate [options] -S stream [-r] [-P parent] [file[revRange] ...]
		/// <br/> 
		/// <br/> 	options: -c changelist# -d -f -h -i -o -n -m max -t -v
		/// <br/> 	         -D&lt;flags&gt; -R&lt;flags&gt;
		/// <br/> 
		/// <br/> 	'p4 integrate' integrates one set of files (the 'source') into
		/// <br/> 	another (the 'target'). 
		/// <br/> 
		/// <br/> 	(See also 'p4 merge' and 'p4 copy', variants of 'p4 integrate' that
		/// <br/> 	may be easier and more effective for the task at hand.) 
		/// <br/> 
		/// <br/> 	Using the client workspace as a staging area, 'p4 integrate' adds and
		/// <br/> 	deletes target files per changes in the source, and schedules all
		/// <br/> 	other affected target files to be resolved.  Target files outside of
		/// <br/> 	the current client view are not affected. Source files need not be
		/// <br/> 	within the client view.
		/// <br/> 
		/// <br/> 	'p4 resolve' must be used to merge file content, and to resolve
		/// <br/> 	filename and filetype changes. 'p4 submit' commits integrated files
		/// <br/> 	to the depot.  Unresolved files may not be submitted.  Integrations
		/// <br/> 	can be shelved with 'p4 shelve' and abandoned with 'p4 revert'.  The
		/// <br/> 	commands 'p4 integrated' and 'p4 filelog' display integration history.
		/// <br/> 
		/// <br/> 	When 'p4 integrate' schedules a workspace file to be resolved, it
		/// <br/> 	leaves it read-only. 'p4 resolve' can operate on a read-only file.
		/// <br/> 	For other pre-submit changes, 'p4 edit' must be used to make the
		/// <br/> 	file writable.
		/// <br/> 
		/// <br/> 	Source and target files can be specified either on the 'p4 integrate'
		/// <br/> 	command line or through a branch view. On the command line, fromFile
		/// <br/> 	is the source file set and toFile is the target file set.  With a
		/// <br/> 	branch view, one or more toFile arguments can be given to limit the
		/// <br/> 	scope of the target file set.
		/// <br/> 
		/// <br/> 	revRange is a revision or a revision range that limits the span of
		/// <br/> 	source history to be probed for unintegrated revisions.  revRange
		/// <br/> 	can be used on fromFile, or on toFile, but not on both.  When used on
		/// <br/> 	toFile, it refers to source revisions, not to target revisions.  For
		/// <br/> 	details about revision specifiers, see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -S flag makes 'p4 integrate' use a stream's branch view.  (See
		/// <br/> 	'p4 help stream'.) The source is the stream itself, and the target is
		/// <br/> 	the stream's parent. With -r, the direction is reversed.  -P can be
		/// <br/> 	used to specify a parent stream other than the stream's actual parent.
		/// <br/> 	Note that to submit integrated stream files, the current client must
		/// <br/> 	be dedicated to the target stream. (See 'p4 help client'.)
		/// <br/> 
		/// <br/> 	The -b flag makes 'p4 integrate' use a user-defined branch view.
		/// <br/> 	(See 'p4 help branch'.) The source is the left side of the branch view
		/// <br/> 	and the target is the right side. With -r, the direction is reversed.
		/// <br/> 
		/// <br/> 	The -s flag can be used with -b to cause fromFile to be treated as
		/// <br/> 	the source, and both sides of the branch view to be treated as the
		/// <br/> 	target, per the branch view mapping.  Optional toFile arguments may
		/// <br/> 	be given to further restrict the scope of the target file set.  The
		/// <br/> 	-r flag is ignored when -s is used.
		/// <br/> 
		/// <br/> 	Note that 'p4 integrate' automatically adusts source-to-target
		/// <br/> 	mappings for moved and renamed files.  (Adjustment occurs only if
		/// <br/> 	the 'p4 move' command was used to move/rename files.) The scope of
		/// <br/> 	source and target file sets must include both the old-named and the
		/// <br/> 	new-named files for mappings to be adjusted.  Moved source files may
		/// <br/> 	cause target files to be scheduled for filename resolves.
		/// <br/> 
		/// <br/> 	The -f flag forces integrate to ignore integration history and treat
		/// <br/> 	all source revisions as unintegrated. It is meant to be used with
		/// <br/> 	revRange to force reintegration of specific, previously integrated
		/// <br/> 	revisions. 
		/// <br/> 
		/// <br/> 	The -i flag enables merging between files that have no prior
		/// <br/> 	integration history.  By default, 'p4 integrate' requires a prior
		/// <br/> 	integration in order to identify a base for merging.  The -i flag
		/// <br/> 	allows the integration, and schedules the target file to be resolved
		/// <br/> 	using the first source revision as the merge base.
		/// <br/> 
		/// <br/> 	The -o flag causes more merge information to be output.  For each
		/// <br/> 	target file scheduled to be resolved, the base file revision and the
		/// <br/> 	source file revision are shown. (After running 'p4 integrate', the
		/// <br/> 	same information is available from 'p4 resolve -o'.)
		/// <br/> 
		/// <br/> 	The -R flags modify the way resolves are scheduled:
		/// <br/> 
		/// <br/> 		-Rb	Schedules 'branch resolves' instead of branching new
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rd	Schedules 'delete resolves' instead of deleting
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rs	Skips cherry-picked revisions already integrated.
		/// <br/> 			This can improve merge results, but can also cause
		/// <br/> 			multiple resolves per file to be scheduled.
		/// <br/> 
		/// <br/> 	The -D flags modify the way deleted files are treated:
		/// <br/> 
		/// <br/> 		-Dt     If the target file has been deleted and the source
		/// <br/> 			file has changed, re-branch the source file on top
		/// <br/> 			of the target file instead of scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Ds     If the source file has been deleted and the target
		/// <br/> 			file has changed, delete the target file instead of
		/// <br/> 			scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Di	If the source file has been deleted and re-added,
		/// <br/> 			probe revisions that precede the deletion to find
		/// <br/> 			unintegrated revisions. By default, 'p4 integrate'
		/// <br/> 			starts probing at the last re-added revision.
		/// <br/> 
		/// <br/> 	The -d flag is a shorthand for all -D flags used together.
		/// <br/> 
		/// <br/> 	The -h flag leaves the target files at the revision currently synced
		/// <br/> 	to the client (the '#have' revision). By default, target files are
		/// <br/> 	automatically synced to the head revision by 'p4 integrate'.
		/// <br/> 
		/// <br/> 	The -t flag propagates source filetypes instead of scheduling
		/// <br/> 	filetype conflicts to be resolved.
		/// <br/> 
		/// <br/> 	The -m flag limits integration to the first 'max' number of files.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of integration, without actually
		/// <br/> 	doing anything.
		/// <br/> 
		/// <br/> 	If -c changelist# is specified, the files are opened in the
		/// <br/> 	designated numbered pending changelist instead of the 'default'
		/// <br/> 	changelist.
		/// <br/> 
		/// <br/> 	The -v flag causes a 'virtual' integration that does not modify
		/// <br/> 	client workspace files unless target files need to be resolved.
		/// <br/> 	After submitting a virtual integration, 'p4 sync' can be used to
		/// <br/> 	update the workspace.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		BidirectionalView = 0x0200,
		/// <summary>
		/// The -t flag propagates the source file's filetype to the target file
		/// (By default, the target file retains its filetype.)  
		/// <br/><b>p4 help integrate</b>
		/// <br/> 
		/// <br/>     integrate -- Integrate one set of files into another
		/// <br/> 
		/// <br/>     p4 integrate [options] fromFile[revRange] toFile
		/// <br/>     p4 integrate [options] -b branch [-r] [toFile[revRange] ...]
		/// <br/>     p4 integrate [options] -b branch -s fromFile[revRange] [toFile ...]
		/// <br/>     p4 integrate [options] -S stream [-r] [-P parent] [file[revRange] ...]
		/// <br/> 
		/// <br/> 	options: -c changelist# -d -f -h -i -o -n -m max -t -v
		/// <br/> 	         -D&lt;flags&gt; -R&lt;flags&gt;
		/// <br/> 
		/// <br/> 	'p4 integrate' integrates one set of files (the 'source') into
		/// <br/> 	another (the 'target'). 
		/// <br/> 
		/// <br/> 	(See also 'p4 merge' and 'p4 copy', variants of 'p4 integrate' that
		/// <br/> 	may be easier and more effective for the task at hand.) 
		/// <br/> 
		/// <br/> 	Using the client workspace as a staging area, 'p4 integrate' adds and
		/// <br/> 	deletes target files per changes in the source, and schedules all
		/// <br/> 	other affected target files to be resolved.  Target files outside of
		/// <br/> 	the current client view are not affected. Source files need not be
		/// <br/> 	within the client view.
		/// <br/> 
		/// <br/> 	'p4 resolve' must be used to merge file content, and to resolve
		/// <br/> 	filename and filetype changes. 'p4 submit' commits integrated files
		/// <br/> 	to the depot.  Unresolved files may not be submitted.  Integrations
		/// <br/> 	can be shelved with 'p4 shelve' and abandoned with 'p4 revert'.  The
		/// <br/> 	commands 'p4 integrated' and 'p4 filelog' display integration history.
		/// <br/> 
		/// <br/> 	When 'p4 integrate' schedules a workspace file to be resolved, it
		/// <br/> 	leaves it read-only. 'p4 resolve' can operate on a read-only file.
		/// <br/> 	For other pre-submit changes, 'p4 edit' must be used to make the
		/// <br/> 	file writable.
		/// <br/> 
		/// <br/> 	Source and target files can be specified either on the 'p4 integrate'
		/// <br/> 	command line or through a branch view. On the command line, fromFile
		/// <br/> 	is the source file set and toFile is the target file set.  With a
		/// <br/> 	branch view, one or more toFile arguments can be given to limit the
		/// <br/> 	scope of the target file set.
		/// <br/> 
		/// <br/> 	revRange is a revision or a revision range that limits the span of
		/// <br/> 	source history to be probed for unintegrated revisions.  revRange
		/// <br/> 	can be used on fromFile, or on toFile, but not on both.  When used on
		/// <br/> 	toFile, it refers to source revisions, not to target revisions.  For
		/// <br/> 	details about revision specifiers, see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -S flag makes 'p4 integrate' use a stream's branch view.  (See
		/// <br/> 	'p4 help stream'.) The source is the stream itself, and the target is
		/// <br/> 	the stream's parent. With -r, the direction is reversed.  -P can be
		/// <br/> 	used to specify a parent stream other than the stream's actual parent.
		/// <br/> 	Note that to submit integrated stream files, the current client must
		/// <br/> 	be dedicated to the target stream. (See 'p4 help client'.)
		/// <br/> 
		/// <br/> 	The -b flag makes 'p4 integrate' use a user-defined branch view.
		/// <br/> 	(See 'p4 help branch'.) The source is the left side of the branch view
		/// <br/> 	and the target is the right side. With -r, the direction is reversed.
		/// <br/> 
		/// <br/> 	The -s flag can be used with -b to cause fromFile to be treated as
		/// <br/> 	the source, and both sides of the branch view to be treated as the
		/// <br/> 	target, per the branch view mapping.  Optional toFile arguments may
		/// <br/> 	be given to further restrict the scope of the target file set.  The
		/// <br/> 	-r flag is ignored when -s is used.
		/// <br/> 
		/// <br/> 	Note that 'p4 integrate' automatically adusts source-to-target
		/// <br/> 	mappings for moved and renamed files.  (Adjustment occurs only if
		/// <br/> 	the 'p4 move' command was used to move/rename files.) The scope of
		/// <br/> 	source and target file sets must include both the old-named and the
		/// <br/> 	new-named files for mappings to be adjusted.  Moved source files may
		/// <br/> 	cause target files to be scheduled for filename resolves.
		/// <br/> 
		/// <br/> 	The -f flag forces integrate to ignore integration history and treat
		/// <br/> 	all source revisions as unintegrated. It is meant to be used with
		/// <br/> 	revRange to force reintegration of specific, previously integrated
		/// <br/> 	revisions. 
		/// <br/> 
		/// <br/> 	The -i flag enables merging between files that have no prior
		/// <br/> 	integration history.  By default, 'p4 integrate' requires a prior
		/// <br/> 	integration in order to identify a base for merging.  The -i flag
		/// <br/> 	allows the integration, and schedules the target file to be resolved
		/// <br/> 	using the first source revision as the merge base.
		/// <br/> 
		/// <br/> 	The -o flag causes more merge information to be output.  For each
		/// <br/> 	target file scheduled to be resolved, the base file revision and the
		/// <br/> 	source file revision are shown. (After running 'p4 integrate', the
		/// <br/> 	same information is available from 'p4 resolve -o'.)
		/// <br/> 
		/// <br/> 	The -R flags modify the way resolves are scheduled:
		/// <br/> 
		/// <br/> 		-Rb	Schedules 'branch resolves' instead of branching new
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rd	Schedules 'delete resolves' instead of deleting
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rs	Skips cherry-picked revisions already integrated.
		/// <br/> 			This can improve merge results, but can also cause
		/// <br/> 			multiple resolves per file to be scheduled.
		/// <br/> 
		/// <br/> 	The -D flags modify the way deleted files are treated:
		/// <br/> 
		/// <br/> 		-Dt     If the target file has been deleted and the source
		/// <br/> 			file has changed, re-branch the source file on top
		/// <br/> 			of the target file instead of scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Ds     If the source file has been deleted and the target
		/// <br/> 			file has changed, delete the target file instead of
		/// <br/> 			scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Di	If the source file has been deleted and re-added,
		/// <br/> 			probe revisions that precede the deletion to find
		/// <br/> 			unintegrated revisions. By default, 'p4 integrate'
		/// <br/> 			starts probing at the last re-added revision.
		/// <br/> 
		/// <br/> 	The -d flag is a shorthand for all -D flags used together.
		/// <br/> 
		/// <br/> 	The -h flag leaves the target files at the revision currently synced
		/// <br/> 	to the client (the '#have' revision). By default, target files are
		/// <br/> 	automatically synced to the head revision by 'p4 integrate'.
		/// <br/> 
		/// <br/> 	The -t flag propagates source filetypes instead of scheduling
		/// <br/> 	filetype conflicts to be resolved.
		/// <br/> 
		/// <br/> 	The -m flag limits integration to the first 'max' number of files.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of integration, without actually
		/// <br/> 	doing anything.
		/// <br/> 
		/// <br/> 	If -c changelist# is specified, the files are opened in the
		/// <br/> 	designated numbered pending changelist instead of the 'default'
		/// <br/> 	changelist.
		/// <br/> 
		/// <br/> 	The -v flag causes a 'virtual' integration that does not modify
		/// <br/> 	client workspace files unless target files need to be resolved.
		/// <br/> 	After submitting a virtual integration, 'p4 sync' can be used to
		/// <br/> 	update the workspace.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		PropogateType = 0x0400,
		/// <summary>
		/// The -v flag speeds integration by not syncing newly-branched files to
		/// the client.  The files can be synced after they are submitted.
		/// </summary>
		DontCopyNewBranchFiles = 0x0800,
		/// <summary>
		/// -Rb     Schedules 'branch resolves' instead of branching new
		///         target files automatically.
		/// </summary>                   
		BranchResolves = 0x1000,
		/// <summary>
		/// -Rd     Schedules 'delete resolves' instead of deleting
		///         target files automatically.                 
		///</summary>
		DeleteResolves = 0x2000,
		/// <summary>
		/// -Rs     Skips cherry-picked revisions already integrated.
		///         This can improve merge results, but can also cause
		///         multiple resolves per file to be scheduled.
		///</summary>
		SkipRevisions = 0x4000
	};

	public partial class Options
	{
		/// <summary>
		/// Options for the integrate command.
		/// </summary>
		/// <param name="fromFile"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help integrate</b>
		/// <br/> 
		/// <br/>     integrate -- Integrate one set of files into another
		/// <br/> 
		/// <br/>     p4 integrate [options] fromFile[revRange] toFile
		/// <br/>     p4 integrate [options] -b branch [-r] [toFile[revRange] ...]
		/// <br/>     p4 integrate [options] -b branch -s fromFile[revRange] [toFile ...]
		/// <br/>     p4 integrate [options] -S stream [-r] [-P parent] [file[revRange] ...]
		/// <br/> 
		/// <br/> 	options: -c changelist# -d -f -h -i -o -n -m max -t -v
		/// <br/> 	         -D&lt;flags&gt; -R&lt;flags&gt;
		/// <br/> 
		/// <br/> 	'p4 integrate' integrates one set of files (the 'source') into
		/// <br/> 	another (the 'target'). 
		/// <br/> 
		/// <br/> 	(See also 'p4 merge' and 'p4 copy', variants of 'p4 integrate' that
		/// <br/> 	may be easier and more effective for the task at hand.) 
		/// <br/> 
		/// <br/> 	Using the client workspace as a staging area, 'p4 integrate' adds and
		/// <br/> 	deletes target files per changes in the source, and schedules all
		/// <br/> 	other affected target files to be resolved.  Target files outside of
		/// <br/> 	the current client view are not affected. Source files need not be
		/// <br/> 	within the client view.
		/// <br/> 
		/// <br/> 	'p4 resolve' must be used to merge file content, and to resolve
		/// <br/> 	filename and filetype changes. 'p4 submit' commits integrated files
		/// <br/> 	to the depot.  Unresolved files may not be submitted.  Integrations
		/// <br/> 	can be shelved with 'p4 shelve' and abandoned with 'p4 revert'.  The
		/// <br/> 	commands 'p4 integrated' and 'p4 filelog' display integration history.
		/// <br/> 
		/// <br/> 	When 'p4 integrate' schedules a workspace file to be resolved, it
		/// <br/> 	leaves it read-only. 'p4 resolve' can operate on a read-only file.
		/// <br/> 	For other pre-submit changes, 'p4 edit' must be used to make the
		/// <br/> 	file writable.
		/// <br/> 
		/// <br/> 	Source and target files can be specified either on the 'p4 integrate'
		/// <br/> 	command line or through a branch view. On the command line, fromFile
		/// <br/> 	is the source file set and toFile is the target file set.  With a
		/// <br/> 	branch view, one or more toFile arguments can be given to limit the
		/// <br/> 	scope of the target file set.
		/// <br/> 
		/// <br/> 	revRange is a revision or a revision range that limits the span of
		/// <br/> 	source history to be probed for unintegrated revisions.  revRange
		/// <br/> 	can be used on fromFile, or on toFile, but not on both.  When used on
		/// <br/> 	toFile, it refers to source revisions, not to target revisions.  For
		/// <br/> 	details about revision specifiers, see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -S flag makes 'p4 integrate' use a stream's branch view.  (See
		/// <br/> 	'p4 help stream'.) The source is the stream itself, and the target is
		/// <br/> 	the stream's parent. With -r, the direction is reversed.  -P can be
		/// <br/> 	used to specify a parent stream other than the stream's actual parent.
		/// <br/> 	Note that to submit integrated stream files, the current client must
		/// <br/> 	be dedicated to the target stream. (See 'p4 help client'.)
		/// <br/> 
		/// <br/> 	The -b flag makes 'p4 integrate' use a user-defined branch view.
		/// <br/> 	(See 'p4 help branch'.) The source is the left side of the branch view
		/// <br/> 	and the target is the right side. With -r, the direction is reversed.
		/// <br/> 
		/// <br/> 	The -s flag can be used with -b to cause fromFile to be treated as
		/// <br/> 	the source, and both sides of the branch view to be treated as the
		/// <br/> 	target, per the branch view mapping.  Optional toFile arguments may
		/// <br/> 	be given to further restrict the scope of the target file set.  The
		/// <br/> 	-r flag is ignored when -s is used.
		/// <br/> 
		/// <br/> 	Note that 'p4 integrate' automatically adusts source-to-target
		/// <br/> 	mappings for moved and renamed files.  (Adjustment occurs only if
		/// <br/> 	the 'p4 move' command was used to move/rename files.) The scope of
		/// <br/> 	source and target file sets must include both the old-named and the
		/// <br/> 	new-named files for mappings to be adjusted.  Moved source files may
		/// <br/> 	cause target files to be scheduled for filename resolves.
		/// <br/> 
		/// <br/> 	The -f flag forces integrate to ignore integration history and treat
		/// <br/> 	all source revisions as unintegrated. It is meant to be used with
		/// <br/> 	revRange to force reintegration of specific, previously integrated
		/// <br/> 	revisions. 
		/// <br/> 
		/// <br/> 	The -i flag enables merging between files that have no prior
		/// <br/> 	integration history.  By default, 'p4 integrate' requires a prior
		/// <br/> 	integration in order to identify a base for merging.  The -i flag
		/// <br/> 	allows the integration, and schedules the target file to be resolved
		/// <br/> 	using the first source revision as the merge base.
		/// <br/> 
		/// <br/> 	The -o flag causes more merge information to be output.  For each
		/// <br/> 	target file scheduled to be resolved, the base file revision and the
		/// <br/> 	source file revision are shown. (After running 'p4 integrate', the
		/// <br/> 	same information is available from 'p4 resolve -o'.)
		/// <br/> 
		/// <br/> 	The -R flags modify the way resolves are scheduled:
		/// <br/> 
		/// <br/> 		-Rb	Schedules 'branch resolves' instead of branching new
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rd	Schedules 'delete resolves' instead of deleting
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rs	Skips cherry-picked revisions already integrated.
		/// <br/> 			This can improve merge results, but can also cause
		/// <br/> 			multiple resolves per file to be scheduled.
		/// <br/> 
		/// <br/> 	The -D flags modify the way deleted files are treated:
		/// <br/> 
		/// <br/> 		-Dt     If the target file has been deleted and the source
		/// <br/> 			file has changed, re-branch the source file on top
		/// <br/> 			of the target file instead of scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Ds     If the source file has been deleted and the target
		/// <br/> 			file has changed, delete the target file instead of
		/// <br/> 			scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Di	If the source file has been deleted and re-added,
		/// <br/> 			probe revisions that precede the deletion to find
		/// <br/> 			unintegrated revisions. By default, 'p4 integrate'
		/// <br/> 			starts probing at the last re-added revision.
		/// <br/> 
		/// <br/> 	The -d flag is a shorthand for all -D flags used together.
		/// <br/> 
		/// <br/> 	The -h flag leaves the target files at the revision currently synced
		/// <br/> 	to the client (the '#have' revision). By default, target files are
		/// <br/> 	automatically synced to the head revision by 'p4 integrate'.
		/// <br/> 
		/// <br/> 	The -t flag propagates source filetypes instead of scheduling
		/// <br/> 	filetype conflicts to be resolved.
		/// <br/> 
		/// <br/> 	The -m flag limits integration to the first 'max' number of files.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of integration, without actually
		/// <br/> 	doing anything.
		/// <br/> 
		/// <br/> 	If -c changelist# is specified, the files are opened in the
		/// <br/> 	designated numbered pending changelist instead of the 'default'
		/// <br/> 	changelist.
		/// <br/> 
		/// <br/> 	The -v flag causes a 'virtual' integration that does not modify
		/// <br/> 	client workspace files unless target files need to be resolved.
		/// <br/> 	After submitting a virtual integration, 'p4 sync' can be used to
		/// <br/> 	update the workspace.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(IntegrateFilesCmdFlags flags,
										int changeList,
										int maxFiles,
										String branch,
										string stream,
										string parent)
		{
			if (changeList >= 0)
			{
				this["-c"] = changeList.ToString();
			}

			if (((flags & IntegrateFilesCmdFlags.BranchIfTargetDeleted) != 0) &&
				((flags & IntegrateFilesCmdFlags.DeleteIfSourceDeleted) != 0) &&
				((flags & IntegrateFilesCmdFlags.IntegrateAllIfSourceDeleted) != 0))
			{
				this["-d"] = null;
			}
			if ((flags & IntegrateFilesCmdFlags.Force) != 0)
			{
				this["-f"] = null;
			}
			if ((flags & IntegrateFilesCmdFlags.LeaveHaveVersion) != 0)
			{
				this["-h"] = null;
			}
			if ((flags & IntegrateFilesCmdFlags.IntegrateUnrelated) != 0)
			{
				this["-i"] = null;
			}
			if ((flags & IntegrateFilesCmdFlags.DisplayBaseFile) != 0)
			{
				this["-o"] = null;
			}
			if ((flags & IntegrateFilesCmdFlags.PreviewIntegrationsOnly) != 0)
			{
				this["-n"] = null;
			}

			if (maxFiles > 0)
			{
				this["-m"] = maxFiles.ToString();
			}

			if ((flags & IntegrateFilesCmdFlags.PropogateType) != 0)
			{
				this["-t"] = null;
			}
			if ((flags & IntegrateFilesCmdFlags.DontCopyNewBranchFiles) != 0)
			{
				this["-v"] = null;
			}

			if (this.ContainsKey("-d") == false)
			{
				if ((flags & IntegrateFilesCmdFlags.BranchIfTargetDeleted) != 0)
				{
					this["-Dt"] = null;
				}
				if ((flags & IntegrateFilesCmdFlags.DeleteIfSourceDeleted) != 0)
				{
					this["-Ds"] = null;
				}
				if ((flags & IntegrateFilesCmdFlags.IntegrateAllIfSourceDeleted) != 0)
				{
					this["-Di"] = null;
				}
			}

			if ((flags & IntegrateFilesCmdFlags.BranchResolves) != 0)
			{
				this["-Rb"] = null;
			}
			if ((flags & IntegrateFilesCmdFlags.DeleteResolves) != 0)
			{
				this["-Rd"] = null;
			}
			if ((flags & IntegrateFilesCmdFlags.SkipRevisions) != 0)
			{
				this["-Rs"] = null;
			}

			if (String.IsNullOrEmpty(branch) == false)
			{
				this["-b"] = branch;
			}
			if (String.IsNullOrEmpty(stream) == false)
			{
				this["-S"] = stream;
			}

			if ((flags & IntegrateFilesCmdFlags.SwapSourceAndTarget) != 0)
			{
				this["-r"] = null;
			}
			if ((flags & IntegrateFilesCmdFlags.BidirectionalView) != 0)
			{
				this["-s"] = null;
			}
		}
	}
	/// <summary>
	/// Integrate command options
	/// </summary>
	public class IntegrateFilesCmdOptions : Options
	{
		/// <summary>
		/// Options for the integrate command.
		/// 
		/// </summary>
		/// <param name="fromFile"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help integrate</b>
		/// <br/> 
		/// <br/>     integrate -- Integrate one set of files into another
		/// <br/> 
		/// <br/>     p4 integrate [options] fromFile[revRange] toFile
		/// <br/>     p4 integrate [options] -b branch [-r] [toFile[revRange] ...]
		/// <br/>     p4 integrate [options] -b branch -s fromFile[revRange] [toFile ...]
		/// <br/>     p4 integrate [options] -S stream [-r] [-P parent] [file[revRange] ...]
		/// <br/> 
		/// <br/> 	options: -c changelist# -d -f -h -i -o -n -m max -t -v
		/// <br/> 	         -D&lt;flags&gt; -R&lt;flags&gt;
		/// <br/> 
		/// <br/> 	'p4 integrate' integrates one set of files (the 'source') into
		/// <br/> 	another (the 'target'). 
		/// <br/> 
		/// <br/> 	(See also 'p4 merge' and 'p4 copy', variants of 'p4 integrate' that
		/// <br/> 	may be easier and more effective for the task at hand.) 
		/// <br/> 
		/// <br/> 	Using the client workspace as a staging area, 'p4 integrate' adds and
		/// <br/> 	deletes target files per changes in the source, and schedules all
		/// <br/> 	other affected target files to be resolved.  Target files outside of
		/// <br/> 	the current client view are not affected. Source files need not be
		/// <br/> 	within the client view.
		/// <br/> 
		/// <br/> 	'p4 resolve' must be used to merge file content, and to resolve
		/// <br/> 	filename and filetype changes. 'p4 submit' commits integrated files
		/// <br/> 	to the depot.  Unresolved files may not be submitted.  Integrations
		/// <br/> 	can be shelved with 'p4 shelve' and abandoned with 'p4 revert'.  The
		/// <br/> 	commands 'p4 integrated' and 'p4 filelog' display integration history.
		/// <br/> 
		/// <br/> 	When 'p4 integrate' schedules a workspace file to be resolved, it
		/// <br/> 	leaves it read-only. 'p4 resolve' can operate on a read-only file.
		/// <br/> 	For other pre-submit changes, 'p4 edit' must be used to make the
		/// <br/> 	file writable.
		/// <br/> 
		/// <br/> 	Source and target files can be specified either on the 'p4 integrate'
		/// <br/> 	command line or through a branch view. On the command line, fromFile
		/// <br/> 	is the source file set and toFile is the target file set.  With a
		/// <br/> 	branch view, one or more toFile arguments can be given to limit the
		/// <br/> 	scope of the target file set.
		/// <br/> 
		/// <br/> 	revRange is a revision or a revision range that limits the span of
		/// <br/> 	source history to be probed for unintegrated revisions.  revRange
		/// <br/> 	can be used on fromFile, or on toFile, but not on both.  When used on
		/// <br/> 	toFile, it refers to source revisions, not to target revisions.  For
		/// <br/> 	details about revision specifiers, see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -S flag makes 'p4 integrate' use a stream's branch view.  (See
		/// <br/> 	'p4 help stream'.) The source is the stream itself, and the target is
		/// <br/> 	the stream's parent. With -r, the direction is reversed.  -P can be
		/// <br/> 	used to specify a parent stream other than the stream's actual parent.
		/// <br/> 	Note that to submit integrated stream files, the current client must
		/// <br/> 	be dedicated to the target stream. (See 'p4 help client'.)
		/// <br/> 
		/// <br/> 	The -b flag makes 'p4 integrate' use a user-defined branch view.
		/// <br/> 	(See 'p4 help branch'.) The source is the left side of the branch view
		/// <br/> 	and the target is the right side. With -r, the direction is reversed.
		/// <br/> 
		/// <br/> 	The -s flag can be used with -b to cause fromFile to be treated as
		/// <br/> 	the source, and both sides of the branch view to be treated as the
		/// <br/> 	target, per the branch view mapping.  Optional toFile arguments may
		/// <br/> 	be given to further restrict the scope of the target file set.  The
		/// <br/> 	-r flag is ignored when -s is used.
		/// <br/> 
		/// <br/> 	Note that 'p4 integrate' automatically adusts source-to-target
		/// <br/> 	mappings for moved and renamed files.  (Adjustment occurs only if
		/// <br/> 	the 'p4 move' command was used to move/rename files.) The scope of
		/// <br/> 	source and target file sets must include both the old-named and the
		/// <br/> 	new-named files for mappings to be adjusted.  Moved source files may
		/// <br/> 	cause target files to be scheduled for filename resolves.
		/// <br/> 
		/// <br/> 	The -f flag forces integrate to ignore integration history and treat
		/// <br/> 	all source revisions as unintegrated. It is meant to be used with
		/// <br/> 	revRange to force reintegration of specific, previously integrated
		/// <br/> 	revisions. 
		/// <br/> 
		/// <br/> 	The -i flag enables merging between files that have no prior
		/// <br/> 	integration history.  By default, 'p4 integrate' requires a prior
		/// <br/> 	integration in order to identify a base for merging.  The -i flag
		/// <br/> 	allows the integration, and schedules the target file to be resolved
		/// <br/> 	using the first source revision as the merge base.
		/// <br/> 
		/// <br/> 	The -o flag causes more merge information to be output.  For each
		/// <br/> 	target file scheduled to be resolved, the base file revision and the
		/// <br/> 	source file revision are shown. (After running 'p4 integrate', the
		/// <br/> 	same information is available from 'p4 resolve -o'.)
		/// <br/> 
		/// <br/> 	The -R flags modify the way resolves are scheduled:
		/// <br/> 
		/// <br/> 		-Rb	Schedules 'branch resolves' instead of branching new
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rd	Schedules 'delete resolves' instead of deleting
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rs	Skips cherry-picked revisions already integrated.
		/// <br/> 			This can improve merge results, but can also cause
		/// <br/> 			multiple resolves per file to be scheduled.
		/// <br/> 
		/// <br/> 	The -D flags modify the way deleted files are treated:
		/// <br/> 
		/// <br/> 		-Dt     If the target file has been deleted and the source
		/// <br/> 			file has changed, re-branch the source file on top
		/// <br/> 			of the target file instead of scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Ds     If the source file has been deleted and the target
		/// <br/> 			file has changed, delete the target file instead of
		/// <br/> 			scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Di	If the source file has been deleted and re-added,
		/// <br/> 			probe revisions that precede the deletion to find
		/// <br/> 			unintegrated revisions. By default, 'p4 integrate'
		/// <br/> 			starts probing at the last re-added revision.
		/// <br/> 
		/// <br/> 	The -d flag is a shorthand for all -D flags used together.
		/// <br/> 
		/// <br/> 	The -h flag leaves the target files at the revision currently synced
		/// <br/> 	to the client (the '#have' revision). By default, target files are
		/// <br/> 	automatically synced to the head revision by 'p4 integrate'.
		/// <br/> 
		/// <br/> 	The -t flag propagates source filetypes instead of scheduling
		/// <br/> 	filetype conflicts to be resolved.
		/// <br/> 
		/// <br/> 	The -m flag limits integration to the first 'max' number of files.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of integration, without actually
		/// <br/> 	doing anything.
		/// <br/> 
		/// <br/> 	If -c changelist# is specified, the files are opened in the
		/// <br/> 	designated numbered pending changelist instead of the 'default'
		/// <br/> 	changelist.
		/// <br/> 
		/// <br/> 	The -v flag causes a 'virtual' integration that does not modify
		/// <br/> 	client workspace files unless target files need to be resolved.
		/// <br/> 	After submitting a virtual integration, 'p4 sync' can be used to
		/// <br/> 	update the workspace.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public IntegrateFilesCmdOptions(IntegrateFilesCmdFlags flags,
										int changeList,
										int maxFiles,
										String branch,
										string stream,
										string parent)
			: base(flags, changeList, maxFiles, branch, stream, parent) { }
	}

	/// <summary>
	/// Flags for the label sync command.
	/// </summary>
	[Flags]
	public enum LabelSyncCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// The -a flag adds the specified file to the label.
		/// </summary>
		AddFile = 0x0001,
		/// <summary>
		/// The -d deletes the specified file from the label, regardless of
		///     revision.
		/// </summary>
		DeleteFile = 0x0002,
		/// <summary>
		/// The -n flag previews the operation without altering the label.
		/// </summary>
		Preview = 0x0004,
		/// <summary>
		/// The -q flag suppresses normal output messages. Messages regarding
		///     errors or exceptional conditions are displayed.
		/// </summary>
		Quiet = 0x0008
	};
	public partial class Options
	{
		/// <summary>
		/// Options for the labelsync command.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="labelName"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help labelsync</b>
		/// <br/> 
		/// <br/>     labelsync -- Apply the label to the contents of the client workspace
		/// <br/> 
		/// <br/>     p4 labelsync [-a -d -n -q] -l label [file[revRange] ...]
		/// <br/> 
		/// <br/> 	Labelsync causes the specified label to reflect the current contents
		/// <br/> 	of the client.  It records the revision of each file currently synced.
		/// <br/> 	The label's name can subsequently be used in a revision specification
		/// <br/> 	as @label to refer to the revision of a file as stored in the label.
		/// <br/> 
		/// <br/> 	Without a file argument, labelsync causes the label to reflect the
		/// <br/> 	contents of the whole client, by adding, deleting, and updating the
		/// <br/> 	label.  If a file is specified, labelsync updates the specified file.
		/// <br/> 
		/// <br/> 	If the file argument includes a revision specification, that revision
		/// <br/> 	is used instead of the revision synced by the client. If the specified
		/// <br/> 	revision is a deleted revision, the label includes that deleted
		/// <br/> 	revision.  See 'p4 help revisions' for details about specifying
		/// <br/> 	revisions.
		/// <br/> 
		/// <br/> 	If the file argument includes a revision range specification,
		/// <br/> 	only files selected by the revision range are updated, and the
		/// <br/> 	highest revision in the range is used.
		/// <br/> 
		/// <br/> 	The -a flag adds the specified file to the label.
		/// <br/> 
		/// <br/> 	The -d deletes the specified file from the label, regardless of
		/// <br/> 	revision.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without altering the label.
		/// <br/> 
		/// <br/> 	Only the owner of a label can run labelsync on that label. A label
		/// <br/> 	that has its Options: field set to 'locked' cannot be updated.
		/// <br/> 
		/// <br/> 	The -q flag suppresses normal output messages. Messages regarding
		/// <br/> 	errors or exceptional conditions are displayed.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(LabelSyncCmdFlags flags)
		{
			Options value = new Options();

			if ((flags & LabelSyncCmdFlags.AddFile) != 0)
			{
				this["-a"] = null;
			}
			if ((flags & LabelSyncCmdFlags.DeleteFile) != 0)
			{
				this["-d"] = null;
			}
			if ((flags & LabelSyncCmdFlags.Preview) != 0)
			{
				this["-p"] = null;
			}
			if ((flags & LabelSyncCmdFlags.Quiet) != 0)
			{
				this["-q"] = null;
			}
		}
	}
	/// <summary>
	/// Labelsync command options
	/// </summary>
	public class LabelSyncCmdOptions : Options
	{
		/// <summary>
		/// Options for the labelsync command.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="labelName"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help labelsync</b>
		/// <br/> 
		/// <br/>     labelsync -- Apply the label to the contents of the client workspace
		/// <br/> 
		/// <br/>     p4 labelsync [-a -d -n -q] -l label [file[revRange] ...]
		/// <br/> 
		/// <br/> 	Labelsync causes the specified label to reflect the current contents
		/// <br/> 	of the client.  It records the revision of each file currently synced.
		/// <br/> 	The label's name can subsequently be used in a revision specification
		/// <br/> 	as @label to refer to the revision of a file as stored in the label.
		/// <br/> 
		/// <br/> 	Without a file argument, labelsync causes the label to reflect the
		/// <br/> 	contents of the whole client, by adding, deleting, and updating the
		/// <br/> 	label.  If a file is specified, labelsync updates the specified file.
		/// <br/> 
		/// <br/> 	If the file argument includes a revision specification, that revision
		/// <br/> 	is used instead of the revision synced by the client. If the specified
		/// <br/> 	revision is a deleted revision, the label includes that deleted
		/// <br/> 	revision.  See 'p4 help revisions' for details about specifying
		/// <br/> 	revisions.
		/// <br/> 
		/// <br/> 	If the file argument includes a revision range specification,
		/// <br/> 	only files selected by the revision range are updated, and the
		/// <br/> 	highest revision in the range is used.
		/// <br/> 
		/// <br/> 	The -a flag adds the specified file to the label.
		/// <br/> 
		/// <br/> 	The -d deletes the specified file from the label, regardless of
		/// <br/> 	revision.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without altering the label.
		/// <br/> 
		/// <br/> 	Only the owner of a label can run labelsync on that label. A label
		/// <br/> 	that has its Options: field set to 'locked' cannot be updated.
		/// <br/> 
		/// <br/> 	The -q flag suppresses normal output messages. Messages regarding
		/// <br/> 	errors or exceptional conditions are displayed.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public LabelSyncCmdOptions(LabelSyncCmdFlags flags)
			: base(flags) { }
	}

	public partial class Options
	{
		/// <summary>
		/// Options for the lock command.
		/// </summary>
		/// <param name="changeList"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help lock</b>
		/// <br/> 
		/// <br/>     lock -- Lock an open file to prevent it from being submitted
		/// <br/> 
		/// <br/>     p4 lock [-c changelist#] [file ...]
		/// <br/> 
		/// <br/> 	The specified files are locked in the depot, preventing any user
		/// <br/> 	other than the current user on the current client from submitting
		/// <br/> 	changes to the files.  If a file is already locked, the lock request
		/// <br/> 	is rejected.  If no file names are specified, all files in the
		/// <br/> 	specified changelist are locked. If changelist number is omitted,
		/// <br/> 	files in the default changelist are locked.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(int changeList)
		{
			if (changeList >= 0)
			{
				this["-c"] = changeList.ToString();
			}
		}
	}
	///<summary>
	/// Lock command options
	///</summary>
	public class LockCmdOptions : Options
	{
		/// <summary>
		/// Options for the lock command.
		/// </summary>
		/// <param name="changeList"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help lock</b>
		/// <br/> 
		/// <br/>     lock -- Lock an open file to prevent it from being submitted
		/// <br/> 
		/// <br/>     p4 lock [-c changelist#] [file ...]
		/// <br/> 
		/// <br/> 	The specified files are locked in the depot, preventing any user
		/// <br/> 	other than the current user on the current client from submitting
		/// <br/> 	changes to the files.  If a file is already locked, the lock request
		/// <br/> 	is rejected.  If no file names are specified, all files in the
		/// <br/> 	specified changelist are locked. If changelist number is omitted,
		/// <br/> 	files in the default changelist are locked.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public LockCmdOptions(int changeList)
			: base(changeList) { }
	}

	/// <summary>
	/// Flags for the move command.
	/// </summary>
	[Flags]
	public enum MoveFileCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// The -f flag forces a move to an existing target file. The file
		/// must be synced and not opened.  The originating source file will
		/// no longer be synced to the client.
		/// </summary>
		Force = 0x0001,
		/// <summary>
		/// The -n flag previews the operation without moving files.
		/// </summary>
		Preview = 0x0002,
		/// <summary>
		/// The -k flag performs the rename on the server without modifying
		/// client files. Use with caution, as an incorrect move can cause
		/// discrepancies between the state of the client and the corresponding
		/// server metadata.
		/// </summary>
		ServerOnly = 0x0004,
	};
	public partial class Options
	{
		/// <summary>
		/// Options for the move command.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changeList"></param>
		/// <param name="fileType"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help move</b>
		/// <br/> 
		/// <br/>     move -- move file(s) from one location to another
		/// <br/>     rename -- synonym for 'move'
		/// <br/> 
		/// <br/>     p4 move [-c changelist#] [-f -n -k] [-t filetype] fromFile toFile
		/// <br/> 
		/// <br/> 	Move takes an already opened file and moves it from one client
		/// <br/> 	location to another, reopening it as a pending depot move.  When
		/// <br/> 	the file is submitted with 'p4 submit', its depot file is moved
		/// <br/> 	accordingly.
		/// <br/> 
		/// <br/> 	Wildcards in fromFile and toFile must match. The fromFile must be
		/// <br/> 	a file open for add or edit.
		/// <br/> 
		/// <br/> 	'p4 opened' lists pending moves. 'p4 diff' can compare a moved
		/// <br/> 	client file with its depot original, 'p4 sync' can schedule an 
		/// <br/> 	update of a moved file, and 'p4 resolve' can resolve the update.
		/// <br/> 
		/// <br/> 	A client file can be moved many times before it is submitted.
		/// <br/> 	Moving a file back to its original location will undo a pending
		/// <br/> 	move, leaving unsubmitted content intact.  Using 'p4 revert'
		/// <br/> 	undoes the move and reverts the unsubmitted content.
		/// <br/> 
		/// <br/> 	If -c changelist# is specified, the file is reopened in the
		/// <br/> 	specified pending changelist as well as being moved.
		/// <br/> 
		/// <br/> 	The -f flag forces a move to an existing target file. The file
		/// <br/> 	must be synced and not opened.  The originating source file will
		/// <br/> 	no longer be synced to the client.
		/// <br/> 
		/// <br/> 	If -t filetype is specified, the file is assigned that filetype.
		/// <br/> 	If the filetype is a partial filetype, the partial filetype is
		/// <br/> 	combined with the current filetype.  See 'p4 help filetypes'.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without moving files.
		/// <br/> 
		/// <br/> 	The -k flag performs the rename on the server without modifying
		/// <br/> 	client files. Use with caution, as an incorrect move can cause
		/// <br/> 	discrepancies between the state of the client and the corresponding
		/// <br/> 	server metadata.
		/// <br/> 
		/// <br/> 	The 'move' command requires a release 2009.1 or newer client. The
		/// <br/> 	'-f' flag requires a 2010.1 client.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(MoveFileCmdFlags flags, int changeList, FileType fileType)
		{
			if (changeList >= 0)
			{
				this["-c"] = changeList.ToString();
			}

			if ((flags & MoveFileCmdFlags.Force) != 0)
			{
				this["-f"] = null;
			}
			if ((flags & MoveFileCmdFlags.Preview) != 0)
			{
				this["-n"] = null;
			}
			if ((flags & MoveFileCmdFlags.ServerOnly) != 0)
			{
				this["-k"] = null;
			}
			if (fileType != null)
			{
				this["-t"] = fileType.ToString();
			}
		}
	}
	///<summary>
	/// Move command options
	///</summary>
	public class MoveCmdOptions : Options
	{
		/// <summary>
		/// Options for the move command.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changeList"></param>
		/// <param name="fileType"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help move</b>
		/// <br/> 
		/// <br/>     move -- move file(s) from one location to another
		/// <br/>     rename -- synonym for 'move'
		/// <br/> 
		/// <br/>     p4 move [-c changelist#] [-f -n -k] [-t filetype] fromFile toFile
		/// <br/> 
		/// <br/> 	Move takes an already opened file and moves it from one client
		/// <br/> 	location to another, reopening it as a pending depot move.  When
		/// <br/> 	the file is submitted with 'p4 submit', its depot file is moved
		/// <br/> 	accordingly.
		/// <br/> 
		/// <br/> 	Wildcards in fromFile and toFile must match. The fromFile must be
		/// <br/> 	a file open for add or edit.
		/// <br/> 
		/// <br/> 	'p4 opened' lists pending moves. 'p4 diff' can compare a moved
		/// <br/> 	client file with its depot original, 'p4 sync' can schedule an 
		/// <br/> 	update of a moved file, and 'p4 resolve' can resolve the update.
		/// <br/> 
		/// <br/> 	A client file can be moved many times before it is submitted.
		/// <br/> 	Moving a file back to its original location will undo a pending
		/// <br/> 	move, leaving unsubmitted content intact.  Using 'p4 revert'
		/// <br/> 	undoes the move and reverts the unsubmitted content.
		/// <br/> 
		/// <br/> 	If -c changelist# is specified, the file is reopened in the
		/// <br/> 	specified pending changelist as well as being moved.
		/// <br/> 
		/// <br/> 	The -f flag forces a move to an existing target file. The file
		/// <br/> 	must be synced and not opened.  The originating source file will
		/// <br/> 	no longer be synced to the client.
		/// <br/> 
		/// <br/> 	If -t filetype is specified, the file is assigned that filetype.
		/// <br/> 	If the filetype is a partial filetype, the partial filetype is
		/// <br/> 	combined with the current filetype.  See 'p4 help filetypes'.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without moving files.
		/// <br/> 
		/// <br/> 	The -k flag performs the rename on the server without modifying
		/// <br/> 	client files. Use with caution, as an incorrect move can cause
		/// <br/> 	discrepancies between the state of the client and the corresponding
		/// <br/> 	server metadata.
		/// <br/> 
		/// <br/> 	The 'move' command requires a release 2009.1 or newer client. The
		/// <br/> 	'-f' flag requires a 2010.1 client.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public MoveCmdOptions(MoveFileCmdFlags flags, int changeList, FileType fileType)
			: base(flags, changeList, fileType) { }
	}
	public partial class Options
	{
		/// <summary>
		/// Options for the reopen command.
		/// </summary>
		/// <param name="changeList"></param>
		/// <param name="fileType"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help reopen</b>
		/// <br/> 
		/// <br/>     reopen -- Change the filetype of an open file or move it to
		/// <br/>               another changelist
		/// <br/> 
		/// <br/>     p4 reopen [-c changelist#] [-t filetype] file ...
		/// <br/> 
		/// <br/> 	Reopen an open file for the current user in order to move it to a
		/// <br/> 	different changelist or change its filetype.
		/// <br/> 
		/// <br/> 	The target changelist must exist; you cannot create a changelist by
		/// <br/> 	reopening a file. To move a file to the default changelist, use
		/// <br/> 	'p4 reopen -c default'.
		/// <br/> 
		/// <br/> 	If -t filetype is specified, the file is assigned that filetype. If
		/// <br/> 	a partial filetype is specified, it is combined with the current
		/// <br/> 	filetype.  For details, see 'p4 help filetypes'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(int changeList, FileType fileType)
		{
			if (changeList > 0)
			{
				this["-c"] = changeList.ToString();
			}
			else if (changeList == 0)
			{
				this["-c"] = "default";
			}

			if (fileType != null)
			{
				this["-t"] = fileType.ToString();
			}
		}
	}

	/// <summary>
	/// Options for the reopen command.
	/// </summary>
	public class ReopenCmdOptions : Options
	{
		/// <summary>
		/// Options for the reopen command.
		/// </summary>
		/// <param name="changeList"></param>
		/// <param name="fileType"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help reopen</b>
		/// <br/> 
		/// <br/>     reopen -- Change the filetype of an open file or move it to
		/// <br/>               another changelist
		/// <br/> 
		/// <br/>     p4 reopen [-c changelist#] [-t filetype] file ...
		/// <br/> 
		/// <br/> 	Reopen an open file for the current user in order to move it to a
		/// <br/> 	different changelist or change its filetype.
		/// <br/> 
		/// <br/> 	The target changelist must exist; you cannot create a changelist by
		/// <br/> 	reopening a file. To move a file to the default changelist, use
		/// <br/> 	'p4 reopen -c default'.
		/// <br/> 
		/// <br/> 	If -t filetype is specified, the file is assigned that filetype. If
		/// <br/> 	a partial filetype is specified, it is combined with the current
		/// <br/> 	filetype.  For details, see 'p4 help filetypes'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public ReopenCmdOptions(int changeList, FileType fileType)
			: base(changeList, fileType) { }
	}

	/// <summary>
	/// Flags for the resolve command.
	/// </summary>
	[Flags]
	public enum ResolveFilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		///    The -A flag can be used to limit the kind of resolving that will be
		///    attempted; without it, everything is attempted:
		///
		///	       -Aa         Resolve attributes.
		/// </summary>
		FileAttributesOnly = 0x0001,
		/// <summary>
		///    The -A flag can be used to limit the kind of resolving that will be
		///    attempted; without it, everything is attempted:
		///        -Ab         Resolve file branching.
		/// </summary>
		FileBranchingOnly = 0x0002,
		/// <summary>
		///    The -A flag can be used to limit the kind of resolving that will be
		///    attempted; without it, everything is attempted:
		///        -Ac         Resolve file content changes.
		/// </summary>
		FileContentChangesOnly = 0x0004,
		/// <summary>
		///    The -A flag can be used to limit the kind of resolving that will be
		///    attempted; without it, everything is attempted:
		///        -Ad         Resolve file deletions.
		/// </summary>
		FileDeletionsOnly = 0x0008,
		/// <summary>
		///    The -A flag can be used to limit the kind of resolving that will be
		///    attempted; without it, everything is attempted:
		///        -Am         Resolve moved and renamed files.
		/// </summary>
		FileMovesOnly = 0x0010,
		/// <summary>
		///    The -A flag can be used to limit the kind of resolving that will be
		///    attempted; without it, everything is attempted:
		///        -At	       Resolve filetype changes.
		/// </summary>
		FileTypeChangesOnly = 0x0020,

		AFlags = FileAttributesOnly | FileBranchingOnly | FileContentChangesOnly | FileDeletionsOnly | FileMovesOnly | FileTypeChangesOnly,

		/// <summary>
		///    The -a flag puts 'p4 resolve' into automatic mode. The user is not
		///    prompted, and files that can't be resolved automatically are skipped:
		///        -as         'Safe' resolve; skip files that need merging.
		///    The -as flag causes the workspace file to be replaced with their file
		///    only if theirs has changed and yours has not.
		/// </summary>
		AutomaticSafeMode = 0x0100,
		/// <summary>
		///    The -a flag puts 'p4 resolve' into automatic mode. The user is not
		///    prompted, and files that can't be resolved automatically are skipped:
		///        -am         Resolve by merging; skip files with conflicts.
		///    The -am flag causes the workspace file to be replaced with the result
		///    of merging theirs with yours. If the merge detected conflicts, the
		///    file is left untouched and unresolved.
		/// </summary>
		AutomaticMergeMode = 0x0200,
		/// <summary>
		///    The -a flag puts 'p4 resolve' into automatic mode. The user is not
		///    prompted, and files that can't be resolved automatically are skipped:
		///        -af         Force acceptance of merged files with conflicts.
		///    The -af flag causes the workspace file to be replaced with the result
		///    of merging theirs with yours, even if there were conflicts.  This can
		///    leave conflict markers in workspace files.
		/// </summary>
		AutomaticForceMergeMode = 0x0400,
		/// <summary>
		///    The -a flag puts 'p4 resolve' into automatic mode. The user is not
		///    prompted, and files that can't be resolved automatically are skipped:
		///        -at         Force acceptance of theirs; overwrites yours.
		///    The -at flag resolves all files by copying theirs into yours. It
		///    should be used with care, as it overwrites any changes made to the
		///    file in the client workspace.
		/// </summary>
		AutomaticTheirsMode = 0x0800,
		/// <summary>
		///    The -a flag puts 'p4 resolve' into automatic mode. The user is not
		///    prompted, and files that can't be resolved automatically are skipped:
		///        -ay         Force acceptance of yours; ignores theirs.
		///    The -ay flag resolves all files by accepting yours and ignoring
		///    theirs. It preserves the content of workspace files.
		/// </summary>
		AutomaticYoursMode = 0x2000,

		aFlags = AutomaticSafeMode | AutomaticMergeMode | AutomaticForceMergeMode | AutomaticTheirsMode | AutomaticYoursMode,

		/// <summary>
		///     The -f flag enables previously resolved files to be resolved again.
		///     By default, after files have been resolved, 'p4 resolve' does not
		///     process them again.
		/// </summary>
		ForceResolve = 0x04000,
		/// <summary>
		///     The -n flag previews the operation without altering files.
		/// </summary>
		PreviewOnly = 0x08000,
		/// <summary>
		///     The -N flag previews the operation with additional information about
		///     any non-content resolve actions that are scheduled.
		/// </summary>
		PreviewPlusOnly = 0x10000,
		/// <summary>
		///     The -o flag displays the base file name and revision to be used
		///     during the the merge.
		/// </summary>
		DisplayBaseFile = 0x20000,
		/// <summary>
		///     The -t flag forces 'p4 resolve' to attempt a textual merge, even for
		///     files with non-text (binary) types.
		/// </summary>
		ForceTextualMerge = 0x40000,
		/// <summary>
		///     The -v flag causes 'p4 resolve' to insert markers for all changes,
		///     not just conflicts.
		/// </summary>
		MarkAllChanges = 0x80000,
		/// <summary>
		/// The -d flags can be used to control handling of whitespace and line
		/// endings when merging files:
		///   -db Ignore Whitespace Changes
		/// </summary>
		IgnoreWhitespaceChanges = 0x100000,
		/// <summary>
		/// The -d flags can be used to control handling of whitespace and line
		/// endings when merging files:
		///   -dw Ingore whitespace altogether.
		/// </summary>
		IgnoreWhitespace = 0x200000,
		/// <summary>
		/// The -d flags can be used to control handling of whitespace and line
		/// endings when merging files:
		///   -dl Ignore Line Endings
		/// </summary>
		IgnoreLineEndings = 0x400000,

		WsFlags = IgnoreWhitespaceChanges | IgnoreWhitespace | IgnoreLineEndings
	}
	/// <summary>
	///  Options for the resolve command
	/// </summary>
	public partial class Options
	{
		/// <summary>
		///  Options for the resolve command
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changeList"></param>
		/// <remarks>
		/// <br/><b>p4 help resolve</b>
		/// <br/> 
		/// <br/>     resolve -- Resolve integrations and updates to workspace files
		/// <br/> 
		/// <br/>     p4 resolve [options] [file ...]
		/// <br/> 
		/// <br/> 	options: -A&lt;flags&gt; -a&lt;flags&gt; -d&lt;flags&gt; -f -n -N -o -t -v
		/// <br/> 		 -c changelist#
		/// <br/> 
		/// <br/> 	'p4 resolve' resolves changes to files in the client workspace.
		/// <br/> 	
		/// <br/> 	'p4 resolve' works only on files that have been scheduled to be 
		/// <br/> 	resolved.  The commands that can schedule resolves are: 'p4 sync',
		/// <br/> 	'p4 update', 'p4 submit', 'p4 merge', and 'p4 integrate'.  Files must
		/// <br/> 	be resolved before they can be submitted.
		/// <br/> 
		/// <br/> 	Resolving involves two sets of files, a source and a target.  The
		/// <br/> 	target is a set of depot files that maps to opened files in the
		/// <br/> 	client workspace.  When resolving an integration, the source is a
		/// <br/> 	different set of depot files than the target.  When resolving an
		/// <br/> 	update, the source is the same set of depot files as the target,
		/// <br/> 	at a different revision.
		/// <br/> 
		/// <br/> 	The 'p4 resolve' file argument specifies the target.  If the file
		/// <br/> 	argument is omitted, all unresolved files are resolved.
		/// <br/> 
		/// <br/> 	Resolving can modify workspace files. To back up files, use 'p4
		/// <br/> 	shelve' before using 'p4 resolve'.
		/// <br/> 
		/// <br/> 	The resolve process is a classic three-way merge. The participating
		/// <br/> 	files are referred to as follows:
		/// <br/> 
		/// <br/> 	  'yours'       The target file open in the client workspace
		/// <br/> 	  'theirs'      The source file in the depot
		/// <br/> 	  'base'        The common ancestor; the highest revision of the
		/// <br/> 	                source file already accounted for in the target.
		/// <br/> 	  'merged'      The merged result.
		/// <br/> 
		/// <br/> 	Filenames, filetypes, and text file content can be resolved by 
		/// <br/> 	accepting 'yours', 'theirs', or 'merged'.  Branching, deletion, and
		/// <br/> 	binary file content can be resolved by accepting either 'yours' or
		/// <br/> 	'theirs'.
		/// <br/> 
		/// <br/> 	When resolving integrated changes, 'p4 resolve' distinguishes among
		/// <br/> 	four results: entirely yours, entirely theirs, a pure merge, or an
		/// <br/> 	edited merge.  The distinction is recorded when resolved files are
		/// <br/> 	submitted, and will be used by future commands to determine whether
		/// <br/> 	integration is needed.
		/// <br/> 
		/// <br/> 	In all cases, accepting 'yours' leaves the target file in its current
		/// <br/> 	state.  The result of accepting 'theirs' is as follows:
		/// <br/> 
		/// <br/> 	   Content:     The target file content is overwritten.
		/// <br/>  	   Branching:	A new target is branched.
		/// <br/>  	   Deletion:    The target file is deleted.
		/// <br/>  	   Filename:	The target file is moved or renamed.
		/// <br/>  	   Filetype:    The target file's type is changed.
		/// <br/> 
		/// <br/> 	For each unresolved change, the user is prompted to accept a result.
		/// <br/> 	Content and non-content changes are resolved separately.  For content,
		/// <br/> 	'p4 resolve' places the merged result into a temporary file in the
		/// <br/> 	client workspace.  If there are any conflicts, the merged file contains
		/// <br/> 	conflict markers that must be removed by the user.
		/// <br/> 
		/// <br/> 	'p4 resolve' displays a count of text diffs and conflicts, and offers
		/// <br/> 	the following prompts:
		/// <br/> 
		/// <br/> 	  Accept:
		/// <br/> 	     at              Keep only changes to their file.
		/// <br/> 	     ay              Keep only changes to your file.
		/// <br/> 	   * am              Keep merged file.
		/// <br/> 	   * ae              Keep merged and edited file.
		/// <br/> 	   * a               Keep autoselected file.
		/// <br/> 
		/// <br/> 	  Diff:
		/// <br/> 	   * dt              See their changes alone.
		/// <br/> 	   * dy              See your changes alone.
		/// <br/> 	   * dm              See merged changes.
		/// <br/> 	     d               Diff your file against merged file.
		/// <br/> 
		/// <br/> 	  Edit:
		/// <br/> 	     et              Edit their file (read only).
		/// <br/> 	     ey              Edit your file (read/write).
		/// <br/> 	   * e               Edit merged file (read/write).
		/// <br/> 
		/// <br/> 	  Misc:
		/// <br/> 	   * m               Run '$P4MERGE base theirs yours merged'.
		/// <br/> 			     (Runs '$P4MERGEUNICODE charset base theirs
		/// <br/> 			      yours merged' if set and the file is a
		/// <br/> 			      unicode file.)
		/// <br/> 	     s               Skip this file.
		/// <br/> 	     h               Print this help message.
		/// <br/> 	     ^C              Quit the resolve operation.
		/// <br/> 
		/// <br/> 	Options marked (*) appear only for text files. The suggested action
		/// <br/> 	will be displayed in brackets. 
		/// <br/> 
		/// <br/> 	The 'merge' (m) option enables you to invoke your own merge program, if
		/// <br/> 	one is configured using the $P4MERGE environment variable.  Four files
		/// <br/> 	are passed to the program: the base, yours, theirs, and the temporary
		/// <br/> 	file. The program is expected to write merge results to the temporary
		/// <br/> 	file.
		/// <br/> 
		/// <br/> 	The -A flag can be used to limit the kind of resolving that will be
		/// <br/> 	attempted; without it, everything is attempted:
		/// <br/> 
		/// <br/> 	    -Ab		Resolve file branching.
		/// <br/> 	    -Ac		Resolve file content changes.
		/// <br/> 	    -Ad		Resolve file deletions.
		/// <br/> 	    -Am		Resolve moved and renamed files.
		/// <br/> 	    -At		Resolve filetype changes.
		/// <br/> 
		/// <br/> 	The -a flag puts 'p4 resolve' into automatic mode. The user is not
		/// <br/> 	prompted, and files that can't be resolved automatically are skipped:
		/// <br/> 
		/// <br/> 	    -as		'Safe' resolve; skip files that need merging.
		/// <br/> 	    -am 	Resolve by merging; skip files with conflicts.
		/// <br/> 	    -af		Force acceptance of merged files with conflicts.
		/// <br/> 	    -at		Force acceptance of theirs; overwrites yours.
		/// <br/> 	    -ay		Force acceptance of yours; ignores theirs.
		/// <br/> 
		/// <br/> 	The -as flag causes the workspace file to be replaced with their file
		/// <br/> 	only if theirs has changed and yours has not.
		/// <br/> 
		/// <br/> 	The -am flag causes the workspace file to be replaced with the result
		/// <br/> 	of merging theirs with yours. If the merge detected conflicts, the
		/// <br/> 	file is left untouched and uresolved.
		/// <br/> 
		/// <br/> 	The -af flag causes the workspace file to be replaced with the result
		/// <br/> 	of merging theirs with yours, even if there were conflicts.  This can
		/// <br/> 	leave conflict markers in workspace files.
		/// <br/> 
		/// <br/> 	The -at flag resolves all files by copying theirs into yours. It 
		/// <br/> 	should be used with care, as it overwrites any changes made to the
		/// <br/> 	file in the client workspace.
		/// <br/> 
		/// <br/> 	The -ay flag resolves all files by accepting yours and ignoring 
		/// <br/> 	theirs. It preserves the content of workspace files.
		/// <br/> 
		/// <br/> 	The -d flags can be used to control handling of whitespace and line
		/// <br/> 	endings when merging files:
		/// <br/> 
		/// <br/> 	    -db		Ingore whitespace changes.
		/// <br/> 	    -dw		Ingore whitespace altogether.
		/// <br/> 	    -dl 	Ignores line endings. 
		/// <br/> 
		/// <br/> 	The -d flags are also passed to the diff options in the 'p4 resolve'
		/// <br/> 	dialog. Additional -d flags that modify the diff output but do not 
		/// <br/> 	modify merge behavior include -dn (RCS), -dc (context), -ds (summary),
		/// <br/> 	and -du (unified). Note that 'p4 resolve' uses text from the client
		/// <br/> 	file if the files differ only in whitespace.
		/// <br/> 
		/// <br/> 	The -f flag enables previously resolved content to be resolved again.
		/// <br/> 	By default, after files have been resolved, 'p4 resolve' does not
		/// <br/> 	process them again.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without altering files.
		/// <br/> 
		/// <br/> 	The -N flag previews the operation with additional information about
		/// <br/> 	any non-content resolve actions that are scheduled.
		/// <br/> 
		/// <br/> 	The -o flag displays the base file name and revision to be used
		/// <br/> 	during the the merge.
		/// <br/> 
		/// <br/> 	The -t flag forces 'p4 resolve' to attempt a textual merge, even for
		/// <br/> 	files with non-text (binary) types.
		/// <br/> 
		/// <br/> 	The -v flag causes 'p4 resolve' to insert markers for all changes,
		/// <br/> 	not just conflicts.
		/// <br/> 
		/// <br/> 	The -c flag limits 'p4 resolve' to the files in changelist#.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(ResolveFilesCmdFlags flags, int changeList)
		{
			if ((flags & ResolveFilesCmdFlags.AFlags) != 0)
			{
				// these can be combined
				string flag = "-A";
                if ((flags & ResolveFilesCmdFlags.FileAttributesOnly) != 0)
                    flag += "a";
				if ((flags & ResolveFilesCmdFlags.FileBranchingOnly) != 0)
					flag += "b";
				if ((flags & ResolveFilesCmdFlags.FileContentChangesOnly) != 0)
					flag += "c";
				if ((flags & ResolveFilesCmdFlags.FileDeletionsOnly) != 0)
					flag += "d";
				if ((flags & ResolveFilesCmdFlags.FileMovesOnly) != 0)
					flag += "m";
				if ((flags & ResolveFilesCmdFlags.FileTypeChangesOnly) != 0)
					flag += "t";

				this[flag] = null;
			}
			if ((flags & ResolveFilesCmdFlags.aFlags) != 0)
			{
				// these are mutually exclusive
				string flag = "-a";
				if ((flags & ResolveFilesCmdFlags.AutomaticSafeMode) != 0)
					flag = "-as";
				else if ((flags & ResolveFilesCmdFlags.AutomaticMergeMode) != 0)
					flag = "-am";
				else if ((flags & ResolveFilesCmdFlags.AutomaticForceMergeMode) != 0)
					flag = "-af";
				else if ((flags & ResolveFilesCmdFlags.AutomaticTheirsMode) != 0)
					flag = "-at";
				else if ((flags & ResolveFilesCmdFlags.AutomaticYoursMode) != 0)
					flag = "-ay";

				this[flag] = null;
			}
			if ((flags & ResolveFilesCmdFlags.WsFlags) != 0)
			{
				// these are mutually exclusive
				string flag = "-d";
				if ((flags & ResolveFilesCmdFlags.IgnoreWhitespaceChanges) != 0)
					flag = "-db";
				else if ((flags & ResolveFilesCmdFlags.IgnoreWhitespace) != 0)
					flag = "-dw";
				else if ((flags & ResolveFilesCmdFlags.IgnoreLineEndings) != 0)
					flag = "-dl";

				this[flag] = null;
			}

			if ((flags & ResolveFilesCmdFlags.ForceResolve) != 0)
			{
				this["-f"] = null;
			}
			if ((flags & ResolveFilesCmdFlags.PreviewOnly) != 0)
			{
				this["-n"] = null;
			}
			if ((flags & ResolveFilesCmdFlags.PreviewPlusOnly) != 0)
			{
				this["-N"] = null;
			}
			if ((flags & ResolveFilesCmdFlags.DisplayBaseFile) != 0)
			{
				this["-o"] = null;
			}
			if ((flags & ResolveFilesCmdFlags.ForceTextualMerge) != 0)
			{
				this["-t"] = null;
			}
			if ((flags & ResolveFilesCmdFlags.MarkAllChanges) != 0)
			{
				this["-v"] = null;
			}

			if (changeList >= 0)
			{
				this["-c"] = changeList.ToString();
			}
		}
	}

	/// <summary>
	/// Options for the resolve command.
	/// </summary>
	public class ResolveCmdOptions : Options
	{
		/// <summary>
		/// Options for the resolve command.
		/// </summary>
		/// <param name="changeList"></param>
		/// <param name="fileType"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help resolve</b>
		/// <br/> 
		/// <br/>     resolve -- Resolve integrations and updates to workspace files
		/// <br/> 
		/// <br/>     p4 resolve [options] [file ...]
		/// <br/> 
		/// <br/> 	options: -A&lt;flags&gt; -a&lt;flags&gt; -d&lt;flags&gt; -f -n -N -o -t -v
		/// <br/> 		 -c changelist#
		/// <br/> 
		/// <br/> 	'p4 resolve' resolves changes to files in the client workspace.
		/// <br/> 	
		/// <br/> 	'p4 resolve' works only on files that have been scheduled to be 
		/// <br/> 	resolved.  The commands that can schedule resolves are: 'p4 sync',
		/// <br/> 	'p4 update', 'p4 submit', 'p4 merge', and 'p4 integrate'.  Files must
		/// <br/> 	be resolved before they can be submitted.
		/// <br/> 
		/// <br/> 	Resolving involves two sets of files, a source and a target.  The
		/// <br/> 	target is a set of depot files that maps to opened files in the
		/// <br/> 	client workspace.  When resolving an integration, the source is a
		/// <br/> 	different set of depot files than the target.  When resolving an
		/// <br/> 	update, the source is the same set of depot files as the target,
		/// <br/> 	at a different revision.
		/// <br/> 
		/// <br/> 	The 'p4 resolve' file argument specifies the target.  If the file
		/// <br/> 	argument is omitted, all unresolved files are resolved.
		/// <br/> 
		/// <br/> 	Resolving can modify workspace files. To back up files, use 'p4
		/// <br/> 	shelve' before using 'p4 resolve'.
		/// <br/> 
		/// <br/> 	The resolve process is a classic three-way merge. The participating
		/// <br/> 	files are referred to as follows:
		/// <br/> 
		/// <br/> 	  'yours'       The target file open in the client workspace
		/// <br/> 	  'theirs'      The source file in the depot
		/// <br/> 	  'base'        The common ancestor; the highest revision of the
		/// <br/> 	                source file already accounted for in the target.
		/// <br/> 	  'merged'      The merged result.
		/// <br/> 
		/// <br/> 	Filenames, filetypes, and text file content can be resolved by 
		/// <br/> 	accepting 'yours', 'theirs', or 'merged'.  Branching, deletion, and
		/// <br/> 	binary file content can be resolved by accepting either 'yours' or
		/// <br/> 	'theirs'.
		/// <br/> 
		/// <br/> 	When resolving integrated changes, 'p4 resolve' distinguishes among
		/// <br/> 	four results: entirely yours, entirely theirs, a pure merge, or an
		/// <br/> 	edited merge.  The distinction is recorded when resolved files are
		/// <br/> 	submitted, and will be used by future commands to determine whether
		/// <br/> 	integration is needed.
		/// <br/> 
		/// <br/> 	In all cases, accepting 'yours' leaves the target file in its current
		/// <br/> 	state.  The result of accepting 'theirs' is as follows:
		/// <br/> 
		/// <br/> 	   Content:     The target file content is overwritten.
		/// <br/>  	   Branching:	A new target is branched.
		/// <br/>  	   Deletion:    The target file is deleted.
		/// <br/>  	   Filename:	The target file is moved or renamed.
		/// <br/>  	   Filetype:    The target file's type is changed.
		/// <br/> 
		/// <br/> 	For each unresolved change, the user is prompted to accept a result.
		/// <br/> 	Content and non-content changes are resolved separately.  For content,
		/// <br/> 	'p4 resolve' places the merged result into a temporary file in the
		/// <br/> 	client workspace.  If there are any conflicts, the merged file contains
		/// <br/> 	conflict markers that must be removed by the user.
		/// <br/> 
		/// <br/> 	'p4 resolve' displays a count of text diffs and conflicts, and offers
		/// <br/> 	the following prompts:
		/// <br/> 
		/// <br/> 	  Accept:
		/// <br/> 	     at              Keep only changes to their file.
		/// <br/> 	     ay              Keep only changes to your file.
		/// <br/> 	   * am              Keep merged file.
		/// <br/> 	   * ae              Keep merged and edited file.
		/// <br/> 	   * a               Keep autoselected file.
		/// <br/> 
		/// <br/> 	  Diff:
		/// <br/> 	   * dt              See their changes alone.
		/// <br/> 	   * dy              See your changes alone.
		/// <br/> 	   * dm              See merged changes.
		/// <br/> 	     d               Diff your file against merged file.
		/// <br/> 
		/// <br/> 	  Edit:
		/// <br/> 	     et              Edit their file (read only).
		/// <br/> 	     ey              Edit your file (read/write).
		/// <br/> 	   * e               Edit merged file (read/write).
		/// <br/> 
		/// <br/> 	  Misc:
		/// <br/> 	   * m               Run '$P4MERGE base theirs yours merged'.
		/// <br/> 			     (Runs '$P4MERGEUNICODE charset base theirs
		/// <br/> 			      yours merged' if set and the file is a
		/// <br/> 			      unicode file.)
		/// <br/> 	     s               Skip this file.
		/// <br/> 	     h               Print this help message.
		/// <br/> 	     ^C              Quit the resolve operation.
		/// <br/> 
		/// <br/> 	Options marked (*) appear only for text files. The suggested action
		/// <br/> 	will be displayed in brackets. 
		/// <br/> 
		/// <br/> 	The 'merge' (m) option enables you to invoke your own merge program, if
		/// <br/> 	one is configured using the $P4MERGE environment variable.  Four files
		/// <br/> 	are passed to the program: the base, yours, theirs, and the temporary
		/// <br/> 	file. The program is expected to write merge results to the temporary
		/// <br/> 	file.
		/// <br/> 
		/// <br/> 	The -A flag can be used to limit the kind of resolving that will be
		/// <br/> 	attempted; without it, everything is attempted:
		/// <br/> 
		/// <br/> 	    -Ab		Resolve file branching.
		/// <br/> 	    -Ac		Resolve file content changes.
		/// <br/> 	    -Ad		Resolve file deletions.
		/// <br/> 	    -Am		Resolve moved and renamed files.
		/// <br/> 	    -At		Resolve filetype changes.
		/// <br/> 
		/// <br/> 	The -a flag puts 'p4 resolve' into automatic mode. The user is not
		/// <br/> 	prompted, and files that can't be resolved automatically are skipped:
		/// <br/> 
		/// <br/> 	    -as		'Safe' resolve; skip files that need merging.
		/// <br/> 	    -am 	Resolve by merging; skip files with conflicts.
		/// <br/> 	    -af		Force acceptance of merged files with conflicts.
		/// <br/> 	    -at		Force acceptance of theirs; overwrites yours.
		/// <br/> 	    -ay		Force acceptance of yours; ignores theirs.
		/// <br/> 
		/// <br/> 	The -as flag causes the workspace file to be replaced with their file
		/// <br/> 	only if theirs has changed and yours has not.
		/// <br/> 
		/// <br/> 	The -am flag causes the workspace file to be replaced with the result
		/// <br/> 	of merging theirs with yours. If the merge detected conflicts, the
		/// <br/> 	file is left untouched and uresolved.
		/// <br/> 
		/// <br/> 	The -af flag causes the workspace file to be replaced with the result
		/// <br/> 	of merging theirs with yours, even if there were conflicts.  This can
		/// <br/> 	leave conflict markers in workspace files.
		/// <br/> 
		/// <br/> 	The -at flag resolves all files by copying theirs into yours. It 
		/// <br/> 	should be used with care, as it overwrites any changes made to the
		/// <br/> 	file in the client workspace.
		/// <br/> 
		/// <br/> 	The -ay flag resolves all files by accepting yours and ignoring 
		/// <br/> 	theirs. It preserves the content of workspace files.
		/// <br/> 
		/// <br/> 	The -d flags can be used to control handling of whitespace and line
		/// <br/> 	endings when merging files:
		/// <br/> 
		/// <br/> 	    -db		Ingore whitespace changes.
		/// <br/> 	    -dw		Ingore whitespace altogether.
		/// <br/> 	    -dl 	Ignores line endings. 
		/// <br/> 
		/// <br/> 	The -d flags are also passed to the diff options in the 'p4 resolve'
		/// <br/> 	dialog. Additional -d flags that modify the diff output but do not 
		/// <br/> 	modify merge behavior include -dn (RCS), -dc (context), -ds (summary),
		/// <br/> 	and -du (unified). Note that 'p4 resolve' uses text from the client
		/// <br/> 	file if the files differ only in whitespace.
		/// <br/> 
		/// <br/> 	The -f flag enables previously resolved content to be resolved again.
		/// <br/> 	By default, after files have been resolved, 'p4 resolve' does not
		/// <br/> 	process them again.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without altering files.
		/// <br/> 
		/// <br/> 	The -N flag previews the operation with additional information about
		/// <br/> 	any non-content resolve actions that are scheduled.
		/// <br/> 
		/// <br/> 	The -o flag displays the base file name and revision to be used
		/// <br/> 	during the the merge.
		/// <br/> 
		/// <br/> 	The -t flag forces 'p4 resolve' to attempt a textual merge, even for
		/// <br/> 	files with non-text (binary) types.
		/// <br/> 
		/// <br/> 	The -v flag causes 'p4 resolve' to insert markers for all changes,
		/// <br/> 	not just conflicts.
		/// <br/> 
		/// <br/> 	The -c flag limits 'p4 resolve' to the files in changelist#.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public ResolveCmdOptions(ResolveFilesCmdFlags flags, int changeList)
			: base(flags, changeList) { }
	}

	/// <summary>
	/// Diff whitespace options flags.
	/// </summary>
	[Flags]
	public enum DiffWhiteSpaceOptions
	{
		/// <summary>
		/// None
		/// </summary>
		none = 0x0000,
		/// <summary>
		/// -db Ignore Whitespace Changes
		/// </summary>
		IgnoreWhitespaceChanges = 0x0001,
		/// <summary>
		/// -dw Ingore whitespace altogether.
		/// </summary>
		IgnoreWhitespace = 0x0002,
		/// <summary>
		/// -dl Ignore Line Endings
		/// </summary>
		IgnoreLineEndings = 0x0004,
		/// <summary>
		/// -dn RCS
		/// </summary>
		RCS = 0x0008,
		/// <summary>
		/// -dc[n] Show context of changes
		/// </summary>
		ShowContext = 0x0010,
		/// <summary>
		/// -ds Summary
		/// </summary>
		ShowSummary = 0x0020,
		/// <summary>
		/// -du[n] Unified
		/// </summary>
		ShowUnified = 0x0040
	};

	/// <summary>
	/// Flags for the submit command.
	/// </summary>
	[Flags]
	public enum SubmitFilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -r flag reopens submitted files in the default changelist after
		/// 	submission.
		/// </summary>
		ReopenFiles = 0x0001,
		/// <summary>
		/// 	The -s flag extends the list of jobs to include the fix status
		/// 	for each job, which becomes the job's status when the changelist
		/// 	is committed.  See 'p4 help change' for details.
		/// 	submission.
		/// </summary>
		IncludeJobs = 0x0002,
        /// <summary>
		/// 	The -e flag submits a shelved changelist without transferring files
		/// 	or modifying the workspace. The shelved change must be owned by
		/// 	the person submitting the change, but the client may be different.
		/// 	However, files shelved to a stream target may only be submitted by
		/// 	a stream client that is mapped to the target stream. In addition,
		/// 	files shelved to a non-stream target cannot be submitted by a
		/// 	stream client. Submitting shelved changes by a task stream
		/// 	client is not supported. To submit a shelved change, all
		/// 	files in the shelved change must be up to date and resolved. No
		/// 	files may be open in any workspace at the same change number.
		/// 	Client submit options (ie revertUnchanged, etc) will be ignored.
		/// 	If the submit is successful, the shelved change and files
		/// 	are cleaned up, and are no longer available to be unshelved or
        /// 	submitted.
		/// </summary>
		SubmitShelved = 0x0004
	}

	public partial class Options
	{
		/// <summary>
		/// Submit command options
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changelist"></param>
		/// <param name="newChangelist"></param>
		/// <param name="description"></param>
		/// <param name="submitOptions"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help submit</b>
		/// <br/> 
		/// <br/>     submit -- Submit open files to the depot
		/// <br/> 
		/// <br/>     p4 submit [-r -s -f option]
		/// <br/>     p4 submit [-r -s -f option] file
		/// <br/>     p4 submit [-r -f option] -d description
		/// <br/>     p4 submit [-r -f option] -d description file
		/// <br/>     p4 submit [-r -f option] -c changelist#
		/// <br/>     p4 submit -i [-r -s -f option]
		/// <br/> 
		/// <br/> 	'p4 submit' commits a pending changelist and its files to the depot.
		/// <br/> 
		/// <br/> 	By default, 'p4 submit' attempts to submit all files in the 'default'
		/// <br/> 	changelist.  Submit displays a dialog where you enter a description
		/// <br/> 	of the change and, optionally, delete files from the list of files
		/// <br/> 	to be checked in. 
		/// <br/> 
		/// <br/> 	To add files to a changelist before submitting, use any of the 
		/// <br/> 	commands that open client workspace files: 'p4 add', 'p4 edit',
		/// <br/> 	etc.
		/// <br/> 
		/// <br/> 	If the file parameter is specified, only files in the default
		/// <br/> 	changelist that match the pattern are submitted.
		/// <br/> 
		/// <br/> 	Files in a stream path can be submitted only by client workspaces
		/// <br/> 	dedicated to the stream. See 'p4 help client'.
		/// <br/> 
		/// <br/> 	Before committing a changelist, 'p4 submit' locks all the files being
		/// <br/> 	submitted. If any file cannot be locked or submitted, the files are 
		/// <br/> 	left open in a numbered pending changelist. 'p4 opened' shows
		/// <br/> 	unsubmitted files and their changelists.
		/// <br/> 
		/// <br/> 	Submit is atomic: if the operation succeeds, all files are updated
		/// <br/> 	in the depot. If the submit fails, no depot files are updated.
		/// <br/> 
		/// <br/> 	The -c flag submits the specified pending changelist instead of the
		/// <br/> 	default changelist. Additional changelists can be created manually, 
		/// <br/> 	using the 'p4 change' command, or automatically as the result of a 
		/// <br/> 	failed attempt to submit the default changelist.
		/// <br/> 
		/// <br/> 	The -d flag passes a description into the specified changelist rather
		/// <br/> 	than displaying the changelist dialog for manual editing. This option
		/// <br/> 	is useful for scripting, but does not allow you to add jobs or modify
		/// <br/> 	the default changelist.
		/// <br/> 
		/// <br/> 	The -f flag enables you to override submit options that are configured
		/// <br/> 	for the client that is submitting the changelist.  This flag overrides
		/// <br/> 	the -r (reopen)flag, if it is specified.  See 'p4 help client' for
		/// <br/> 	details about submit options.
		/// <br/> 
		/// <br/> 	The -i flag reads a changelist specification from the standard input.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -r flag reopens submitted files in the default changelist after
		/// <br/> 	submission.
		/// <br/> 
		/// <br/> 	The -s flag extends the list of jobs to include the fix status
		/// <br/> 	for each job, which becomes the job's status when the changelist
		/// <br/> 	is committed.  See 'p4 help change' for details.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(SubmitFilesCmdFlags flags, int changelist, Changelist newChangelist,
			string description, ClientSubmitOptions submitOptions)
		{
			if (newChangelist != null)
				this["-i"] = newChangelist.ToString();

			if ((flags & SubmitFilesCmdFlags.ReopenFiles) != 0)
			{
				this["-r"] = null;
			}
			if ((flags & SubmitFilesCmdFlags.IncludeJobs) != 0)
			{
				this["-s"] = null;
			}
			if (submitOptions != null)
			{
				this["-f"] = submitOptions.ToString();
			}
			if (String.IsNullOrEmpty(description) == false)
			{
				this["-d"] = description;
			}
			if (changelist > -1)
			{
				this["-c"] = changelist.ToString();
			}
            if ((flags&SubmitFilesCmdFlags.SubmitShelved)!=0)
            {
                this.Clear();
                // -e cannot be used with any other flags
                this["-e"] = changelist.ToString();
            }
		}
	}

	/// <summary>
	/// Submit command options
	/// </summary>
	public class SubmitCmdOptions : Options
	{
		/// <summary>
		/// Submit command options
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changelist"></param>
		/// <param name="newChangelist"></param>
		/// <param name="description"></param>
		/// <param name="submitOptions"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help submit</b>
		/// <br/> 
		/// <br/>     submit -- Submit open files to the depot
		/// <br/> 
		/// <br/>     p4 submit [-r -s -f option]
		/// <br/>     p4 submit [-r -s -f option] file
		/// <br/>     p4 submit [-r -f option] -d description
		/// <br/>     p4 submit [-r -f option] -d description file
		/// <br/>     p4 submit [-r -f option] -c changelist#
		/// <br/>     p4 submit -i [-r -s -f option]
		/// <br/> 
		/// <br/> 	'p4 submit' commits a pending changelist and its files to the depot.
		/// <br/> 
		/// <br/> 	By default, 'p4 submit' attempts to submit all files in the 'default'
		/// <br/> 	changelist.  Submit displays a dialog where you enter a description
		/// <br/> 	of the change and, optionally, delete files from the list of files
		/// <br/> 	to be checked in. 
		/// <br/> 
		/// <br/> 	To add files to a changelist before submitting, use any of the 
		/// <br/> 	commands that open client workspace files: 'p4 add', 'p4 edit',
		/// <br/> 	etc.
		/// <br/> 
		/// <br/> 	If the file parameter is specified, only files in the default
		/// <br/> 	changelist that match the pattern are submitted.
		/// <br/> 
		/// <br/> 	Files in a stream path can be submitted only by client workspaces
		/// <br/> 	dedicated to the stream. See 'p4 help client'.
		/// <br/> 
		/// <br/> 	Before committing a changelist, 'p4 submit' locks all the files being
		/// <br/> 	submitted. If any file cannot be locked or submitted, the files are 
		/// <br/> 	left open in a numbered pending changelist. 'p4 opened' shows
		/// <br/> 	unsubmitted files and their changelists.
		/// <br/> 
		/// <br/> 	Submit is atomic: if the operation succeeds, all files are updated
		/// <br/> 	in the depot. If the submit fails, no depot files are updated.
		/// <br/> 
		/// <br/> 	The -c flag submits the specified pending changelist instead of the
		/// <br/> 	default changelist. Additional changelists can be created manually, 
		/// <br/> 	using the 'p4 change' command, or automatically as the result of a 
		/// <br/> 	failed attempt to submit the default changelist.
		/// <br/> 
		/// <br/> 	The -d flag passes a description into the specified changelist rather
		/// <br/> 	than displaying the changelist dialog for manual editing. This option
		/// <br/> 	is useful for scripting, but does not allow you to add jobs or modify
		/// <br/> 	the default changelist.
		/// <br/> 
		/// <br/> 	The -f flag enables you to override submit options that are configured
		/// <br/> 	for the client that is submitting the changelist.  This flag overrides
		/// <br/> 	the -r (reopen)flag, if it is specified.  See 'p4 help client' for
		/// <br/> 	details about submit options.
		/// <br/> 
		/// <br/> 	The -i flag reads a changelist specification from the standard input.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -r flag reopens submitted files in the default changelist after
		/// <br/> 	submission.
		/// <br/> 
		/// <br/> 	The -s flag extends the list of jobs to include the fix status
		/// <br/> 	for each job, which becomes the job's status when the changelist
		/// <br/> 	is committed.  See 'p4 help change' for details.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public SubmitCmdOptions(SubmitFilesCmdFlags flags, int changelist, Changelist newChangelist,
			string description, ClientSubmitOptions submitOptions)
			: base(flags, changelist, newChangelist, description, submitOptions) { }
	}
	/// <summary>
	/// Flags for the resolved command.
	/// </summary>
	[Flags]
	public enum GetResolvedFilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -r flag reopens submitted files in the default changelist after
		/// 	submission.
		/// </summary>
		IncludeBaseRevision = 0x0001,
	}

	public partial class Options
	{
		/// <summary>
		/// Resolved command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help resolved</b>
		/// <br/> 
		/// <br/>     resolved -- Show files that have been resolved but not submitted
		/// <br/> 
		/// <br/>     p4 resolved [-o] [file ...]
		/// <br/> 
		/// <br/> 	'p4 resolved' lists file updates and integrations that have been 
		/// <br/> 	resolved but not yet submitted.  To see unresolved integrations, 
		/// <br/> 	use 'p4 resolve -n'.  To see already submitted integrations, use 
		/// <br/> 	'p4 integrated'.
		/// <br/> 
		/// <br/> 	If a depot file path is specified, the output lists resolves for
		/// <br/> 	'theirs' files that match the specified path.  If a client file
		/// <br/> 	path is specified, the output lists resolves for 'yours' files
		/// <br/> 	that match the specified path.
		/// <br/> 
		/// <br/> 	The -o flag reports the revision used as the base during the
		/// <br/> 	resolve.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(GetResolvedFilesCmdFlags flags)
		{
			if ((flags & GetResolvedFilesCmdFlags.IncludeBaseRevision) != 0)
			{
				this["-o"] = null;
			}
		}
	}

	/// <summary>
	/// Options for Resolve command
	/// </summary>
	public class ResolvedCmdOptions : Options
	{
		/// <summary>
		/// Resolved command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help resolved</b>
		/// <br/> 
		/// <br/>     resolved -- Show files that have been resolved but not submitted
		/// <br/> 
		/// <br/>     p4 resolved [-o] [file ...]
		/// <br/> 
		/// <br/> 	'p4 resolved' lists file updates and integrations that have been 
		/// <br/> 	resolved but not yet submitted.  To see unresolved integrations, 
		/// <br/> 	use 'p4 resolve -n'.  To see already submitted integrations, use 
		/// <br/> 	'p4 integrated'.
		/// <br/> 
		/// <br/> 	If a depot file path is specified, the output lists resolves for
		/// <br/> 	'theirs' files that match the specified path.  If a client file
		/// <br/> 	path is specified, the output lists resolves for 'yours' files
		/// <br/> 	that match the specified path.
		/// <br/> 
		/// <br/> 	The -o flag reports the revision used as the base during the
		/// <br/> 	resolve.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public ResolvedCmdOptions(GetResolvedFilesCmdFlags flags)
			:base(flags)
		{}
	}
	/// <summary>
	/// Flags for the revert command.
	/// </summary>
	[Flags]
	public enum RevertFilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -a flag  reverts only files that are open for edit or integrate
		/// 	and are unchanged or missing. Files with pending integration records
		/// 	are left open. The file arguments are optional when -a is specified.
		/// </summary>
		UnchangedOnly = 0x0001,
		/// <summary>
		/// 	The -n flag displays a preview of the operation.
		/// </summary>
		Preview = 0x0002,
		/// <summary>
		/// 	The -k flag marks the file as reverted in server metadata without
		/// 	altering files in the client workspace.
		/// </summary>
		ServerOnly = 0x0004,
	}

	public partial class Options
	{
		/// <summary>
		/// Revert command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changelist"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help revert</b>
		/// <br/> 
		/// <br/>     revert -- Discard changes from an opened file
		/// <br/> 
		/// <br/>     p4 revert [-a -n -k -c changelist#] file ...
		/// <br/> 
		/// <br/> 	Revert an open file to the revision that was synced from the depot,
		/// <br/> 	discarding any edits or integrations that have been made.  You must
		/// <br/> 	explicitly specify the files to be reverted.  Files are removed from
		/// <br/> 	the changelist in which they are open.  Locked files are unlocked.
		/// <br/> 
		/// <br/> 	The -a flag  reverts only files that are open for edit or integrate
		/// <br/> 	and are unchanged or missing. Files with pending integration records
		/// <br/> 	are left open. The file arguments are optional when -a is specified.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of the operation.
		/// <br/> 
		/// <br/> 	The -k flag marks the file as reverted in server metadata without
		/// <br/> 	altering files in the client workspace.
		/// <br/> 
		/// <br/> 	The -c flag reverts files that are open in the specified changelist.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(RevertFilesCmdFlags flags, int changelist)
		{
			if ((flags & RevertFilesCmdFlags.UnchangedOnly) != 0)
			{
				this["-a"] = null;
			}
			if ((flags & RevertFilesCmdFlags.Preview) != 0)
			{
				this["-n"] = null;
			}
			if ((flags & RevertFilesCmdFlags.ServerOnly) != 0)
			{
				this["-k"] = null;
			}

			if (changelist > 0)
			{
				this["-c"] = changelist.ToString();
			}

            if (changelist == 0)
            {
                this["-c"] = "default";
            }
		}
	}
	public class RevertCmdOptions : Options
	{
		/// <summary>
		/// Revert command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changelist"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help revert</b>
		/// <br/> 
		/// <br/>     revert -- Discard changes from an opened file
		/// <br/> 
		/// <br/>     p4 revert [-a -n -k -c changelist#] file ...
		/// <br/> 
		/// <br/> 	Revert an open file to the revision that was synced from the depot,
		/// <br/> 	discarding any edits or integrations that have been made.  You must
		/// <br/> 	explicitly specify the files to be reverted.  Files are removed from
		/// <br/> 	the changelist in which they are open.  Locked files are unlocked.
		/// <br/> 
		/// <br/> 	The -a flag  reverts only files that are open for edit or integrate
		/// <br/> 	and are unchanged or missing. Files with pending integration records
		/// <br/> 	are left open. The file arguments are optional when -a is specified.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of the operation.
		/// <br/> 
		/// <br/> 	The -k flag marks the file as reverted in server metadata without
		/// <br/> 	altering files in the client workspace.
		/// <br/> 
		/// <br/> 	The -c flag reverts files that are open in the specified changelist.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public RevertCmdOptions(RevertFilesCmdFlags flags, int changelist)
			: base( flags, changelist)
		{}
	}
	/// <summary>
	/// Flags for the shelve command.
	/// </summary>
	[Flags]
	public enum ShelveFilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -f (force) flag must be used with the -c or -i flag to overwrite
		/// 	any existing shelved files in a pending changelist.
		/// </summary>
		Force = 0x0001,
		/// <summary>
		/// 	The -r flag (used with -c or -i) enables you to replace all shelved
		/// 	files in that changelist with the files opened in your own workspace
		/// 	at that changelist number. Only the user and client workspace of the
		/// 	pending changelist can replace its shelved files.
		/// </summary>
		Replace = 0x0002,
		/// <summary>
		/// 	The -d flag (used with -c) deletes the shelved files in the specified
		/// 	changelist so that they can no longer be unshelved.  By default, only
		/// 	the user and client of the pending changelist can delete its shelved
		/// 	files. A user with 'admin' access can delete shelved files by including
		/// 	the -f flag to force the operation.
		/// </summary>
		Delete = 0x0004,
	}

	public partial class Options
	{
		/// <summary>
		/// Shelve command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="newChangelist"></param>
		/// <param name="changelistId"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help shelve</b>
		/// <br/> 
		/// <br/>     shelve -- Store files from a pending changelist into the depot
		/// <br/> 
		/// <br/>     p4 shelve [files]
		/// <br/>     p4 shelve -i [-f | -r]
		/// <br/>     p4 shelve -r -c changelist#
		/// <br/>     p4 shelve -c changelist# [-f] [file ...]
		/// <br/>     p4 shelve -d -c changelist# [-f] [file ...]
		/// <br/> 
		/// <br/> 	'p4 shelve' creates, modifies or deletes shelved files in a pending
		/// <br/> 	changelist. Shelved files remain in the depot until they are deleted
		/// <br/> 	(using 'p4 shelve -d') or replaced by subsequent shelve commands.
		/// <br/> 	After 'p4 shelve', the user can revert the files and restore them
		/// <br/> 	later using 'p4 unshelve'.  Other users can 'p4 unshelve' the stored
		/// <br/> 	files into their own workspaces.
		/// <br/> 
		/// <br/> 	Files that have been shelved can be accessed by the 'p4 diff',
		/// <br/> 	'p4 diff2', 'p4 files' and 'p4 print' commands using the revision
		/// <br/> 	specification '@=change', where 'change' is the pending changelist
		/// <br/> 	number.
		/// <br/> 
		/// <br/> 	By default, 'p4 shelve' creates a changelist, adds files from the
		/// <br/> 	user's default changelist, then shelves those files in the depot.
		/// <br/> 	The user is presented with a text changelist form displayed using
		/// <br/> 	the editor configured using the $P4EDITOR environment variable.
		/// <br/> 
		/// <br/> 	If a file pattern is specified, 'p4 shelve' shelves the files that
		/// <br/> 	match the pattern.
		/// <br/> 
		/// <br/> 	The -i flag reads the pending changelist specification with shelved
		/// <br/> 	files from the standard input.  The user's editor is not invoked.
		/// <br/> 	To modify an existing changelist with shelved files, specify the
		/// <br/> 	changelist number using the -c flag.
		/// <br/> 
		/// <br/> 	The -c flag specifies the pending changelist that contains shelved
		/// <br/> 	files to be created, deleted, or modified. Only the user and client
		/// <br/> 	of the pending changelist can add or modify its shelved files.
		/// <br/> 
		/// <br/> 	The -f (force) flag must be used with the -c or -i flag to overwrite
		/// <br/> 	any existing shelved files in a pending changelist.
		/// <br/> 
		/// <br/> 	The -r flag (used with -c or -i) enables you to replace all shelved
		/// <br/> 	files in that changelist with the files opened in your own workspace
		/// <br/> 	at that changelist number. Only the user and client workspace of the
		/// <br/> 	pending changelist can replace its shelved files.
		/// <br/> 
		/// <br/> 	The -d flag (used with -c) deletes the shelved files in the specified
		/// <br/> 	changelist so that they can no longer be unshelved.  By default, only
		/// <br/> 	the user and client of the pending changelist can delete its shelved
		/// <br/> 	files. A user with 'admin' access can delete shelved files by including
		/// <br/> 	the -f flag to force the operation.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(ShelveFilesCmdFlags flags, Changelist newChangelist, int changelistId)
		{
			if (newChangelist != null)
				this["-i"] = newChangelist.ToString();

			if ((flags & ShelveFilesCmdFlags.Delete) != 0)
			{
				this["-d"] = null;
			}

			if ((flags & ShelveFilesCmdFlags.Replace) != 0)
			{
				this["-r"] = null;
			}

			if (changelistId > 0)
			{
				this["-c"] = changelistId.ToString();
			}

			if ((flags & ShelveFilesCmdFlags.Force) != 0)
			{
				this["-f"] = null;
			}
		}
	}

	/// <summary>
	/// Shelve command options.
	/// </summary>
	public class ShelveFilesCmdOptions : Options
	{
		/// <summary>
		/// Shelve command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="newChangelist"></param>
		/// <param name="changelistId"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help shelve</b>
		/// <br/> 
		/// <br/>     shelve -- Store files from a pending changelist into the depot
		/// <br/> 
		/// <br/>     p4 shelve [files]
		/// <br/>     p4 shelve -i [-f | -r]
		/// <br/>     p4 shelve -r -c changelist#
		/// <br/>     p4 shelve -c changelist# [-f] [file ...]
		/// <br/>     p4 shelve -d -c changelist# [-f] [file ...]
		/// <br/> 
		/// <br/> 	'p4 shelve' creates, modifies or deletes shelved files in a pending
		/// <br/> 	changelist. Shelved files remain in the depot until they are deleted
		/// <br/> 	(using 'p4 shelve -d') or replaced by subsequent shelve commands.
		/// <br/> 	After 'p4 shelve', the user can revert the files and restore them
		/// <br/> 	later using 'p4 unshelve'.  Other users can 'p4 unshelve' the stored
		/// <br/> 	files into their own workspaces.
		/// <br/> 
		/// <br/> 	Files that have been shelved can be accessed by the 'p4 diff',
		/// <br/> 	'p4 diff2', 'p4 files' and 'p4 print' commands using the revision
		/// <br/> 	specification '@=change', where 'change' is the pending changelist
		/// <br/> 	number.
		/// <br/> 
		/// <br/> 	By default, 'p4 shelve' creates a changelist, adds files from the
		/// <br/> 	user's default changelist, then shelves those files in the depot.
		/// <br/> 	The user is presented with a text changelist form displayed using
		/// <br/> 	the editor configured using the $P4EDITOR environment variable.
		/// <br/> 
		/// <br/> 	If a file pattern is specified, 'p4 shelve' shelves the files that
		/// <br/> 	match the pattern.
		/// <br/> 
		/// <br/> 	The -i flag reads the pending changelist specification with shelved
		/// <br/> 	files from the standard input.  The user's editor is not invoked.
		/// <br/> 	To modify an existing changelist with shelved files, specify the
		/// <br/> 	changelist number using the -c flag.
		/// <br/> 
		/// <br/> 	The -c flag specifies the pending changelist that contains shelved
		/// <br/> 	files to be created, deleted, or modified. Only the user and client
		/// <br/> 	of the pending changelist can add or modify its shelved files.
		/// <br/> 
		/// <br/> 	The -f (force) flag must be used with the -c or -i flag to overwrite
		/// <br/> 	any existing shelved files in a pending changelist.
		/// <br/> 
		/// <br/> 	The -r flag (used with -c or -i) enables you to replace all shelved
		/// <br/> 	files in that changelist with the files opened in your own workspace
		/// <br/> 	at that changelist number. Only the user and client workspace of the
		/// <br/> 	pending changelist can replace its shelved files.
		/// <br/> 
		/// <br/> 	The -d flag (used with -c) deletes the shelved files in the specified
		/// <br/> 	changelist so that they can no longer be unshelved.  By default, only
		/// <br/> 	the user and client of the pending changelist can delete its shelved
		/// <br/> 	files. A user with 'admin' access can delete shelved files by including
		/// <br/> 	the -f flag to force the operation.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public ShelveFilesCmdOptions(ShelveFilesCmdFlags flags, Changelist newChangelist, int changelistId)
			: base(flags, newChangelist, changelistId) { }
	}

	/// <summary>
	/// Flags for the sync command.
	/// </summary>
	[Flags]
	public enum SyncFilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -f flag forces resynchronization even if the client already
		/// 	has the file, and overwriting any writable files.  This flag doesn't
		/// 	affect open files.
		/// </summary>
		Force = 0x0001,
		/// <summary>
		/// 	The -L flag can be used with multiple file arguments that are in
		/// 	full depot syntax and include a valid revision number. When this
		/// 	flag is used the arguments are processed together by building an
		/// 	internal table similar to a label. This file list processing is
		/// 	significantly faster than having to call the internal query engine
		/// 	for each individual file argument. However, the file argument syntax
		/// 	is strict and the command will not run if an error is encountered.
		/// </summary>
		ProcessLikeLabel = 0x0002,
		/// <summary>
		/// 	The -n flag previews the operation without updating the workspace.
		/// </summary>
		Preview = 0x0004,
		/// <summary>
		/// 	The -k flag updates server metadata without syncing files. It is
		/// 	intended to enable you to ensure that the server correctly reflects
		/// 	the state of files in the workspace while avoiding a large data
		/// 	transfer. Caution: an erroneous update can cause the server to
		/// 	incorrectly reflect the state of the workspace.
		/// </summary>
		ServerOnly = 0x0008,
		/// <summary>
		/// 	The -p flag populates the client workspace, but does not update the
		/// 	server to reflect those updates.  Any file that is already synced or
		/// 	opened will be bypassed with a warning message.  This option is very
		/// 	useful for build clients or when publishing content without the
		/// 	need to track the state of the client workspace.
		/// </summary>
		PopulateClient = 0x0010,
		/// <summary>
		/// 	The -q flag suppresses normal output messages. Messages regarding
		/// 	errors or exceptional conditions are not suppressed.
		/// </summary>
		Quiet = 0x0020,
		/// <summary>
		/// 	The -s flag adds a safety check before sending content to the client
		/// 	workspace.  This check uses MD5 digests to compare the content on the
		/// 	clients workspace against content that was last synced.  If the file
		/// 	has been modified outside of Perforce's control then an error message
		/// 	is displayed and the file is not overwritten.  This check adds some
		/// 	extra processing which will affect the performance of the operation.
		/// </summary>
		SafeMode = 0x0020
	}

	public partial class Options
	{
		/// <summary>
		/// Sync command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="maxItems"></param>
		/// <remarks>
		/// <br/><b>p4 help sync</b>
		/// <br/> 
		/// <br/>     sync -- Synchronize the client with its view of the depot
		/// <br/>     flush -- synonym for 'sync -k'
		/// <br/>     update -- synonym for 'sync -s'
		/// <br/> 
		/// <br/>     p4 sync [-f -L -n -k -q] [-m max] [file[revRange] ...]
		/// <br/>     p4 sync [-L -n -q -s] [-m max] [file[revRange] ...]
		/// <br/>     p4 sync [-L -n -p -q] [-m max] [file[revRange] ...]
		/// <br/> 
		/// <br/> 	Sync updates the client workspace to reflect its current view (if
		/// <br/> 	it has changed) and the current contents of the depot (if it has
		/// <br/> 	changed). The client view maps client and depot file names and
		/// <br/> 	locations.
		/// <br/> 
		/// <br/> 	Sync adds files that are in the client view and have not been
		/// <br/> 	retrieved before.  Sync deletes previously retrieved files that
		/// <br/> 	are no longer in the client view or have been deleted from the
		/// <br/> 	depot.  Sync updates files that are still in the client view and
		/// <br/> 	have been updated in the depot.
		/// <br/> 
		/// <br/> 	By default, sync affects all files in the client workspace. If file
		/// <br/> 	arguments are given, sync limits its operation to those files.
		/// <br/> 	The file arguments can contain wildcards.
		/// <br/> 
		/// <br/> 	If the file argument includes a revision specifier, then the given
		/// <br/> 	revision is retrieved.  Normally, the head revision is retrieved.
		/// <br/> 	See 'p4 help revisions' for help specifying revisions.
		/// <br/> 
		/// <br/> 	If the file argument includes a revision range specification,
		/// <br/> 	only files selected by the revision range are updated, and the
		/// <br/> 	highest revision in the range is used.
		/// <br/> 
		/// <br/> 	Normally, sync does not overwrite workspace files that the user has
		/// <br/> 	manually made writable.  Setting the 'clobber' option in the
		/// <br/> 	client specification disables this safety check.
		/// <br/> 
		/// <br/> 	The -f flag forces resynchronization even if the client already
		/// <br/> 	has the file, and overwriting any writable files.  This flag doesn't
		/// <br/> 	affect open files.
		/// <br/> 
		/// <br/> 	The -L flag can be used with multiple file arguments that are in
		/// <br/> 	full depot syntax and include a valid revision number. When this
		/// <br/> 	flag is used the arguments are processed together by building an
		/// <br/> 	internal table similar to a label. This file list processing is
		/// <br/> 	significantly faster than having to call the internal query engine
		/// <br/> 	for each individual file argument. However, the file argument syntax
		/// <br/> 	is strict and the command will not run if an error is encountered.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without updating the workspace.
		/// <br/> 
		/// <br/> 	The -k flag updates server metadata without syncing files. It is
		/// <br/> 	intended to enable you to ensure that the server correctly reflects
		/// <br/> 	the state of files in the workspace while avoiding a large data
		/// <br/> 	transfer. Caution: an erroneous update can cause the server to
		/// <br/> 	incorrectly reflect the state of the workspace.
		/// <br/> 
		/// <br/> 	The -p flag populates the client workspace, but does not update the
		/// <br/> 	server to reflect those updates.  Any file that is already synced or
		/// <br/> 	opened will be bypassed with a warning message.  This option is very
		/// <br/> 	useful for build clients or when publishing content without the
		/// <br/> 	need to track the state of the client workspace.
		/// <br/> 
		/// <br/> 	The -q flag suppresses normal output messages. Messages regarding
		/// <br/> 	errors or exceptional conditions are not suppressed.
		/// <br/> 
		/// <br/> 	The -s flag adds a safety check before sending content to the client
		/// <br/> 	workspace.  This check uses MD5 digests to compare the content on the
		/// <br/> 	clients workspace against content that was last synced.  If the file
		/// <br/> 	has been modified outside of Perforce's control then an error message
		/// <br/> 	is displayed and the file is not overwritten.  This check adds some
		/// <br/> 	extra processing which will affect the performance of the operation.
		/// <br/> 
		/// <br/> 	The -m flag limits sync to the first 'max' number of files. This
		/// <br/> 	option is useful in conjunction with tagged output and the '-n'
		/// <br/> 	flag, to preview how many files will be synced without transferring
		/// <br/> 	all the file data.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(SyncFilesCmdFlags flags, int maxItems)
		{
			if ((flags & SyncFilesCmdFlags.Force) != 0)
			{
				this["-f"] = null;
			}
			if ((flags & SyncFilesCmdFlags.ProcessLikeLabel) != 0)
			{
				this["-L"] = null;
			}
			if ((flags & SyncFilesCmdFlags.Preview) != 0)
			{
				this["-n"] = null;
			}
			if ((flags & SyncFilesCmdFlags.ServerOnly) != 0)
			{
				this["-k"] = null;
			}
			if ((flags & SyncFilesCmdFlags.PopulateClient) != 0)
			{
				this["-p"] = null;
			}
			if ((flags & SyncFilesCmdFlags.Quiet) != 0)
			{
				this["-q"] = null;
			}
			if ((flags & SyncFilesCmdFlags.SafeMode) != 0)
			{
				this["-s"] = null;
			}
			if (maxItems > 0)
			{
				this["-m"] = maxItems.ToString();
			}
		}
	}
		/// <summary>
		/// Sync command options.
		/// </summary>
	public class SyncFilesCmdOptions: Options
	{
		/// <summary>
		/// Sync command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="maxItems"></param>
		/// <remarks>
		/// <br/><b>p4 help sync</b>
		/// <br/> 
		/// <br/>     sync -- Synchronize the client with its view of the depot
		/// <br/>     flush -- synonym for 'sync -k'
		/// <br/>     update -- synonym for 'sync -s'
		/// <br/> 
		/// <br/>     p4 sync [-f -L -n -k -q] [-m max] [file[revRange] ...]
		/// <br/>     p4 sync [-L -n -q -s] [-m max] [file[revRange] ...]
		/// <br/>     p4 sync [-L -n -p -q] [-m max] [file[revRange] ...]
		/// <br/> 
		/// <br/> 	Sync updates the client workspace to reflect its current view (if
		/// <br/> 	it has changed) and the current contents of the depot (if it has
		/// <br/> 	changed). The client view maps client and depot file names and
		/// <br/> 	locations.
		/// <br/> 
		/// <br/> 	Sync adds files that are in the client view and have not been
		/// <br/> 	retrieved before.  Sync deletes previously retrieved files that
		/// <br/> 	are no longer in the client view or have been deleted from the
		/// <br/> 	depot.  Sync updates files that are still in the client view and
		/// <br/> 	have been updated in the depot.
		/// <br/> 
		/// <br/> 	By default, sync affects all files in the client workspace. If file
		/// <br/> 	arguments are given, sync limits its operation to those files.
		/// <br/> 	The file arguments can contain wildcards.
		/// <br/> 
		/// <br/> 	If the file argument includes a revision specifier, then the given
		/// <br/> 	revision is retrieved.  Normally, the head revision is retrieved.
		/// <br/> 	See 'p4 help revisions' for help specifying revisions.
		/// <br/> 
		/// <br/> 	If the file argument includes a revision range specification,
		/// <br/> 	only files selected by the revision range are updated, and the
		/// <br/> 	highest revision in the range is used.
		/// <br/> 
		/// <br/> 	Normally, sync does not overwrite workspace files that the user has
		/// <br/> 	manually made writable.  Setting the 'clobber' option in the
		/// <br/> 	client specification disables this safety check.
		/// <br/> 
		/// <br/> 	The -f flag forces resynchronization even if the client already
		/// <br/> 	has the file, and overwriting any writable files.  This flag doesn't
		/// <br/> 	affect open files.
		/// <br/> 
		/// <br/> 	The -L flag can be used with multiple file arguments that are in
		/// <br/> 	full depot syntax and include a valid revision number. When this
		/// <br/> 	flag is used the arguments are processed together by building an
		/// <br/> 	internal table similar to a label. This file list processing is
		/// <br/> 	significantly faster than having to call the internal query engine
		/// <br/> 	for each individual file argument. However, the file argument syntax
		/// <br/> 	is strict and the command will not run if an error is encountered.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without updating the workspace.
		/// <br/> 
		/// <br/> 	The -k flag updates server metadata without syncing files. It is
		/// <br/> 	intended to enable you to ensure that the server correctly reflects
		/// <br/> 	the state of files in the workspace while avoiding a large data
		/// <br/> 	transfer. Caution: an erroneous update can cause the server to
		/// <br/> 	incorrectly reflect the state of the workspace.
		/// <br/> 
		/// <br/> 	The -p flag populates the client workspace, but does not update the
		/// <br/> 	server to reflect those updates.  Any file that is already synced or
		/// <br/> 	opened will be bypassed with a warning message.  This option is very
		/// <br/> 	useful for build clients or when publishing content without the
		/// <br/> 	need to track the state of the client workspace.
		/// <br/> 
		/// <br/> 	The -q flag suppresses normal output messages. Messages regarding
		/// <br/> 	errors or exceptional conditions are not suppressed.
		/// <br/> 
		/// <br/> 	The -s flag adds a safety check before sending content to the client
		/// <br/> 	workspace.  This check uses MD5 digests to compare the content on the
		/// <br/> 	clients workspace against content that was last synced.  If the file
		/// <br/> 	has been modified outside of Perforce's control then an error message
		/// <br/> 	is displayed and the file is not overwritten.  This check adds some
		/// <br/> 	extra processing which will affect the performance of the operation.
		/// <br/> 
		/// <br/> 	The -m flag limits sync to the first 'max' number of files. This
		/// <br/> 	option is useful in conjunction with tagged output and the '-n'
		/// <br/> 	flag, to preview how many files will be synced without transferring
		/// <br/> 	all the file data.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public SyncFilesCmdOptions(SyncFilesCmdFlags flags, int maxItems)
			:base (flags, maxItems)
		{
		}
	}	/// <summary>
	/// Flags for the unlock command.
	/// </summary>
	[Flags]
	public enum UnlockFilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	By default, files can be unlocked only by the changelist owner. The
		/// 	-f flag enables you to unlock files in changelists owned by other
		/// 	users. The -f flag requires 'admin' access, which is granted by 'p4
		/// 	protect'.
		/// </summary>
		Force = 0x0001
	}
	public partial class Options
	{
		/// <summary>
		/// Unlock command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changelistId"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help unlock</b>
		/// <br/> 
		/// <br/>     unlock -- Release a locked file, leaving it open
		/// <br/> 
		/// <br/>     p4 unlock [-c changelist#] [-f] [file ...]
		/// <br/> 
		/// <br/> 	'p4 unlock' releases locks on the specified files, which must be
		/// <br/> 	open in the specified pending changelist. If you omit the changelist
		/// <br/> 	number, the default changelist is assumed. If you omit the file name,
		/// <br/> 	all locked files are unlocked.
		/// <br/> 
		/// <br/> 	By default, files can be unlocked only by the changelist owner. The
		/// <br/> 	-f flag enables you to unlock files in changelists owned by other
		/// <br/> 	users. The -f flag requires 'admin' access, which is granted by 'p4
		/// <br/> 	protect'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(UnlockFilesCmdFlags flags, int changelistId)
		{
			if (changelistId > 0)
			{
				this["-c"] = changelistId.ToString();
			}

			if ((flags & UnlockFilesCmdFlags.Force) != 0)
			{
				this["-f"] = null;
			}
		}
	}
		/// <summary>
		/// Unlock command options.
		/// </summary>
	public class UnlockFilesCmdOptions : Options
	{
		/// <summary>
		/// Unlock command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changelistId"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help unlock</b>
		/// <br/> 
		/// <br/>     unlock -- Release a locked file, leaving it open
		/// <br/> 
		/// <br/>     p4 unlock [-c changelist#] [-f] [file ...]
		/// <br/> 
		/// <br/> 	'p4 unlock' releases locks on the specified files, which must be
		/// <br/> 	open in the specified pending changelist. If you omit the changelist
		/// <br/> 	number, the default changelist is assumed. If you omit the file name,
		/// <br/> 	all locked files are unlocked.
		/// <br/> 
		/// <br/> 	By default, files can be unlocked only by the changelist owner. The
		/// <br/> 	-f flag enables you to unlock files in changelists owned by other
		/// <br/> 	users. The -f flag requires 'admin' access, which is granted by 'p4
		/// <br/> 	protect'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public UnlockFilesCmdOptions(UnlockFilesCmdFlags flags, int changelistId)
			: base(flags, changelistId)	{}
	}	
	/// <summary>
	/// Flags for the unshelve command.
	/// </summary>
	[Flags]
	public enum UnshelveFilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -f flag forces the clobbering of any writeable but unopened files
		/// 	that are being unshelved.
		/// </summary>
		Force = 0x0001,
		/// <summary>
		/// 	The -n flag previews the operation without changing any files or
		/// 	metadata.
		/// </summary>
		Preview = 0x0002,
	}

	public partial class Options
	{
		/// <summary>
		/// Unshelve command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changelistId"></param>
		/// <param name="newChangelistId"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help unshelve</b>
		/// <br/> 
		/// <br/>     unshelve -- Restore shelved files from a pending change into a workspace
		/// <br/> 
		/// <br/>     p4 unshelve -s changelist# [-f -n] [-c changelist#] [file ...]
		/// <br/> 
		/// <br/> 	'p4 unshelve' retrieves shelved files from the specified pending
		/// <br/> 	changelist, opens them in a pending changelist and copies them
		/// <br/> 	to the invoking user's workspace.  Unshelving files from a pending
		/// <br/> 	changelist is restricted by the user's permissions on the files.
		/// <br/> 	A successful unshelve operation places the shelved files on the
		/// <br/> 	user's workspace with the same open action and pending integration
		/// <br/> 	history as if it had originated from that user and client.
		/// <br/> 
		/// <br/> 	Unshelving a file over an already opened file is only permitted
		/// <br/> 	if both shelved file and opened file are opened for 'edit'. After
		/// <br/> 	unshelving, the workspace file is flagged as unresolved, and
		/// <br/> 	'p4 resolve' must be run to resolve the differences between the
		/// <br/> 	shelved file and the workspace file.
		/// <br/> 
		/// <br/> 	The -s flag specifies the number of the pending changelist that
		/// <br/> 	contains the shelved files.
		/// <br/> 
		/// <br/> 	If a file pattern is specified, 'p4 unshelve' unshelves files that
		/// <br/> 	match the pattern.
		/// <br/> 
		/// <br/> 	The -c flag specifies the changelist to which files are unshelved.
		/// <br/> 	By default,  'p4 unshelve' opens shelved files in the default
		/// <br/> 	changelist.
		/// <br/> 
		/// <br/> 	The -f flag forces the clobbering of any writeable but unopened files
		/// <br/> 	that are being unshelved.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without changing any files or
		/// <br/> 	metadata.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(UnshelveFilesCmdFlags flags, int changelistId, int newChangelistId)
		{
			this["-s"] = changelistId.ToString();

			if ((flags & UnshelveFilesCmdFlags.Force) != 0)
			{
				this["-f"] = null;
			}

			if ((flags & UnshelveFilesCmdFlags.Preview) != 0)
			{
				this["-n"] = null;
			}

			if (newChangelistId > 0)
			{
				this["-c"] = newChangelistId.ToString();
			}

			if (newChangelistId == 0)
			{
				this["-c"] = "default";
			}
		}
	}

	/// <summary>
	/// Unshelve command options.
	/// </summary>
	public class UnshelveFilesCmdOptions : Options
	{
		/// <summary>
		/// Unshelve command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changelistId"></param>
		/// <param name="newChangelistId"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help unshelve</b>
		/// <br/> 
		/// <br/>     unshelve -- Restore shelved files from a pending change into a workspace
		/// <br/> 
		/// <br/>     p4 unshelve -s changelist# [-f -n] [-c changelist#] [file ...]
		/// <br/> 
		/// <br/> 	'p4 unshelve' retrieves shelved files from the specified pending
		/// <br/> 	changelist, opens them in a pending changelist and copies them
		/// <br/> 	to the invoking user's workspace.  Unshelving files from a pending
		/// <br/> 	changelist is restricted by the user's permissions on the files.
		/// <br/> 	A successful unshelve operation places the shelved files on the
		/// <br/> 	user's workspace with the same open action and pending integration
		/// <br/> 	history as if it had originated from that user and client.
		/// <br/> 
		/// <br/> 	Unshelving a file over an already opened file is only permitted
		/// <br/> 	if both shelved file and opened file are opened for 'edit'. After
		/// <br/> 	unshelving, the workspace file is flagged as unresolved, and
		/// <br/> 	'p4 resolve' must be run to resolve the differences between the
		/// <br/> 	shelved file and the workspace file.
		/// <br/> 
		/// <br/> 	The -s flag specifies the number of the pending changelist that
		/// <br/> 	contains the shelved files.
		/// <br/> 
		/// <br/> 	If a file pattern is specified, 'p4 unshelve' unshelves files that
		/// <br/> 	match the pattern.
		/// <br/> 
		/// <br/> 	The -c flag specifies the changelist to which files are unshelved.
		/// <br/> 	By default,  'p4 unshelve' opens shelved files in the default
		/// <br/> 	changelist.
		/// <br/> 
		/// <br/> 	The -f flag forces the clobbering of any writeable but unopened files
		/// <br/> 	that are being unshelved.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without changing any files or
		/// <br/> 	metadata.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public UnshelveFilesCmdOptions(UnshelveFilesCmdFlags flags, int changelistId, int newChangelistId)
			: base(flags, changelistId, newChangelistId) { }
	}

	/// <summary>
	/// Flags for the copy command.
	/// </summary>
	[Flags]
	public enum CopyFilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -n flag displays a preview of the copy, without actually doing
		/// 	anything.
		/// </summary>
		Preview = 0x0001,
		/// <summary>
		/// 	The -v flag causes a 'virtual' copy that does not modify client
		/// 	workspace files.  After submitting a virtual integration, 'p4 sync'
		/// 	can be used to update the workspace.
		/// </summary>
		Virtual = 0x0002,
		/// <summary>
		/// 	The -r flag causes the direction of the copy to be reversed when 
		/// 	used with a branch (-b) or stream (-S) copy.
		/// </summary>
		Reverse = 0x0004,
		/// <summary>
		/// 	The -F flag can be used with -S to force copying even though the
		/// 	stream does not expect a copy to occur in the direction indicated.
		/// 	Normally 'p4 copy' enforces the expected flow of change dictated
		/// 	by the stream's spec. The 'p4 istat' command summarizes a stream's
		/// 	expected flow of change.
		/// </summary>
		Force = 0x0008,
		/// <summary>
		/// 	The -s flag can be used with -b to cause fromFile to be treated as
		/// 	the source, and both sides of the user-defined branch view to be
		/// 	treated as the target, per the branch view mapping.  Optional toFile
		/// 	arguments may be given to further restrict the scope of the target
		/// 	file set. -r is ignored when -s is used.
		/// </summary>
		SourceBranch = 0x0010
	}
	public partial class Options
	{
		/// <summary>
		/// Copy command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="branchName"></param>
		/// <param name="streamName"></param>
		/// <param name="parentStream"></param>
		/// <param name="changelistId"></param>
		/// <param name="maxItems"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help copy</b>
		/// <br/> 
		/// <br/>     copy -- Copy one set of files to another
		/// <br/> 
		/// <br/>     p4 copy [options] fromFile[rev] toFile
		/// <br/>     p4 copy [options] -b branch [-r] [toFile[rev] ...]
		/// <br/>     p4 copy [options] -b branch -s fromFile[rev] [toFile ...]
		/// <br/>     p4 copy [options] -S stream [-P parent] [-F] [-r] [toFile[rev] ...]
		/// <br/> 
		/// <br/> 	options: -c changelist# -n -v -m max
		/// <br/> 
		/// <br/> 	'p4 copy' copies one set of files (the 'source') into another (the
		/// <br/> 	'target').
		/// <br/> 
		/// <br/> 	Using the client workspace as a staging area, 'p4 copy' makes the
		/// <br/> 	target identical to the source by branching, replacing, or deleting
		/// <br/> 	files.  'p4 submit' submits copied files to the depot. 'p4 revert'
		/// <br/> 	can be used to revert copied files instead of submitting them.  The
		/// <br/> 	history of copied files can be shown with 'p4 filelog' or 'p4
		/// <br/> 	integrated'.
		/// <br/> 
		/// <br/> 	Target files that are already identical to the source, or that are
		/// <br/> 	outside of the client view, are not affected by 'p4 copy'. Opened,
		/// <br/> 	non-identical target files cause 'p4 copy' to exit with a warning. 
		/// <br/> 	When 'p4 copy' creates or modifies files in the workspace, it leaves
		/// <br/> 	them read-only; 'p4 edit' can make them writable.  Files opened by
		/// <br/> 	'p4 copy' do not need to be resolved.
		/// <br/> 
		/// <br/> 	Source and target files (fromFile and toFile) can be specified on
		/// <br/> 	the 'p4 copy' command line or through a branch view. On the command
		/// <br/> 	line, fromFile is the source file set and toFile is the target file
		/// <br/> 	set.  With a branch view, one or more toFile arguments can be given
		/// <br/> 	to limit the scope of the target file set.
		/// <br/> 
		/// <br/> 	A revision specifier can be used to select the revision to copy; by
		/// <br/> 	default, the head revision is copied. The revision specifier can be
		/// <br/> 	used on fromFile, or on toFile, but not on both.  When used on toFile,
		/// <br/> 	it refers to source revisions, not to target revisions.  A range may
		/// <br/> 	not be used as a revision specifier.  For revision syntax, see 'p4
		/// <br/> 	help revisions'.
		/// <br/> 
		/// <br/> 	The -S flag makes 'p4 copy' use a stream's branch view.  (See 'p4 help
		/// <br/> 	stream'.) The source is the stream itself, and the target is the
		/// <br/> 	stream's parent. With -r, the direction is reversed.  -P can be used
		/// <br/> 	to specify a parent stream other than the stream's actual parent.
		/// <br/> 	Note that to submit copied stream files, the current client must
		/// <br/> 	be dedicated to the target stream. (See 'p4 help client'.)
		/// <br/> 
		/// <br/> 	The -F flag can be used with -S to force copying even though the
		/// <br/> 	stream does not expect a copy to occur in the direction indicated.
		/// <br/> 	Normally 'p4 copy' enforces the expected flow of change dictated
		/// <br/> 	by the stream's spec. The 'p4 istat' command summarizes a stream's
		/// <br/> 	expected flow of change.
		/// <br/> 
		/// <br/> 	The -b flag makes 'p4 copy' use a user-defined branch view.  (See
		/// <br/> 	'p4 help branch'.) The source is the left side of the branch view
		/// <br/> 	and the target is the right side. With -r, the direction is reversed.
		/// <br/> 
		/// <br/> 	The -s flag can be used with -b to cause fromFile to be treated as
		/// <br/> 	the source, and both sides of the user-defined branch view to be
		/// <br/> 	treated as the target, per the branch view mapping.  Optional toFile
		/// <br/> 	arguments may be given to further restrict the scope of the target
		/// <br/> 	file set. -r is ignored when -s is used.
		/// <br/> 
		/// <br/> 	The -c changelist# flag opens files in the designated (numbered)
		/// <br/> 	pending changelist instead of the default changelist.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of the copy, without actually doing
		/// <br/> 	anything.
		/// <br/> 
		/// <br/> 	The -m flag limits the actions to the first 'max' number of files.
		/// <br/> 
		/// <br/> 	The -v flag causes a 'virtual' copy that does not modify client
		/// <br/> 	workspace files.  After submitting a virtual integration, 'p4 sync'
		/// <br/> 	can be used to update the workspace.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(CopyFilesCmdFlags flags, string branchName, string streamName, string parentStream, int changelistId, int maxItems)
		{
			if (changelistId >= 0)
			{
				this["-c"] = changelistId.ToString();
			}

			if ((flags & CopyFilesCmdFlags.Preview) != 0)
			{
				this["-n"] = null;
			}

			if ((flags & CopyFilesCmdFlags.Virtual) != 0)
			{
				this["-v"] = null;
			}

			if (maxItems >= 0)
			{
				this["-m"] = maxItems.ToString();
			}

			if (String.IsNullOrEmpty(branchName) != true)
			{
				this["-b"] = branchName;
			}

			if (String.IsNullOrEmpty(streamName) != true)
			{
				this["-S"] = streamName;
			}

			if (String.IsNullOrEmpty(parentStream) != true)
			{
				this["-P"] = parentStream;
			}

			if ((flags & CopyFilesCmdFlags.Force) != 0)
			{
				this["-F"] = null;
			}

			if ((flags & CopyFilesCmdFlags.Reverse) != 0)
			{
				this["-r"] = null;
			}

			if ((flags & CopyFilesCmdFlags.SourceBranch) != 0)
			{
				this["-r"] = null;
			}
		}
	}
	/// <summary>
	/// Copy command options.
	/// </summary>
	public class CopyFilesCmdOptions : Options
	{
		/// <summary>
		/// Copy command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="branchName"></param>
		/// <param name="streamName"></param>
		/// <param name="parentStream"></param>
		/// <param name="changelistId"></param>
		/// <param name="maxItems"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help copy</b>
		/// <br/> 
		/// <br/>     copy -- Copy one set of files to another
		/// <br/> 
		/// <br/>     p4 copy [options] fromFile[rev] toFile
		/// <br/>     p4 copy [options] -b branch [-r] [toFile[rev] ...]
		/// <br/>     p4 copy [options] -b branch -s fromFile[rev] [toFile ...]
		/// <br/>     p4 copy [options] -S stream [-P parent] [-F] [-r] [toFile[rev] ...]
		/// <br/> 
		/// <br/> 	options: -c changelist# -n -v -m max
		/// <br/> 
		/// <br/> 	'p4 copy' copies one set of files (the 'source') into another (the
		/// <br/> 	'target').
		/// <br/> 
		/// <br/> 	Using the client workspace as a staging area, 'p4 copy' makes the
		/// <br/> 	target identical to the source by branching, replacing, or deleting
		/// <br/> 	files.  'p4 submit' submits copied files to the depot. 'p4 revert'
		/// <br/> 	can be used to revert copied files instead of submitting them.  The
		/// <br/> 	history of copied files can be shown with 'p4 filelog' or 'p4
		/// <br/> 	integrated'.
		/// <br/> 
		/// <br/> 	Target files that are already identical to the source, or that are
		/// <br/> 	outside of the client view, are not affected by 'p4 copy'. Opened,
		/// <br/> 	non-identical target files cause 'p4 copy' to exit with a warning. 
		/// <br/> 	When 'p4 copy' creates or modifies files in the workspace, it leaves
		/// <br/> 	them read-only; 'p4 edit' can make them writable.  Files opened by
		/// <br/> 	'p4 copy' do not need to be resolved.
		/// <br/> 
		/// <br/> 	Source and target files (fromFile and toFile) can be specified on
		/// <br/> 	the 'p4 copy' command line or through a branch view. On the command
		/// <br/> 	line, fromFile is the source file set and toFile is the target file
		/// <br/> 	set.  With a branch view, one or more toFile arguments can be given
		/// <br/> 	to limit the scope of the target file set.
		/// <br/> 
		/// <br/> 	A revision specifier can be used to select the revision to copy; by
		/// <br/> 	default, the head revision is copied. The revision specifier can be
		/// <br/> 	used on fromFile, or on toFile, but not on both.  When used on toFile,
		/// <br/> 	it refers to source revisions, not to target revisions.  A range may
		/// <br/> 	not be used as a revision specifier.  For revision syntax, see 'p4
		/// <br/> 	help revisions'.
		/// <br/> 
		/// <br/> 	The -S flag makes 'p4 copy' use a stream's branch view.  (See 'p4 help
		/// <br/> 	stream'.) The source is the stream itself, and the target is the
		/// <br/> 	stream's parent. With -r, the direction is reversed.  -P can be used
		/// <br/> 	to specify a parent stream other than the stream's actual parent.
		/// <br/> 	Note that to submit copied stream files, the current client must
		/// <br/> 	be dedicated to the target stream. (See 'p4 help client'.)
		/// <br/> 
		/// <br/> 	The -F flag can be used with -S to force copying even though the
		/// <br/> 	stream does not expect a copy to occur in the direction indicated.
		/// <br/> 	Normally 'p4 copy' enforces the expected flow of change dictated
		/// <br/> 	by the stream's spec. The 'p4 istat' command summarizes a stream's
		/// <br/> 	expected flow of change.
		/// <br/> 
		/// <br/> 	The -b flag makes 'p4 copy' use a user-defined branch view.  (See
		/// <br/> 	'p4 help branch'.) The source is the left side of the branch view
		/// <br/> 	and the target is the right side. With -r, the direction is reversed.
		/// <br/> 
		/// <br/> 	The -s flag can be used with -b to cause fromFile to be treated as
		/// <br/> 	the source, and both sides of the user-defined branch view to be
		/// <br/> 	treated as the target, per the branch view mapping.  Optional toFile
		/// <br/> 	arguments may be given to further restrict the scope of the target
		/// <br/> 	file set. -r is ignored when -s is used.
		/// <br/> 
		/// <br/> 	The -c changelist# flag opens files in the designated (numbered)
		/// <br/> 	pending changelist instead of the default changelist.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of the copy, without actually doing
		/// <br/> 	anything.
		/// <br/> 
		/// <br/> 	The -m flag limits the actions to the first 'max' number of files.
		/// <br/> 
		/// <br/> 	The -v flag causes a 'virtual' copy that does not modify client
		/// <br/> 	workspace files.  After submitting a virtual integration, 'p4 sync'
		/// <br/> 	can be used to update the workspace.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public CopyFilesCmdOptions(CopyFilesCmdFlags flags, string branchName, string streamName, string parentStream, int changelistId, int maxItems)
			:base(flags, branchName, streamName, parentStream, changelistId, maxItems) {}
	}

	/// <summary>
	/// Flags for the merge command.
	/// </summary>
	[Flags]
	public enum MergeFilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -n flag displays a preview of the copy, without actually doing
		/// 	anything.
		/// </summary>
		Preview = 0x0001,
		/// <summary>
		/// 	The -r flag causes the direction of the copy to be reversed when 
		/// 	used with a branch (-b) or stream (-S) copy.
		/// </summary>
		Reverse = 0x0002,
		/// <summary>
		/// 	The -F flag can be used with -S to force copying even though the
		/// 	stream does not expect a copy to occur in the direction indicated.
		/// 	Normally 'p4 copy' enforces the expected flow of change dictated
		/// 	by the stream's spec. The 'p4 istat' command summarizes a stream's
		/// 	expected flow of change.
		/// </summary>
		Force = 0x0004,
		/// <summary>
		/// 	The -s flag can be used with -b to cause fromFile to be treated as
		/// 	the source, and both sides of the user-defined branch view to be
		/// 	treated as the target, per the branch view mapping.  Optional toFile
		/// 	arguments may be given to further restrict the scope of the target
		/// 	file set. -r is ignored when -s is used.
		/// </summary>
		SourceBranch = 0x0008
	}
	public partial class Options
	{
		/// <br/><b>p4 help merge</b>
		/// <br/> 
		/// <br/>     merge -- Merge one set of files into another 
		/// <br/> 
		/// <br/>     p4 merge [options] fromFile[revRange] toFile
		/// <br/>     p4 merge [options] -b branch [-r] [toFile[revRange] ...]
		/// <br/>     p4 merge [options] -b branch -s fromFile[revRange] [toFile ...]
		/// <br/>     p4 merge [options] -S stream [-P parent] [-F] [-r] [toFile[revRange] ...]
		/// <br/> 
		/// <br/> 	options: -c changelist# -n -m max
		/// <br/> 
		/// <br/> 	'p4 merge' merges changes from one set of files (the 'source') into 
		/// <br/> 	another (the 'target'). It is a simplified form of the 'p4 integrate'
		/// <br/> 	command.
		/// <br/> 
		/// <br/> 	Using the client workspace as a staging area, 'p4 merge' branches and
		/// <br/> 	deletes target files per changes in the source, and schedules all 
		/// <br/> 	other affected target files to be resolved.  Target files outside of
		/// <br/> 	the current client view are not affected. Source files need not be
		/// <br/> 	within the client view.
		/// <br/> 
		/// <br/> 	'p4 resolve' must be used to merge file content, and to resolve 
		/// <br/> 	filename and filetype changes. 'p4 submit' commits merged files to
		/// <br/> 	the depot.  Unresolved files may not be submitted.  Merged files can
		/// <br/> 	be shelved with 'p4 shelve' and abandoned with 'p4 revert'.  The 
		/// <br/> 	commands 'p4 integrated' and 'p4 filelog' display merge history.
		/// <br/> 
		/// <br/> 	When 'p4 merge' schedules a workspace file to be resolved, it leaves
		/// <br/> 	it read-only. 'p4 resolve' can operate on a read-only file;  for 
		/// <br/> 	other pre-submit changes, 'p4 edit' must be used to make the file 
		/// <br/> 	writable.
		/// <br/> 
		/// <br/> 	Source and target files can be specified either on the 'p4 merge'
		/// <br/> 	command line or through a branch view. On the command line, fromFile
		/// <br/> 	is the source file set and toFile is the target file set.  With a 
		/// <br/> 	branch view, one or more toFile arguments can be given to limit the 
		/// <br/> 	scope of the target file set. 
		/// <br/> 
		/// <br/> 	Each file in the target is mapped to a file in the source. Mapping 
		/// <br/> 	adjusts automatically for files that have been moved or renamed, as
		/// <br/> 	long as 'p4 move' was used to move/rename files.  The scope of source
		/// <br/> 	and target file sets must include both old-named and new-named files
		/// <br/> 	for mappings to be adjusted.  Moved source files may schedule moves 
		/// <br/> 	to be resolved in target files. 
		/// <br/> 
		/// <br/> 	revRange is a revision or a revision range that limits the span of
		/// <br/> 	source history to be probed for unintegrated revisions.  revRange 
		/// <br/> 	can be used on fromFile, or on toFile, but not on both.  When used
		/// <br/> 	on toFile, it refers to source revisions, not to target revisions.
		/// <br/> 	For details about revision specifiers, see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -S flag makes 'p4 merge' use a stream's branch view.  (See 'p4
		/// <br/> 	help stream'.) The source is the stream itself, and the target is
		/// <br/> 	the stream's parent. With -r, the direction is reversed.  -P can be
		/// <br/> 	used to specify a parent stream other than the stream's actual parent.
		/// <br/> 	Note that to submit merged stream files, the current client must
		/// <br/> 	be dedicated to the target stream. (See 'p4 help client'.)
		/// <br/> 
		/// <br/> 	The -F flag can be used with -S to force merging even though the
		/// <br/> 	stream does not expect a merge to occur in the direction indicated.
		/// <br/> 	Normally 'p4 merge' enforces the expected flow of change dictated
		/// <br/> 	by the stream's spec. The 'p4 istat' command summarizes a stream's
		/// <br/> 	expected flow of change.
		/// <br/> 
		/// <br/> 	The -b flag makes 'p4 merge' use a user-defined branch view.  (See
		/// <br/> 	'p4 help branch'.) The source is the left side of the branch view
		/// <br/> 	and the target is the right side. With -r, the direction is reversed.
		/// <br/> 
		/// <br/> 	The -s flag can be used with -b to cause fromFile to be treated as
		/// <br/> 	the source, and both sides of the branch view to be treated as the
		/// <br/> 	target, per the branch view mapping.  Optional toFile arguments may
		/// <br/> 	be given to further restrict the scope of the target file set.  The
		/// <br/> 	-r flag is ignored when -s is used.
		/// <br/> 
		/// <br/> 

		public Options(MergeFilesCmdFlags flags, string branchName, string streamName, string parentStream, int changelistId, int maxItems)
		{
			if (changelistId >= 0)
			{
				this["-c"] = changelistId.ToString();
			}

			if ((flags & MergeFilesCmdFlags.Preview) != 0)
			{
				this["-n"] = null;
			}

			if (maxItems >= 0)
			{
				this["-m"] = maxItems.ToString();
			}

			if (String.IsNullOrEmpty(branchName) != true)
			{
				this["-b"] = branchName;
			}

			if (String.IsNullOrEmpty(streamName) != true)
			{
				this["-S"] = streamName;
			}

			if (String.IsNullOrEmpty(parentStream) != true)
			{
				this["-P"] = parentStream;
			}

			if ((flags & MergeFilesCmdFlags.Force) != 0)
			{
				this["-F"] = null;
			}

			if ((flags & MergeFilesCmdFlags.Reverse) != 0)
			{
				this["-r"] = null;
			}

			if ((flags & MergeFilesCmdFlags.SourceBranch) != 0)
			{
				this["-r"] = null;
			}
		}
	}
	/// <summary>
	/// Merge command options.
	/// </summary>
	public class MergeFilesCmdOptions : Options
	{
		/// <br/><b>p4 help merge</b>
		/// <br/> 
		/// <br/>     merge -- Merge one set of files into another 
		/// <br/> 
		/// <br/>     p4 merge [options] fromFile[revRange] toFile
		/// <br/>     p4 merge [options] -b branch [-r] [toFile[revRange] ...]
		/// <br/>     p4 merge [options] -b branch -s fromFile[revRange] [toFile ...]
		/// <br/>     p4 merge [options] -S stream [-P parent] [-F] [-r] [toFile[revRange] ...]
		/// <br/> 
		/// <br/> 	options: -c changelist# -n -m max
		/// <br/> 
		/// <br/> 	'p4 merge' merges changes from one set of files (the 'source') into 
		/// <br/> 	another (the 'target'). It is a simplified form of the 'p4 integrate'
		/// <br/> 	command.
		/// <br/> 
		/// <br/> 	Using the client workspace as a staging area, 'p4 merge' branches and
		/// <br/> 	deletes target files per changes in the source, and schedules all 
		/// <br/> 	other affected target files to be resolved.  Target files outside of
		/// <br/> 	the current client view are not affected. Source files need not be
		/// <br/> 	within the client view.
		/// <br/> 
		/// <br/> 	'p4 resolve' must be used to merge file content, and to resolve 
		/// <br/> 	filename and filetype changes. 'p4 submit' commits merged files to
		/// <br/> 	the depot.  Unresolved files may not be submitted.  Merged files can
		/// <br/> 	be shelved with 'p4 shelve' and abandoned with 'p4 revert'.  The 
		/// <br/> 	commands 'p4 integrated' and 'p4 filelog' display merge history.
		/// <br/> 
		/// <br/> 	When 'p4 merge' schedules a workspace file to be resolved, it leaves
		/// <br/> 	it read-only. 'p4 resolve' can operate on a read-only file;  for 
		/// <br/> 	other pre-submit changes, 'p4 edit' must be used to make the file 
		/// <br/> 	writable.
		/// <br/> 
		/// <br/> 	Source and target files can be specified either on the 'p4 merge'
		/// <br/> 	command line or through a branch view. On the command line, fromFile
		/// <br/> 	is the source file set and toFile is the target file set.  With a 
		/// <br/> 	branch view, one or more toFile arguments can be given to limit the 
		/// <br/> 	scope of the target file set. 
		/// <br/> 
		/// <br/> 	Each file in the target is mapped to a file in the source. Mapping 
		/// <br/> 	adjusts automatically for files that have been moved or renamed, as
		/// <br/> 	long as 'p4 move' was used to move/rename files.  The scope of source
		/// <br/> 	and target file sets must include both old-named and new-named files
		/// <br/> 	for mappings to be adjusted.  Moved source files may schedule moves 
		/// <br/> 	to be resolved in target files. 
		/// <br/> 
		/// <br/> 	revRange is a revision or a revision range that limits the span of
		/// <br/> 	source history to be probed for unintegrated revisions.  revRange 
		/// <br/> 	can be used on fromFile, or on toFile, but not on both.  When used
		/// <br/> 	on toFile, it refers to source revisions, not to target revisions.
		/// <br/> 	For details about revision specifiers, see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -S flag makes 'p4 merge' use a stream's branch view.  (See 'p4
		/// <br/> 	help stream'.) The source is the stream itself, and the target is
		/// <br/> 	the stream's parent. With -r, the direction is reversed.  -P can be
		/// <br/> 	used to specify a parent stream other than the stream's actual parent.
		/// <br/> 	Note that to submit merged stream files, the current client must
		/// <br/> 	be dedicated to the target stream. (See 'p4 help client'.)
		/// <br/> 
		/// <br/> 	The -F flag can be used with -S to force merging even though the
		/// <br/> 	stream does not expect a merge to occur in the direction indicated.
		/// <br/> 	Normally 'p4 merge' enforces the expected flow of change dictated
		/// <br/> 	by the stream's spec. The 'p4 istat' command summarizes a stream's
		/// <br/> 	expected flow of change.
		/// <br/> 
		/// <br/> 	The -b flag makes 'p4 merge' use a user-defined branch view.  (See
		/// <br/> 	'p4 help branch'.) The source is the left side of the branch view
		/// <br/> 	and the target is the right side. With -r, the direction is reversed.
		/// <br/> 
		/// <br/> 	The -s flag can be used with -b to cause fromFile to be treated as
		/// <br/> 	the source, and both sides of the branch view to be treated as the
		/// <br/> 	target, per the branch view mapping.  Optional toFile arguments may
		/// <br/> 	be given to further restrict the scope of the target file set.  The
		/// <br/> 	-r flag is ignored when -s is used.
		/// <br/> 
		/// <br/> 

		public MergeFilesCmdOptions(MergeFilesCmdFlags flags, string branchName, string streamName, string parentStream, int changelistId, int maxItems)
			: base(flags, branchName, streamName, parentStream, changelistId, maxItems) { }
	}

	/// <summary>
	/// Flags for the fix command.
	/// </summary>
	[Flags]
	public enum FixJobsCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -d flag deletes the specified fixes.  This operation does not
		/// 	otherwise affect the specified changelist or jobs.
		/// </summary>
		Delete = 0x0001
	}
	public partial class Options
	{
		/// <summary>
		/// Fix command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changelistId"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help fix</b>
		/// <br/> 
		/// <br/>     fix -- Mark jobs as being fixed by the specified changelist
		/// <br/> 
		/// <br/>     p4 fix [-d] [-s status] -c changelist# jobName ...
		/// <br/> 
		/// <br/> 	'p4 fix' marks each named job as being fixed by the changelist
		/// <br/> 	number specified with -c.  The changelist can be pending or
		/// <br/> 	submitted and the jobs can open or closed (fixed by another
		/// <br/> 	changelist).
		/// <br/> 
		/// <br/> 	If the changelist has already been submitted and the job is still
		/// <br/> 	open, then 'p4 fix' marks the job closed.  If the changelist has not
		/// <br/> 	been submitted and the job is still open, the job is closed when the
		/// <br/> 	changelist is submitted.  If the job is already closed, it remains
		/// <br/> 	closed.
		/// <br/> 
		/// <br/> 	The -d flag deletes the specified fixes.  This operation does not
		/// <br/> 	otherwise affect the specified changelist or jobs.
		/// <br/> 
		/// <br/> 	The -s flag uses the specified status instead of the default defined
		/// <br/> 	in the job specification.  This status is reported by 'p4 fixes'. 
		/// <br/> 	The 'p4 fix' and 'p4 change' (of a submitted changelist) and 'p4 submit'
		/// <br/> 	(of a pending changelist) commands set the job's status to the fix's
		/// <br/> 	status for each job associated with the change.  If the fix's status
		/// <br/> 	is 'same', the job's status is left unchanged.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(FixJobsCmdFlags flags, int changelistId, string status)
		{
			if ((flags & FixJobsCmdFlags.Delete) != 0)
			{
				this["-d"] = null;
			}

			if (String.IsNullOrEmpty(status) != true)
			{
				this["-s"] = status;
			}

			if (changelistId >= 0)
			{
				this["-c"] = changelistId.ToString();
			}
		}
	}
	/// <summary>
	/// Fix command options.
	/// </summary>
	public class FixJobsCmdOptions : Options
	{
		/// <summary>
		/// Fix command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changelistId"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help fix</b>
		/// <br/> 
		/// <br/>     fix -- Mark jobs as being fixed by the specified changelist
		/// <br/> 
		/// <br/>     p4 fix [-d] [-s status] -c changelist# jobName ...
		/// <br/> 
		/// <br/> 	'p4 fix' marks each named job as being fixed by the changelist
		/// <br/> 	number specified with -c.  The changelist can be pending or
		/// <br/> 	submitted and the jobs can open or closed (fixed by another
		/// <br/> 	changelist).
		/// <br/> 
		/// <br/> 	If the changelist has already been submitted and the job is still
		/// <br/> 	open, then 'p4 fix' marks the job closed.  If the changelist has not
		/// <br/> 	been submitted and the job is still open, the job is closed when the
		/// <br/> 	changelist is submitted.  If the job is already closed, it remains
		/// <br/> 	closed.
		/// <br/> 
		/// <br/> 	The -d flag deletes the specified fixes.  This operation does not
		/// <br/> 	otherwise affect the specified changelist or jobs.
		/// <br/> 
		/// <br/> 	The -s flag uses the specified status instead of the default defined
		/// <br/> 	in the job specification.  This status is reported by 'p4 fixes'. 
		/// <br/> 	The 'p4 fix' and 'p4 change' (of a submitted changelist) and 'p4 submit'
		/// <br/> 	(of a pending changelist) commands set the job's status to the fix's
		/// <br/> 	status for each job associated with the change.  If the fix's status
		/// <br/> 	is 'same', the job's status is left unchanged.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public FixJobsCmdOptions(FixJobsCmdFlags flags, int changelistId, string status)
			: base( flags, changelistId, status)
		{}
	}
	/// <summary>
	/// Flags for the user command.
	/// </summary>
	[Flags]
	public enum UserCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -d flag deletes the specified user (unless the user has files
		/// 	open).
		/// </summary>
		Delete = 0x0001,
		/// <summary>
		/// 	The -o flag writes the user specification to the standard output.
		/// 	The user's editor is not invoked.
		/// </summary>
		Output = 0x0002,
		/// <summary>
		/// 	The -i flag reads a user specification from the standard input.
		/// 	The user's editor is not invoked.
		/// </summary>
		Input = 0x0004,
		/// <summary>
		/// 	The -f flag forces the creation, update or deletion of the specified
		/// 	user, and enables you to change the Last Modified date. By default,
		/// 	users can only delete or modify their own user specifications.  The
		/// 	-f flag requires 'super' access, which is granted by 'p4 protect'.
		/// </summary>
		Force = 0x0008,
	}

	public partial class Options
	{
		/// <summary>
		/// User command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help user</b>
		/// <br/> 
		/// <br/>     user -- Create or edit a user specification
		/// <br/> 
		/// <br/>     p4 user [-f] [name]
		/// <br/>     p4 user -d [-f] name
		/// <br/>     p4 user -o [name]
		/// <br/>     p4 user -i [-f]
		/// <br/> 
		/// <br/> 	Create a new user specification or edit an existing user specification.
		/// <br/> 	The specification form is put into a temporary file and the editor
		/// <br/> 	(configured by the environment variable $P4EDITOR) is invoked.
		/// <br/> 
		/// <br/> 	Normally, a user specification is created automatically the first
		/// <br/> 	time that the user issues any command that updates the depot. The
		/// <br/> 	'p4 user' command is typically used to edit the user's subscription
		/// <br/> 	list for change review.
		/// <br/> 
		/// <br/> 	The user specification form contains the following fields:
		/// <br/> 
		/// <br/> 	User:        The user name (read-only).
		/// <br/> 
		/// <br/> 	Email:       The user's email address (Default: user@client).
		/// <br/> 
		/// <br/> 	Update:      The date the specification was last modified (read-only).
		/// <br/> 
		/// <br/> 	Access:      The date that the user last issued a client command.
		/// <br/> 
		/// <br/> 	FullName:    The user's real name.
		/// <br/> 
		/// <br/> 	JobView:     Selects jobs that are displayed when the user creates
		/// <br/> 		     a changelist. These jobs can be closed automatically
		/// <br/> 		     when the user submits the changelist. For a description
		/// <br/> 		     of jobview syntax, see 'p4 help jobview'
		/// <br/> 
		/// <br/> 	Reviews:     The subscription list for change review.  There is no
		/// <br/> 	             limit on the number of lines that this field can contain.
		/// <br/> 		     You can include the following wildcards:
		/// <br/> 
		/// <br/> 			 ...            matches any characters including /
		/// <br/> 			 *              matches any character except /
		/// <br/> 
		/// <br/> 	Password:    The user's password.  See 'p4 help passwd'.
		/// <br/> 
		/// <br/> 	Type:        Must be 'service', operator, or 'standard'. Default is
		/// <br/> 		     'standard'. Once set, the user type cannot be changed.
		/// <br/> 
		/// <br/> 	The -d flag deletes the specified user (unless the user has files
		/// <br/> 	open).
		/// <br/> 
		/// <br/> 	The -o flag writes the user specification to the standard output.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads a user specification from the standard input.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -f flag forces the creation, update or deletion of the specified
		/// <br/> 	user, and enables you to change the Last Modified date. By default,
		/// <br/> 	users can only delete or modify their own user specifications.  The
		/// <br/> 	-f flag requires 'super' access, which is granted by 'p4 protect'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(UserCmdFlags flags)
		{
			if ((flags & UserCmdFlags.Output) != 0)
			{
				this["-o"] = null;
			}

			if ((flags & UserCmdFlags.Input) != 0)
			{
				this["-i"] = null;
			}

			if ((flags & UserCmdFlags.Delete) != 0)
			{
				this["-d"] = null;
			}

			if ((flags & UserCmdFlags.Force) != 0)
			{
				this["-f"] = null;
			}
		}
	}
	/// <summary>
	/// User command options.
	/// </summary>
	public class UserCmdOptions : Options
	{
		/// <summary>
		/// User command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help user</b>
		/// <br/> 
		/// <br/>     user -- Create or edit a user specification
		/// <br/> 
		/// <br/>     p4 user [-f] [name]
		/// <br/>     p4 user -d [-f] name
		/// <br/>     p4 user -o [name]
		/// <br/>     p4 user -i [-f]
		/// <br/> 
		/// <br/> 	Create a new user specification or edit an existing user specification.
		/// <br/> 	The specification form is put into a temporary file and the editor
		/// <br/> 	(configured by the environment variable $P4EDITOR) is invoked.
		/// <br/> 
		/// <br/> 	Normally, a user specification is created automatically the first
		/// <br/> 	time that the user issues any command that updates the depot. The
		/// <br/> 	'p4 user' command is typically used to edit the user's subscription
		/// <br/> 	list for change review.
		/// <br/> 
		/// <br/> 	The user specification form contains the following fields:
		/// <br/> 
		/// <br/> 	User:        The user name (read-only).
		/// <br/> 
		/// <br/> 	Email:       The user's email address (Default: user@client).
		/// <br/> 
		/// <br/> 	Update:      The date the specification was last modified (read-only).
		/// <br/> 
		/// <br/> 	Access:      The date that the user last issued a client command.
		/// <br/> 
		/// <br/> 	FullName:    The user's real name.
		/// <br/> 
		/// <br/> 	JobView:     Selects jobs that are displayed when the user creates
		/// <br/> 		     a changelist. These jobs can be closed automatically
		/// <br/> 		     when the user submits the changelist. For a description
		/// <br/> 		     of jobview syntax, see 'p4 help jobview'
		/// <br/> 
		/// <br/> 	Reviews:     The subscription list for change review.  There is no
		/// <br/> 	             limit on the number of lines that this field can contain.
		/// <br/> 		     You can include the following wildcards:
		/// <br/> 
		/// <br/> 			 ...            matches any characters including /
		/// <br/> 			 *              matches any character except /
		/// <br/> 
		/// <br/> 	Password:    The user's password.  See 'p4 help passwd'.
		/// <br/> 
		/// <br/> 	Type:        Must be 'service', operator, or 'standard'. Default is
		/// <br/> 		     'standard'. Once set, the user type cannot be changed.
		/// <br/> 
		/// <br/> 	The -d flag deletes the specified user (unless the user has files
		/// <br/> 	open).
		/// <br/> 
		/// <br/> 	The -o flag writes the user specification to the standard output.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads a user specification from the standard input.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -f flag forces the creation, update or deletion of the specified
		/// <br/> 	user, and enables you to change the Last Modified date. By default,
		/// <br/> 	users can only delete or modify their own user specifications.  The
		/// <br/> 	-f flag requires 'super' access, which is granted by 'p4 protect'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public UserCmdOptions(UserCmdFlags flags)
			: base(flags) { }
	}
	/// <summary>
	/// Flags for the users command.
	/// </summary>
	[Flags]
	public enum UsersCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -a flag includes service and operator users in the output.
		/// </summary>	
		IncludeAll = 0x0001,
		/// <summary>
		/// 	The -l flag includes additional information in the output.  The -l
		/// 	flag requires 'super' access, which is granted by 'p4 protect'.
		/// </summary>
		LongForm = 0x0002
	}

	public partial class Options
	{
		/// <summary>
		/// Users command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="maxItems"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help users</b>
		/// <br/> 
		/// <br/>     users -- List Perforce users
		/// <br/> 
		/// <br/>     p4 users [-l -a -r -c] [-m max] [user ...]
		/// <br/> 
		/// <br/> 	Lists all Perforce users or users that match the 'user' argument.
		/// <br/> 	The report includes the last time that each user accessed the system.
		/// <br/> 
		/// <br/> 	The -m max flag limits output to the first 'max' number of users.
		/// <br/> 
		/// <br/> 	The -a flag includes service and operator users in the output.
		/// <br/> 
		/// <br/> 	The -l flag includes additional information in the output.  The -l
		/// <br/> 	flag requires 'super' access, which is granted by 'p4 protect'.
		/// <br/> 
		/// <br/> 	The -r and -c flags are only allowed on replica servers.  When
		/// <br/> 	-r is given only users who have used a replica are reported and
		/// <br/> 	when -c is given only the user information from the central server
		/// <br/> 	is reported.  Otherwise on a replica server, the user list will
		/// <br/> 	be slightly different from the master server as the user access times
		/// <br/> 	will reflect replica usage or master usage whichever is newer.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(UsersCmdFlags flags, int maxItems)
		{
			if ((flags & UsersCmdFlags.IncludeAll) != 0)
			{
				this["-a"] = null;
			}

			if ((flags & UsersCmdFlags.LongForm) != 0)
			{
				this["-l"] = null;
			}

			if (maxItems >= 0)
			{
				this["-m"] = maxItems.ToString();
			}
		}
	}
	/// <summary>
	/// Users command options
	/// </summary>
	public class UsersCmdOptions : Options
	{
		/// <summary>
		/// Users command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="maxItems"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help users</b>
		/// <br/> 
		/// <br/>     users -- List Perforce users
		/// <br/> 
		/// <br/>     p4 users [-l -a -r -c] [-m max] [user ...]
		/// <br/> 
		/// <br/> 	Lists all Perforce users or users that match the 'user' argument.
		/// <br/> 	The report includes the last time that each user accessed the system.
		/// <br/> 
		/// <br/> 	The -m max flag limits output to the first 'max' number of users.
		/// <br/> 
		/// <br/> 	The -a flag includes service and operator users in the output.
		/// <br/> 
		/// <br/> 	The -l flag includes additional information in the output.  The -l
		/// <br/> 	flag requires 'super' access, which is granted by 'p4 protect'.
		/// <br/> 
		/// <br/> 	The -r and -c flags are only allowed on replica servers.  When
		/// <br/> 	-r is given only users who have used a replica are reported and
		/// <br/> 	when -c is given only the user information from the central server
		/// <br/> 	is reported.  Otherwise on a replica server, the user list will
		/// <br/> 	be slightly different from the master server as the user access times
		/// <br/> 	will reflect replica usage or master usage whichever is newer.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public UsersCmdOptions(UsersCmdFlags flags, int maxItems)
			: base(flags, maxItems) { }
	}

	/// <summary>
	/// Flags for the client command.
	/// </summary>
	[Flags]
	public enum ClientCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -d flag deletes the specified spec, as long as the client
		/// 	workspace has no opened files or pending changes.  (See 'p4 help
		/// 	opened'.) The -f flag forces the delete.
		/// </summary>
		Delete = 0x0001,
		/// <summary>
		/// 	The -o flag writes the named client spec to the standard output.
		/// 	The user's editor is not invoked.
		/// </summary>
		Output = 0x0002,
		/// <summary>
		/// 	The -i flag reads a client spec from the standard input.  The
		/// 	user's editor is not invoked.
		/// </summary>
		Input = 0x0004,
		/// <summary>
		/// 	The -f flag can force the updating of locked clients; normally
		/// 	locked clients can only be modified by their owner.  -f also allows
		/// 	the last modified date to be set.  The -f flag requires 'admin'
		/// 	access granted by 'p4 protect'.
		/// </summary>
		Force = 0x0008,
		/// <summary>
		/// 	The -s flag is used to switch an existing client spec's view without
		///     invoking the editor.  It can be used with -S to switch to a stream
		///     view, or with -t to switch to a view defined in another client spec.
		///     Switching views is not allowed in a client that has opened files.
		///     The -f flag can be used with -s to force switching with opened files.
		///     View switching has no effect on files in a client workspace until
		///     'p4 sync' is run.
		/// </summary>
		Switch = 0x0010
	}

	public partial class Options
	{
		/// <summary>
		/// Client command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help client</b>
		/// <br/> 
		/// <br/>     client -- Create or edit a client workspace specification and its view
		/// <br/>     workspace -- Synonym for 'client'
		/// <br/> 
		/// <br/>     p4 client [-f -t template] [name]
		/// <br/>     p4 client -d [-f] name
		/// <br/>     p4 client -o [-t template] [name]
		/// <br/>     p4 client -S stream [[-c change] -o] [name]
		/// <br/>     p4 client -s [-f] -S stream [name]
		/// <br/>     p4 client -s [-f] -t template [name]
		/// <br/>     p4 client -i [-f]
		/// <br/> 
		/// <br/> 	Creates a new client specification ('spec') or edits an existing
		/// <br/> 	spec.  A client spec is a named mapping of depot files to workspace
		/// <br/> 	files.
		/// <br/> 
		/// <br/> 	The 'p4 client' command puts the client spec into a temporary file
		/// <br/> 	and invokes the editor configured by the environment variable
		/// <br/> 	$P4EDITOR.  For new workspaces, the client name defaults to the
		/// <br/> 	$P4CLIENT environment variable, if set, or to the current host name.
		/// <br/> 	Saving the file creates or modifies the client spec.
		/// <br/> 
		/// <br/> 	The client spec contains the following fields:
		/// <br/> 
		/// <br/> 	Client:      The client name.
		/// <br/> 
		/// <br/> 	Host:        If set, restricts access to the named host.
		/// <br/> 		     If unset, access is allowed from any host.
		/// <br/> 
		/// <br/> 	Owner:       The user who created this client.
		/// <br/> 
		/// <br/> 	Update:      The date that this spec was last modified.
		/// <br/> 
		/// <br/> 	Access:      The date that this client was last used in any way.
		/// <br/> 
		/// <br/> 	Description: A short description of the workspace.
		/// <br/> 
		/// <br/> 	Root:        The root directory of the workspace (specified in local
		/// <br/> 		     file system syntax), under which all versioned files
		/// <br/> 		     will be placed. If you change this setting, you must
		/// <br/> 		     physically relocate any files that currently reside
		/// <br/> 		     there.  On Windows client machines, you can specify the
		/// <br/> 		     root as "null" to enable you to map files to multiple
		/// <br/> 		     drives.
		/// <br/> 
		/// <br/> 	AltRoots:    Up to two optional alternate client workspace roots.
		/// <br/> 		     The first of the main and alternate roots to match the
		/// <br/> 		     client program's current working directory is used. If
		/// <br/> 		     none match, the main root is used. 'p4 info' displays
		/// <br/> 		     the root that is being used.
		/// <br/> 
		/// <br/> 	Options:     Flags to configure the client behavior. Defaults
		/// <br/> 		     are marked with *.
		/// <br/> 
		/// <br/> 		allwrite	Leaves all files writable on the client;
		/// <br/> 		noallwrite *	by default, only files opened by 'p4 edit'
		/// <br/> 				are writable. If set, files might be clobbered
		/// <br/> 				as a result of ignoring the clobber option
		/// <br/> 				(see below).
		/// <br/> 
		/// <br/> 		clobber		Permits 'p4 sync' to overwrite writable
		/// <br/> 		noclobber *	files on the client.  noclobber is ignored if
		/// <br/> 				allwrite is set.
		/// <br/> 
		/// <br/> 		compress 	Compresses data sent between the client
		/// <br/> 		nocompress *	and server to speed up slow connections.
		/// <br/> 
		/// <br/> 		locked   	Allows only the client owner to use or change
		/// <br/> 		unlocked *	the client spec.  Prevents the client spec from
		/// <br/> 				being deleted.
		/// <br/> 
		/// <br/> 		modtime  	Causes 'p4 sync' and 'p4 submit' to preserve
		/// <br/> 		nomodtime *	file modification time, as with files with the
		/// <br/> 				+m type modifier. (See 'p4 help filetypes'.)
		/// <br/> 				With nomodtime, file timestamps are updated by
		/// <br/> 				sync and submit operations.
		/// <br/> 
		/// <br/> 		rmdir		Makes 'p4 sync' attempt to delete a workspace
		/// <br/> 		normdir *	directory when all files in it are removed.
		/// <br/> 
		/// <br/> 	SubmitOptions:  Flags to change submit behaviour.
		/// <br/> 
		/// <br/> 		submitunchanged     All open files are submitted (default).
		/// <br/> 
		/// <br/> 		revertunchanged     Files that have content or type changes
		/// <br/> 		                    are submitted. Unchanged files are
		/// <br/> 		                    reverted.
		/// <br/> 
		/// <br/> 		leaveunchanged      Files that have content or type changes
		/// <br/> 		                    are submitted. Unchanged files are moved
		/// <br/> 		                    to the default changelist.
		/// <br/> 
		/// <br/> 		        +reopen     Can be appended to the submit option flag
		/// <br/> 		                    to cause submitted files to be reopened in
		/// <br/> 		                    the default changelist.
		/// <br/> 		                    Example: submitunchanged+reopen
		/// <br/> 
		/// <br/> 	LineEnd:    Set line-ending character(s) for client text files.
		/// <br/> 
		/// <br/> 		local		mode that is native to the client (default).
		/// <br/> 		unix		linefeed: UNIX style.
		/// <br/> 		mac		carriage return: Macintosh style.
		/// <br/> 		win		carriage return-linefeed: Windows style.
		/// <br/> 		share		hybrid: writes UNIX style but reads UNIX,
		/// <br/> 				Mac or Windows style.
		/// <br/> 
		/// <br/> 	View:        Maps files in the depot to files in your client
		/// <br/> 		     workspace.  Defines the files that you want in your
		/// <br/> 		     client workspace and specifies where you want them
		/// <br/> 		     to reside.  The default view maps all depot files
		/// <br/> 		     onto the client.  See 'p4 help views' for view syntax.
		/// <br/> 		     A new view takes effect on the next 'p4 sync'.
		/// <br/> 
		/// <br/> 	Stream:      The stream to which this client's view will be dedicated.
		/// <br/> 		     (Files in stream paths can be submitted only by dedicated
		/// <br/> 		     stream clients.) When this optional field is set, the
		/// <br/> 		     View field will be automatically replaced by a stream
		/// <br/> 		     view as the client spec is saved.
		/// <br/> 
		/// <br/> 	Note: changing the client root does not actually move the client
		/// <br/> 	files; you must relocate them manually.  Similarly, changing
		/// <br/> 	the 'LineEnd' option does not actually update the client files;
		/// <br/> 	you can refresh them with 'p4 sync -f'.
		/// <br/> 
		/// <br/> 	The -d flag deletes the specified spec, as long as the client
		/// <br/> 	workspace has no opened files or pending changes.  (See 'p4 help
		/// <br/> 	opened'.) The -f flag forces the delete.
		/// <br/> 
		/// <br/> 	The -o flag writes the named client spec to the standard output.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads a client spec from the standard input.  The
		/// <br/> 	user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -t template flag, where 'template' is the name of another client
		/// <br/> 	spec, causes the View and Options fields to be replaced by those of
		/// <br/> 	the template.
		/// <br/> 
		/// <br/> 	The -f flag can force the updating of locked clients; normally
		/// <br/> 	locked clients can only be modified by their owner.  -f also allows
		/// <br/> 	the last modified date to be set.  The -f flag requires 'admin'
		/// <br/> 	access granted by 'p4 protect'.
		/// <br/> 
		/// <br/> 	The -s flag is used to switch an existing client spec's view without
		/// <br/> 	invoking the editor.  It can be used with -S to switch to a stream
		/// <br/> 	view, or with -t to switch to a view defined in another client spec.
		/// <br/> 	Switching views is not allowed in a client that has opened files.
		/// <br/> 	The -f flag can be used with -s to force switching with opened files.
		/// <br/> 	View switching has no effect on files in a client workspace until
		/// <br/> 	'p4 sync' is run.
		/// <br/> 
		/// <br/> 	Without -s, the '-S stream' flag can be used to create a new client
		/// <br/> 	spec dedicated to a stream. If the client spec already exists, and
		/// <br/> 	-S is used without -s, it is ignored.
		/// <br/> 
		/// <br/> 	The '-S stream' flag can be used with '-o -c change' to inspect an
		/// <br/> 	old stream client view. It yields the client spec that would have
		/// <br/> 	been created for the stream at the moment the change was recorded.
		/// <br/> 
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(ClientCmdFlags flags)
			: this(flags, null, null, -1) { }
		/// <summary>
		/// Client command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="template"></param>
		/// <param name="stream"></param>
		/// <param name="change"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help client</b>
		/// <br/> 
		/// <br/>     client -- Create or edit a client workspace specification and its view
		/// <br/>     workspace -- Synonym for 'client'
		/// <br/> 
		/// <br/>     p4 client [-f -t template] [name]
		/// <br/>     p4 client -d [-f] name
		/// <br/>     p4 client -o [-t template] [name]
		/// <br/>     p4 client -S stream [[-c change] -o] [name]
		/// <br/>     p4 client -s [-f] -S stream [name]
		/// <br/>     p4 client -s [-f] -t template [name]
		/// <br/>     p4 client -i [-f]
		/// <br/> 
		/// <br/> 	Creates a new client specification ('spec') or edits an existing
		/// <br/> 	spec.  A client spec is a named mapping of depot files to workspace
		/// <br/> 	files.
		/// <br/> 
		/// <br/> 	The 'p4 client' command puts the client spec into a temporary file
		/// <br/> 	and invokes the editor configured by the environment variable
		/// <br/> 	$P4EDITOR.  For new workspaces, the client name defaults to the
		/// <br/> 	$P4CLIENT environment variable, if set, or to the current host name.
		/// <br/> 	Saving the file creates or modifies the client spec.
		/// <br/> 
		/// <br/> 	The client spec contains the following fields:
		/// <br/> 
		/// <br/> 	Client:      The client name.
		/// <br/> 
		/// <br/> 	Host:        If set, restricts access to the named host.
		/// <br/> 		     If unset, access is allowed from any host.
		/// <br/> 
		/// <br/> 	Owner:       The user who created this client.
		/// <br/> 
		/// <br/> 	Update:      The date that this spec was last modified.
		/// <br/> 
		/// <br/> 	Access:      The date that this client was last used in any way.
		/// <br/> 
		/// <br/> 	Description: A short description of the workspace.
		/// <br/> 
		/// <br/> 	Root:        The root directory of the workspace (specified in local
		/// <br/> 		     file system syntax), under which all versioned files
		/// <br/> 		     will be placed. If you change this setting, you must
		/// <br/> 		     physically relocate any files that currently reside
		/// <br/> 		     there.  On Windows client machines, you can specify the
		/// <br/> 		     root as "null" to enable you to map files to multiple
		/// <br/> 		     drives.
		/// <br/> 
		/// <br/> 	AltRoots:    Up to two optional alternate client workspace roots.
		/// <br/> 		     The first of the main and alternate roots to match the
		/// <br/> 		     client program's current working directory is used. If
		/// <br/> 		     none match, the main root is used. 'p4 info' displays
		/// <br/> 		     the root that is being used.
		/// <br/> 
		/// <br/> 	Options:     Flags to configure the client behavior. Defaults
		/// <br/> 		     are marked with *.
		/// <br/> 
		/// <br/> 		allwrite	Leaves all files writable on the client;
		/// <br/> 		noallwrite *	by default, only files opened by 'p4 edit'
		/// <br/> 				are writable. If set, files might be clobbered
		/// <br/> 				as a result of ignoring the clobber option
		/// <br/> 				(see below).
		/// <br/> 
		/// <br/> 		clobber		Permits 'p4 sync' to overwrite writable
		/// <br/> 		noclobber *	files on the client.  noclobber is ignored if
		/// <br/> 				allwrite is set.
		/// <br/> 
		/// <br/> 		compress 	Compresses data sent between the client
		/// <br/> 		nocompress *	and server to speed up slow connections.
		/// <br/> 
		/// <br/> 		locked   	Allows only the client owner to use or change
		/// <br/> 		unlocked *	the client spec.  Prevents the client spec from
		/// <br/> 				being deleted.
		/// <br/> 
		/// <br/> 		modtime  	Causes 'p4 sync' and 'p4 submit' to preserve
		/// <br/> 		nomodtime *	file modification time, as with files with the
		/// <br/> 				+m type modifier. (See 'p4 help filetypes'.)
		/// <br/> 				With nomodtime, file timestamps are updated by
		/// <br/> 				sync and submit operations.
		/// <br/> 
		/// <br/> 		rmdir		Makes 'p4 sync' attempt to delete a workspace
		/// <br/> 		normdir *	directory when all files in it are removed.
		/// <br/> 
		/// <br/> 	SubmitOptions:  Flags to change submit behaviour.
		/// <br/> 
		/// <br/> 		submitunchanged     All open files are submitted (default).
		/// <br/> 
		/// <br/> 		revertunchanged     Files that have content or type changes
		/// <br/> 		                    are submitted. Unchanged files are
		/// <br/> 		                    reverted.
		/// <br/> 
		/// <br/> 		leaveunchanged      Files that have content or type changes
		/// <br/> 		                    are submitted. Unchanged files are moved
		/// <br/> 		                    to the default changelist.
		/// <br/> 
		/// <br/> 		        +reopen     Can be appended to the submit option flag
		/// <br/> 		                    to cause submitted files to be reopened in
		/// <br/> 		                    the default changelist.
		/// <br/> 		                    Example: submitunchanged+reopen
		/// <br/> 
		/// <br/> 	LineEnd:    Set line-ending character(s) for client text files.
		/// <br/> 
		/// <br/> 		local		mode that is native to the client (default).
		/// <br/> 		unix		linefeed: UNIX style.
		/// <br/> 		mac		carriage return: Macintosh style.
		/// <br/> 		win		carriage return-linefeed: Windows style.
		/// <br/> 		share		hybrid: writes UNIX style but reads UNIX,
		/// <br/> 				Mac or Windows style.
		/// <br/> 
		/// <br/> 	View:        Maps files in the depot to files in your client
		/// <br/> 		     workspace.  Defines the files that you want in your
		/// <br/> 		     client workspace and specifies where you want them
		/// <br/> 		     to reside.  The default view maps all depot files
		/// <br/> 		     onto the client.  See 'p4 help views' for view syntax.
		/// <br/> 		     A new view takes effect on the next 'p4 sync'.
		/// <br/> 
		/// <br/> 	Stream:      The stream to which this client's view will be dedicated.
		/// <br/> 		     (Files in stream paths can be submitted only by dedicated
		/// <br/> 		     stream clients.) When this optional field is set, the
		/// <br/> 		     View field will be automatically replaced by a stream
		/// <br/> 		     view as the client spec is saved.
		/// <br/> 
		/// <br/> 	Note: changing the client root does not actually move the client
		/// <br/> 	files; you must relocate them manually.  Similarly, changing
		/// <br/> 	the 'LineEnd' option does not actually update the client files;
		/// <br/> 	you can refresh them with 'p4 sync -f'.
		/// <br/> 
		/// <br/> 	The -d flag deletes the specified spec, as long as the client
		/// <br/> 	workspace has no opened files or pending changes.  (See 'p4 help
		/// <br/> 	opened'.) The -f flag forces the delete.
		/// <br/> 
		/// <br/> 	The -o flag writes the named client spec to the standard output.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads a client spec from the standard input.  The
		/// <br/> 	user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -t template flag, where 'template' is the name of another client
		/// <br/> 	spec, causes the View and Options fields to be replaced by those of
		/// <br/> 	the template.
		/// <br/> 
		/// <br/> 	The -f flag can force the updating of locked clients; normally
		/// <br/> 	locked clients can only be modified by their owner.  -f also allows
		/// <br/> 	the last modified date to be set.  The -f flag requires 'admin'
		/// <br/> 	access granted by 'p4 protect'.
		/// <br/> 
		/// <br/> 	The -s flag is used to switch an existing client spec's view without
		/// <br/> 	invoking the editor.  It can be used with -S to switch to a stream
		/// <br/> 	view, or with -t to switch to a view defined in another client spec.
		/// <br/> 	Switching views is not allowed in a client that has opened files.
		/// <br/> 	The -f flag can be used with -s to force switching with opened files.
		/// <br/> 	View switching has no effect on files in a client workspace until
		/// <br/> 	'p4 sync' is run.
		/// <br/> 
		/// <br/> 	Without -s, the '-S stream' flag can be used to create a new client
		/// <br/> 	spec dedicated to a stream. If the client spec already exists, and
		/// <br/> 	-S is used without -s, it is ignored.
		/// <br/> 
		/// <br/> 	The '-S stream' flag can be used with '-o -c change' to inspect an
		/// <br/> 	old stream client view. It yields the client spec that would have
		/// <br/> 	been created for the stream at the moment the change was recorded.
		/// <br/> 
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(ClientCmdFlags flags, string template,
			string stream, int change)
		{
			if ((flags & ClientCmdFlags.Output) != 0)
			{
				this["-o"] = null;
			}

			if ((flags & ClientCmdFlags.Input) != 0)
			{
				this["-i"] = null;
			}

			if ((flags & ClientCmdFlags.Delete) != 0)
			{
				this["-d"] = null;
			}

			if ((flags & ClientCmdFlags.Force) != 0)
			{
				this["-f"] = null;
			}

			if ((flags & ClientCmdFlags.Switch) != 0)
			{
				this["-s"] = null;
			}

			if (String.IsNullOrEmpty(template) != true)
			{
				this["-t"] = template;
			}

			if (String.IsNullOrEmpty(stream) != true)
			{
				this["-S"] = stream;
			}

			if (change > 0)
			{
				this["-c"] = change.ToString();
			}
		}
	}
	///<summary>
	/// Client command options
	/// </summary>
	public class ClientCmdOptions : Options
	{
		/// <summary>
		/// Client command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help client</b>
		/// <br/> 
		/// <br/>     client -- Create or edit a client workspace specification and its view
		/// <br/>     workspace -- Synonym for 'client'
		/// <br/> 
		/// <br/>     p4 client [-f -t template] [name]
		/// <br/>     p4 client -d [-f] name
		/// <br/>     p4 client -o [-t template] [name]
		/// <br/>     p4 client -S stream [[-c change] -o] [name]
		/// <br/>     p4 client -s [-f] -S stream [name]
		/// <br/>     p4 client -s [-f] -t template [name]
		/// <br/>     p4 client -i [-f]
		/// <br/> 
		/// <br/> 	Creates a new client specification ('spec') or edits an existing
		/// <br/> 	spec.  A client spec is a named mapping of depot files to workspace
		/// <br/> 	files.
		/// <br/> 
		/// <br/> 	The 'p4 client' command puts the client spec into a temporary file
		/// <br/> 	and invokes the editor configured by the environment variable
		/// <br/> 	$P4EDITOR.  For new workspaces, the client name defaults to the
		/// <br/> 	$P4CLIENT environment variable, if set, or to the current host name.
		/// <br/> 	Saving the file creates or modifies the client spec.
		/// <br/> 
		/// <br/> 	The client spec contains the following fields:
		/// <br/> 
		/// <br/> 	Client:      The client name.
		/// <br/> 
		/// <br/> 	Host:        If set, restricts access to the named host.
		/// <br/> 		     If unset, access is allowed from any host.
		/// <br/> 
		/// <br/> 	Owner:       The user who created this client.
		/// <br/> 
		/// <br/> 	Update:      The date that this spec was last modified.
		/// <br/> 
		/// <br/> 	Access:      The date that this client was last used in any way.
		/// <br/> 
		/// <br/> 	Description: A short description of the workspace.
		/// <br/> 
		/// <br/> 	Root:        The root directory of the workspace (specified in local
		/// <br/> 		     file system syntax), under which all versioned files
		/// <br/> 		     will be placed. If you change this setting, you must
		/// <br/> 		     physically relocate any files that currently reside
		/// <br/> 		     there.  On Windows client machines, you can specify the
		/// <br/> 		     root as "null" to enable you to map files to multiple
		/// <br/> 		     drives.
		/// <br/> 
		/// <br/> 	AltRoots:    Up to two optional alternate client workspace roots.
		/// <br/> 		     The first of the main and alternate roots to match the
		/// <br/> 		     client program's current working directory is used. If
		/// <br/> 		     none match, the main root is used. 'p4 info' displays
		/// <br/> 		     the root that is being used.
		/// <br/> 
		/// <br/> 	Options:     Flags to configure the client behavior. Defaults
		/// <br/> 		     are marked with *.
		/// <br/> 
		/// <br/> 		allwrite	Leaves all files writable on the client;
		/// <br/> 		noallwrite *	by default, only files opened by 'p4 edit'
		/// <br/> 				are writable. If set, files might be clobbered
		/// <br/> 				as a result of ignoring the clobber option
		/// <br/> 				(see below).
		/// <br/> 
		/// <br/> 		clobber		Permits 'p4 sync' to overwrite writable
		/// <br/> 		noclobber *	files on the client.  noclobber is ignored if
		/// <br/> 				allwrite is set.
		/// <br/> 
		/// <br/> 		compress 	Compresses data sent between the client
		/// <br/> 		nocompress *	and server to speed up slow connections.
		/// <br/> 
		/// <br/> 		locked   	Allows only the client owner to use or change
		/// <br/> 		unlocked *	the client spec.  Prevents the client spec from
		/// <br/> 				being deleted.
		/// <br/> 
		/// <br/> 		modtime  	Causes 'p4 sync' and 'p4 submit' to preserve
		/// <br/> 		nomodtime *	file modification time, as with files with the
		/// <br/> 				+m type modifier. (See 'p4 help filetypes'.)
		/// <br/> 				With nomodtime, file timestamps are updated by
		/// <br/> 				sync and submit operations.
		/// <br/> 
		/// <br/> 		rmdir		Makes 'p4 sync' attempt to delete a workspace
		/// <br/> 		normdir *	directory when all files in it are removed.
		/// <br/> 
		/// <br/> 	SubmitOptions:  Flags to change submit behaviour.
		/// <br/> 
		/// <br/> 		submitunchanged     All open files are submitted (default).
		/// <br/> 
		/// <br/> 		revertunchanged     Files that have content or type changes
		/// <br/> 		                    are submitted. Unchanged files are
		/// <br/> 		                    reverted.
		/// <br/> 
		/// <br/> 		leaveunchanged      Files that have content or type changes
		/// <br/> 		                    are submitted. Unchanged files are moved
		/// <br/> 		                    to the default changelist.
		/// <br/> 
		/// <br/> 		        +reopen     Can be appended to the submit option flag
		/// <br/> 		                    to cause submitted files to be reopened in
		/// <br/> 		                    the default changelist.
		/// <br/> 		                    Example: submitunchanged+reopen
		/// <br/> 
		/// <br/> 	LineEnd:    Set line-ending character(s) for client text files.
		/// <br/> 
		/// <br/> 		local		mode that is native to the client (default).
		/// <br/> 		unix		linefeed: UNIX style.
		/// <br/> 		mac		carriage return: Macintosh style.
		/// <br/> 		win		carriage return-linefeed: Windows style.
		/// <br/> 		share		hybrid: writes UNIX style but reads UNIX,
		/// <br/> 				Mac or Windows style.
		/// <br/> 
		/// <br/> 	View:        Maps files in the depot to files in your client
		/// <br/> 		     workspace.  Defines the files that you want in your
		/// <br/> 		     client workspace and specifies where you want them
		/// <br/> 		     to reside.  The default view maps all depot files
		/// <br/> 		     onto the client.  See 'p4 help views' for view syntax.
		/// <br/> 		     A new view takes effect on the next 'p4 sync'.
		/// <br/> 
		/// <br/> 	Stream:      The stream to which this client's view will be dedicated.
		/// <br/> 		     (Files in stream paths can be submitted only by dedicated
		/// <br/> 		     stream clients.) When this optional field is set, the
		/// <br/> 		     View field will be automatically replaced by a stream
		/// <br/> 		     view as the client spec is saved.
		/// <br/> 
		/// <br/> 	Note: changing the client root does not actually move the client
		/// <br/> 	files; you must relocate them manually.  Similarly, changing
		/// <br/> 	the 'LineEnd' option does not actually update the client files;
		/// <br/> 	you can refresh them with 'p4 sync -f'.
		/// <br/> 
		/// <br/> 	The -d flag deletes the specified spec, as long as the client
		/// <br/> 	workspace has no opened files or pending changes.  (See 'p4 help
		/// <br/> 	opened'.) The -f flag forces the delete.
		/// <br/> 
		/// <br/> 	The -o flag writes the named client spec to the standard output.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads a client spec from the standard input.  The
		/// <br/> 	user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -t template flag, where 'template' is the name of another client
		/// <br/> 	spec, causes the View and Options fields to be replaced by those of
		/// <br/> 	the template.
		/// <br/> 
		/// <br/> 	The -f flag can force the updating of locked clients; normally
		/// <br/> 	locked clients can only be modified by their owner.  -f also allows
		/// <br/> 	the last modified date to be set.  The -f flag requires 'admin'
		/// <br/> 	access granted by 'p4 protect'.
		/// <br/> 
		/// <br/> 	The -s flag is used to switch an existing client spec's view without
		/// <br/> 	invoking the editor.  It can be used with -S to switch to a stream
		/// <br/> 	view, or with -t to switch to a view defined in another client spec.
		/// <br/> 	Switching views is not allowed in a client that has opened files.
		/// <br/> 	The -f flag can be used with -s to force switching with opened files.
		/// <br/> 	View switching has no effect on files in a client workspace until
		/// <br/> 	'p4 sync' is run.
		/// <br/> 
		/// <br/> 	Without -s, the '-S stream' flag can be used to create a new client
		/// <br/> 	spec dedicated to a stream. If the client spec already exists, and
		/// <br/> 	-S is used without -s, it is ignored.
		/// <br/> 
		/// <br/> 	The '-S stream' flag can be used with '-o -c change' to inspect an
		/// <br/> 	old stream client view. It yields the client spec that would have
		/// <br/> 	been created for the stream at the moment the change was recorded.
		/// <br/> 
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public ClientCmdOptions(ClientCmdFlags flags)
			: base(flags, null, null, -1) { }
		/// <summary>
		/// Client command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help client</b>
		/// <br/> 
		/// <br/>     client -- Create or edit a client workspace specification and its view
		/// <br/>     workspace -- Synonym for 'client'
		/// <br/> 
		/// <br/>     p4 client [-f -t template] [name]
		/// <br/>     p4 client -d [-f] name
		/// <br/>     p4 client -o [-t template] [name]
		/// <br/>     p4 client -S stream [[-c change] -o] [name]
		/// <br/>     p4 client -s [-f] -S stream [name]
		/// <br/>     p4 client -s [-f] -t template [name]
		/// <br/>     p4 client -i [-f]
		/// <br/> 
		/// <br/> 	Creates a new client specification ('spec') or edits an existing
		/// <br/> 	spec.  A client spec is a named mapping of depot files to workspace
		/// <br/> 	files.
		/// <br/> 
		/// <br/> 	The 'p4 client' command puts the client spec into a temporary file
		/// <br/> 	and invokes the editor configured by the environment variable
		/// <br/> 	$P4EDITOR.  For new workspaces, the client name defaults to the
		/// <br/> 	$P4CLIENT environment variable, if set, or to the current host name.
		/// <br/> 	Saving the file creates or modifies the client spec.
		/// <br/> 
		/// <br/> 	The client spec contains the following fields:
		/// <br/> 
		/// <br/> 	Client:      The client name.
		/// <br/> 
		/// <br/> 	Host:        If set, restricts access to the named host.
		/// <br/> 		     If unset, access is allowed from any host.
		/// <br/> 
		/// <br/> 	Owner:       The user who created this client.
		/// <br/> 
		/// <br/> 	Update:      The date that this spec was last modified.
		/// <br/> 
		/// <br/> 	Access:      The date that this client was last used in any way.
		/// <br/> 
		/// <br/> 	Description: A short description of the workspace.
		/// <br/> 
		/// <br/> 	Root:        The root directory of the workspace (specified in local
		/// <br/> 		     file system syntax), under which all versioned files
		/// <br/> 		     will be placed. If you change this setting, you must
		/// <br/> 		     physically relocate any files that currently reside
		/// <br/> 		     there.  On Windows client machines, you can specify the
		/// <br/> 		     root as "null" to enable you to map files to multiple
		/// <br/> 		     drives.
		/// <br/> 
		/// <br/> 	AltRoots:    Up to two optional alternate client workspace roots.
		/// <br/> 		     The first of the main and alternate roots to match the
		/// <br/> 		     client program's current working directory is used. If
		/// <br/> 		     none match, the main root is used. 'p4 info' displays
		/// <br/> 		     the root that is being used.
		/// <br/> 
		/// <br/> 	Options:     Flags to configure the client behavior. Defaults
		/// <br/> 		     are marked with *.
		/// <br/> 
		/// <br/> 		allwrite	Leaves all files writable on the client;
		/// <br/> 		noallwrite *	by default, only files opened by 'p4 edit'
		/// <br/> 				are writable. If set, files might be clobbered
		/// <br/> 				as a result of ignoring the clobber option
		/// <br/> 				(see below).
		/// <br/> 
		/// <br/> 		clobber		Permits 'p4 sync' to overwrite writable
		/// <br/> 		noclobber *	files on the client.  noclobber is ignored if
		/// <br/> 				allwrite is set.
		/// <br/> 
		/// <br/> 		compress 	Compresses data sent between the client
		/// <br/> 		nocompress *	and server to speed up slow connections.
		/// <br/> 
		/// <br/> 		locked   	Allows only the client owner to use or change
		/// <br/> 		unlocked *	the client spec.  Prevents the client spec from
		/// <br/> 				being deleted.
		/// <br/> 
		/// <br/> 		modtime  	Causes 'p4 sync' and 'p4 submit' to preserve
		/// <br/> 		nomodtime *	file modification time, as with files with the
		/// <br/> 				+m type modifier. (See 'p4 help filetypes'.)
		/// <br/> 				With nomodtime, file timestamps are updated by
		/// <br/> 				sync and submit operations.
		/// <br/> 
		/// <br/> 		rmdir		Makes 'p4 sync' attempt to delete a workspace
		/// <br/> 		normdir *	directory when all files in it are removed.
		/// <br/> 
		/// <br/> 	SubmitOptions:  Flags to change submit behaviour.
		/// <br/> 
		/// <br/> 		submitunchanged     All open files are submitted (default).
		/// <br/> 
		/// <br/> 		revertunchanged     Files that have content or type changes
		/// <br/> 		                    are submitted. Unchanged files are
		/// <br/> 		                    reverted.
		/// <br/> 
		/// <br/> 		leaveunchanged      Files that have content or type changes
		/// <br/> 		                    are submitted. Unchanged files are moved
		/// <br/> 		                    to the default changelist.
		/// <br/> 
		/// <br/> 		        +reopen     Can be appended to the submit option flag
		/// <br/> 		                    to cause submitted files to be reopened in
		/// <br/> 		                    the default changelist.
		/// <br/> 		                    Example: submitunchanged+reopen
		/// <br/> 
		/// <br/> 	LineEnd:    Set line-ending character(s) for client text files.
		/// <br/> 
		/// <br/> 		local		mode that is native to the client (default).
		/// <br/> 		unix		linefeed: UNIX style.
		/// <br/> 		mac		carriage return: Macintosh style.
		/// <br/> 		win		carriage return-linefeed: Windows style.
		/// <br/> 		share		hybrid: writes UNIX style but reads UNIX,
		/// <br/> 				Mac or Windows style.
		/// <br/> 
		/// <br/> 	View:        Maps files in the depot to files in your client
		/// <br/> 		     workspace.  Defines the files that you want in your
		/// <br/> 		     client workspace and specifies where you want them
		/// <br/> 		     to reside.  The default view maps all depot files
		/// <br/> 		     onto the client.  See 'p4 help views' for view syntax.
		/// <br/> 		     A new view takes effect on the next 'p4 sync'.
		/// <br/> 
		/// <br/> 	Stream:      The stream to which this client's view will be dedicated.
		/// <br/> 		     (Files in stream paths can be submitted only by dedicated
		/// <br/> 		     stream clients.) When this optional field is set, the
		/// <br/> 		     View field will be automatically replaced by a stream
		/// <br/> 		     view as the client spec is saved.
		/// <br/> 
		/// <br/> 	Note: changing the client root does not actually move the client
		/// <br/> 	files; you must relocate them manually.  Similarly, changing
		/// <br/> 	the 'LineEnd' option does not actually update the client files;
		/// <br/> 	you can refresh them with 'p4 sync -f'.
		/// <br/> 
		/// <br/> 	The -d flag deletes the specified spec, as long as the client
		/// <br/> 	workspace has no opened files or pending changes.  (See 'p4 help
		/// <br/> 	opened'.) The -f flag forces the delete.
		/// <br/> 
		/// <br/> 	The -o flag writes the named client spec to the standard output.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads a client spec from the standard input.  The
		/// <br/> 	user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -t template flag, where 'template' is the name of another client
		/// <br/> 	spec, causes the View and Options fields to be replaced by those of
		/// <br/> 	the template.
		/// <br/> 
		/// <br/> 	The -f flag can force the updating of locked clients; normally
		/// <br/> 	locked clients can only be modified by their owner.  -f also allows
		/// <br/> 	the last modified date to be set.  The -f flag requires 'admin'
		/// <br/> 	access granted by 'p4 protect'.
		/// <br/> 
		/// <br/> 	The -s flag is used to switch an existing client spec's view without
		/// <br/> 	invoking the editor.  It can be used with -S to switch to a stream
		/// <br/> 	view, or with -t to switch to a view defined in another client spec.
		/// <br/> 	Switching views is not allowed in a client that has opened files.
		/// <br/> 	The -f flag can be used with -s to force switching with opened files.
		/// <br/> 	View switching has no effect on files in a client workspace until
		/// <br/> 	'p4 sync' is run.
		/// <br/> 
		/// <br/> 	Without -s, the '-S stream' flag can be used to create a new client
		/// <br/> 	spec dedicated to a stream. If the client spec already exists, and
		/// <br/> 	-S is used without -s, it is ignored.
		/// <br/> 
		/// <br/> 	The '-S stream' flag can be used with '-o -c change' to inspect an
		/// <br/> 	old stream client view. It yields the client spec that would have
		/// <br/> 	been created for the stream at the moment the change was recorded.
		/// <br/> 
		/// <br/> 
		/// <br/> 
		/// </remarks>

		public ClientCmdOptions(ClientCmdFlags flags, string template,
			string stream, int change)
			: base(flags, template, stream, change) { }
	}
	/// <summary>
	/// Flags for the clients command.
	/// </summary>
	[Flags]
	public enum ClientsCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -t flag displays the time as well as the date.
		/// </summary>
		IncludeTime = 0x0001,
		/// <summary>
		/// 	The -e nameFilter flag lists workspaces with a name that matches
		/// 	the nameFilter pattern, for example: -e 'svr-dev-rel*'. -E makes
		/// 	the matching case-insensitive.
		/// </summary>
		IgnoreCase = 0x0002
	}

	public partial class Options
	{
		/// <summary>
		/// Clients command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="userName"></param>
		/// <param name="nameFilter"></param>
		/// <param name="maxItems"></param>
		/// <param name="stream"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help clients</b>
		/// <br/> 
		/// <br/>     clients -- Display list of clients
		/// <br/>     workspaces -- synonym for 'clients'
		/// <br/> 
		/// <br/>     p4 clients [-t] [-u user] [[-e|-E] nameFilter -m max] [-S stream]
		/// <br/> 
		/// <br/> 	Lists all client workspaces currently defined in the server.
		/// <br/> 
		/// <br/> 	The -t flag displays the time as well as the date.
		/// <br/> 
		/// <br/> 	The -u user flag lists client workspaces that are owned by the
		/// <br/> 	specified user.
		/// <br/> 
		/// <br/> 	The -e nameFilter flag lists workspaces with a name that matches
		/// <br/> 	the nameFilter pattern, for example: -e 'svr-dev-rel*'. -E makes
		/// <br/> 	the matching case-insensitive.
		/// <br/> 
		/// <br/> 	The -m max flag limits output to the specified number of workspaces.
		/// <br/> 
		/// <br/> 	The -S stream flag limits output to the client workspaces dedicated 
		/// <br/> 	to the stream.
		/// <br/> 
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(ClientsCmdFlags flags, string userName, string nameFilter,
				int maxItems, string stream)
		{
			if ((flags & ClientsCmdFlags.IncludeTime) != 0)
			{
				this["-t"] = null;
			}

			if (String.IsNullOrEmpty(userName) != true)
			{
				this["-u"] = userName;
			}

			if (String.IsNullOrEmpty(nameFilter) != true)
			{
				if ((flags & ClientsCmdFlags.IgnoreCase) != 0)
				{
					this["-E"] = nameFilter;
				}
				else
				{
					this["-e"] = nameFilter;
				}
			}

			if (maxItems >= 0)
			{
				this["-m"] = maxItems.ToString();
			}

			if (String.IsNullOrEmpty(stream) != true)
			{
				this["-S"] = stream;
			}
		}
	}
	///<summary>
	/// Clients command options
	/// </summary>
	public class ClientsCmdOptions : Options
	{
		/// <summary>
		/// Clients command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="userName"></param>
		/// <param name="nameFilter"></param>
		/// <param name="maxItems"></param>
		/// <param name="stream"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help clients</b>
		/// <br/> 
		/// <br/>     clients -- Display list of clients
		/// <br/>     workspaces -- synonym for 'clients'
		/// <br/> 
		/// <br/>     p4 clients [-t] [-u user] [[-e|-E] nameFilter -m max] [-S stream]
		/// <br/> 
		/// <br/> 	Lists all client workspaces currently defined in the server.
		/// <br/> 
		/// <br/> 	The -t flag displays the time as well as the date.
		/// <br/> 
		/// <br/> 	The -u user flag lists client workspaces that are owned by the
		/// <br/> 	specified user.
		/// <br/> 
		/// <br/> 	The -e nameFilter flag lists workspaces with a name that matches
		/// <br/> 	the nameFilter pattern, for example: -e 'svr-dev-rel*'. -E makes
		/// <br/> 	the matching case-insensitive.
		/// <br/> 
		/// <br/> 	The -m max flag limits output to the specified number of workspaces.
		/// <br/> 
		/// <br/> 	The -S stream flag limits output to the client workspaces dedicated 
		/// <br/> 	to the stream.
		/// <br/> 
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public ClientsCmdOptions(ClientsCmdFlags flags, string userName, string nameFilter,
				int maxItems, string stream)
			: base(flags, userName, nameFilter, maxItems, stream) { }
	}
	/// <summary>
	/// Flags for the change command.
	/// </summary>
	[Flags]
	public enum ChangeCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -d flag deletes a pending changelist, if it has no opened files
		/// 	and no pending fixes associated with it.  Use 'p4 opened -a' to
		/// 	report on opened files and 'p4 reopen' to move them to another
		/// 	changelist.  Use 'p4 fixes -c changelist#' to report on pending
		/// 	fixes and 'p4 fix -d -c changelist# jobs...' to delete pending
		/// 	fixes. The changelist can be deleted only by the user and client
		/// 	who created it, or by a user with 'admin' privilege using the -f
		/// 	flag.
		/// </summary>
		Delete = 0x0001,
		/// <summary>
		/// 	The -o flag writes the changelist specification to the standard
		/// 	output.  The user's editor is not invoked.
		/// </summary>
		Output = 0x0002,
		/// <summary>
		/// 	The -i flag reads a changelist specification from the standard
		/// 	input.  The user's editor is not invoked.
		/// </summary>
		Input = 0x0004,
		/// <summary>
		/// 	The -f flag forces the update or deletion of other users' pending
		/// 	changelists.  -f can also force the deletion of submitted changelists
		/// 	after they have been emptied of files using 'p4 obliterate'.  By
		/// 	default, submitted changelists cannot be changed.  The -f flag can
		/// 	also force display of the 'Description' field in a restricted
		/// 	changelist. The -f flag requires 'admin' access granted by 'p4
		/// 	protect'.  The -f and -u flags are mutually exclusive.
		/// </summary>
		Force = 0x0008,
		/// <summary>
		/// 	The -u flag can force the update of a submitted change by the owner
		/// 	of the change. Only the Jobs, Type, and Description fields can be
		/// 	changed	using the -u flag. The -f and -u flags cannot be used in
		/// 	the same change command.
		/// </summary>
		Update = 0x0010,
		/// <summary>
		/// 	The -s flag extends the list of jobs to include the fix status
		/// 	for each job.  On new changelists, the fix status begins as the
		/// 	special status 'ignore', which, if left unchanged simply excludes
		/// 	the job from those being fixed.  Otherwise, the fix status, like
		/// 	that applied with 'p4 fix -s', becomes the job's status when
		/// 	the changelist is committed.  Note that this option exists
		/// 	to support integration with external defect trackers.
		/// </summary>
		IncludeJobs = 0x0020
	}

	public partial class Options
	{
		/// <summary>
		/// Change command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help change</b>
		/// <br/> 
		/// <br/>     change -- Create or edit a changelist description
		/// <br/>     changelist -- synonym for 'change'
		/// <br/> 
		/// <br/>     p4 change [-s] [-f | -u] [changelist#]
		/// <br/>     p4 change -d [-f -s] changelist#
		/// <br/>     p4 change -o [-s] [-f] [changelist#]
		/// <br/>     p4 change -i [-s] [-f | -u] 
		/// <br/>     p4 change -t restricted | public [-f | -u] changelist#
		/// <br/> 
		/// <br/> 	'p4 change' creates and edits changelists and their descriptions.
		/// <br/> 	With no argument, 'p4 change' creates a new changelist.  If a
		/// <br/> 	changelist number is specified, 'p4 change' edits an existing
		/// <br/> 	pending changelist.  In both cases, the changelist specification
		/// <br/> 	is placed into a form and the user's editor is invoked.
		/// <br/> 
		/// <br/> 	The -d flag deletes a pending changelist, if it has no opened files
		/// <br/> 	and no pending fixes associated with it.  Use 'p4 opened -a' to
		/// <br/> 	report on opened files and 'p4 reopen' to move them to another
		/// <br/> 	changelist.  Use 'p4 fixes -c changelist#' to report on pending
		/// <br/> 	fixes and 'p4 fix -d -c changelist# jobs...' to delete pending
		/// <br/> 	fixes. The changelist can be deleted only by the user and client
		/// <br/> 	who created it, or by a user with 'admin' privilege using the -f
		/// <br/> 	flag.
		/// <br/> 
		/// <br/> 	The -o flag writes the changelist specification to the standard
		/// <br/> 	output.  The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads a changelist specification from the standard
		/// <br/> 	input.  The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -f flag forces the update or deletion of other users' pending
		/// <br/> 	changelists.  -f can also force the deletion of submitted changelists
		/// <br/> 	after they have been emptied of files using 'p4 obliterate'.  By
		/// <br/> 	default, submitted changelists cannot be changed.  The -f flag can
		/// <br/> 	also force display of the 'Description' field in a restricted
		/// <br/> 	changelist. The -f flag requires 'admin' access granted by 'p4
		/// <br/> 	protect'.  The -f and -u flags are mutually exclusive.
		/// <br/> 
		/// <br/> 	The -u flag can force the update of a submitted change by the owner
		/// <br/> 	of the change. Only the Jobs, Type, and Description fields can be
		/// <br/> 	changed	using the -u flag. The -f and -u flags cannot be used in
		/// <br/> 	the same change command.
		/// <br/> 
		/// <br/> 	The -s flag extends the list of jobs to include the fix status
		/// <br/> 	for each job.  On new changelists, the fix status begins as the
		/// <br/> 	special status 'ignore', which, if left unchanged simply excludes
		/// <br/> 	the job from those being fixed.  Otherwise, the fix status, like
		/// <br/> 	that applied with 'p4 fix -s', becomes the job's status when
		/// <br/> 	the changelist is committed.  Note that this option exists
		/// <br/> 	to support integration with external defect trackers.
		/// <br/> 
		/// <br/> 	The -t flag changes the 'Type' of the change to 'restricted'
		/// <br/> 	or 'public' without displaying the change form. This option is
		/// <br/> 	useful for running in a trigger or script.
		/// <br/> 
		/// <br/> 	The 'Type' field can be used to hide the change or its description
		/// <br/> 	from users. Valid values for this field are 'public' (default), and
		/// <br/> 	'restricted'. A shelved or committed change that is 'restricted' is
		/// <br/> 	accessible only to users who own the change or have 'list' permission
		/// <br/> 	to at least one file in the change.  A pending (not shelved) change
		/// <br/> 	is accessible to its owner.  Public changes are accessible to all
		/// <br/> 	users. This setting affects the output of the 'p4 change',
		/// <br/> 	'p4 changes', and 'p4 describe' commands.
		/// <br/> 
		/// <br/> 	If a user is not permitted to have access to a restricted change,
		/// <br/> 	The 'Description' text is replaced with a 'no permission' message
		/// <br/> 	(see 'Type' field). Users with admin permission can override the
		/// <br/> 	restriction using the -f flag.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(ChangeCmdFlags flags)
			: this(flags, ChangeListType.None) { }
		/// <summary>
		/// Change command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="newType"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help change</b>
		/// <br/> 
		/// <br/>     change -- Create or edit a changelist description
		/// <br/>     changelist -- synonym for 'change'
		/// <br/> 
		/// <br/>     p4 change [-s] [-f | -u] [changelist#]
		/// <br/>     p4 change -d [-f -s] changelist#
		/// <br/>     p4 change -o [-s] [-f] [changelist#]
		/// <br/>     p4 change -i [-s] [-f | -u] 
		/// <br/>     p4 change -t restricted | public [-f | -u] changelist#
		/// <br/> 
		/// <br/> 	'p4 change' creates and edits changelists and their descriptions.
		/// <br/> 	With no argument, 'p4 change' creates a new changelist.  If a
		/// <br/> 	changelist number is specified, 'p4 change' edits an existing
		/// <br/> 	pending changelist.  In both cases, the changelist specification
		/// <br/> 	is placed into a form and the user's editor is invoked.
		/// <br/> 
		/// <br/> 	The -d flag deletes a pending changelist, if it has no opened files
		/// <br/> 	and no pending fixes associated with it.  Use 'p4 opened -a' to
		/// <br/> 	report on opened files and 'p4 reopen' to move them to another
		/// <br/> 	changelist.  Use 'p4 fixes -c changelist#' to report on pending
		/// <br/> 	fixes and 'p4 fix -d -c changelist# jobs...' to delete pending
		/// <br/> 	fixes. The changelist can be deleted only by the user and client
		/// <br/> 	who created it, or by a user with 'admin' privilege using the -f
		/// <br/> 	flag.
		/// <br/> 
		/// <br/> 	The -o flag writes the changelist specification to the standard
		/// <br/> 	output.  The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads a changelist specification from the standard
		/// <br/> 	input.  The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -f flag forces the update or deletion of other users' pending
		/// <br/> 	changelists.  -f can also force the deletion of submitted changelists
		/// <br/> 	after they have been emptied of files using 'p4 obliterate'.  By
		/// <br/> 	default, submitted changelists cannot be changed.  The -f flag can
		/// <br/> 	also force display of the 'Description' field in a restricted
		/// <br/> 	changelist. The -f flag requires 'admin' access granted by 'p4
		/// <br/> 	protect'.  The -f and -u flags are mutually exclusive.
		/// <br/> 
		/// <br/> 	The -u flag can force the update of a submitted change by the owner
		/// <br/> 	of the change. Only the Jobs, Type, and Description fields can be
		/// <br/> 	changed	using the -u flag. The -f and -u flags cannot be used in
		/// <br/> 	the same change command.
		/// <br/> 
		/// <br/> 	The -s flag extends the list of jobs to include the fix status
		/// <br/> 	for each job.  On new changelists, the fix status begins as the
		/// <br/> 	special status 'ignore', which, if left unchanged simply excludes
		/// <br/> 	the job from those being fixed.  Otherwise, the fix status, like
		/// <br/> 	that applied with 'p4 fix -s', becomes the job's status when
		/// <br/> 	the changelist is committed.  Note that this option exists
		/// <br/> 	to support integration with external defect trackers.
		/// <br/> 
		/// <br/> 	The -t flag changes the 'Type' of the change to 'restricted'
		/// <br/> 	or 'public' without displaying the change form. This option is
		/// <br/> 	useful for running in a trigger or script.
		/// <br/> 
		/// <br/> 	The 'Type' field can be used to hide the change or its description
		/// <br/> 	from users. Valid values for this field are 'public' (default), and
		/// <br/> 	'restricted'. A shelved or committed change that is 'restricted' is
		/// <br/> 	accessible only to users who own the change or have 'list' permission
		/// <br/> 	to at least one file in the change.  A pending (not shelved) change
		/// <br/> 	is accessible to its owner.  Public changes are accessible to all
		/// <br/> 	users. This setting affects the output of the 'p4 change',
		/// <br/> 	'p4 changes', and 'p4 describe' commands.
		/// <br/> 
		/// <br/> 	If a user is not permitted to have access to a restricted change,
		/// <br/> 	The 'Description' text is replaced with a 'no permission' message
		/// <br/> 	(see 'Type' field). Users with admin permission can override the
		/// <br/> 	restriction using the -f flag.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(ChangeCmdFlags flags, ChangeListType newType)
		{
			if ((flags & ChangeCmdFlags.Output) != 0)
			{
				this["-o"] = null;
			}

			if ((flags & ChangeCmdFlags.Input) != 0)
			{
				this["-i"] = null;
			}

			if ((flags & ChangeCmdFlags.Delete) != 0)
			{
				this["-d"] = null;
			}

			if (newType != ChangeListType.None)
			{
				this["-t"] = new StringEnum<ChangeListType>(newType).ToString();
			}

			if ((flags & ChangeCmdFlags.IncludeJobs) != 0)
			{
				this["-s"] = null;
			}

			if ((flags & ChangeCmdFlags.Force) != 0)
			{
				this["-f"] = null;
			}
			else if ((flags & ChangeCmdFlags.Update) != 0)
			{
				this["-u"] = null;
			}
		}
	}

	/// <summary>
	/// Change command options.
	/// </summary>
	public class ChangeCmdOptions : Options
	{
		/// <summary>
		/// Change command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help change</b>
		/// <br/> 
		/// <br/>     change -- Create or edit a changelist description
		/// <br/>     changelist -- synonym for 'change'
		/// <br/> 
		/// <br/>     p4 change [-s] [-f | -u] [changelist#]
		/// <br/>     p4 change -d [-f -s] changelist#
		/// <br/>     p4 change -o [-s] [-f] [changelist#]
		/// <br/>     p4 change -i [-s] [-f | -u] 
		/// <br/>     p4 change -t restricted | public [-f | -u] changelist#
		/// <br/> 
		/// <br/> 	'p4 change' creates and edits changelists and their descriptions.
		/// <br/> 	With no argument, 'p4 change' creates a new changelist.  If a
		/// <br/> 	changelist number is specified, 'p4 change' edits an existing
		/// <br/> 	pending changelist.  In both cases, the changelist specification
		/// <br/> 	is placed into a form and the user's editor is invoked.
		/// <br/> 
		/// <br/> 	The -d flag deletes a pending changelist, if it has no opened files
		/// <br/> 	and no pending fixes associated with it.  Use 'p4 opened -a' to
		/// <br/> 	report on opened files and 'p4 reopen' to move them to another
		/// <br/> 	changelist.  Use 'p4 fixes -c changelist#' to report on pending
		/// <br/> 	fixes and 'p4 fix -d -c changelist# jobs...' to delete pending
		/// <br/> 	fixes. The changelist can be deleted only by the user and client
		/// <br/> 	who created it, or by a user with 'admin' privilege using the -f
		/// <br/> 	flag.
		/// <br/> 
		/// <br/> 	The -o flag writes the changelist specification to the standard
		/// <br/> 	output.  The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads a changelist specification from the standard
		/// <br/> 	input.  The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -f flag forces the update or deletion of other users' pending
		/// <br/> 	changelists.  -f can also force the deletion of submitted changelists
		/// <br/> 	after they have been emptied of files using 'p4 obliterate'.  By
		/// <br/> 	default, submitted changelists cannot be changed.  The -f flag can
		/// <br/> 	also force display of the 'Description' field in a restricted
		/// <br/> 	changelist. The -f flag requires 'admin' access granted by 'p4
		/// <br/> 	protect'.  The -f and -u flags are mutually exclusive.
		/// <br/> 
		/// <br/> 	The -u flag can force the update of a submitted change by the owner
		/// <br/> 	of the change. Only the Jobs, Type, and Description fields can be
		/// <br/> 	changed	using the -u flag. The -f and -u flags cannot be used in
		/// <br/> 	the same change command.
		/// <br/> 
		/// <br/> 	The -s flag extends the list of jobs to include the fix status
		/// <br/> 	for each job.  On new changelists, the fix status begins as the
		/// <br/> 	special status 'ignore', which, if left unchanged simply excludes
		/// <br/> 	the job from those being fixed.  Otherwise, the fix status, like
		/// <br/> 	that applied with 'p4 fix -s', becomes the job's status when
		/// <br/> 	the changelist is committed.  Note that this option exists
		/// <br/> 	to support integration with external defect trackers.
		/// <br/> 
		/// <br/> 	The -t flag changes the 'Type' of the change to 'restricted'
		/// <br/> 	or 'public' without displaying the change form. This option is
		/// <br/> 	useful for running in a trigger or script.
		/// <br/> 
		/// <br/> 	The 'Type' field can be used to hide the change or its description
		/// <br/> 	from users. Valid values for this field are 'public' (default), and
		/// <br/> 	'restricted'. A shelved or committed change that is 'restricted' is
		/// <br/> 	accessible only to users who own the change or have 'list' permission
		/// <br/> 	to at least one file in the change.  A pending (not shelved) change
		/// <br/> 	is accessible to its owner.  Public changes are accessible to all
		/// <br/> 	users. This setting affects the output of the 'p4 change',
		/// <br/> 	'p4 changes', and 'p4 describe' commands.
		/// <br/> 
		/// <br/> 	If a user is not permitted to have access to a restricted change,
		/// <br/> 	The 'Description' text is replaced with a 'no permission' message
		/// <br/> 	(see 'Type' field). Users with admin permission can override the
		/// <br/> 	restriction using the -f flag.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public ChangeCmdOptions(ChangeCmdFlags flags)
			: base(flags, ChangeListType.None) { }
		/// <summary>
		/// Change command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="newType"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help change</b>
		/// <br/> 
		/// <br/>     change -- Create or edit a changelist description
		/// <br/>     changelist -- synonym for 'change'
		/// <br/> 
		/// <br/>     p4 change [-s] [-f | -u] [changelist#]
		/// <br/>     p4 change -d [-f -s] changelist#
		/// <br/>     p4 change -o [-s] [-f] [changelist#]
		/// <br/>     p4 change -i [-s] [-f | -u] 
		/// <br/>     p4 change -t restricted | public [-f | -u] changelist#
		/// <br/> 
		/// <br/> 	'p4 change' creates and edits changelists and their descriptions.
		/// <br/> 	With no argument, 'p4 change' creates a new changelist.  If a
		/// <br/> 	changelist number is specified, 'p4 change' edits an existing
		/// <br/> 	pending changelist.  In both cases, the changelist specification
		/// <br/> 	is placed into a form and the user's editor is invoked.
		/// <br/> 
		/// <br/> 	The -d flag deletes a pending changelist, if it has no opened files
		/// <br/> 	and no pending fixes associated with it.  Use 'p4 opened -a' to
		/// <br/> 	report on opened files and 'p4 reopen' to move them to another
		/// <br/> 	changelist.  Use 'p4 fixes -c changelist#' to report on pending
		/// <br/> 	fixes and 'p4 fix -d -c changelist# jobs...' to delete pending
		/// <br/> 	fixes. The changelist can be deleted only by the user and client
		/// <br/> 	who created it, or by a user with 'admin' privilege using the -f
		/// <br/> 	flag.
		/// <br/> 
		/// <br/> 	The -o flag writes the changelist specification to the standard
		/// <br/> 	output.  The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads a changelist specification from the standard
		/// <br/> 	input.  The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -f flag forces the update or deletion of other users' pending
		/// <br/> 	changelists.  -f can also force the deletion of submitted changelists
		/// <br/> 	after they have been emptied of files using 'p4 obliterate'.  By
		/// <br/> 	default, submitted changelists cannot be changed.  The -f flag can
		/// <br/> 	also force display of the 'Description' field in a restricted
		/// <br/> 	changelist. The -f flag requires 'admin' access granted by 'p4
		/// <br/> 	protect'.  The -f and -u flags are mutually exclusive.
		/// <br/> 
		/// <br/> 	The -u flag can force the update of a submitted change by the owner
		/// <br/> 	of the change. Only the Jobs, Type, and Description fields can be
		/// <br/> 	changed	using the -u flag. The -f and -u flags cannot be used in
		/// <br/> 	the same change command.
		/// <br/> 
		/// <br/> 	The -s flag extends the list of jobs to include the fix status
		/// <br/> 	for each job.  On new changelists, the fix status begins as the
		/// <br/> 	special status 'ignore', which, if left unchanged simply excludes
		/// <br/> 	the job from those being fixed.  Otherwise, the fix status, like
		/// <br/> 	that applied with 'p4 fix -s', becomes the job's status when
		/// <br/> 	the changelist is committed.  Note that this option exists
		/// <br/> 	to support integration with external defect trackers.
		/// <br/> 
		/// <br/> 	The -t flag changes the 'Type' of the change to 'restricted'
		/// <br/> 	or 'public' without displaying the change form. This option is
		/// <br/> 	useful for running in a trigger or script.
		/// <br/> 
		/// <br/> 	The 'Type' field can be used to hide the change or its description
		/// <br/> 	from users. Valid values for this field are 'public' (default), and
		/// <br/> 	'restricted'. A shelved or committed change that is 'restricted' is
		/// <br/> 	accessible only to users who own the change or have 'list' permission
		/// <br/> 	to at least one file in the change.  A pending (not shelved) change
		/// <br/> 	is accessible to its owner.  Public changes are accessible to all
		/// <br/> 	users. This setting affects the output of the 'p4 change',
		/// <br/> 	'p4 changes', and 'p4 describe' commands.
		/// <br/> 
		/// <br/> 	If a user is not permitted to have access to a restricted change,
		/// <br/> 	The 'Description' text is replaced with a 'no permission' message
		/// <br/> 	(see 'Type' field). Users with admin permission can override the
		/// <br/> 	restriction using the -f flag.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public ChangeCmdOptions(ChangeCmdFlags flags, ChangeListType newType)
			:base(flags, newType)
		{}
	}
	/// <summary>
	/// Flags for the changes command.
	/// </summary>
	[Flags]
	public enum ChangesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -i flag also includes any changelists integrated into the
		/// 	specified files.
		/// </summary>
		IncludeIntegrated = 0x0001,
		/// <summary>
		/// 	The -t flag displays the time as well as the date.
		/// </summary>
		IncludeTime = 0x0002,
		/// <summary>
		/// 	The -l flag displays the full text of the changelist
		/// 	descriptions.
		/// </summary>
		FullDescription = 0x0004,
		/// <summary>
		/// 	The -L flag displays the changelist descriptions, truncated to 250
		/// 	characters if longer.
		/// </summary>
		LongDescription = 0x0008,
		/// <summary>
		/// 	The -f flag enables admin users to view restricted changes.
		/// </summary>
		ViewRestricted = 0x0010
	}

	/// <summary>
	/// Flags for the status of a changelist.
	/// </summary>
	[Flags]
	public enum ChangeListStatus
	{
		/// <summary>
		/// No status specified.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// Pending changelist.
		/// </summary>
		Pending = 0x0001,
		/// <summary>
		/// Shelved changelist.
		/// </summary>
		Shelved = 0x0002,
		/// <summary>
		/// Submitted changelist.
		/// </summary>
		Submitted = 0x0004
	}

	public partial class Options
	{
		/// <summary>
		/// Changes command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="clientName"></param>
		/// <param name="maxItems"></param>
		/// <param name="status"></param>
		/// <param name="userName"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help changes</b>
		/// <br/> 
		/// <br/>     changes -- Display list of pending and submitted changelists
		/// <br/>     changelists -- synonym for 'changes'
		/// <br/> 
		/// <br/>     p4 changes [options] [file[revRange] ...]
		/// <br/> 
		/// <br/> 	options: -i -t -l -L -f -c client -m max -s status -u user
		/// <br/> 
		/// <br/> 	Returns a list of all pending and submitted changelists currently
		/// <br/> 	stored in the server.
		/// <br/> 
		/// <br/> 	If files are specified, 'p4 changes' lists only changelists that
		/// <br/> 	affect those files.  If the file specification includes a revision
		/// <br/> 	range, 'p4 changes' lists only submitted changelists that affect
		/// <br/> 	the specified revisions.  See 'p4 help revisions' for details.
		/// <br/> 
		/// <br/> 	If files are not specified, 'p4 changes' limits its report
		/// <br/> 	according to each change's type ('public' or 'restricted').
		/// <br/> 	If a submitted or shelved change is restricted, the change is
		/// <br/> 	not reported unless the user owns the change or has list
		/// <br/> 	permission for at least one file in the change. Only the owner
		/// <br/> 	of a restricted and pending (not shelved) change is permitted
		/// <br/> 	to see it.
		/// <br/> 
		/// <br/> 	The -i flag also includes any changelists integrated into the
		/// <br/> 	specified files.
		/// <br/> 
		/// <br/> 	The -t flag displays the time as well as the date.
		/// <br/> 
		/// <br/> 	The -l flag displays the full text of the changelist
		/// <br/> 	descriptions.
		/// <br/> 
		/// <br/> 	The -L flag displays the changelist descriptions, truncated to 250
		/// <br/> 	characters if longer.
		/// <br/> 
		/// <br/> 	The -f flag enables admin users to view restricted changes.
		/// <br/> 
		/// <br/> 	The -c client flag displays only submitted by the specified client.
		/// <br/> 
		/// <br/> 	The -m max flag limits changes to the 'max' most recent.
		/// <br/> 
		/// <br/> 	The -s status flag limits the output to pending, shelved or
		/// <br/> 	submitted changelists.
		/// <br/> 
		/// <br/> 	The -u user flag displays only changes owned by the specified user.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(ChangesCmdFlags flags, string clientName, int maxItems,
				ChangeListStatus status, string userName)
		{
			if ((flags & ChangesCmdFlags.IncludeIntegrated) != 0)
			{
				this["-i"] = null;
			}

			if ((flags & ChangesCmdFlags.IncludeTime) != 0)
			{
				this["-t"] = null;
			}

			if ((flags & ChangesCmdFlags.FullDescription) != 0)
			{
				this["-l"] = null;
			}

			if ((flags & ChangesCmdFlags.LongDescription) != 0)
			{
				this["-L"] = null;
			}

			if ((flags & ChangesCmdFlags.ViewRestricted) != 0)
			{
				this["-f"] = null;
			}

			if (String.IsNullOrEmpty(clientName) != true)
			{
				this["-c"] = clientName;
			}

			if (maxItems >= 0)
			{
				this["-m"] = maxItems.ToString();
			}

			if (status != ChangeListStatus.None)
			{
				this["-s"] = new StringEnum<ChangeListStatus>(status).ToString(StringEnumCase.Lower); ;
			}

			if (String.IsNullOrEmpty(userName) != true)
			{
				this["-u"] = userName;
			}
		}
	}
	/// <summary>
	/// Changes command options.
	/// </summary>
	public class ChangesCmdOptions : Options
	{
		/// <summary>
		/// Changes command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="clientName"></param>
		/// <param name="maxItems"></param>
		/// <param name="status"></param>
		/// <param name="userName"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help changes</b>
		/// <br/> 
		/// <br/>     changes -- Display list of pending and submitted changelists
		/// <br/>     changelists -- synonym for 'changes'
		/// <br/> 
		/// <br/>     p4 changes [options] [file[revRange] ...]
		/// <br/> 
		/// <br/> 	options: -i -t -l -L -f -c client -m max -s status -u user
		/// <br/> 
		/// <br/> 	Returns a list of all pending and submitted changelists currently
		/// <br/> 	stored in the server.
		/// <br/> 
		/// <br/> 	If files are specified, 'p4 changes' lists only changelists that
		/// <br/> 	affect those files.  If the file specification includes a revision
		/// <br/> 	range, 'p4 changes' lists only submitted changelists that affect
		/// <br/> 	the specified revisions.  See 'p4 help revisions' for details.
		/// <br/> 
		/// <br/> 	If files are not specified, 'p4 changes' limits its report
		/// <br/> 	according to each change's type ('public' or 'restricted').
		/// <br/> 	If a submitted or shelved change is restricted, the change is
		/// <br/> 	not reported unless the user owns the change or has list
		/// <br/> 	permission for at least one file in the change. Only the owner
		/// <br/> 	of a restricted and pending (not shelved) change is permitted
		/// <br/> 	to see it.
		/// <br/> 
		/// <br/> 	The -i flag also includes any changelists integrated into the
		/// <br/> 	specified files.
		/// <br/> 
		/// <br/> 	The -t flag displays the time as well as the date.
		/// <br/> 
		/// <br/> 	The -l flag displays the full text of the changelist
		/// <br/> 	descriptions.
		/// <br/> 
		/// <br/> 	The -L flag displays the changelist descriptions, truncated to 250
		/// <br/> 	characters if longer.
		/// <br/> 
		/// <br/> 	The -f flag enables admin users to view restricted changes.
		/// <br/> 
		/// <br/> 	The -c client flag displays only submitted by the specified client.
		/// <br/> 
		/// <br/> 	The -m max flag limits changes to the 'max' most recent.
		/// <br/> 
		/// <br/> 	The -s status flag limits the output to pending, shelved or
		/// <br/> 	submitted changelists.
		/// <br/> 
		/// <br/> 	The -u user flag displays only changes owned by the specified user.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public ChangesCmdOptions(ChangesCmdFlags flags, string clientName, int maxItems,
				ChangeListStatus status, string userName)
			:base(flags, clientName, maxItems, status,  userName)
		{
		}
	}
	/// <summary>
	/// Flags for the group command.
	/// </summary>
	[Flags]
	public enum GroupCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -d flag deletes a pending changelist, if it has no opened files
		/// 	and no pending fixes associated with it.  Use 'p4 opened -a' to
		/// 	report on opened files and 'p4 reopen' to move them to another
		/// 	changelist.  Use 'p4 fixes -c changelist#' to report on pending
		/// 	fixes and 'p4 fix -d -c changelist# jobs...' to delete pending
		/// 	fixes. The changelist can be deleted only by the user and client
		/// 	who created it, or by a user with 'admin' privilege using the -f
		/// 	flag.
		/// </summary>
		Delete = 0x0001,
		/// <summary>
		/// 	The -o flag writes the changelist specification to the standard
		/// 	output.  The user's editor is not invoked.
		/// </summary>
		Output = 0x0002,
		/// <summary>
		/// 	The -i flag reads a changelist specification from the standard
		/// 	input.  The user's editor is not invoked.
		/// </summary>
		Input = 0x0004,
		/// <summary>
		/// 	The -a flag enables a user without 'super' access to modify the group
		/// 	if that user is an 'owner' of that group. Group owners	are specified
		/// 	in the 'Owners' field of the group spec.
		/// </summary>
		OwnerAccess = 0x0008,
        /// <summary>
        /// 	The -A flag enables a user with 'admin' access to add a new group.
        ///     Existing groups may not be modified when this flag is used.
        /// </summary>
        AdminAdd = 0x0010
	}

	public partial class Options
	{
		/// <summary>
		/// Group command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help group</b>
		/// <br/> 
		/// <br/>     group -- Change members of user group
		/// <br/> 
		/// <br/>     p4 group [-a] name
		/// <br/>     p4 group -d [-a] name
		/// <br/>     p4 group -o name
		/// <br/>     p4 group -i [-a]
		/// <br/> 
		/// <br/> 	Create a group or modify the membership of an existing group.
		/// <br/> 	A group can contain users and other groups. The group specification
		/// <br/> 	is put into a temporary file and the editor (configured by the
		/// <br/> 	environment variable $P4EDITOR) is invoked.
		/// <br/> 
		/// <br/> 	A group exists when it has any users or other groups in it, and
		/// <br/> 	ceases to exist if all users and groups in it are removed.
		/// <br/> 
		/// <br/> 	Each group has MaxResults, MaxScanRows, and MaxLockTime fields,
		/// <br/> 	which limit the resources committed to operations performed by
		/// <br/> 	members of the group.  For these fields, 'unlimited' or 'unset'
		/// <br/> 	means no limit for that	group.  An individual user's limit is the
		/// <br/> 	highest of any group with a limit to which he belongs, unlimited if
		/// <br/> 	any of his groups has 'unlimited' for that field, or unlimited
		/// <br/> 	if he belongs to no group with a limit.  See 'p4 help maxresults'
		/// <br/> 	for more information on MaxResults, MaxScanRows and MaxLockTime.
		/// <br/> 
		/// <br/> 	Each group also has a Timeout field, which specifies how long (in
		/// <br/> 	seconds)  a 'p4 login' ticket remains valid.  A value of 'unset' or
		/// <br/> 	'unlimited' is equivalent to no timeout. An individual's timeout is
		/// <br/> 	the highest of any group with a limit to which he belongs, unlimited
		/// <br/> 	if any of his groups has 'unlimited' for the timeout value, or
		/// <br/> 	unlimited if he belongs to no group with a limit. See 'p4 help login'
		/// <br/> 	for more information.
		/// <br/> 
		/// <br/> 	Each group has a PasswordTimeout field, which determines how long a
		/// <br/> 	password remains valid for members of the group.
		/// <br/> 
		/// <br/> 	The -d flag deletes a group.
		/// <br/> 
		/// <br/> 	The -o flag writes the group specification to standard output. The
		/// <br/> 	user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads a group specification from standard input. The
		/// <br/> 	user's editor is not invoked.  The new group specification replaces
		/// <br/> 	the previous one.
		/// <br/> 
		/// <br/> 	The -a flag enables a user without 'super' access to modify the group
		/// <br/> 	if that user is an 'owner' of that group. Group owners	are specified
		/// <br/> 	in the 'Owners' field of the group spec.
		/// <br/> 
		/// <br/>   The -A flag enables a user with 'admin' access to add a new group.
        /// <br/>   Existing groups may not be modified when this flag is used.
        /// <br/>
		/// <br/> 	All commands that require access granted by 'p4 protect' consider a
		/// <br/> 	user's groups when calculating access levels.
		/// <br/> 
		/// <br/> 	'p4 group' requires 'super' access granted by 'p4 protect' unless
		/// <br/> 	invoked with the '-a' flag by a qualified user.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(GroupCmdFlags flags)
		{
			if ((flags & GroupCmdFlags.Output) != 0)
			{
				this["-o"] = null;
			}

			if ((flags & GroupCmdFlags.Input) != 0)
			{
				this["-i"] = null;
			}

			if ((flags & GroupCmdFlags.Delete) != 0)
			{
				this["-d"] = null;
			}

			if ((flags & GroupCmdFlags.OwnerAccess) != 0)
			{
				this["-a"] = null;
			}

            if ((flags & GroupCmdFlags.AdminAdd) != 0)
            {
                this["-A"] = null;
            }

		}
	}
	/// <summary>
	/// Group command options.
	/// </summary>
	public class GroupCmdOptions : Options
	{
		/// <summary>
		/// Group command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help group</b>
		/// <br/> 
		/// <br/>     group -- Change members of user group
		/// <br/> 
		/// <br/>     p4 group [-a] name
		/// <br/>     p4 group -d [-a] name
		/// <br/>     p4 group -o name
		/// <br/>     p4 group -i [-a]
		/// <br/> 
		/// <br/> 	Create a group or modify the membership of an existing group.
		/// <br/> 	A group can contain users and other groups. The group specification
		/// <br/> 	is put into a temporary file and the editor (configured by the
		/// <br/> 	environment variable $P4EDITOR) is invoked.
		/// <br/> 
		/// <br/> 	A group exists when it has any users or other groups in it, and
		/// <br/> 	ceases to exist if all users and groups in it are removed.
		/// <br/> 
		/// <br/> 	Each group has MaxResults, MaxScanRows, and MaxLockTime fields,
		/// <br/> 	which limit the resources committed to operations performed by
		/// <br/> 	members of the group.  For these fields, 'unlimited' or 'unset'
		/// <br/> 	means no limit for that	group.  An individual user's limit is the
		/// <br/> 	highest of any group with a limit to which he belongs, unlimited if
		/// <br/> 	any of his groups has 'unlimited' for that field, or unlimited
		/// <br/> 	if he belongs to no group with a limit.  See 'p4 help maxresults'
		/// <br/> 	for more information on MaxResults, MaxScanRows and MaxLockTime.
		/// <br/> 
		/// <br/> 	Each group also has a Timeout field, which specifies how long (in
		/// <br/> 	seconds)  a 'p4 login' ticket remains valid.  A value of 'unset' or
		/// <br/> 	'unlimited' is equivalent to no timeout. An individual's timeout is
		/// <br/> 	the highest of any group with a limit to which he belongs, unlimited
		/// <br/> 	if any of his groups has 'unlimited' for the timeout value, or
		/// <br/> 	unlimited if he belongs to no group with a limit. See 'p4 help login'
		/// <br/> 	for more information.
		/// <br/> 
		/// <br/> 	Each group has a PasswordTimeout field, which determines how long a
		/// <br/> 	password remains valid for members of the group.
		/// <br/> 
		/// <br/> 	The -d flag deletes a group.
		/// <br/> 
		/// <br/> 	The -o flag writes the group specification to standard output. The
		/// <br/> 	user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads a group specification from standard input. The
		/// <br/> 	user's editor is not invoked.  The new group specification replaces
		/// <br/> 	the previous one.
		/// <br/> 
		/// <br/> 	The -a flag enables a user without 'super' access to modify the group
		/// <br/> 	if that user is an 'owner' of that group. Group owners	are specified
		/// <br/> 	in the 'Owners' field of the group spec.
		/// <br/> 
		/// <br/> 	All commands that require access granted by 'p4 protect' consider a
		/// <br/> 	user's groups when calculating access levels.
		/// <br/> 
		/// <br/> 	'p4 group' requires 'super' access granted by 'p4 protect' unless
		/// <br/> 	invoked with the '-a' flag by a qualified user.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public GroupCmdOptions(GroupCmdFlags flags)
			: base(flags) { }
	}

	/// <summary>
	/// Flags for the groups command.
	/// </summary>
	[Flags]
	public enum GroupsCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -i flag also displays groups that the user or group belongs to
		/// 	indirectly by means of membership in subgroups.
		/// 	(The group parameter for the command can be a user or group name)
		/// </summary>
		IncludeIndirect = 0x0001,
		/// <summary>
		/// 	The -v flag displays the MaxResults, MaxScanRows, MaxLockTime, and
		/// 	Timeout values for the specified group.
		/// </summary>
		IncludeAllValues = 0x0002,
	}

	public partial class Options
	{
		/// <summary>
		/// Groups command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="maxItems"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help groups</b>
		/// <br/> 
		/// <br/>     groups -- List groups (of users)
		/// <br/> 
		/// <br/>     p4 groups [-m max] [[[-i] user | group] | [-v [group]]]
		/// <br/> 
		/// <br/> 	List all user groups defined in the server. If a user argument is,
		/// <br/> 	specified, only groups containing that user are displayed. If a group
		/// <br/> 	argument is specified, only groups containing the group are displayed.
		/// <br/> 
		/// <br/> 	The -i flag also displays groups that the user or group belongs to
		/// <br/> 	indirectly by means of membership in subgroups.
		/// <br/> 
		/// <br/> 	The -m max flag limits output to the specified number of groups.
		/// <br/> 
		/// <br/> 	The -v flag displays the MaxResults, MaxScanRows, MaxLockTime, and
		/// <br/> 	Timeout values for the specified group.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(GroupsCmdFlags flags, int maxItems)
		{
			if (maxItems >= 0)
			{
				this["-m"] = maxItems.ToString();
			}

			if ((flags & GroupsCmdFlags.IncludeIndirect) != 0)
			{
				this["-i"] = null;
			}

			if ((flags & GroupsCmdFlags.IncludeAllValues) != 0)
			{
				this["-v"] = null;
			}
		}
	}
	/// <summary>
	/// Groups command options.
	/// </summary>
	public class GroupsCmdOptions : Options
	{
		public GroupsCmdOptions(GroupsCmdFlags flags, int maxItems)
			: base(flags, maxItems) { }
	}
	/// <summary>
	/// Flags for the job command.
	/// </summary>
	[Flags]
	public enum JobCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -d flag deletes the specified job. You cannot delete a job if
		/// 	it has pending or submitted fixes associated with it.
		/// </summary>
		Delete = 0x0001,
		/// <summary>
		/// 	The -o flag writes the job specification to the standard output.
		/// 	The user's editor is not invoked.
		/// </summary>
		Output = 0x0002,
		/// <summary>
		/// 	The -i flag reads a job specification from the standard input. The
		/// 	user's editor is not invoked.
		/// </summary>
		Input = 0x0004,
		/// <summary>
		/// 	The -f flag enables you set fields that are read-only by default.
		/// 	The -f flag requires 'admin' access, which is granted using the
		/// 	'p4 protect' command.
		/// </summary>
		Force = 0x0008,
	}

	public partial class Options
	{
		/// <summary>
		/// Options for job command.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help job</b>
		/// <br/> 
		/// <br/>     job -- Create or edit a job (defect) specification
		/// <br/> 
		/// <br/>     p4 job [-f] [jobName]
		/// <br/>     p4 job -d jobName
		/// <br/>     p4 job -o [jobName]
		/// <br/>     p4 job -i [-f]
		/// <br/> 
		/// <br/> 	The 'p4 job' command creates and edits job specifications using an
		/// <br/> 	ASCII form. A job is a defect, enhancement, or other unit of
		/// <br/> 	intended work.The 'p4 fix' command associates changelists with jobs.
		/// <br/> 
		/// <br/> 	With no arguments, 'p4 job' creates an empty job specification
		/// <br/> 	and invokes the user's editor.  When the specification is saved,
		/// <br/> 	a job name of the form jobNNNNNN is assigned.  If the jobName
		/// <br/> 	parameter is specified on the command line, the job is created or
		/// <br/> 	opened for editing.
		/// <br/> 
		/// <br/> 	As jobs are entered or updated, all fields are indexed for searching
		/// <br/> 	Text fields are broken into individual alphanumeric words (punctuation
		/// <br/> 	and whitespace are ignored) and each word is case-folded and entered
		/// <br/> 	into the word index.  Date fields are converted to an internal
		/// <br/> 	representation (seconds since 1970/01/01 00:00:00) and entered
		/// <br/> 	into the date index.
		/// <br/> 
		/// <br/> 	The fields that compose a job are defined by the 'p4 jobspec' command.
		/// <br/> 	Perforce provides a default job specification that you can edit.
		/// <br/> 
		/// <br/> 	The -d flag deletes the specified job. You cannot delete a job if
		/// <br/> 	it has pending or submitted fixes associated with it.
		/// <br/> 
		/// <br/> 	The -o flag writes the job specification to the standard output.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads a job specification from the standard input. The
		/// <br/> 	user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -f flag enables you set fields that are read-only by default.
		/// <br/> 	The -f flag requires 'admin' access, which is granted using the
		/// <br/> 	'p4 protect' command.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(JobCmdFlags flags)
		{
			if ((flags & JobCmdFlags.Output) != 0)
			{
				this["-o"] = null;
			}

			if ((flags & JobCmdFlags.Input) != 0)
			{
				this["-i"] = null;
			}

			if ((flags & JobCmdFlags.Delete) != 0)
			{
				this["-d"] = null;
			}

			if ((flags & JobCmdFlags.Force) != 0)
			{
				this["-f"] = null;
			}
		}
	}
	///<summary>
	///Job command options
	/// </summary>
	public class JobCmdOptions:Options
	{
				/// <summary>
		/// Options for job command.
		/// </summary>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help job</b>
		/// <br/> 
		/// <br/>     job -- Create or edit a job (defect) specification
		/// <br/> 
		/// <br/>     p4 job [-f] [jobName]
		/// <br/>     p4 job -d jobName
		/// <br/>     p4 job -o [jobName]
		/// <br/>     p4 job -i [-f]
		/// <br/> 
		/// <br/> 	The 'p4 job' command creates and edits job specifications using an
		/// <br/> 	ASCII form. A job is a defect, enhancement, or other unit of
		/// <br/> 	intended work.The 'p4 fix' command associates changelists with jobs.
		/// <br/> 
		/// <br/> 	With no arguments, 'p4 job' creates an empty job specification
		/// <br/> 	and invokes the user's editor.  When the specification is saved,
		/// <br/> 	a job name of the form jobNNNNNN is assigned.  If the jobName
		/// <br/> 	parameter is specified on the command line, the job is created or
		/// <br/> 	opened for editing.
		/// <br/> 
		/// <br/> 	As jobs are entered or updated, all fields are indexed for searching
		/// <br/> 	Text fields are broken into individual alphanumeric words (punctuation
		/// <br/> 	and whitespace are ignored) and each word is case-folded and entered
		/// <br/> 	into the word index.  Date fields are converted to an internal
		/// <br/> 	representation (seconds since 1970/01/01 00:00:00) and entered
		/// <br/> 	into the date index.
		/// <br/> 
		/// <br/> 	The fields that compose a job are defined by the 'p4 jobspec' command.
		/// <br/> 	Perforce provides a default job specification that you can edit.
		/// <br/> 
		/// <br/> 	The -d flag deletes the specified job. You cannot delete a job if
		/// <br/> 	it has pending or submitted fixes associated with it.
		/// <br/> 
		/// <br/> 	The -o flag writes the job specification to the standard output.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads a job specification from the standard input. The
		/// <br/> 	user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -f flag enables you set fields that are read-only by default.
		/// <br/> 	The -f flag requires 'admin' access, which is granted using the
		/// <br/> 	'p4 protect' command.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		JobCmdOptions(JobCmdFlags flags)
			:base(flags) {}
	}
	/// <summary>
	/// Flags for the jobs command.
	/// </summary>
	[Flags]
	public enum JobsCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -i flag includes any fixes made by changelists integrated into
		/// 	the specified files.
		/// </summary>
		IncludeIntegratedFixes = 0x0001,
		/// <summary>
		/// 	The -l flag produces long output with the full text of the job
		/// 	descriptions.
		/// </summary>
		LongDescriptions = 0x0002,
		/// <summary>
		/// 	The -r flag sorts the jobs in reverse order (by job name).
		/// </summary>
		ReverseSort = 0x0004,
		/// <summary>
		/// 	The -R flag rebuilds the jobs table and reindexes each job, which
		/// 	is necessary after upgrading to 98.2.  'p4 jobs -R' requires that
		/// 	that the user be an operator or have 'super' access granted by
		/// 	'p4 protect'.
		/// </summary>
		RebuildJobsTable = 0x008,
	}

	public partial class Options
	{
		/// <summary>
		/// Jobs command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="jobView"></param>
		/// <param name="maxItems"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help jobs</b>
		/// <br/> 
		/// <br/>     jobs -- Display list of jobs
		/// <br/> 
		/// <br/>     p4 jobs [-e jobview -i -l -m max -r] [file[revRange] ...]
		/// <br/>     p4 jobs -R
		/// <br/> 
		/// <br/> 	Lists jobs in the server. If a file specification is included, fixes
		/// <br/> 	for submitted changelists affecting the specified files are listed.
		/// <br/> 	The file specification can include wildcards and a revision range.
		/// <br/> 	 See 'p4 help revisions' for details about specifying revisions.
		/// <br/> 
		/// <br/> 	The -e flag lists jobs matching the expression specified in the
		/// <br/> 	jobview parameter. For a description of jobview syntax, see 'p4 help
		/// <br/> 	jobview'.
		/// <br/> 
		/// <br/> 	The -i flag includes any fixes made by changelists integrated into
		/// <br/> 	the specified files.
		/// <br/> 
		/// <br/> 	The -l flag produces long output with the full text of the job
		/// <br/> 	descriptions.
		/// <br/> 
		/// <br/> 	The -m max flag limits the output to the first 'max' jobs, ordered
		/// <br/> 	by their job name.
		/// <br/> 
		/// <br/> 	The -r flag sorts the jobs in reverse order (by job name).
		/// <br/> 
		/// <br/> 	The -R flag rebuilds the jobs table and reindexes each job, which
		/// <br/> 	is necessary after upgrading to 98.2.  'p4 jobs -R' requires that
		/// <br/> 	that the user be an operator or have 'super' access granted by
		/// <br/> 	'p4 protect'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(JobsCmdFlags flags, string jobView, int maxItems)
		{
			if (String.IsNullOrEmpty(jobView) == false)
			{
				this["-e"] = jobView;
			}

			if ((flags & JobsCmdFlags.IncludeIntegratedFixes) != 0)
			{
				this["-i"] = null;
			}

			if ((flags & JobsCmdFlags.LongDescriptions) != 0)
			{
				this["-l"] = null;
			}

			if (maxItems >= 0)
			{
				this["-m"] = maxItems.ToString();
			}

			if ((flags & JobsCmdFlags.ReverseSort) != 0)
			{
				this["-r"] = null;
			}

			if ((flags & JobsCmdFlags.RebuildJobsTable) != 0)
			{
				this["-R"] = null;
			}
		}
	}
	/// <summary>
	/// Jobs command options
	/// </summary>
	public class JobsCmdOptions : Options
	{
		/// <summary>
		/// Jobs command options.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="jobView"></param>
		/// <param name="maxItems"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help jobs</b>
		/// <br/> 
		/// <br/>     jobs -- Display list of jobs
		/// <br/> 
		/// <br/>     p4 jobs [-e jobview -i -l -m max -r] [file[revRange] ...]
		/// <br/>     p4 jobs -R
		/// <br/> 
		/// <br/> 	Lists jobs in the server. If a file specification is included, fixes
		/// <br/> 	for submitted changelists affecting the specified files are listed.
		/// <br/> 	The file specification can include wildcards and a revision range.
		/// <br/> 	 See 'p4 help revisions' for details about specifying revisions.
		/// <br/> 
		/// <br/> 	The -e flag lists jobs matching the expression specified in the
		/// <br/> 	jobview parameter. For a description of jobview syntax, see 'p4 help
		/// <br/> 	jobview'.
		/// <br/> 
		/// <br/> 	The -i flag includes any fixes made by changelists integrated into
		/// <br/> 	the specified files.
		/// <br/> 
		/// <br/> 	The -l flag produces long output with the full text of the job
		/// <br/> 	descriptions.
		/// <br/> 
		/// <br/> 	The -m max flag limits the output to the first 'max' jobs, ordered
		/// <br/> 	by their job name.
		/// <br/> 
		/// <br/> 	The -r flag sorts the jobs in reverse order (by job name).
		/// <br/> 
		/// <br/> 	The -R flag rebuilds the jobs table and reindexes each job, which
		/// <br/> 	is necessary after upgrading to 98.2.  'p4 jobs -R' requires that
		/// <br/> 	that the user be an operator or have 'super' access granted by
		/// <br/> 	'p4 protect'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public JobsCmdOptions(JobsCmdFlags flags, string jobView, int maxItems)
			: base(flags, jobView, maxItems) { }
	}
	/// <summary>
	/// Flags for the files command.
	/// </summary>
	[Flags]
	public enum FilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -a flag displays all revisions within the specific range, rather
		/// 	than just the highest revision in the range.
		/// </summary>
		AllRevisions = 0x0001,
		/// <summary>
		/// 	The -A flag displays files in archive depots.
		/// </summary>
		IncludeArchives = 0x0002,
	}

	public partial class Options
	{
		/// <summary>
		/// Options for the files command.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="maxItems"></param>
		/// <returns></returns>
		/// <remarks>
		/// p4 help Files
		/// </remarks>
		public Options(FilesCmdFlags flags, int maxItems)
		{
			if ((flags & FilesCmdFlags.AllRevisions) != 0)
			{
				this["-a"] = null;
			}

			if ((flags & FilesCmdFlags.IncludeArchives) != 0)
			{
				this["-A"] = null;
			}

			if (maxItems >= 0)
			{
				this["-m"] = maxItems.ToString();
			}
		}
	}
	/// <summary>
	/// Files command options
	/// </summary>
	public class FilesCmdOptions : Options
	{
		/// <summary>
		/// Options for the files command.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="maxItems"></param>
		/// <returns></returns>
		/// <remarks>
		/// p4 help Files
		/// </remarks>
		public FilesCmdOptions(FilesCmdFlags flags, int maxItems)
			: base(flags, maxItems) { }
	}
	/// <summary>
	/// Flags for the filelog command.
	/// </summary>
	[Flags]
	public enum FileLogCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// The -i flag, includes inherited file history. If a file was created by
		/// branching (using 'p4 integrate'), filelog lists the revisions of the
		/// file's ancestors up to the branch points that led to the specified
		/// revision.  File history inherited by renaming (using 'p4 move') is
		/// always displayed regardless of whether -i is specified.
		/// </summary>
		IncludeInherited = 0x0001,
		/// <summary>
		/// The -h flag, displays file content history instead of file name
		/// history.  The list includes revisions of other files that were
		/// branched or copied (using 'p4 integrate' and 'p4 resolve -at') to
		/// the specified revision.  Revisions that were replaced by copying
		/// or branching are omitted, even if they are part of the history of
		/// the specified revision.
		/// </summary>
		DisplayContentHistory = 0x0002,
		/// <summary>
		/// The -t flag, displays the time as well as the date.
		/// </summary>
		IncludeTime = 0x0004,
		/// <summary>
		/// The -l flag lists the full text of the changelist descriptions.
		/// </summary>
		LongOutput = 0x0008,
		/// <summary>
		/// The -L flag lists the full text of the changelist descriptions,
		/// truncated to 250 characters if longer.
		/// </summary>
		TruncatedLongOutput = 0x0010,
		/// <summary>
		/// The -s flag displays a shortened form of filelog that omits
		/// non-contributory integrations.
		/// </summary>
		ShortForm = 0x0020
	};

	public partial class Options
	{
		/// <summary>
		/// Options for the filelog command.
		/// </summary>
		public Options(FileLogCmdFlags flags, int changelistId, int maxItems)
		{
			if (changelistId >= 0)
			{
				this["-c"] = changelistId.ToString();
			}

			if ((flags & FileLogCmdFlags.IncludeInherited) != 0)
			{
				this["-i"] = null;
			}

			if ((flags & FileLogCmdFlags.DisplayContentHistory) != 0)
			{
				this["-h"] = null;
			}

			if ((flags & FileLogCmdFlags.LongOutput) != 0)
			{
				this["-l"] = null;
			}

			if ((flags & FileLogCmdFlags.TruncatedLongOutput) != 0)
			{
				this["-L"] = null;
			}

			if ((flags & FileLogCmdFlags.IncludeTime) != 0)
			{
				this["-t"] = null;
			}

			if (maxItems >= 0)
			{
				this["-m"] = maxItems.ToString();
			}

			if ((flags & FileLogCmdFlags.ShortForm) != 0)
			{
				this["-s"] = null;
			}
		}
	}

	/// <summary>
	/// Options for the filelog command.
	/// </summary>
	public class FilelogCmdOptions : Options
	{
		/// <summary>
		/// Options for the filelog command.
		/// </summary>
		public FilelogCmdOptions(FileLogCmdFlags flags, int changelistId, int maxItems)
			: base(flags, changelistId, maxItems) {}
	}

	/// <summary>
	/// Flags for the login command.
	/// </summary>
	[Flags]
	public enum LoginCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// <br/> 	The -a flag causes the server to issue a ticket that is valid on all
		/// <br/> 	host machines.
		/// </summary>
		AllHosts = 0x0001,
		/// <summary>
		/// <br/> 	The -p flag displays the ticket, but does not store it on the client
		/// <br/> 	machine.
		/// </summary>
		DisplayTicket = 0x0002,
		/// <summary>
		/// <br/> 	The -s flag displays the status of the current ticket (if there is
		/// <br/> 	one).
		/// </summary>
		DisplayStatus = 0x0004,
	};

	public partial class Options
	{
		/// <summary>
		/// Options for the login command.
		/// </summary>
		public Options(LoginCmdFlags flags, string host)
		{
			if ((flags & LoginCmdFlags.AllHosts) != 0)
			{
				this["-a"] = null;
			}

			if ((flags & LoginCmdFlags.DisplayTicket) != 0)
			{
				this["-p"] = null;
			}

			if ((flags & LoginCmdFlags.DisplayStatus) != 0)
			{
				this["-s"] = null;
			}

			if (string.IsNullOrEmpty(host) == false)
			{
				this["-h"] = host;
			}
		}
	}
		/// <summary>
		/// Options for the login command.
		/// </summary>
	public class LoginCmdOptions : Options
	{
		/// <summary>
		/// Options for the login command.
		/// </summary>
		public LoginCmdOptions(LoginCmdFlags flags, string host)
			: base(flags, host) {}
	}

	/// <summary>
	/// Flags for the logout command.
	/// </summary>
	[Flags]
	public enum LogoutCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// <br/> 	The -a flag causes the server to issue a ticket that is valid on all
		/// <br/> 	host machines.
		/// </summary>
		AllHosts = 0x0001,
	};
	public partial class Options
	{
		/// <summary>
		/// Options for the logout command.
		/// </summary>
		public Options(LogoutCmdFlags flags, string host)
		{
			if ((flags & LogoutCmdFlags.AllHosts) != 0)
			{
				this["-a"] = null;
			}
		}
	}
	/// <summary>
	/// Options for the logout command.
	/// </summary>
	public class LogoutCmdOptions : Options
	{
		/// <summary>
		/// Options for the logout command.
		/// </summary>
		public LogoutCmdOptions(LogoutCmdFlags flags, string host)
			: base(flags, host) { }
	}
	/// <summary>
	/// Flags for the tag command.
	/// </summary>
	[Flags]
	public enum TagFilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// The -d deletes the association between the specified files and the
		/// label, regardless of revision.
		/// </summary>
		Delete = 0x0001,
		/// <summary>
		/// The -n flag previews the results of the operation.
		/// </summary>
		ListOnly = 0x0002,
	};

	public partial class Options
	{
		/// <summary>
		/// Options for the tag command.
		/// </summary>
		public Options(TagFilesCmdFlags flags, string label)
		{
			if ((flags & TagFilesCmdFlags.Delete) != 0)
			{
				this["-d"] = null;
			}

			if ((flags & TagFilesCmdFlags.ListOnly) != 0)
			{
				this["-n"] = null;
			}

			if (string.IsNullOrEmpty(label) != true)
			{
				this["-l"] = label;
			}
		}
	}
	/// <summary>
	/// Tag command options
	/// </summary>
	public class TagCmdOptions : Options
	{
		/// <summary>
		/// Options for the tag command.
		/// </summary>
		public TagCmdOptions(TagFilesCmdFlags flags, string label)
			: base(flags, label) { }
	}
	/// <summary>
	/// Flags for the stream command.
	/// </summary>
	[Flags]
	public enum StreamCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -d flag deletes the specified stream (unless the stream is
		/// 	referenced by child streams or stream clients).
		/// </summary>
		Delete = 0x0001,
		/// <summary>
		/// 	The -o flag writes the stream specification to the standard output.
		/// 	The user's editor is not invoked.
		/// </summary>
		Output = 0x0002,
		/// <summary>
		/// 	The -v may be used with -o to expose the automatically generated
		/// 	client view for this stream.
		/// </summary>
		View = 0x0004,
		/// <summary>
		/// 	The -i flag reads a stream specification from the standard input.
		/// 	The user's editor is not invoked.
		/// </summary>
		Input = 0x0008,
		/// <summary>
		/// 	The -f flag allows a user other than the owner to modify or delete
		/// 	a locked stream. It requires 'admin' access granted by 'p4 protect'.
		/// </summary>
		Force = 0x0010,
	}

	/// <summary>
	/// Options for the stream command
	/// </summary>
	public partial class Options
	{
		public Options(StreamCmdFlags flags, string parent, string type)
		{
			if ((flags & StreamCmdFlags.Output) != 0)
			{
				this["-o"] = null;
			}

			if ((flags & StreamCmdFlags.View) != 0)
			{
				this["-v"] = null;
			}

			if ((flags & StreamCmdFlags.Input) != 0)
			{
				this["-i"] = null;
			}

			if (String.IsNullOrEmpty(type) != true)
			{
				this["-t"] = type;
			}

			if ((flags & StreamCmdFlags.Delete) != 0)
			{
				this["-d"] = null;
			}

			if ((flags & StreamCmdFlags.Force) != 0)
			{
				this["-f"] = null;
			}

			if (String.IsNullOrEmpty(parent) != true)
			{
				this["-P"] = parent;
			}
		}
	}
	/// <summary>
	/// Stream command options
	/// </summary>
	public class StreamCmdOptions : Options
	{
		/// <summary>
		/// Stream command options
		/// </summary>
		public StreamCmdOptions(StreamCmdFlags flags, string parent, string type)
			: base(flags, parent, type) { }
	}
	/// <summary>
	/// Flags for the streams command.
	/// </summary>
	[Flags]
	public enum StreamsCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
	}

	/// <summary>
	/// Options for the streams command
	/// </summary>
	public partial class Options
	{
		public Options(StreamsCmdFlags flags, string filter, string tagged, string streampath, int maxItems)
		{
			if (String.IsNullOrEmpty(filter) != true)
			{
				this["-F"] = filter;
			}

			if (String.IsNullOrEmpty(tagged) != true)
			{
				this["-T"] = tagged;
			}

			if (maxItems >= 0)
			{
				this["-m"] = maxItems.ToString();
			}
		}
	}
	/// <summary>
	/// Streams command options
	/// </summary>
	public class StreamsCmdOptions : Options
	{
		/// <summary>
		/// Streams command options
		/// </summary>
		public StreamsCmdOptions(StreamsCmdFlags flags, string filter, string tagged,
			string streampath, int maxItems)
			: base(flags, filter, tagged, streampath, maxItems) { }
	}
    /// <summary>
    /// Flags for the istat command.
    /// </summary>
    [Flags]
    public enum GetStreamMetaDataCmdFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0x0000,
        /// <summary>
        /// 	The -r flag shows the status of integration to the stream from its
        /// 	parent. By default, status of integration in the other direction is
        /// 	shown, from the stream to its parent.
        /// </summary>
        Reverse = 0x0001,
        /// <summary>
        /// 	The -a flag shows status of integration in both directions.
        /// </summary>
        All = 0x0002,
        /// <summary>
        /// 	The -c flag forces 'p4 istat' to assume the cache is stale; it
        /// 	causes a search for pending integrations.  Use of this flag can
        /// 	impact server performance.
        /// </summary>
        Refresh = 0x0004,
        /// <summary>
        /// 	The -s flag shows cached state without refreshing stale data.
        /// </summary>
        Cached = 0x0008
    }

    /// <summary>
    /// Options for the istat command
    /// </summary>
    public partial class Options
    {
        public Options(GetStreamMetaDataCmdFlags flags)
        {
            if ((flags & GetStreamMetaDataCmdFlags.Reverse) != 0)
            {
                this["-r"] = null;
            }

            if ((flags & GetStreamMetaDataCmdFlags.All) != 0)
            {
                this["-a"] = null;
            }

            if ((flags & GetStreamMetaDataCmdFlags.Refresh) != 0)
            {
                this["-c"] = null;
            }

            if ((flags & GetStreamMetaDataCmdFlags.Cached) != 0)
            {
                this["-s"] = null;
            }
        }
    }
    /// <summary>
    /// GetStreamMetaData command options
    /// </summary>
    public class GetStreamMetaDataCmdOptions : Options
    {
        /// <summary>
        /// Stream command options
        /// </summary>
        public GetStreamMetaDataCmdOptions(GetStreamMetaDataCmdFlags flags)
            : base(flags) { }
    }
	/// <summary>
	/// Flags for the depot command.
	/// </summary>
	[Flags]
	public enum DepotCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -d flag deletes the specified depot. If any files reside in
		/// 	the depot, they must be removed with 'p4 obliterate' before
		/// 	deleting the depot.
		/// </summary>
		Delete = 0x0001,
		/// <summary>
		/// 	The -o flag writes the depot specification to the standard output.
		/// 	The user's editor is not invoked.
		/// </summary>
		Output = 0x0002,
		/// <summary>
		/// 	The -i flag reads a depot specification from the standard input.
		/// 	The user's editor is not invoked.
		/// </summary>
		Input = 0x0004,
	}

	/// <summary>
	/// 
	/// </summary>
	public partial class Options
	{
		public Options(DepotCmdFlags flags)
		{
			if ((flags & DepotCmdFlags.Output) != 0)
			{
				this["-o"] = null;
			}

			if ((flags & DepotCmdFlags.Input) != 0)
			{
				this["-i"] = null;
			}

			if ((flags & DepotCmdFlags.Delete) != 0)
			{
				this["-d"] = null;
			}

		}
	}
	/// <summary>
	/// depot command options
	/// </summary>
	public class DepotCmdOptions : Options
	{
		/// <summary>
		/// Options for the depot command
		/// </summary>
		public DepotCmdOptions(DepotCmdFlags flags)
			: base(flags) { }
	}
	/// <summary>
	/// Flags for the branch command.
	/// </summary>
	[Flags]
	public enum BranchSpecCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -d flag deletes the named branch spec.
		/// </summary>
		Delete = 0x0001,
		/// <summary>
		/// 	 The -o flag writes the branch spec to standard output.
		///      The user's editor is not invoked.
		/// </summary>
		Output = 0x0002,
		/// <summary>
		/// 	The -i flag reads a branch specification from the standard input.
		/// 	The user's editor is not invoked.
		/// </summary>
		Input = 0x0004,
		/// <summary>
		/// 	The -f flag enables a user with 'admin' privilege to delete the
		/// 	spec or set the 'last modified' date.  By default, specs can be
		/// 	deleted only by their owner.
		/// </summary>
		Force = 0x0008,
	}

	/// <summary>
	/// Options for the branch command
	/// </summary>
	public partial class Options
	{
		public Options(BranchSpecCmdFlags flags, string stream, string parent)
		{
			if ((flags & BranchSpecCmdFlags.Output) != 0)
			{
				this["-o"] = null;
			}

			if ((flags & BranchSpecCmdFlags.Input) != 0)
			{
				this["-i"] = null;
			}

			if ((flags & BranchSpecCmdFlags.Delete) != 0)
			{
				this["-d"] = null;
			}

			if ((flags & BranchSpecCmdFlags.Force) != 0)
			{
				this["-f"] = null;
			}

			if (String.IsNullOrEmpty(stream) != true)
			{
				this["-S"] = stream;
			}

			if (String.IsNullOrEmpty(parent) != true)
			{
				this["-P"] = parent;
			}

		}
	}
	/// <summary>
	/// Branch command options
	/// </summary>
	public class BranchCmdOptions : Options
	{
		/// <summary>
		/// Branch command options
		/// </summary>
		public BranchCmdOptions(BranchSpecCmdFlags flags, string stream, string parent)
			: base(flags, stream, parent) { }
	}

	/// <summary>
	/// Flags for the branches command.
	/// </summary>
	[Flags]
	public enum BranchSpecsCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -t flag displays the time as well as the date.
		/// </summary>
		Time = 0x0001,
        /// <summary>
        /// 	The -e nameFilter flag lists branchspecs with a name that matches
        /// 	the nameFilter pattern, for example: -e 'branchspec*'. -E makes
        /// 	the matching case-insensitive.
        /// </summary>
        IgnoreCase = 0x0002
	}

	/// <summary>
	/// Options for the branches command
	/// </summary>
	public partial class Options
	{
		public Options(BranchSpecsCmdFlags flags, string user, string nameFilter, int maxItems)
		{
			if ((flags & BranchSpecsCmdFlags.Time) != 0)
			{
				this["-t"] = null;
			}

			if (String.IsNullOrEmpty(user) != true)
			{
				this["-u"] = user;
			}

            if (String.IsNullOrEmpty(nameFilter) != true)
            {
                if ((flags & BranchSpecsCmdFlags.IgnoreCase) != 0)
                {
                    this["-E"] = nameFilter;
                }
                else
                {
                    this["-e"] = nameFilter;
                }
            }

			if (maxItems >= 0)
			{
				this["-m"] = maxItems.ToString();
			}
		}
	}
	/// <summary>
	/// Branches command options
	/// </summary>
	public class BranchesCmdOptions : Options
	{
		/// <summary>
		/// Branches command options
		/// </summary>
		public BranchesCmdOptions(BranchSpecsCmdFlags flags, string user, string nameFilter, int maxItems)
			:base(flags,user,nameFilter,maxItems) {}
	}
	/// <summary>
	/// Flags for the label command.
	/// </summary>
	[Flags]
	public enum LabelCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -d flag deletes the named label spec.
		/// </summary>
		Delete = 0x0001,
		/// <summary>
		/// 	 The -o flag writes the label spec to standard output.
		///      The user's editor is not invoked.
		/// </summary>
		Output = 0x0002,
		/// <summary>
		/// 	The -i flag reads a label specification from the standard input.
		/// 	The user's editor is not invoked.
		/// </summary>
		Input = 0x0004,
		/// <summary>
		/// 	The -f flag forces the deletion of a label. By default, locked
		/// 	labels can only be deleted by their owner.  The -f flag also
		/// 	permits the Last Modified date to be set.  The -f flag requires
		/// 	'admin' access, which is granted by 'p4 protect'.
		/// </summary>
		Force = 0x0008,
	}
	/// <summary>
	/// Options for the label command
	/// </summary>
	public partial class Options
	{
		public Options(LabelCmdFlags flags, string template)
		{
			if ((flags & LabelCmdFlags.Output) != 0)
			{
				this["-o"] = null;
			}

			if ((flags & LabelCmdFlags.Input) != 0)
			{
				this["-i"] = null;
			}

			if ((flags & LabelCmdFlags.Delete) != 0)
			{
				this["-d"] = null;
			}

			if ((flags & LabelCmdFlags.Force) != 0)
			{
				this["-f"] = null;
			}

			if (String.IsNullOrEmpty(template) != true)
			{
				this["-t"] = template;
			}

		}
	}
	/// <summary>
	/// Label command options
	/// </summary>
	public class LabelCmdOptions : Options
	{
		/// <summary>
		/// Label command options
		/// </summary>
		public LabelCmdOptions(LabelCmdFlags flags, string template)
			:base(flags,template) {}
	}
	/// <summary>
	/// Flags for the labels command.
	/// </summary>
	[Flags]
	public enum LabelsCmdFlags
	{
		/// <summary>
		///     No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -t flag displays the time as well as the date.
		/// </summary>
		Time = 0x0001,
        /// <summary>
        /// 	The -e nameFilter flag lists labels with a name that matches
        /// 	the nameFilter pattern, for example: -e 'label*'. -E makes
        /// 	the matching case-insensitive.
        /// </summary>
        IgnoreCase = 0x0002
	}

	/// <summary>
	/// Options for the labels command
	/// </summary>
	public partial class Options
	{
		public Options(LabelsCmdFlags flags, string user, string nameFilter,
			int maxItems, string fileRevRange)
		{
			if ((flags & LabelsCmdFlags.Time) != 0)
			{
				this["-t"] = null;
			}

			if (String.IsNullOrEmpty(user) != true)
			{
				this["-u"] = user;
			}

            if (String.IsNullOrEmpty(nameFilter) != true)
            {
                if ((flags & LabelsCmdFlags.IgnoreCase) != 0)
                {
                    this["-E"] = nameFilter;
                }
                else
                {
                    this["-e"] = nameFilter;
                }
            }

			if (maxItems >= 0)
			{
				this["-m"] = maxItems.ToString();
			}
		}
	}
	/// <summary>
	/// Labels command options
	/// </summary>
	public class LabelsCmdOptions : Options
	{
		/// <summary>
		/// Labels command options
		/// </summary>
		public LabelsCmdOptions(LabelsCmdFlags flags, string user, string nameFilter,
			int maxItems, string fileRevRange)
			: base(flags, user, nameFilter, maxItems, fileRevRange) { }
	}
	/// <summary>
	/// Flags for the diff2 command.
	/// </summary>
	[Flags]
	public enum GetDepotFileDiffsCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -d flag deletes the specified stream (unless the stream is
		/// 	referenced by child streams or stream clients).
		/// </summary>
		RCS = 0x0001,
		/// <summary>
		/// 	-dn RCS output.
		/// </summary>
		Context = 0x0002,
		/// <summary>
		/// 	-dc[n] context
		/// </summary>
		Summary = 0x0004,
		/// <summary>
		/// 	-dc[n] context
		/// </summary>
		Unified = 0x0008,
		/// <summary>
		/// 	-dc[n] context
		/// </summary>
		IgnoreWhitespaceChanges = 0x0010,
		/// <summary>
		///     -dc[n] context
		/// </summary>
		IgnoreWhitespace = 0x0020,
		/// <summary>
		///     -dc[n] context
		/// </summary>
		IgnoreLineEndings = 0x0040,
		/// <summary>
		///     The -q omits files that have identical content and types and
		///     suppresses the actual diff for all files.
		/// </summary>
		Supress = 0x0080,
		/// <summary>
		///     The -t flag forces 'p4 diff2' to diff binary files.
		/// </summary>
		DiffBinary = 0x0100,
		/// <summary>
		///     The -u flag  uses the GNU diff -u format and displays only files
		///     that differ. The file names and dates are in Perforce syntax, but
		///     but the output can be used by the patch program.
		/// </summary>
		GNU = 0x0200,
	}
	/// <summary>
	/// Options for the diff2 command
	/// </summary>
	public partial class Options
	{
		public Options(GetDepotFileDiffsCmdFlags flags,
			int contextLines, int unifiedLines, string branch,
			string stream, string parent)
		{
			if ((flags & GetDepotFileDiffsCmdFlags.RCS) != 0)
			{
				this["-dn"] = null;
			}

			if (((flags & GetDepotFileDiffsCmdFlags.Context) != 0)
				&& (contextLines >= 0))
			{
                this["-dc"+contextLines.ToString()]=null;
			}

			if ((flags & GetDepotFileDiffsCmdFlags.Summary) != 0)
			{
				this["-ds"] = null;
			}

			if (((flags & GetDepotFileDiffsCmdFlags.Unified) != 0)
				&& (contextLines >= 0))
			{
                this["-du"+ unifiedLines.ToString()]=null;
			}

			if ((flags & GetDepotFileDiffsCmdFlags.IgnoreWhitespaceChanges) != 0)
			{
				this["-db"] = null;
			}

			if ((flags & GetDepotFileDiffsCmdFlags.IgnoreWhitespace) != 0)
			{
				this["-dw"] = null;
			}

			if ((flags & GetDepotFileDiffsCmdFlags.IgnoreLineEndings) != 0)
			{
				this["-dl"] = null;
			}

			if ((flags & GetDepotFileDiffsCmdFlags.Supress) != 0)
			{
				this["-q"] = null;
			}

			if ((flags & GetDepotFileDiffsCmdFlags.DiffBinary) != 0)
			{
				this["-t"] = null;
			}

			if ((flags & GetDepotFileDiffsCmdFlags.GNU) != 0)
			{
				this["-u"] = null;
			}

			if (String.IsNullOrEmpty(branch) != true)
			{
				this["-b"] = branch;
			}

			if (String.IsNullOrEmpty(stream) != true)
			{
				this["-S"] = stream;
			}

			if (String.IsNullOrEmpty(parent) != true)
			{
				this["-P"] = parent;
			}
		}
	}
	/// <summary>
	/// Diff2 command options
	/// </summary>
	public class GetDepotFileDiffsCmdOptions:Options
	{
		/// <summary>
		/// Diff2 command options
		/// </summary>
		public GetDepotFileDiffsCmdOptions(GetDepotFileDiffsCmdFlags flags,
			int contextLines, int unifiedLines, string branch,
			string stream, string parent)
			:base(flags,contextLines,unifiedLines,branch,stream,parent) {}
	}
	///// possibly redundant with FilesCmd
	///// <summary>
	///// Flags for the files command.
	///// </summary>
	//[Flags]
	//public enum GetFilesCmdFlags
	//{
	//    /// <summary>
	//    /// No flags.
	//    /// </summary>
	//    None = 0x0000,
	//    /// <summary>
	//    /// 	The -a flag displays all revisions within the specific range,
	//    /// 	rather than just the highest revision in the range.
	//    /// </summary>
	//    AllRevisions = 0x0001,
	//    /// <summary>
	//    /// 	The -A flag displays files in archive depots.
	//    /// </summary>
	//    Archive = 0x0002
	//}

	///// <summary>
	///// Options for the Files command
	///// </summary>
	//public partial class Options
	//{
	//    public Options(GetFilesCmdFlags flags, int maxItems)
	//    {
	//        if ((flags & GetFilesCmdFlags.AllRevisions) != 0)
	//        {
	//            this["-a"] = null;
	//        }

	//        if ((flags & GetFilesCmdFlags.Archive) != 0)
	//        {
	//            this["-A"] = null;
	//        }

	//        if (maxItems > 0)
	//        {
	//            this["-m"] = maxItems.ToString();
	//        }

	//    }
	//}

	/// <summary>
	/// Flags for the opened command.
	/// </summary>
	[Flags]
	public enum GetOpenedFilesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -a flag lists opened files in all clients.  By default,
		/// 	only files opened by the current client are listed.
		/// </summary>
		AllClients = 0x0001
	}

	/// <summary>
	/// Options for GetOpenedFiles
	/// </summary>
	public partial class Options
	{
		/// <summary>
		/// Options for GetOpenedFiles
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changelist"></param>
		/// <param name="client"></param>
		/// <param name="user"></param>
		/// <param name="maxItems"></param>
		/// <remarks>
		/// <br/><b>p4 help opened</b>
		/// <br/> 
		/// <br/>     opened -- List open files and display file status
		/// <br/> 
		/// <br/>     p4 opened [-a -c changelist# -C client -u user -m max] [file ...]
		/// <br/> 
		/// <br/> 	Lists files currently opened in pending changelists, or, for
		/// <br/> 	specified files, show whether they are currently opened or locked.
		/// <br/> 	If the file specification is omitted, all files open in the current
		/// <br/> 	client workspace are listed.
		/// <br/> 
		/// <br/> 	The -a flag lists opened files in all clients.  By default, only
		/// <br/> 	files opened by the current client are listed.
		/// <br/> 
		/// <br/> 	The -c changelist# flag lists files opened in the specified
		/// <br/> 	changelist#.
		/// <br/> 
		/// <br/> 	The -C client flag lists files open in the specified client workspace.
		/// <br/> 
		/// <br/> 	The -u user flag lists files opened by the specified user.
		/// <br/> 
		/// <br/> 	The -m max flag limits output to the first 'max' number of files.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(GetOpenedFilesCmdFlags flags, string changelist, string client, string user, int maxItems)
		{
			if ((flags & GetOpenedFilesCmdFlags.AllClients) != 0)
			{
				this["-a"] = null;
			}

			if (String.IsNullOrEmpty(changelist) != true)
			{
				this["-c"] = changelist;
			}

			if (String.IsNullOrEmpty(client) != true)
			{
				this["-C"] = client;
			}

			if (String.IsNullOrEmpty(user) != true)
			{
				this["-u"] = user;
			}

			if (maxItems > 0)
			{
				this["-m"] = maxItems.ToString();
			}

		}
	}

	/// <summary>
	/// GetOpenedFiles options
	/// </summary>
	public class GetOpenedFilesOptions : Options
	{
		/// <summary>
		/// Options for GetOpenedFiles
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="changelist"></param>
		/// <param name="client"></param>
		/// <param name="user"></param>
		/// <param name="maxItems"></param>
		/// <remarks>
		/// <br/><b>p4 help opened</b>
		/// <br/> 
		/// <br/>     opened -- List open files and display file status
		/// <br/> 
		/// <br/>     p4 opened [-a -c changelist# -C client -u user -m max] [file ...]
		/// <br/> 
		/// <br/> 	Lists files currently opened in pending changelists, or, for
		/// <br/> 	specified files, show whether they are currently opened or locked.
		/// <br/> 	If the file specification is omitted, all files open in the current
		/// <br/> 	client workspace are listed.
		/// <br/> 
		/// <br/> 	The -a flag lists opened files in all clients.  By default, only
		/// <br/> 	files opened by the current client are listed.
		/// <br/> 
		/// <br/> 	The -c changelist# flag lists files opened in the specified
		/// <br/> 	changelist#.
		/// <br/> 
		/// <br/> 	The -C client flag lists files open in the specified client workspace.
		/// <br/> 
		/// <br/> 	The -u user flag lists files opened by the specified user.
		/// <br/> 
		/// <br/> 	The -m max flag limits output to the first 'max' number of files.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public GetOpenedFilesOptions(GetOpenedFilesCmdFlags flags, string changelist, string client, string user, int maxItems)
			: base(flags, changelist, client, user, maxItems) { }
	}

	/// <summary>
	/// Flags for the fstat command.
	/// </summary>
	[Flags]
	public enum GetFileMetadataCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// The -L flag can be used with multiple file arguments that are in
		/// full depot syntax and include a valid revision number. When this
		/// flag is used the arguments are processed together by building an
		/// internal table similar to a label. This file list processing is
		/// significantly faster than having to call the internal query engine
		/// for each individual file argument. However, the file argument syntax
		/// is strict and the command will not run if an error is encountered.
		/// </summary>
		MultiFileArgs = 0x0001,
		/// <summary>
		/// The -r flag sorts the output in reverse order.
		/// </summary>
		Reverse = 0x0002,
		/// <summary>
		/// -Of     output all revisions for the given files (this
		///         option suppresses other* and resolve* fields)
		/// </summary>
		AllRevisions = 0x0004,
		/// <summary>
		/// -Ol     output a fileSize and digest field for each revision
		///         (this may be expensive to compute)
		/// </summary>
		FileSize = 0x0008,
		/// <summary>
		/// -Op     output the local file path in both Perforce syntax
		///         (//client/) as 'clientFile' and host form as 'path'
		/// </summary>
		LocalPath = 0x0010,
		/// <summary>
		///  -Or     output pending integration record information for
		///          files opened on the current client, or if used with
		///          '-e <change> -Rs', on the shelved change
		/// </summary>
		PendingInteg = 0x0020,
		/// <summary>
		/// -Os     exclude client-related data from output
		/// </summary>
		ExcludeClientData = 0x0040,
		/// <summary>
		/// -Rc     limit output to  files mapped in the client view
		/// </summary>
		ClientMapped = 0x0080,
		/// <summary>
		/// -Rh     limit output to files synced to the client workspace
		/// </summary>
		Synced = 0x0100,
		/// <summary>
		/// -Rn     limit output to files opened not at the head revision
		/// </summary>
		NotHeadRev = 0x0200,
		/// <summary>
		/// -Ro     limit output to files opened
		/// </summary>
		Opened = 0x0400,
		/// <summary>
		/// -Rr     limit output to files opened that have been resolved
		/// </summary>
		Resolved = 0x0800,
		/// <summary>
		/// -Rs     limit output to files shelved (requires -e)
		/// </summary>
		Shelved = 0x1000,
		/// <summary>
		/// -Ru     limit output to files opened that need resolving
		/// </summary>
		NeedsResolve = 0x2000,
		/// <summary>
		/// -St     sort by filetype
		/// </summary>
		FileTypeSort = 0x4000,
		/// <summary>
		/// -Sd     sort by date
		/// </summary>
		DateSort = 0x8000,
		/// <summary>
		/// -Sr     sort by head revision
		/// </summary>
		HeadRevSort = 0x10000,
		/// <summary>
		/// -Sh     sort by have revision
		/// </summary>
		HaveRevSort = 0x20000,
		/// <summary>
		/// -Ss     sort by filesize
		/// </summary>
		FileSizeSort = 0x40000
	}

	/// <summary>
	/// Options for the fstat command
	/// </summary>
	public partial class Options
	{
		/// <summary>
		/// Options for the fstat command
		/// </summary>
		/// <remarks>
		/// <br/><b>p4 help fstat</b>
		/// <br/> 
		/// <br/>     fstat -- Dump file info
		/// <br/> 
		/// <br/>     p4 fstat [-F filter -L -T fields -m max -r] [-c | -e changelist#]
		/// <br/> 	[-Ox -Rx -Sx] file[rev] ...
		/// <br/> 
		/// <br/> 	Fstat lists information about files, one line per file.  Fstat is
		/// <br/> 	intended for use in Perforce API applications, where the output can
		/// <br/> 	be accessed as variables, but its output is also suitable for parsing
		/// <br/> 	from the client command output in scripts.
		/// <br/> 
		/// <br/> 	The fields that fstat displays are:
		/// <br/> 
		/// <br/> 		clientFile           -- local path (host or Perforce syntax)
		/// <br/> 		depotFile            -- name in depot
		/// <br/> 		movedFile            -- name in depot of moved to/from file
		/// <br/> 		path                 -- local path (host syntax)
		/// <br/> 		isMapped             -- set if mapped client file is synced
		/// <br/> 		shelved              -- set if file is shelved
		/// <br/> 		headAction           -- action at head rev, if in depot
		/// <br/> 		headChange           -- head rev changelist#, if in depot
		/// <br/> 		headRev              -- head rev #, if in depot
		/// <br/> 		headType             -- head rev type, if in depot
		/// <br/> 		headTime             -- head rev changelist time, if in depot
		/// <br/> 		headModTime	     -- head rev mod time, if in depot
		/// <br/> 		movedRev             -- head rev # of moved file
		/// <br/> 		haveRev              -- rev had on client, if on client
		/// <br/> 		desc                 -- change description
		/// <br/> 		digest               -- MD5 digest (fingerprint)
		/// <br/> 		fileSize             -- file size
		/// <br/> 		action               -- open action, if opened
		/// <br/> 		type                 -- open type, if opened
		/// <br/> 		actionOwner          -- user who opened file, if opened
		/// <br/> 		change               -- open changelist#, if opened
		/// <br/> 		resolved             -- resolved integration records
		/// <br/> 		unresolved           -- unresolved integration records
		/// <br/> 		otherOpen            -- set if someone else has it open
		/// <br/> 		otherOpen#           -- list of user@client with file opened
		/// <br/> 		otherLock            -- set if someone else has it locked
		/// <br/> 		otherLock#           -- user@client with file locked
		/// <br/> 		otherAction#         -- open action, if opened by someone else
		/// <br/> 		otherChange#         -- changelist, if opened by someone else
		/// <br/> 		ourLock              -- set if this user/client has it locked
		/// <br/> 		resolveAction#       -- pending integration record action
		/// <br/> 		resolveBaseFile#     -- pending integration base file
		/// <br/> 		resolveBaseRev#      -- pending integration base rev
		/// <br/> 		resolveFromFile#     -- pending integration from file
		/// <br/> 		resolveStartFromRev# -- pending integration from start rev
		/// <br/> 		resolveEndFromRev#   -- pending integration from end rev
		/// <br/> 
		/// <br/> 	The -F flag lists only files satisfying the filter expression. This
		/// <br/> 	filter syntax is similar to the one used for 'jobs -e jobview' and is
		/// <br/> 	used to evaluate the contents of the fields in the preceding list.
		/// <br/> 	Filtering is case-sensitive.
		/// <br/> 
		/// <br/> 	        Example: -Ol -F "fileSize &gt; 1000000 & headType=text"
		/// <br/> 
		/// <br/> 	Note: filtering is not optimized with indexes for performance.
		/// <br/> 
		/// <br/> 	The -L flag can be used with multiple file arguments that are in
		/// <br/> 	full depot syntax and include a valid revision number. When this
		/// <br/> 	flag is used the arguments are processed together by building an
		/// <br/> 	internal table similar to a label. This file list processing is
		/// <br/> 	significantly faster than having to call the internal query engine
		/// <br/> 	for each individual file argument. However, the file argument syntax
		/// <br/> 	is strict and the command will not run if an error is encountered.
		/// <br/> 
		/// <br/> 	The -T fields flag returns only the specified fields. The field names
		/// <br/> 	can be specified using a comma- or space-delimited list.
		/// <br/> 
		/// <br/> 	        Example: -Ol -T "depotFile, fileSize"
		/// <br/> 
		/// <br/> 	The -m max flag limits output to the specified number of files.
		/// <br/> 
		/// <br/> 	The -r flag sorts the output in reverse order.
		/// <br/> 
		/// <br/> 	The -c changelist# flag displays files	modified after the specified
		/// <br/> 	changelist was submitted.  This operation is much faster than using
		/// <br/> 	a revision range on the affected files.
		/// <br/> 
		/// <br/> 	The -e changelist# flag lists files modified by the specified
		/// <br/> 	changelist. When used with the -Ro flag, only pending changes are
		/// <br/> 	considered, to ensure that files opened for add are included. This
		/// <br/> 	option also displays the change description.
		/// <br/> 
		/// <br/> 	The -O options modify the output as follows:
		/// <br/> 
		/// <br/> 	        -Of     output all revisions for the given files (this
		/// <br/> 			option suppresses other* and resolve* fields)
		/// <br/> 
		/// <br/> 	        -Ol     output a fileSize and digest field for each revision
		/// <br/> 			(this may be expensive to compute)
		/// <br/> 
		/// <br/> 		-Op	output the local file path in both Perforce syntax
		/// <br/> 			(//client/) as 'clientFile' and host form as 'path'
		/// <br/> 
		/// <br/> 	        -Or     output pending integration record information for
		/// <br/> 	                files opened on the current client, or if used with
		/// <br/>                        '-e &lt;change&gt; -Rs', on the shelved change
		/// <br/> 
		/// <br/> 		-Os	exclude client-related data from output
		/// <br/> 
		/// <br/> 	The -R option limits output to specific files:
		/// <br/> 
		/// <br/> 		-Rc	files mapped in the client view
		/// <br/> 		-Rh	files synced to the client workspace
		/// <br/> 	        -Rn     files opened not at the head revision
		/// <br/> 		-Ro	files opened
		/// <br/> 	        -Rr     files opened that have been resolved
		/// <br/> 		-Rs	files shelved (requires -e)
		/// <br/> 	        -Ru     files opened that need resolving
		/// <br/> 
		/// <br/> 	The -S option changes the order of output:
		/// <br/> 
		/// <br/> 		-St	sort by filetype
		/// <br/> 		-Sd	sort by date
		/// <br/> 		-Sr	sort by head revision
		/// <br/> 		-Sh	sort by have revision
		/// <br/> 		-Ss	sort by filesize
		/// <br/> 
		/// <br/> 	For compatibility, the following flags are also supported:
		/// <br/> 	-C (-Rc) -H (-Rh) -W (-Ro) -P (-Op) -l (-Ol) -s (-Os).
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(GetFileMetadataCmdFlags flags, string filter, string taggedFields, int maxItems, string afterChangelist, string byChangelist)
		{
			if ((flags & GetFileMetadataCmdFlags.MultiFileArgs) != 0)
			{
				this["-L"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.Reverse) != 0)
			{
				this["-r"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.AllRevisions) != 0)
			{
				this["-Of"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.FileSize) != 0)
			{
				this["-Ol"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.LocalPath) != 0)
			{
				this["-Op"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.PendingInteg) != 0)
			{
				this["-Or"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.ExcludeClientData) != 0)
			{
				this["Os"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.ClientMapped) != 0)
			{
				this["-Rc"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.Synced) != 0)
			{
				this["-Rh"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.NotHeadRev) != 0)
			{
				this["-Rn"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.Opened) != 0)
			{
				this["-Ro"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.Resolved) != 0)
			{
				this["-Rr"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.Shelved) != 0)
			{
				this["-Rs"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.NeedsResolve) != 0)
			{
				this["-Ru"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.FileTypeSort) != 0)
			{
				this["-St"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.DateSort) != 0)
			{
				this["-Sd"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.HeadRevSort) != 0)
			{
				this["-Sr"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.HaveRevSort) != 0)
			{
				this["-Sh"] = null;
			}

			if ((flags & GetFileMetadataCmdFlags.FileSizeSort) != 0)
			{
				this["-Ss"] = null;
			}

			if (String.IsNullOrEmpty(filter) != true)
			{
				this["-F"] = filter;
			}

			if (String.IsNullOrEmpty(taggedFields) != true)
			{
				this["-T"] = taggedFields;
			}

			if (maxItems > 0)
			{
				this["-m"] = maxItems.ToString();
			}

			if (String.IsNullOrEmpty(afterChangelist) != true)
			{
				this["-c"] = afterChangelist;
			}

			if (String.IsNullOrEmpty(byChangelist) != true)
			{
				this["-e"] = byChangelist;
			}

		}
	}

	/// <summary>
	/// GetFileMetaData options (uses the fstat command)
	/// </summary>
	public class GetFileMetaDataCmdOptions : Options
	{
		/// <summary>
		/// Options for GetFileMetaData (uses the fstat command)
		/// </summary>
		/// <remarks>
		/// <br/><b>p4 help fstat</b>
		/// <br/> 
		/// <br/>     fstat -- Dump file info
		/// <br/> 
		/// <br/>     p4 fstat [-F filter -L -T fields -m max -r] [-c | -e changelist#]
		/// <br/> 	[-Ox -Rx -Sx] file[rev] ...
		/// <br/> 
		/// <br/> 	Fstat lists information about files, one line per file.  Fstat is
		/// <br/> 	intended for use in Perforce API applications, where the output can
		/// <br/> 	be accessed as variables, but its output is also suitable for parsing
		/// <br/> 	from the client command output in scripts.
		/// <br/> 
		/// <br/> 	The fields that fstat displays are:
		/// <br/> 
		/// <br/> 		clientFile           -- local path (host or Perforce syntax)
		/// <br/> 		depotFile            -- name in depot
		/// <br/> 		movedFile            -- name in depot of moved to/from file
		/// <br/> 		path                 -- local path (host syntax)
		/// <br/> 		isMapped             -- set if mapped client file is synced
		/// <br/> 		shelved              -- set if file is shelved
		/// <br/> 		headAction           -- action at head rev, if in depot
		/// <br/> 		headChange           -- head rev changelist#, if in depot
		/// <br/> 		headRev              -- head rev #, if in depot
		/// <br/> 		headType             -- head rev type, if in depot
		/// <br/> 		headTime             -- head rev changelist time, if in depot
		/// <br/> 		headModTime	     -- head rev mod time, if in depot
		/// <br/> 		movedRev             -- head rev # of moved file
		/// <br/> 		haveRev              -- rev had on client, if on client
		/// <br/> 		desc                 -- change description
		/// <br/> 		digest               -- MD5 digest (fingerprint)
		/// <br/> 		fileSize             -- file size
		/// <br/> 		action               -- open action, if opened
		/// <br/> 		type                 -- open type, if opened
		/// <br/> 		actionOwner          -- user who opened file, if opened
		/// <br/> 		change               -- open changelist#, if opened
		/// <br/> 		resolved             -- resolved integration records
		/// <br/> 		unresolved           -- unresolved integration records
		/// <br/> 		otherOpen            -- set if someone else has it open
		/// <br/> 		otherOpen#           -- list of user@client with file opened
		/// <br/> 		otherLock            -- set if someone else has it locked
		/// <br/> 		otherLock#           -- user@client with file locked
		/// <br/> 		otherAction#         -- open action, if opened by someone else
		/// <br/> 		otherChange#         -- changelist, if opened by someone else
		/// <br/> 		ourLock              -- set if this user/client has it locked
		/// <br/> 		resolveAction#       -- pending integration record action
		/// <br/> 		resolveBaseFile#     -- pending integration base file
		/// <br/> 		resolveBaseRev#      -- pending integration base rev
		/// <br/> 		resolveFromFile#     -- pending integration from file
		/// <br/> 		resolveStartFromRev# -- pending integration from start rev
		/// <br/> 		resolveEndFromRev#   -- pending integration from end rev
		/// <br/> 
		/// <br/> 	The -F flag lists only files satisfying the filter expression. This
		/// <br/> 	filter syntax is similar to the one used for 'jobs -e jobview' and is
		/// <br/> 	used to evaluate the contents of the fields in the preceding list.
		/// <br/> 	Filtering is case-sensitive.
		/// <br/> 
		/// <br/> 	        Example: -Ol -F "fileSize &gt; 1000000 & headType=text"
		/// <br/> 
		/// <br/> 	Note: filtering is not optimized with indexes for performance.
		/// <br/> 
		/// <br/> 	The -L flag can be used with multiple file arguments that are in
		/// <br/> 	full depot syntax and include a valid revision number. When this
		/// <br/> 	flag is used the arguments are processed together by building an
		/// <br/> 	internal table similar to a label. This file list processing is
		/// <br/> 	significantly faster than having to call the internal query engine
		/// <br/> 	for each individual file argument. However, the file argument syntax
		/// <br/> 	is strict and the command will not run if an error is encountered.
		/// <br/> 
		/// <br/> 	The -T fields flag returns only the specified fields. The field names
		/// <br/> 	can be specified using a comma- or space-delimited list.
		/// <br/> 
		/// <br/> 	        Example: -Ol -T "depotFile, fileSize"
		/// <br/> 
		/// <br/> 	The -m max flag limits output to the specified number of files.
		/// <br/> 
		/// <br/> 	The -r flag sorts the output in reverse order.
		/// <br/> 
		/// <br/> 	The -c changelist# flag displays files	modified after the specified
		/// <br/> 	changelist was submitted.  This operation is much faster than using
		/// <br/> 	a revision range on the affected files.
		/// <br/> 
		/// <br/> 	The -e changelist# flag lists files modified by the specified
		/// <br/> 	changelist. When used with the -Ro flag, only pending changes are
		/// <br/> 	considered, to ensure that files opened for add are included. This
		/// <br/> 	option also displays the change description.
		/// <br/> 
		/// <br/> 	The -O options modify the output as follows:
		/// <br/> 
		/// <br/> 	        -Of     output all revisions for the given files (this
		/// <br/> 			option suppresses other* and resolve* fields)
		/// <br/> 
		/// <br/> 	        -Ol     output a fileSize and digest field for each revision
		/// <br/> 			(this may be expensive to compute)
		/// <br/> 
		/// <br/> 		-Op	output the local file path in both Perforce syntax
		/// <br/> 			(//client/) as 'clientFile' and host form as 'path'
		/// <br/> 
		/// <br/> 	        -Or     output pending integration record information for
		/// <br/> 	                files opened on the current client, or if used with
		/// <br/>                        '-e &lt;change&gt; -Rs', on the shelved change
		/// <br/> 
		/// <br/> 		-Os	exclude client-related data from output
		/// <br/> 
		/// <br/> 	The -R option limits output to specific files:
		/// <br/> 
		/// <br/> 		-Rc	files mapped in the client view
		/// <br/> 		-Rh	files synced to the client workspace
		/// <br/> 	        -Rn     files opened not at the head revision
		/// <br/> 		-Ro	files opened
		/// <br/> 	        -Rr     files opened that have been resolved
		/// <br/> 		-Rs	files shelved (requires -e)
		/// <br/> 	        -Ru     files opened that need resolving
		/// <br/> 
		/// <br/> 	The -S option changes the order of output:
		/// <br/> 
		/// <br/> 		-St	sort by filetype
		/// <br/> 		-Sd	sort by date
		/// <br/> 		-Sr	sort by head revision
		/// <br/> 		-Sh	sort by have revision
		/// <br/> 		-Ss	sort by filesize
		/// <br/> 
		/// <br/> 	For compatibility, the following flags are also supported:
		/// <br/> 	-C (-Rc) -H (-Rh) -W (-Ro) -P (-Op) -l (-Ol) -s (-Os).
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public GetFileMetaDataCmdOptions(GetFileMetadataCmdFlags flags, string filter, string taggedFields, int maxItems, string afterChangelist, string byChangelist)
			: base (flags,  filter, taggedFields, maxItems, afterChangelist, byChangelist) {}
	}
	/// <summary>
	/// Flags for the dirs command.
	/// </summary>
	[Flags]
	public enum GetDepotDirsCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -C flag lists only directories that fall within the
		/// 	current client view.
		/// </summary>
		CurrentClientOnly = 0x0001,
		/// <summary>
		/// 	The -D flag includes directories containing only deleted
		/// 	files.
		/// </summary>
		IncludeDeletedFilesDirs = 0x0002,
		/// <summary>
		/// 	The -H flag lists directories containing files synced to
		/// 	the current client workspace.
		/// </summary>
		SyncedDirs = 0x0004
	}

	/// <summary>
	/// Options for the dirs command
	/// </summary>
	public partial class Options
	{
		public Options(GetDepotDirsCmdFlags flags, string stream)
		{
			if ((flags & GetDepotDirsCmdFlags.CurrentClientOnly) != 0)
			{
				this["-C"] = null;
			}

			if ((flags & GetDepotDirsCmdFlags.IncludeDeletedFilesDirs) != 0)
			{
				this["-D"] = null;
			}

			if ((flags & GetDepotDirsCmdFlags.SyncedDirs) != 0)
			{
				this["-H"] = null;
			}

			if (String.IsNullOrEmpty(stream) != true)
			{
				this["-S"] = stream;
			}

		}
	}
	/// <summary>
	/// Dirs command options
	/// </summary>
	public class GetDepotDirsCmdOptions : Options
	{
		public GetDepotDirsCmdOptions(GetDepotDirsCmdFlags flags, string stream)
			:base(flags,stream) {}
	}
	/// <summary>
	/// Flags for the print command.
	/// </summary>
	[Flags]
	public enum GetFileContentsCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -C flag lists only directories that fall within the
		/// 	current client view.
		/// </summary>
		AllRevisions = 0x0001,
		/// <summary>
		/// 	The -D flag includes directories containing only deleted
		/// 	files.
		/// </summary>
		Supress = 0x0002
	}

	/// <summary>
	/// Options for the print command
	/// </summary>
	public partial class Options
	{
		/// <summary>
		/// Command options for GetFileContentsCmd() 
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="localFile"></param>
		/// <remarks>
		/// <br/><b>p4 help print</b>
		/// <br/> 
		/// <br/>     print -- Write a depot file to standard output
		/// <br/> 
		/// <br/>     p4 print [-a -o localFile -q] file[revRange] ...
		/// <br/> 
		/// <br/> 	Retrieve the contents of a depot file to the client's standard output.
		/// <br/> 	The file is not synced.  If file is specified using client syntax,
		/// <br/> 	Perforce uses the client view to determine the corresponding depot
		/// <br/> 	file.
		/// <br/> 
		/// <br/> 	By default, the head revision is printed.  If the file argument
		/// <br/> 	includes a revision, the specified revision is printed.  If the
		/// <br/> 	file argument has a revision range,  then only files selected by
		/// <br/> 	that revision range are printed, and the highest revision in the
		/// <br/> 	range is printed. For details about revision specifiers, see 'p4
		/// <br/> 	help revisions'.
		/// <br/> 
		/// <br/> 	The -a flag prints all revisions within the specified range, rather
		/// <br/> 	than just the highest revision in the range.
		/// <br/> 
		/// <br/> 	The -o localFile flag redirects the output to the specified file on
		/// <br/> 	the client filesystem.
		/// <br/> 
		/// <br/> 	The -q flag suppresses the initial line that displays the file name
		/// <br/> 	and revision.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(GetFileContentsCmdFlags flags, string localFile)
		{
			if ((flags & GetFileContentsCmdFlags.AllRevisions) != 0)
			{
				this["-a"] = null;
			}

			if ((flags & GetFileContentsCmdFlags.Supress) != 0)
			{
				this["-q"] = null;
			}

			if (String.IsNullOrEmpty(localFile) != true)
			{
				this["-o"] = localFile;
			}

		}
	}

	/// <summary>
	/// Print command options
	/// </summary>
	public class GetFileContentsCmdOptions : Options
	{
		/// <summary>
		/// Command options for GetFileContentsCmd() 
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="localFile"></param>
		/// <remarks>
		/// <br/><b>p4 help print</b>
		/// <br/> 
		/// <br/>     print -- Write a depot file to standard output
		/// <br/> 
		/// <br/>     p4 print [-a -o localFile -q] file[revRange] ...
		/// <br/> 
		/// <br/> 	Retrieve the contents of a depot file to the client's standard output.
		/// <br/> 	The file is not synced.  If file is specified using client syntax,
		/// <br/> 	Perforce uses the client view to determine the corresponding depot
		/// <br/> 	file.
		/// <br/> 
		/// <br/> 	By default, the head revision is printed.  If the file argument
		/// <br/> 	includes a revision, the specified revision is printed.  If the
		/// <br/> 	file argument has a revision range,  then only files selected by
		/// <br/> 	that revision range are printed, and the highest revision in the
		/// <br/> 	range is printed. For details about revision specifiers, see 'p4
		/// <br/> 	help revisions'.
		/// <br/> 
		/// <br/> 	The -a flag prints all revisions within the specified range, rather
		/// <br/> 	than just the highest revision in the range.
		/// <br/> 
		/// <br/> 	The -o localFile flag redirects the output to the specified file on
		/// <br/> 	the client filesystem.
		/// <br/> 
		/// <br/> 	The -q flag suppresses the initial line that displays the file name
		/// <br/> 	and revision.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public GetFileContentsCmdOptions(GetFileContentsCmdFlags flags, string localFile)
			: base(flags, localFile) {}
	}

	/// <summary>
	/// Flags for the filelog command.
	/// </summary>
	[Flags]
	public enum GetFileHistoryCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -i flag includes inherited file history. If a file was created by
		/// 	branching (using 'p4 integrate'), filelog lists the revisions of the
		/// 	file's ancestors up to the branch points that led to the specified
		/// 	revision.  File history inherited by renaming (using 'p4 move') is
		/// 	always displayed regardless of whether -i is specified.
		/// </summary>
		IncludeInherited = 0x0001,
		/// <summary>
		/// 	The -h flag displays file content history instead of file name
		/// 	history.  The list includes revisions of other files that were
		/// 	branched or copied (using 'p4 integrate' and 'p4 resolve -at') to
		/// 	the specified revision.  Revisions that were replaced by copying
		/// 	or branching are omitted, even if they are part of the history of
		/// 	the specified revision.
		/// </summary>
		ContentHistory = 0x0002,
		/// <summary>
		/// 	The -t flag displays the time as well as the date.
		/// </summary>
		Time = 0x0004,
		/// <summary>
		/// 	The -l flag lists the full text of the changelist descriptions.
		/// </summary>
		FullDescription = 0x0008,
		/// <summary>
		/// 	The -L flag lists the full text of the changelist descriptions,
		/// 	truncated to 250 characters if longer.
		/// </summary>
		TruncatedDescription = 0x0010,
		/// <summary>
		/// 	The -s flag displays a shortened form of filelog that omits
		/// 	non-contributory integrations.
		/// </summary>
		Shortened = 0x0020
	}

	/// <summary>
	/// Options for the filelog command
	/// </summary>
	public partial class Options
	{
		public Options(GetFileHistoryCmdFlags flags, int changeList, int maxRevs)
		{
			if ((flags & GetFileHistoryCmdFlags.IncludeInherited) != 0)
			{
				this["-i"] = null;
			}

			if ((flags & GetFileHistoryCmdFlags.ContentHistory) != 0)
			{
				this["-h"] = null;
			}

			if ((flags & GetFileHistoryCmdFlags.Time) != 0)
			{
				this["-t"] = null;
			}

			if ((flags & GetFileHistoryCmdFlags.FullDescription) != 0)
			{
				this["-l"] = null;
			}

			if ((flags & GetFileHistoryCmdFlags.TruncatedDescription) != 0)
			{
				this["-L"] = null;
			}

			if ((flags & GetFileHistoryCmdFlags.Shortened) != 0)
			{
				this["-s"] = null;
			}

			if (changeList > 0)
			{
				this["-c"] = changeList.ToString();
			}

			if (maxRevs > 0)
			{
				this["-m"] = maxRevs.ToString();
			}
		}
	}
	/// <summary>
	/// Filelog command options
	/// </summary>
	public class GetFileHistoryCmdOptions:Options
	{
		/// <summary>
		/// Filelog command options
		/// </summary>
		public GetFileHistoryCmdOptions(GetFileHistoryCmdFlags flags, int changeList, int maxRevs)
			:base(flags,changeList,maxRevs) {}
	}
	/// <summary>
	/// Flags for the annotate command.
	/// </summary>
	[Flags]
	public enum GetFileAnnotationsCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -a flag includes both deleted files and lines no longer
		/// 	present at the head revision. In the latter case, both the
		/// 	starting and ending revision for each line is displayed.
		/// </summary>
		AllResults = 0x0001,
		/// <summary>
		/// 	The -c flag directs the annotate command to output changelist
		/// 	numbers rather than revision numbers for each line.
		/// </summary>
		UseChangeNumbers = 0x0002,
		/// <summary>
		///     -db Ignore Whitespace Changes
		/// </summary>
		IgnoreWhitespaceChanges = 0x0004,
		/// <summary>
		///     -dw Ingore whitespace altogether.
		/// </summary>
		IgnoreWhitespace = 0x0008,
		/// <summary>
		///     -dl Ignore Line Endings
		/// </summary>
		IgnoreLineEndings = 0x0010,
		/// <summary>
		/// 	The -i flag follows branches.  If a file was created by
		/// 	branching, 'p4 annotate' includes the revisions of the
		/// 	source file up to the branch point, just as 'p4 filelog -i'
		/// 	does.  If a file has history prior to being created by
		/// 	branching (such as a file that was branched on top of a
		/// 	deleted file), -i ignores those prior revisions and follows
		/// 	the source.  -i implies -c.
		/// </summary>
		FollowBranches = 0x0020,
		/// <summary>
		/// 	The -I flag follows all integrations into the file.  If a
		/// 	line was introduced into the file by a merge, the source of
		/// 	the merge is displayed as the changelist that introduced the
		/// 	line. If the source itself was the result of an integration,
		/// 	that source is used instead, and so on.  -I implies -c.
		/// </summary>
		FollowIntegrations = 0x0040,
		/// <summary>
		/// 	The -q flag suppresses the one-line header that is displayed
		/// 	by default for each file.
		/// </summary>
		Supress = 0x0080
	}

	/// <summary>
	/// Options for the annotate command
	/// </summary>
	public partial class Options
	{
		public Options(GetFileAnnotationsCmdFlags flags, string localFile)
		{
			if ((flags & GetFileAnnotationsCmdFlags.AllResults) != 0)
			{
				this["-a"] = null;
			}

			if ((flags & GetFileAnnotationsCmdFlags.UseChangeNumbers) != 0)
			{
				this["-c"] = null;
			}

			if ((flags & GetFileAnnotationsCmdFlags.IgnoreWhitespaceChanges) != 0)
			{
				this["-db"] = null;
			}

			if ((flags & GetFileAnnotationsCmdFlags.IgnoreWhitespace) != 0)
			{
				this["-dw"] = null;
			}

			if ((flags & GetFileAnnotationsCmdFlags.IgnoreLineEndings) != 0)
			{
				this["-dl"] = null;
			}

			if ((flags & GetFileAnnotationsCmdFlags.FollowBranches) != 0)
			{
				this["-i"] = null;
			}

			if ((flags & GetFileAnnotationsCmdFlags.FollowIntegrations) != 0)
			{
				this["-I"] = null;
			}

			if ((flags & GetFileAnnotationsCmdFlags.Supress) != 0)
			{
				this["-q"] = null;
			}

		}
	}
	/// <summary>
	/// Annotate command options
	/// </summary>
	public class GetFileAnnotationsCmdOptions: Options
	{
		/// <summary>
		/// Annotate command options
		/// </summary>
		public GetFileAnnotationsCmdOptions(GetFileAnnotationsCmdFlags flags, string localFile)
			:base(flags,localFile) {}
	}
	/// <summary>
	/// Flags for the fixes command.
	/// </summary>
	[Flags]
	public enum GetFixesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -C flag lists only directories that fall within the
		/// 	current client view.
		/// </summary>
		IncludeIntegrations = 0x0001
	}

	/// <summary>
	/// Options for the fixes command
	/// </summary>
	public partial class Options
	{
		public Options(GetFixesCmdFlags flags, int changelistId, string jobId, int maxFixes)
		{
			if ((flags & GetFixesCmdFlags.IncludeIntegrations) != 0)
			{
				this["-i"] = null;
			}

			if (changelistId > 0)
			{
				this["-c"] = changelistId.ToString();
			}

			if (String.IsNullOrEmpty(jobId) != true)
			{
				this["-j"] = jobId;
			}

			if (maxFixes > 0)
			{
				this["-m"] = maxFixes.ToString();
			}


		}
	}
	/// <summary>
	/// Fixes command options
	/// </summary>
	public class GetFixesCmdOptions : Options
	{
	/// <summary>
	/// Fixes command options
	/// </summary>
	public GetFixesCmdOptions(GetFixesCmdFlags flags, int changelistId, string jobId, int maxFixes)
		:base(flags, changelistId, jobId,maxFixes) {}
	}
	/// <summary>
	/// Flags for the grep command.
	/// </summary>
	[Flags]
	public enum GetFileLineMatchesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -a flag searches all revisions within the specified
		/// 	range. By default only the highest revision in the range
		/// 	is searched.
		/// </summary>
		AllRevisions = 0x0001,
		/// <summary>
		/// 	The -i flag causes the pattern matching to be case-insensitive.
		/// 	By default, matching is case-sensitive.
		/// </summary>
		CaseInsensitive = 0x0002,
		/// <summary>
		/// 	The -n flag displays the matching line number after the file
		/// 	revision number. By default, matches are displayed as
		/// 	revision#: &lt;text&gt;.
		/// </summary>
		IncludeLineNumbers = 0x0004,
		/// <summary>
		/// 	The -v flag displays files with non-matching lines.
		/// </summary>
		NonMatchingLines = 0x0008,
		/// <summary>
		/// 	The -t flag searches binary files.  By default, only text files
		/// 	are searched.
		/// </summary>
		SearchBinaries = 0x0010,
		/// <summary>
		/// 	The -L flag displays the name of each selected file from which no
		/// 	output would normally have been displayed. Scanning stops on the
		/// 	first match.
		/// </summary>
		NameNoOutput = 0x0020,
		/// <summary>
		/// 	The -l flag display the name of each selected file containing
		/// 	 matching text. Scanning stops on the first match.
		/// </summary>
		NameMatchingText = 0x0040,
		/// <summary>
		/// 	The -s flag suppresses error messages that result from abandoning
		/// 	files that have a maximum number of characters in a single line that
		/// 	are greater than 4096.  By default, an error is reported when grep
		/// 	abandons such files.
		/// </summary>
		Supress = 0x0080,
		/// <summary>
		/// 	The -F flag is used to interpret the pattern as a fixed string.
		/// </summary>
		FixedPattern = 0x0100,
		/// <summary>
		/// 	The -G flag is used to interpret the pattern as a regular expression.
		/// </summary>
		RegularExpression = 0x0200
	}

	/// <summary>
	/// Options for the grep command
	/// </summary>
	public partial class Options
	{
		public Options(GetFileLineMatchesCmdFlags flags, int outputContext,
			int trailingContext, int leadingContext)
		{
			if ((flags & GetFileLineMatchesCmdFlags.AllRevisions) != 0)
			{
				this["-a"] = null;
			}

			if ((flags & GetFileLineMatchesCmdFlags.CaseInsensitive) != 0)
			{
				this["-i"] = null;
			}

			if ((flags & GetFileLineMatchesCmdFlags.IncludeLineNumbers) != 0)
			{
				this["-n"] = null;
			}

			if ((flags & GetFileLineMatchesCmdFlags.NonMatchingLines) != 0)
			{
				this["-v"] = null;
			}

			if ((flags & GetFileLineMatchesCmdFlags.FixedPattern) != 0)
			{
				this["-F"] = null;
			}

			if ((flags & GetFileLineMatchesCmdFlags.RegularExpression) != 0)
			{
				this["-G"] = null;
			}

			if ((flags & GetFileLineMatchesCmdFlags.NameNoOutput) != 0)
			{
				this["-L"] = null;
			}

			if ((flags & GetFileLineMatchesCmdFlags.NameMatchingText) != 0)
			{
				this["-l"] = null;
			}

			if ((flags & GetFileLineMatchesCmdFlags.SearchBinaries) != 0)
			{
				this["-t"] = null;
			}

			if ((flags & GetFileLineMatchesCmdFlags.Supress) != 0)
			{
				this["-s"] = null;
			}

			if (trailingContext > 0)
			{
				this["-A"] = trailingContext.ToString();
			}

			if (leadingContext > 0)
			{
				this["-B"] = leadingContext.ToString();
			}

			if (outputContext > 0)
			{
				this["-C"] = outputContext.ToString();
			}


		}
	}
	/// <summary>
	/// Grep command options
	/// </summary>
	public class GetFileLineMatchesCmdOptions:Options
	{
		/// <summary>
		/// Grep command options
		/// </summary>
		public GetFileLineMatchesCmdOptions(GetFileLineMatchesCmdFlags flags, int outputContext,
			int trailingContext, int leadingContext)
			:base(flags,outputContext,trailingContext,leadingContext) {}
	}
	/// <summary>
	/// Flags for the integrated command.
	/// </summary>
	[Flags]
	public enum GetSubmittedIntegrationsCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -r flag reverses the mappings in the branch view, swapping the
		/// 	target files and source files.  The -b branch flag is required.
		/// </summary>
		ReverseMappings = 0x0001
	}

	/// <summary>
	/// Options for the integrated command
	/// </summary>
	public partial class Options
	{
		public Options(GetSubmittedIntegrationsCmdFlags flags, string branchSpec)
		{
			if ((flags & GetSubmittedIntegrationsCmdFlags.ReverseMappings) != 0)
			{
				this["-r"] = null;
			}

			if (String.IsNullOrEmpty(branchSpec) != true)
			{
				this["-b"] = branchSpec;
			}

		}
	}
	/// <summary>
	/// Integrated command options
	/// </summary>
	public class GetSubmittedIntegrationsCmdOptions:Options
	{
		/// <summary>
		/// Integrated command options
		/// </summary>
		public GetSubmittedIntegrationsCmdOptions(GetSubmittedIntegrationsCmdFlags flags, string branchSpec)
			:base(flags,branchSpec) {}
	}
	/// <summary>
	/// Flags for the protects command.
	/// </summary>
	[Flags]
	public enum GetProtectionEntriesCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	If the -a flag is specified, protection lines for all users are
		/// 	displayed.  If the -g group flag or -u user flag is specified,
		/// 	protection lines for that group or user are displayed.
		/// </summary>
		AllUsers = 0x0001,
		/// <summary>
		/// 	If the -m flag is given, a single word summary of the maximum
		/// 	access level is reported. Note that this summary does not take
		/// 	exclusions into account.
		/// </summary>
		AccessSummary = 0x0002
	}

	/// <summary>
	/// Options for the protects command
	/// </summary>
	public partial class Options
	{
		public Options(GetProtectionEntriesCmdFlags flags, string groupName,
			string userName, string hostName)
		{
			if ((flags & GetProtectionEntriesCmdFlags.AllUsers) != 0)
			{
				this["-a"] = null;
			}

			if ((flags & GetProtectionEntriesCmdFlags.AccessSummary) != 0)
			{
				this["-m"] = null;
			}

			if (String.IsNullOrEmpty(groupName) != true)
			{
				this["-g"] = groupName;
			}

			if (String.IsNullOrEmpty(userName) != true)
			{
				this["-u"] = userName;
			}

			if (String.IsNullOrEmpty(hostName) != true)
			{
				this["-h"] = hostName;
			}

		}
	}
	/// <summary>
	/// Protects command options
	/// </summary>
	public class GetProtectionEntriesCmdOptions:Options
	{
	/// <summary>
	/// Protects command options
	/// </summary>
	public GetProtectionEntriesCmdOptions(GetProtectionEntriesCmdFlags flags, string groupName,
			string userName, string hostName)
		:base(flags,groupName,userName,hostName) {}
	}
	/// <summary>
	/// Flags for the reviews command.
	/// </summary>
	[Flags]
	public enum GetReviewersCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000
	}

	/// <summary>
	/// Options for the reviews command
	/// </summary>
	public partial class Options
	{
		public Options(GetReviewersCmdFlags flags, int changelistId)
		{
			if (changelistId >= 0)
			{
				this["-c"] = changelistId.ToString();
			}
		}
	}
	/// <summary>
	/// Reviews command options
	/// </summary>
	public class GetReviewersCmdOptions:Options
	{
	/// <summary>
	/// Reviews command options
	/// </summary>
	public GetReviewersCmdOptions(GetReviewersCmdFlags flags, int changelistId)
		:base(flags,changelistId){}
	}
	/// <summary>
	/// Flags for the triggers command.
	/// </summary>
	[Flags]
	public enum GetTriggerTableCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// The -o flag writes the trigger table to the standard output.
		/// The user's editor is not invoked.
		/// </summary>
		Output = 0x0001,
		/// <summary>
		/// The -i flag writes the trigger table from the standard input.
		/// The user's editor is not invoked.
		/// </summary>
		Input = 0x0002
	}

	/// <summary>
	/// Options for the triggers command
	/// </summary>
	public partial class Options
	{
		public Options(GetTriggerTableCmdFlags flags)
		{
			if ((flags & GetTriggerTableCmdFlags.Output) != 0)
			{
				this["-o"] = null;
			}

			if ((flags & GetTriggerTableCmdFlags.Input) != 0)
			{
				this["-i"] = null;
			}

		}
	}
	/// <summary>
	/// Triggers command options
	/// </summary>
	public class GetTriggerTableCmdOptions:Options
	{
	/// <summary>
	/// Triggers command options
	/// </summary>
	public GetTriggerTableCmdOptions(GetTriggerTableCmdFlags flags)
		:base(flags) {}
	}
	/// <summary>
	/// Flags for the typemap command.
	/// </summary>
	[Flags]
	public enum GetTypeMapCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// The -o flag writes the typemap table to the standard output.
		/// The user's editor is not invoked.
		/// </summary>
		Output = 0x0001,
		/// <summary>
		/// The -i flag writes the typemap table from the standard input.
		/// The user's editor is not invoked.
		/// </summary>
		Input = 0x0002
	}

	/// <summary>
	/// Options for the typemap command
	/// </summary>
	public partial class Options
	{
		public Options(GetTypeMapCmdFlags flags)
		{
			if ((flags & GetTypeMapCmdFlags.Output) != 0)
			{
				this["-o"] = null;
			}

			if ((flags & GetTypeMapCmdFlags.Input) != 0)
			{
				this["-i"] = null;
			}

		}
	}
	/// <summary>
	/// GetTypeMap command options
	/// </summary>
	public class GetTypeMapCmdOptions:Options
	{
	/// <summary>
	/// GetTypeMap command options
	/// </summary>
	public GetTypeMapCmdOptions(GetTypeMapCmdFlags flags)
		:base(flags) {}
	}
	/// <summary>
	/// Flags for the protect command.
	/// </summary>
	[Flags]
	public enum GetProtectionTableCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// The -o flag writes the protection table to the standard output.
		/// The user's editor is not invoked.
		/// </summary>
		Output = 0x0001,
		/// <summary>
		/// The -i flag writes the protection table from the standard input.
		/// The user's editor is not invoked.
		/// </summary>
		Input = 0x0002
	}

	/// <summary>
	/// Options for the protect command
	/// </summary>
	public partial class Options
	{
		public Options(GetProtectionTableCmdFlags flags)
		{
			if ((flags & GetProtectionTableCmdFlags.Output) != 0)
			{
				this["-o"] = null;
			}

			if ((flags & GetProtectionTableCmdFlags.Input) != 0)
			{
				this["-i"] = null;
			}

		}
	}
	/// <summary>
	/// protect command options
	/// </summary>
	public class GetProtectionTableCmdOptions:Options
	{
	/// <summary>
	/// protect command options
	/// </summary>
	public GetProtectionTableCmdOptions(GetProtectionTableCmdFlags flags)
		:base(flags) {}
	}
	/// <summary>
	/// Flags for the counter command.
	/// </summary>
	[Flags]
	public enum CounterCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// The -f flag sets or deletes counters used by Perforce,  which are
		/// listed by 'p4 help counters'. Important: Never set the 'change'
		/// counter to a value that is lower than its current value.
		/// </summary>
		Set = 0x0001,
		/// <summary>
		/// The -d flag deletes counters used by Perforce,  which are listed
		/// by 'p4 help counters'. Important: Never set the 'change' counter
		/// to a value that is lower than its current value. must be used with
		/// -f.
		/// </summary>
		Delete = 0x0002,
		/// <summary>
		/// The -i flag increments a counter by 1 and returns the new value.
		/// This option is used instead of a value argument and can only be
		/// used with numeric counters.
		/// </summary>
		Increment = 0x0004
	}

	/// <summary>
	/// Options for the counter command
	/// </summary>
	public partial class Options
	{
		public Options(CounterCmdFlags flags)
		{
			if ((flags & CounterCmdFlags.Delete) != 0)
			{
				this["-d"] = null;
				this["-f"] = null;
			}

			if ((flags & CounterCmdFlags.Set) != 0)
			{
				this["-f"] = null;
			}
			
			if ((flags & CounterCmdFlags.Increment) != 0)
			{
				this["-i"] = null;
			}
		}
	}
	/// <summary>
	/// Counter command options
	/// </summary>
	public class CoutnerCmdOptions:Options
	{
	/// <summary>
	/// Counter command options
	/// </summary>
	public CoutnerCmdOptions(CounterCmdFlags flags)
		:base(flags) {}
	}
	/// <summary>
	/// Flags for the describe command.
	/// </summary>
	[Flags]
	public enum DescribeChangelistCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -d flag deletes the specified stream (unless the stream is
		/// 	referenced by child streams or stream clients).
		/// </summary>
		RCS = 0x0001,
		/// <summary>
		/// 	-dn RCS output.
		/// </summary>
		Context = 0x0002,
		/// <summary>
		/// 	-dc[n] context
		/// </summary>
		Summary = 0x0004,
		/// <summary>
		/// 	-dc[n] context
		/// </summary>
		Unified = 0x0008,
		/// <summary>
		/// 	-dc[n] context
		/// </summary>
		IgnoreWhitespaceChanges = 0x0010,
		/// <summary>
		///     -dc[n] context
		/// </summary>
		IgnoreWhitespace = 0x0020,
		/// <summary>
		///     -dc[n] context
		/// </summary>
		IgnoreLineEndings = 0x0040,
		/// <summary>
		///     The -s flag omits the diffs of files that were updated.
		/// </summary>
		Omit = 0x0080,
		/// <summary>
		///     The -S flag lists files that are shelved for the specified changelist
		///     and displays diffs of the files against their previous revision.
		/// </summary>
		Shelved = 0x0100,
		/// <summary>
		///     The -f flag forces display of the descriptions in a restricted
		///     change.  The -f flag requires 'admin' access, which is granted
		///     using 'p4 protect'.
		/// </summary>
		Force = 0x0200,
	}
	public partial class Options
	{
		/// <summary>
		/// Options for the Describe command
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="contextLines"></param>
		/// <param name="unifiedLines"></param>
		/// <remarks>
		/// <br/><b>p4 help describe</b>
		/// <br/> 
		/// <br/>     describe -- Display a changelist description
		/// <br/> 
		/// <br/>     p4 describe [-d&lt;flags&gt; -s -S -f] changelist# ...
		/// <br/> 
		/// <br/> 	Display a changelist description, including the changelist number,
		/// <br/> 	user, client, date of submission, textual description, list of
		/// <br/> 	affected files and diffs of files updated.  Pending changelists
		/// <br/> 	are indicated as 'pending' and file diffs are not displayed.
		/// <br/> 
		/// <br/> 	For restricted changelists, 'no permission' is displayed if the user
		/// <br/> 	is not permitted to view the change (see 'p4 help change'). If a
		/// <br/> 	submitted or shelved change is restricted, the description is hidden
		/// <br/> 	unless the user is the owner of the change or has list permission for
		/// <br/> 	at least one file in the change. To view restricted pending (not
		/// <br/> 	shelved) changes, the user must be the owner of the change.
		/// <br/> 
		/// <br/> 	The -d&lt;flags&gt; passes one or more flags to the built-in diff routine
		/// <br/> 	to modify the output: -dn (RCS), -dc[n] (context), -ds (summary),
		/// <br/> 	-du[n] (unified), -db (ignore whitespace changes), -dw (ignore
		/// <br/> 	whitespace), -dl (ignore line endings). The optional argument to
		/// <br/> 	to -dc specifies number of context lines.
		/// <br/> 
		/// <br/> 	The -s flag omits the diffs of files that were updated.
		/// <br/> 
		/// <br/> 	The -S flag lists files that are shelved for the specified changelist
		/// <br/> 	and displays diffs of the files against their previous revision.
		/// <br/> 
		/// <br/> 	The -f flag forces display of the descriptions in a restricted
		/// <br/> 	change.  The -f flag requires 'admin' access, which is granted
		/// <br/> 	using 'p4 protect'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Options(DescribeChangelistCmdFlags flags,
			int contextLines, int unifiedLines)
		{
			if ((flags & DescribeChangelistCmdFlags.RCS) != 0)
			{
				this["-dn"] = null;
			}

			if (((flags & DescribeChangelistCmdFlags.Context) != 0)
				&& (contextLines >= 0))
			{
				this["-dc"] = contextLines.ToString();
			}

			if ((flags & DescribeChangelistCmdFlags.Summary) != 0)
			{
				this["-ds"] = null;
			}

			if (((flags & DescribeChangelistCmdFlags.Unified) != 0)
				&& (contextLines >= 0))
			{
				this["-du"] = unifiedLines.ToString();
			}

			if ((flags & DescribeChangelistCmdFlags.IgnoreWhitespaceChanges) != 0)
			{
				this["-db"] = null;
			}

			if ((flags & DescribeChangelistCmdFlags.IgnoreWhitespace) != 0)
			{
				this["-dw"] = null;
			}

			if ((flags & DescribeChangelistCmdFlags.IgnoreLineEndings) != 0)
			{
				this["-dl"] = null;
			}

			if ((flags & DescribeChangelistCmdFlags.Omit) != 0)
			{
				this["-s"] = null;
			}

			if ((flags & DescribeChangelistCmdFlags.Shelved) != 0)
			{
				this["-S"] = null;
			}

			if ((flags & DescribeChangelistCmdFlags.Force) != 0)
			{
				this["-f"] = null;
			}

		}
	}
	/// <summary>
	/// Options for the Describe command
	/// </summary>
	public class DescribeCmdOptions : Options
	{
		/// <summary>
		/// Options for the Describe command
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="contextLines"></param>
		/// <param name="unifiedLines"></param>
		/// <remarks>
		/// <br/><b>p4 help describe</b>
		/// <br/> 
		/// <br/>     describe -- Display a changelist description
		/// <br/> 
		/// <br/>     p4 describe [-d&lt;flags&gt; -s -S -f] changelist# ...
		/// <br/> 
		/// <br/> 	Display a changelist description, including the changelist number,
		/// <br/> 	user, client, date of submission, textual description, list of
		/// <br/> 	affected files and diffs of files updated.  Pending changelists
		/// <br/> 	are indicated as 'pending' and file diffs are not displayed.
		/// <br/> 
		/// <br/> 	For restricted changelists, 'no permission' is displayed if the user
		/// <br/> 	is not permitted to view the change (see 'p4 help change'). If a
		/// <br/> 	submitted or shelved change is restricted, the description is hidden
		/// <br/> 	unless the user is the owner of the change or has list permission for
		/// <br/> 	at least one file in the change. To view restricted pending (not
		/// <br/> 	shelved) changes, the user must be the owner of the change.
		/// <br/> 
		/// <br/> 	The -d&lt;flags&gt; passes one or more flags to the built-in diff routine
		/// <br/> 	to modify the output: -dn (RCS), -dc[n] (context), -ds (summary),
		/// <br/> 	-du[n] (unified), -db (ignore whitespace changes), -dw (ignore
		/// <br/> 	whitespace), -dl (ignore line endings). The optional argument to
		/// <br/> 	to -dc specifies number of context lines.
		/// <br/> 
		/// <br/> 	The -s flag omits the diffs of files that were updated.
		/// <br/> 
		/// <br/> 	The -S flag lists files that are shelved for the specified changelist
		/// <br/> 	and displays diffs of the files against their previous revision.
		/// <br/> 
		/// <br/> 	The -f flag forces display of the descriptions in a restricted
		/// <br/> 	change.  The -f flag requires 'admin' access, which is granted
		/// <br/> 	using 'p4 protect'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public DescribeCmdOptions(DescribeChangelistCmdFlags flags,
			int contextLines, int unifiedLines)
			: base(flags, contextLines, unifiedLines)
		{
		}
	}

	/// <summary>
	/// Flags for the trust command.
	/// </summary>
	[Flags]
	public enum TrustCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -l flag lists existing known fingerprints.
		/// </summary>
		List = 0x0001,
		/// <summary>
		/// 	The -y flag will cause prompts to be automatically accepted.
		/// </summary>
		AutoAccept = 0x0002,
		/// <summary>
		/// 	The -n flag will cause prompts to be automatically refused.
		/// </summary>
		AutoReject = 0x0004,
		/// <summary>
		/// 	The -d flag will remove an existing trusted fingerprint of a connection.
		/// </summary>
		Delete = 0x0008,
		/// <summary>
		/// 	The -f flag will force the replacement of a mismatched fingerprint.
		/// </summary>
		ForceReplacement = 0x0010,
		/// <summary>
		///     The -i flag will allow a specific fingerprint to be installed.
		/// </summary>
		Install = 0x0020,
		/// <summary>
		///     The -r flag specifies that a replacement fingerprint is to be
		///     affected.  Replacement fingerprints can be used in anticipation
		///     of a server replacing its key.  If a replacement fingerprint
		///     exists for a connection and the primary fingerprint does not match
		///     while the replacement fingerprint does, the replacement fingerprint
		///     will replace the primary.  This flag can be combined with -l, -i,
		///     or -d.
		/// </summary>
		Replacement = 0x0040,
	}

	public partial class Options
	{
		/// <summary>
		/// Options for the trust command
		/// </summary>
		/// <param name="flags"></param>
		/// <remarks>
		/// <br/><b>p4 trust -h</b>
		/// <br/> 
		/// <br/>         trust -- Establish trust of an SSL connection
		/// <br/> 
		/// <br/>         p4 trust [ -l -y -n -d -f -r -i &lt;fingerprint&gt; ]
		/// <br/> 
		/// <br/>         Establish trust of an SSL connection.  This client command manages
		/// <br/>         the p4 trust file.  This file contains fingerprints of the keys
		/// <br/>         received on ssl connections.  When an SSL connection is made, this
		/// <br/>         file is examined to determine if the SSL connection has been used
		/// <br/>         before and if the key is the same as a previously seen key for that
		/// <br/>         connection.  Establishing trust with a connection prevents undetected
		/// <br/>         communication interception (man-in-the-middle) attacks.
		/// <br/> 
		/// <br/>         Most options are mutually exclusive.  Only the -r and -f options
		/// <br/>         can be combined with the others.
		/// <br/> 
		/// <br/>         The -l flag lists existing known fingerprints.
		/// <br/> 
		/// <br/>         Without options, this command will make a connection to a server
		/// <br/>         and examine the key if present, if one cannot be found this command
		/// <br/>         will show a fingerprint and ask if this connection should be trusted.
		/// <br/>         If a fingerprint exists and does not match, an error that a possible
		/// <br/>         security problems exists will be displayed.
		/// <br/> 
		/// <br/>         The -y flag will cause prompts to be automatically accepted.
		/// <br/> 
		/// <br/>         The -n flag will cause prompts to be automatically refused.
		/// <br/> 
		/// <br/>         The -d flag will remove an existing trusted fingerprint of a connection.
		/// <br/> 
		/// <br/>         The -f flag will force the replacement of a mismatched fingerprint.
		/// <br/> 
		/// <br/>         The -i flag will allow a specific fingerprint to be installed.
		/// <br/> 
		/// <br/>         The -r flag specifies that a replacement fingerprint is to be
		/// <br/>         affected.  Replacement fingerprints can be used in anticipation
		/// <br/>         of a server replacing its key.  If a replacement fingerprint
		/// <br/>         exists for a connection and the primary fingerprint does not match
		/// <br/>         while the replacement fingerprint does, the replacement fingerprint
		/// <br/>         will replace the primary.  This flag can be combined with -l, -i,
		/// <br/>         or -d. 
		/// </remarks>
		public Options(TrustCmdFlags flags)
		{
			if ((flags & TrustCmdFlags.List) != 0)
			{
				this["-l"] = null;
			}

			if ((flags & TrustCmdFlags.AutoAccept) != 0)
			{
				this["-y"] = null;
			}

			if ((flags & TrustCmdFlags.AutoReject) != 0)
			{
				this["-n"] = null;
			}

			if ((flags & TrustCmdFlags.Delete) != 0)
			{
				this["-d"] = null;
			}

			if ((flags & TrustCmdFlags.ForceReplacement) != 0)
			{
				this["-f"] = null;
			}

			if ((flags & TrustCmdFlags.Install) != 0)
			{
				this["-i"] = null;
			}

			if ((flags & TrustCmdFlags.Replacement) != 0)
			{
				this["-r"] = null;
			}
		}
	}
	/// <summary>
	/// Options for the Trust command
	/// </summary>
	public class TrustCmdOptions : Options
	{
		/// <summary>
		/// Options for the Describe command
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="contextLines"></param>
		/// <param name="unifiedLines"></param>
		/// <remarks>
		/// <br/><b>p4 trust -h</b>
		/// <br/> 
		/// <br/>         trust -- Establish trust of an SSL connection
		/// <br/> 
		/// <br/>         p4 trust [ -l -y -n -d -f -r -i &lt;fingerprint&gt; ]
		/// <br/> 
		/// <br/>         Establish trust of an SSL connection.  This client command manages
		/// <br/>         the p4 trust file.  This file contains fingerprints of the keys
		/// <br/>         received on ssl connections.  When an SSL connection is made, this
		/// <br/>         file is examined to determine if the SSL connection has been used
		/// <br/>         before and if the key is the same as a previously seen key for that
		/// <br/>         connection.  Establishing trust with a connection prevents undetected
		/// <br/>         communication interception (man-in-the-middle) attacks.
		/// <br/> 
		/// <br/>         Most options are mutually exclusive.  Only the -r and -f options
		/// <br/>         can be combined with the others.
		/// <br/> 
		/// <br/>         The -l flag lists existing known fingerprints.
		/// <br/> 
		/// <br/>         Without options, this command will make a connection to a server
		/// <br/>         and examine the key if present, if one cannot be found this command
		/// <br/>         will show a fingerprint and ask if this connection should be trusted.
		/// <br/>         If a fingerprint exists and does not match, an error that a possible
		/// <br/>         security problems exists will be displayed.
		/// <br/> 
		/// <br/>         The -y flag will cause prompts to be automatically accepted.
		/// <br/> 
		/// <br/>         The -n flag will cause prompts to be automatically refused.
		/// <br/> 
		/// <br/>         The -d flag will remove an existing trusted fingerprint of a connection.
		/// <br/> 
		/// <br/>         The -f flag will force the replacement of a mismatched fingerprint.
		/// <br/> 
		/// <br/>         The -i flag will allow a specific fingerprint to be installed.
		/// <br/> 
		/// <br/>         The -r flag specifies that a replacement fingerprint is to be
		/// <br/>         affected.  Replacement fingerprints can be used in anticipation
		/// <br/>         of a server replacing its key.  If a replacement fingerprint
		/// <br/>         exists for a connection and the primary fingerprint does not match
		/// <br/>         while the replacement fingerprint does, the replacement fingerprint
		/// <br/>         will replace the primary.  This flag can be combined with -l, -i,
		/// <br/>         or -d. 
		/// </remarks>
		public TrustCmdOptions(TrustCmdFlags flags)
			: base(flags)
		{
		}
	}

	/// <summary>
	/// Flags for the info command.
	/// </summary>
	[Flags]
	public enum InfoCmdFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// 	The -s option produces 'short' output that omits any information
		///       that requires a database lookup such as the client root).
		/// </summary>
		Short = 0x0001,
	}

	public partial class Options
	{
		/// <summary>
		/// Options for the trust command
		/// </summary>
		/// <param name="flags"></param>
		/// <remarks>
		/// <br/><b>p4 help info</b>
		/// <br/>info -- Display client/server information
		/// <br/>
		/// <br/>p4 info [-s]
		/// <br/>        Info lists information about the current client (user name,
		/// <br/>		 client name, applicable client root, client current directory,
		/// <br/>		 and the client IP address) and some server information (server
		/// <br/>		 IP address, server root, date, uptime, version and license data).
		/// <br/>
		/// <br/>        The -s option produces 'short' output that omits any information
		/// <br/>        that requires a database lookup such as the client root).
		/// </remarks>
		public Options(InfoCmdFlags flags)
		{
			if ((flags & InfoCmdFlags.Short) != 0)
			{
				this["-s"] = null;
			}
		}
	}
	/// <summary>
	/// Options for the Trust command
	/// </summary>
	public class InfoCmdOptions : Options
	{
		/// <summary>
		/// Options for the Describe command
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="contextLines"></param>
		/// <param name="unifiedLines"></param>
		/// <remarks>
		/// <br/><b>p4 help info</b>
		/// <br/>info -- Display client/server information
		/// <br/>
		/// <br/>p4 info [-s]
		/// <br/>        Info lists information about the current client (user name,
		/// <br/>		 client name, applicable client root, client current directory,
		/// <br/>		 and the client IP address) and some server information (server
		/// <br/>		 IP address, server root, date, uptime, version and license data).
		/// <br/>
		/// <br/>        The -s option produces 'short' output that omits any information
		/// <br/>        that requires a database lookup such as the client root).
		/// </remarks>
		public InfoCmdOptions(InfoCmdFlags flags)
			: base(flags)
		{
		}
	}
}
