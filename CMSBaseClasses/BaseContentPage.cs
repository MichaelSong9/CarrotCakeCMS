﻿using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Profile;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Carrotware.CMS.Core;
using Carrotware.CMS.UI.Controls;
using Carrotware.Web.UI.Controls;
using Carrotware.CMS.Interface;
/*
* CarrotCake CMS
* http://carrotware.com/
*
* Copyright 2011, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: October 2011
*/
namespace Carrotware.CMS.UI.Base {
	public class BaseContentPage : BasePage {

		protected ContentContainer contCenter { get; set; }
		protected ContentContainer contRight { get; set; }
		protected ContentContainer contLeft { get; set; }

		protected SiteData theSite { get; set; }

		protected Guid guidContentID = Guid.Empty;

		protected ContentPage pageContents = new ContentPage();
		protected List<PageWidget> pageWidgets = new List<PageWidget>();


		protected string PageTitlePattern {
			get {
				string x = "{0} - {1}";
				try { x = System.Configuration.ConfigurationManager.AppSettings["PageTitlePattern"].ToString(); } catch { }
				return x;
			}
		}

		private int iCtrl = 0;

		private string CtrlId {
			get {
				return "WidgetID_" + (iCtrl++);
			}
		}


		protected void AssignContentZones(ContentContainer pageArea, ContentContainer pageSource) {

			pageArea.IsAdminMode = pageSource.IsAdminMode;

			pageArea.Text = pageSource.Text;

			pageArea.ZoneChar = pageSource.ZoneChar;

			pageArea.DatabaseKey = pageSource.DatabaseKey;
		}


