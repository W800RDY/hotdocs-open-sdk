﻿/* Copyright (c) 2013, HotDocs Limited
   Use, modification and redistribution of this source is subject
   to the New BSD License as set out in LICENSE.TXT. */

using System;
using System.IO;

namespace HotDocs.Sdk
{
	/// <summary>
	/// A type of document that can be produced by assembling a document from a template.
	/// </summary>
	public enum DocumentType
	{
		/// <summary></summary>
		Unknown = 0,
		/// <summary>
		/// Document type native to the template. See Template.NativeDocumentType. For example,
		/// a WordPerfect WPD file is native to a WordPerfect WPT template.
		/// </summary>
		Native,
		/// <summary></summary>
		WordDOCX,
		/// <summary></summary>
		WordRTF,
		/// <summary></summary>
		WordDOC,
		/// <summary></summary>
		WordPerfect,
		/// <summary></summary>
		PDF,
		/// <summary></summary>
		HPD,
		/// <summary></summary>
		HFD,
		/// <summary></summary>
		PlainText,
		/// <summary></summary>
		HTML,
		/// <summary>
		/// HTML with images included as embedded URIs.
		/// </summary>
		HTMLwDataURIs,
		/// <summary>
		/// MIME HTML
		/// </summary>
		MHTML,
		/// <summary></summary>
		XML 
	}

	/// <summary>
	/// The type of HotDocs template.
	/// </summary>
	public enum TemplateType
	{
		/// <summary></summary>
		Unknown,
		/// <summary>
		/// Templates that only include an interview. (.cmp files.) No document is assembled from an interview-only template.
		/// </summary>
		InterviewOnly,
		/// <summary></summary>
		WordDOCX,
		/// <summary></summary>
		WordRTF,
		/// <summary></summary>
		WordPerfect,
		/// <summary></summary>
		HotDocsHFT,
		/// <summary></summary>
		HotDocsPDF,
		/// <summary></summary>
		PlainText 
	}

	/// <summary>
	/// This class represents a template that is managed by the host application, and
	/// (optionally) some assembly parameters (as specified by switches) for that template.
	/// The location of the template is defined by Template.Location.
	/// </summary>
	public class Template
	{
		private string _title = null;//A cached title when non-null.

		//Constructors
		/// <summary>
		/// Construct a Template object.
		/// </summary>
		/// <param name="fileName">The template file name.</param>
		/// <param name="location">The location of the template.</param>
		/// <include file="../Shared/Help.xml" path="Help/string/param[@name='switches']"></include>
		/// <param name="key">Uniquely identifies the template. A key is necessary for templates without a fixed file name, such as when stored in a DMS or other database. An empty string may be used for templates with a fixed file name.</param>
		public Template(string fileName, TemplateLocation location, string switches = "", string key = "")
		{
			if (fileName == null || location == null)
				throw new Exception("Invalid parameter.");

			if (switches == null) switches = "";
			if (key == null) key = "";

			FileName = fileName;
			Location = location;
			Switches = switches;
			Key = key;
		}
		/// <summary>
		/// Construct a Template object for the main template in a package.
		/// </summary>
		/// <param name="location">The template location as a package location.</param>
		/// <include file="../Shared/Help.xml" path="Help/string/param[@name='switches']"></include>
		/// <param name="key">Uniquely identifies the template. A key is necessary for templates without a fixed file name, such as when stored in a DMS or other database. An empty string may be used for templates with a fixed file name.</param>
		public Template(PackageTemplateLocation location, string switches = "", string key = "")
		{
			if (location == null || switches == null || key == null)
				throw new Exception("Invalid parameter.");

			HotDocs.Sdk.TemplateInfo ti = location.GetPackageManifest().MainTemplate;
			FileName = ti.FileName;
			Location = location;
			Switches = switches;
			Key = key;
		}
		/// <summary>
		/// Returns a locator string to recreate the template object at a later time.
		/// Use the Locate method to recreate the object.
		/// </summary>
		/// <returns></returns>
		public string CreateLocator()
		{
			string locator = FileName + "|" + Switches + "|" + Key +"|" + Location.CreateLocator();
			return Util.EncryptString(locator);
		}
		/// <summary>
		/// Returns a Template created from a locator string generated by CreateLocator.
		/// </summary>
		/// <param name="locator">A locator string provided by CreateLocator.</param>
		/// <returns></returns>
		public static Template Locate(string locator)
		{
			if (locator == null)
				throw new Exception("Invalid parameter.");

			string decryptedLocator = Util.DecryptString(locator);
			string[] tokens = decryptedLocator.Split('|');
			if (tokens.Length != 4)
				throw new Exception("Invalid template locator.");

			string fileName = tokens[0];
			string switches = tokens[1];
			string key = tokens[2];
			string locationLocator = tokens[3];

			Template template = new Template(fileName, TemplateLocation.Locate(locationLocator), switches);
			template.Key = key;
			template.UpdateFileName();
			return template;
		}

