using HelpMe.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<ActionResult> GetAllNotifications()
        {
            //решил переделать метод

            /*
             * СТАРАЯ логика была следующая
            --находим все нотификации
            --из них выбираем непрочитанные
            --меняем их статус на прочтенные
            --затем берём первые 5 нотификаций из всего списка и возвращаем в качестве результат*/

            /*
             НОВАЯ логика такова:
             --находим первые 5 нотификаций в порядке убывания даты создания со статусом непрочтён
             --возвращаем
             а менять статусы на прочтенные предоставляется юзеру по нажатию на галочку в дропдауне
             тогда срабатывает метод MarkHeaderNotesAsRead
             после чего эти 5 сообщения отмечаются в бд как прочтенные
             подгружаются новые 5 непрочтённых
             */

            //как-то так

            string reqId = User.Identity.GetUserId();
            var unreadNotifications = db.Notifications.Where(n => n.UserId == reqId && n.Status == NotificationStatus.Unreading).OrderByDescending(n => n.StartDate).Take(5);
            var notes = await unreadNotifications.Select(n => new
            {
                n.Id,
                n.Description,
                n.CustomId
            }).ToListAsync();

            return Json(notes, JsonRequestBehavior.AllowGet);

            //string reqId = User.Identity.GetUserId();
            //var notifications = db.Notifications.Where(n => n.UserId == reqId);
            //var notificationsUnread = notifications.Where(s => s.Status == NotificationStatus.Unreading);

            //foreach (var i in notificationsUnread)
            //{
            //    db.Entry(i).State = EntityState.Modified;
            //    i.Status = NotificationStatus.Reading;
            //}

            //await db.SaveChangesAsync();
            //var notes = await notifications.Select(a => new
            //{
            //   a.Description,
            //   a.CustomId
            //}).ToListAsync();

            //return Json(notes, JsonRequestBehavior.AllowGet);
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

        //данный метод служит для отметки данного одного непрочтённого уведомления как прочтённого
        public async Task<ActionResult> MarkNoteAsRead(string noteId)
        {
            if (string.IsNullOrEmpty(noteId)) return Json(true);

            var userId = User.Identity.GetUserId();
            var currentNotification = await db.Notifications.Where(n => n.UserId == userId && n.Id == int.Parse(noteId)).FirstOrDefaultAsync();
            currentNotification.Status = NotificationStatus.Reading;

            await db.SaveChangesAsync();

            return Json(true);
        }

        //данный метод служит для отметки всех непрочтённых уведомлений В ШАПКЕ (где колокольчик) как прочтённых
        //принимает строку из id нужных уведомлений
        public async Task<ActionResult> MarkHeaderNotesAsRead(string[] ids)
        {
            if (ids == null) return Json(true);

            var notifications = await db.Notifications.Where(n => ids.Contains(n.Id.ToString())).ToListAsync();
            foreach(var note in notifications)
            {
                note.Status = NotificationStatus.Reading;
            }
            await db.SaveChangesAsync();
            return Json(true);
        }

        //данный метод служит для отметки вообще всех непрочтённых уведомлений как прочтённых
        public async Task<ActionResult> MarkAllNotesAsRead()
        {
            var userId = User.Identity.GetUserId();
            var unreadNotes = await db.Notifications.Where(n => n.UserId == userId && n.Status == NotificationStatus.Unreading).ToListAsync();
            foreach (var n in unreadNotes)
            {
                n.Status = NotificationStatus.Reading;
            }
            await db.SaveChangesAsync();

            return Json(true);
        }


        //для отладки нужно было
        //позволяет быстро сделать все уведомления непрочитанными
        //смотри в Layout, LayoutDash и LayoutChat

        public async Task<ActionResult> TestMakeAllUnread(string userId)
        {
            var unreadNotes = await db.Notifications.Where(n => n.UserId == userId && n.Status == NotificationStatus.Reading).ToListAsync();
            foreach (var n in unreadNotes)
            {
                n.Status = NotificationStatus.Unreading;
            }
            await db.SaveChangesAsync();

            return Json(true);
        }


        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}