//
//  http://playentertainment.company
//  

using QFSW.QC;

using RNCryptor;

using System;
using System.Collections.Generic;
using System.Linq;
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
            this.GetContacts();
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

                    // foreach (Contact contact in this.contacts)
                    // {
                    //     Debug.Log(contact.alias);
                    // }
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

        [Command(".message")]
        public void SendMessage(string text, int chatIndex = -1)
        {
            if (chatIndex == -1) return;
            if (text == null) return;

            Chat chat = this.chats[chatIndex];

            this.SendMessage(text, null, 0, chat.id);
            // Debug.Log(chat.name);

            // Contact contact = this.contacts[chatIndex];

            // Debug.Log(contact.alias);
        }

        public void SendMessage(string content, string replyUuid, long contactId = 0, long chatId = 0, long amount = 0, long messagePrice = 0, bool boost = false)
        {
            string encrypted = this.encryptText(1, content);

            Debug.Log(encrypted);

            Dictionary<object, object> remote_text_map = Helpers.makeRemoteTextMap(content, contactId, chatId);

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("contact_id", contactId);

            parameters.Add("chat_id", null);
            if (chatId != 0)
            {
                parameters["chat_id"] = chatId;
            }

            parameters.Add("text", encrypted);

            parameters.Add("amount", null);
            if (amount != 0)
            {
                parameters["amount"] = chatId;
            }

            parameters.Add("reply_uuid", replyUuid);
            parameters.Add("boost", boost);

            if (messagePrice != 0) parameters.Add("message_price", messagePrice);

            if (chatId == 0)
            {
                Debug.Log("no chat id");

                Action<string, object, string, Action<string>> request = this.api.AddMethod("POST", this.api.url);
                request(string.Format("/messages"), parameters, null, delegate (string text)
                {
                    Debug.Log(text);

                    // Json_Contacts json = JsonUtility.FromJson<Json_Contacts>(text);

                    // if (json.success)
                    // {
                    //     this.contacts = json.response.contacts;
                    //     this.chats = json.response.chats;
                    //     this.subscriptions = json.response.subscriptions;

                    //     foreach (Contact contact in this.contacts)
                    //     {
                    //         Debug.Log(contact.alias);
                    //     }
                    // }
                });
            }
            else
            {

            }
        }

        string encryptText(long contactId, string text)
        {
            Contact contact = this.contacts.FirstOrDefault(i => i.id == contactId);

            if (contact == null) return string.Empty;

            string encrypted = Helpers.encryptPublic(text, contact.public_key);
            return encrypted;
        }
        #endregion
    }
}