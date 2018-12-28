using System;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Kesco.App.Web.Roles
{
    /// <summary>
    ///     Обработчик, который выводит членом каких групп AD является текущий пользователь
    /// </summary>
    public class X : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write(UserPrincipal.Current.Name);
            context.Response.Write(" -> ");
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity != null)
                context.Response.Write(windowsIdentity.Name);
            context.Response.Write(Environment.NewLine);
            context.Response.Write("----------------------------------");
            context.Response.Write(Environment.NewLine);

            var groups = UserPrincipal.Current.GetGroups();
            var groupNames = groups.Select(x => x.SamAccountName).ToList().OrderBy(x => x).ToList();
            groupNames.ForEach(delegate(string groupName)
            {
                context.Response.Write(groupName);
                context.Response.Write(Environment.NewLine);
            });
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}