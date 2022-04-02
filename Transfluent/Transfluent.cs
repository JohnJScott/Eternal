// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Eternal.EternalUtilities;

namespace Transfluent
{
	public partial class Transfluent : Form
	{
		/// <summary></summary>
		private readonly Dictionary<string, LocalizationArchive> Archives = new Dictionary<string, LocalizationArchive>();

		/// <summary></summary>
		private readonly Dictionary<string, LocalizationArchiveInfo> ArchivesInfo = new Dictionary<string, LocalizationArchiveInfo>();

		/// <summary></summary>
		private AuthenticationResponse AuthenticationData;

		/// <summary></summary>
		public UserConfiguration Config;

		/// <summary></summary>
		private LocalizationManifest CurrentManifest;

		/// <summary></summary>
		private LanguagesResponse LanguagesData;

		/// <summary></summary>
		private Dictionary<string, Dictionary<string, string>> LocalizationStore = new Dictionary<string, Dictionary<string, string>>();

		/// <summary></summary>
		private string ManifestBaseFileName;

		/// <summary></summary>
		private string ManifestBasePath;

		/// <summary></summary>
		public bool Running = false;

		/// <summary>
		/// </summary>
		public Transfluent()
		{
			InitializeComponent();
		}

		/// <summary>
		/// </summary>
		public void SetWaitMode()
		{
			Application.UseWaitCursor = true;
			SetMenuState( false );
			Application.DoEvents();
		}

