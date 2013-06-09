/*

Copyright (C) 2007-2011 by Gustavo Duarte and Bernardo Vieira.
 
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 
*/
using System;
using System.Collections.Generic;
using System.Web;

namespace Common.Helpers {
	public delegate void Handler(HttpContext context);

	public class HttpAction {
		public string Name;
		public Handler Handler;
		public string MimeType;
	}

	public abstract class HttpHandlerBase : IHttpHandler {
		protected abstract List<HttpAction> GetActions();

		public void ProcessRequest(HttpContext context) {
			context.Response.StatusCode = 200;
			context.Response.CacheControl = "no-cache";
			context.Response.ContentType = "text/plain";
			context.Response.BufferOutput = false;

			HttpAction httpAction = GetActionForRequest(context.Request.QueryString[null]);

			if (null == httpAction) {
				ShowUsageAndDie(context);
				return;
			}

			if (!string.IsNullOrEmpty(httpAction.MimeType)) {
				context.Response.ContentType = httpAction.MimeType;
			}

			try {
				httpAction.Handler(context);	
			} catch (Exception ex) {
				try {
					context.Response.Write("; showError({0});".Fi(ex.ToString().ToJson()));
				} catch {}
			}
			
			context.Response.End();
		}

		public abstract bool IsReusable { get; }

		private HttpAction GetActionForRequest(string actionName) {
			if (string.IsNullOrEmpty(actionName)) {
				return null;
			}

			actionName = actionName.Trim();

			foreach (HttpAction action in this.GetActions()) {
				if (0 == string.Compare(actionName, action.Name, StringComparison.OrdinalIgnoreCase)) {
					return action;
				}
			}

			return null;
		}

		private void ShowUsageAndDie(HttpContext context) {
			context.Response.StatusCode = 400;
			context.Response.Write("Bad request. You must specify the action you wish to take. One of: ");
			foreach (HttpAction action in this.GetActions()) {
				context.Response.Write("\n" + action.Name);
			}
		}

	}
}