/*******************************************************************************

Copyright (c) 2010, Perforce Software, Inc.  All rights reserved.

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
 * Name		: P4Command.cs
 *
 * Author	: dbb
 *
 * Description	: Classes encapsulting running a command on the P4 server, then
 *  collecting and returning the bundled results.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
///<summary>
/// test summary WHAT DOES THIS LOOK LIKE
/// 
/// 
///</summary>
namespace Perforce.P4
{
	///// <summary>
	///// TaggedInfo
	///// 
	///// Contains a leveled hashed table based on the output from the command.
	///// All Info a given level is gathered in a Hashtable under the next higher
	///// level, forming an object dictionary for the information returned by the
	///// command
	///// 
	///// level0 item (TaggedInfoItem)
	/////   level1 data (key:value)
	/////   level1 data (key:value)
	/////   level1 data (key:value)
	/////   ...
	/////   level1 Item (TaggedInfoItem)    *** NOT IMPLEMENTED ***
	/////     level2 data (key:value)  *** NOT IMPLEMENTED ***
	/////     level2 data (key:value)  *** NOT IMPLEMENTED ***
	/////     ...
	///// </summary>
	///// <remarks>
	///// This class is not need. Structured data representing objects should be 
	///// obtained using 'tagged' protocol to create object data. 
	///// </remarks>
	//public class TaggedInfo
	//{
	//    private TaggedInfoItem rootItem;

	//    public TaggedInfoItem RootItem
	//    {
	//        get { return rootItem; }
	//    }

	//    private int currentLevel;
	//    private TaggedInfoItem currentItem;

	//    public TaggedInfo()
	//    {
	//        rootItem = new TaggedInfoItem();

	//        currentLevel = -1;
	//        currentItem = rootItem;
	//    }

	//    // use info out to build up the list
	//    private void OnInfoOut(int level, String info)
	//    {
	//    }
	//}
	//public class TextOutput
	//{
	//    private String value;

	//    public TextOutput(String s)
	//    {
	//        value = s;
	//    }

	//    // user-defined conversion from TextOutput to String
	//    public static implicit operator String(TextOutput t)
	//    {
	//        return t.value;
	//    }

	//    // user-defined conversion from TextOutput to String[]
	//    public static implicit operator String[](TextOutput t)
	//    {
	//        return t.value.Split(new char[] { '\r','\n'});
	//    }

	//    // user-defined conversion from String to TextOutput
	//    public static implicit operator TextOutput(String s)
	//    {
	//        return new TextOutput(s);
	//    }

	//    public String[] Lines
	//    {
	//        get { return value.Split(new char[] { '\r','\n'}); }
	//    }
	//}

	/// <summary>
	/// Class wrapping command execution.
	/// </summary>
	public class P4Command : IDisposable
	{
		// Server
		public P4Server pServer { get; private set; }

		// Server
		public Connection Connection { get; private set; }

		/// <summary>
		/// Command opcode
		/// </summary>
		private String cmd = String.Empty;

		/// <summary>
		/// Unique Id set each time command is run
		/// </summary>
		public uint CommandId { get; private set; }
		/// <summary>
		/// The arguments used by the command
		/// </summary>
		private StringList args;

		/// <summary>
		/// Tagged protocol flag
		/// </summary>
		private bool tagged;

		private Dictionary<String, String> responses;

		/// <summary>
		/// Capture info results so they can be reformatted
		/// </summary>
		public P4Server.InfoResultsDelegate infoResultsCallbackFn = null;

		// various flavors of output collected over the run of this command
		InfoList infoOutput = null;
		//ClientErrorList errorOutput = null;
		//String textOutput = null;
		//TaggedObjectList taggedOutput = null;
		//byte[] binaryOutput = null;

		// Our override so the info output can be built up as a list and not a string
		private P4Server.InfoResultsDelegate onInfoResultsDelegate = null;

		// Handle any Resolve callbacks  from the server
		public P4Server.ResolveHandlerDelegate CmdResolveHandler { get; set; }

		// Handle any Resolve callbacks  from the server
		public P4Server.ResolveAHandlerDelegate CmdResolveAHandler { get; set; }

		// Handle any input prompts from the server
		public P4Server.PromptHandlerDelegate CmdPromptHandler { get; set; }

		/// <summary>
		/// Get the info results from the command execution
		/// </summary>
		public InfoList InfoOutput
		{
			get { return infoOutput; }
		}

		/// <summary>
		/// Get the error results from the command execution
		/// </summary>
		public P4ClientErrorList ErrorOutput
		{
			get 
			{
				P4ClientError conErr = pServer.ConnectionError;
				P4ClientErrorList errors = pServer.GetErrorResults(CommandId);
				if (conErr != null)
				{
					if ((errors == null) || (errors.Count == 0))
					{
						errors = new P4ClientErrorList(conErr);
					}
					else
					{
						errors.Insert(0, conErr);
					}
				}
				return errors; 
			}
		}

		/// <summary>
		/// Get the text output from the command execution
		/// </summary>
		public String TextOutput
		{
			get { return pServer.GetTextResults(CommandId); }
		}

		/// <summary>
		/// Get the tagged results from the command execution
		/// </summary>
		public TaggedObjectList TaggedOutput
		{
			get { return pServer.GetTaggedOutput(CommandId); }
		}

		/// <summary>
		/// Get the binary from the command execution
		/// </summary>
		public byte[] BinaryOutput
		{
			get { return pServer.GetBinaryResults(CommandId); }
		}

		/// <summary>
		/// Create a new command
		/// </summary>
		public P4Command(P4Server server)
			:this (server, null)
		{
		}

		/// <summary>
		/// Create a new command
		/// </summary>
		public P4Command(P4Server server, P4Server.PromptHandlerDelegate promptHandler)
		{
			if (server == null)
			{
				throw new ArgumentNullException("server",
					"P4Command requires a P4Server");
			}
			pServer = server;

			CommandId = server.getCmdId();

			onInfoResultsDelegate =
				new P4Server.InfoResultsDelegate(OnInfoOut);

			if (promptHandler != null)
				CmdPromptHandler = promptHandler;
			else
				CmdPromptHandler =
					new P4Server.PromptHandlerDelegate(HandlePrompt);

		}

		/// <summary>
		/// Constructer
		/// </summary>
		/// <param name="connection">Connection to the target Repository</param>
		/// <param name="command">Command String i.e 'submit'</param>
		/// <param name="taggedOutput">Run in tagged protocol</param>
		/// <param name="arguments">Arguments for the command</param>
		public P4Command(	Connection connection,
							String command,
							bool taggedOutput,
							params String[] arguments)
			: this(connection._p4server, command, null, taggedOutput, arguments)
		{
			Connection = connection;
		}

		/// <summary>
		/// Constructer
		/// </summary>
		/// <param name="repository">Target Repository</param>
		/// <param name="command">Command String i.e 'submit'</param>
		/// <param name="taggedOutput">Run in tagged protocol</param>
		/// <param name="arguments">Arguments for the command</param>
		public P4Command(	Repository repository,
							String command,
							bool taggedOutput,
							params String[] arguments)
			: this(repository.Connection._p4server, command, null, taggedOutput, arguments)
		{
			Connection = repository.Connection;
		}

		/// <summary>
		/// Constructer
		/// </summary>
		/// <param name="repository">Target Repository</param>
		/// <param name="command">Command String i.e 'submit'</param>
		/// <param name="taggedOutput">Run in tagged protocol</param>
		/// <param name="arguments">Arguments for the command</param>
		public P4Command(	Repository repository,
							String command,
							P4Server.PromptHandlerDelegate promptHandler,
							bool taggedOutput,
							params String[] arguments)
			: this(repository.Connection._p4server, command, promptHandler, taggedOutput, arguments)
		{
			Connection = repository.Connection;
		}

		/// <summary>
		/// Constructer
		/// </summary>
		/// <param name="server">Target P4Server</param>
		/// <param name="command">Command String i.e 'submit'</param>
		/// <param name="taggedOutput">Run in tagged protocol</param>
		/// <param name="arguments">Arguments for the command</param>
		public P4Command(	P4Server server,
							String command,
							bool taggedOutput,
							params String[] arguments)
			: this(server, command, null, taggedOutput, arguments)
		{
		}

		/// <summary>
		/// Constructer
		/// </summary>
		/// <param name="server">Target P4Server</param>
		/// <param name="command">Command String i.e 'submit'</param>
		/// <param name="taggedOutput">Run in tagged protocol</param>
		/// <param name="arguments">Arguments for the command</param>
		public P4Command(   P4Server server,
							String command,
							P4Server.PromptHandlerDelegate promptHandler,
							bool taggedOutput,
							params String[] arguments)
			: this(server, promptHandler)
		{
			cmd = command;
			tagged = taggedOutput;
			args = arguments;
		}

		/// <summary>
		/// Command String i.e 'submit'
		/// </summary>
		public String Cmd
		{
			get { return cmd; }
			set { cmd = value; }
		}

		/// <summary>
		/// Arguments for the command
		/// </summary>
		public StringList Args
		{
			get { return args; }
			set { args = value; }
		}

		/// <summary>
		/// Run in tagged protocol
		/// </summary>
		public bool Tagged
		{
			get { return tagged; }
			set { tagged = value; }
		}

		/// <summary>
		/// Dictionary of responses to prompts from the server, where the key
		/// is the expected prompt from the server and the value is the 
		/// desired response.
		/// </summary>
		public Dictionary<String, String> Responses
		{
			get { return responses; }
			set { responses = value; }
		}

		/// <summary>
		/// Use the infoResultsReceived event to build up a list of info data.
		/// </summary>
		/// <param name="level">level of the message</param>
		/// <param name="info">message text</param>
		private void OnInfoOut(uint cmdId, int level, String info)
		{
			infoOutput.Add(new InfoLine(cmdId, level, info));
		}

		/// <summary>
		/// Respond to a prompt from the server for input
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="displayText"></param>
		/// <returns></returns>
		private String HandlePrompt(uint cmdId, String msg, bool displayText)
		{
			if ((responses == null) || (cmdId != CommandId))
				return null;

			if (responses.ContainsKey(msg))
				return responses[msg];
			if (responses.ContainsKey("DefaultResponse"))
				return responses["DefaultResponse"];
			if (responses.ContainsKey(String.Empty))
				return responses[String.Empty];
			return null;
		}

		/// <summary>
		/// Data to be processed by the command
		/// </summary>
		public String DataSet
		{
			get { return pServer.GetDataSet(CommandId); }
			set { pServer.SetDataSet(CommandId, value); }
		}
		/// <summary>
		/// Run the command supplying additional arguments
		/// </summary>
		/// <param name="flags">Additional arguments inserted in front of the current arguments</param>
		/// <returns>Success/Failure</returns>
		public P4CommandResult Run(StringList flags)
		{
			lock (this)
			{
				P4CommandResult results = null;
				results = new P4CommandResult(this, flags);
				return results;
			}
		}

		/// <summary>
		/// Run the command using the existing arguments
		/// </summary>
		/// <returns></returns>
		public P4CommandResult Run()
		{
			return new P4CommandResult(this);
		}

		/// <summary>
		/// Submit the default changelist
		/// </summary>
		/// <param name="description">Description to be included with the changelist</param>
		/// <returns></returns>
		//public P4CommandResult Submit(String description)
		//{
		//    if (String.IsNullOrEmpty(description))
		//    {
		//        P4Exception.Throw(ErrorSeverity.E_WARN, "Must provide description when checking in default change list");
		//        return null;
		//    }
		//    return Submit(false, false, null, description, -1, null, null);
		//}

		///// <summary>
		///// Submit the default changelist, with options
		///// </summary>
		///// <param name="reopenFiles"></param>
		///// <param name="updateJobStatus"></param>
		///// <param name="overrideWsOptions"></param>
		///// <param name="description"></param>
		///// <returns></returns>
		//public P4CommandResult Submit(bool reopenFiles, bool updateJobStatus, P4Workspace.SubmitFlags overrideWsOptions, String description)
		//{
		//    if (String.IsNullOrEmpty(description))
		//    {
		//        P4Exception.Throw(ErrorSeverity.E_WARN, "Must provide description when checking in default change list");
		//        return null;
		//    }
		//    return Submit(reopenFiles, updateJobStatus, overrideWsOptions, null, -1, null, null);
		//}

		///// <summary>
		///// Submit the numbered pending changelist
		///// </summary>
		///// <param name="reopenFiles"></param>
		///// <param name="updateJobStatus"></param>
		///// <param name="overrideWsOptions"></param>
		///// <param name="changeListNumber"></param>
		///// <returns></returns>
		//public P4CommandResult Submit(bool reopenFiles, bool updateJobStatus, P4Workspace.SubmitFlags overrideWsOptions, long changeListNumber)
		//{
		//    return Submit(reopenFiles, updateJobStatus, overrideWsOptions, null, changeListNumber, null, null);
		//}

		///// <summary>
		///// Submit the new changelist
		///// </summary>
		///// <param name="reopenFiles"></param>
		///// <param name="updateJobStatus"></param>
		///// <param name="overrideWsOptions"></param>
		///// <param name="changeList"></param>
		///// <returns></returns>
		//public P4CommandResult Submit(bool reopenFiles, bool updateJobStatus, P4Workspace.SubmitFlags overrideWsOptions, P4Change changeList)
		//{
		//    return Submit(reopenFiles, updateJobStatus, overrideWsOptions, null, -1, changeList, null);

		//}

		///// <summary>
		///// Submit a single file
		///// </summary>
		///// <param name="reopenFiles"></param>
		///// <param name="updateJobStatus"></param>
		///// <param name="overrideWsOptions"></param>
		///// <param name="description"></param>
		///// <param name="file"></param>
		///// <returns></returns>
		//public P4CommandResult Submit(bool reopenFiles, bool updateJobStatus, P4Workspace.SubmitFlags overrideWsOptions, String description, String file)
		//{
		//    if (String.IsNullOrEmpty(description))
		//    {
		//        P4Exception.Throw(ErrorSeverity.E_WARN, "Must provide description when checking in from the default change list");
		//        return null;
		//    }
		//    return Submit(reopenFiles, updateJobStatus, overrideWsOptions, description, -1, null, file);
		//}

		///// <summary>
		///// Run a submit command
		///// </summary>
		///// <remarks>The changelistNumbe, changeList, and</remarks>
		///// <param name="reopenFiles"></param>
		///// <param name="updateJobStatus"></param>
		///// <param name="overrideWsOptions"></param>
		///// <param name="description"></param>
		///// <param name="changeListNumber"></param>
		///// <param name="changeList"></param>
		///// <param name="file"></param>
		///// <returns></returns>
		//internal P4CommandResult Submit(bool reopenFiles, bool updateJobStatus, P4Workspace.SubmitFlags overrideWsOptions, 
		//    String description, long changeListNumber, P4Change changeList, String file)
		//{
		//    cmd = "submit";
		//    tagged = true;

		//    args = new StringList();

		//    if (reopenFiles)
		//        args.Add("-r");

		//    if (updateJobStatus)
		//        args.Add("-s");

		//    if ((overrideWsOptions != null) && (overrideWsOptions != P4Workspace.SubmitFlagVals.none))
		//    {
		//        args.Add("-f");
		//        args.Add(overrideWsOptions.ToString());
		//    }
		//    if (!String.IsNullOrEmpty(description))
		//    {
		//        args.Add("-d");
		//        args.Add(description);
		//    }
		//    // submitting a new changelist by change# so ignore the changelist and file params
		//    if (changeListNumber > -1)
		//    {
		//        args.Add("-c");
		//        args.Add(changeListNumber.ToString());

		//        return Run(args);
		//    }

		//    // submitting a new changelist using the -i option, so ignore the params list
		//    else if (changeList != null)
		//    {
		//        args.Add("-i");

		//        pServer.DataSet = changeList.ToString();
		//        return Run(args);
		//    }
		//    if (!String.IsNullOrEmpty(file))
		//        args.Add(file);

		//    return Run(); ;
		//}

		///// <summary>
		///// Options for automatic resolve
		///// </summary>
		//public enum AutomaticResolve : int { 
		//    /// <summary>
		//    /// 
		//    /// </summary>
		//    none = 0,
		//    /// <summary>
		//    /// Automatic Mode
		//    /// </summary>
		//    /// <remarks>
		//    /// Automatic mode: if there are conflicts, the file is skipped; 
		//    /// if there are no conflicts and yours hasn't changed it accepts 
		//    /// theirs; if theirs hasn't changed it accepts yours; if both 
		//    /// yours and theirs have changed it accepts the merge. Files that
		//    /// have no base for merging (such as binary files) are skipped.
		//    /// </remarks>
		//    AutoMerge,
		//    /// <summary>
		//    /// Force Merge
		//    /// </summary>
		//    /// <remarks>
		//    /// Accept the  merged file even if there are conflicts.
		//    /// </remarks>
		//    ForceMerge,
		//    /// <summary>
		//    /// Safe Merge
		//    /// </summary>
		//    /// <remarks>
		//    /// Accept only files that have either your changes or their 
		//    /// changes, but not both. Files with changes to both yours and 
		//    /// theirs are skipped.
		//    /// </remarks>
		//    Safe,
		//    /// <summary>
		//    /// Accept Theirs
		//    /// </summary>
		//    /// <remarks>
		//    /// Skip the merge.  Instead it automatically accepts their version 
		//    /// of the file.  It should be used with care, as it overwrites any 
		//    /// changes made to the file in the client workspace.
		//    /// </remarks>
		//    Theirs,
		//    /// <summary>
		//    /// Accept Yours
		//    /// </summary>
		//    /// <remarks>
		//    /// Skip the merge.  Instead it automatically accepts your version 
		//    /// of the file.
		//    /// </remarks>
		//    Yours 
		//};
		///// <summary>
		///// The corresponding flags for the automatic resolve
		///// </summary>
		//private static String[] AutomaticResolveFlags = new String[] { String.Empty, "-am", "-af", "-as", "-at", "-ay" };
		///// <summary>
		///// Diff option flags
		///// </summary>
		//[Flags]
		//public enum DiffOptions
		//{
		//    /// <summary>
		//    /// None
		//    /// </summary>
		//    none = 0x0000,
		//    /// <summary>
		//    /// Ignore Whitespace Changes
		//    /// </summary>
		//    IgnoreWhitespaceChanges = 0x0001,
		//    /// <summary>
		//    /// Ignore Whitespace
		//    /// </summary>
		//    IgnoreWhitespace = 0x0002,
		//    /// <summary>
		//    /// Ignore Line Endings
		//    /// </summary>
		//    IgnoreLineEndings = 0x0004,
		//    /// <summary>
		//    /// RCS output
		//    /// </summary>
		//    RCS = 0x0008,
		//    /// <summary>
		//    /// Show context of changes
		//    /// </summary>
		//    context = 0x0010,
		//    /// <summary>
		//    /// Summary
		//    /// </summary>
		//    summary = 0x0020,
		//    /// <summary>
		//    /// Unified
		//    /// </summary>
		//    unified = 0x0040
		//};
		///// <summary>
		///// Add the diff options to a parameter list
		///// </summary>
		///// <param name="options">The diff options</param>
		///// <param name="flagList">Parameter list to be appended</param>
		//internal void AddDiffOptions(DiffOptions options, ref StringList flagList)
		//{
		//    if (options == DiffOptions.none)
		//        return;

		//    if ((options & DiffOptions.IgnoreWhitespaceChanges) != 0)
		//        flagList.Add("-db");
		//    if ((options & DiffOptions.IgnoreWhitespace) != 0)
		//        flagList.Add("-dw");
		//    if ((options & DiffOptions.IgnoreLineEndings) != 0)
		//        flagList.Add("-dl");
		//    if ((options & DiffOptions.RCS) != 0)
		//        flagList.Add("-dn");
		//    if ((options & DiffOptions.context) != 0)
		//        flagList.Add("-dc");
		//    if ((options & DiffOptions.summary) != 0)
		//        flagList.Add("-ds");
		//    if ((options & DiffOptions.unified) != 0)
		//        flagList.Add("-du");

		//    return;
		//}

		///// <summary>
		///// Run a resolve command using one of the automatic resolve actions
		///// </summary>
		///// <param name="mode">Which mode of automatic resolve to use</param>
		///// <param name="diffOptions">Options to use when diffing the two files</param>
		///// <param name="force">Enable resolving files previously marked as resolved</param>
		///// <param name="previewOnly">Preview only</param>
		///// <param name="displayBaseFile">Displays the base file name and revision to be used</param>
		///// <param name="forceTextualMerge">Force merge to attempt a textual merge</param>
		///// <param name="markAllChanges">Insert markers for all changes</param>
		///// <param name="files">Optional list of files to resolve</param>
		///// <returns></returns>
		//public P4CommandResult Resolve(AutomaticResolve mode,
		//    DiffOptions diffOptions, bool force, bool previewOnly,
		//    bool displayBaseFile, bool forceTextualMerge,
		//    bool markAllChanges, params String[] files)
		//{
		//    if (mode == AutomaticResolve.none)
		//    {
		//        throw new ArgumentException("Must specify an automatic merging mode.", "mode");
		//    }
		//    return Resolve(mode, null, null, diffOptions, force, previewOnly, displayBaseFile, forceTextualMerge, markAllChanges, files);
		//}

		///// <summary>
		///// Run a resolve supplying a dictionary of responses for prompts
		///// </summary>
		///// <param name="responses">Dictionary of responses keyed by the expected prompt string</param>
		///// <param name="diffOptions">Options to use when diffing the two files</param>
		///// <param name="force">Enable resolving files previously marked as resolved</param>
		///// <param name="previewOnly">Preview only</param>
		///// <param name="displayBaseFile">Displays the base file name and revision to be used</param>
		///// <param name="forceTextualMerge">Force merge to attempt a textual merge</param>
		///// <param name="markAllChanges">Insert markers for all changes</param>
		///// <param name="files">Optional list of files to resolve</param>
		///// <returns></returns>
		//public P4CommandResult Resolve(Dictionary<String, String> responses,
		//    DiffOptions diffOptions, bool force, bool previewOnly,
		//    bool displayBaseFile, bool forceTextualMerge,
		//    bool markAllChanges, params String[] files)
		//{
		//    if (responses == null)
		//    {
		//        throw new ArgumentNullException("responses");
		//    }
		//    return Resolve(AutomaticResolve.none, responses, null, diffOptions, force, previewOnly, displayBaseFile, forceTextualMerge, markAllChanges, files);
		//}

		///// <summary>
		///// Performing a resolve providing a delegate to handle prompts
		///// </summary>
		///// <param name="promptHandler">Prompt handler delegate</param>
		///// <param name="diffOptions">Options to use when diffing the two files</param>
		///// <param name="force">Enable resolving files previously marked as resolved</param>
		///// <param name="previewOnly">Preview only</param>
		///// <param name="displayBaseFile">Displays the base file name and revision to be used</param>
		///// <param name="forceTextualMerge">Force merge to attempt a textual merge</param>
		///// <param name="markAllChanges">Insert markers for all changes</param>
		///// <param name="files">Optional list of files to resolve</param>
		///// <returns></returns>
		//public P4CommandResult Resolve(P4Server.PromptHandlerDelegate promptHandler,
		//    DiffOptions diffOptions, bool force, bool previewOnly,
		//    bool displayBaseFile, bool forceTextualMerge,
		//    bool markAllChanges, params String[] files)
		//{
		//    if (promptHandler == null)
		//    {
		//        throw new ArgumentNullException("promptHandler");
		//    }
		//    return Resolve(AutomaticResolve.none, null, promptHandler, diffOptions, force, previewOnly, displayBaseFile, forceTextualMerge, markAllChanges, files);
		//}

		//internal P4CommandResult Resolve(AutomaticResolve mode,
		//    Dictionary<String, String> promtpResponses,
		//    P4Server.PromptHandlerDelegate promptHandler,
		//    DiffOptions diffOptions, bool force, bool previewOnly,
		//    bool displayBaseFile, bool forceTextualMerge,
		//    bool markAllChanges, params String[] files)
		//{
		//    cmd = "resolve";
		//    tagged = false;

		//    args = files;
		//    StringList flags = new StringList();

		//    if (mode != AutomaticResolve.none)
		//    {
		//        flags.Add(AutomaticResolveFlags[(int)mode]);
		//    }
		//    else if (promtpResponses != null)
		//    {
		//        responses = promtpResponses;
		//    }
		//    else if (promptHandler != null)
		//    {
		//        CmdPromptHandler = promptHandler;
		//    }
		//    else
		//    {
		//        throw new ArgumentException("Must provide either AutomaticResolve mode, Prompt Response dictionary, or a Prompt Handler");
		//    }
		//    AddDiffOptions(diffOptions, ref flags);

		//    if (force)
		//        flags.Add("-f");

		//    if (previewOnly)
		//        flags.Add("-n");

		//    if (displayBaseFile)
		//        flags.Add("-o");

		//    if (forceTextualMerge)
		//        flags.Add("-t");

		//    if (markAllChanges)
		//        flags.Add("-v");

		//    P4CommandResult results = Run(flags);

		//    if (promptHandler != null) // reset the prompt handle if overridden
		//        CmdPromptHandler = new P4Server.PromptHandlerDelegate(HandlePrompt);
		//    return results;
		//}

		///// <summary>
		///// Diff file selection options, only list the files that satisfy the specified criteria
		///// </summary>
		//public enum DiffFileList : int {
		//    /// <summary>
		//    /// None
		//    /// </summary>
		//    none = 0, 
		//    /// <summary>
		//    /// Opened files that differ from the revision in the depot or are missing.
		//    /// </summary>
		//    a, 
		//    /// <summary>
		//    /// Files that have been opened for integrate, resolved, and subsequently 
		//    /// modified. 
		//    /// </summary>
		//    b, 
		//    /// <summary>
		//    /// Unopened files that are missing on the client.
		//    /// </summary>
		//    d,
		//    /// <summary>
		//    /// Unopened files that differ from the revision in the depot.
		//    /// </summary>
		//    e,  
		//    /// <summary>
		//    /// Every unopened file, along with the status of 'same', 'diff', or 'missing' 
		//    /// as compared to its revision in the depot.
		//    /// </summary>
		//    l, 
		//    /// <summary>
		//    /// Opened files that do not differ from the revision in the depot.
		//    /// </summary>
		//    r 
		//};
		///// <summary>
		///// Map of the diff options to the strings for the flags
		///// </summary>
		//private static String[] DiffFileListFlags = new String[] {"-sa", "-sba", "-sda", "-sea", "-sla", "-sr" }; 

		///// <summary>
		///// Run a diff command
		///// </summary>
		///// <param name="diffOptions">Modify the output based on the specified option</param>
		///// <param name="force">diff every file, regardless of whether they are opened
		///// or the client has synced the specified revision</param>
		///// <param name="maxFiles">limit output to the first 'max' number of files</param>
		///// <param name="listOptions">Only list the files that satisfy the specified criteria</param>
		///// <param name="forceTextualMerge"></param>
		///// <param name="files">(optional) list of files to diff</param>
		///// <returns>The results of the command</returns>
		//internal P4CommandResult Diff(DiffOptions diffOptions, 
		//    bool force, int maxFiles, DiffFileList listOptions,
		//    bool forceTextualMerge, params String[] files)
		//{
		//    cmd = "diff";
		//    tagged = false;

		//    args = files;
		//    StringList flags = new StringList();

		//    AddDiffOptions(diffOptions, ref flags);

		//    if (force)
		//        flags.Add("-f");

		//    if (maxFiles > 0)
		//    {
		//        flags.Add("-m");
		//        flags.Add(maxFiles.ToString());
		//    }
		//    if (listOptions != DiffFileList.none)
		//        flags.Add(DiffFileListFlags[(int)listOptions]);

		//    if (forceTextualMerge)
		//        flags.Add("-t");

		//    return Run(flags);
		//}

		/// <summary>
		/// Run the command supplying additional arguments
		/// </summary>
		/// <param name="flags">Additional arguments inserted in front of the current arguments</param>
		/// <returns></returns>
		internal bool RunInt(StringList flags)
		{
			lock (this)
			{
				// Capture the the info output
				if (onInfoResultsDelegate != null)
					pServer.InfoResultsReceived += onInfoResultsDelegate;

				// Handle any Resolve callbacks from the server
				if (CmdResolveHandler != null)
					pServer.ResolveHandler = CmdResolveHandler;

				// Handle any Resolve callbacks from the server
				if (CmdResolveAHandler != null)
					pServer.ResolveAHandler = CmdResolveAHandler;

				// Handle any prompts for input from the server
				if (CmdPromptHandler != null)
					pServer.PromptHandler = CmdPromptHandler;

				// clear any saved results
				infoOutput = new InfoList();
				//errorOutput = null;
				//textOutput = null;
				//taggedOutput = null;

				Exception lastError = null;

				bool success = false;
				try
				{
					StringList paramList = flags + args;

					pServer.EchoCommand(cmd, paramList);

					while (true)
					{
						//retries--;
						try
						{
							success = pServer.RunCommand(	cmd,
															CommandId,
															tagged,
															paramList,
															paramList == null ? 0 : paramList.Count);
							break;
						}
						catch (P4Exception ex)
						{
							if (ex is P4CommandCanceledException)
							{
								throw;
							}
							if (ex is P4CommandTimeOutException)
							{
								if (Connection != null)
								{
									Connection.Disconnect();
								}
								throw;
							}
							if (lastError != null)
							{
								if (Connection != null)
								{
									Connection.Disconnect();
								}
								// been here before, so don't try again
								string msg = string.Format("The connection to the Perforce server at {0} has been lost", pServer.Port);
								P4Exception p4ex = new P4LostConnectionException(ErrorSeverity.E_FATAL, msg);
								throw;
							}
							lastError = ex;

							if ((ex.Message.Contains("socket: WSA")) ||
								P4ClientError.IsTCPError(ex.ErrorCode) || P4ClientError.IsSSLError(ex.ErrorCode))
							{
								try
								{
									pServer.Reconnect();
								}
								catch
								{
									if (Connection != null)
									{
										Connection.Disconnect();
									}
									string msg = string.Format("The connection to the Perforce server at {0} has been lost", pServer.Port);
									P4Exception p4ex = new P4LostConnectionException(ErrorSeverity.E_FATAL, msg);
									throw;
								}
							}
							else
							{
								throw;
							}
						}
						catch (Exception ex)
						{
							throw;
						}
					}
					if (success)
					{
						//errorOutput = pServer.GetErrorResults();
						//textOutput = pServer.GetTextResults();
						//taggedOutput = pServer.GetTaggedOutput();
						//binaryOutput = pServer.GetBinaryResults();
						// info output is gathered by OnInfoOut()
					}
					else
					{
						//errorOutput = pServer.GetErrorResults();
						// info output is gathered by OnInfoOut()
					}
				}
				catch (Exception ex)
				{
					LogFile.LogException("P4Command", ex);
					throw;
				}
				finally
				{
					// Cancel the redirected the output, this will reset the callbacks if this command does not have callbacks set
					pServer.InfoResultsReceived -= onInfoResultsDelegate;
					pServer.PromptHandler = null;
					pServer.ResolveHandler = null;
					pServer.ResolveAHandler = null;
				}

				return success;
			}
		}

		/// <summary>
		/// Dispose of any resources 
		/// </summary>
		public virtual void Dispose()
		{
			if ((pServer != null) && (pServer.KeepAlive != null))
			{
				pServer.KeepAlive.CommandCompleted(CommandId);
			}
			pServer.ReleaseConnection(CommandId);
		}
	}
}
