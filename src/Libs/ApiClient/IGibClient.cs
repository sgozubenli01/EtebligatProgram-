using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtNotif.Libs.ApiClient
{
    public record Notification(string Id, string Title, string Content, System.DateTime Date);

    public interface IGibClient
    {
        Task<bool> AuthenticateAsync(string vkn, string password);
        Task<List<Notification>> GetNotificationsAsync(string vkn);
    }
}
