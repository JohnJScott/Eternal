// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.SymbolStore;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Samples.Debugging.CorSymbolStore;

[assembly: CLSCompliant( false )]

namespace PdbInterrogator
{

	public partial class PdbViewer : Form
	{
		public PdbViewer()
		{
			InitializeComponent();

			ISymbolReader Reader = SymbolAccess.GetReaderForFile( SymbolFormat.PDB, "D:\\depot\\SoF1\\Win32\\Release\\sof.exe", null );
		}
	}
}
