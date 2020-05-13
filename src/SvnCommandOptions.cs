using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.SVN.Revisions;

namespace Common.SVN
{
	internal class SvnCommandOptions
	{
		public string Username { get; set; }
		public string Password { get; set; }

		/// <summary>
		/// The directory in which to execute the SVN command.
		/// </summary>
		public string WorkingDirectory { get; set; }

		/// <summary>
		/// Whether not to cache authorization info. Default is true (i.e. Auth info will not be cached.)
		/// </summary>
		public bool CacheAuthCredentials { get; set; }

		/// <summary>
		/// Whether not to ignore
		/// </summary>
		public bool IgnoreStandardExcludes { get; set; }

		/// <summary>
		/// The comment to use for a command.
		/// </summary>
		public string Comment { get; set; }

		/// <summary>
		/// The revision to use for the executing command.
		/// </summary>
		public Revision Revision { get; set; }

		public DepthType Depth { get; set; }

		public List<string> MiscArgs { get; private set; }

		public List<SvnPath> Paths { get; private set; }

		public SvnCommandOptions()
		{
			CacheAuthCredentials = false;
			IgnoreStandardExcludes = true;
			Revision = Revision.HEAD;
			Depth = DepthType.NotSet;
			MiscArgs = new List<string>();
			Paths = new List<SvnPath>();
		}

		#region ICloneable Members

		public SvnCommandOptions Clone()
		{
			SvnCommandOptions clone = new SvnCommandOptions();
			clone.CacheAuthCredentials = this.CacheAuthCredentials;
			clone.IgnoreStandardExcludes = this.IgnoreStandardExcludes;
			clone.Password = this.Password;
			clone.Username = this.Username;
			clone.WorkingDirectory = this.WorkingDirectory;
			return clone;
		}

		#endregion

	}
}
