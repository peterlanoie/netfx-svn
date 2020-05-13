using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using Common.Diagnostics;
using Common.SVN.Revisions;

namespace Common.SVN
{
	public class SvnHelper
	{
		private SvnCommand _command;
		private SvnCommandOptions _defaultOptions;

		public event EventHandler<EventArgs<string>> ErrorMessage;
		public event EventHandler<EventArgs<string>> OutputMessage;
		public event EventHandler<EventArgs<string>> VerboseMessage;
		public event EventHandler<EventArgs<string>> DebugMessage;

		public string WorkingDirectory
		{
			get { return _defaultOptions.WorkingDirectory; }
			set { _defaultOptions.WorkingDirectory = value; }
		}

		public SvnHelper()
			: this(new SvnCommandOptions())
		{
		}

		public SvnHelper(string username, string password)
			: this(new SvnCommandOptions() { Username = username, Password = password })
		{
		}

		private SvnHelper(SvnCommandOptions defaultOptions)
		{
			_defaultOptions = defaultOptions;
			_command = new SvnCommand();
			_command.ErrorMessage += new EventHandler<EventArgs<string>>(_command_ErrorMessage);
			_command.OutputMessage += new EventHandler<EventArgs<string>>(_command_OutputMessage);
			_command.DebugMessage += new EventHandler<EventArgs<string>>(_command_DebugMessage);
		}

		void _command_DebugMessage(object sender, EventArgs<string> e)
		{
			DoDebugMessage(e.Data);
		}

		void _command_OutputMessage(object sender, EventArgs<string> e)
		{
			if (OutputMessage != null)
			{
				OutputMessage(this, e);
			}
		}

		void _command_ErrorMessage(object sender, EventArgs<string> e)
		{
			if (ErrorMessage != null)
			{
				ErrorMessage(this, e);
			}
		}

		void DoDebugMessage(string messageFormat, params object[] args)
		{
			if (DebugMessage != null)
			{
				DebugMessage(this, new EventArgs<string>(string.Format(messageFormat, args)));
			}
		}

		void DoVerboseMessage(string messageFormat, params object[] args)
		{
			if (VerboseMessage != null)
			{
				VerboseMessage(this, new EventArgs<string>(string.Format(messageFormat, args)));
			}
		}

		/// <summary>
		/// Gets the Subversion repository revision number for the specified <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path to check</param>
		/// <returns>The revision number of the specified repository</returns>
		public long GetRevisionNumber(SvnPath path)
		{
			return GetRevisionNumber(path, Revision.HEAD);
		}

		/// <summary>
		/// Gets the Subversion repository revision number for the specified <paramref name="path"/> and <paramref name="revision"/>.
		/// </summary>
		/// <param name="path">The path to check</param>
		/// <returns>The revision number of the specified repository</returns>
		public long GetRevisionNumber(SvnPath path, Revision revision)
		{
			return long.Parse(GetInfoValue(path, "/info/entry/@revision", revision));
		}

		/// <summary>
		/// Gets the url of the root of the repository <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path to check</param>
		/// <returns>The root url of the repository</returns>
		public string GetRepositoryRoot(SvnPath path)
		{
			return GetInfoValue(path, "/info/entry/repository/root");
		}

		/// <summary>
		/// Gets the url of the root of the repository <paramref name="path"/> at <paramref name="revision"/>.
		/// </summary>
		/// <param name="path">The path to check</param>
		/// <returns>The root url of the repository</returns>
		public string GetRepositoryRoot(SvnPath path, Revision revision)
		{
			return GetInfoValue(path, "/info/entry/repository/root", revision);
		}

		/// <summary>
		/// Gets the url of the specified repository path
		/// </summary>
		/// <param name="path">The path to check</param>
		/// <returns>The url of the repository</returns>
		public string GetRepositoryUrl(SvnPath path)
		{
			return GetInfoValue(path, "/info/entry/url");
		}

		/// <summary>
		/// Gets the name of the person who made the last change in the repository
		/// </summary>
		/// <param name="path">The path to check</param>
		/// <returns>The name of the last committer</returns>
		public string GetLastChangedAuthor(SvnPath path)
		{
			return GetInfoValue(path, "/info/entry/commit/author");
		}

