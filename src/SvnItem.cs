using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.SVN
{
	//<entry kind="file">
	//    <name>cleanvssfiles.bat</name>
	//    <size>228</size>
	//    <commit revision="498">
	//        <author>admin</author>
	//        <date>2009-05-08T16:39:35.641375Z</date>
	//    </commit>
	//</entry>

	public enum SvnItemType
	{
		Unknown,
		Directory,
		File,
	}

	public class SvnItem
	{
		public SvnItemType Type { get; set; }
		
		public int Size { get; set; }

		public List<SvnItem> ChildItems { get; set; }

		public SvnCommit Commit { get; set; }

		public string Name { get; set; }

		public bool HasChildren
		{
			get { return ChildItems.Count > 0; }
		}

		public SvnItem()
		{
			ChildItems = new List<SvnItem>();
		}

		public override string ToString()
		{
			return string.Format("{0}|{1}|{2}|r{3} @{4} by {5}", Name, Type, Size, Commit.Revision, Commit.Date, Commit.Author);
		}

	}
}
