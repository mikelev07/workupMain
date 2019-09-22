using HelpMe.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace HelpMe.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            string uId = User.Identity.GetUserId();
            var unreadCount = db.Notifications.Where(n => n.UserId == uId).Where(n => n.Status == NotificationStatus.Unreading).Count();
            // ViewBag.Count = unreadCount;<span id="messUnreadCount">3</span>



            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public async System.Threading.Tasks.Task<ActionResult> GetAllNotifications()
        {
            string reqId = User.Identity.GetUserId();
            var notifications = db.Notifications.Where(u => u.UserId == reqId);
            foreach (var i in notifications)
            {
                db.Entry(i).State = EntityState.Modified;
                i.Status = NotificationStatus.Reading;
            }
            await db.SaveChangesAsync();
            var notes = await notifications.Select(a => new
            {
                a.Url,
                a.UserName,
                a.ExUserName,
                a.Description,
                a.Title
            }).Take(5).ToListAsync();

            return Json(notes, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUnreadingCount()
        {
            string reqId = User.Identity.GetUserId();
            var unreadCount = db.Notifications.Where(u => u.UserId == reqId).Where(n => n.Status == NotificationStatus.Unreading).Count();
            
            return new JsonResult { Data = unreadCount, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult GetUnreadingMessageCount()
        {
            string reqId = User.Identity.GetUserId();
            var unreadCount = db.Messages.Where(s => s.Status == MessageStatus.Undreading).Where(u => u.UserToId == reqId).Count();

            return new JsonResult { Data = unreadCount, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}