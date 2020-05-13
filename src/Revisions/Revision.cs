using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.SVN.Revisions
{
	public abstract class Revision
	{
		protected abstract string GetRevisionString();

		/// <summary>
		/// Returns the appropriate string for the revision: number, keyword or formatted date.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return GetRevisionString();
		}

		public readonly static TextRevision HEAD = new TextRevision("HEAD");

		public readonly static TextRevision BASE = new TextRevision("BASE");

		public readonly static TextRevision COMMITTED = new TextRevision("COMMITTED");

		public readonly static TextRevision PREV = new TextRevision("PREV");

		public readonly static TextRevision Empty = new TextRevision(null);

		public static Revision Make(long revision)
		{
			return new NumericRevision(revision);
		}

		public static Revision Make(string revision)
		{
			if(string.IsNullOrEmpty(revision))
			{
				return HEAD;
			}
			if(revision.Contains(':'))
			{
				string[] parts = revision.Split(':');
				return new RevisionRange(Revision.Make(parts[0]), Revision.Make(parts[1]));
			}
			switch(revision.ToUpper())
			{
				case "HEAD": return HEAD;
				case "BASE": return BASE;
				case "COMMITTED": return COMMITTED;
				case "PREV": return PREV;
				default:
					long lngRev;
					if(long.TryParse(revision, out lngRev))
					{
						if(lngRev > 0)
						{
							return new NumericRevision(lngRev);
						}
					}
					return HEAD;
			}
		}
	}
}
