// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System.Collections.Generic;

namespace Transfluent
{
	/// <summary>
	/// 
	/// </summary>
	public enum Operation
	{
		None,
		UploadUntranslated,
		DownloadTranslated,
		RefreshArchives,
	}

	/// <summary>
	/// 
	/// </summary>
	public class Parameters
	{
		public Operation Command = Operation.None;
		public string LanguageFolder = null;
		public string ManifestFileName = null;
		public string Password = null;
		public string UserName = null;
	}

	/// <summary>
	/// </summary>
	public class AuthenticationInfo
	{
		public string token { get; set; }
		public string expires { get; set; }
	}

	public class AuthenticationResponse
	{
		public string status { get; set; }
		public AuthenticationInfo response { get; set; }
	}

	/// <summary>
	/// </summary>
	public class LanguagesInfo
	{
		public string name { get; set; }
		public string code { get; set; }
		public int id { get; set; }
	}

	public class LanguagesResponse
	{
		public LanguagesResponse()
		{
			response = new Dictionary<string, LanguagesInfo>();
		}

		public string status { get; set; }
		public Dictionary<string, LanguagesInfo> response { get; set; }
	}

	/// <summary>
	/// </summary>
	public class TextPostResponse
	{
		public string status { get; set; }
		public bool response { get; set; }
	}

	/// <summary>
	/// </summary>
	public class TextGetResponse
	{
		public string status { get; set; }
		public string response { get; set; }
	}

	/// <summary>
	/// </summary>
	public class TextStatusInfo
	{
		public bool is_translated { get; set; }
	}

	public class TextStatusGetResponse
	{
		public string status { get; set; }
		public TextStatusInfo response { get; set; }
	}

	/// <summary>
	/// </summary>
	public class LocalizationItem
	{
		public string Text { get; set; }
	}

	public class LocalizationArchiveEntry
	{
		public LocalizationItem Source { get; set; }
		public LocalizationItem Translation { get; set; }
	}

	public class LocalizationArchive
	{
		public string Namespace { get; set; }
		public List<LocalizationArchiveEntry> Children { get; set; }
		public List<LocalizationArchive> Subnamespaces { get; set; }
	}

	public class LocalizationArchiveInfo
	{
		public string LanguageCode;

		public int TotalLineCount;
		public int TotalWordCount;
		public int UntranslatedLineCount;
		public int UntranslatedWordCount;
		public bool bArchiveDirty;

		public LocalizationArchiveInfo( string InLanguageCode )
		{
			LanguageCode = InLanguageCode;

			ClearCounters();
		}

		public void ClearCounters()
		{
			bArchiveDirty = false;
			UntranslatedLineCount = 0;
			TotalLineCount = 0;
			UntranslatedWordCount = 0;
			TotalWordCount = 0;
		}
	}

	public class LocalizationKeyItem
	{
		public string Key { get; set; }
		public string Path { get; set; }
	}

	public class LocalizationManifestEntry
	{
		public LocalizationItem Source { get; set; }
		public LocalizationItem Translation { get; set; }
		public List<LocalizationKeyItem> Keys { get; set; }
	}

	public class LocalizationManifest
	{
		public string Namespace { get; set; }
		public List<LocalizationManifestEntry> Children { get; set; }
		public List<LocalizationManifest> Subnamespaces { get; set; }

		public bool FindKey( string SourceText, List<string> Namespaces, ref string TextId )
		{
			if( Subnamespaces != null && Namespaces.Count > 0 )
			{
				foreach( LocalizationManifest Manifest in Subnamespaces )
				{
					if( Namespaces[0] == Manifest.Namespace )
					{
						Namespaces.RemoveAt( 0 );
						return Manifest.FindKey( SourceText, Namespaces, ref TextId );
					}
				}
			}

			if( Children != null )
			{
				foreach( LocalizationManifestEntry Entry in Children )
				{
					if( Entry.Source.Text == SourceText )
					{
						TextId = Entry.Keys[0].Key;
						return true;
					}
				}

				return false;
			}

			return false;
		}
	}

	public class UpdatedItem
	{
		public string FullKey;
		public string LanguageFolder;
		public string Translation;

		public UpdatedItem( string InFullKey, string InLanguageFolder, string InTranslation )
		{
			FullKey = InFullKey;
			LanguageFolder = InLanguageFolder;
			Translation = InTranslation;
		}
	}
}