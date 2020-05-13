using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.SVN.Revisions
{
	public class DateRevision : Revision
	{
		private DateTime _date;

		internal DateRevision(DateTime date)
		{
			_date = date;
		}

		protected override string GetRevisionString()
		{
			return string.Format("{{0}}", _date);
		}
	}
}
