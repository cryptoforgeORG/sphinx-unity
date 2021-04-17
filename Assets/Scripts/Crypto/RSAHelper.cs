//
//  http://playentertainment.company
//  

using System.Security.Cryptography;
using System.Text;
using System;

public class RSAHelper
{
    public static string Encrypt(string value, string pemPublicKey)
    {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportParameters(ConvertFromPemPublicKey(pemPublicKey));
            var encryptedBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(value), false);
            return Convert.ToBase64String(encryptedBytes);
        }
    }

    public static string Decrypt(string encryptedData, string pemPrivateKey)
    {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportParameters(ConvertFromPemPrivateKey(pemPrivateKey));
            var data = rsa.Decrypt(Convert.FromBase64String(encryptedData), false);
            return Encoding.UTF8.GetString(data, 0, data.Length);
        }
    }

    // http://blog.csdn.net/liguo9860/article/details/40922919
    //
    public static RSAParameters ConvertFromPemPublicKey(string pemFileConent)
    {
        pemFileConent = pemFileConent.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Replace("\n", "").Replace("\r", "");
        byte[] keyData = Convert.FromBase64String(pemFileConent);
        bool keySize1024 = (keyData.Length == 162);
        bool keySize2048 = (keyData.Length == 270 || keyData.Length == 294);

        if (!(keySize1024 || keySize2048))
        {
            throw new ArgumentException("pem file content is incorrect, Only support the key size is 1024 or 2048");
        }
        byte[] modulusData = (keySize1024 ? new byte[128] : new byte[256]);
        var exponentData = new byte[3];

        //Array.Copy(keyData, (keySize1024 ? 29 : 33), modulusData, 0, (keySize1024 ? 128 : 256));
        //Array.Copy(keyData, (keySize1024 ? 159 : 291), exponentData, 0, 3);

        Array.Copy(keyData, keyData.Length - exponentData.Length, exponentData, 0, exponentData.Length);
        Array.Copy(keyData, 9, modulusData, 0, modulusData.Length);

        var para = new RSAParameters { Modulus = modulusData, Exponent = exponentData };
        return para;
    }

    // http://blog.csdn.net/liguo9860/article/details/40922919
    //
    public static RSAParameters ConvertFromPemPrivateKey(string pemFileConent)
    {
        if (string.IsNullOrEmpty(pemFileConent))
        {
            throw new ArgumentNullException("pemFileConent", "This arg cann't be empty.");
        }

        pemFileConent = pemFileConent.Replace("-----BEGIN RSA PRIVATE KEY-----", "").Replace("-----END RSA PRIVATE KEY-----", "").Replace("\n", "").Replace("\r", "");

        byte[] keyData = Convert.FromBase64String(pemFileConent);
        bool keySize1024 = (keyData.Length == 609 || keyData.Length == 610);
        bool keySize2048 = (keyData.Length == 1190 || keyData.Length == 1191 || keyData.Length == 1192);

        if (!(keySize1024 || keySize2048))
        {
            throw new ArgumentException("pem file content is incorrect, Only support the key size is 1024 or 2048");
        }

        int index = (keySize1024 ? 11 : 12);
        byte[] pemModulus = (keySize1024 ? new byte[128] : new byte[256]);
        Array.Copy(keyData, index, pemModulus, 0, pemModulus.Length);

        index += pemModulus.Length;
        index += 2;
        var pemPublicExponent = new byte[3];
        Array.Copy(keyData, index, pemPublicExponent, 0, 3);

        index += 3;
        index += 4;
        if (keyData[index] == 0)
        {
            index++;
        }
        byte[] pemPrivateExponent = (keySize1024 ? new byte[128] : new byte[256]);
        Array.Copy(keyData, index, pemPrivateExponent, 0, pemPrivateExponent.Length);

        index += pemPrivateExponent.Length;
        index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
        byte[] pemPrime1 = (keySize1024 ? new byte[64] : new byte[128]);
        Array.Copy(keyData, index, pemPrime1, 0, pemPrime1.Length);

        index += pemPrime1.Length;
        index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
        byte[] pemPrime2 = (keySize1024 ? new byte[64] : new byte[128]);
        Array.Copy(keyData, index, pemPrime2, 0, pemPrime2.Length);

        index += pemPrime2.Length;
        index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
        byte[] pemExponent1 = (keySize1024 ? new byte[64] : new byte[128]);
        Array.Copy(keyData, index, pemExponent1, 0, pemExponent1.Length);

        index += pemExponent1.Length;
        index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
        byte[] pemExponent2 = (keySize1024 ? new byte[64] : new byte[128]);
        Array.Copy(keyData, index, pemExponent2, 0, pemExponent2.Length);

        index += pemExponent2.Length;
        index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
        byte[] pemCoefficient = (keySize1024 ? new byte[64] : new byte[128]);
        Array.Copy(keyData, index, pemCoefficient, 0, pemCoefficient.Length);

        var para = new RSAParameters
        {
            Modulus = pemModulus,
            Exponent = pemPublicExponent,
            D = pemPrivateExponent,
            P = pemPrime1,
            Q = pemPrime2,
            DP = pemExponent1,
            DQ = pemExponent2,
            InverseQ = pemCoefficient
        };
        return para;
    }
}