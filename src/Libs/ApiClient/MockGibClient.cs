using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtNotif.Libs.ApiClient
{
    public class MockGibClient : IGibClient
    {
        public Task<bool> AuthenticateAsync(string vkn, string password)
        {
            // Demo: her zaman success (gerçekte API hit edilecek)
            return Task.FromResult(true);
        }

        public Task<List<Notification>> GetNotificationsAsync(string vkn)
        {
            var now = DateTime.Now;
            var list = new List<Notification>
            {
                new Notification(Guid.NewGuid().ToString(), "E-Tebligat Örneği", "Bu bir demo bildiridir.", now.AddDays(-1)),
                new Notification(Guid.NewGuid().ToString(), "Vergi Bildirimi", "İlgili duyuru içeriği.", now)
            };
            return Task.FromResult(list);
        }
    }
}
