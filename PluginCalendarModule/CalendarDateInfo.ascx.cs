﻿using System;
using System.Collections.Generic;
using System.Linq;
using Carrotware.CMS.Interface;

namespace Carrotware.CMS.UI.Plugins.CalendarModule {

	public partial class CalendarDateInfo : WidgetParmDataUserControl {
		public DateTime theEventDate = DateTime.Now.Date;

		protected void Page_Load(object sender, EventArgs e) {
			if (!String.IsNullOrEmpty(Request.QueryString["calendardate"])) {
				theEventDate = Convert.ToDateTime(ParmParser.GetStringParameterFromQuery("calendardate"));
			}

			if (!IsPostBack) {
				SetCalendar();
			}
		}

		protected void SetCalendar() {
			using (dbCalendarDataContext db = dbCalendarDataContext.GetDataContext()) {
				var lst = (from c in db.tblCalendars
						   where c.EventDate == theEventDate
							&& c.IsActive == true
							&& c.SiteID == this.SiteID
						   orderby c.EventDate
						   select c).ToList();

				rEvents.DataSource = lst;
				rEvents.DataBind();
			}
		}
	}
}