		//Public methods.
		/// <summary>
		/// Returns the template title as defined in the template's manifest.
		/// </summary>
		/// <returns></returns>
		public string GetTitle()
		{
			if (_title == null)
			{
				try
				{
					TemplateManifest manifest = GetManifest(ManifestParseFlags.ParseTemplateInfo);
					_title = manifest.Title;
				}
				catch (Exception)
				{
					_title = "";
				}
			}
			return _title;
		}
		/// <summary>
		/// Gets the template manifest for this template. Can optionally parse an entire template manifest spanning tree.
		/// See <see cref="ManifestParseFlags"/> for details.
		/// </summary>
		/// <param name="parseFlags">See <see cref="ManifestParseFlags"/>.</param>
		/// <returns></returns>
		public TemplateManifest GetManifest(ManifestParseFlags parseFlags)
		{
			return TemplateManifest.ParseManifest(GetFullPath(), parseFlags);
		}
		/// <summary>
		/// Request that the Template.Location update the file name as needed.
		/// </summary>
		/// <remarks>Call this method to request that the file name</remarks>
		public void UpdateFileName()
		{
			string updatedFileName;
			if (Location.GetUpdatedFileName(this, out updatedFileName))
			{
				if (updatedFileName == null || updatedFileName == "")
					throw new Exception("Invalid file name.");
				FileName = updatedFileName;
			}
		}
		/// <summary>
		/// Returns the full path (based on the directory specified by Location.GetTemplateDirectory) of the template.
		/// </summary>
		/// <returns></returns>
		public string GetFullPath()
		{
			if (Location == null)
				throw new Exception("No location has been specified.");
			return Path.Combine(Location.GetTemplateDirectory(), FileName);
		}

		//Public properties.
		/// <summary>
		/// The file name (including extension) of the template. No path information is included.
		/// </summary>
		public string FileName { get; private set; }
		/// <summary>
		/// Defines the location of the template.
		/// </summary>
		public TemplateLocation Location { get; private set; }
		/// <summary>
		/// Command line switches that may be applicable when assembling the template, as provided to the ASSEMBLE instruction.
		/// </summary>
		public string Switches { get; set; }
		/// <summary>
		/// A key identifying the template. When using a template management scheme where the template file itself is temporary
		/// (such as with a DMS) set this key to help HotDocs Server to keep track of which server files are for which template.
		/// If not empty, this key is also used internally by HotDocs Server for caching purposes.
		/// </summary>
		public string Key { get; private set; }
		/// <summary>
		/// If the host app wants to know, this property does what's necessary to
		/// tell you the type of template you're dealing with.
		/// </summary>
		public TemplateType TemplateType
		{
			get
			{
				switch (Path.GetExtension(FileName).ToLowerInvariant())
				{
					case ".cmp": return TemplateType.InterviewOnly;
					case ".docx": return TemplateType.WordDOCX;
					case ".rtf": return TemplateType.WordRTF;
					case ".hpt": return TemplateType.HotDocsPDF;
					case ".hft": return TemplateType.HotDocsHFT;
					case ".wpt": return TemplateType.WordPerfect;
					case ".ttx": return TemplateType.PlainText;
					default: return TemplateType.Unknown;
				}
			}
		}
		/// <summary>
		/// Parses command-line switches to inform the host app whether or not
		/// an interview should be displayed for this template.
		/// </summary>
		public bool HasInterview
		{
			get
			{
				string switches = String.IsNullOrEmpty(Switches) ? String.Empty : Switches.ToLower();
				return !switches.Contains("/nw") && !switches.Contains("/naw") && !switches.Contains("/ni");
			}
		}
		/// <summary>
		/// Based on TemplateType, tells the host app whether this type of template
		/// generates a document or not (although regardless, ALL template types
		/// need to be assembled in order to participate in assembly queues)
		/// </summary>
		public bool GeneratesDocument
		{
			get
			{
				TemplateType type = TemplateType;
				return type != TemplateType.InterviewOnly && type != TemplateType.Unknown;
			}
		}
		/// <summary>
		/// Based on the template file extension, get the document type native to the template type.
		/// </summary>
		public DocumentType NativeDocumentType
		{
			get
			{
				string ext = Path.GetExtension(FileName);
				if (string.Compare(ext, ".docx", true) == 0)
					return DocumentType.WordDOCX;
				if (string.Compare(ext, ".rtf", true) == 0)
					return DocumentType.WordRTF;
				if (string.Compare(ext, ".hpt", true) == 0)
					return DocumentType.PDF;
				if (string.Compare(ext, ".hft", true) == 0)
					return DocumentType.HFD;
				if (string.Compare(ext, ".wpt", true) == 0)
					return DocumentType.WordPerfect;
				if (string.Compare(ext, ".ttx", true) == 0)
					return DocumentType.PlainText;
				return DocumentType.Unknown;
			}
		}
	}
}
