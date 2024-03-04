// Copyright Eternal Developments LLC. All Rights Reserved.

using System.Text;
using YamlDotNet.Serialization;

namespace Eternal.ConsoleUtilities
{
	/// <summary>
	/// Stub for Yaml reader settings
	/// </summary>
	public class YamlReaderSettings
	{
	}

	/// <summary>
	/// Stub for Yaml writer settings
	/// </summary>
	public class YamlWriterSettings
	{
	}

	/// <summary>A class to encapsulate reading and writing of Yaml files.</summary>
	public static class YamlHelper
	{
		/// <summary>Create sensible default settings for reading Yaml files.</summary>
		/// <returns>Yaml reader settings to use.</returns>
		private static IDeserializer GetDefaultYamlReaderSettings( YamlReaderSettings? writeSettings )
		{
			IDeserializer default_reader_settings = new DeserializerBuilder().Build();
			return default_reader_settings;
		}

		/// <summary>Create sensible default settings for writing Yaml files.</summary>
		/// <returns>Yaml writer settings to use.</returns>
		private static ISerializer GetDefaultYamlWriterSettings( YamlWriterSettings? writeSettings )
		{
			ISerializer default_writer_settings = new SerializerBuilder().WithIndentedSequences().Build();
			return default_writer_settings;
		}

		/// <summary>Parse a Yaml file into an instance of the class.</summary>
		/// <param name="yamlFileName">Name of Xml file to parse.</param>
		/// <param name="customSettings">Optional custom serialisation settings.</param>
		/// <typeparam name="TClass">The type of the class to create and parse.</typeparam>
		/// <returns>An instance of the class parsed from the Yaml file.</returns>
		/// <remarks>An instance created with the default constructor is returned if there is a problem parsing the file. An error is printed if any exception is encountered.</remarks>
		public static TClass? ReadYamlFile<TClass>( string yamlFileName, YamlReaderSettings? customSettings = null )
			where TClass : new()
		{
			TClass? instance = new TClass();
			FileInfo yaml_file_info = new FileInfo( yamlFileName );
			try
			{
				if( yaml_file_info.Exists )
				{
					StreamReader reader = yaml_file_info.OpenText();
					string yaml_data = reader.ReadToEnd();
					reader.Close();

					IDeserializer deserializer = GetDefaultYamlReaderSettings( customSettings );
					instance = deserializer.Deserialize<TClass>( yaml_data );
				}
			}
			catch( Exception exception )
			{
				ConsoleLogger.Error( "Exception during deserialization of " + yaml_file_info.FullName + " with exception " + exception.Message );
			}

			return instance;
		}

		/// <summary>
		/// Deserialize a string into a class instance
		/// </summary>
		/// <typeparam name="TClass">The class to create and populate.</typeparam>
		/// <param name="yamlData">The yaml data.</param>
		/// <param name="customSettings">Custom reader settings.</param>
		/// <returns>A new instance of TClass with data populated from the input string.</returns>
		public static TClass? ReadYaml<TClass>( string yamlData, YamlReaderSettings? customSettings = null )
			where TClass : new()
		{
			TClass? instance = new TClass();

			try
			{
				IDeserializer deserializer = GetDefaultYamlReaderSettings( customSettings );
				instance = deserializer.Deserialize<TClass>( yamlData );
			}
			catch( Exception exception )
			{
				ConsoleLogger.Error( "Exception during deserialization of yaml data with exception " + exception.Message );
			}

			return instance;
		}

		/// <summary>Write an instance of a class to a Yaml file.</summary>
		/// <param name="yamlFileName">Name of Xml file to write the class to.</param>
		/// <param name="instance">The instance of the class to write to disk.</param>
		/// <typeparam name="TClass">Type of the class to write as Xml.</typeparam>
		/// <returns>True if the Yaml file was successfully written.</returns>
		/// <remarks>An error is printed if any exception is encountered.</remarks>
		public static bool WriteYamlFile<TClass>( string yamlFileName, TClass instance, YamlWriterSettings? customSettings = null )
		{
			bool write_successful = false;

			FileInfo yaml_file_info = new FileInfo( yamlFileName );
			try
			{
				if( yaml_file_info.Exists && yaml_file_info.IsReadOnly )
				{
					yaml_file_info.IsReadOnly = false;
				}

				if( yaml_file_info.Exists )
				{
					yaml_file_info.Delete();
					yaml_file_info.Refresh();
				}

				using( StreamWriter writer = new StreamWriter( yaml_file_info.FullName, false, Encoding.Unicode ) )
				{
					ISerializer serializer = GetDefaultYamlWriterSettings( customSettings );
					writer.WriteLine( serializer.Serialize( instance ) );
				}

				write_successful = true;
			}
			catch( Exception exception )
			{
				ConsoleLogger.Error( "Exception during yaml serialization of " + yaml_file_info.FullName + " with exception " + exception.Message );
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
		public static string WriteYaml<TClass>( TClass instance, YamlWriterSettings? customSettings = null )
		{
			string yaml_output = "";

			try
			{
				ISerializer serializer = GetDefaultYamlWriterSettings( customSettings );
				yaml_output = serializer.Serialize( instance );
			}
			catch( Exception exception )
			{
				ConsoleLogger.Error( "Exception during yaml serialization with exception " + exception.Message );
			}

			return yaml_output;
		}
	}
}
