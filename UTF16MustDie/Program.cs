// Copyright 2021 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Eternal.EternalUtilities;
using Perforce.P4;

namespace UTF16MustDie
{
    class Program
    {
        static void Main( string[] args )
        {
			ConsoleLogger.Title( "UTF16 Must DIE! A utility to fix up UTF-16 files in Perforce; both well formed and corrupted." );



			StringBuilder result = new StringBuilder();

	        string bad_file = args[0];
			Stream bad_stream = new FileStream( bad_file, FileMode.Open );
			BinaryReader reader = new BinaryReader( bad_stream );

			UInt16 BOM = reader.ReadUInt16();
			ConsoleLogger.Log( "File " + bad_file + " has BOM of 0x" + BOM.ToString( "X2" ) );

			do
			{
				UInt16 character = reader.ReadUInt16();
				if( character == 0x0a0d )
				{
					int test = reader.PeekChar();
					if( test == 0x0000 )
					{
						reader.ReadByte();

						character = 0x000a;
					}
				}

				result.Append( ( char )character );
			}
			while( reader.BaseStream.Position != reader.BaseStream.Length );

			reader.Close();

			File.WriteAllText( bad_file + ".good", result.ToString(), Encoding.UTF8 );
        }
    }
}
