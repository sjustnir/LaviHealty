using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using Microsoft.AspNet.FriendlyUrls;
using Microsoft.AspNet.FriendlyUrls.Resolvers;

namespace LaviHealty
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            var settings = new FriendlyUrlSettings {AutoRedirectMode = RedirectMode.Permanent};
            routes.EnableFriendlyUrls(settings, new MyWebFormsFriendlyUrlResolver());
        }
    }

    public class MyWebFormsFriendlyUrlResolver : WebFormsFriendlyUrlResolver
    {
        protected override bool TrySetMobileMasterPage(HttpContextBase httpContext, Page page, string mobileSuffix)
        {
            return mobileSuffix != "Mobile" && base.TrySetMobileMasterPage(httpContext, page, mobileSuffix);
        }
    }
}