		/// <summary>
		/// Gets the number of the latest revision in which the <paramref name="path"/> has changed.
		/// </summary>
		/// <param name="path">The path to check</param>
		/// <returns>The latest revision number in which the file or directory specified in <paramref name="path"/> has changed.</returns>
		public int GetLastChangedRev(SvnPath path)
		{
			return int.Parse(GetInfoValue(path, "/info/entry/commit/@revision"));
		}

		/// <summary>
		/// Gets the SVN status of the supplied working copy <paramref name="path"/>.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string GetPathStatus(string path)
		{
			return GetStatusValue(SvnPath.CreateWCPath(path), "/status/target/entry/wc-status/@item");
		}

		/// <summary>
		/// Checks the path status for any items that are changes (add, modify or delete).
		/// </summary>
		/// <param name="path">Working copy path to check.</param>
		/// <returns></returns>
		public bool PathHasChanges(string path)
		{
			return PathHasChanges(path, false);
		}

		/// <summary>
		/// Checks the path status for any items that have changes (add, modify or delete)
		/// with the option of including unversioned files.
		/// </summary>
		/// <param name="path">Working copy path to check.</param>
		/// <returns></returns>
		public bool PathHasChanges(string path, bool includeUnversioned)
		{
			XmlDocument objXml = new XmlDocument();
			XmlNodeList lstNodes;
			SvnCommandOptions options = _defaultOptions.Clone();
			options.Paths.Add(SvnPath.CreateWCPath(path));
			objXml = _command.GetXml("status", options);
			string strFilter = "@item='deleted' or @item='added' or @item='modified' or @item='missing'";
			if (includeUnversioned)
			{
				strFilter = string.Concat(strFilter, " or @item='unversioned'");
			}
			lstNodes = objXml.SelectNodes(string.Format("//wc-status[{0}]", strFilter));
			return lstNodes.Count > 0;
		}

		private string GetInfoValue(SvnPath path, string xpath)
		{
			return GetInfoValue(path, xpath, Revision.HEAD);
		}

		private string GetInfoValue(SvnPath path, string xpath, Revision revision)
		{
			return GetXmlValue("info", path, xpath, revision);
		}

		private string GetStatusValue(SvnPath path, string xpath)
		{
			return GetXmlValue("status", path, xpath);
		}

		private string GetXmlValue(string command, SvnPath path, string xpath)
		{
			return GetXmlValue(command, path, xpath, Revision.HEAD);
		}

		private string GetXmlValue(string command, SvnPath path, string xpath, Revision revision)
		{
			XmlDocument objXml = new XmlDocument();
			XmlNode objNode;
			string strValue = string.Empty;

			SvnCommandOptions options = _defaultOptions.Clone();
			options.Revision = revision;
			options.Paths.Add(path);

			objXml = _command.GetXml(command, options);
			objNode = objXml.SelectSingleNode(xpath);
			if (objNode != null)
			{
				switch (objNode.NodeType)
				{
					case XmlNodeType.Attribute:
						strValue = objNode.Value;
						break;
					default:
						strValue = objNode.InnerText;
						break;
				}
			}
			else
			{
				throw new Exception(string.Format("Value for xpath '{0}' in SVN {1} output not found.", xpath, command));
			}

			return strValue;
		}

		/// <summary>
		/// Reverts a working copy specified by <paramref name="path"/> to its original state.
		/// </summary>
		/// <param name="path">Working copy path.</param>
		/// <param name="depth">Affective depth of the command.</param>
		public void Revert(string path, DepthType depth)
		{
			SvnCommandOptions options = _defaultOptions.Clone();
			options.Depth = depth;
			options.Paths.Add(SvnPath.CreateWCPath(path));
			_command.Run("revert", options);
		}

		/// <summary>
		/// Reverts a working copy specified by <paramref name="path"/> to its original.
		/// </summary>
		/// <param name="path">Working copy path.</param>
		public void Revert(string path)
		{
			Revert(path, DepthType.NotSet);
		}

		/// <summary>
		/// Updates the working copy specified in <paramref name="path"/> to the latest (HEAD) revision.
		/// </summary>
		/// <param name="path">Working copy path.</param>
		public void Update(string path)
		{
			Update(path, Revision.HEAD);
		}

		/// <summary>
		/// Updates the working copy specified in <paramref name="path"/> to the specified numeric <paramref name="revision"/>.
		/// </summary>
		/// <param name="path">Working copy path to update.</param>
		/// <param name="revision">Numeric revision to which the working path will be updated.</param>
		public void Update(string path, long revision)
		{
			Update(path, RevisionFactory.Create(revision));
		}

