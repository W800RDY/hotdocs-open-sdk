﻿/* Copyright (c) 2013, HotDocs Limited
   Use, modification and redistribution of this source is subject
   to the New BSD License as set out in LICENSE.TXT. */

//TODO: Add XML comments where missing.
//TODO: Add method parameter validation.
//TODO: Add appropriate unit tests.

using System.IO;

namespace HotDocs.Sdk
{
	/// <summary>
	/// 
	/// </summary>
	public class PathTemplateLocation : TemplateLocation
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="templateDir"></param>
		public PathTemplateLocation(string templateDir)
		{
			_templateDir = templateDir;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override TemplateLocation Duplicate()
		{
			return new PathTemplateLocation(_templateDir);
		}

		//TODO: Allow for readonly files.
		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public override Stream GetFile(string fileName)
		{
			string filePath = Path.Combine(GetTemplateDirectory(), fileName);
			return new FileStream(filePath, FileMode.Open);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string GetTemplateDirectory()
		{
			return _templateDir;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected override string SerializeContent()
		{
			return _templateDir;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		protected override void DeserializeContent(string content)
		{
			_templateDir = content;
		}

		private string _templateDir;
	}
}
