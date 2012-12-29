﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*
* CarrotCake CMS
* http://www.carrotware.com/
*
* Copyright 2011, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: October 2011
*/


namespace Carrotware.CMS.Core {

	public class WordPressPost {
		public enum WPPostType {
			Unknown,
			Attachment,
			BlogPost,
			Page
		}

		public WordPressPost() { }

		public string PostTitle { get; set; }
		public string PostName { get; set; }
		public string PostContent { get; set; }
		public DateTime PostDateUTC { get; set; }
		public bool IsPublished { get; set; }

		public int PostOrder { get; set; }
		public int PostID { get; set; }
		public int ParentPostID { get; set; }

		public WPPostType PostType { get; set; }

		public List<string> Categories { get; set; }
		public List<string> Tags { get; set; }

		public Guid ImportRootID { get; set; }
		public Guid? ImportParentRootID { get; set; }
		public string ImportFileSlug { get; set; }
		public string ImportFileName { get; set; }


		public override string ToString() {
			return PostTitle + " : " + PostType.ToString() + " , #" + PostID;
		}

	}
}