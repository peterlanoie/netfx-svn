using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.SVN
{
	[Serializable]
	public class NotWorkingCopyException : Exception
	{
		public NotWorkingCopyException(): base("The specificed path is not a working copy.") { }
	}
}
