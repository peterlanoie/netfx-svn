using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.SVN.Revisions
{
	public class NumericRevision : Revision
	{
		public long Number { get; set; }

		internal NumericRevision(long number)
		{
			Number = number;
		}

		protected override string GetRevisionString()
		{
			return Number.ToString();
		}
	}
}