		/// <summary>
		/// </summary>
		public void ClearWaitMode()
		{
			Application.UseWaitCursor = false;
			SetMenuState( true );
			Application.DoEvents();
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public static bool IsInWaitMode()
		{
			return Application.UseWaitCursor;
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		private string GetConfigurationFileName()
		{
			return Path.ChangeExtension( Assembly.GetExecutingAssembly().Location, "json" );
		}

		/// <summary>
		/// </summary>
		private void ApplySettings()
		{
			FormsLogger.SuppressLogs = Config.SuppressLogging;
			ConsoleLogger.SuppressLogs = Config.SuppressLogging;

			FormsLogger.VerboseLogs = Config.VerboseLogging;
			ConsoleLogger.VerboseLogs = Config.VerboseLogging;
		}

		/// <summary>
		/// </summary>
		private void ReadConfigurationFile()
		{
			Config = JsonHelper.ReadJsonFile<UserConfiguration>( GetConfigurationFileName() );
			ApplySettings();
		}

		/// <summary>
		/// </summary>
		private void WriteConfigurationFile()
		{
			JsonHelper.WriteJsonFile( GetConfigurationFileName(), Config );
			ApplySettings();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="UserName"></param>
		/// <param name="Password"></param>
		/// <param name="ManifestFileName"></param>
		/// <param name="LanguageFolder"></param>
		/// <param name="Command"></param>
		/// <returns></returns>
		public int RunCommandline( Parameters CommandLine )
		{
			Running = true;
			ReadConfigurationFile();

			AuthenticationData = Login( CommandLine.UserName, CommandLine.Password );
			LanguagesData = GetSupportedLanguages();
			CurrentManifest = LoadManifest( CommandLine.ManifestFileName );

			switch( CommandLine.Command )
			{
				case Operation.UploadUntranslated:
					UploadUntranslated( null, CommandLine.LanguageFolder );
					break;

				case Operation.DownloadTranslated:
					DownloadTranslated( null, CommandLine.LanguageFolder );
					UpdateArchives( CommandLine.LanguageFolder );
					break;

				case Operation.RefreshArchives:
					UploadUntranslated( null, CommandLine.LanguageFolder );
					DownloadTranslated( null, CommandLine.LanguageFolder );
					UpdateArchives( CommandLine.LanguageFolder );
					break;
			}

			return 0;
		}

		/// <summary>
		/// </summary>
		/// <param name="UserName"></param>
		/// <param name="Password"></param>
		public void Initialize( string UserName, string Password )
		{
			FormsLogger.SetRecipient( this, RichTextBoxLog );

			Show();

			Logger.Title( "Loading settings" );
			ReadConfigurationFile();

			AuthenticationData = Login( UserName, Password );

			if( AuthenticationData != null && AuthenticationData.status == "OK" )
			{
				Logger.Log( " ... status '" + AuthenticationData.status + "'" );
				Logger.Log( " ... password expires '" + AuthenticationData.response.expires + "'" );

				LanguagesData = GetSupportedLanguages();

				LoadManifestClick( null, null );

				if( CurrentManifest != null )
				{
					PopulateMenus();

					Running = true;
				}
			}
			else
			{
				MessageBox.Show( "Unable to login with the given user email and password combination.", "Invalid Credentials", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		/// <summary>
		/// </summary>
		public void Tick()
		{
		}

		/// <summary>
		/// </summary>
		public void Shutdown()
		{
			WriteConfigurationFile();
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void QuitClick( object Sender, EventArgs EventArguments )
		{
			Running = false;
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void FormClosedClick( object Sender, FormClosedEventArgs EventArguments )
		{
			Running = false;
		}

		/// <summary>
		/// </summary>
		/// <param name="LanguageFolder"></param>
		/// <returns></returns>
		private string GetLanguageCode( string LanguageFolder )
		{
			return ArchivesInfo[LanguageFolder].LanguageCode;
		}

		/// <summary>
		/// </summary>
		/// <param name="LanguageFolder"></param>
		/// <returns></returns>
		private int GetLanguageId( string LanguageFolder )
		{
			return LanguagesData.response[GetLanguageCode( LanguageFolder )].id;
		}

		/// <summary>
		/// </summary>
		/// <param name="LanguageFolder"></param>
		/// <returns></returns>
		private string GetLanguageName( string LanguageFolder )
		{
			return LanguagesData.response[GetLanguageCode( LanguageFolder )].name;
		}

		/// <summary>
		/// </summary>
		/// <param name="LanguageFolder"></param>
		/// <returns></returns>
		private string GetFullLanguageDescription( string LanguageFolder )
		{
			return "'" + GetLanguageCode( LanguageFolder ) + "' (" + GetLanguageName( LanguageFolder ) + ")";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FullKey"></param>
		/// <param name="GroupId"></param>
		/// <param name="TextId"></param>
		private void GetGroupAndTextIds( string FullKey, out string GroupId, out string TextId )
		{
			List<string> KeyParts = FullKey.Split( ".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries ).ToList();

			TextId = KeyParts.Last();
			KeyParts.RemoveAt( KeyParts.Count - 1 );
			GroupId = String.Join( ".", KeyParts );
		}

		/// <summary>
		/// </summary>
		private void PopulateMenus()
		{
			RemoveTranslationsToolStripMenuItem.DropDownItems.Clear();
			UploadUntranslatedToolStripMenuItem.DropDownItems.Clear();
			DownloadTranslatedToolStripMenuItem.DropDownItems.Clear();

			foreach( string LanguageFolder in ArchivesInfo.Keys )
			{
				if( LanguageFolder != Config.DefaultLanguage )
				{
					ToolStripItem NewItem = new ToolStripMenuItem( GetFullLanguageDescription( LanguageFolder ) );
					NewItem.Click += RemoveTranslationsClick;
					NewItem.Tag = LanguageFolder;
					RemoveTranslationsToolStripMenuItem.DropDownItems.Add( NewItem );

					NewItem = new ToolStripMenuItem( GetFullLanguageDescription( LanguageFolder ) );
					NewItem.Click += UploadUntranslatedClick;
					NewItem.Tag = LanguageFolder;
					UploadUntranslatedToolStripMenuItem.DropDownItems.Add( NewItem );

					NewItem = new ToolStripMenuItem( GetFullLanguageDescription( LanguageFolder ) );
					NewItem.Click += DownloadTranslatedClick;
					NewItem.Tag = LanguageFolder;
					DownloadTranslatedToolStripMenuItem.DropDownItems.Add( NewItem );
				}
			}

			SetMenuState( true );
		}

		/// <summary>
		/// </summary>
		/// <param name="bEnabled"></param>
		private void SetMenuState( bool bEnabled )
		{
			LoadManifestToolStripMenuItem.Enabled = bEnabled;
			SpawnUE4CommandletToolStripMenuItem.Enabled = bEnabled;

			foreach( ToolStripItem Item in RemoveTranslationsToolStripMenuItem.DropDownItems )
			{
				Item.Enabled = bEnabled;
			}

			foreach( ToolStripItem Item in UploadUntranslatedToolStripMenuItem.DropDownItems )
			{
				Item.Enabled = bEnabled;
			}

			foreach( ToolStripItem Item in DownloadTranslatedToolStripMenuItem.DropDownItems )
			{
				Item.Enabled = bEnabled;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private int GetUntranslatedLineTotal()
		{
			int UntranslatedLines = 0;
			foreach( string LanguageFolder in ArchivesInfo.Keys )
			{
				if( LanguageFolder != Config.DefaultLanguage )
				{
					UntranslatedLines += ArchivesInfo[LanguageFolder].UntranslatedLineCount;
				}
			}

			return UntranslatedLines;
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void LoadManifestClick( object Sender, EventArgs EventArguments )
		{
			GenericOpenFileDialog.Title = "Open an Unreal Engine 4 localization manifest";

			if( GenericOpenFileDialog.ShowDialog() == DialogResult.OK )
			{
				SetWaitMode();
				CurrentManifest = LoadManifest( GenericOpenFileDialog.FileName );
				ClearWaitMode();
			}
			else
			{
				Logger.Log( " ... canceled." );
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="EventArguments"></param>
		private void UploadUntranslatedClick( object Sender, EventArgs EventArguments )
		{
			ToolStripMenuItem Item = ( ToolStripMenuItem ) Sender;
			string LanguageFolder = ( string ) Item.Tag;
			if( LanguageFolder != null )
			{
				SetWaitMode();
				string Explanation = "Uploading all untranslated strings for the language " + GetFullLanguageDescription( LanguageFolder );
				Logger.Title( Explanation );

				int UntranslatedLines = ArchivesInfo[LanguageFolder].UntranslatedLineCount;

				using( GenericProgressBar ProgressBar = new GenericProgressBar( "Uploading untranslated strings", Explanation, UntranslatedLines ) )
				{
					UploadUntranslated( ProgressBar, LanguageFolder );
				}

				Logger.Success( " ... uploaded " + UntranslatedLines + " strings" );
				ClearWaitMode();
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void UploadAllUntranslatedClick( object Sender, EventArgs EventArguments )
		{
			Logger.Title( "Uploading all untranslated strings for all languages" );
			SetWaitMode();
			string Explanation = "Uploading all untranslated strings for the language for all languages";
			Logger.Title( Explanation );

			int UntranslatedLines = GetUntranslatedLineTotal();

			using( GenericProgressBar ProgressBar = new GenericProgressBar( "Uploading untranslated strings", Explanation, UntranslatedLines ) )
			{
				UploadUntranslated( ProgressBar, null );
			}

			Logger.Success( " ... uploaded " + UntranslatedLines + " strings" );
			ClearWaitMode();
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void DownloadTranslatedClick( object Sender, EventArgs EventArguments )
		{
			ToolStripMenuItem Item = ( ToolStripMenuItem ) Sender;
			string LanguageFolder = ( string ) Item.Tag;
			if( LanguageFolder != null )
			{
				SetWaitMode();
				string Explanation = "Downloading all untranslated strings for the language " + GetFullLanguageDescription( LanguageFolder );
				Logger.Title( Explanation );

				int UntranslatedLines = ArchivesInfo[LanguageFolder].UntranslatedLineCount;

				using( GenericProgressBar ProgressBar = new GenericProgressBar( "Downloading translated strings", Explanation, UntranslatedLines ) )
				{
					DownloadTranslated( ProgressBar, LanguageFolder );
				}

				UpdateArchives( LanguageFolder );

				ClearWaitMode();
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void DownloadAllTranslatedClick( object Sender, EventArgs EventArguments )
		{
			Logger.Title( "Downloading all translated strings for all languages" );
			SetWaitMode();
			string Explanation = "Downloading all untranslated strings for all languages.";
			Logger.Title( Explanation );

			int UntranslatedLines = GetUntranslatedLineTotal();

			using( GenericProgressBar ProgressBar = new GenericProgressBar( "Downloading translated strings", Explanation, UntranslatedLines ) )
			{
				DownloadTranslated( ProgressBar, null );
			}

			UpdateArchives( null );

			ClearWaitMode();
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void RemoveTranslationsClick( object Sender, EventArgs EventArguments )
		{
			ToolStripMenuItem Item = ( ToolStripMenuItem ) Sender;
			string LanguageFolder = ( string ) Item.Tag;
			if( LanguageFolder != null )
			{
				RemoveLanguageTranslations( LanguageFolder );
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void SettingsClick( object Sender, EventArgs EventArguments )
		{
			using( SettingsDialog Dialog = new SettingsDialog( this ) )
			{
				Dialog.ShowDialog();
				WriteConfigurationFile();
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="PathName"></param>
		/// <returns></returns>
		private bool CheckForWritable( string PathName )
		{
			FileInfo Info = new FileInfo( PathName );
			if( Info.IsReadOnly )
			{
				Logger.Error( "File (" + Info.FullName + ") is read only; this will cause the commandlet to fail. Does it need to be opened for edit?" );
				return false;
			}

			return true;
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void SpawnUE4Click( object Sender, EventArgs EventArguments )
		{
			Logger.Title( "Attempting to spawn the commandlets to generate localization files." );

			string ExecutableLocation = Path.Combine( ManifestBasePath, "..", "..", "..", "Binaries", "Win64", "UE4Editor-Cmd.exe" );
			FileInfo ExecutableInfo = new FileInfo( ExecutableLocation );
			if( !ExecutableInfo.Exists )
			{
				Logger.Error( "Cannot find executable to run the commandlet (" + ExecutableInfo.FullName +
							"). A game needs to be compiled with the Development-Editor configuration, and that game name needs to be set in the settings." );
				return;
			}

			string ConfigLocation = Path.Combine( ManifestBasePath, "..", "..", "..", "Config", "Localization", Path.ChangeExtension( ManifestBaseFileName, "ini" ) );
			FileInfo ConfigInfo = new FileInfo( ConfigLocation );
			if( !ConfigInfo.Exists )
			{
				Logger.Error( "Cannot find configuration file to generate archives (" + ConfigInfo.FullName + "). A configuration file of the same name as the archive needs to exist, and be properly setup." );
				return;
			}

			string ManifestFileName = Path.Combine( ManifestBasePath, ManifestBaseFileName );
			if( !CheckForWritable( ManifestFileName ) )
			{
				return;
			}

			foreach( string LanguageFolder in Archives.Keys )
			{
				string ArchiveFileName = Path.Combine( ManifestBasePath, LanguageFolder, Path.ChangeExtension( ManifestBaseFileName, "archive" ) );
				if( !CheckForWritable( ArchiveFileName ) )
				{
					return;
				}

				string LocResFileName = Path.Combine( ManifestBasePath, LanguageFolder, Path.ChangeExtension( ManifestBaseFileName, "locres" ) );
				if( !CheckForWritable( LocResFileName ) )
				{
					return;
				}
			}

			string WorkingDirectory = Path.Combine( ManifestBasePath, "..", "..", ".." );
			string RelativeConfigLocation = Path.Combine( "..", "..", "Config", "Localization", Path.ChangeExtension( ManifestBaseFileName, "ini" ) );
			ConsoleProcess CommandletProcess = new ConsoleProcess( ExecutableInfo.FullName, WorkingDirectory, null, Config.GameName, "GatherTextCommandlet", "-Config=" + RelativeConfigLocation );

			int ExitCode = CommandletProcess.Wait( 120 * 1000 );
			if( ExitCode < 0 )
			{
				Logger.Error( "Commandlet failed; please examine the game log. The path is 'ProjectRoot/Saved/Logs/*.log'" );
				return;
			}

			Logger.Success( "Localization commandlets completed successfully!" );
		}
	}
}