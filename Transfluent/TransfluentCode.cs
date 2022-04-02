// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eternal.EternalUtilities;
using Newtonsoft.Json.Linq;

namespace Transfluent
{
	public partial class Transfluent
	{
		private string URLEncode( string Text )
		{
			Text = Text.Replace( "#", "%23" );
			return Text;
		}

		/// <summary>
		/// </summary>
		/// <param name="UserName"></param>
		/// <param name="Password"></param>
		/// <returns></returns>
		private AuthenticationResponse Login( string UserName, string Password )
		{
			Logger.Title( "Attempting to login with user '" + UserName + "'" );

			string URL = "https://transfluent.com/v2/authenticate/?email=" + UserName + "&password=" + Password;
			string Result = BasicWebRequest.RequestWebResponse( URL, "GET" );
			return JsonHelper.ReadJson<AuthenticationResponse>( Result );
		}

		/// <summary>
		/// </summary>
		/// <param name="JsonData"></param>
		/// <returns></returns>
		private LanguagesResponse ParseJson( string JsonData )
		{
			LanguagesResponse Languages = new LanguagesResponse();

			JObject Json = JObject.Parse( JsonData );
			List<JToken> Tokens = Json["response"].Children().ToList();
			foreach( JToken Token in Tokens )
			{
				List<JToken> SubTokens = Token.Children().Children().Children().ToList();

				LanguagesInfo Info = new LanguagesInfo();

				List<JToken> name = SubTokens[0].Children().ToList();
				Info.name = name[0].Value<string>();

				List<JToken> code = SubTokens[1].Children().ToList();
				Info.code = code[0].Value<string>().ToLower();

				List<JToken> id = SubTokens[2].Children().ToList();
				Info.id = id[0].Value<int>();

				Languages.response.Add( Info.code, Info );
			}

			return Languages;
		}

