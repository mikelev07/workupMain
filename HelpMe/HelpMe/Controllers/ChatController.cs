using HelpMe.Hubs;
using HelpMe.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HelpMe.Controllers
{
    public class ChatController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        const int pageSize = 2;
        // GET: Chat
        public async Task<ActionResult> Index(int? id, int? dialogId = null)
        {
            //int page = id ?? 0;
            //var userToId = db.ChatDialogs.Where(i => i.Id == dialogId).FirstOrDefault().UserToId;
            if (dialogId != null)
            {
                var userToId = db.ChatDialogs.Where(i => i.Id == dialogId).FirstOrDefault().UserToId;
                string requestId = User.Identity.GetUserId();
                var userTo = db.Users.Include(u => u.Messages).Where(u => u.Id == userToId).FirstOrDefault();
                var userFrom = db.Users.Include(u => u.Messages).Where(u => u.Id == requestId).FirstOrDefault();

                var messages = db.Messages.Include(u => u.UserFrom)
                                          .Include(u => u.UserTo)
                                          .Where(d => d.ChatDialogId == dialogId)
                                          .Where(m => (m.UserFromId == userFrom.Id && m.UserToId == userTo.Id) ||
                                          (m.UserFromId == userTo.Id) && m.UserToId == userFrom.Id).OrderBy(m => m.DateSend);

                /* ViewBag.Dialogs = await db.Messages.Include(u => u.UserFrom)
                                                .Include(u => u.UserTo)
                                                .Where(u => u.UserToId != requestId)
                                                .GroupBy(car => car.UserToId)
                                                .Select(g => g.FirstOrDefault()).ToListAsync(); */

                ViewBag.Dialogs = await db.ChatDialogs.Include(u => u.UserFrom)
                                                .Include(u => u.UserTo)
                                                .Include(m => m.Messages)
                                                .Where(u => u.UserFromId == requestId)
                                                .ToListAsync();

                return View(await messages.ToListAsync());
            }
            else
            {
                string requestId = User.Identity.GetUserId();
                var openDialog = db.ChatDialogs.Where(i => i.UserFromId == requestId)
                                               .Where(s => s.Status == DialogStatus.Open)
                                               .FirstOrDefault();

                ViewBag.UserToName = openDialog?.UserTo?.UserName;

                if (openDialog != null)
                {

                    var userToId = openDialog.UserToId;

                    var userTo = db.Users.Include(u => u.Messages).Where(u => u.Id == userToId).FirstOrDefault();
                    var userFrom = db.Users.Include(u => u.Messages).Where(u => u.Id == requestId).FirstOrDefault();

                    var messages = db.Messages.Include(u => u.UserFrom)
                                              .Include(u => u.UserTo)
                                              .Where(d => d.ChatDialogId == openDialog.Id)
                                              .Where(m => (m.UserFromId == userFrom.Id && m.UserToId == userTo.Id) ||
                                              (m.UserFromId == userTo.Id) && m.UserToId == userFrom.Id).OrderBy(m => m.DateSend);


                    ViewBag.Dialogs = await db.ChatDialogs.Include(u => u.UserFrom)
                                                     .Include(u => u.UserTo)
                                                     .Include(m => m.Messages)
                                                     .Where(u => u.UserFromId == requestId)
                                                     .ToListAsync();

                  //  if (Request.IsAjaxRequest())
                  //  {
                   //     return PartialView("LoadHistory", GetItemsPage(page));
                  //  }
                  //  return View(GetItemsPage(page));

                  return View(await messages.ToListAsync());
                }
                ViewBag.Dialogs = await db.ChatDialogs.Include(u => u.UserFrom)
                                                    .Include(u => u.UserTo)
                                                    .Include(m => m.Messages)
                                                    .Where(u => u.UserFromId == requestId)
                                                    .ToListAsync();

                return View();
            }
        }

       // private List<MessageStoreViewModel> GetItemsPage(int page = 1)
      //  {
        //    var itemsToSkip = page * pageSize;
            
         //   return db.Messages.OrderBy(t => t.Id).Skip(itemsToSkip).
           //     Take(pageSize).ToList();
       // }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMessage([Bind(Include = "Id,Description,DateSend,Status,UserFromId,UserToId")] MessageStoreViewModel message)
        {
            if (ModelState.IsValid)
            {
                var diaologs = db.ChatDialogs.Include(u => u.UserFrom)
                                             .Include(u => u.UserTo)
                                             .Include(m => m.Messages);

                if (!diaologs.Any(u => (u.UserFromId == message.UserFromId && u.UserToId == message.UserToId)
                            || (u.UserFromId == message.UserToId && u.UserToId == message.UserFromId)))
                {
                    MessageStoreViewModel messageStoreViewModelPartner = new MessageStoreViewModel();
                    ChatDialog dialog = new ChatDialog();
                    ChatDialog dialogTo = new ChatDialog();
                    dialog.Id = 1;
                    dialogTo.Id = 2;
                    message.DateSend = DateTime.Now;
                    message.Status = MessageStatus.Reading;
                    messageStoreViewModelPartner.UserFromId = db.Users.Where(x => x.Id == message.UserFromId).FirstOrDefault().Id;
                    messageStoreViewModelPartner.UserToId = db.Users.Where(x => x.Id == message.UserToId).FirstOrDefault().Id;
                    messageStoreViewModelPartner.Description = message.Description;
                    messageStoreViewModelPartner.DateSend = DateTime.Now;
                    messageStoreViewModelPartner.Status = MessageStatus.Undreading;
                    dialog.Messages.Add(message);
                    dialog.UserFromId = message.UserFromId;
                    dialog.UserToId = message.UserToId;
                    dialog.Status = DialogStatus.Close;

                    dialogTo.Messages.Add(messageStoreViewModelPartner);
                    dialogTo.UserFromId = message.UserToId;
                    dialogTo.UserToId = message.UserFromId;
                    dialogTo.Status = DialogStatus.Close;

                    db.ChatDialogs.Add(dialogTo);
                    db.ChatDialogs.Add(dialog);
                    db.SaveChanges();
                    return RedirectToAction("Index", "Chat", new { dialogId = dialog.Id });
                }
                else
                {
                    var dialog = diaologs.Where(u => u.UserFromId == message.UserFromId && u.UserToId == message.UserToId).FirstOrDefault();
                    var dialogTo = diaologs.Where(u => u.UserFromId == message.UserToId && u.UserToId == message.UserFromId).FirstOrDefault();
                    MessageStoreViewModel messageStoreViewModelPartner = new MessageStoreViewModel();
                    message.DateSend = DateTime.Now;
                    message.Status = MessageStatus.Reading;
                    messageStoreViewModelPartner.UserFromId = db.Users.Where(x => x.Id == message.UserFromId).FirstOrDefault().Id;
                    messageStoreViewModelPartner.UserToId = db.Users.Where(x => x.Id == message.UserToId).FirstOrDefault().Id;
                    messageStoreViewModelPartner.Description = message.Description;
                    messageStoreViewModelPartner.DateSend = DateTime.Now;
                    messageStoreViewModelPartner.Status = MessageStatus.Undreading;
                    dialog.Messages.Add(message);
                    dialogTo.Messages.Add(messageStoreViewModelPartner);
                    db.SaveChanges();
                    return RedirectToAction("Index", "Chat", new { dialogId = dialog.Id });
                }
            }

            return View(message);
        }

        [HttpGet]
        public ActionResult GetLastMessage(string userName)
        {
            string requestId = User.Identity.GetUserId();
            var userTo = db.Users.Include(u => u.Messages).Where(u => u.UserName == userName).FirstOrDefault();
            var userFrom = db.Users.Include(u => u.Messages).Where(u => u.Id == requestId).FirstOrDefault();
            var dialogId = db.ChatDialogs.Where(u => u.UserFromId == userFrom.Id && u.UserToId == userTo.Id).FirstOrDefault().Id;

            var messages = db.Messages.Include(u => u.UserFrom)
                                      .Include(u => u.UserTo)
                                      .Where(d => d.ChatDialogId == dialogId)
                                      .Where(m => (m.UserFromId == userFrom.Id && m.UserToId == userTo.Id) ||
                                      (m.UserFromId == userTo.Id) && m.UserToId == userFrom.Id).OrderBy(m => m.DateSend);

            var lastMessage = messages.OrderByDescending(o => o.Id).FirstOrDefault().Description;

            return Json(lastMessage, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> LoadHistory(string userName)
        {
            string requestId = User.Identity.GetUserId();
            var userTo = db.Users.Include(u => u.Messages).Where(u => u.UserName == userName).FirstOrDefault();
            var userFrom = db.Users.Include(u => u.Messages).Where(u => u.Id == requestId).FirstOrDefault();

            var allOpenDialogs = db.ChatDialogs.Where(i => i.UserFromId == requestId)
                                           .Where(s => s.Status == DialogStatus.Open);

            foreach (var i in allOpenDialogs)
            {
                db.Entry(i).State = EntityState.Modified;
                i.Status = DialogStatus.Close;

            }

            var dialog = db.ChatDialogs.Include(c => c.UserFrom).Include(c => c.UserTo)
                                        .Where(c => c.UserFromId == userFrom.Id && c.UserToId == userTo.Id).FirstOrDefault();

            db.Entry(dialog).State = EntityState.Modified;
            dialog.Status = DialogStatus.Open;

            var messages = db.Messages.Include(u => u.UserFrom)
                                      .Include(u => u.UserTo)
                                      .Where(d => d.ChatDialogId == dialog.Id)
                                      .Where(m => (m.UserFromId == userFrom.Id && m.UserToId == userTo.Id) ||
                                      (m.UserFromId == userTo.Id) && m.UserToId == userFrom.Id).OrderBy(m => m.DateSend);

            foreach (var item in messages)
            {
                db.Entry(item).State = EntityState.Modified;
                item.Status = MessageStatus.Reading;

            }

            await db.SaveChangesAsync();

            return PartialView(await messages.ToListAsync());
        }
    }
}