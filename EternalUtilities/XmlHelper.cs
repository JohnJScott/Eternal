// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Eternal.EternalUtilities
{
	/// <summary>A class to encapsulate reading and writing of Xml files.</summary>
	public static class XmlHelper
	{
		/// <summary>Create sensible default settings for reading Xml files.</summary>
		/// <returns>Xml reader settings to use.</returns>
		private static XmlReaderSettings GetDefaultXmlReaderSettings()
		{
			XmlReaderSettings DefaultReaderSettings = new XmlReaderSettings();

			DefaultReaderSettings.CloseInput = true;
			DefaultReaderSettings.IgnoreComments = true;

			return DefaultReaderSettings;
		}

		/// <summary>Create sensible default settings for writing Xml files.</summary>
		/// <returns>Xml writer settings to use.</returns>
		private static XmlWriterSettings GetDefaultXmlWriterSettings()
		{
			XmlWriterSettings DefaultWriterSettings = new XmlWriterSettings();

			DefaultWriterSettings.CloseOutput = true;
			DefaultWriterSettings.Indent = true;
			DefaultWriterSettings.IndentChars = "\t";
			DefaultWriterSettings.NewLineChars = Environment.NewLine;
			DefaultWriterSettings.OmitXmlDeclaration = true;

			return DefaultWriterSettings;
		}

		/// <summary>Callback for parsed attributes that are unknown.</summary>
		/// <param name="Sender">The serializer object that came across the unknown attribute while parsing.</param>
		/// <param name="Arguments">Details and location of the unknown attribute.</param>
		/// <remarks>Details of the missing item are logged in verbose mode.</remarks>
		private static void UnknownXmlAttribute( object Sender, XmlAttributeEventArgs Arguments )
		{
			ConsoleLogger.Verbose( " ... unknown attribute " + Arguments.Attr + " at line " + Arguments.LineNumber + " position " + Arguments.LinePosition );
		}

		/// <summary>Callback for parsed elements that are unknown.</summary>
		/// <param name="Sender">The serializer object that came across the unknown element while parsing.</param>
		/// <param name="Arguments">Details and location of the unknown element.</param>
		/// <remarks>Details of the missing item are logged in verbose mode.</remarks>
		private static void UnknownXmlElement( object Sender, XmlElementEventArgs Arguments )
		{
			ConsoleLogger.Verbose( " ... unknown element '" + Arguments.Element.Name + "' at line " + Arguments.LineNumber + " position " + Arguments.LinePosition );
		}

		/// <summary>Callback for parsed nodes that are unknown.</summary>
		/// <param name="Sender">The serializer object that came across the unknown node while parsing.</param>
		/// <param name="Arguments">Details and location of the unknown node.</param>
		/// <remarks>Details of the missing item are logged in verbose mode.</remarks>
		private static void UnknownXmlNode( object Sender, XmlNodeEventArgs Arguments )
		{
			ConsoleLogger.Verbose( " ... unknown node '" + Arguments.Name + "' at line " + Arguments.LineNumber + " position " + Arguments.LinePosition );
		}

		/// <summary>Parse an Xml file into an instance of the class.</summary>
		/// <param name="XmlFileName">Name of Xml file to parse.</param>
		/// <param name="CustomSettings">Optional Xml reader settings to use.</param>
		/// <typeparam name="TClass">The type of the class to create and parse.</typeparam>
		/// <returns>An instance of the class parsed from the Xml file.</returns>
		/// <remarks>An instance created with the default constructor is returned if there is a problem parsing the file. An error is printed if any exception is encountered.</remarks>
		public static TClass ReadXmlFile<TClass>( string XmlFileName, XmlReaderSettings CustomSettings = null ) 
			where TClass : new()
		{
			TClass Instance = new TClass();

			FileInfo XmlFileInfo = new FileInfo( XmlFileName );
			try
			{
				if( XmlFileInfo.Exists )
				{
					if( CustomSettings == null )
					{
						CustomSettings = GetDefaultXmlReaderSettings();
					}

					using( XmlReader Reader = XmlReader.Create( XmlFileInfo.FullName, CustomSettings ) )
					{
						XmlSerializer Serializer = new XmlSerializer( typeof( TClass ) );

						Serializer.UnknownAttribute += UnknownXmlAttribute;
						Serializer.UnknownElement += UnknownXmlElement;
						Serializer.UnknownNode += UnknownXmlNode;

						Instance = ( TClass )Serializer.Deserialize( Reader );
					}
				}
			}
			catch( Exception Ex )
			{
				ConsoleLogger.Error( "Exception during deserialization of " + XmlFileInfo.FullName + " with exception " + Ex.Message );
			}

			return Instance;
		}

		/// <summary>Write an instance of a class to an Xml file.</summary>
		/// <param name="XmlFileName">Name of Xml file to write the class to.</param>
		/// <param name="Instance">The instance of the class to write to disk.</param>
		/// <param name="CustomSettings">Optional custom writer settings to refine the Xml output.</param>
		/// <typeparam name="TClass">Type of the class to write as Xml.</typeparam>
		/// <returns>True if the Xml file was successfully written.</returns>
		/// <remarks>An error is printed if any exception is encountered.</remarks>
		public static bool WriteXmlFile<TClass>( string XmlFileName, TClass Instance, XmlWriterSettings CustomSettings = null )
		{
			bool WriteSuccessful = false;

			FileInfo XmlFileInfo = new FileInfo( XmlFileName );
			try
			{
				if( CustomSettings == null )
				{
					CustomSettings = GetDefaultXmlWriterSettings();
				}

				using( XmlWriter Writer = XmlWriter.Create( XmlFileInfo.FullName, CustomSettings ) )
				{
					XmlSerializer Serializer = new XmlSerializer( typeof( TClass ) );

					Serializer.UnknownAttribute += UnknownXmlAttribute;
					Serializer.UnknownElement += UnknownXmlElement;
					Serializer.UnknownNode += UnknownXmlNode;

					Serializer.Serialize( Writer, Instance );
					WriteSuccessful = true;
				}
			}
			catch( Exception Ex )
			{
				ConsoleLogger.Error( "Exception during serialization of " + XmlFileInfo.FullName + " with exception " + Ex.Message );
			}

			return WriteSuccessful;
		}
	}
}
