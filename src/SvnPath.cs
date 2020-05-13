using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.SVN.Revisions;

namespace Common.SVN
{
	public abstract class SvnPath
	{
		public string Value { get; private set; }
		public abstract string AsArgument(string command, Revision revision);

		public static SvnPath CreateWCPath(string path)
		{
			return new WCPath() { Value = (path) };
		}

		public static SvnPath CreateSvnPath(string path)
		{
			return new RepoPath() { Value = NormalizePath(path) };
		}

		public static string NormalizePath(string path)
		{
			return (new Uri(path.Replace('\\', '/'))).ToString();
		}

	}
}
