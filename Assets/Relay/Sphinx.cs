//
//  http://playentertainment.company
//  

using QFSW.QC;

using RNCryptor;

using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace PlayEntertainment.Sphinx
{
    enum STATE
    {
        NEVER,
        ONCE
    }

    [CommandPrefix("sphinx")]
    public class Sphinx : MonoBehaviour
    {
        public string restoreString = string.Empty;

        public static Sphinx Instance = null;

        public QuantumConsole quantum;

        STATE state = STATE.NEVER;

        public string code_Invite = string.Empty;
        public string code_Recovery = string.Empty;

        public Balance balance;

        public List<Contact> contacts;
        public List<Chat> chats;
        public List<Subscription> subscriptions;
        public List<Msg> messages;

        public long lastFetched = 0;

        public API api;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this.gameObject);
            }

            DontDestroyOnLoad(this.gameObject);
        }

        public void ProcessCode(string code, Action<bool, Action<string>> callback)
        {
            this.SetRecoveryCode(code);

            callback(true, delegate (string pin)
            {
                this.Recover(pin);
            });
        }

        #region - Recovery Flow -
        public void SetRecoveryCode(string code)
        {
            PlayerPrefs.SetString("recovery_code", code);

            this.code_Recovery = code;
        }

        void Recover(string pin)
        {
            byte[] decodedBytes = Convert.FromBase64String(this.code_Recovery);
            string decodedText = Encoding.UTF8.GetString(decodedBytes);

            string[] separatingStrings = { "::" };
            string[] words = decodedText.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);

            string encrypted = words[1];

            this.DecryptWithPin(encrypted, pin);
        }

        void DecryptWithPin(string encrypted, string pin)
        {
            var decryptor = new Decryptor();

            byte[] encryptedBytes = Convert.FromBase64String(encrypted);

            byte[] decryptedBytes = decryptor.Decrypt(encryptedBytes, pin);

            this.restoreString = Encoding.UTF8.GetString(decryptedBytes);

            this.Launch();
        }

        public void Launch()
        {
            this.Restore();
            this.GetBalance();
        }

        public string GetPrivateKey()
        {
            string[] separatingStrings = { "::" };
            string[] words = this.restoreString.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);

            string key = words[0];

            return key;
        }

        public string GetPublicKey()
        {
            string[] separatingStrings = { "::" };
            string[] words = this.restoreString.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);

            string key = words[1];

            return key;
        }

        public void Restore()
        {
            string[] separatingStrings = { "::" };
            string[] words = this.restoreString.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);

            string key = words[0];
            string ip = words[2];
            string token = words[3];

            this.api = new API(ip, "x-user-token", token, this, delegate ()
            {

            });
        }
        #endregion

        #region - Invite Flow -
        public void SetInviteCode(string code)
        {
            PlayerPrefs.SetString("invite_code", code);

            this.code_Invite = code;
        }

        public void Initialize()
        {
            this.code_Invite = PlayerPrefs.GetString("invite_code");

            byte[] decodedBytes = Convert.FromBase64String(this.code_Invite);
            string decodedText = Encoding.UTF8.GetString(decodedBytes);

            Debug.Log(decodedText);

            this.SignupWithIP(decodedText);
        }
        public void SignupWithIP(string code)
        {
            string[] separatingStrings = { "::" };
            string[] words = code.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);

            string ip = words[1];
            string password = words[2];

            Debug.Log(ip + " " + password);
            this.api = new API(ip, string.Empty, string.Empty, this, delegate ()
            {
                Debug.Log("A");
            });

            string token = this.GenerateToken(password);

            Action<string, object, string, Action<object>> request = this.api.AddMethod("POST", this.api.url);

            Dictionary<string, object> data = new Dictionary<string, object>();

            data.Add("token", token);

            request(string.Format("/contacts/tokens?pwd={0}", password), data, null, delegate (object result)
            {

            });
        }

        public string GenerateToken(string password)
        {
            Debug.Log("GenerateToken");

            if (this.api == null)
            {
                Debug.LogError("Need Api");
            }

            string token = this.RandomString(20);

            Debug.Log(token);

            return token;
        }

        public string RandomString(int length)
        {
            string randomString = "";

            const string glyphs = "abcdefghijklmnopqrstuvwxyz"; //add the characters you want

            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            int seed = (int)(DateTime.UtcNow - epochStart).TotalSeconds;

            UnityEngine.Random.InitState(seed);

            for (int i = 0; i < length; i++)
            {
                randomString += glyphs[UnityEngine.Random.Range(0, glyphs.Length)];
            }

            return randomString;
        }
        #endregion

        #region - Api - 

        [Command(".balance")]
        public void GetBalance()
        {
            Action<string, object, string, Action<string>> request = this.api.AddMethod("GET", this.api.url);
            request(string.Format("/balance"), null, null, delegate (string text)
            {
                Json_Balance json = JsonUtility.FromJson<Json_Balance>(text);

                if (json.success)
                {
                    this.balance = json.response;

                    if (this.state == STATE.NEVER)
                    {
                        Sphinx.Instance.quantum.gameObject.SetActive(true);
                        PlayerPrefs.SetString("restore_string", this.restoreString);
                        this.state = STATE.ONCE;
                    }

                    Debug.Log(this.balance.balance);
                }
            });
        }

        [Command(".contacts")]
        public void GetContacts()
        {
            Action<string, object, string, Action<string>> request = this.api.AddMethod("GET", this.api.url);
            request(string.Format("/contacts"), null, null, delegate (string text)
            {
                Json_Contacts json = JsonUtility.FromJson<Json_Contacts>(text);

                if (json.success)
                {
                    this.contacts = json.response.contacts;
                    this.chats = json.response.chats;
                    this.subscriptions = json.response.subscriptions;

                    foreach (Contact contact in this.contacts)
                    {
                        Debug.Log(contact.alias);
                    }
                }
            });
        }

        [Command(".chats")]
        public void GetChats()
        {
            foreach (Chat chat in this.chats)
            {
                Debug.Log(chat.name);
            }
        }

        // Messages
        [Command(".messages")]
        public void GetMessages()
        {
            this.RestoreMessages();
        }

        void RestoreMessages()
        {
            bool done = false;
            int offset = 0;
            int limit = 20;

            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            string param_Date = dateTime.ToString("yyyy-MM-dd\\%20HH:mm:ss");

            Action<string, object, string, Action<string>> request = this.api.AddMethod("GET", this.api.url);
            request(string.Format("/msgs?limit={0}&offset={1}&date={2}", limit, offset, param_Date), null, null, delegate (string text)
            {
                done = true;

                Json_Messages json = JsonUtility.FromJson<Json_Messages>(text);

                if (json.success)
                {
                    this.messages = json.response.new_messages;

                    if (this.messages.Count <= 0) return;

                    while (!done)
                    {

                    }

                    DateTime now = DateTime.Now;
                    this.lastFetched = ((DateTimeOffset)now).ToUnixTimeSeconds();

                    this.messages = Helpers.decodeMessages(this.messages);

                    foreach (Msg message in this.messages)
                    {
                        if (message.message_content != null)
                        {
                            Debug.Log(message.message_content);
                        }
                    }
                }
            });
        }

        // Messages
        [Command(".crypto")]
        public void Crypto()
        {
            string publicKey = @"-----BEGIN PUBLIC KEY-----
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCzVBO/HUhgU4cIRS2UE255r2uG
EaVuPAxrANab5z7rv/hUm1t1TW9G6qaLvXraUS2c6m4PW+VVY8j/fViIy9XLhd2I
dYsbuTNyV6gQVnA4tdMdnJdrvfzaXiIoPzP3u9Ll8LEQSW2iiludxwBlVz/VdCiA
EYBMuYmrmSHsan5ObQIDAQAB
-----END PUBLIC KEY-----";

            string privateKey = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQCzVBO/HUhgU4cIRS2UE255r2uGEaVuPAxrANab5z7rv/hUm1t1
TW9G6qaLvXraUS2c6m4PW+VVY8j/fViIy9XLhd2IdYsbuTNyV6gQVnA4tdMdnJdr
vfzaXiIoPzP3u9Ll8LEQSW2iiludxwBlVz/VdCiAEYBMuYmrmSHsan5ObQIDAQAB
AoGBAIHSy1zfQSdjMO2ez0lU6/SyN0BfBAmS9VZ9y+AgACBB4PC3a/W28mk/tQST
Tx5ACKqB2N3LpHI2BCxaPT8DeilfjaUibpOqXJ918+oXmOEpEBpEz2FzkzZWeUOo
8bkDiuEE1RyzQEQExxCbiLQFCpX0NNIpccrTYJ3wRZfroojNAkEA4Pd9xxscmxQM
s4aQgE+/paQWR//B2JQjhsxvYfLhrMUKgWMWpfm5EfhG3AlIE/N7iZADPLZ+2JMG
2ljnE3VdzwJBAMwQ6DJpueg2Yj5+ufTVhFBIkJZXWGIZv5FWmzaycfJOafg/ToRB
QWjU7Dr7buxQ4jgFI6eZcN8uM5pcer/ouwMCQDRCSb2O1r5PkgPCJp8n52UbEPH4
v5cIEpiltNoUCciQnTghRImZ0RwTiKJkpZG85d22zomz+xNkVBs0u7kRcpECQAfH
NTKGuSFSwVfkeK4OXWa5/Vjdp27F0Hl3tZ7WGmXD+2IM968u1ZFrXD27S7USOC0u
dPd0b8rx9eGSWNNryYUCQQCWbiNhKEDQ1+yauWhZwamsc2Zl/Gde0eQYrWlmoZRE
r4w8Xlt8XVBTNBf5ljALeVtXOs0LNuWqnmy1v7wMPvnN
-----END RSA PRIVATE KEY-----";

            string text = RSAHelper.Encrypt("Hello Rocky", publicKey);

            var data = RSAHelper.Decrypt(text.Trim(), privateKey);
            Debug.Log(data);

        }
        #endregion
    }
}