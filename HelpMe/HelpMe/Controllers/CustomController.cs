﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HelpMe.Models;
using Microsoft.AspNet.Identity;
using HelpMe.Hubs;
using System.IO;
using PagedList;

namespace HelpMe.Controllers
{
    public class CustomController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
      
        // GET: Custom
        public ActionResult Index(int page = 1)
        {  
            var customViewModels = db.Customs.Include(c => c.Comments).Include(c => c.User).OrderBy(x => x.Id);
            int pageSize = 9; // количество объектов на страницу
            IEnumerable<CustomViewModel> customPerPages = customViewModels.Skip((page - 1) * pageSize).Take(pageSize);
            PageInfo pageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = customViewModels.Count() };
            CustomIndexViewModel ivm = new CustomIndexViewModel { PageInfo = pageInfo, Customs = customPerPages };
            return View(ivm);
        }

        public ActionResult LoadAttaches(int? id)
        {
            var allAttaches = db.Attachments.Include(c => c.CustomViewModel).Where(c => c.CustomViewModelId == id).ToList();
            return PartialView(allAttaches);
        }

      
        public async Task<JsonResult> DeleteAttach(int? id)
        {
           
            AttachModel attach = await db.Attachments.FindAsync(id);
            var customId = attach.CustomViewModelId;
            db.Attachments.Remove(attach);
            await db.SaveChangesAsync();
            return Json(true);
        }

        [HttpPost]
        public async Task<JsonResult> Upload()
        {
            var idCustom = Convert.ToInt32(Request.Form["CustomViewModelId"]);
            CustomViewModel customViewModel = await db.Customs.Include(c => c.User).Include(a => a.Attachments).FirstOrDefaultAsync(c => c.Id == idCustom);
            var count = customViewModel.Attachments.Count;
            if (customViewModel.ExecutorId == User.Identity.GetUserId() && count < 10)
            {
                AttachModel attach = new AttachModel();

                foreach (string file in Request.Files)
                {
                    var upload = Request.Files[file];

                    if (upload != null)
                    {
                        // получаем имя файла
                        string fileName = System.IO.Path.GetFileName(upload.FileName);
                        // сохраняем файл в папку Files в проекте
                        upload.SaveAs(Server.MapPath("~/Files/" + fileName));
                        string path = Server.MapPath("~/Files/" + fileName);
                        // сохраняем файл в папку Files в проекте
                        upload.SaveAs(path);
                        attach.Id = 1;
                        attach.ExecutorPrice = Convert.ToInt32(Request.Form["ExecutorPrice"]);
                        attach.CustomViewModelId = Convert.ToInt32(Request.Form["CustomViewModelId"]);
                        attach.AttachFilePath = path;
                        attach.AttachStatus = AttachStatus.NotPurchased;
                        attach.UserId = User.Identity.GetUserId();
                        db.Attachments.Add(attach);

                        await db.SaveChangesAsync();
                    }
                }
            }
            return Json("Файл загружен");
        }

        public async Task<ActionResult> Buy(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // optimaize
            AttachModel attachViewModel = await db.Attachments.Include(c => c.User).Include(c => c.CustomViewModel)
                                                              .FirstOrDefaultAsync(c => c.Id == id);

            if (attachViewModel.AttachStatus != AttachStatus.Purchased)
            {

                string myId = User.Identity.GetUserId();
                Wallet wallet = db.Wallets.Where(x => x.UserId == myId).FirstOrDefault();

                if (wallet.Summ - attachViewModel.ExecutorPrice > 0)
                {
                    wallet.Summ -= attachViewModel.ExecutorPrice;
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                attachViewModel.AttachStatus = AttachStatus.Purchased;
                if (attachViewModel.CustomViewModel.Attachments.Where(c => c.AttachStatus == AttachStatus.Purchased).Count() == 0)
                    attachViewModel.CustomViewModel.Status = CustomStatus.CheckCustom;
                db.Entry(wallet).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return Content("<div class='task-tags'><span>Работа куплена </span></div>", "text/html");
                // return RedirectToAction("Details", "Custom", new { id = attachViewModel.CustomViewModel.Id });
            } else { 
                ViewBag.DangerText = "Решение куплено!";
            }
            return RedirectToAction("Details", "Custom", new { id = attachViewModel.CustomViewModel.Id });

        }



        public async Task<ActionResult> ChooseExecutor(int? id, string userId)
        {
            if (id == null && userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CustomViewModel customViewModel = await db.Customs.Include(c => c.Comments).Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
           

            if (customViewModel.UserId != customViewModel.ExecutorId)
            {
                customViewModel.ExecutorId = userId;
                CommentViewModel comment = await db.Comments.Include(c => c.CustomViewModel).Include(c => User).FirstOrDefaultAsync(c => c.UserId == customViewModel.ExecutorId);
                db.Entry(customViewModel).State = EntityState.Modified;
                customViewModel.ExecutorPrice = comment.OfferPrice;
                customViewModel.Status = CustomStatus.Check; // заявка выполняется
            }
           
            await db.SaveChangesAsync();
            return RedirectToAction("Details", "Custom", new { id = customViewModel.Id });
        }

        [HttpPost]
        public async Task<ActionResult> CustomSearch(string name)
        {
            var allCustoms = await db.Customs.Where(a => a.Name.Contains(name)).ToListAsync();
            if (allCustoms.Count <= 0)
            {
                return HttpNotFound();
            }
            return PartialView(allCustoms);
        }

        private void SendMessage(string message)
        {
            // Получаем контекст хаба
            var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            Notification notification = new Notification { Id = 1, Title = "123", Status = NotificationStatus.Unreading };
            db.Notifications.Add(notification);
            db.SaveChanges();
            context.Clients.All.displayMessage(message);
        }


        [HttpPost]
        public async Task<ActionResult> SendSolution(int? id, HttpPostedFileBase upload)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CustomViewModel customViewModel = await db.Customs.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);

            if (customViewModel.ExecutorId == User.Identity.GetUserId())
            {
                if (ModelState.IsValid && upload != null)
                {
                    // получаем имя файла
                    string fileName = System.IO.Path.GetFileName(upload.FileName);
                    string path = Server.MapPath("~/Files/" + fileName);
                    // сохраняем файл в папку Files в проекте
                    upload.SaveAs(path);
                    db.Entry(customViewModel).State = EntityState.Modified;
                    customViewModel.FilePath = path;
                    customViewModel.Status = CustomStatus.NeedBuy; // вложения требует покупки
                  //  customViewModel.AttachStatus = AttachStatus.NotPurchased; // решение не куплено
                    await db.SaveChangesAsync();
                }
            }

            return RedirectToAction("Details", "Custom", new { id = customViewModel.Id }); ;
        }

    
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetComment(int id)
        {
            var results = db.Comments.Select(e => new
            {
                e.Id,
                e.Name,
                e.Description
            }).Where(e => e.Id == id).FirstOrDefault();

            return new JsonResult() { Data = results, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public JsonResult People(string Description, int CustomViewModelId, int OfferPrice, int Days)
        {
            var comment = new CommentViewModel { Description = Description, OfferPrice = OfferPrice, Days= Days, UserId = User.Identity.GetUserId(), CustomViewModelId = CustomViewModelId };
            CustomViewModel customViewModel = db.Customs.Include(c => c.Comments).FirstOrDefault(c => c.Id == CustomViewModelId);
            if (customViewModel.Comments.Where(c => c.UserId == User.Identity.GetUserId()).Count() >= 1)
            {
                return null;
            }
            else
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Json(new
                {
                    Id = comment.Id,
                    Description = comment.Description,
                    OfferPrice = comment.OfferPrice,
                    CustomViewModelId = comment.CustomViewModelId,
                    UserId = comment.UserId
                }, JsonRequestBehavior.AllowGet);
                // return new JsonResult() { Data = comment, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }


        public FileResult GetFileAttach(string path, int? id)
        {
            AttachModel attachViewModel = db.Attachments.Include(c => c.User).Include(c => c.CustomViewModel)
                                                            .FirstOrDefault(c => c.Id == id);
            // Тип файла - content-type
            if (attachViewModel.AttachStatus == AttachStatus.Purchased && attachViewModel.CustomViewModel.ExecutorId != User.Identity.GetUserId())
            {
                string file_type = "application/" + Path.GetExtension(path);
                string file_name = Path.GetFileName(path);

                return File(path, file_type, file_name);
            } else
            {
                return null;
            }

        }

        public FileResult GetFile(string path)
        {
                string file_type = "application/" + Path.GetExtension(path);
                string file_name = Path.GetFileName(path);

                return File(path, file_type, file_name);
        }

        // GET: Custom/Details/5
        public async Task<ActionResult> Details(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // optimaize
            CustomViewModel customViewModel = await db.Customs.Include(c => c.Comments)
                                                              .Include(c => c.CategoryTask)
                                                              .Include(c => c.TypeTask)
                                                              .Include(c => c.User)
                                                              .Include(c => c.Attachments)
                                                              .FirstOrDefaultAsync(c => c.Id == id);

            if (customViewModel == null)
            {
                return HttpNotFound();
            }

            return View(customViewModel);
        }

        // GET: Custom/Create
        public ActionResult Create()
        {
            SelectList types = new SelectList(db.CustomTypes, "Id", "Name"); // выбор типа задачи
            ViewBag.Types = types;
            SelectList categories = new SelectList(db.TaskCategories, "Id", "Name"); // выбор типа задачи
            ViewBag.Categories = categories;
            return View();
        }

        // GET: Custom/Create
        public async Task<ActionResult> Accept(int? id)
        {
            // optimaize
            CustomViewModel customViewModel = await db.Customs.Include(c => c.Comments)
                                                              .Include(c => c.CategoryTask)
                                                              .Include(c => c.TypeTask)
                                                              .Include(c => c.User)
                                                              .Include(c => c.Attachments)
                                                              .FirstOrDefaultAsync(c => c.Id == id);

            db.Entry(customViewModel).State = EntityState.Modified;
            customViewModel.Status = CustomStatus.Close;
            await db.SaveChangesAsync();
            return RedirectToAction("Details", "Custom", new { id = customViewModel.Id });
        }

        public async Task<ActionResult> Revision(int? id)
        {
            // optimaize
            CustomViewModel customViewModel = await db.Customs.Include(c => c.Comments)
                                                              .Include(c => c.CategoryTask)
                                                              .Include(c => c.TypeTask)
                                                              .Include(c => c.User)
                                                              .Include(c => c.Attachments)
                                                              .FirstOrDefaultAsync(c => c.Id == id);

            db.Entry(customViewModel).State = EntityState.Modified;
            customViewModel.Status = CustomStatus.Revision; // на доработку
            await db.SaveChangesAsync();
            return RedirectToAction("Details", "Custom", new { id = customViewModel.Id });
        }

        // POST: Custom/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Description,AttachFilePath,AttachFile,UserId,TypeTaskId,CategoryTaskId,EndingDate,MinPrice,MaxPrice")] CustomViewModel customViewModel)
        {
            if (ModelState.IsValid)
            {
                string fileName = Path.GetFileName(customViewModel.AttachFile.FileName);
                string path = Server.MapPath("~/Files/" + fileName);
                // сохраняем файл в папку Files в проекте
                customViewModel.AttachFile.SaveAs(path);
                customViewModel.AttachFilePath = "~/Files/" + fileName;
                customViewModel.Status = CustomStatus.Open; // открытая заявка
                customViewModel.UserId = User.Identity.GetUserId();
                db.Customs.Add(customViewModel);
                await db.SaveChangesAsync();
                SendMessage("Добавлен новый заказ");
                return RedirectToAction("Index");
            }
        
            return View(customViewModel);
        }

        // GET: Custom/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CustomViewModel customViewModel = await db.Customs.FindAsync(id);

            if (customViewModel == null)
            {
                return HttpNotFound();
            }

            if (customViewModel.UserId == User.Identity.GetUserId())
            {
                return View(customViewModel);
            }
            else { return new HttpStatusCodeResult(HttpStatusCode.BadRequest); }
        }

        // POST: Custom/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,AttachFilePath,AttachFile,UserId,TypeTaskId,CategoryTaskId,EndingDate,MinPrice,MaxPrice")] CustomViewModel customViewModel)
        { 
            if (ModelState.IsValid)
            {
                db.Entry(customViewModel).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return RedirectToAction("Details", "Custom", new { id = customViewModel.Id }); 
            }
           
            return View(customViewModel);
        }

        // GET: Custom/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomViewModel customViewModel = await db.Customs.FindAsync(id);
            if (customViewModel == null)
            {
                return HttpNotFound();
            }

            if (customViewModel.UserId == User.Identity.GetUserId())
            {
                return View(customViewModel);
            }
            else { return new HttpStatusCodeResult(HttpStatusCode.BadRequest); }
        }

        // POST: Custom/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            CustomViewModel customViewModel = await db.Customs.FindAsync(id);
            db.Customs.Remove(customViewModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