		/// <summary>
		/// </summary>
		private LanguagesResponse GetSupportedLanguages()
		{
			Logger.Title( "Getting supported languages" );
			string URL = "https://transfluent.com/v2/languages";
			string Result = BasicWebRequest.RequestWebResponse( URL, "POST" );
			LanguagesResponse LanguagesData = ParseJson( Result );

			Logger.Success( " ... found " + LanguagesData.response.Count + " Transfluent languages" );

			List<LanguagesInfo> Languages = LanguagesData.response.Values.ToList();
			Languages = Languages.OrderBy( x => x.code ).ToList();
			foreach( LanguagesInfo Info in Languages )
			{
				Logger.Verbose( " ... '" + Info.code + "' (" + Info.name + ") (id: " + Info.id + ")" );
			}

			return LanguagesData;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Manifest"></param>
		/// <param name="Text"></param>
		/// <returns></returns>
		private string GetFullKey( LocalizationManifest Manifest, string GroupId, string Text )
		{
			List<string> Namespaces = GroupId.Split( ".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries ).ToList();
			string FullKey = null;
			string TextId = "Unknown";
			if( Manifest.FindKey( Text, Namespaces, ref TextId ) )
			{
				FullKey = GroupId + "." + TextId;
			}

			return FullKey;
		}

		/// <summary>
		/// </summary>
		/// <param name="Archive"></param>
		private void ValidateDefaultLanguage( LocalizationArchive Archive )
		{
			if( Archive.Children != null )
			{
				foreach( LocalizationArchiveEntry Entry in Archive.Children )
				{
					if( Entry.Translation.Text != Entry.Source.Text )
					{
						Logger.Warning( "Translation does not match source for default language '" + Config.DefaultLanguage + "'; updating ..." );
						Entry.Translation.Text = Entry.Source.Text;
					}
				}
			}

			if( Archive.Subnamespaces != null )
			{
				foreach( LocalizationArchive ChildArchive in Archive.Subnamespaces )
				{
					ValidateDefaultLanguage( ChildArchive );
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="ManifestPath"></param>
		/// <returns></returns>
		private void DiscoverLanguages( string ManifestPath )
		{
			string LocalizationFolder = Path.GetDirectoryName( ManifestPath );
			string ArchiveName = Path.ChangeExtension( ManifestBaseFileName, "archive" );

			DirectoryInfo DirInfo = new DirectoryInfo( LocalizationFolder );
			foreach( DirectoryInfo SubDir in DirInfo.GetDirectories() )
			{
				bool bLanguageSupported = false;

				FileInfo Info = new FileInfo( Path.Combine( SubDir.FullName, ArchiveName ) );
				if( Info.Exists )
				{
					string LanguageFolder = SubDir.Name.ToLower();

					if( LanguagesData.response.Keys.Contains( LanguageFolder ) )
					{
						ArchivesInfo.Add( LanguageFolder, new LocalizationArchiveInfo( LanguageFolder ) );

						Logger.Success( " ... found " + GetFullLanguageDescription( LanguageFolder ) );
						bLanguageSupported = true;
					}
					else if( Config.LanguageAliases.Keys.Contains( LanguageFolder ) )
					{
						string FullLanguageName = Config.LanguageAliases[LanguageFolder];
						ArchivesInfo.Add( LanguageFolder, new LocalizationArchiveInfo( FullLanguageName ) );

						Logger.Success( " ... found '" + LanguageFolder + "' via an alias; using " + GetFullLanguageDescription( LanguageFolder ) );
						bLanguageSupported = true;
					}
				}

				if( !bLanguageSupported )
				{
					Logger.Warning( "Unsupported language: '" + SubDir.Name + "'" );
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Entry"></param>
		/// <param name="ArchiveInfo"></param>
		private void UpdateStats( string LanguageFolder, LocalizationArchiveInfo ArchiveInfo )
		{
			foreach( Dictionary<string, string> LocalizedStrings in LocalizationStore.Values )
			{
				if( LocalizedStrings.Keys.Contains( Config.DefaultLanguage ) )
				{
					string Source = LocalizedStrings[Config.DefaultLanguage];
					int SourceWordCount = Source.Split( " \t.,;:".ToCharArray(), StringSplitOptions.RemoveEmptyEntries ).Length;

					ArchiveInfo.TotalLineCount++;
					ArchiveInfo.TotalWordCount += SourceWordCount;

					if( LocalizedStrings.Keys.Contains( LanguageFolder ) )
					{
						if( LocalizedStrings[LanguageFolder].Length == 0 )
						{
							ArchiveInfo.UntranslatedLineCount++;
							ArchiveInfo.UntranslatedWordCount += SourceWordCount;
						}
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Manifest"></param>
		/// <param name="Archive"></param>
		/// <param name="LanguageId"></param>
		private void CacheArchive( LocalizationManifest Manifest, LocalizationArchive Archive, string Namespace, string LanguageFolder )
		{
			string CurrentNamespace = Archive.Namespace;
			if( Namespace.Length > 0 )
			{
				CurrentNamespace = Namespace + "." + Archive.Namespace;
			}

			if( Archive.Children != null )
			{
				foreach( LocalizationArchiveEntry Entry in Archive.Children )
				{
					// Create a cached copy of full key against all the translations of that key
					string FullKey = GetFullKey( Manifest, CurrentNamespace, Entry.Source.Text );
					if( FullKey != null )
					{
						if( !LocalizationStore.Keys.Contains( FullKey ) )
						{
							Dictionary<string, string> NewKey = new Dictionary<string, string>();
							LocalizationStore.Add( FullKey, NewKey );
						}

						LocalizationStore[FullKey].Add( LanguageFolder, Entry.Translation.Text );
					}
				}
			}

			if( Archive.Subnamespaces != null )
			{
				// Recurse into any child namespaces
				foreach( LocalizationArchive ChildArchive in Archive.Subnamespaces )
				{
					CacheArchive( Manifest, ChildArchive, CurrentNamespace, LanguageFolder );
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ManifestFileName"></param>
		/// <returns></returns>
		private LocalizationManifest LoadManifest( string ManifestFileName )
		{
			Archives.Clear();
			ArchivesInfo.Clear();
			LocalizationStore.Clear();

			Logger.Title( "Opening: " + ManifestFileName );

			// Load in the localization manifest
			LocalizationManifest Manifest = JsonHelper.ReadJsonFile<LocalizationManifest>( ManifestFileName );
			if( Manifest.Children == null && Manifest.Subnamespaces == null )
			{
				Logger.Error( "Failed to load manifest!" );
				return null;
			}

			Logger.Log( " ... found " + Manifest.Subnamespaces.Count + " namespaces." );

			ManifestBasePath = Path.GetDirectoryName( ManifestFileName );
			ManifestBaseFileName = Path.GetFileName( ManifestFileName );

			// Locate languages based on child folders of the manifest folder containing an archive file
			DiscoverLanguages( ManifestFileName );
			Logger.Log( " ... found " + ArchivesInfo.Count + " Unreal Engine languages" );

			// Cache all the localized strings in the archive files into a store
			foreach( string LanguageFolder in ArchivesInfo.Keys )
			{
				string ArchiveFolder = Path.GetDirectoryName( ManifestFileName );
				string ArchiveFileName = Path.ChangeExtension( Path.GetFileName( ManifestFileName ), "archive" );
				string ArchivePathName = Path.Combine( ArchiveFolder, LanguageFolder, ArchiveFileName );
				Logger.Log( " ... loading archive: " + ArchivePathName );

				// Read in the archive file
				LocalizationArchive NewArchive = JsonHelper.ReadJsonFile<LocalizationArchive>( ArchivePathName );
				Archives.Add( LanguageFolder, NewArchive );

				// Cache every entry in the archive to the store
				Logger.Log( " ... caching archive for " + GetFullLanguageDescription( LanguageFolder ) );
				CacheArchive( Manifest, NewArchive, "", LanguageFolder );

				// Update the stats for the number of localized lines
				LocalizationArchiveInfo ArchiveInfo = ArchivesInfo[LanguageFolder];
				UpdateStats( LanguageFolder, ArchiveInfo );

				// If this language is the source language, make sure all the source and translated strings match
				if( LanguageFolder == Config.DefaultLanguage )
				{
					Logger.Log( " ... ensuring all translated items match the source items for the default language" );
					ValidateDefaultLanguage( NewArchive );
				}

				Logger.Success( " ... " + ArchiveInfo.TotalLineCount + " total lines (" + ArchiveInfo.TotalWordCount + " words), of which " + ArchiveInfo.UntranslatedLineCount + " lines are untranslated (" +
								ArchiveInfo.UntranslatedWordCount + " words)" );
			}

			FormsLogger.Success( " ... loaded " + ArchivesInfo.Count + " archives." );
			return Manifest;
		}

		/// <summary>
		/// </summary>
		/// <param name="Archive"></param>
		/// <param name="LanguageId"></param>
		private void UploadUntranslated( GenericProgressBar ProgressBar, string LanguageFolder )
		{
			if( Running )
			{
				foreach( string FullKey in LocalizationStore.Keys )
				{
					bool bUpload = false;
					Dictionary<string, string> LocalizedStrings = LocalizationStore[FullKey];
					if( LanguageFolder == null )
					{
						foreach( string LocalizedString in LocalizedStrings.Values )
						{
							if( LocalizedString.Length == 0 )
							{
								bUpload = true;
								break;
							}
						}
					}
					else
					{
						if( LocalizedStrings.Keys.Contains( LanguageFolder ) )
						{
							bUpload = ( LocalizedStrings[LanguageFolder].Length == 0 );
						}
					}

					if( bUpload )
					{
						if( ProgressBar != null )
						{
							ProgressBar.Bump();
						}

						if( LocalizedStrings.Keys.Contains( Config.DefaultLanguage ) )
						{
							int DefaultLanguageId = GetLanguageId( Config.DefaultLanguage );
							string Text = URLEncode( LocalizedStrings[Config.DefaultLanguage] );
							string GroupId;
							string TextId;
							GetGroupAndTextIds( FullKey, out GroupId, out TextId );

							string URL = "https://transfluent.com/v2/text/?language=" + DefaultLanguageId + "&text_id=" + TextId + "&group_id=" + GroupId + "&text=" + Text + "&token=" + AuthenticationData.response.token;
							string JsonResponse = BasicWebRequest.RequestWebResponse( URL, "POST" );

							TextPostResponse Response = JsonHelper.ReadJson<TextPostResponse>( JsonResponse );
							if( Response != null && Response.status == "OK" && Response.response )
							{
								Logger.Verbose( " ... uploaded '" + FullKey + "'" );
							}
							else
							{
								Logger.Warning( " ... failed to upload '" + FullKey + "' \"" + Text + "\"" );
							}
						}
						else
						{
							Logger.Warning( " ... could not find default language version of '" + FullKey + "'" );
						}
					}
				}
			}
		}

		/// <summary>
		/// Download translated strings from the Transfluent server
		/// </summary>
		/// <param name="ProgressBar">A progress bar to show the download progress. null displays no UI.</param>
		/// <param name="FilterLanguageFolder">The folder of the language to download, or null for all languages.</param>
		private void DownloadTranslated( GenericProgressBar ProgressBar, string FilterLanguageFolder )
		{
			if( Running )
			{
				List<UpdatedItem> UpdatedItems = new List<UpdatedItem>();

				// Iterate over the localization store, check for the translated status of a key, and download if it's translated
				foreach( string FullKey in LocalizationStore.Keys )
				{
					Dictionary<string, string> LocalizedStrings = LocalizationStore[FullKey];
					foreach( string LanguageFolder in LocalizedStrings.Keys )
					{
						if( FilterLanguageFolder == null || LanguageFolder == FilterLanguageFolder )
						{
							if( LocalizedStrings[LanguageFolder].Length == 0 )
							{
								if( ProgressBar != null )
								{
									ProgressBar.Bump();
								}

								int LanguageId = GetLanguageId( LanguageFolder );
								string GroupId;
								string TextId;
								GetGroupAndTextIds( FullKey, out GroupId, out TextId );

								// Check to see if the string has been localized
								string StatusURL = "https://transfluent.com/v2/text/status/?language=" + LanguageId + "&text_id=" + TextId + "&group_id=" + GroupId + "&token=" + AuthenticationData.response.token;
								string JsonStatusResponse = BasicWebRequest.RequestWebResponse( StatusURL, "GET" );
								TextStatusGetResponse Status = JsonHelper.ReadJson<TextStatusGetResponse>( JsonStatusResponse );

								if( Status != null && Status.response.is_translated )
								{
									string URL = "https://transfluent.com/v2/text/?language=" + LanguageId + "&text_id=" + TextId + "&group_id=" + GroupId + "&token=" + AuthenticationData.response.token;
									string JsonGetResponse = BasicWebRequest.RequestWebResponse( URL, "GET" );

									TextGetResponse NewTranslation = JsonHelper.ReadJson<TextGetResponse>( JsonGetResponse );

									if( NewTranslation != null && NewTranslation.status == "OK" )
									{
										UpdatedItems.Add( new UpdatedItem( FullKey, LanguageFolder, NewTranslation.response ) );
										Logger.Verbose( " ... downloaded '" + FullKey + "'" );
									}
									else
									{
										Logger.Warning( " ... failed to download '" + FullKey + "' \"" + Text + "\"" );
									}
								}
							}
						}
					}
				}

				// Update the store with downloaded items
				foreach( UpdatedItem Item in UpdatedItems )
				{
					LocalizationStore[Item.FullKey][Item.LanguageFolder] = Item.Translation;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Manifest"></param>
		/// <param name="Archive"></param>
		/// <param name="ArchiveInfo"></param>
		/// <param name="Namespace"></param>
		/// <param name="LanguageFolder"></param>
		private void UpdateArchive( LocalizationManifest Manifest, LocalizationArchive Archive, LocalizationArchiveInfo ArchiveInfo, string Namespace, string LanguageFolder )
		{
			string CurrentNamespace = Archive.Namespace;
			if( Namespace.Length > 0 )
			{
				CurrentNamespace = Namespace + "." + Archive.Namespace;
			}

			if( Archive.Children != null )
			{
				foreach( LocalizationArchiveEntry Entry in Archive.Children )
				{
					string FullKey = GetFullKey( Manifest, CurrentNamespace, Entry.Source.Text );
					if( FullKey != null )
					{
						string StoreVersion = LocalizationStore[FullKey][LanguageFolder];
						if( StoreVersion != Entry.Translation.Text )
						{
							Entry.Translation.Text = StoreVersion;
							ArchiveInfo.bArchiveDirty = true;
						}
					}
				}
			}

			if( Archive.Subnamespaces != null )
			{
				foreach( LocalizationArchive ChildArchive in Archive.Subnamespaces )
				{
					UpdateArchive( Manifest, ChildArchive, ArchiveInfo, CurrentNamespace, LanguageFolder );
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FilterLanguageFolder"></param>
		private void UpdateArchives( string FilterLanguageFolder )
		{
			foreach( string LanguageFolder in Archives.Keys )
			{
				bool bUpdateArchive = ( LanguageFolder != Config.DefaultLanguage ) && ( FilterLanguageFolder == null || FilterLanguageFolder == LanguageFolder );
				if( bUpdateArchive )
				{
					LocalizationArchive Archive = Archives[LanguageFolder];
					LocalizationArchiveInfo ArchiveInfo = ArchivesInfo[LanguageFolder];

					ArchiveInfo.ClearCounters();
					UpdateStats( LanguageFolder, ArchiveInfo );

					UpdateArchive( CurrentManifest, Archive, ArchiveInfo, "", LanguageFolder );

					Logger.Log( " ... " + ArchiveInfo.TotalLineCount + " total lines (" + ArchiveInfo.TotalWordCount + " words), of which " + ArchiveInfo.UntranslatedLineCount + " lines are untranslated (" +
								ArchiveInfo.UntranslatedWordCount + " words)" );

					if( ArchiveInfo.bArchiveDirty )
					{
						string ArchivePathName = Path.Combine( ManifestBasePath, LanguageFolder, Path.ChangeExtension( ManifestBaseFileName, "archive" ) );
						if( JsonHelper.WriteJsonFile( ArchivePathName, Archive ) )
						{
							Logger.Success( " ... updated " + ArchivePathName );
							ArchiveInfo.bArchiveDirty = false;
						}
						else
						{
							Logger.Warning( "Failed to write '" + ArchivePathName + "'; is it read only/not opened for edit?" );
						}
					}
					else
					{
						Logger.Success( " ... archive for " + GetFullLanguageDescription( LanguageFolder ) + " unchanged!" );
					}
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void RemoveLanguageTranslations( string LanguageFolder )
		{
			Logger.Title( "Removing all translated strings for " + GetFullLanguageDescription( LanguageFolder ) );
			foreach( string FullKey in LocalizationStore.Keys )
			{
				LocalizationStore[FullKey][LanguageFolder] = "";
			}

			UpdateArchives( LanguageFolder );

			Logger.Success( " ... removed all translated text for " + GetFullLanguageDescription( LanguageFolder ) );
		}
	}
}