 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Perforce.P4
{

	/// <summary>
	/// Represents a Perforce server and connection.
	/// </summary>
	public partial class Repository : IDisposable
	{
		/// <summary>
		/// Create a repository on the specified server.
		/// </summary>
		/// <param name="server">The repository server./// </param>
		public Repository (Server server)
		{
			Server = server;
		}
		public Server Server {get; private set;}

		private Connection _connection;
		public Connection Connection
		{
			get
			{
				if (_connection == null)
				{
					_connection = new Connection(Server);
				}
				return _connection;
			}
		}
		/// <summary>
		/// Return a list of FileSpecs of files in the depot that correspond
		/// to the passed-in FileSpecs. 
		/// </summary>
		/// <param name="filespecs"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help files</b>
		/// <br/> 
		/// <br/>     files -- List files in the depot
		/// <br/> 
		/// <br/>     p4 files [ -a ] [ -A ] [ -m max ] file[revRange] ...
		/// <br/> 
		/// <br/> 	List details about specified files: depot file name, revision,
		/// <br/> 	file, type, change action and changelist number of the current
		/// <br/> 	head revision. If client syntax is used to specify the file
		/// <br/> 	argument, the client view mapping is used to determine the
		/// <br/> 	corresponding depot files.
		/// <br/> 
		/// <br/> 	By default, the head revision is listed.  If the file argument
		/// <br/> 	specifies a revision, then all files at that revision are listed.
		/// <br/> 	If the file argument specifies a revision range, the highest revision
		/// <br/> 	in the range is used for each file. For details about specifying
		/// <br/> 	revisions, see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -a flag displays all revisions within the specific range, rather
		/// <br/> 	than just the highest revision in the range.
		/// <br/> 
		/// <br/> 	The -A flag displays files in archive depots.
		/// <br/> 
		/// <br/> 	The -m flag limits files to the first 'max' number of files.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		/// <example>
		///		To get a maximum of 10 files from the repository:
		///		<code> 
		///			
		///			Options opts = new Options(GetFilesCmdFlags.None, 10);
		///			FileSpec fs = new FileSpec(new DepotPath("//depot/..."), null);
		///			List<FileSpec> lfs = new List<FileSpec>();
		///			lfs.Add(fs);
		///			IList&#60;FileSpec&#62; files = _repository.getDepotFiles(lfs, opts);
		///			
		///		</code>
		/// </example>
		/// <seealso cref="GetFilesCmdFlags"/> 
		public IList<FileSpec> GetDepotFiles(IList<FileSpec> filespecs, Options options)
		{
			P4.P4Command filesCmd = new P4Command(this, "files", true, FileSpec.ToStrings(filespecs));
			P4.P4CommandResult r = filesCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			List<FileSpec> value = new List<FileSpec>();

			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{
				string path = obj["depotFile"];
				PathSpec ps = new DepotPath(path);
				int rev = 0;
				int.TryParse(obj["rev"], out rev);
				FileSpec fs = new FileSpec(ps, new Revision(rev));
				value.Add(fs);

			}
			return value;
		}

		/// <summary>
		/// Return a list of FileSpecs of files opened for specified changelists.
		/// </summary>
		/// <param name="filespecs"></param>
		/// <param name="options"></param>
		/// <returns></returns>
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
		public IList<File> GetOpenedFiles(IList<FileSpec> filespecs, Options options)
		{
			P4.P4Command openedCmd = new P4Command(this, "opened", true, FileSpec.ToStrings(filespecs));
			P4.P4CommandResult r = openedCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			List<File> value = new List<File>();

			DepotPath dps = null;
			ClientPath cps = null;
			int revision = 0;
			Revision rev = new Revision(0);
			Revision haveRev = new Revision(0);
			StringEnum<FileAction> action = null;
			int change = -1;
			FileType type = null;
			DateTime submittime = DateTime.MinValue;
			string user = string.Empty;
			string client = string.Empty;


			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{
				if (obj.ContainsKey("depotFile"))
				{
					dps = new DepotPath(obj["depotFile"]);
				}

				if (obj.ContainsKey("clientFile"))
				{
					cps = new ClientPath(obj["clientFile"]);
				}

				if (obj.ContainsKey("rev"))
				{
					int.TryParse(obj["rev"], out revision);
					rev = new Revision(revision);
				}

				if (obj.ContainsKey("haveRev"))
				{
					int.TryParse(obj["haveRev"], out revision);
					haveRev = new Revision(revision);
				}

				if (obj.ContainsKey("action"))
				{
					action = obj["action"];
				}

				if (obj.ContainsKey("change"))
				{
					int.TryParse(obj["change"], out change);
				}

				if (obj.ContainsKey("type"))
				{
					type = new FileType(obj["type"]);
				}

				if (obj.ContainsKey("user"))
				{
					user = obj["user"];
				}

				if (obj.ContainsKey("client"))
				{
					client = obj["client"];
				}

				File f = new File(dps, cps, rev, haveRev, change, action, type, submittime, user, client);
				value.Add(f);
			}
			return value;
		}

		/// <summary>
		/// Use the p4 fstat command to get the file metadata for the files
		/// matching the FileSpec. 
		/// </summary>
		/// <param name="filespec"></param>
		/// <param name="options"></param>
		/// <returns></returns>
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
		/// <br/> 		reresolvable         -- reresolvable integration records
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
		public IList<FileMetaData> GetFileMetaData(Options options, params FileSpec[] filespecs)
		{
			string[] paths = FileSpec.ToEscapedStrings(filespecs);
			P4.P4Command fstatCmd = new P4Command(this, "fstat", true, paths);
			P4.P4CommandResult r = fstatCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			List<FileMetaData> value = new List<FileMetaData>();

			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{
				FileMetaData fmd = new FileMetaData();
				fmd.FromFstatCmdTaggedData(obj);
				value.Add(fmd);
			}
			return value;
		}
		public IList<FileMetaData> GetFileMetaData(IList<FileSpec> filespecs, Options options)
		{
			return GetFileMetaData(options, filespecs.ToArray());
		}
		/// <summary>
		/// Get the File objects associated with the passed-in FileSpec list. 
		/// </summary>
		/// <param name="options"></param>
		/// <param name="filespecs"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help files</b>
		/// <br/> 
		/// <br/>     files -- List files in the depot
		/// <br/> 
		/// <br/>     p4 files [ -a ] [ -A ] [ -m max ] file[revRange] ...
		/// <br/> 
		/// <br/> 	List details about specified files: depot file name, revision,
		/// <br/> 	file, type, change action and changelist number of the current
		/// <br/> 	head revision. If client syntax is used to specify the file
		/// <br/> 	argument, the client view mapping is used to determine the
		/// <br/> 	corresponding depot files.
		/// <br/> 
		/// <br/> 	By default, the head revision is listed.  If the file argument
		/// <br/> 	specifies a revision, then all files at that revision are listed.
		/// <br/> 	If the file argument specifies a revision range, the highest revision
		/// <br/> 	in the range is used for each file. For details about specifying
		/// <br/> 	revisions, see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -a flag displays all revisions within the specific range, rather
		/// <br/> 	than just the highest revision in the range.
		/// <br/> 
		/// <br/> 	The -A flag displays files in archive depots.
		/// <br/> 
		/// <br/> 	The -m flag limits files to the first 'max' number of files.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public IList<File> GetFiles(Options options, params FileSpec[] filespecs)
		{
			P4.P4Command fstatCmd = new P4Command(this, "files", true, FileSpec.ToStrings(filespecs));
			P4.P4CommandResult r = fstatCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			List<File> value = new List<File>();

			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{
				File val = new File();
				val.ParseFilesCmdTaggedData(obj);
				value.Add(val);
			}
			return value;
		}
		public IList<File> GetFiles(IList<FileSpec> filespecs, Options options)
		{
			return GetFiles(options, filespecs.ToArray());
		}
		/// <summary>
		/// List selected directory paths in the repository. 
		/// </summary>
		/// <param name="dirs"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help dirs</b>
		/// <br/> 
		/// <br/>     dirs -- List depot subdirectories
		/// <br/> 
		/// <br/>     p4 dirs [-C -D -H] [-S stream] dir[revRange] ...
		/// <br/> 
		/// <br/> 	List directories that match the specified file pattern (dir).
		/// <br/> 	This command does not support the recursive wildcard (...).
		/// <br/> 	Use the * wildcard instead.
		/// <br/> 
		/// <br/> 	Perforce does not track directories individually. A path is treated
		/// <br/> 	as a directory if there are any undeleted files with that path as a
		/// <br/> 	prefix.
		/// <br/> 
		/// <br/> 	By default, all directories containing files are listed. If the dir
		/// <br/> 	argument includes a revision range, only directories containing files
		/// <br/> 	in the range are listed. For details about specifying file revisions,
		/// <br/> 	see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -C flag lists only directories that fall within the current
		/// <br/> 	client view.
		/// <br/> 
		/// <br/> 	The -D flag includes directories containing only deleted files.
		/// <br/> 
		/// <br/> 	The -H flag lists directories containing files synced to the current
		/// <br/> 	client workspace.
		/// <br/> 
		/// <br/> 	The -S flag limits output to depot directories mapped in a stream's
		/// <br/> 	client view.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public IList<String> GetDepotDirs(Options options, params string[] dirs)
		{

			P4.P4Command dirsCmd = new P4Command(this, "dirs", false, dirs);
			P4.P4CommandResult r = dirsCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			List<String> value = new List<String>();

			foreach (P4.InfoLine l in r.InfoOutput)
			{
				value.Add(l.Info);
			}
			return value;
		}
		public IList<String> GetDepotDirs(IList<String> dirs, Options options)
		{
			return GetDepotDirs(options, dirs.ToArray());
		}

		/// <summary>
		/// Return the contents of the files identified by the passed-in file specs. 
		/// </summary>
		/// <param name="print"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// GetFileContents
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

		public IList<string> GetFileContents(Options options, params FileSpec[] filespecs)
		{
			P4.P4Command printCmd = new P4Command(this, "print", true, FileSpec.ToEscapedStrings(filespecs));
			P4.P4CommandResult r = printCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			IList<string> value = new List<string>();
			if (r.TaggedOutput != null)
			{
				if ((options == null) ||
				(options.ContainsKey("-q") == false))
				{
					foreach (P4.TaggedObject obj in r.TaggedOutput)
					{
						string path = string.Empty;
						string rev = string.Empty;
						if (obj.ContainsKey("depotFile"))
						{
							value.Add(obj["depotFile"]);
						}
					}

				}
			}

			value.Add(r.TextOutput);
			return value;
		}
		public IList<string> GetFileContents(IList<FileSpec> filespecs, Options options)
		{
			return GetFileContents(options, filespecs.ToArray());
		}

		/// <summary>
		/// Get the revision history data for the passed-in file specs. 
		/// </summary>    
		/// <param name="filespecs"></param>
		/// <param name="options">See: <see cref="Options.FileLogCmdOptions"/></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help filelog</b>
		/// <br/> 
		/// <br/>     filelog -- List revision history of files
		/// <br/> 
		/// <br/>     p4 filelog [-c changelist# -h -i -l -L -t -m maxRevs -s] file[revRange] ...
		/// <br/> 
		/// <br/> 	List the revision history of the specified files, from the most
		/// <br/> 	recent revision to the first.  If the file specification includes
		/// <br/> 	a revision, the command lists revisions at or prior to the specified
		/// <br/> 	revision.  If the file specification includes a revision range,
		/// <br/> 	the command lists only the specified revisions. See 'p4 help revisions'
		/// <br/> 	for details.
		/// <br/> 
		/// <br/> 	The -c changelist# flag displays files submitted at the specified
		/// <br/> 	changelist number.
		/// <br/> 
		/// <br/> 	The -i flag includes inherited file history. If a file was created by
		/// <br/> 	branching (using 'p4 integrate'), filelog lists the revisions of the
		/// <br/> 	file's ancestors up to the branch points that led to the specified
		/// <br/> 	revision.  File history inherited by renaming (using 'p4 move') is
		/// <br/> 	always displayed regardless of whether -i is specified.
		/// <br/> 
		/// <br/> 	The -h flag displays file content history instead of file name
		/// <br/> 	history.  The list includes revisions of other files that were
		/// <br/> 	branched or copied (using 'p4 integrate' and 'p4 resolve -at') to
		/// <br/> 	the specified revision.  Revisions that were replaced by copying
		/// <br/> 	or branching are omitted, even if they are part of the history of
		/// <br/> 	the specified revision.
		/// <br/> 
		/// <br/> 	The -t flag displays the time as well as the date.
		/// <br/> 
		/// <br/> 	The -l flag lists the full text of the changelist descriptions.
		/// <br/> 
		/// <br/> 	The -L flag lists the full text of the changelist descriptions,
		/// <br/> 	truncated to 250 characters if longer.
		/// <br/> 
		/// <br/> 	The -m maxRevs displays at most 'maxRevs' revisions per file of
		/// <br/> 	the file[rev] argument specified.
		/// <br/> 
		/// <br/> 	The -s flag displays a shortened form of filelog that omits
		/// <br/> 	non-contributory integrations.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		/// <seealso cref="Options.FileLogCmdOptions"/>
		public IList<FileHistory> GetFileHistory(Options options, params FileSpec[] filespecs)
		{
			P4.P4Command filesCmd = new P4Command(this, "filelog", true, FileSpec.ToEscapedStrings(filespecs));
			P4.P4CommandResult r = filesCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			IList<FileHistory> value = new List<FileHistory>();
			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{
				int idx = 0;
				
				while (true)
				{
					string key = String.Format("rev{0}", idx);
					int revision = -1;
					
					if (obj.ContainsKey(key))
						int.TryParse(obj[key], out revision);
					else
						break;

					int changelistid = -1;
					key = String.Format("change{0}", idx);
					if (obj.ContainsKey(key))
						int.TryParse(obj[key], out changelistid);

					StringEnum<FileAction> action = "None";
					key = String.Format("action{0}", idx);
					if (obj.ContainsKey(key))
						action = obj[key];

					DateTime date = new DateTime();
					long unixTime = 0;
					key = String.Format("time{0}", idx);
					if (obj.ContainsKey(key))
						unixTime = Int64.Parse(obj[key]);
						date = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(unixTime);

					string username = null;
					key = String.Format("user{0}", idx);
					if (obj.ContainsKey(key))
						username = obj[key];

					string description = null;
					key = String.Format("desc{0}", idx);
					if (obj.ContainsKey(key))
						description = obj[key];

					string digest = null;
					key = String.Format("digest{0}", idx);
					if (obj.ContainsKey(key))
						digest = obj[key];

					long filesize = -1;
					key = String.Format("fileSize{0}", idx);
					if (obj.ContainsKey(key))
						long.TryParse(obj[key], out filesize);

					string clientname = null;
					key = String.Format("client{0}", idx);
					if (obj.ContainsKey(key))
						clientname = obj[key];

					PathSpec depotpath = new DepotPath(obj["depotFile"]);

					FileType filetype = null;
					key = String.Format("type{0}", idx);
					if (obj.ContainsKey(key))
						filetype = new FileType(obj[key]);

					List<RevisionIntegrationSummary> integrationsummaries = new List<RevisionIntegrationSummary>();

					int idx2 = 0;
					key = String.Format("how{0},{1}", idx, idx2);
					while (obj.ContainsKey(key))
					{
						string how = obj[key];
						key = String.Format("file{0},{1}", idx, idx2);
						string frompath = obj[key];

						key = String.Format("srev{0},{1}", idx, idx2);
						string srev = obj[key];

						VersionSpec startrev = new Revision(-1);

						if (srev.StartsWith("#h")
							|
							srev.StartsWith("#n"))
						{
							if (srev.Contains("#none"))
							{
								startrev = Revision.None;
							}

							if (srev.Contains("#have"))
							{
								startrev = Revision.Have;
							}

							if (srev.Contains("#head"))
							{
								startrev = Revision.Head;
							}
						}
						else
						{
							srev = srev.Trim('#');
							int rev = Convert.ToInt16(srev);
							startrev = new Revision(rev);
						}

						key = String.Format("erev{0},{1}", idx, idx2);
						string erev = obj[key];

						VersionSpec endrev = new Revision(-1);

						if (erev.StartsWith("#h")
							|
							erev.StartsWith("#n"))
						{
							if (erev.Contains("#none"))
							{
								endrev = Revision.None;
							}

							if (srev.Contains("#have"))
							{
								endrev = Revision.Have;
							}

							if (srev.Contains("#head"))
							{
								endrev = Revision.Head;
							}
						}
						else
						{
							erev = erev.Trim('#');
							int rev = Convert.ToInt16(erev);
							endrev = new Revision(rev);
						}

						RevisionIntegrationSummary integrationsummary = new RevisionIntegrationSummary(
							  new FileSpec(new DepotPath(frompath),
							  new VersionRange(startrev, endrev)), how);
						
						integrationsummaries.Add(integrationsummary);

						idx2++;
						key = String.Format("how{0},{1}", idx, idx2);
					}

					FileHistory fh = new FileHistory(revision, changelistid, action,
				date, username, filetype, description, digest, filesize, depotpath, clientname, integrationsummaries);

					value.Add(fh);

					idx++;
					
				}
			}
			return value;
		}
		public IList<FileHistory> GetFileHistory(IList<FileSpec> filespecs, Options options)
		{
			return GetFileHistory(options, filespecs.ToArray());
		}
		/// <summary>
		/// Get content and existence diff details for two depot files.
		/// </summary>    
		/// <param name="filelog"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help diff</b>
		/// <br/> 
		/// <br/>     diff -- Display diff of client file with depot file
		/// <br/> 
		/// <br/>     p4 diff [-d&lt;flags&gt; -f -m max -s&lt;flag&gt; -t] [file[rev] ...]
		/// <br/> 
		/// <br/> 	On the client machine, diff a client file against the corresponding
		/// <br/> 	revision in the depot. The file is compared only if the file is
		/// <br/> 	opened for edit or a revision is provided. See 'p4 help revisions'
		/// <br/> 	for details about specifying revisions.
		/// <br/> 
		/// <br/> 	If the file specification is omitted, all open files are diffed.
		/// <br/> 	This option can be used to view pending changelists.
		/// <br/> 
		/// <br/> 	The -d&lt;flags&gt; modify the output as follows: -dn (RCS), -dc[n] (context),
		/// <br/> 	-ds (summary), -du[n] (unified), -db (ignore whitespace changes),
		/// <br/> 	-dw (ignore whitespace), -dl (ignore line endings). The optional
		/// <br/> 	argument to -dc specifies number of context lines.
		/// <br/> 
		/// <br/> 	The -f flag diffs every file, regardless of whether they are opened
		/// <br/> 	or the client has synced the specified revision.  This option can be
		/// <br/> 	used to verify the contents of the client workspace.
		/// <br/> 
		/// <br/> 	The -m max flag limits output to the first 'max' number of files,
		/// <br/> 	unless the -s flag is used, in which case it is ignored.
		/// <br/> 
		/// <br/> 	The -s options lists the files that satisfy the following criteria:
		/// <br/> 
		/// <br/> 		-sa     Opened files that differ from the revision
		/// <br/> 			in the depot or are missing.
		/// <br/> 
		/// <br/> 		-sb     Files that have been opened for integrate, resolved,
		/// <br/> 			and subsequently modified.
		/// <br/> 
		/// <br/> 		-sd     Unopened files that are missing on the client.
		/// <br/> 
		/// <br/> 		-se     Unopened files that differ from the revision
		/// <br/> 			in the depot.
		/// <br/> 
		/// <br/> 		-sl	Every unopened file, along with the status of
		/// <br/> 			'same, 'diff', or 'missing' as compared to the
		/// <br/> 			corresponding revision in the depot.
		/// <br/> 
		/// <br/> 		-sr     Opened files that do not differ from the revision in
		/// <br/> 			the depot.
		/// <br/> 
		/// <br/> 	The -t flag forces 'p4 diff' to diff binary files.
		/// <br/> 
		/// <br/> 	If the environment variable $P4DIFF is set,  the specified diff
		/// <br/> 	program is launched in place of the default Perforce client diff.
		/// <br/> 	The -d&lt;flags&gt; option can be used to pass arguments to the diff
		/// <br/> 	program.  Because the -s flag is only implemented internally, any
		/// <br/> 	-d&lt;flags&gt; option used with the -s&lt;flag&gt; is ignored. To configure a
		/// <br/> 	diff program for Unicode files, set the environment variable
		/// <br/> 	$P4DIFFUNICODE. Specify the file's character set as the first
		/// <br/> 	argument to the program.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public IList<DepotFileDiff> GetDepotFileDiffs(string filespecleft, string filespecright, Options options)
		{
			P4.P4Command GetDepotFileDiffs = new P4Command(this, "diff2", true, filespecleft, filespecright);

			P4.P4CommandResult r = GetDepotFileDiffs.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			IList<DepotFileDiff> value = new List<DepotFileDiff>();

			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{
				DepotFileDiff val = new DepotFileDiff();
				val.FromGetDepotFileDiffsCmdTaggedOutput(obj, _connection, options);
				value.Add(val);

			}
			return value;
			
		}

		/// <summary>
		/// Return FileAnnotation objects for the listed FileSpecs. 
		/// </summary>    
		/// <param name="filelog"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help annotate</b>
		/// <br/> 
		/// <br/>     annotate -- Print file lines and their revisions
		/// <br/> 
		/// <br/>     p4 annotate [-aciIq -d&lt;flags&gt;] file[revRange] ...
		/// <br/> 
		/// <br/> 	Prints all lines of the specified files, indicating the revision that
		/// <br/> 	introduced each line into the file.
		/// <br/> 
		/// <br/> 	If the file argument includes a revision, then only revisions up to
		/// <br/> 	the specified revision are displayed.  If the file argument has a
		/// <br/> 	revision range, only revisions within that range are displayed. For
		/// <br/> 	details about specifying revisions, see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -a flag includes both deleted files and lines no longer present
		/// <br/> 	at the head revision. In the latter case, both the starting and ending
		/// <br/> 	revision for each line is displayed.
		/// <br/> 
		/// <br/> 	The -c flag directs the annotate command to output changelist numbers
		/// <br/> 	rather than revision numbers for each line.
		/// <br/> 
		/// <br/> 	The -d&lt;flags&gt; change the way whitespace and/or line endings are
		/// <br/> 	treated: -db (ignore whitespace changes), -dw (ignore whitespace),
		/// <br/> 	-dl (ignore line endings).
		/// <br/> 
		/// <br/> 	The -i flag follows branches.  If a file was created by branching,
		/// <br/> 	'p4 annotate' includes the revisions of the source file up to the
		/// <br/> 	branch point, just as 'p4 filelog -i' does.  If a file has history
		/// <br/> 	prior to being created by branching (such as a file that was branched
		/// <br/> 	on top of a deleted file), -i ignores those prior revisions and
		/// <br/> 	follows the source.  -i implies -c.
		/// <br/> 
		/// <br/> 	The -I flag follows all integrations into the file.  If a line was
		/// <br/> 	introduced into the file by a merge, the source of the merge is
		/// <br/> 	displayed as the changelist that introduced the line. If the source
		/// <br/> 	itself was the result of an integration, that source is used instead,
		/// <br/> 	and so on.  -I implies -c.
		/// <br/> 
		/// <br/> 	The -q flag suppresses the one-line header that is displayed by
		/// <br/> 	default for each file.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public IList<FileAnnotation> GetFileAnnotations(IList<FileSpec> filespecs, Options options)
		{

			P4.P4Command annotateCmd = new P4Command(this, "annotate", true, FileSpec.ToStrings(filespecs));

			P4.P4CommandResult r = annotateCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}

			bool changelist = false;
			string opts;
			if (options != null)
			{
				opts = options.Keys.ToString();
				if (opts.Contains("c"))
				{ changelist = true; }                               
			}
			
			string dp = null;
			string line = null;
			int lower = -1; 
			int upper = -1; 
			IList<FileAnnotation> value = new List<FileAnnotation>();
			//FileAnnotation fa = new FileAnnotation(new FileSpec(new DepotPath(dp), new VersionRange(lower, upper)), line);

			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{
				if (obj.ContainsKey("depotFile"))
				{
					dp = obj["depotFile"];
					line = null;
					lower = -1;
					upper = -1;
					continue;
				}

				if (obj.ContainsKey("lower"))
				{
					int l = -1;
					int.TryParse(obj["lower"], out l);
					lower = l;
				}

				if (obj.ContainsKey("upper"))
				{
					int u = -1;
					int.TryParse(obj["upper"], out u);
					upper = u;
				}

				if (obj.ContainsKey("data"))
				{
					line = obj["data"];
				}

				if (dp != null
					&&
					line != null
					&&
					lower != -1
					&&
					upper != -1)
				{
					FileSpec fs = new FileSpec();
					if (changelist == true)
					{
						fs = new FileSpec(new DepotPath(dp), new VersionRange(new ChangelistIdVersion(lower), new ChangelistIdVersion(upper)));
					}
					else
					{
						fs = new FileSpec(new DepotPath(dp), new VersionRange(new Revision(lower), new Revision(upper)));
					}
					FileAnnotation fa = new FileAnnotation(fs, line);
					value.Add(fa);
				}
			}
			return value;
		}


		/// <summary>
		/// Tag depot files with the passed-in label. 
		/// </summary>    
		/// <param name="filespecs"></param>
		/// <param name="labelid"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help tag</b>
		/// <br/> 
		/// <br/>     tag -- Tag files with a label
		/// <br/> 
		/// <br/>     p4 tag [-d -n] -l label file[revRange] ...
		/// <br/> 
		/// <br/> 	Tag associates the named label with the file revisions specified by
		/// <br/> 	the file argument.  After file revisions are tagged with a label,
		/// <br/> 	revision specifications of the form '@label' can be used to refer
		/// <br/> 	to them.
		/// <br/> 
		/// <br/> 	If the file argument does not include a revision specification, the
		/// <br/> 	head revisions is tagged.  See 'p4 help revisions' for revision
		/// <br/> 	specification options.
		/// <br/> 
		/// <br/> 	If the file argument includes a revision range specification, only
		/// <br/> 	the files with revisions in that range are tagged.  Files with more
		/// <br/> 	than one revision in the range are tagged at the highest revision.
		/// <br/> 
		/// <br/> 	The -d deletes the association between the specified files and the
		/// <br/> 	label, regardless of revision.
		/// <br/> 
		/// <br/> 	The -n flag previews the results of the operation.
		/// <br/> 
		/// <br/> 	Tag can be used with an existing label (see 'p4 help labels') or
		/// <br/> 	with a new one.  An existing label can be used only by its owner,
		/// <br/> 	and only if it is unlocked. (See 'p4 help label').
		/// <br/> 
		/// <br/> 	To list the file revisions tagged with a label, use 'p4 files
		/// <br/> 	@label'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public IList<FileSpec> TagFiles(IList<FileSpec> filespecs, string labelid, Options options)
		{
			
			P4.P4Command tagCmd = new P4Command(this, "tag", true, FileSpec.ToStrings(filespecs));
			options["-l"] = labelid;
			
			P4.P4CommandResult r = tagCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			IList<FileSpec> value = new List<FileSpec>();

			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{
				string revision = obj["rev"];
				int rev = Convert.ToInt16(revision);
				VersionSpec version = new Revision(rev);
				DepotPath path = new DepotPath(obj["depotFile"]);
				FileSpec fs = new FileSpec(path, version);
				value.Add(fs);
			}
			return value;
		}

		/// <summary>
		/// List fixes affecting files and / or jobs and / or changelists. 
		/// </summary>    
		/// <param name="filespecs"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help fixes</b>
		/// <br/> 
		/// <br/>     fixes -- List jobs with fixes and the changelists that fix them
		/// <br/> 
		/// <br/>     p4 fixes [-i -m max -c changelist# -j jobName] [file[revRange] ...]
		/// <br/> 
		/// <br/> 	'p4 fixes' list fixed jobs and the number of the changelist that
		/// <br/> 	 contains the fix.Fixes are associated with changelists using the
		/// <br/> 	'p4 fix' command or by editing and submitting changelists.
		/// <br/> 
		/// <br/> 	The 'p4 fixes' command lists both submitted and pending changelists.
		/// <br/> 
		/// <br/> 	By default, 'p4 fixes' lists all fixes.  This list can be limited
		/// <br/> 	as follows: to list fixes for a specified job, use the -j jobName
		/// <br/> 	flag.  To list fixes for a specified changelist, use -c changelist#.
		/// <br/> 	To list fixes that affect specified files, include the file argument.
		/// <br/> 	The file pattern can include wildcards and revision specifiers. For
		/// <br/> 	details about revision specifiers, see 'p4 help revisions'
		/// <br/> 
		/// <br/> 	The -i flag also includes any fixes made by changelists integrated
		/// <br/> 	into the specified files.
		/// <br/> 
		/// <br/> 	The -m max flag limits output to the specified number of job
		/// <br/> 	fixes.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public IList<Fix> GetFixes(IList<FileSpec> filespecs, Options options)
		{
			P4.P4Command fixesCmd = new P4Command(this, "fixes", true, FileSpec.ToStrings(filespecs));
			P4.P4CommandResult r = fixesCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			IList<Fix> value = new List<Fix>();

			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{
				value.Add(Fix.FromFixesCmdTaggedOutput(obj));
			}
			return value;
		}

		
		/// <summary>
		/// Get a list of matching lines in the passed-in file specs. 
		/// </summary>    
		/// <param name="filespecs"></param>
		/// <param name="pattern"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help grep</b>
		/// <br/> 
		/// <br/>     grep -- Print lines matching a pattern
		/// <br/> 
		/// <br/>     p4 grep [options] -e pattern file[revRange]...
		/// <br/> 
		/// <br/> 	options: -a -i -n -A &lt;num&gt; -B &lt;num&gt; -C &lt;num&gt; -t -s (-v|-l|-L) (-F|-G)
		/// <br/> 
		/// <br/> 	Searches files for lines that match the specified regular expression,
		/// <br/> 	which can contain wildcards.  The parser used by the Perforce server
		/// <br/> 	is based on V8 regexp and might not be compatible with later parsers,
		/// <br/> 	but the majority of functionality is available.
		/// <br/> 
		/// <br/> 	By default the head revision is searched. If the file argument includes
		/// <br/> 	a revision specification, all corresponding revisions are searched.
		/// <br/> 	If the file argument includes a revision range, only files in that
		/// <br/> 	range are listed, and the highest revision in the range is searched.
		/// <br/> 	For details about revision specifiers, see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -a flag searches all revisions within the specified range. By
		/// <br/> 	default only the highest revision in the range is searched.
		/// <br/> 
		/// <br/> 	The -i flag causes the pattern matching to be case-insensitive. By
		/// <br/> 	default, matching is case-sensitive.
		/// <br/> 
		/// <br/> 	The -n flag displays the matching line number after the file revision
		/// <br/> 	number. By default, matches are displayed as revision#: &lt;text&gt;.
		/// <br/> 
		/// <br/> 	The -v flag displays files with non-matching lines.
		/// <br/> 
		/// <br/> 	The -F flag is used to interpret the pattern as a fixed string.
		/// <br/> 
		/// <br/> 	The -G flag is used to interpret the pattern as a regular expression,
		/// <br/> 	which is the default behavior.
		/// <br/> 
		/// <br/> 	The -L flag displays the name of each selected file from which no
		/// <br/> 	output would normally have been displayed. Scanning stops on the
		/// <br/> 	first match.
		/// <br/> 
		/// <br/> 	The -l flag display the name of each selected file containing
		/// <br/> 	 matching text. Scanning stops on the first match.
		/// <br/> 
		/// <br/> 	The -s flag suppresses error messages that result from abandoning
		/// <br/> 	files that have a maximum number of characters in a single line that
		/// <br/> 	are greater than 4096.  By default, an error is reported when grep
		/// <br/> 	abandons such files.
		/// <br/> 
		/// <br/> 	The -t flag searches binary files.  By default, only text files are
		/// <br/> 	searched.
		/// <br/> 
		/// <br/> 	The -A &lt;num&gt; flag displays the specified number of lines of trailing
		/// <br/> 	context after matching lines.
		/// <br/> 
		/// <br/> 	The -B &lt;num&gt; flag displays the specified number of lines of leading
		/// <br/> 	context before matching lines.
		/// <br/> 
		/// <br/> 	The -C &lt;num&gt; flag displays the specified number of lines of output
		/// <br/> 	context.
		/// <br/> 
		/// <br/> 	Regular expressions:
		/// <br/> 
		/// <br/> 	A regular expression is zero or more branches, separated by `|'. It
		/// <br/> 	matches anything that matches one of the branches.
		/// <br/> 
		/// <br/> 	A branch is zero or more pieces, concatenated.  It matches a match
		/// <br/> 	for the first, followed by a match for the second, etc.
		/// <br/> 
		/// <br/> 	A piece is an atom possibly followed by `*', `+', or `?'.  An atom
		/// <br/> 	followed by `*' matches a sequence of 0 or more matches of the atom.
		/// <br/> 	An atom followed by `+' matches a sequence of 1 or more matches of
		/// <br/> 	the atom.  An atom followed by `?' matches a match of the atom, or
		/// <br/> 	the null string.
		/// <br/> 
		/// <br/> 	An atom is a regular expression in parentheses (matching a match for
		/// <br/> 	the regular expression),  a range (see below),  `.'  (matching any
		/// <br/> 	single character),  `^' (matching the null string at the beginning
		/// <br/> 	of the input string),  `$' (matching the null string at the end of
		/// <br/> 	the input string),  a `\' followed by a single character (matching
		/// <br/> 	that character),  or a single character with no other significance
		/// <br/> 	(matching that character).
		/// <br/> 
		/// <br/> 	A range is a sequence of characters enclosed in `[]'.  It normally
		/// <br/> 	matches any single character from the sequence.  If the sequence
		/// <br/> 	begins with `^',  it matches any single character not from the rest
		/// <br/> 	of the sequence.  If two characters in the sequence are separated by
		/// <br/> 	`-', this is shorthand for the full list of ASCII characters between
		/// <br/> 	them (e.g. `[0-9]' matches any decimal digit).  To include a literal
		/// <br/> 	`]' in the sequence, make it the first character (following a possible
		/// <br/> 	`^').  To include a literal `-', make it the first or last character.
		/// <br/> 
		/// <br/> 
		/// </remarks>

		public IList<FileLineMatch> GetFileLineMatches(IList<FileSpec> filespecs, string pattern, Options options)
		{
			P4.P4Command grepCmd = new P4Command(this, "grep", true, FileSpec.ToStrings(filespecs));
			options["-e"] = pattern;
			P4.P4CommandResult r = grepCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			IList<FileLineMatch> value = new List<FileLineMatch>();

			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{
				FileLineMatch val = new FileLineMatch();
				val.ParseGrepCmdTaggedData(obj);
				value.Add(val);
			}
			return value;
		}

		/// <summary>
		/// Get a list of submitted integrations for the passed-in file specs. 
		/// </summary>    
		/// <param name="filespecs"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help integrated</b>
		/// <br/> 
		/// <br/>     integrated -- List integrations that have been submitted
		/// <br/> 
		/// <br/>     p4 integrated [-r] [-b branch] [file ...]
		/// <br/> 
		/// <br/> 	The p4 integrated command lists integrations that have been submitted.
		/// <br/> 	To list unresolved integrations, use 'p4 resolve -n'.  To list
		/// <br/> 	resolved but unsubmitted integrations, use 'p4 resolved'.
		/// <br/> 
		/// <br/> 	If the -b branch flag is specified, only files integrated from the
		/// <br/> 	source to target files in the branch view are listed.  Qualified
		/// <br/> 	files are listed, even if they were integrated without using the
		/// <br/> 	branch view.
		/// <br/> 
		/// <br/> 	The -r flag reverses the mappings in the branch view, swapping the
		/// <br/> 	target files and source files.  The -b branch flag is required.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public IList<FileIntegrationRecord> GetSubmittedIntegrations(IList<FileSpec> filespecs, Options options)
		{
			P4.P4Command integratedCmd = new P4Command(this, "integrated", true, FileSpec.ToStrings(filespecs));
			P4.P4CommandResult r = integratedCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			IList<FileIntegrationRecord> value = new List<FileIntegrationRecord>();

			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{
				FileIntegrationRecord val = new FileIntegrationRecord();
				val.ParseIntegratedCmdTaggedData(obj);
				value.Add(val);

			}
			return value;
		}



		/// <summary>
		/// Get a list of Perforce protection entries for the passed-in file specs 
		/// </summary>    
		/// <param name="filespecs"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help protects</b>
		/// <br/> 
		/// <br/>     protects -- Display protections defined for a specified user and path
		/// <br/> 
		/// <br/>     p4 protects [-a | -g group | -u user] [-h host] [-m] [file ...]
		/// <br/> 
		/// <br/> 	'p4 protects' displays the lines from the protections table that
		/// <br/> 	apply to the current user.  The protections table is managed using
		/// <br/> 	the 'p4 protect' command.
		/// <br/> 
		/// <br/> 	If the -a flag is specified, protection lines for all users are
		/// <br/> 	displayed.  If the -g group flag or -u user flag is specified,
		/// <br/> 	protection lines for that group or user are displayed.
		/// <br/> 
		/// <br/> 	If the -h host flag is specified, the protection lines that apply
		/// <br/> 	to the specified host (IP address) are displayed.
		/// <br/> 
		/// <br/> 	If the -m flag is given, a single word summary of the maximum
		/// <br/> 	access level is reported. Note that this summary does not take
		/// <br/> 	exclusions into account.
		/// <br/> 
		/// <br/> 	If the file argument is specified, protection lines that apply to
		/// <br/> 	the specified files are displayed.
		/// <br/> 
		/// <br/> 	The -a/-g/-u flags require 'super' access granted by 'p4 protect'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public IList<ProtectionEntry> GetProtectionEntries(IList<FileSpec> filespecs, Options options)
		{
			P4.P4Command protectsCmd = new P4Command(this, "protects", true, FileSpec.ToStrings(filespecs));
			P4.P4CommandResult r = protectsCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			IList<ProtectionEntry> value = new List<ProtectionEntry>();

			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{

				StringEnum<ProtectionMode> mode = obj["perm"];
				StringEnum<EntryType> type = "User";
				if (obj.ContainsKey("isgroup"))
				{
					type = "Group";
				}
				string name = obj["user"];
				string host = obj["host"];
				string path = obj["depotFile"];
				ProtectionEntry pte = new ProtectionEntry(mode, type, name, host, path);

				value.Add(pte);
			}
			return value;
		}

		/// <summary>
		/// List Perforce users assigned to review files. 
		/// </summary>    
		/// <param name="filespecs"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help reviews</b>
		/// <br/> 
		/// <br/>     reviews -- List the users who are subscribed to review files
		/// <br/> 
		/// <br/>     p4 reviews [-c changelist#] [file ...]
		/// <br/> 
		/// <br/> 	'p4 reviews' lists all users who have subscribed to review the
		/// <br/> 	specified files, the files in the specified changelist, or all files
		/// <br/> 	(the default).  To subscribe to review files, issue the 'p4 user'
		/// <br/> 	command and edit the 'Reviews field'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public IList<User> GetReviewers (IList<FileSpec> filespecs, Options options)
		{
			P4.P4Command reviewsCmd = new P4Command(this, "reviews", true, FileSpec.ToStrings(filespecs));
			P4.P4CommandResult r = reviewsCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			List<User> value = new List<User>();

			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{
				string id = obj["user"];
				string fullname = obj["name"];
				string password = string.Empty;
				string emailaddress = obj["email"];
				DateTime updated = DateTime.MinValue;
				DateTime accessed = DateTime.MinValue;
				string jobview = string.Empty;
				List<string> reviews = new List<string>();
				UserType type = UserType.Standard;
				FormSpec spec = new FormSpec(null,null, null, null, null, null, null);
				User user = new User(id, fullname, password, emailaddress, updated, accessed, jobview, reviews, type, spec);
				value.Add(user);
			}
			return value;
		}

		/// <summary>
		/// Get a FormSpec of the specified form type. 
		/// </summary>    
		/// <param name="spectype"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help spec</b>
		/// <br/> 
		/// <br/>     spec -- Edit spec definitions (unsupported)
		/// <br/> 
		/// <br/>     p4 spec [-d -i -o] type
		/// <br/> 
		/// <br/> 	Edit any type of specification: branch, change, client, depot,
		/// <br/> 	group, job, label, spec, stream, trigger, typemap, or user. Only
		/// <br/> 	the comments and the formatting hints can be changed. Any fields
		/// <br/> 	that you add during editing are discarded when the spec is saved.
		/// <br/> 
		/// <br/> 	'p4 jobspec' is equivalent to 'p4 spec job', and any custom spec
		/// <br/> 	(include the job spec) can be deleted with 'p4 spec -d type'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public FormSpec GetFormSpec(Options options, string spectype)
		{
			StringList cmdArgs = new StringList();
			cmdArgs.Add(spectype);
			P4.P4Command specCmd = new P4Command(this, "spec", true, cmdArgs);
			P4.P4CommandResult r = specCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{
				FormSpec val = FormSpec.FromSpecCmdTaggedOutput(obj);

				return val;
			}

			return null;

		}


		/// <summary>
		/// Get the repository's trigger table. 
		/// </summary>    
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help triggers</b>
		/// <br/> 
		/// <br/>     triggers -- Modify list of server triggers
		/// <br/> 
		/// <br/>     p4 triggers
		/// <br/>     p4 triggers -o
		/// <br/>     p4 triggers -i
		/// <br/> 
		/// <br/> 	'p4 triggers' edits the table of triggers, which are used for
		/// <br/> 	change submission validation, form validation, external authentication,
		/// <br/> 	external job fix integration, and external archive integration.
		/// <br/> 
		/// <br/> 	Triggers are administrator-defined commands that the server runs
		/// <br/> 	to perform the following:
		/// <br/> 
		/// <br/> 	Validate changelist submissions.
		/// <br/> 
		/// <br/> 	    The server runs changelist triggers before the file transfer,
		/// <br/> 	    between file transfer and changelist commit, or after the commit
		/// <br/> 
		/// <br/> 	Validate shelve operations.
		/// <br/> 
		/// <br/> 	    The server runs shelve triggers before files are shelved, after
		/// <br/> 	    files are shelved, or when shelved files have been discarded
		/// <br/> 	    (via shelve -d).
		/// <br/> 
		/// <br/> 	Manipulate and validate forms.
		/// <br/> 
		/// <br/> 	    The server runs form-validating triggers between generating
		/// <br/> 	    and outputting the form, between inputting and parsing the
		/// <br/> 	    form, between parsing and saving the form, or when deleting
		/// <br/> 	    the form.
		/// <br/> 
		/// <br/> 	Authenticate or change a user password.
		/// <br/> 
		/// <br/> 	    The server runs authentication triggers to either validate
		/// <br/> 	    a user password during login or when setting a new password.
		/// <br/> 
		/// <br/> 	Intercept job fix additions or deletions.
		/// <br/> 
		/// <br/> 	    The server run fix triggers prior to adding or deleting a fix
		/// <br/> 	    between a job and changelist.
		/// <br/> 
		/// <br/> 	Access external archive files.
		/// <br/> 
		/// <br/> 	    For files with the +X filetype modifier, the server runs an
		/// <br/> 	    archive trigger to read, write, or delete files in the archive.
		/// <br/> 
		/// <br/> 	The trigger form has a single entry 'Triggers', followed by any
		/// <br/> 	number of trigger lines.  Triggers are executed in the order listed
		/// <br/> 	and if a trigger fails, subsequent triggers are not run.  A trigger
		/// <br/> 	succeeds if the executed command exits returning 0 and fails otherwise.
		/// <br/> 	Normally the failure of a trigger prevents the operation from
		/// <br/> 	completing, except for the commit triggers, which run after the
		/// <br/> 	operation is complete.
		/// <br/> 
		/// <br/> 	Each trigger line contains a trigger name, a trigger type, a depot
		/// <br/> 	file path pattern or form type, and a command to run.
		/// <br/> 
		/// <br/> 	Name:   The name of the trigger.  For change triggers, a run of the
		/// <br/> 		same trigger name on contiguous lines is treated as a single
		/// <br/> 		trigger so that multiple paths can be specified.  Only the
		/// <br/> 		command of the first such trigger line is used.
		/// <br/> 
		/// <br/> 	Type:	When the trigger is to execute:
		/// <br/> 
		/// <br/> 		archive:
		/// <br/> 		    Execute an archive trigger for the server to access
		/// <br/> 		    any file with the +X filetype modifier.
		/// <br/> 
		/// <br/> 		auth-check:
		/// <br/> 		service-check:
		/// <br/> 		    Execute an authentication check trigger to verify a
		/// <br/> 		    user's password against an external password manager
		/// <br/> 		    during login or when setting a new password.
		/// <br/> 
		/// <br/> 		auth-check-sso:
		/// <br/> 		    Facilitate a single sign-on user authentication. This
		/// <br/> 		    configuration requires two programs or scripts to run;
		/// <br/> 		    one on the client, the other on the server.
		/// <br/> 
		/// <br/> 		    client:
		/// <br/> 		        Set the environment variable 'P4LOGINSSO' to point to
		/// <br/> 		        a script that can be executed to obtain the user's
		/// <br/> 		        credentials or other information that the server
		/// <br/> 		        trigger can verify.  The client-side script must
		/// <br/> 		        write the message to the standard output
		/// <br/> 		        (max length 128K).
		/// <br/> 
		/// <br/> 		        Example:  P4LOGINSSO=/Users/joe/bin/runsso
		/// <br/> 
		/// <br/> 		        The 'server address' can be optionally passed to the
		/// <br/> 		        client script by appending %serverAddress% to the
		/// <br/> 		        client command string, as in:
		/// <br/> 
		/// <br/> 		        P4LOGINSSO="/Users/joe/bin/runsso %serverAddress%"
		/// <br/> 
		/// <br/> 		    server:
		/// <br/> 		        Execute an authentication (sso) trigger that gets
		/// <br/> 		        this message from the standard input and returns an
		/// <br/> 		        exit status of 0 (for verified) or otherwise failed.
		/// <br/> 
		/// <br/> 		        Example:
		/// <br/> 		            sso auth-check-sso auth "/secure/verify %user%"
		/// <br/> 
		/// <br/> 		    The user must issue the 'p4 login' command, but no
		/// <br/> 		    password prompting is invoked.  If the server
		/// <br/> 		    determines that the user is valid, they are issued a
		/// <br/> 		    Perforce ticket just as if they had logged in with a
		/// <br/> 		    password.
		/// <br/> 
		/// <br/> 		    Pre-2007.2 clients cannot run a client-side single
		/// <br/> 		    sign-on.  Specifying an 'auth-check' trigger as a backup
		/// <br/> 		    for a user to gain access will prompt the user for a
		/// <br/> 		    password if it's an older client or P4LOGINSSO has not
		/// <br/> 		    been configured.
		/// <br/> 
		/// <br/> 		    Unlike passwords which are encrypted, the sso message is
		/// <br/> 		    sent to the server in clear text.
		/// <br/> 
		/// <br/> 		auth-set:
		/// <br/> 		    Execute an authentication set trigger to send a new
		/// <br/> 		    password to an external password manager.
		/// <br/> 
		/// <br/> 		change-submit:
		/// <br/> 		    Execute pre-submit trigger after changelist has been
		/// <br/> 		    created and files locked but prior to file transfer.
		/// <br/> 
		/// <br/> 		change-content:
		/// <br/> 		    Execute mid-submit trigger after file transfer but prior
		/// <br/> 		    to commit.  Files can be accessed by the 'p4 diff2',
		/// <br/> 		    'p4 files', 'p4 fstat', and 'p4 print' commands using
		/// <br/> 		    the revision specification '@=change', where 'change' is
		/// <br/> 		    the pending changelist number passed as %changelist%.
		/// <br/> 
		/// <br/> 		change-commit:
		/// <br/> 		    Execute post-submit trigger after changelist commit.
		/// <br/> 
		/// <br/> 		fix-add:
		/// <br/> 		    Execute fix trigger prior to adding a fix.  The special
		/// <br/> 		    variable %jobs% is available for expansion and must be
		/// <br/> 		    the last argument to the trigger as it expands to one
		/// <br/> 		    argument for each job listed on the 'p4 fix' command.
		/// <br/> 
		/// <br/> 		fix-delete:
		/// <br/> 		    Execute fix trigger prior to deleting a fix.  The special
		/// <br/> 		    variable %jobs% is available for expansion and must be
		/// <br/> 		    the last argument to the trigger as it expands to one
		/// <br/> 		    argument for each job listed on the 'p4 fix -d' command.
		/// <br/> 
		/// <br/> 		form-out:
		/// <br/> 		    Execute form trigger on generation of form.	 Trigger may
		/// <br/> 		    modify form.
		/// <br/> 
		/// <br/> 		form-in:
		/// <br/> 		    Execute form trigger on input of form before its contents
		/// <br/> 		    are parsed and validated.  Trigger may modify form.
		/// <br/> 
		/// <br/> 		form-save:
		/// <br/> 		    Execute form trigger prior to save of form after its
		/// <br/> 		    contents are parsed.
		/// <br/> 
		/// <br/> 		form-commit:
		/// <br/> 		    Execute form trigger after it has been committed, allowing
		/// <br/> 		    access to automatically generated fields (jobname, dates
		/// <br/> 		    etc).  It cannot modify the form.  This trigger for job
		/// <br/> 		    forms is run by 'p4 job' and 'p4 fix' (after the status
		/// <br/> 		    is updated), 'p4 change' (if the job is added or deleted)
		/// <br/> 		    and 'p4 submit' (if the job is associated with the change).
		/// <br/> 		    The 'form-commit' trigger has access to the new job name
		/// <br/> 		    created with 'p4 job', while the 'form-in' and 'form-save'
		/// <br/> 		    triggers are run before the job name is created.  The
		/// <br/> 		    special variable %action% is available on the job
		/// <br/> 		    'form-commit' trigger command line, and is expanded when
		/// <br/> 		    the job is modified by a fix.
		/// <br/> 
		/// <br/> 		form-delete:
		/// <br/> 		    Execute form trigger prior to delete of form after its
		/// <br/> 		    contents are parsed.
		/// <br/> 
		/// <br/> 		shelve-submit:
		/// <br/> 		    Execute pre-shelve trigger after changelist has been
		/// <br/> 		    created but prior to file transfer.
		/// <br/> 
		/// <br/> 		shelve-commit:
		/// <br/> 		    Execute post-shelve trigger after files are shelved.
		/// <br/> 
		/// <br/> 		shelve-delete:
		/// <br/> 		    Execute shelve trigger prior to discarding shelved files.
		/// <br/> 
		/// <br/> 	Path:   For change and submit triggers, a file pattern to match files
		/// <br/> 		in the changelist.  This file pattern can be an exclusion
		/// <br/> 		mapping (-pattern), to exclude files.  For form triggers, the
		/// <br/> 		name of the form (branch, client, etc).  For fix triggers
		/// <br/> 		'fix' is required as the path value.  For authentication
		/// <br/> 		triggers, 'auth' is required as the path value. For archive
		/// <br/> 		triggers, a file pattern to match the name of the file being
		/// <br/> 		accessed in the archive.  Note that, due to lazy copying when
		/// <br/> 		branching files, the name of the file in the archive can not
		/// <br/> 		be the same as the name of the file in the depot.
		/// <br/> 
		/// <br/> 	Command: The OS command to run for validation.  If the command
		/// <br/> 		contains spaces, enclose it in double quotes.  The
		/// <br/> 		following variables are expanded in the command string:
		/// <br/> 
		/// <br/> 		    %client% -- the client issuing the command
		/// <br/> 		    %clienthost% -- the hostname of the client
		/// <br/> 		    %clientip% -- the IP address of the client
		/// <br/> 		    %serverhost% -- the hostname of the server
		/// <br/> 		    %serverip% -- the IP address of the server
		/// <br/> 		    %serverport% -- the IP address:port of the server
		/// <br/> 		    %serverroot% -- the value of the server's $P4ROOT
		/// <br/> 		    %user% -- the user issuing the command
		/// <br/> 
		/// <br/> 		    %changelist% -- the changelist being submitted
		/// <br/> 		    %changeroot% -- the root path of files submitted
		/// <br/> 		    %oldchangelist% -- the pre-commit changelist number
		/// <br/> 
		/// <br/> 			(More information can be gathered about the
		/// <br/> 			changelist being submitted by running
		/// <br/> 			'p4 describe %changelist%'.)
		/// <br/> 
		/// <br/> 		    %formfile% -- path to temp file containing form
		/// <br/> 		    %formname% -- the form's name (branch name, etc)
		/// <br/> 		    %formtype% -- the type of form (branch, etc)
		/// <br/> 		    %action% -- added/deleted/submitted on job form-commit
		/// <br/> 
		/// <br/> 		    %jobs% -- list of job names for fix triggers
		/// <br/> 
		/// <br/> 		    %op% -- read/write/delete for archive access
		/// <br/> 		    %file% -- name of archive file
		/// <br/> 		    %rev% -- revision of archive file
		/// <br/> 
		/// <br/> 		The command's standard input is empty for change, shelve,
		/// <br/> 		fix, and auth triggers; it is the form contents for form
		/// <br/> 		triggers; and it is the file content for the archive trigger.
		/// <br/> 
		/// <br/> 		If the command fails, the command's standard output (not
		/// <br/> 		error output) is sent to the client as the text of a trigger
		/// <br/> 		failure error message.
		/// <br/> 
		/// <br/> 		If the command succeeds, the command's standard output is
		/// <br/> 		sent as an unadorned message to the client for all triggers
		/// <br/> 		except archive triggers; for archive triggers, the command's
		/// <br/> 		standard output is the file content.
		/// <br/> 
		/// <br/> 	The -o flag writes the trigger table to the standard output.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads the trigger table from the standard input.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	'p4 triggers' requires 'super' access granted by 'p4 protect'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public IList<Trigger> GetTriggerTable(Options options)
		{
			P4.P4Command triggersCmd = new P4Command(this, "triggers", true);
			P4.P4CommandResult r = triggersCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			List<Trigger> value = new List<Trigger>();

			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{
				System.Text.StringBuilder sb = new StringBuilder();
				foreach (KeyValuePair<string, string> key in obj)
				{
					sb.Remove(0, sb.Length);
					sb.AppendLine((string.Format("{0} {1}", key.Key.ToString(), key.Value)));
					string line = sb.ToString();
					if (line.StartsWith("Triggers"))
					{
						line = line.Trim();
						string[] entries = line.Split(' ');
						string name = entries[1];
						string ent = entries[2];
						ent = ent.Replace("-","");
						StringEnum<TriggerType> type = ent;
						string path = entries[3];
						string command = entries[4] + " " + entries[5];
						string ord = entries[0];
						ord = ord.Remove(0, 8);
						int order = 0;
						order = Convert.ToInt16(ord);
						Trigger trig = new Trigger(name, order, type, path, command);
						value.Add(trig);
					}
				}
			}
			return value;
		}

		/// <summary>
		/// Get the repository's type map. 
		/// </summary>    
		/// <returns></returns>
		/// <remarks>
		/// runs the command p4 typemap -o
		/// </remarks>
		/// <remarks>
		/// <br/><b>p4 help typemap</b>
		/// <br/> 
		/// <br/>     typemap -- Edit the filename-to-filetype mapping table
		/// <br/> 
		/// <br/>     p4 typemap
		/// <br/>     p4 typemap -o
		/// <br/>     p4 typemap -i
		/// <br/> 
		/// <br/> 	'p4 typemap' edits a name-to-type mapping table for 'p4 add', which
		/// <br/> 	uses the table to assign a file's filetype based on its name.
		/// <br/> 
		/// <br/> 	The typemap form has a single field, 'TypeMap', followed by any
		/// <br/> 	number of typemap lines.  Each typemap line contains a filetype
		/// <br/> 	and a depot file path pattern:
		/// <br/> 
		/// <br/> 	Filetype:   See 'p4 help filetypes' for a list of valid filetypes.
		/// <br/> 
		/// <br/> 	Path:       Names to be mapped to the filetype.  The mapping is
		/// <br/> 		    a file pattern in depot syntax.  When a user adds a file
		/// <br/> 		    matching this pattern, its default filetype is the
		/// <br/> 		    file type specified in the table.  To exclude files from
		/// <br/> 		    the typemap, use exclusionary (-pattern) mappings.
		/// <br/> 		    To match all files anywhere in the depot hierarchy,
		/// <br/> 		    the pattern must begin with '//...'.  To match files
		/// <br/> 		    with a specified suffix, use '//.../*.suffix' or
		/// <br/> 		    use '//....suffix' (four dots).
		/// <br/> 
		/// <br/> 	Later entries override earlier entries. If no matching entry is found
		/// <br/> 	in the table, 'p4 add' determines the filetype by examining the file's
		/// <br/> 	contents and execution permission bits.
		/// <br/> 
		/// <br/> 	The -o flag writes the typemap table to standard output. The user's
		/// <br/> 	editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads the typemap table from standard input. The user's
		/// <br/> 	editor is not invoked.
		/// <br/> 
		/// <br/> 	'p4 typemap' requires 'admin' access, which is granted by 'p4 protect'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public IList<TypeMapEntry> GetTypeMap()
		{
			P4.P4Command typemapCmd = new P4Command(this, "typemap", true, "-o");
			P4.P4CommandResult r = typemapCmd.Run();
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			List<TypeMapEntry> value = new List<TypeMapEntry>();

			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{
				int ord = 0;
				string key = String.Format("TypeMap{0}", ord);

				while (obj.ContainsKey(key))
				{
					value.Add(new TypeMapEntry(obj[key]));
					ord++;
					key = String.Format("TypeMap{0}", ord);
				}
				return value;
			}
			return value;
		}

		//public string CreateSpec(List<TypeMapEntry> map)
		//{
		//    StringBuilder val = new StringBuilder(map.Count * 256);
		//    val.AppendLine("TypeMap:");
		//    for (int idx = 0; idx < map.Count; idx++)
		//    {
		//        val.AppendLine(String.Format("/t{0}", map[idx].ToString()));
		//    }
		//    return base.ToString();
		//}
		
		 

		/// <summary>
		/// Get the repository's protection table. 
		/// </summary>    
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help protect</b>
		/// <br/> 
		/// <br/>     protect -- Modify protections in the server namespace
		/// <br/> 
		/// <br/>     p4 protect
		/// <br/>     p4 protect -o
		/// <br/>     p4 protect -i
		/// <br/> 
		/// <br/> 	'p4 protect' edits the protections table in a text form.
		/// <br/> 
		/// <br/> 	Each line in the table contains a protection mode, a group/user
		/// <br/> 	indicator, the group/user name, client host ID and a depot file
		/// <br/> 	path pattern. Users receive the highest privilege that is granted
		/// <br/> 	on any line.
		/// <br/> 
		/// <br/> 	Note: remote depot are accessed using the pseudo-user 'remote'.
		/// <br/> 	To control access from other servers that define your server as
		/// <br/> 	a remote server, grant appropriate permissions to the 'remote' user.
		/// <br/> 
		/// <br/> 	     Mode:   The permission level or right being granted or denied.
		/// <br/> 		     Each permission level includes all the permissions above
		/// <br/> 		     it, except for 'review'. Each permission only includes
		/// <br/> 		     the specific right and no lesser rights.  This approach
		/// <br/> 		     enables you to deny individual rights without having to
		/// <br/> 		     re-grant lesser rights. Modes prefixed by '=' are rights.
		/// <br/> 		     All other modes are permission levels.
		/// <br/> 
		/// <br/>       Valid modes are:
		/// <br/> 
		/// <br/> 		     list   - users can see names but not contents of files;
		/// <br/> 			      users can see all non-file related metadata
		/// <br/> 			      (clients, users, changelists, jobs, etc.)
		/// <br/> 
		/// <br/> 		     read   - users can sync, diff, and print files
		/// <br/> 
		/// <br/> 		     open   - users can open files (add, edit. delete,
		/// <br/> 			      integrate)
		/// <br/> 
		/// <br/> 		     write  - users can submit open files
		/// <br/> 
		/// <br/> 		     admin  - permits those administrative commands and
		/// <br/> 			      command options that don't affect the server's
		/// <br/> 			      security.
		/// <br/> 
		/// <br/> 		     super  - access to all commands and command options.
		/// <br/> 
		/// <br/> 		     review - permits access to the 'p4 review' command;
		/// <br/> 			      implies read access
		/// <br/> 
		/// <br/> 		     =read  - if this right is denied, users can't sync,
		/// <br/> 			      diff, or print files
		/// <br/> 
		/// <br/> 		     =branch - if this right is denied, users are not
		/// <br/> 			       permitted to use files as a source
		/// <br/> 			       for 'p4 integrate'
		/// <br/> 
		/// <br/> 		     =open   = if this right is denied, users cannot open
		/// <br/> 			       files (add, edit, delete, integrate)
		/// <br/> 
		/// <br/> 		     =write  = if this right is denied, users cannot submit
		/// <br/> 			       open files
		/// <br/> 
		/// <br/> 	     Group/User indicator: specifies the grantee is a group or user.
		/// <br/> 
		/// <br/> 	     Name:   A Perforce group or user name; can include wildcards.
		/// <br/> 
		/// <br/> 	     Host:   The IP address of a client host; can include wildcards.
		/// <br/> 
		/// <br/> 	     Path:   The part of the depot to which access is being granted
		/// <br/> 	             or denied.  To deny access to a depot path, preface the
		/// <br/> 	             path with a "-" character. These exclusionary mappings
		/// <br/> 	             apply to all access levels, even if only one access
		/// <br/> 	             level is specified in the first field.
		/// <br/> 
		/// <br/> 	The -o flag writes the protection table	to the standard output.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads the protection table from the standard input.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	After protections are defined, 'p4 protect' requires 'super'
		/// <br/> 	access.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public IList<ProtectionEntry> GetProtectionTable(Options options)
		{
			P4.P4Command protectCmd = new P4Command(this, "protect", true);
			P4.P4CommandResult r = protectCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			List<ProtectionEntry> value = new List<ProtectionEntry>();

			foreach (P4.TaggedObject obj in r.TaggedOutput)
				
			{
				System.Text.StringBuilder sb = new StringBuilder();
				foreach (KeyValuePair<string, string> key in obj)
				{
					sb.Remove(0, sb.Length);
					sb.AppendLine((string.Format("{0} {1}", key.Key.ToString(), key.Value)));
					string line = sb.ToString();
					if (line.StartsWith("Protections"))
					{
					line = line.Trim();
					string[] entries = line.Split(' ');
					StringEnum<ProtectionMode> mode = entries[1];
					StringEnum<EntryType> type = entries[2];
					string grouporusername = entries[3];
					string host = entries[4];
					string path = entries[5];
					ProtectionEntry pe = new ProtectionEntry(mode, type, grouporusername, host, path);
					value.Add(pe);
					}
				}
			}
			return value;
		}

		/// <summary>
		/// Get the Perforce counters for this repository. 
		/// </summary>    
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help counters</b>
		/// <br/> 
		/// <br/>     counters -- Display list of known counters
		/// <br/> 
		/// <br/>     p4 counters
		/// <br/> 
		/// <br/> 	Lists the counters in use by the server.  The server
		/// <br/> 	uses the following counters directly:
		/// <br/> 
		/// <br/> 	    change           Current change number
		/// <br/> 	    job              Current job number
		/// <br/> 	    journal          Current journal number
		/// <br/> 	    lastCheckpointAction Data about the last complete checkpoint
		/// <br/> 	    logger           Event log index used by 'p4 logger'
		/// <br/> 	    traits           Internal trait lot number used by 'p4 attribute'
		/// <br/> 	    upgrade          Server database upgrade level
		/// <br/> 
		/// <br/> 	Other counters can be created by the 'p4 counter' or 'p4 review'
		/// <br/> 	commands.
		/// <br/> 
		/// <br/> 	The names 'minClient', 'minClientMessage', 'monitor',
		/// <br/> 	'security', and 'unicode' are reserved names: do not use them
		/// <br/> 	as ordinary counters.
		/// <br/> 
		/// <br/> 	For general-purpose server configuration, see 'p4 help configure'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public IList<Counter> GetCounters(Options options)
		{
			P4.P4Command countersCmd = new P4Command(this, "counters", true);
			P4.P4CommandResult r = countersCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			IList<Counter> val = new List<Counter>();

			foreach (P4.TaggedObject obj in r.TaggedOutput)
			{
				string name = obj["counter"];
				string value = obj["value"];
								
				Counter counter = new Counter(name, value);

				val.Add(counter);
			}
			return val;
		}
		 

		/// <summary>
		/// Get a named Perforce counter value from the repository. 
		/// </summary>    
		/// <param name="name"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help counter</b>
		/// <br/> 
		/// <br/>      counter -- Display, set, or delete a counter
		/// <br/> 
		/// <br/>      p4 counter name
		/// <br/>      p4 counter [-f] name value
		/// <br/>      p4 counter [-f] -d name
		/// <br/>      p4 counter [-f] -i name
		/// <br/> 
		/// <br/> 	The first form displays the value of the specified counter.
		/// <br/> 
		/// <br/> 	The second form sets the counter to the specified value.
		/// <br/> 
		/// <br/> 	The third form deletes the counter.  This option usually has the
		/// <br/> 	same effect as setting the counter to 0.
		/// <br/> 
		/// <br/> 	The -f flag sets or deletes counters used by Perforce,  which are
		/// <br/> 	listed by 'p4 help counters'. Important: Never set the 'change'
		/// <br/> 	counter to a value that is lower than its current value.
		/// <br/> 
		/// <br/> 	The -i flag increments a counter by 1 and returns the new value.
		/// <br/> 	This option is used instead of a value argument and can only be
		/// <br/> 	used with numeric counters.
		/// <br/> 
		/// <br/> 	Counters can be assigned textual values as well as numeric ones, 
		/// <br/> 	despite the name 'counter'.
		/// <br/> 
		/// <br/> 	'p4 counter' requires 'review' access granted by 'p4 protect'.
		/// <br/> 	The -f flag requires that the user be an operator or have 'super'
		/// <br/> 	access.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Counter GetCounter(String name, Options options)
		{
			P4.P4Command counterCmd = new  P4.P4Command(_connection, "counter", true, name);
			P4.P4CommandResult r = counterCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}
			if ((r.TaggedOutput == null) || (r.TaggedOutput.Count <= 0))
			{
				return null;
			}
			foreach (P4.TaggedObject obj in r.TaggedOutput)

			{
				string countername = obj["counter"];
				string value = obj["value"];

				Counter counter = new Counter(countername, value);
				return counter;
			}

			return null;

		}

		/// <summary>
		/// Delete a Perforce counter from the repository. 
		/// </summary>    
		/// <param name="name"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help counter</b>
		/// <br/> 
		/// <br/>      counter -- Display, set, or delete a counter
		/// <br/> 
		/// <br/>      p4 counter name
		/// <br/>      p4 counter [-f] name value
		/// <br/>      p4 counter [-f] -d name
		/// <br/>      p4 counter [-f] -i name
		/// <br/> 
		/// <br/> 	The first form displays the value of the specified counter.
		/// <br/> 
		/// <br/> 	The second form sets the counter to the specified value.
		/// <br/> 
		/// <br/> 	The third form deletes the counter.  This option usually has the
		/// <br/> 	same effect as setting the counter to 0.
		/// <br/> 
		/// <br/> 	The -f flag sets or deletes counters used by Perforce,  which are
		/// <br/> 	listed by 'p4 help counters'. Important: Never set the 'change'
		/// <br/> 	counter to a value that is lower than its current value.
		/// <br/> 
		/// <br/> 	The -i flag increments a counter by 1 and returns the new value.
		/// <br/> 	This option is used instead of a value argument and can only be
		/// <br/> 	used with numeric counters.
		/// <br/> 
		/// <br/> 	Counters can be assigned textual values as well as numeric ones, 
		/// <br/> 	despite the name 'counter'.
		/// <br/> 
		/// <br/> 	'p4 counter' requires 'review' access granted by 'p4 protect'.
		/// <br/> 	The -f flag requires that the user be an operator or have 'super'
		/// <br/> 	access.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Object DeleteCounter(String name, Options options)
		{
			StringList cmdArgs = new StringList();
			cmdArgs.Add("-f");
			cmdArgs.Add("-d");
			cmdArgs.Add(name);
			P4.P4Command delcounterCmd = new P4Command(this, "counter", false, cmdArgs);
			P4.P4CommandResult r = delcounterCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return null;
			}

			return r.InfoOutput;

		}


		#region IDisposable Members

		public void Dispose()
		{
			_connection.Dispose();
		}

		#endregion
	}
}
