using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesLib
{
    public static class CryptUtility
    {
        private static string _aesIv = CryptResource.AesIV;
        private static string _aesKey = CryptResource.AesKey;

        /// <summary>
        /// 文字列をAESで暗号化
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] EncryptString(string text)
        {
            // AES暗号化サービスプロバイダ
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 128;
            aes.IV = Encoding.UTF8.GetBytes(_aesIv);
            aes.Key = Encoding.UTF8.GetBytes(_aesKey);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;



            if (string.IsNullOrEmpty(text))
            {
                return (byte[])Enumerable.Empty<byte>();
            }

            // 文字列をバイト型配列に変換
            byte[] src = Encoding.Unicode.GetBytes(text);

            // 暗号化する
            using (ICryptoTransform encrypt = aes.CreateEncryptor())
            {
                byte[] dest = encrypt.TransformFinalBlock(src, 0, src.Length);

                // バイト型配列からBase64形式の文字列に変換
                return dest;
            }
        }

        /// <summary>
        /// 文字列をAESで復号化
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string DecryptString(byte[] src)
        {
            // AES暗号化サービスプロバイダ
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 128;
            aes.IV = Encoding.UTF8.GetBytes(_aesIv);
            aes.Key = Encoding.UTF8.GetBytes(_aesKey);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Base64形式の文字列からバイト型配列に変換
            //byte[] src = System.Convert.FromBase64String(text);

            if(src.Length == 0)
            {
                return "";
            }

            // 複号化する
            using (ICryptoTransform decrypt = aes.CreateDecryptor())
            {
                try
                {
                    byte[] dest = decrypt.TransformFinalBlock(src, 0, src.Length);
                    return Encoding.Unicode.GetString(dest);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
