// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Eternal.EternalUtilities
{
	/// <summary>A class to encapsulate reading and writing of Json files.</summary>
	public static class JsonHelper
	{
		/// <summary>Create sensible default settings for reading Json files.</summary>
		/// <returns>Json reader settings to use.</returns>
		private static JsonSerializerSettings GetDefaultJsonReaderSettings()
		{
			JsonSerializerSettings DefaultReaderSettings = new JsonSerializerSettings();
			return DefaultReaderSettings;
		}

		/// <summary>Create sensible default settings for writing Json files.</summary>
		/// <returns> writer settings to use.</returns>
		private static JsonSerializerSettings GetDefaultJsonWriterSettings()
		{
			JsonSerializerSettings DefaultWriterSettings = new JsonSerializerSettings();
			DefaultWriterSettings.Formatting = Formatting.Indented;
			return DefaultWriterSettings;
		}

		/// <summary>Parse a Json file into an instance of the class.</summary>
		/// <param name="JsonFileName">Name of Xml file to parse.</param>
		/// <param name="CustomSettings">Optional custom serialisation settings.</param>
		/// <typeparam name="TClass">The type of the class to create and parse.</typeparam>
		/// <returns>An instance of the class parsed from the Json file.</returns>
		/// <remarks>An instance created with the default constructor is returned if there is a problem parsing the file. An error is printed if any exception is encountered.</remarks>
		public static TClass ReadJsonFile<TClass>( string JsonFileName, JsonSerializerSettings CustomSettings = null ) 
			where TClass : new()
		{
			TClass Instance = new TClass();
			FileInfo JsonFileInfo = new FileInfo( JsonFileName );
			try
			{
				if( JsonFileInfo.Exists )
				{
					StreamReader Reader = JsonFileInfo.OpenText();
					string JsonData = Reader.ReadToEnd();
					Reader.Close();

					if( CustomSettings == null )
					{
						CustomSettings = GetDefaultJsonReaderSettings();
					}

					Instance = JsonConvert.DeserializeObject<TClass>( JsonData, CustomSettings );
				}
			}
			catch( Exception Ex )
			{
				ConsoleLogger.Error( "Exception during deserialization of " + JsonFileInfo.FullName + " with exception " + Ex.Message );
			}

			return Instance;
		}

		/// <summary>
		/// Deserialize a string into a class instance
		/// </summary>
		/// <typeparam name="TClass">The class to create and populate.</typeparam>
		/// <param name="JsonData">The json data.</param>
		/// <param name="CustomSettings">Custom reader settings.</param>
		/// <returns>A new instance of TClass with data populated from the input string.</returns>
		public static TClass ReadJson<TClass>( string JsonData, JsonSerializerSettings CustomSettings = null )
			where TClass : new()
		{
			TClass Instance = new TClass();

			try
			{
				if( CustomSettings == null )
				{
					CustomSettings = GetDefaultJsonReaderSettings();
				}

				Instance = JsonConvert.DeserializeObject<TClass>( JsonData, CustomSettings );
			}
			catch( Exception Ex )
			{
				ConsoleLogger.Error( "Exception during deserialization of json data with exception " + Ex.Message );
			}

			return Instance;
		}

		/// <summary>Write an instance of a class to a Json file.</summary>
		/// <param name="JsonFileName">Name of Xml file to write the class to.</param>
		/// <param name="Instance">The instance of the class to write to disk.</param>
		/// <typeparam name="TClass">Type of the class to write as Xml.</typeparam>
		/// <returns>True if the Json file was successfully written.</returns>
		/// <remarks>An error is printed if any exception is encountered.</remarks>
		public static bool WriteJsonFile<TClass>( string JsonFileName, TClass Instance )
		{
			bool WriteSuccessful = false;

			FileInfo JsonFileInfo = new FileInfo( JsonFileName );
			try
			{
				if( JsonFileInfo.Exists && JsonFileInfo.IsReadOnly )
				{
					JsonFileInfo.IsReadOnly = false;
				}

				if( JsonFileInfo.Exists )
				{
					JsonFileInfo.Delete();
					JsonFileInfo.Refresh();
				}

				JsonSerializer Serializer = new JsonSerializer();
				using( StreamWriter Writer = new StreamWriter( JsonFileInfo.FullName, false, Encoding.Unicode ) )
				{
					JsonTextWriter Json = new JsonTextWriter( Writer );
					Json.Formatting = Formatting.Indented;
					Json.IndentChar = '\t';
					Json.Indentation = 1;

					Serializer.Serialize( Json, Instance );
				}

				WriteSuccessful = true;
			}
			catch( Exception Ex )
			{
				ConsoleLogger.Error( "Exception during json serialization of " + JsonFileInfo.FullName + " with exception " + Ex.Message );
			}

			return WriteSuccessful;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TClass"></typeparam>
		/// <param name="Instance"></param>
		/// <param name="CustomSettings"></param>
		/// <returns></returns>
		public static string WriteJson<TClass>( TClass Instance, JsonSerializerSettings CustomSettings = null )
		{
			string JsonOutput = "";

			try
			{
				if( CustomSettings == null )
				{
					CustomSettings = GetDefaultJsonWriterSettings();
				}

				JsonOutput = JsonConvert.SerializeObject( Instance, CustomSettings );
			}
			catch( Exception Ex )
			{
				ConsoleLogger.Error( "Exception during json serialization with exception " + Ex.Message );
			}

			return JsonOutput;
		}
	}
}
