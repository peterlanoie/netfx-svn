using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.SVN.Revisions
{
	public class RevisionFactory
	{

		public static Revision Create(DateTime rev)
		{
			return new DateRevision(rev);
		}

		public static Revision Create(long rev)
		{
			return new NumericRevision(rev);
		}

		public static Revision Create(string rev)
		{
			DateTime date;
			long number;
			string strRev;

			if(rev == null)
			{
				return Revision.HEAD;
			}
			strRev = rev.Trim();

			if(DateTime.TryParse(strRev, out date))
			{
				return Create(date);
			}

			if(long.TryParse(strRev, out number))
			{
				return Create(number);
			}

			switch(strRev.ToUpper())
			{
				case "BASE": return Revision.BASE;
				case "COMMITTED": return Revision.COMMITTED;
				case "":
				case "HEAD": return Revision.HEAD;
				case "PREV": return Revision.PREV;
				default:
					throw new InvalidRevisionFormatException(rev);
			}
		}

	}
}