		protected void LoadPageControls(Control page) {

			HtmlMeta metaGenerator = new HtmlMeta();
			metaGenerator.Name = "generator";
			metaGenerator.Content = string.Format("CarrotCake CMS {0}", CurrentDLLVersion);
			Page.Header.Controls.Add(metaGenerator);

			theSite = siteHelper.GetCurrentSite();

			if (theSite != null) {
				if (theSite.BlockIndex) {
					HtmlMeta metaNoCrawl = new HtmlMeta();
					metaNoCrawl.Name = "robots";
					metaNoCrawl.Content = "noindex,nofollow";
					Page.Header.Controls.Add(metaNoCrawl);
				}
			}

			string path = SiteData.CurrentScriptName.ToLower();

			pageContents = null;

			if (path.Length < 3) {
				if (SiteData.IsAdmin || SiteData.IsEditor) {
					pageContents = pageHelper.FindHome(SiteData.CurrentSiteID, null);
				} else {
					pageContents = pageHelper.FindHome(SiteData.CurrentSiteID, true);
				}
			} else {
				string pageName = path;
				if (SiteData.IsAdmin || SiteData.IsEditor) {
					pageContents = pageHelper.GetLatestContent(SiteData.CurrentSiteID, null, pageName);
				} else {
					pageContents = pageHelper.GetLatestContent(SiteData.CurrentSiteID, true, pageName);
				}
			}

			if (pageContents != null) {
				guidContentID = pageContents.Root_ContentID;
			}

			if (SiteData.AdvancedEditMode) {
				pageWidgets = widgetHelper.GetWidgets(guidContentID, null);
			} else {
				pageWidgets = widgetHelper.GetWidgets(guidContentID, true);
			}

			if (pageContents != null) {
				HtmlMeta metaDesc = new HtmlMeta();
				HtmlMeta metaKey = new HtmlMeta();

				metaDesc.Name = "description";
				metaKey.Name = "keywords";
				metaDesc.Content = string.IsNullOrEmpty(pageContents.MetaDescription) ? theSite.MetaDescription : pageContents.MetaDescription;
				metaKey.Content = string.IsNullOrEmpty(pageContents.MetaKeyword) ? theSite.MetaKeyword : pageContents.MetaKeyword;

				if (!string.IsNullOrEmpty(metaDesc.Content)) {
					Page.Header.Controls.Add(metaDesc);
				}
				if (!string.IsNullOrEmpty(metaKey.Content)) {
					Page.Header.Controls.Add(metaKey);
				}
			}

			if (SiteData.AdvancedEditMode) {
				if (cmsHelper.cmsAdminContent == null) {
					cmsHelper.cmsAdminContent = pageContents;
					cmsHelper.cmsAdminWidget = (from w in pageWidgets
												orderby w.WidgetOrder
												select w).ToList();
				} else {
					pageContents = cmsHelper.cmsAdminContent;
					pageWidgets = (from w in cmsHelper.cmsAdminWidget
								   orderby w.WidgetOrder
								   select w).ToList();
				}
			} else {
				if (SiteData.CurrentUserGuid != Guid.Empty) {
					cmsHelper.cmsAdminContent = null;
					cmsHelper.cmsAdminWidget = null;
				}
			}

			SetPageTitle(pageContents);

			contCenter = new ContentContainer();
			contLeft = new ContentContainer();
			contRight = new ContentContainer();

			if (pageContents != null) {

				DateTime dtModified = pageContents.EditDate;
				string strModifed = dtModified.ToString("r");
				Response.AppendHeader("Last-Modified", strModifed);
				Response.Cache.SetLastModified(dtModified);

				DateTime dtExpire = System.DateTime.Now.AddMinutes(1);
				Response.Cache.SetExpires(dtExpire);

				contCenter.Text = pageContents.PageText;
				contLeft.Text = pageContents.LeftPageText;
				contRight.Text = pageContents.RightPageText;

				contCenter.DatabaseKey = pageContents.Root_ContentID;
				contLeft.DatabaseKey = pageContents.Root_ContentID;
				contRight.DatabaseKey = pageContents.Root_ContentID;

				if (Page.User.Identity.IsAuthenticated) {

					Response.Cache.SetNoServerCaching();
					Response.Cache.SetCacheability(HttpCacheability.NoCache);
					dtExpire = DateTime.Now.AddMinutes(-10);
					Response.Cache.SetExpires(dtExpire);

					if (!SiteData.AdvancedEditMode) {

						if (SiteData.IsAdmin || SiteData.IsEditor) {

							Control editor = Page.LoadControl("~/Manage/ucEditNotifier.ascx");
							Page.Form.Controls.Add(editor);
						}

					} else {

						contCenter.IsAdminMode = true;
						contLeft.IsAdminMode = true;
						contRight.IsAdminMode = true;

						contCenter.ZoneChar = "c";
						contLeft.ZoneChar = "l";
						contRight.ZoneChar = "r";

						contCenter.Text = pageContents.PageText;
						contLeft.Text = pageContents.LeftPageText;
						contRight.Text = pageContents.RightPageText;

						Control editor = Page.LoadControl("~/Manage/ucAdvancedEdit.ascx");
						Page.Form.Controls.Add(editor);

						MarkWidgets(page, true);
					}
				}


				if (pageWidgets.Count > 0) {
					CMSConfigHelper cmsHelper = new CMSConfigHelper();

					//find each placeholder in use ONCE!
					List<KeyedControl> lstPlaceholders = (from d in pageWidgets
														  where d.Root_ContentID == pageContents.Root_ContentID
														  select new KeyedControl {
															  KeyName = d.PlaceholderName,
															  KeyControl = FindTheControl(d.PlaceholderName, page)
														  }).Distinct().ToList();

					List<PageWidget> lstWidget = (from d in pageWidgets
												  where d.Root_ContentID == pageContents.Root_ContentID
													&& d.IsWidgetActive == true
													&& d.IsWidgetPendingDelete == false
												  select d).ToList();

					foreach (PageWidget theWidget in lstWidget) {

						//WidgetContainer plcHolder = (WidgetContainer)FindTheControl(theWidget.PlaceholderName, page);
						WidgetContainer plcHolder = (WidgetContainer)(from d in lstPlaceholders
																	  where d.KeyName == theWidget.PlaceholderName
																	  select d.KeyControl).FirstOrDefault();

						if (plcHolder != null) {
							Control widget = new Control();

							if (theWidget.ControlPath.EndsWith(".ascx")) {
								if (File.Exists(Server.MapPath(theWidget.ControlPath))) {
									try {
										widget = Page.LoadControl(theWidget.ControlPath);
									} catch (Exception ex) {
										Literal lit = new Literal();
										lit.Text = "<b>ERROR: " + theWidget.ControlPath + "</b> <br />\r\n" + ex.ToString();
										widget = lit;
									}
								} else {
									Literal lit = new Literal();
									lit.Text = "MISSING FILE: " + theWidget.ControlPath;
									widget = lit;
								}
							}

							if (theWidget.ControlPath.ToLower().StartsWith("class:")) {
								try {
									Assembly a = Assembly.GetExecutingAssembly();
									string className = theWidget.ControlPath.Replace("CLASS:", "");
									Type t = Type.GetType(className);
									Object o = Activator.CreateInstance(t);

									if (o != null) {
										widget = o as Control;
									} else {
										Literal lit = new Literal();
										lit.Text = "OOPS: " + theWidget.ControlPath;
										widget = lit;
									}
								} catch (Exception ex) {
									Literal lit = new Literal();
									lit.Text = "<b>ERROR: " + theWidget.ControlPath + "</b> <br />\r\n" + ex.ToString();
									widget = lit;
								}
							}

							widget.ID = CtrlId;

							IWidget w = null;
							if (widget is IWidget) {
								w = widget as IWidget;
								w.SiteID = SiteData.CurrentSiteID;
								w.PageWidgetID = theWidget.Root_WidgetID;
								w.RootContentID = theWidget.Root_ContentID;
							}

							if (widget is IWidgetParmData) {
								IWidgetParmData wp = widget as IWidgetParmData;
								List<WidgetProps> lstProp = theWidget.ParseDefaultControlProperties();

								wp.PublicParmValues = lstProp.ToDictionary(t => t.KeyName, t => t.KeyValue);
							}

							if (widget is IWidgetRawData) {
								IWidgetRawData wp = widget as IWidgetRawData;
								wp.RawWidgetData = theWidget.ControlProperties;
							}

							if (widget is IWidgetEditStatus) {
								IWidgetEditStatus wes = widget as IWidgetEditStatus;
								wes.IsBeingEdited = SiteData.AdvancedEditMode;
							}

							if (SiteData.AdvancedEditMode) {
								WidgetWrapper plcWrapper = new WidgetWrapper();
								plcWrapper.IsAdminMode = true;
								plcWrapper.ControlPath = theWidget.ControlPath;
								plcWrapper.ControlTitle = theWidget.ControlPath;

								CMSPlugin plug = (from p in cmsHelper.ToolboxPlugins
												  where p.FilePath.ToLower() == plcWrapper.ControlPath.ToLower()
												  select p).FirstOrDefault();

								if (plug != null) {
									plcWrapper.ControlTitle = plug.Caption;
								}

								plcWrapper.Order = theWidget.WidgetOrder;
								plcWrapper.DatabaseKey = theWidget.Root_WidgetID;

								plcWrapper.Controls.Add(widget);
								plcHolder.Controls.Add(plcWrapper);

								if (w != null) {
									if (w.EnableEdit) {
										string sScript = w.JSEditFunction;
										if (string.IsNullOrEmpty(sScript)) {
											sScript = "cmsGenericEdit('" + pageContents.Root_ContentID + "','" + plcWrapper.DatabaseKey + "')";
										}

										plcWrapper.JSEditFunction = sScript;
									}
								}
							} else {
								plcHolder.Controls.Add(widget);
							}

						}
					}

					cmsHelper.Dispose();
				}
			}
		}


		private void SetPageTitle(ContentPage pageData) {

			Page.Title = string.Format(PageTitlePattern, theSite.SiteName, pageData.TitleBar);

			if (!pageData.PageActive) {
				if (SiteData.IsAdmin || SiteData.IsEditor) {
					Page.Title = string.Format(PageTitlePattern, "* UNPUBLISHED * " + theSite.SiteName, pageData.TitleBar);
				}
			}
		}

		protected void MarkWidgets(Control X, bool bAdmin) {
			//add the command click event to the link buttons on the datagrid heading
			foreach (Control c in X.Controls) {
				if (c is WidgetContainer) {
					WidgetContainer ph = (WidgetContainer)c;
					ph.IsAdminMode = bAdmin;
				} else {
					MarkWidgets(c, bAdmin);
				}
			}
		}


	}
}
