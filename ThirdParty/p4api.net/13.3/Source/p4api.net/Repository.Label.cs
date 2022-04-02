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
 * Name		: Repository.Label.cs
 *
 * Author	: wjb
 *
 * Description	: Label operations for the Repository.
 *
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Perforce.P4
{
	public partial class Repository
	{
		/// <summary>
		/// Create a new label in the repository.
		/// </summary>
		/// <param name="label">Label specification for the new label</param>
		/// <param name="options">The '-i' flag is required when creating a new label </param>
		/// <returns>The Label object if new label was created, null if creation failed</returns>
		/// <remarks> The '-i' flag is added if not specified by the caller
		/// <br/>
		/// <br/><b>p4 help label</b>
		/// <br/> 
		/// <br/>     label -- Create or edit a label specification
		/// <br/> 
		/// <br/>     p4 label [-f -t template] name
		/// <br/>     p4 label -d [-f] name
		/// <br/>     p4 label -o [-t template] name
		/// <br/>     p4 label -i [-f]
		/// <br/> 
		/// <br/> 	Create  or edit a label. The name parameter is required. The
		/// <br/> 	specification form is put into a temporary file and the editor
		/// <br/> 	(configured by the environment variable $P4EDITOR) is invoked.
		/// <br/> 
		/// <br/> 	The label specification form contains the following fields:
		/// <br/> 
		/// <br/> 	Label:       The label name (read only.)
		/// <br/> 
		/// <br/> 	Owner:       The user who created this label.  Can be changed.
		/// <br/> 
		/// <br/> 	Update:      The date that this specification was last modified.
		/// <br/> 
		/// <br/> 	Access:      The date of the last 'labelsync' or use of '@label'
		/// <br/> 		     referencing this label.
		/// <br/> 
		/// <br/> 	Description: A short description of the label (optional).
		/// <br/> 
		/// <br/> 	Options:     Flags to change the label behavior.
		/// <br/> 
		/// <br/> 	             locked	Prevents users other than the label owner
		/// <br/> 				from changing the specification. Prevents
		/// <br/> 				the label from being deleted. Prohibits
		/// <br/> 				'p4 labelsync'.
		/// <br/> 
		/// <br/> 	Revision:    An optional revision specification for an automatic
		/// <br/> 		     label.  Enclose in double quotes if it contains the
		/// <br/> 		     # (form comment) character.
		/// <br/> 
		/// <br/> 	View:        A mapping that selects files from the depot. The
		/// <br/> 		     default view selects all depot files. Only the left
		/// <br/> 		     side of the mapping is used for labels.
		/// <br/> 
		/// <br/> 	A label is a named collection of revisions.  A label is either
		/// <br/> 	automatic or static.  An automatic label refers to the revisions
		/// <br/> 	given in the View: and Revision: fields.  A static label refers to
		/// <br/> 	the revisions that are associated with the label using the 'p4 tag'
		/// <br/> 	or 'p4 labelsync' commands.  A static label cannot have a Revison:
		/// <br/> 	field. See 'p4 help revisions' for information on using labels as
		/// <br/> 	revision specifiers.  
		/// <br/> 
		/// <br/> 	Flag -d deletes the specified label. You cannot delete a locked label.
		/// <br/> 	The -f flag forces the delete.
		/// <br/> 
		/// <br/> 	The -o flag writes the label specification to standard output.  The
		/// <br/> 	user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads a label specification from standard input.  The
		/// <br/> 	user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -t flag copies the view and options from the template label to
		/// <br/> 	the new label.
		/// <br/> 
		/// <br/> 	The -f flag forces the deletion of a label. By default, locked labels
		/// <br/> 	can only be deleted by their owner.  The -f flag also permits the
		/// <br/> 	Last Modified date to be set.  The -f flag requires 'admin' access,
		/// <br/> 	which is granted by 'p4 protect'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Label CreateLabel(Label label, Options options)
		{
			if (label == null)
			{
				throw new ArgumentNullException("label");

			}
			P4Command cmd = new P4Command(this, "label", true);

			cmd.DataSet = label.ToString();

			if (options == null)
			{
				options = new Options((LabelCmdFlags.Input), null);
			}
			if (options.ContainsKey("-i") == false)
			{
				options["-i"] = null;
			}

			P4CommandResult results = cmd.Run(options);
			if (results.Success)
			{
				return label;
			}
			else
			{
				P4Exception.Throw(results.ErrorList);
			}
			return null;
		}
		/// <summary>
		/// Create a new label in the repository.
		/// </summary>
		/// <param name="label">Label specification for the new label</param>
		/// <returns>The Label object if new label was created, null if creation failed</returns>
		public Label CreateLabel(Label label)
		{
			return CreateLabel(label, null);
		}
		/// <summary>
		/// Update the record for a label in the repository
		/// </summary>
		/// <param name="label">Label specification for the label being updated</param>
		/// <returns>The Label object if new depot was saved, null if creation failed</returns>
		public Label UpdateLabel(Label label)
		{
			return CreateLabel(label, null);
		}
		/// <summary>
		/// Get the record for an existing label from the repository.
		/// </summary>
		/// <param name="label">Label name</param>
        /// <param name="options">Flags used when fetching an existing label</param>
		/// <returns>The Label object if label was found, null if creation failed</returns>
		public Label GetLabel(string label, string template, Options options)
		{
			if (label == null)
			{
				throw new ArgumentNullException("label");

			}
			P4Command cmd = new P4Command(this, "label", true, label);

			if (options == null)
			{
				options = new Options((LabelCmdFlags.Output), template);
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
				Label value = new Label();
				value.FromLabelCmdTaggedOutput((results.TaggedOutput[0]));

				return value;
			}
			else
			{
				P4Exception.Throw(results.ErrorList);
			}
			return null;
		}
		public Label GetLabel(string label)
		{
			return GetLabel(label, null, null);
		}

		/// <summary>
		/// Get a list of labels from the repository
		/// </summary>
		/// <returns>A list containing the matching labels</returns>
		/// <remarks>
		/// <br/><b>p4 help labels</b>
		/// <br/> 
		/// <br/>     labels -- Display list of defined labels
		/// <br/> 
		/// <br/>     p4 labels [-t] [-u user] [[-e|-E] nameFilter -m max] [file[revrange]]
		/// <br/> 
		/// <br/> 	Lists labels defined in the server.
		/// <br/> 
		/// <br/> 	If files are specified, 'p4 labels' lists the labels that contain
		/// <br/> 	those files.  If you include a file specification, automatic lablels
		/// <br/> 	are omitted from the list.  If the file specification includes a
		/// <br/> 	revision range, 'p4 labels' lists labels that contain the specified
		/// <br/> 	revisions.  See 'p4 help revisions for details about specifying
		/// <br/> 	revisions.
		/// <br/> 
		/// <br/> 	The -t flag displays the time as well as the date.
		/// <br/> 
		/// <br/> 	The -u user flag lists labels owned by the specified user.
		/// <br/> 
		/// <br/> 	The -e nameFilter flag lists labels with names that match the
		/// <br/> 	the nameFilter pattern, for example:  -e 'svr-dev-rel*'. -E makes
		/// <br/> 	the matching case-insensitive.
		/// <br/> 
		/// <br/> 	The -m max flag limits output to the first 'max' number of labels.
		/// <br/> 
		/// <br/> 
		/// </remarks>

		public IList<Label> GetLabels(Options options, params FileSpec[] files)
		{
			P4Command cmd = null;
			if ((files != null) && (files.Length > 0))
			{
				cmd = new P4Command(this, "labels", true, FileSpec.ToStrings(files));
			}
			else
			{
				cmd = new P4Command(this, "labels", true);
			}

			P4CommandResult results = cmd.Run(options);
			if (results.Success)
			{
				if ((results.TaggedOutput == null) || (results.TaggedOutput.Count <= 0))
				{
					return null;
				}
				List<Label> value = new List<Label>();
				foreach (TaggedObject obj in results.TaggedOutput)
				{
					Label label = new Label();
					label.FromLabelsCmdTaggedOutput(obj);
					value.Add(label);
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
		/// Delete a label from the repository
		/// </summary>
		/// <param name="label">The label to be deleted</param>
		/// <param name="options">The 'f' and '-d' flags are valid when deleting an
		/// existing label</param>
		public void DeleteLabel(Label label, Options options)
		{
			if (label == null)
			{
				throw new ArgumentNullException("label");

			}
			P4Command cmd = new P4Command(this, "label", true, label.Id);

			if (options == null)
			{
				options = new Options(LabelCmdFlags.Delete, null);
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
	}
}
