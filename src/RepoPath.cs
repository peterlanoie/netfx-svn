using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.SVN
{
	internal class RepoPath : SvnPath
	{
		public override string AsArgument(string command, Revisions.Revision revision)
		{
			if((new string[] { "info", "export" }).Contains(command))
			{
				return string.Format("{0}@{1}", Value, revision.ToString());
			}
			else
			{
				return Value;
			}
		}
	}
}