		/// <summary>
		/// Updates the working copy specified in <paramref name="path"/> to the revision at the start of the specified <paramref name="date"/>.
		/// </summary>
		/// <param name="path">Working copy path to update.</param>
		/// <param name="revision">Numeric revision to which the working path will be updated.</param>
		public void Update(string path, DateTime date)
		{
			Update(path, RevisionFactory.Create(date));
		}

		/// <summary>
		/// Updates the working copy specified in <paramref name="path"/> to the specified <paramref name="revision"/>.
		/// Acceptable values are: HEAD, BASE, COMMITTED & PREV a validly formatted date or numeric value.
		/// </summary>
		/// <param name="path">Working copy path to update.</param>
		/// <param name="revision">Keyword revision to which the working path will be updated.</param>
		public void Update(string path, string revision)
		{
			Update(path, RevisionFactory.Create(revision));
		}

		/// <summary>
		/// Updates the working copy specified in <paramref name="path"/> to the specified numeric <paramref name="revision"/>.
		/// </summary>
		/// <param name="path">Working copy path to update.</param>
		/// <param name="revision">Numeric revision to which the working path will be updated.</param>
		public void Update(string path, Revision revision)
		{
			Update(path, revision, DepthType.Infinity);
		}

		/// <summary>
		/// Updates the working copy specified in <paramref name="path"/> to the specified numeric <paramref name="revision"/>.
		/// </summary>
		/// <param name="path">Working copy path to update.</param>
		/// <param name="revision">Numeric revision to which the working path will be updated.</param>
		/// <param name="depth">The depth to which the operation will apply.</param>
		public void Update(string path, Revision revision, DepthType depth)
		{
			SvnCommandOptions options = _defaultOptions.Clone();
			options.Revision = revision;
			options.Depth = depth;
			options.Paths.Add(SvnPath.CreateWCPath(path));
			_command.Run("update", options);
		}

		/// <summary>
		/// Checks out the <paramref name="uri"/> to the <paramref name="workingCopyPath"/>.
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="workingCopyPath"></param>
		public void CheckOut(string uri, string workingCopyPath, Revision revision)
		{
			SvnCommandOptions options = _defaultOptions.Clone();
			options.Revision = revision;
			options.Paths.Add(SvnPath.CreateSvnPath(uri));
			options.Paths.Add(SvnPath.CreateWCPath(workingCopyPath));
			_command.Run("checkout", options);
		}

		/// <summary>
		/// Exports the <paramref name="depth"/> of the <paramref name="revision" /> of the <paramref name="uri"/> to the <paramref name="localPath"/>.
		/// </summary>
		/// <param name="uri">URI to export.</param>
		/// <param name="revision">Repository revision to export.</param>
		/// <param name="depth">Depth of the export.</param>
		/// <param name="localPath">Local path to which the files will be exported.</param>
		/// <param name="force">Whether or not to force overwrite existing files. Required if target directory exists, even if empty.</param>
		public void Export(string uri, Revision revision, DepthType depth, string localPath, bool force)
		{
			SvnCommandOptions options = _defaultOptions.Clone();
			options.Revision = revision;
			options.Depth = depth;
			options.MiscArgs.Add(force ? "--force" : null);
			options.Paths.Add(SvnPath.CreateSvnPath(uri));
			options.Paths.Add(SvnPath.CreateWCPath(localPath));
			_command.Run("export", options);
		}

		/// <summary>
		/// Checks out the <paramref name="uri"/> to the <paramref name="workingCopyPath"/>.
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="workingCopyPath"></param>
		public void CheckOut(Uri uri, string workingCopyPath, Revision revision)
		{
			CheckOut(uri.ToString(), workingCopyPath, revision);
		}

