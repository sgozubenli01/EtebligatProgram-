using System;
using System.Security.Cryptography;
using System.Text;

namespace EtNotif.Libs.Security
{
    public static class CryptoHelper
    {
        // CurrentUser scope. Production: Vault/HSM önerilir.
        public static string ProtectToBase64(string plainText)
        {
            if (plainText == null) return null;
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedBytes);
        }

        public static string UnprotectFromBase64(string protectedBase64)
        {
            if (protectedBase64 == null) return null;
            var protectedBytes = Convert.FromBase64String(protectedBase64);
            var bytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
