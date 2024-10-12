using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventMangementSystem.Hubs
{
    public class ChatHub : Hub
    {
        public void Send(int eventId, string userName, string message)
        {
            Clients.Group(eventId.ToString()).broadcastMessage(userName, message, DateTime.Now.ToString("g"));
        }

        public void JoinEvent(int eventId)
        {
            Groups.Add(Context.ConnectionId, eventId.ToString());
        }
    }
}
