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
 * Name		: ClientMetadata.cs
 *
 * Author(s)	: wjb
 *
 * Description	: Class used to abstract client side application data.
 *
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// Metadata from the client associated with a connection.
	/// </summary>
	public class ClientMetadata
	{
		public ClientMetadata()
		{
		}
		public ClientMetadata
			(
			string name,
			string hostname,
			string address,
			string currentdirectory,
			string root
			)
		{
			Name = name;
			HostName = hostname;
			Address = address;
			CurrentDirectory = currentdirectory;
			Root = root;
		}

		#region properties

		public string Name { get; set; }
		public string HostName { get; set; }
		public string Address { get; set; }
		public string CurrentDirectory { get; set; }
		public string Root { get; set; }

		#endregion

		#region fromTaggedOutput
		/// <summary>
		/// Read the fields from the tagged output of an info command
		/// </summary>
		/// <param name="objectInfo">Tagged output from the 'info' command</param>
		public void FromGetClientMetadataCmdTaggedOutput(TaggedObject objectInfo)
		{

			if (objectInfo.ContainsKey("clientApplication"))
				Name = objectInfo["clientApplication"];

			if (objectInfo.ContainsKey("clientHost"))
				HostName = objectInfo["clientHost"];

			if (objectInfo.ContainsKey("clientAddress"))
				Address = objectInfo["clientAddress"];

			if (objectInfo.ContainsKey("clientCwd"))
				CurrentDirectory = objectInfo["clientCwd"];

			if (objectInfo.ContainsKey("clientRoot"))
				Root = objectInfo["clientRoot"];

		}
		#endregion
	}
}
