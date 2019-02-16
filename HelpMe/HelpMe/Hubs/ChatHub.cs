using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
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
            var userId = db.Users.Where(n => n.UserName == request.User.Identity.Name).FirstOrDefault().Id;
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
        
        public void Send(string name, string message, string partnerId, string toUserName)
        {
            MessageStoreViewModel messageStoreViewModel = new MessageStoreViewModel();
            MessageStoreViewModel messageStoreViewModelPartner = new MessageStoreViewModel();
            string reqId = Context.User.Identity.GetUserId();

            var uId = db.Users.Where(x => x.UserName == toUserName).FirstOrDefault().Id;
            Clients.User(uId).addMessage(name, message, partnerId);
            Clients.User(reqId).addMessage(name, message);
            //Clients.Client(partnerId).addMessage(name, message, partnerId);
            //Clients.Client(Context.ConnectionId).addMessage(name, message);
            int myDialogId = db.ChatDialogs.Where(i => i.UserFromId == reqId && i.UserToId == uId).FirstOrDefault().Id;
            int partnerDialogId = db.ChatDialogs.Where(i => i.UserFromId == uId && i.UserToId == reqId).FirstOrDefault().Id;

            messageStoreViewModel.UserFromId = db.Users.Where(x => x.Id == reqId).FirstOrDefault().Id;
            messageStoreViewModel.UserToId = db.Users.Where(x => x.UserName == toUserName).FirstOrDefault().Id;
            messageStoreViewModel.Description = message;
            messageStoreViewModel.DateSend = DateTime.Now;
            messageStoreViewModel.Status = MessageStatus.Reading;
            messageStoreViewModel.ChatDialogId = myDialogId;

            messageStoreViewModelPartner.UserFromId = db.Users.Where(x => x.Id == reqId).FirstOrDefault().Id;
            messageStoreViewModelPartner.UserToId = db.Users.Where(x => x.UserName == toUserName).FirstOrDefault().Id;
            messageStoreViewModelPartner.Description = message;
            messageStoreViewModelPartner.DateSend = DateTime.Now;
            messageStoreViewModelPartner.Status = MessageStatus.Undreading;
            messageStoreViewModelPartner.ChatDialogId = partnerDialogId;

            db.Messages.Add(messageStoreViewModel);
            db.Messages.Add(messageStoreViewModelPartner);
            db.SaveChanges();
            if (partnerId == null)
            {
                SendMessage("Новое сообщение...", partnerId);
            }
        }

        public void IsTyping(string html)
        {
            SayWhoIsTyping(html);
        }

        public void SayWhoIsTyping(string html)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            context.Clients.All.sayWhoIsTyping(html);
            
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
                userList.Remove(item);
                var id = Context.ConnectionId;
                Clients.All.onUserDisconnected(id, user.UserName);
            }

            return base.OnDisconnected(stopCalled);
        }
        
    }
}