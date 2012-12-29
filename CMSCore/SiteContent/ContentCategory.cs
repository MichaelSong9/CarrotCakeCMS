﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carrotware.CMS.Data;
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
	public class ContentCategory : IDisposable, IContentMetaInfo {

		private CarrotCMSDataContext db = CarrotCMSDataContext.GetDataContext();
		//private CarrotCMSDataContext db = CompiledQueries.dbConn;

		public ContentCategory() { }

		public Guid ContentCategoryID { get; set; }
		public Guid SiteID { get; set; }
		public string CategoryText { get; set; }
		public string CategorySlug { get; set; }
		public string CategoryURL { get; set; }
		public int? UseCount { get; set; }

		#region IDisposable Members

		public void Dispose() {
			if (db != null) {
				db.Dispose();
			}
		}

		#endregion

		public override bool Equals(Object obj) {
			//Check for null and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) return false;
			if (obj is ContentCategory) {
				ContentCategory p = (ContentCategory)obj;
				return (this.SiteID == p.SiteID
						&& this.CategorySlug.ToLower() == p.CategorySlug.ToLower());
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return CategorySlug.GetHashCode() ^ SiteID.GetHashCode();
		}

		internal static ContentCategory CreateCategory(vw_carrot_CategoryCounted c) {
			ContentCategory cc = null;
			if (c != null) {
				cc = new ContentCategory();
				cc.ContentCategoryID = c.ContentCategoryID;
				cc.SiteID = c.SiteID;
				cc.CategorySlug = c.CategorySlug;
				cc.CategoryText = c.CategoryText;
				cc.UseCount = c.UseCount;
			}

			return cc;
		}

		internal static ContentCategory CreateCategory(vw_carrot_CategoryURL c) {
			ContentCategory cc = null;
			if (c != null) {
				cc = new ContentCategory();
				cc.ContentCategoryID = c.ContentCategoryID;
				cc.SiteID = c.SiteID;
				cc.CategoryURL = c.CategoryUrl;
				cc.CategoryText = c.CategoryText;
				cc.UseCount = c.UseCount;
			}

			return cc;
		}


		internal static ContentCategory CreateCategory(carrot_ContentCategory c) {
			ContentCategory cc = null;
			if (c != null) {
				cc = new ContentCategory();
				cc.ContentCategoryID = c.ContentCategoryID;
				cc.SiteID = c.SiteID;
				cc.CategorySlug = c.CategorySlug;
				cc.CategoryText = c.CategoryText;
			}

			return cc;
		}

		public static ContentCategory Get(Guid CategoryID) {
			ContentCategory _item = null;
			using (CarrotCMSDataContext _db = CarrotCMSDataContext.GetDataContext()) {
				carrot_ContentCategory query = CompiledQueries.cqGetContentCategoryByID(_db, CategoryID);
				_item = CreateCategory(query);
			}

			return _item;
		}


		public static int GetSimilar(Guid SiteID, Guid CategoryID, string categorySlug) {

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.GetDataContext()) {
				IQueryable<carrot_ContentCategory> query = CompiledQueries.cqGetContentCategoryNoMatch(_db, SiteID, CategoryID, categorySlug);

				return query.Count();
			}
		}


		public static List<ContentCategory> BuildCategoryList(Guid rootContentID) {

			List<ContentCategory> _types = null;

			using (CarrotCMSDataContext _db = CarrotCMSDataContext.GetDataContext()) {
				IQueryable<carrot_ContentCategory> query = CompiledQueries.cqGetContentCategoryByContentID(_db, rootContentID);

				_types = (from d in query.ToList()
						  select CreateCategory(d)).ToList();
			}

			return _types;
		}

		public void Save() {
			bool bNew = false;
			carrot_ContentCategory s = CompiledQueries.cqGetContentCategoryByID(db, this.ContentCategoryID);

			if (s == null) {
				s = new carrot_ContentCategory();
				s.ContentCategoryID = Guid.NewGuid();
				s.SiteID = this.SiteID;
				bNew = true;
			}

			s.CategorySlug = ContentPageHelper.ScrubSlug(this.CategorySlug);
			s.CategoryText = this.CategoryText;

			if (bNew) {
				db.carrot_ContentCategories.InsertOnSubmit(s);
			}

			this.ContentCategoryID = s.ContentCategoryID;

			db.SubmitChanges();
		}


		#region IContentMetaInfo Members

		public void SetValue(Guid ContentMetaInfoID) {
			this.ContentCategoryID = ContentMetaInfoID;
		}

		public Guid ContentMetaInfoID {
			get { return this.ContentCategoryID; }
		}

		public string MetaInfoText {
			get { return this.CategoryText; }
		}

		public string MetaInfoURL {
			get { return this.CategoryURL; }
		}

		public int MetaInfoCount {
			get { return this.UseCount == null ? 0 : Convert.ToInt32(this.UseCount); }
		}
		#endregion
	}



}