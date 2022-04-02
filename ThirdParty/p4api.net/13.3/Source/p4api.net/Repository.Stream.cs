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
 * Name		: Repository.Stream.cs
 *
 * Author	: wjb
 *
 * Description	: Stream operations for the Reposity.
 *
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	public partial class Repository
	{
		/// <summary>
		/// Create a new stream in the repository.
		/// </summary>
		/// <param name="stream">Stream specification for the new stream</param>
		/// <param name="options">The '-i' flag is required when creating a new stream</param>
		/// <returns>The Stream object if new stream was created, null if creation failed</returns>
		/// <remarks> The '-i' flag is added if not specified by the caller
		/// <br/>
		/// <br/><b>p4 help stream</b>
		/// <br/> 
		/// <br/>     stream -- Create, delete, or modify a stream specification
		/// <br/> 
		/// <br/>     p4 stream [-P parent] -t type name
		/// <br/>     p4 stream [-f] [-d] [-o [-v]] [-P parent] -t type name
		/// <br/>     p4 stream -i [-f] 
		/// <br/> 
		/// <br/> 	A stream specification ('spec') names a path in a stream depot to be
		/// <br/> 	treated as a stream.  (See 'p4 help streamintro'.)  The spec also
		/// <br/> 	defines the stream's lineage, its view, and its expected flow of
		/// <br/> 	change.
		/// <br/> 
		/// <br/> 	The 'p4 stream' command puts the stream spec into a temporary file and
		/// <br/> 	invokes the editor configured by the environment variable $P4EDITOR.
		/// <br/> 	When creating a stream, the type of the stream must be specified with
		/// <br/> 	the '-t' flag.  Saving the file creates or modifies the stream spec.
		/// <br/> 
		/// <br/> 	Creating a stream spec does not branch a new stream.  To branch a
		/// <br/> 	stream, use 'p4 copy -r -S stream', where 'stream' is the name of a
		/// <br/> 	stream spec.
		/// <br/> 
		/// <br/> 	The stream spec contains the following fields:
		/// <br/> 
		/// <br/> 	Stream:   The stream's path in a stream depot, of the form
		/// <br/> 	          //depotname/streamname. This is both the name of the stream
		/// <br/> 	          spec and the permanent, unique identifier of the stream.
		/// <br/> 
		/// <br/> 	Update:   The date this stream spec was last changed.
		/// <br/> 
		/// <br/> 	Access:   The date of the last command used with this spec.
		/// <br/> 
		/// <br/> 	Owner:    The stream's owner. Can be changed.
		/// <br/> 
		/// <br/> 	Name:     An alternate name of the stream, for use in display outputs.
		/// <br/> 	          Defaults to the 'streamname' portion of the stream path.
		/// <br/> 	          Can be changed. 
		/// <br/> 
		/// <br/> 	Parent:   The parent of this stream. Can be 'none' if the stream type
		/// <br/> 	          is 'mainline',  otherwise must be set to an existing stream
		/// <br/> 	          identfier, of the form //depotname/streamname.
		/// <br/> 	          Can be changed.
		/// <br/> 
		/// <br/> 	Type:     'mainline', 'development', or 'release'.  Default is 
		/// <br/> 	          'development'.  Defines the expected flow of change between
		/// <br/> 	          a stream and its parent: A development stream expects to
		/// <br/> 	          merge from the parent and copy to the parent; a release
		/// <br/> 	          stream expects to copy from the parent and merge to the
		/// <br/> 	          parent.  Can be changed
		/// <br/> 
		/// <br/> 	Description: An optional description of the stream.
		/// <br/> 
		/// <br/> 	Options:  Flags to configure stream behavior. Defaults are marked *:
		/// <br/> 
		/// <br/> 	          unlocked *      Indicates whether the stream spec is locked
		/// <br/> 	          locked          against modifications. If locked, the spec
		/// <br/> 	                          may not be deleted, and only its owner may
		/// <br/> 	                          modify it.
		/// <br/> 
		/// <br/> 	          allsubmit *     Indicates whether all users or only the
		/// <br/> 	          ownersubmit     of the stream may submit changes to the
		/// <br/> 	                          stream path.
		/// <br/> 
		/// <br/> 	          toparent *      Indicates whether integration from the
		/// <br/> 	          notoparent      stream to its parent is expected to occur.
		/// <br/> 
		/// <br/> 	          fromparent *    Indicates whether integration to the stream
		/// <br/> 	          nofromparent    from its parent is expected to occur.
		/// <br/> 
		/// <br/> 	Paths:    One or more lines that define file paths in the stream view.
		/// <br/> 	          Each line is of the form:
		/// <br/> 
		/// <br/> 	              &lt;path_type&gt; &lt;view_path&gt; [&lt;depot_path&gt;]
		/// <br/> 
		/// <br/> 	          where &lt;path_type&gt; is a single keyword, &lt;view_path&gt; is a file
		/// <br/> 	          path with no leading slashes, and the optional &lt;depot_path&gt;
		/// <br/> 	          is a file path beginning with '//'.  Both &lt;view_path&gt; and
		/// <br/> 	          &lt;depot_path&gt; may contain trailing wildcards, but no leading
		/// <br/> 	          or embedded wildcards.  Lines in the Paths field may appear
		/// <br/> 	          in any order.  A duplicated &lt;view_path&gt; overrides its
		/// <br/> 	          preceding entry.
		/// <br/> 
		/// <br/> 	          For example:
		/// <br/> 
		/// <br/> 	              share   src/...
		/// <br/> 	              import  lib/abc/...  //over/there/abc/...
		/// <br/> 	              isolate bin/*
		/// <br/> 
		/// <br/> 	          Default is:
		/// <br/> 
		/// <br/> 	              share   ...
		/// <br/> 
		/// <br/> 	          The &lt;path_type&gt; keyword must be one of:
		/// <br/> 
		/// <br/> 	          share:  &lt;view_path&gt; will be included in client views and
		/// <br/> 	                  in branch views. Files in this path are accessible
		/// <br/> 	                  to workspaces, can be submitted to the stream, and
		/// <br/> 	                  can be integrated with the parent stream.
		/// <br/> 
		/// <br/> 	          isolate: &lt;view_path&gt; will be included in client views but
		/// <br/> 	                   not in branch views. Files in this path are
		/// <br/> 	                   accessible to workspaces, can be submitted to the
		/// <br/> 	                   stream, but are not integratable with the parent
		/// <br/> 	                   stream. 
		/// <br/> 
		/// <br/> 	          import: &lt;view_path&gt; will be included in client views but
		/// <br/> 	                  not in branch views. Files in this path are mapped
		/// <br/> 	                  as in the parent stream's view (the default) or to
		/// <br/> 	                  &lt;depot_path&gt; (optional); they are accessible to
		/// <br/> 	                  workspaces, but can not be submitted or integrated
		/// <br/> 	                  to the stream.
		/// <br/> 
		/// <br/> 	          exclude: &lt;view_path&gt; will be excluded from client views
		/// <br/> 	                   and branch views. Files in this path are not
		/// <br/> 	                   accessible to workspaces, and can't be submitted
		/// <br/> 	                   or integrated to the stream.
		/// <br/> 
		/// <br/> 	          Paths are inherited by child stream views. A child stream's
		/// <br/> 	          paths can downgrade the inherited view, but not upgrade it.
		/// <br/> 	          (For instance, a child stream can downgrade a shared path to
		/// <br/> 	          an isolated path, but it can't upgrade an isolated path to a
		/// <br/> 	          shared path.) Note that &lt;depot_path&gt; is relevant only when
		/// <br/> 	          &lt;path_type&gt; is 'import'.
		/// <br/> 
		/// <br/> 	Remapped: Optional; one or more lines that define how stream view paths
		/// <br/> 	          are to be remapped in client views. Each line is of the form:
		/// <br/> 
		/// <br/> 	              &lt;view_path_1&gt; &lt;view_path_2&gt;
		/// <br/> 
		/// <br/> 	          where &lt;view_path_1&gt; and &lt;view_path_2&gt; are Perforce view paths
		/// <br/> 	          with no leading slashes and no leading or embedded wildcards.
		/// <br/> 	          For example:
		/// <br/> 
		/// <br/> 	              ...    x/...
		/// <br/> 	              y/*    y/z/*
		/// <br/> 
		/// <br/> 	          Line ordering in the Remapped field is significant; if more
		/// <br/> 	          than one line remaps the same files, the later line has
		/// <br/> 	          precedence.  Remapping is inherited by child stream client
		/// <br/> 	          views.
		/// <br/> 
		/// <br/> 	Ignored: Optional; a list of file or directory names to be ignored in
		/// <br/> 	         client views. For example:
		/// <br/> 
		/// <br/> 	             /tmp      # ignores files named 'tmp'
		/// <br/> 	             /tmp/...  # ignores dirs named 'tmp'
		/// <br/> 	             .tmp      # ignores file names ending in '.tmp'
		/// <br/> 
		/// <br/> 	         Lines in the Ignored field may appear in any order.  Ignored
		/// <br/> 	         names are inherited by child stream client views.
		/// <br/> 
		/// <br/> 	The -d flag causes the stream spec to be deleted.  A stream spec may
		/// <br/> 	not be deleted if it is referenced by child streams or stream clients.
		/// <br/> 	Deleting a stream spec does not remove stream files, but it does mean
		/// <br/> 	changes can no longer be submitted to the stream's path.
		/// <br/> 
		/// <br/> 	The -o flag causes the stream spec to be written to the standard
		/// <br/> 	output. The user's editor is not invoked. -v may be used with -o to
		/// <br/> 	expose the automatically generated client view for this stream.
		/// <br/> 	('p4 help branch' describes how to expose the branch view.)
		/// <br/> 
		/// <br/> 	The -P flag can be used to insert a value into the Parent field of a
		/// <br/> 	new stream spec. It has no effect on an existing spec.
		/// <br/> 
		/// <br/> 	The -t flag is used to insert a value into the type field of a
		/// <br/> 	new stream spec and to adjust the default fromparent option
		/// <br/> 	for a new 'release' -type stream. The flag has no effect on an
		/// <br/> 	existing spec.
		/// <br/> 
		/// <br/> 	The -i flag causes a stream spec to be read from the standard input.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -f flag allows a user other than the owner to modify or delete a
		/// <br/> 	locked stream. It requires 'admin' access granted by 'p4 protect'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Stream CreateStream(Stream stream, Options options)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			P4Command cmd = new P4Command(this, "stream", true);

			cmd.DataSet = stream.ToString();

			if (options == null)
			{
				options = new Options((StreamCmdFlags.Input), null, null);
			}
			if (options.ContainsKey("-i") == false)
			{
				options["-i"] = null;
			}
			
			P4CommandResult results = cmd.Run(options);
			if (results.Success)
			{
				return stream;
			}
			else
			{
				P4Exception.Throw(results.ErrorList);
			}
			return null;
		}
		/// <summary>
		/// Create a new stream in the repository.
		/// </summary>
		/// <param name="stream">Stream specification for the new stream</param>
		/// <returns>The Stream object if new stream was created, null if creation failed</returns>
		public Stream CreateStream(Stream stream)
		{
			return CreateStream(stream, null);
		}
		/// <summary>
		/// Update the record for a stream in the repository
		/// </summary>
		/// <param name="stream">Stream specification for the stream being updated</param>
		/// <returns>The Stream object if new stream was saved, null if creation failed</returns>
		public Stream UpdateStream(Stream stream)
		{
			return CreateStream(stream, null);
		}
		/// <summary>
		/// Get the record for an existing stream from the repository.
		/// </summary>
		/// <param name="stream">Stream name</param>
		/// <param name="options">There are no valid flags to use when fetching an existing stream</param>
		/// <returns>The Stream object if new stream was found, null if creation failed</returns>
		public Stream GetStream(string stream, string parent, Options options)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");

			}
			P4Command cmd = new P4Command(this, "stream", true, stream);

			if (options == null)
			{
				options = new Options((StreamCmdFlags.Output), parent, null);
			}
			if (options.ContainsKey("-o") == false)
			{
				options["-o"] = null;
			}

			P4CommandResult results = cmd.Run(options);
			if (results.Success)
			{
				if ((results.TaggedOutput == null) || (results.TaggedOutput.Count <= 0))
				{
					return null;
				}
				Stream value = new Stream();
				value.FromStreamCmdTaggedOutput((results.TaggedOutput[0]));

				return value;
			}
			else
			{
				P4Exception.Throw(results.ErrorList);
			}
			return null;
		}
		public Stream GetStream(string stream)
		{
			return GetStream(stream, null, null);
		}
		/// <summary>
		/// Get a list of streams from the repository
		/// </summary>
		/// <param name="options">options for the streams command<see cref="StreamsOptions"/></param>
		/// <returns>A list containing the matching streams</returns>
		/// <remarks>
		/// <br/><b>p4 help streams</b>
		/// <br/> 
		/// <br/>     streams -- Display list of streams
		/// <br/> 
		/// <br/>     p4 streams [-F filter -T fields -m max] [streamPath ...]
		/// <br/> 
		/// <br/> 	Reports the list of all streams currently known to the system.  If
		/// <br/> 	a 'streamPath' argument is specified, the list of streams is limited
		/// <br/> 	to those matching the supplied path.
		/// <br/> 
		/// <br/> 	For each stream, a single line of output lists the stream depot path,
		/// <br/> 	the type, the parent stream depot path, and the stream name.
		/// <br/> 
		/// <br/> 	The -F filter flag limits the output to files satisfying the expression
		/// <br/> 	given as 'filter'.  This filter expression is similar to the one used
		/// <br/> 	by 'jobs -e jobview',  except that fields must match those above and
		/// <br/> 	are case sensitive.
		/// <br/> 
		/// <br/> 	        e.g. -F "Parent=//Ace/MAIN & Type=development"
		/// <br/> 
		/// <br/> 	Note: the filtering takes place post-compute phase; there are no
		/// <br/> 	indexes to optimize performance.
		/// <br/> 
		/// <br/> 	The -T fields flag (used with tagged output) limits the fields output
		/// <br/> 	to those specified by a list given as 'fields'.  These field names can
		/// <br/> 	be separated by a space or a comma.
		/// <br/> 
		/// <br/> 	        e.g. -T "Stream, Owner"
		/// <br/> 
		/// <br/> 	The -m max flag limits output to the first 'max' number of streams.
		/// <br/> 
		/// <br/> 
		/// </remarks>

		public IList<Stream> GetStreams(Options options, params FileSpec[] files)
		{
			P4Command cmd = null;
			if ((files != null) && (files.Length > 0))
			{
				cmd = new P4Command(this, "streams", true, FileSpec.ToStrings(files));
			}
			else
			{
				cmd = new P4Command(this, "streams", true);
			}

			P4CommandResult results = cmd.Run(options);
			if (results.Success)
			{
				if ((results.TaggedOutput == null) || (results.TaggedOutput.Count <= 0))
				{
					return null;
				}
				List<Stream> value = new List<Stream>();
				foreach (TaggedObject obj in results.TaggedOutput)
				{
					Stream stream = new Stream();
					stream.FromStreamsCmdTaggedOutput(obj);
					value.Add(stream);
				}
				return value;
			}
			else
			{
				P4Exception.Throw(results.ErrorList);
			}
			return null;
		}
		/// <summary>
		/// Delete a stream from the repository
		/// </summary>
		/// <param name="stream">The stream to be deleted</param>
		/// <param name="options">Only the '-f' flag is valid when deleting an existing stream</param>
		public void DeleteStream(Stream stream, Options options)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");

			}
			P4Command cmd = new P4Command(this, "stream", true, stream.Id);

			if (options == null)
			{
				options = new Options((StreamCmdFlags.Delete | StreamCmdFlags.Force), null, null);
			}
			if (options.ContainsKey("-d") == false)
			{
				options["-d"] = null;
			}

            P4CommandResult results = cmd.Run(options);
			if (results.Success == false)
			{
				P4Exception.Throw(results.ErrorList);
			}
		}
        		/// <summary>
		/// Delete a stream from the repository
		/// </summary>
		/// <param name="stream">The stream to be deleted</param>
		/// <param name="options">Only the '-f' flag is valid when deleting an existing stream</param>
        public StreamMetaData GetStreamMetaData(Stream stream, Options options)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");

            }
            P4Command cmd = new P4Command(this, "istat", true, stream.Id);

            P4CommandResult results = cmd.Run(options);
            if (results.Success)
            {
                if ((results.TaggedOutput == null) || (results.TaggedOutput.Count <= 0))
                {
                    return null;
                }
                StreamMetaData value = new StreamMetaData();
                value.FromIstatCmdTaggedData((results.TaggedOutput[0]));

                return value;
            }
            else
            {
                P4Exception.Throw(results.ErrorList);
            }
            return null;
        }
        

	}
}
