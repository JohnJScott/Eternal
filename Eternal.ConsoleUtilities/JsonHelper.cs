// Copyright 2015-2022 Eternal Developments LLC. All Rights Reserved.

using System.Text;
using Newtonsoft.Json;

namespace Eternal.ConsoleUtilities
{
	/// <summary>A class to encapsulate reading and writing of Json files.</summary>
	public static class JsonHelper
	{
		/// <summary>Create sensible default settings for reading Json files.</summary>
		/// <returns>Json reader settings to use.</returns>
		private static JsonSerializerSettings GetDefaultJsonReaderSettings()
		{
			JsonSerializerSettings default_reader_settings = new JsonSerializerSettings();
			return default_reader_settings;
		}

		/// <summary>Create sensible default settings for writing Json files.</summary>
		/// <returns> writer settings to use.</returns>
		private static JsonSerializerSettings GetDefaultJsonWriterSettings()
		{
			JsonSerializerSettings default_writer_settings = new JsonSerializerSettings();
			default_writer_settings.Formatting = Formatting.Indented;
			return default_writer_settings;
		}

		/// <summary>Parse a Json file into an instance of the class.</summary>
		/// <param name="jsonFileName">Name of Xml file to parse.</param>
		/// <param name="customSettings">Optional custom serialisation settings.</param>
		/// <typeparam name="TClass">The type of the class to create and parse.</typeparam>
		/// <returns>An instance of the class parsed from the Json file.</returns>
		/// <remarks>An instance created with the default constructor is returned if there is a problem parsing the file. An error is printed if any exception is encountered.</remarks>
		public static TClass? ReadJsonFile<TClass>( string jsonFileName, JsonSerializerSettings? customSettings = null )
			where TClass : new()
		{
			TClass? instance = new TClass();
			FileInfo json_file_info = new FileInfo( jsonFileName );
			try
			{
				if( json_file_info.Exists )
				{
					StreamReader reader = json_file_info.OpenText();
					string json_data = reader.ReadToEnd();
					reader.Close();

					if( customSettings == null )
					{
						customSettings = GetDefaultJsonReaderSettings();
					}

					instance = JsonConvert.DeserializeObject<TClass>( json_data, customSettings );
				}
			}
			catch( Exception exception )
			{
				ConsoleLogger.Error( "Exception during deserialization of " + json_file_info.FullName + " with exception " + exception.Message );
			}

			return instance;
		}

		/// <summary>
		/// Deserialize a string into a class instance
		/// </summary>
		/// <typeparam name="TClass">The class to create and populate.</typeparam>
		/// <param name="jsonData">The json data.</param>
		/// <param name="customSettings">Custom reader settings.</param>
		/// <returns>A new instance of TClass with data populated from the input string.</returns>
		public static TClass? ReadJson<TClass>( string jsonData, JsonSerializerSettings? customSettings = null )
			where TClass : new()
		{
			TClass? instance = new TClass();

			try
			{
				if( customSettings == null )
				{
					customSettings = GetDefaultJsonReaderSettings();
				}

				instance = JsonConvert.DeserializeObject<TClass>( jsonData, customSettings );
			}
			catch( Exception exception )
			{
				ConsoleLogger.Error( "Exception during deserialization of json data with exception " + exception.Message );
			}

			return instance;
		}

		/// <summary>Write an instance of a class to a Json file.</summary>
		/// <param name="jsonFileName">Name of Xml file to write the class to.</param>
		/// <param name="instance">The instance of the class to write to disk.</param>
		/// <typeparam name="TClass">Type of the class to write as Xml.</typeparam>
		/// <returns>True if the Json file was successfully written.</returns>
		/// <remarks>An error is printed if any exception is encountered.</remarks>
		public static bool WriteJsonFile<TClass>( string jsonFileName, TClass instance )
		{
			bool write_successful = false;

			FileInfo json_file_info = new FileInfo( jsonFileName );
			try
			{
				if( json_file_info.Exists && json_file_info.IsReadOnly )
				{
					json_file_info.IsReadOnly = false;
				}

				if( json_file_info.Exists )
				{
					json_file_info.Delete();
					json_file_info.Refresh();
				}

				JsonSerializer serializer = new JsonSerializer();
				using( StreamWriter writer = new StreamWriter( json_file_info.FullName, false, Encoding.Unicode ) )
				{
					JsonTextWriter json = new JsonTextWriter( writer );
					json.Formatting = Formatting.Indented;
					json.IndentChar = '\t';
					json.Indentation = 1;

					serializer.Serialize( json, instance );
				}

				write_successful = true;
			}
			catch( Exception exception )
			{
				ConsoleLogger.Error( "Exception during json serialization of " + json_file_info.FullName + " with exception " + exception.Message );
			}

			return write_successful;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TClass"></typeparam>
		/// <param name="instance"></param>
		/// <param name="customSettings"></param>
		/// <returns></returns>
		public static string WriteJson<TClass>( TClass instance, JsonSerializerSettings? customSettings = null )
		{
			string json_output = "";

			try
			{
				if( customSettings == null )
				{
					customSettings = GetDefaultJsonWriterSettings();
				}

				json_output = JsonConvert.SerializeObject( instance, customSettings );
			}
			catch( Exception exception )
			{
				ConsoleLogger.Error( "Exception during json serialization with exception " + exception.Message );
			}

			return json_output;
		}
	}
}
