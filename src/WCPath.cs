using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.SVN
{
	internal class WCPath : SvnPath
	{
		public override string AsArgument(string command, Revisions.Revision revision)
		{
			return Value;
		}
	}
}
