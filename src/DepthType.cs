using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.SVN
{
	/// <summary>
	/// The depth that an operation will apply to.
	/// </summary>
	public enum DepthType
	{
		/// <summary>
		/// Default depth indicating no choice.
		/// </summary>
		NotSet,

		/// <summary>
		/// Just this item (really only useful for directory operations)
		/// </summary>
		Empty,

		/// <summary>
		/// Just the files in the operation directory. 
		/// </summary>
		Files,

		/// <summary>
		/// All immediate children (files and directories) of the operation directory.
		/// </summary>
		Immediates,

		/// <summary>
		/// All child items at infinite depths.
		/// </summary>
		Infinity,
	}
}
