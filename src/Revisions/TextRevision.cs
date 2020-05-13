using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.SVN.Revisions
{
	public class TextRevision : Revision
	{
		private string _word;

		internal TextRevision(string word)
		{
			_word = word;
		}

		protected override string GetRevisionString()
		{
			return _word;
		}

	}
}
