using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.SVN.Revisions;

namespace Common.SVN
{
	public class SvnLogEntry
	{
		public Revision Revision { get; set; }
		public string Author { get; set; }
		public DateTime Date { get; set; }
		public List<SvnLogEntryPath> Paths { get; set; }
		public string Message { get; set; }

		public SvnLogEntry()
		{
			Paths = new List<SvnLogEntryPath>();
		}

		public override string ToString()
		{
			return string.Format("{0}|{1}|{2}|{3}", Revision, Date, Author, Message);
		}
	}
}
