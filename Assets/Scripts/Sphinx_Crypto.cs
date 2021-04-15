using System.Diagnostics;
//
//  http://playentertainment.company
//  


namespace PlayEntertainment.Sphinx
{
    public class Crypto
    {
        static public string Encrypt(string text, string pemPublicKey)
        {
            return RSAHelper.Encrypt(text, pemPublicKey);
        }

        static public string Decrypt(string encrypted, string pemPrivateKey)
        {
            return RSAHelper.Decrypt(encrypted, pemPrivateKey);
        }
    }
}