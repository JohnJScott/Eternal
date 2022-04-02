// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Design;
using System.ComponentModel;
using System.Text;

namespace Transfluent
{
	public class UserConfiguration
	{
		public UserConfiguration()
		{
			DefaultLanguage = "en";
			LanguageAliases = new Dictionary<string, string>
			{
				{"de", "de-de"},
				{"en", "en-us"},
				{"es", "es-es"},
				{"fr", "fr-fr"},
				{"hi", "hi-in"},
				{"ja", "ja-jp"},
				{"ko", "ko-kr"},
				{"ru", "ru-ru"},
			};

			SuppressLogging = false;
			VerboseLogging = false;
			GameName = "UnrealTournament";
		}

		/// <summary></summary>
		[Category( "Language settings" )]
		[Description( "The default language used to populate untranslated text. Defaults to 'en'." )]
		public string DefaultLanguage { get; set; }

		/// <summary></summary>
		[Category( "Language settings" )]
		[Description( "If an Unreal Engine 4 language is not supported by Transfluent, an attempt will be made to alias the unknown Unreal Engine name to a known Transfluent name using these settings." )]
		public Dictionary<string, string> LanguageAliases { get; set; }

		/// <summary></summary>
		[Category( "Usability settings" )]
		[Description( "Whether to suppress logging when running the application. Defaults to false." )]
		public bool SuppressLogging { get; set; }

		/// <summary></summary>
		[Category( "Usability settings" )]
		[Description( "Whether to enable verbose logging when running the application. Defaults to false. It is only recommended to enable this feature if issues are occurring." )]
		public bool VerboseLogging { get; set; }

		/// <summary></summary>
		[Category( "Unreal Engine 4 configuration" )]
		[Description( "The game used to spawn the commandlet." )]
		public string GameName { get; set; }
	}
}