﻿using HelpMe.Hubs;
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

        // GET: Chat
        public async Task<ActionResult> Index(int? dialogId=null)
        {
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
                                                .Where(u => u.UserFromId == requestId )
                                                .ToListAsync();

                return View(await messages.ToListAsync());
            }
            else
            {
                string requestId = User.Identity.GetUserId();
                
                ViewBag.Dialogs = await db.ChatDialogs.Include(u => u.UserFrom)
                                                 .Include(u => u.UserTo)
                                                 .Where(u => u.UserFromId == requestId)
                                                 .ToListAsync();
                return View();
            }
        }


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
                    message.Status = MessageStatus.Undreading;
                    messageStoreViewModelPartner.UserFromId = db.Users.Where(x => x.Id == message.UserFromId).FirstOrDefault().Id;
                    messageStoreViewModelPartner.UserToId = db.Users.Where(x => x.Id == message.UserToId).FirstOrDefault().Id;
                    messageStoreViewModelPartner.Description = message.Description;
                    messageStoreViewModelPartner.DateSend = DateTime.Now;
                    
                    dialog.Messages.Add(message);
                    dialog.UserFromId = message.UserFromId;
                    dialog.UserToId = message.UserToId;

                    dialogTo.Messages.Add(messageStoreViewModelPartner);
                    dialogTo.UserFromId = message.UserToId;
                    dialogTo.UserToId = message.UserFromId;

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
                    message.Status = MessageStatus.Undreading;
                    messageStoreViewModelPartner.UserFromId = db.Users.Where(x => x.Id == message.UserFromId).FirstOrDefault().Id;
                    messageStoreViewModelPartner.UserToId = db.Users.Where(x => x.Id == message.UserToId).FirstOrDefault().Id;
                    messageStoreViewModelPartner.Description = message.Description;
                    messageStoreViewModelPartner.DateSend = DateTime.Now;
                    dialog.Messages.Add(message);
                    dialogTo.Messages.Add(messageStoreViewModelPartner);
                    return RedirectToAction("Index", "Chat", new { dialogId = dialog.Id });
                }  
            }

            return View(message);
        }



        [HttpGet]
        public async Task<ActionResult> LoadHistory(string userName)
        {
            string requestId = User.Identity.GetUserId();
            var userTo = db.Users.Include(u => u.Messages).Where(u => u.UserName == userName).FirstOrDefault();
            var userFrom = db.Users.Include(u => u.Messages).Where(u => u.Id == requestId).FirstOrDefault();

            var dialog = db.ChatDialogs.Include(c => c.UserFrom).Include(c => c.UserTo)
                                        .Where(c => c.UserFromId == userFrom.Id && c.UserToId == userTo.Id).FirstOrDefault();

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