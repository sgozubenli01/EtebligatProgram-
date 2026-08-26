using System;

namespace EtNotif.Libs.Data
{
    public class Taxpayer
    {
        public int Id { get; set; }
        public string Vkn { get; set; }            // Vergi Kimlik No / VKN
        public string DisplayName { get; set; }
        public string EncryptedPassword { get; set; } // Protected via DPAPI (base64)
        public DateTime? LastCheckedAt { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
