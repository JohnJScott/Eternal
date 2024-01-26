// Copyright Eternal Developments LLC. All Rights Reserved.

using System.Xml;
using System.Xml.Serialization;

namespace Eternal.ConsoleUtilities
{
	/// <summary>A class to encapsulate reading and writing of Xml files.</summary>
	public static class XmlHelper
	{
		/// <summary>Create sensible default settings for reading Xml files.</summary>
		/// <returns>Xml reader settings to use.</returns>
		private static XmlReaderSettings GetDefaultXmlReaderSettings()
		{
			XmlReaderSettings default_reader_settings = new XmlReaderSettings();

			default_reader_settings.CloseInput = true;
			default_reader_settings.IgnoreComments = true;

			return default_reader_settings;
		}

		/// <summary>Create sensible default settings for writing Xml files.</summary>
		/// <returns>Xml writer settings to use.</returns>
		private static XmlWriterSettings GetDefaultXmlWriterSettings()
		{
			XmlWriterSettings default_writer_settings = new XmlWriterSettings();

			default_writer_settings.CloseOutput = true;
			default_writer_settings.Indent = true;
			default_writer_settings.IndentChars = "\t";
			default_writer_settings.NewLineChars = Environment.NewLine;
			default_writer_settings.OmitXmlDeclaration = true;

			return default_writer_settings;
		}

		/// <summary>Callback for parsed attributes that are unknown.</summary>
		/// <param name="sender">The serializer object that came across the unknown attribute while parsing.</param>
		/// <param name="arguments">Details and location of the unknown attribute.</param>
		/// <remarks>Details of the missing item are logged in verbose mode.</remarks>
		private static void UnknownXmlAttribute( object? sender, XmlAttributeEventArgs arguments )
		{
			ConsoleLogger.Verbose( " ... unknown attribute " + arguments.Attr + " at line " + arguments.LineNumber + " position " + arguments.LinePosition );
		}

		/// <summary>Callback for parsed elements that are unknown.</summary>
		/// <param name="sender">The serializer object that came across the unknown element while parsing.</param>
		/// <param name="arguments">Details and location of the unknown element.</param>
		/// <remarks>Details of the missing item are logged in verbose mode.</remarks>
		private static void UnknownXmlElement( object? sender, XmlElementEventArgs arguments )
		{
			ConsoleLogger.Verbose( " ... unknown element '" + arguments.Element.Name + "' at line " + arguments.LineNumber + " position " + arguments.LinePosition );
		}

		/// <summary>Callback for parsed nodes that are unknown.</summary>
		/// <param name="sender">The serializer object that came across the unknown node while parsing.</param>
		/// <param name="arguments">Details and location of the unknown node.</param>
		/// <remarks>Details of the missing item are logged in verbose mode.</remarks>
		private static void UnknownXmlNode( object? sender, XmlNodeEventArgs arguments )
		{
			ConsoleLogger.Verbose( " ... unknown node '" + arguments.Name + "' at line " + arguments.LineNumber + " position " + arguments.LinePosition );
		}

		/// <summary>Parse an Xml file into an instance of the class.</summary>
		/// <param name="xmlFileName">Name of Xml file to parse.</param>
		/// <param name="customSettings">Optional Xml reader settings to use.</param>
		/// <typeparam name="TClass">The type of the class to create and parse.</typeparam>
		/// <returns>An instance of the class parsed from the Xml file.</returns>
		/// <remarks>An instance created with the default constructor is returned if there is a problem parsing the file. An error is printed if any exception is encountered.</remarks>
		public static TClass? ReadXmlFile<TClass>( string xmlFileName, XmlReaderSettings? customSettings = null )
			where TClass : new()
		{
			TClass? instance = new TClass();

			FileInfo xml_file_info = new FileInfo( xmlFileName );
			try
			{
				if( xml_file_info.Exists )
				{
					if( customSettings == null )
					{
						customSettings = GetDefaultXmlReaderSettings();
					}

					using( XmlReader Reader = XmlReader.Create( xml_file_info.FullName, customSettings ) )
					{
						XmlSerializer Serializer = new XmlSerializer( typeof( TClass ) );

						Serializer.UnknownAttribute += UnknownXmlAttribute;
						Serializer.UnknownElement += UnknownXmlElement;
						Serializer.UnknownNode += UnknownXmlNode;

						instance = ( TClass? )Serializer.Deserialize( Reader );
					}
				}
			}
			catch( Exception exception )
			{
				ConsoleLogger.Error( "Exception during deserialization of " + xml_file_info.FullName + " with exception " + exception.Message );
			}

			return instance;
		}

		/// <summary>Write an instance of a class to an Xml file.</summary>
		/// <param name="xmlFileName">Name of Xml file to write the class to.</param>
		/// <param name="instance">The instance of the class to write to disk.</param>
		/// <param name="customSettings">Optional custom writer settings to refine the Xml output.</param>
		/// <typeparam name="TClass">Type of the class to write as Xml.</typeparam>
		/// <returns>True if the Xml file was successfully written.</returns>
		/// <remarks>An error is printed if any exception is encountered.</remarks>
		public static bool WriteXmlFile<TClass>( string xmlFileName, TClass instance, XmlWriterSettings? customSettings = null )
		{
			bool write_successful = false;

			FileInfo xml_file_info = new FileInfo( xmlFileName );
			try
			{
				if( customSettings == null )
				{
					customSettings = GetDefaultXmlWriterSettings();
				}

				using( XmlWriter Writer = XmlWriter.Create( xml_file_info.FullName, customSettings ) )
				{
					XmlSerializer Serializer = new XmlSerializer( typeof( TClass ) );

					Serializer.UnknownAttribute += UnknownXmlAttribute;
					Serializer.UnknownElement += UnknownXmlElement;
					Serializer.UnknownNode += UnknownXmlNode;

					Serializer.Serialize( Writer, instance );
					write_successful = true;
				}
			}
			catch( Exception exception )
			{
				ConsoleLogger.Error( "Exception during serialization of " + xml_file_info.FullName + " with exception " + exception.Message );
			}

			return write_successful;
		}
	}
}
