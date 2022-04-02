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
 * Name		: Label.cs
 *
 * Author(s)	: wjb, dbb
 *
 * Description	: Class used to abstract a label in Perforce.
 *
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// A label specification in a Perforce repository. 
	/// </summary>
    public class Label
    {
        public Label()
        {
        }

        public Label
            (
            string id,
            string owner,
            DateTime update,
            DateTime access,
            string description,
            bool locked,
            string revision,
            ViewMap viewmap,
            FormSpec spec,
            string options
            )
        {
            Id = id;
            Owner = owner;
            Update = update;
            Access = access;
            Description = description;
            Locked = locked;
            Revision = revision;
            ViewMap = viewmap;
            Spec = spec;
            Options = options;
        }

        private bool _initialized;
        private FormBase _baseForm;

        #region properties

        public string Id { get; set; }
        public string Owner { get; set; }
        public DateTime Update { get; set; }
        public DateTime Access { get; set; }
        public string Description { get; set; }
		public bool Locked { get; set; }
		private bool _autoreload { get; set; }
		public bool Autoreload 
		{
			get { return _autoreload; }
			set
			{
				if (value)
				{
					IncludeAutoreloadOption = true;
				}
				_autoreload = value;
			}
		}
		public bool IncludeAutoreloadOption { get; set; }
		public string Revision { get; set; }
        public ViewMap ViewMap { get; set; }
        public FormSpec Spec { get; set; }

	    [Obsolete("Use Locked Property")]
	    public string Options
	    {
	        get
	        {
				string value = Locked ? "locked" : "unlocked";
				if (IncludeAutoreloadOption)
				{
					if (Autoreload)
					{
						value += " autoreload";
					}
					else
					{
						value += " noautoreload";
					}
				}
				return value;
	        }
            set
            {
                Locked = (value.Contains("unlocked")) == false;
				IncludeAutoreloadOption = value.Contains("autoreload");
				Autoreload = IncludeAutoreloadOption && (value.Contains("noautoreload") == false);
            }
	    }

        #endregion
        #region fromTaggedOutput
        /// <summary>
        /// Read the fields from the tagged output of a label command
        /// </summary>
        /// <param name="objectInfo">Tagged output from the 'label' command</param>
        public void FromLabelCmdTaggedOutput(TaggedObject objectInfo)
        {
            _initialized = true;
            _baseForm = new FormBase();

            _baseForm.SetValues(objectInfo);

            if (objectInfo.ContainsKey("Label"))
                Id = objectInfo["Label"];

            if (objectInfo.ContainsKey("Owner"))
                Owner = objectInfo["Owner"];

            if (objectInfo.ContainsKey("Update"))
            {
                DateTime v = DateTime.MinValue;
                DateTime.TryParse(objectInfo["Update"], out v);
                Update = v;
            }

            if (objectInfo.ContainsKey("Access"))
            {
                DateTime v = DateTime.MinValue;
                DateTime.TryParse(objectInfo["Access"], out v);
                Access = v;
            }

            if (objectInfo.ContainsKey("Description"))
                Description = objectInfo["Description"];

            if (objectInfo.ContainsKey("Revision"))
                Revision = objectInfo["Revision"];

            if (objectInfo.ContainsKey("Options"))
            {
				Options = objectInfo["Options"] as string;
			}
            else
                Locked = false;

            int idx = 0;
            string key = String.Format("View{0}", idx);
            if (objectInfo.ContainsKey(key))
            {
                ViewMap = new ViewMap();
                while (objectInfo.ContainsKey(key))
                {
                    ViewMap.Add(objectInfo[key]);
                    idx++;
                    key = String.Format("View{0}", idx);
                }
            }
            else
            {
                ViewMap = null;
            }
        }
        #endregion

        #region client spec support
        /// <summary>
        /// Parse the fields from a label specification 
        /// </summary>
        /// <param name="spec">Text of the label specification in server format</param>
        /// <returns></returns>
        public bool Parse(String spec)
        {
            _baseForm = new FormBase();

            _baseForm.Parse(spec); // parse the values into the underlying dictionary

            if (_baseForm.ContainsKey("Label"))
            {
                Id = _baseForm["Label"] as string;
            }

            if (_baseForm.ContainsKey("Owner"))
            {
                Owner = _baseForm["Owner"] as string;
            }

            if (_baseForm.ContainsKey("Update"))
            {
                DateTime v = DateTime.MinValue;
                DateTime.TryParse(_baseForm["Update"] as string, out v);
                Update = v;
            }

            if (_baseForm.ContainsKey("Access"))
            {
                DateTime v = DateTime.MinValue;
                DateTime.TryParse(_baseForm["Access"] as string, out v);
                Access = v;
            }

			if (_baseForm.ContainsKey("Description"))
			{
				object d = _baseForm["Description"];
				if (d is string)
				{
					Description = _baseForm["Description"] as string;
				}
				else if (d is string[])
				{
					string[] a = d as string[];
					Description = string.Empty;
					for (int idx = 0; idx < a.Length; idx++)
					{
						if (idx > 0)
						{
							Description += "\r\n";
						}
						Description += a[idx];
					}
				}
				else if (d is IList<string>)
				{
					IList<string> l = d as IList<string>;
					Description = string.Empty;
					for (int idx = 0; idx < l.Count; idx++)
					{
						if (idx > 0)
						{
							Description += "\r\n";
						}
						Description += l[idx];
					}
				}
			}

            if (_baseForm.ContainsKey("Options"))
            {
				Options = _baseForm["Options"] as string;
			}

            if (_baseForm.ContainsKey("Revision"))
            {
                Revision = _baseForm["Revision"] as string;
            }

            if ((_baseForm.ContainsKey("View")) && (_baseForm["View"] is IList<string>))
            {
                IList<string> lines = _baseForm["View"] as IList<string>;
                ViewMap = new ViewMap(lines.ToArray());
            }

            return true;
        }

        /// <summary>
        /// Format of a label specification used to save a label to the server
        /// </summary>
        private static String LabelFormat =
                                                    "Label:\t{0}\r\n" +
                                                    "\r\n" +
                                                    "Update:\t{1}\r\n" +
                                                    "\r\n" +
                                                    "Access:\t{2}\r\n" +
                                                    "\r\n" +
                                                    "Owner:\t{3}\r\n" +
                                                    "\r\n" +
                                                    "Description:\r\n\t{4}\r\n" +
                                                    "\r\n" +
                                                    "Options:\t{5}\r\n" +
                                                    "\r\n" +
                                                    "{6}" +
                                                    "View:\r\n\t{7}\r\n";


        /// <summary>
        /// Convert to specification in server format
        /// </summary>
        /// <returns></returns>
        override public String ToString()
        {
            String viewStr = String.Empty;
            if (ViewMap != null)
                viewStr = ViewMap.ToString().Replace("\r\n", "\r\n\t").Trim();
            String OptionsStr = string.Empty;
			if (Locked)
			{
				OptionsStr = "locked";
			}
			else
			{
				OptionsStr = "unlocked";
			}

			if (IncludeAutoreloadOption)
			{
				if (Autoreload)
				{
					OptionsStr += " autoreload";
				}
				else
				{
					OptionsStr += " noautoreload";
				}
			}
            String revStr = String.Empty;
            if (Revision != null)
            {
                revStr = string.Format("Revision:\t{0}\r\n\r\n", Revision);
            }
            String value = String.Format(LabelFormat, Id,
                FormBase.FormatDateTime(Update), FormBase.FormatDateTime(Access),
                Owner, Description, OptionsStr, revStr, viewStr);
            return value;
        }
        #endregion

        /// <summary>
        /// Read the fields from the tagged output of a labels command
        /// </summary>
        /// <param name="objectInfo">Tagged output from the 'labels' command</param>
        public void FromLabelsCmdTaggedOutput(TaggedObject objectInfo)
        {
            _initialized = true;
            _baseForm = new FormBase();

            _baseForm.SetValues(objectInfo);

            if (objectInfo.ContainsKey("label"))
                Id = objectInfo["label"];

            if (objectInfo.ContainsKey("Owner"))
                Owner = objectInfo["Owner"];

            if (objectInfo.ContainsKey("Access"))
            {
                Access = FormBase.ConvertUnixTime(objectInfo["Access"]);
            }

            if (objectInfo.ContainsKey("Update"))
            {
                Update = FormBase.ConvertUnixTime(objectInfo["Update"]);
            }

            if (objectInfo.ContainsKey("Options"))
            {
				Options = objectInfo["Options"] as string;
			}
            else
                Locked = false;

            if (objectInfo.ContainsKey("Description"))
                Description = objectInfo["Description"];

            if (objectInfo.ContainsKey("Revision"))
                Revision = objectInfo["Revision"];

        }
    }
}
