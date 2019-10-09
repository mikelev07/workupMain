using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using HelpMe.Helpers;
using HelpMe.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;

namespace HelpMe.Hubs
{
    [System.Web.Http.Authorize]
    public class CustomUserIdProvider : IUserIdProvider
    {
        ApplicationDbContext db = new ApplicationDbContext();
        [System.Web.Http.Authorize]
        public string GetUserId(IRequest request)
        {
            var userId = request.User.Identity.GetUserId();
            return userId.ToString();
        }
    }
    /// <summary>
    /// Хаб для общего чата
    /// </summary>
    public class ChatHub : Hub
    {
        ApplicationDbContext db = new ApplicationDbContext();
        static List<User> userList = new List<User>();
        // Отправка сообщений
        
        public void Send(string name, string message, string partnerId, string toUserName, List<string> fileUrls)
        {

            MessageStoreViewModel messageStoreViewModel = new MessageStoreViewModel();
            MessageStoreViewModel messageStoreViewModelPartner = new MessageStoreViewModel();
            string reqId = Context.User.Identity.GetUserId();

            var uId = db.Users.Where(x => x.UserName == toUserName).FirstOrDefault().Id;
          
            //Clients.Client(partnerId).addMessage(name, message, partnerId);
            //Clients.Client(Context.ConnectionId).addMessage(name, message);
            int myDialogId = db.ChatDialogs.Where(i => i.UserFromId == reqId && i.UserToId == uId).FirstOrDefault().Id;
            var partnerDialog = db.ChatDialogs.Where(i => i.UserFromId == uId && i.UserToId == reqId).FirstOrDefault();
           
            if (partnerDialog == null)
            {
                partnerDialog = new ChatDialog();
                partnerDialog.Id = 3;
                partnerDialog.UserFromId = db.Users.Where(x => x.UserName == toUserName).FirstOrDefault().Id;
                partnerDialog.UserToId =  db.Users.Where(x => x.Id == reqId).FirstOrDefault().Id;
                //partnerDialog.Status = DialogStatus.Close;
                db.ChatDialogs.Add(partnerDialog);
                db.SaveChanges();
            }
            int partnerDialogId = partnerDialog.Id;

            messageStoreViewModel.UserFromId = db.Users.Where(x => x.Id == reqId).FirstOrDefault().Id;
            messageStoreViewModel.UserToId = db.Users.Where(x => x.UserName == toUserName).FirstOrDefault().Id;
            messageStoreViewModel.Description = message;
            messageStoreViewModel.DateSend = DateTime.Now;
            messageStoreViewModel.Status = MessageStatus.Reading;
            messageStoreViewModel.ChatDialogId = myDialogId;


            messageStoreViewModelPartner.Status = MessageStatus.Undreading;
            messageStoreViewModelPartner.UserFromId = messageStoreViewModel.UserFromId;
            messageStoreViewModelPartner.UserToId = messageStoreViewModel.UserToId;
            messageStoreViewModelPartner.Description = message;
            messageStoreViewModelPartner.DateSend = DateTime.Now;
            messageStoreViewModelPartner.MessageAttaches = messageStoreViewModel.MessageAttaches;

           

            
            var openDialog = db.ChatDialogs.Where(i => i.Id == partnerDialogId).FirstOrDefault();
            //if (openDialog.Status == DialogStatus.Open)
            //    messageStoreViewModelPartner.Status = MessageStatus.Reading;
            //else
            //    messageStoreViewModelPartner.Status = MessageStatus.Undreading;


            messageStoreViewModelPartner.ChatDialogId = partnerDialogId;

            var dateSend = messageStoreViewModel.DateSend.ToJavaScriptMilliseconds();
            Clients.User(uId).addMessage(name, message, dateSend, fileUrls);
            Clients.User(reqId).addMessage(name, message, dateSend, fileUrls);

            Clients.User(uId).displayUnreadCountMessage();
     
            db.Messages.Add(messageStoreViewModel);
            db.Messages.Add(messageStoreViewModelPartner);
            db.SaveChanges();

            if (fileUrls != null)
            {
                for (var i = 0; i < fileUrls.Count; i++)
                {
                    string url = fileUrls[i];
                    string nameAttach = Path.GetFileName(url);

                    var attach = new MessageAttach() { AttachName = nameAttach, AttachUrl = url, MessageStoreViewModelId = messageStoreViewModel.Id };
                    db.MessageAttaches.Add(attach);

                }

                db.SaveChanges();

                for (var i = fileUrls.Count - 1; i >= 0; i--)
                {
                    string url = fileUrls[i];
                    string nameAttach = Path.GetFileName(url);
                    var attach = new MessageAttach() { AttachName = nameAttach, AttachUrl = url, MessageStoreViewModelId = messageStoreViewModelPartner.Id };
                    db.MessageAttaches.Add(attach);
                }

                db.SaveChanges();
            }

            if (partnerId == null)
            {
                SendMessage("Новое сообщение...", partnerId);
            }
        }

        public void IsTyping(string html, string toUserName, string username)
        {
            SayWhoIsTyping(html, toUserName, username);
        }

        public void SayWhoIsTyping(string html, string toUserName, string username)
        {
            var uId = db.Users.Where(x => x.UserName == toUserName).FirstOrDefault().Id;
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            context.Clients.User(uId).sayWhoIsTyping(html, toUserName, username);
            // context.Clients.All.sayWhoIsTyping(html); 
        }

        // Подключение нового пользователя
        [System.Web.Http.Authorize]
        public void Connect()
        {
            string reqId = Context.User.Identity.GetUserId();
            // находим пользователя в БД по его почте
            User user = db.Users.Where(x => x.Id == reqId).FirstOrDefault();

            // берем айди клиента из контекста
            string id = Context.ConnectionId;
       
            // если такого айдишника нет в списке
            if (!db.Users.Any(x => x.ConnectionId == id) && user != null 
                && !userList.Any(x => x.ConnectionId == id))
            {
                db.Entry(user).State = EntityState.Modified;
                user.ConnectionId = id;
                user.IsOnline = true;
                db.SaveChanges();
                userList.Add(user);

                // Посылаем сообщение текущему пользователю
                Clients.Caller.onConnected(id, user.UserName, userList);
                
                // Посылаем сообщение всем пользователям, кроме текущего
                Clients.AllExcept(id).onNewUserConnected(id, user.UserName);
            }
        }

        private void SendMessage(string message, string partnerId)
        {
            string id = Context.ConnectionId;
            // Получаем контекст хаба
            // var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            // отправляем сообщение
            Clients.Client(partnerId).displayMessage(message, id);
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            string reqId = Context.User.Identity.GetUserId();
            User user = db.Users.Where(x => x.Id == reqId).FirstOrDefault();
            var item = userList.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                db.Entry(user).State = EntityState.Modified;
                user.IsOnline = false;
                db.SaveChanges();
                userList.Remove(item);
                var id = Context.ConnectionId;
                Clients.All.onUserDisconnected(id, user.UserName);
            }

            return base.OnDisconnected(stopCalled);
        }
        
    }
}