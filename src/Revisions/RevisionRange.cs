using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.SVN.Revisions
{
	public class RevisionRange : Revision
	{
		public Revision Start { get; set; }
		public Revision End { get; set; }

		public RevisionRange(Revision start, Revision end)
		{
			Start = start;
			End = end;
		}

		protected override string GetRevisionString()
		{
			if(Start == null)
			{
				return End.ToString();
			}
			if(End == null)
			{
				return Start.ToString();
			}
			return string.Format("{0}:{1}", Start.ToString(), End.ToString());
		}

	}
}
