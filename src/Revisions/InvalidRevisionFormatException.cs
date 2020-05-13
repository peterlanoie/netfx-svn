using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.SVN.Revisions
{
	[Serializable]
	public class InvalidRevisionFormatException : Exception
	{
		public InvalidRevisionFormatException() { }
		public InvalidRevisionFormatException(string rev) : base(GetMessage(rev)) { }

		private static string GetMessage(string rev)
		{
			return string.Format("Revision value '{0}' can not be parsed to an acceptable format.", rev);
		}
	}
}