		/// <summary>
		/// Schedules appropriate changes to working copy: adds unversioned files and deletes missing files.
		/// </summary>
		/// <param name="path">Working copy path to modify.</param>
		/// <param name="noIgnore">Whether not to ignore the default ignores.</param>
		public bool ScheduleChanges(string path, bool noIgnore)
		{
			XmlDocument xmlDoc;
			XmlNodeList xmlNodes;
			XmlNode objStatusNode;
			List<string> lstDeletes = new List<string>();
			List<string> lstAdds = new List<string>();
			string nodePath, nodeAction;
			int skip = 0, take = 10;

			DoDebugMessage("retrieving working copy status");
			SvnCommandOptions options = _defaultOptions;
			options.Paths.Add(SvnPath.CreateWCPath(path));

			xmlDoc = _command.GetXml("status", options);

			xmlNodes = xmlDoc.SelectNodes("//entry[wc-status/@item!='normal']");

			if (xmlNodes.Count > 0)
			{
				foreach (XmlNode node in xmlNodes)
				{
					objStatusNode = node.SelectSingleNode("wc-status");
					nodeAction = objStatusNode.Attributes["item"].Value;
					nodePath = node.Attributes["path"].Value;
					switch (nodeAction)
					{
						case "missing":
							DoVerboseMessage("found missing item: {0}", nodePath);
							lstDeletes.Add(nodePath);
							break;
						case "unversioned":
							DoVerboseMessage("found unversioned item: {0}", nodePath);
							lstAdds.Add(nodePath);
							break;
						default:
							DoDebugMessage("ignoring action '{0}' on path '{1}'", nodeAction, nodePath);
							break;
					}
				}

				if (lstAdds.Count > 0)
				{
					skip = 0;
					DoDebugMessage("scheduling add changes in chunks of '{0}'", take);
					while (lstAdds.Count > skip)
					{
						DoDebugMessage("scheduling the next {0} adds", take);
						ScheduleChange("add", noIgnore, lstAdds.Skip(skip).Take(take).ToArray());
						skip += take;
					}
				}
				if (lstDeletes.Count > 0)
				{
					skip = 0;
					DoDebugMessage("scheduling delete changes in chunks of '{0}'", take);
					while (lstDeletes.Count > skip)
					{
						DoDebugMessage("scheduling the next {0} deletes", take + skip);
						ScheduleChange("delete", false, lstDeletes.Skip(skip).Take(take).ToArray());
						skip += take;
					}
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		private void ScheduleChange(string command, bool noignore, string[] paths)
		{
			SvnCommandOptions options = _defaultOptions.Clone();
			options.IgnoreStandardExcludes = !noignore;
			foreach (var path in paths)
			{
				options.Paths.Add(SvnPath.CreateWCPath(path));
			}
			_command.Run(command, options);
		}

		/// <summary>
		/// Commits a change to working copy <paramref name="path"/>.
		/// </summary>
		/// <param name="path"></param>
		public void Commit(string path, string comment)
		{
			Commit(new List<string>(new string[] { path }), comment);
		}

		/// <summary>
		/// Commits changes to working copy <paramref name="paths"/>.
		/// </summary>
		/// <param name="paths"></param>
		public void Commit(List<string> paths, string comment)
		{
			SvnCommandOptions options = _defaultOptions.Clone();
			options.Comment = comment;
			paths.ForEach(p => options.Paths.Add(SvnPath.CreateWCPath(p)));
			_command.Run("commit", options);
		}

		/// <summary>
		/// Returns whether or not the specified <paramref name="path"/> is a working copy.
		/// </summary>
		/// <param name="path">The local path to check.</param>
		/// <returns></returns>
		public bool IsWorkingCopy(string path)
		{
			try
			{
				SvnCommandOptions options = _defaultOptions.Clone();
				options.Paths.Add(SvnPath.CreateWCPath(path));
				options.Depth = DepthType.Empty;
				_command.GetXml("status", options);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a list of the immediate items in the HEAD revision of the specified <paramref name="path"/>.
		/// </summary>
		/// <param name="path">Path of which the contents will be listed.</param>
		/// <returns></returns>
		public List<SvnItem> List(SvnPath path)
		{
			return List(path, Revision.HEAD, DepthType.Immediates);
		}

		/// <summary>
		/// Gets a list of the immediate items in the specified <paramref name="revision"/> of the specified <paramref name="path"/>.
		/// </summary>
		/// <param name="path">Path of which the contents will be listed.</param>
		/// <param name="revision">Revision to list.</param>
		/// <returns></returns>
		public List<SvnItem> List(SvnPath path, Revision revision)
		{
			return List(path, revision, DepthType.Immediates);
		}

		/// <summary>
		/// Gets a list of items up to the defined <paramref name="depth"/> in the specified <paramref name="path"/>.
		/// </summary>
		/// <param name="path">Path of which the contents will be listed.</param>
		/// <param name="depth">Desired depth of the listings.</param>
		/// <returns></returns>
		public List<SvnItem> List(SvnPath path, DepthType depth)
		{
			return List(path, Revision.HEAD, depth);
		}

		/// <summary>
		/// Gets a list of items in the specified <paramref name="path"/>.
		/// </summary>
		/// <param name="path">Path of which the contents will be listed.</param>
		/// <param name="revision">Revision to list.</param>
		/// <param name="depth">Desired depth of the listings.</param>
		/// <returns></returns>
		public List<SvnItem> List(SvnPath path, Revision revision, DepthType depth)
		{
			List<SvnItem> result = new List<SvnItem>();
			SvnItem item = null;
			SvnCommandOptions options = _defaultOptions.Clone();
			options.Depth = depth;
			options.Revision = revision;
			options.Paths.Add(path);

			XmlReader xReader = _command.GetXmlReader("list", options);

			while (xReader.Read())
			{
				if (xReader.NodeType == XmlNodeType.EndElement)
				{
					continue;
				}
				switch (xReader.Name.ToLower())
				{
					case "entry":
						item = new SvnItem();
						result.Add(item);
						item.Type = GetItemType(xReader.GetAttribute("kind"));
						item.Commit = new SvnCommit();
						break;
					case "name":
						item.Name = xReader.ReadString();
						break;

					case "size":
						item.Size = xReader.ReadElementContentAsInt();
						break;

					case "commit":
						item.Commit.Revision = int.Parse(xReader.GetAttribute("revision"));
						break;
					case "author":
						item.Commit.Author = xReader.ReadString();
						break;

					case "date":
						item.Commit.Date = xReader.ReadElementContentAsDateTime();
						break;
				}
			}

			return result;
		}

		private SvnItemType GetItemType(string kind)
		{
			switch (kind.ToLower())
			{
				case "dir": return SvnItemType.Directory;
				case "file": return SvnItemType.File;
				default: return SvnItemType.Unknown;
			}
		}

		public List<SvnLogEntry> GetLog(SvnPath path)
		{
			return GetLog(path, Revision.HEAD, 100);
		}

		public List<SvnLogEntry> GetLog(SvnPath path, int limit)
		{
			return GetLog(path, Revision.HEAD, limit);
		}

		public List<SvnLogEntry> GetLog(SvnPath path, Revision startRevision)
		{
			return GetLog(path, startRevision, 100);
		}

		public List<SvnLogEntry> GetLog(SvnPath path, Revision startRevision, int limit)
		{
			XmlReader xReader;
			List<SvnLogEntry> result = new List<SvnLogEntry>();
			SvnLogEntry item = null;
			SvnLogEntryPath actionPath = null;
			SvnCommandOptions options;
			string temp;

			options = _defaultOptions.Clone();
			options.Paths.Add(path);

			options.Revision = new RevisionRange(startRevision, new NumericRevision(1));
			options.MiscArgs.Add(string.Concat("--limit ", limit));
			xReader = _command.GetXmlReader("log", options);

			while (xReader.Read())
			{
				if (xReader.NodeType == XmlNodeType.EndElement)
				{
					continue;
				}
				switch (xReader.Name.ToLower())
				{
					case "logentry":
						item = new SvnLogEntry();
						result.Add(item);
						item.Revision = new NumericRevision(long.Parse(xReader.GetAttribute("revision")));
						break;

					case "author":
						item.Author = xReader.ReadString();
						break;

					case "date":
						item.Date = xReader.ReadElementContentAsDateTime();
						break;

					case "path":
						actionPath = new SvnLogEntryPath();
						item.Paths.Add(actionPath);
						actionPath.Type = GetActionType(xReader.GetAttribute("action"));
						actionPath.CopyFromPath = xReader.GetAttribute("copyfrom-path");
						temp = xReader.GetAttribute("copyfrom-rev");
						if (temp != null)
						{
							actionPath.CopyFromRevision = Revision.Make(temp);
						}
						actionPath.Path = xReader.ReadString();
						break;

					case "msg":
						item.Message = xReader.ReadString();
						break;
				}
			}

			return result;
		}

		private SvnLogActionType GetActionType(string action)
		{
			switch (action.ToUpper())
			{
				case "A": return SvnLogActionType.Added;
				case "D": return SvnLogActionType.Deleted;
				case "M": return SvnLogActionType.Modified;
				default: return SvnLogActionType.Unknown;
			}
		}

	}
}