using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.SVN
{
	[Serializable]
	public class SvnException : Exception
	{
		public SvnException() { }
		public SvnException(string message) : base(message) { }
		public SvnException(string message, Exception inner) : base(message, inner) { }
		protected SvnException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
