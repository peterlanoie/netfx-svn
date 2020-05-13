using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.SVN.Revisions;

namespace Common.SVN
{
	public class SvnLogEntryPath
	{
		public string Path { get; set; }
		public SvnLogActionType Type { get; set; }
		public string CopyFromPath { get; set; }
		public Revision CopyFromRevision { get; set; }
	}

	public enum SvnLogActionType
	{
		Added,

		Modified,

		Deleted,

		Unknown,
	}
}
