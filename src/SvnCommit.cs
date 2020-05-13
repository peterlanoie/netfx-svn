using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.SVN
{
	public class SvnCommit
	{
		public int Revision { get; set; }

		public string Author { get; set; }

		public DateTime Date { get; set; }
	}
}
