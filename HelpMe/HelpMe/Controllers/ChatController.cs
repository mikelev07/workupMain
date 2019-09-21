using HelpMe.Hubs;
using HelpMe.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HelpMe.Controllers
{
    public class ChatController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        const int pageSize = 2;
        // GET: Chat
        public async Task<ActionResult> Index(int? id)
        {
            string userFromId = User.Identity.GetUserId();
            string openDialogId = HttpContext.Request.Cookies["OpenDialogId"]?.Value;
            ViewBag.Dialogs = await db.ChatDialogs
                    .Include(u => u.UserFrom)
                    .Include(u => u.UserTo)
                    .Include(m => m.Messages)
                    .Where(u => u.UserFromId == userFromId)
                    .ToListAsync();

            //если в куках есть id последнего открытого диалога, то выбираем нужный диалог и грузим историю сообщений
            //иначе оставляем область переписки пустой
            if (openDialogId != null)
            {
                var currentDialog = await db.ChatDialogs.Where(m => m.Id.ToString() == openDialogId).FirstOrDefaultAsync();
                var userToId = currentDialog.UserToId;
                var messages = await db.Messages.Include(u => u.UserFrom)
                                          .Include(u => u.UserTo)
                                          .Include(u => u.MessageAttaches)
                                          .Where(d => d.ChatDialogId.ToString() == openDialogId)
                                          .Where(m => (m.UserFromId == userFromId && m.UserToId == userToId) ||
                                          (m.UserFromId == userToId) && m.UserToId == userFromId)
                                          .OrderBy(m => m.DateSend).ToListAsync();

                ViewBag.UserToName = currentDialog?.UserTo?.UserName;
                return View(messages);
            }
            return View();
        }

        public FileResult GetChatFile(string path)
        {
            string file_type = "application/" + Path.GetExtension(path);
            string file_name = Path.GetFileName(path);

            return File(path, file_type, file_name);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public async Task<JsonResult> DialogSearchAsync(string dName)
        {
            var requestId = User.Identity.GetUserId();
            var dialogs = await db.ChatDialogs.Include(u => u.UserFrom)
                                                    .Include(u => u.UserTo)
                                                    .Include(m => m.Messages)
                                                    .Where(u => u.UserFromId == requestId)
                                                    .ToListAsync();

            var filtDialog = dialogs.Where(a => a.UserTo.UserName.Contains(dName)).FirstOrDefault().UserTo.UserName;

            return Json(filtDialog, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> CustomSearch(string dName)
        {
            var requestId = User.Identity.GetUserId();
            var dialogs = await db.ChatDialogs.Include(u => u.UserFrom)
                                                    .Include(u => u.UserTo)
                                                    .Include(m => m.Messages)
                                                    .Where(u => u.UserFromId == requestId)
                                                    .ToListAsync();

            var openDialog = db.ChatDialogs.Where(i => i.UserFromId == requestId)
                                           //.Where(s => s.Status == DialogStatus.Open)
                                           .FirstOrDefault();

            ViewBag.UserToName = openDialog?.UserTo?.UserName;

            var filtDialog = dialogs.Where(a => a.UserTo.UserName.Contains(dName));
            return PartialView(filtDialog);
        }


        public async Task<ActionResult> LoadDialogs()
        {
            var requestId = User.Identity.GetUserId();
            var dialogs = await db.ChatDialogs.Include(u => u.UserFrom)
                                                    .Include(u => u.UserTo)
                                                    .Include(m => m.Messages)
                                                    .Where(u => u.UserFromId == requestId)
                                                    .ToListAsync();

            var openDialog = db.ChatDialogs.Where(i => i.UserFromId == requestId)
                                           //.Where(s => s.Status == DialogStatus.Open)
                                           .FirstOrDefault();

            ViewBag.UserToName = openDialog?.UserTo?.UserName;

            return PartialView(dialogs);
        }

        [HttpPost]
        public JsonResult UploadAttach()
        {
            //var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            string pathFile = null;
            List<string> pathList = new List<string>();

            foreach (string file in Request.Files)
            {
                var upload = Request.Files[file];
                if (upload != null)
                {
                    // получаем имя файла
                    //  string fileName = System.IO.Path.GetFileName(upload.FileName);
                    // сохраняем файл в папку Files в проекте
                    upload.SaveAs(Server.MapPath("~/Files/" + upload.FileName));
                    pathFile = Server.MapPath("~/Files/" + upload.FileName);
                    pathList.Add(pathFile);
                }
            }
            return Json(pathList);
        }

        // private List<MessageStoreViewModel> GetItemsPage(int page = 1)
        //  {
        //    var itemsToSkip = page * pageSize;

        //   return db.Messages.OrderBy(t => t.Id).Skip(itemsToSkip).
        //     Take(pageSize).ToList();
        // }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddMessage([Bind(Include = "Id,Description,DateSend,Status,UserFromId,UserToId")] MessageStoreViewModel message)
        {
            if (ModelState.IsValid)
            {
                var dialogs = await db.ChatDialogs.Include(u => u.UserFrom)
                                             .Include(u => u.UserTo)
                                             .Include(m => m.Messages).ToListAsync();
                var userFromId = message.UserFromId;
                var userToId = message.UserToId;
                var messageDescription = message.Description;

                message.DateSend = DateTime.Now;
                message.Status = MessageStatus.Reading;

                //сообщение для получателя
                MessageStoreViewModel messageStoreViewModelPartner = new MessageStoreViewModel()
                {
                    UserFromId = userFromId,
                    UserToId = userToId,
                    Description = messageDescription,
                    DateSend = DateTime.Now,
                    Status = MessageStatus.Undreading
                };

                if (!dialogs.Any(u => (u.UserFromId == userFromId && u.UserToId == userToId)
                || (u.UserFromId == userToId && u.UserToId == userFromId)))
                {
                    ChatDialog dialogFrom = new ChatDialog();
                    dialogFrom.Id = 1;
                    dialogFrom.Messages.Add(message);
                    dialogFrom.UserFromId = userFromId;
                    dialogFrom.UserToId = userToId;
                    //dialogFrom.Status = DialogStatus.Close;

                    ChatDialog dialogTo = new ChatDialog();
                    dialogTo.Id = 2;
                    dialogTo.Messages.Add(messageStoreViewModelPartner);
                    dialogTo.UserFromId = userToId;
                    dialogTo.UserToId = userFromId;
                    //dialogTo.Status = DialogStatus.Close;

                    db.ChatDialogs.Add(dialogTo);
                    db.ChatDialogs.Add(dialogFrom);
                    await db.SaveChangesAsync();


                    var nameTo = (await db.Users.Where(x => x.Id == userToId).FirstOrDefaultAsync()).UserName;
                    var nameFrom = (await db.Users.Where(x => x.Id == userFromId).FirstOrDefaultAsync()).UserName;
                    var unCount = dialogTo.Messages.Where(m => m.Status == MessageStatus.Undreading).Count();
                    var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                    context.Clients.User(userToId).addDialog(nameTo, nameFrom, messageDescription, unCount);

                    HttpContext.Response.Cookies["OpenDialogId"].Value = dialogFrom.Id.ToString();
                    return RedirectToAction("Index", "Chat");
                }
                else
                {
                    var dialogFrom = dialogs.Where(u => u.UserFromId == userFromId && u.UserToId == userToId).FirstOrDefault();
                    if (dialogFrom == null)
                    {
                        dialogFrom = new ChatDialog();
                        dialogFrom.Id = 3;
                        dialogFrom.UserFromId = userFromId;
                        dialogFrom.UserToId = userToId;
                        //dialogFrom.Status = DialogStatus.Close;
                        db.ChatDialogs.Add(dialogFrom);
                    }
                    var dialogTo = dialogs.Where(u => u.UserFromId == userToId && u.UserToId == userFromId).FirstOrDefault();
                    if (dialogTo == null)
                    {
                        dialogTo = new ChatDialog();
                        dialogTo.Id = 4;
                        dialogTo.UserFromId = userToId;
                        dialogTo.UserToId = userFromId;
                        //dialogTo.Status = DialogStatus.Close;
                        db.ChatDialogs.Add(dialogTo);
                    }

                    dialogFrom.Messages.Add(message);
                    dialogTo.Messages.Add(messageStoreViewModelPartner);
                    await db.SaveChangesAsync();

                    HttpContext.Response.Cookies["OpenDialogId"].Value = dialogFrom.Id.ToString();
                    return RedirectToAction("Index", "Chat");
                }
            }

            return View(message);
        }

        [HttpGet]
        public ActionResult GetLastMessage(string userToId, int dialogId)
        {
            string userFromId = User.Identity.GetUserId();

            var messages = db.Messages.Include(u => u.UserFrom)
                                      .Include(u => u.UserTo)
                                      .Where(d => d.ChatDialogId == dialogId)
                                      .Where(m => (m.UserFromId == userFromId && m.UserToId == userToId) ||
                                      (m.UserFromId == userToId) && m.UserToId == userFromId).OrderBy(m => m.DateSend);

            var lastMessage = messages.OrderByDescending(o => o.Id).FirstOrDefault().Description;

            return Json(lastMessage, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> DeleteDialog(string userName)
        {
            string requestId = User.Identity.GetUserId();
            var userTo = db.Users.Include(u => u.Messages).Where(u => u.UserName == userName).FirstOrDefault();
            var userFrom = db.Users.Include(u => u.Messages).Where(u => u.Id == requestId).FirstOrDefault();
            string userToId = userTo.Id;
            var dialog = await db.ChatDialogs.Where(i => i.UserFromId == requestId)
                                               .Where(i => i.UserToId == userToId).FirstOrDefaultAsync();

            var messagesOfDialog = db.Messages.Where(i => i.ChatDialogId == dialog.Id);

            db.Messages.RemoveRange(messagesOfDialog);


            var openDialog = db.ChatDialogs.Where(i => i.UserFromId == requestId).FirstOrDefault();
            //openDialog.Status = DialogStatus.Open;

            db.ChatDialogs.Remove(dialog);
            await db.SaveChangesAsync();
            return Json(true);
        }

        [HttpGet]
        public async Task<ActionResult> LoadHistory(string userToId, int dialogId)
        {
            string userFromId = User.Identity.GetUserId();

            var messages = await db.Messages
                                      .Include(u => u.MessageAttaches)
                                      .Where(d => d.ChatDialogId == dialogId).ToListAsync();

            HttpContext.Response.Cookies["OpenDialogId"].Value = dialogId.ToString();

            //нужно доработать
            //foreach (var message in messages)
            //{
            //    db.Entry(message).State = EntityState.Modified;
            //    message.Status = MessageStatus.Reading;
            //}
            //await db.SaveChangesAsync();

            return PartialView(messages);
        }
    }
}