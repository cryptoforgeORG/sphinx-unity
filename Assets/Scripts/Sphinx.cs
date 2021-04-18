using System.Net.Mail;
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

        public Contact me;

        public Dictionary<long, List<Msg>> messages;

        public long lastFetched = 0;

        public API api_Relay;
        public API api_Memes;

        public Retina retina;
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

            // PlayerPrefs.DeleteAll();
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
            this.Recover();
            this.GetBalance();
            this.GetContacts();

            this.Authenticate();

            this.Restore();
        }

        public void Authenticate()
        {
            // Authenticate meme server
            Server server = new Server();

            server.host = "memes-staging.n2n2.chat";
            server.token = "";


            string publicKey = me.public_key;

            this.api_Memes = new API(string.Format("https://{0}", server.host), string.Empty, string.Empty, this, null);

            Action<string, object, string, Action<string>> request_Signer = this.api_Relay.AddMethod("GET", this.api_Relay.url);

            Action<string, object, string, Action<string>> request_Verify = this.api_Memes.AddMethod("POST", this.api_Memes.url);

            Action<string, object, string, Action<string>> request = this.api_Memes.AddMethod("GET", this.api_Memes.url);

            request(string.Format("/ask"), null, null, delegate (string text)
                {
                    Json_Ask json = JsonUtility.FromJson<Json_Ask>(text);
                    string challenge = json.challenge;
                    string id = json.id;

                    Action<string> lambda = delegate (string text)
                    {
                        Json_Signer json = JsonUtility.FromJson<Json_Signer>(text);

                        if (json.success)
                        {
                            Dictionary<string, object> data = new Dictionary<string, object>();

                            data.Add("id", id);
                            data.Add("sig", json.response.sig);
                            data.Add("pubkey", this.me.public_key);

                            request_Verify(string.Format("/verify"), data, "application/x-www-form-urlencoded", delegate (string text)
                            {
                                Json_Verify json = JsonUtility.FromJson<Json_Verify>(text);
                                Debug.Log(json.token);
                            });
                        }
                    };

                    request_Signer(string.Format("/signer/{0}", challenge), null, null, lambda);
                });
            //
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

        public void Recover()
        {
            string[] separatingStrings = { "::" };
            string[] words = this.restoreString.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);

            string key = words[0];
            string ip = words[2];
            string token = words[3];

            this.api_Relay = new API(ip, "x-user-token", token, this, delegate ()
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

            this.SignupWithIP(decodedText);
        }
        public void SignupWithIP(string code)
        {
            string[] separatingStrings = { "::" };
            string[] words = code.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);

            string ip = words[1];
            string password = words[2];

            this.api_Relay = new API(ip, string.Empty, string.Empty, this, delegate ()
            {

            });

            string token = this.GenerateToken(password);

            Action<string, object, string, Action<object>> request = this.api_Relay.AddMethod("POST", this.api_Relay.url);

            Dictionary<string, object> data = new Dictionary<string, object>();

            data.Add("token", token);

            request(string.Format("/contacts/tokens?pwd={0}", password), data, null, delegate (object result)
            {

            });
        }

        public string GenerateToken(string password)
        {
            if (this.api_Relay == null)
            {
                Debug.LogError("Need Api");
            }

            string token = this.RandomString(20);

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
            Action<string, object, string, Action<string>> request = this.api_Relay.AddMethod("GET", this.api_Relay.url);
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

                    this.quantum.LogToConsole(string.Format("Balance: " + this.balance.balance));
                }
            });
        }

        [Command(".contacts")]
        public void GetContacts()
        {
            Action<string, object, string, Action<string>> request = this.api_Relay.AddMethod("GET", this.api_Relay.url);
            request(string.Format("/contacts"), null, null, delegate (string text)
            {
                Json_Contacts json = JsonUtility.FromJson<Json_Contacts>(text);

                if (json.success)
                {
                    this.contacts = json.response.contacts;
                    this.chats = json.response.chats;
                    this.subscriptions = json.response.subscriptions;


                    this.me = this.contacts.FirstOrDefault(c => c.id == 1);
                }
            });
        }

        [Command(".tribes")]
        public void GetChats()
        {
            foreach (Chat chat in this.chats)
            {
                Debug.Log(chat.id + " " + chat.uuid + " " + chat.name);
            }
        }

        // Messages
        [Command(".restore")]
        public void Restore()
        {
            this.RestoreMessages();
        }

        void RestoreMessages()
        {
            int offset = 0;
            int limit = 200;
            int MAX_MSGS_RESTORE = 5000;

            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            string param_Date = dateTime.ToString("yyyy-MM-dd\\%20HH:mm:ss");

            Dictionary<long, List<Msg>> all = new Dictionary<long, List<Msg>>();
            Action<string, object, string, Action<string>> request = this.api_Relay.AddMethod("GET", this.api_Relay.url);

            Action<string> lambda = null;

            lambda = delegate (string text)
            {
                Json_Messages json = JsonUtility.FromJson<Json_Messages>(text);

                if (json.success)
                {
                    if (json.response.new_messages.Count > 0)
                    {
                        List<Msg> decodedMessages = Helpers.decodeMessages(json.response.new_messages);

                        all = Helpers.orgMsgsFromExisting(all, decodedMessages);

                        if (json.response.new_messages.Count < 200)
                        {
                            this.FinishRestore(all);
                            return;
                        }
                    }
                    else
                    {
                        this.FinishRestore(all);
                        return;
                    }

                    offset += 200;
                    if (offset >= MAX_MSGS_RESTORE)
                    {
                        this.FinishRestore(all);
                        return;
                    }
                    else
                    {
                        request(string.Format("/msgs?limit={0}&offset={1}&date={2}", limit, offset, param_Date), null, null, lambda);
                    }
                }

                // this.FinishRestore(all);
            };

            request(string.Format("/msgs?limit={0}&offset={1}&date={2}", limit, offset, param_Date), null, null, lambda);
        }

        public void FinishRestore(Dictionary<long, List<Msg>> all)
        {
            Debug.Log("restore_done");

            Helpers.sortAllMsgs(ref all);

            this.messages = all;

            DateTime now = DateTime.Now;
            this.lastFetched = ((DateTimeOffset)now).ToUnixTimeSeconds();

            List<Msg> attachments = new List<Msg>();

            // Output
            foreach (KeyValuePair<long, List<Msg>> entry in this.messages)
            {
                this.quantum.LogToConsole("chatId: " + entry.Key);

                if (entry.Key != 3) continue;

                foreach (Msg message in entry.Value)
                {
                    if (message.type == (int)MESSAGE_TYPE.attachment)
                    {
                        // Debug.Log(JsonUtility.ToJson(message).ToString());
                        this.quantum.LogToConsole("id: " + message.id);
                        this.quantum.LogToConsole(JsonUtility.ToJson(Helpers.parseLDAT(message.media_token)).ToString());

                        attachments.Add(message);
                    }
                }

                this.quantum.LogToConsole("===");
                this.quantum.LogToConsole("===");
                this.quantum.LogToConsole("===");

                Msg value = entry.Value.Last();

                if (value.type == (int)MESSAGE_TYPE.attachment)
                {
                    LDAT ldat = Helpers.parseLDAT(value.media_token);
                    this.quantum.LogToConsole(JsonUtility.ToJson(ldat).ToString());
                }

                this.quantum.Deactivate();
                this.retina.Show();
                this.retina.AddToFeed(attachments);
            }
        }

        [Command(".message.tribe")]
        public void SendMessage_Tribe(string text, int chatId = -0)
        {
            if (chatId == 0) return;
            if (text == null) return;

            // Chat chat = this.chats[chatIndex];

            Chat chat = this.chats.FirstOrDefault(c => c.id == chatId);

            if (chat == null) return;

            this.SendMessage(text, null, 0, chat.id);

            Debug.Log(chat.name);

            // Contact contact = this.contacts[chatIndex];

            // Debug.Log(contact.alias);
        }

        [Command(".message.contact")]
        public void SendMessage_Contact(string text, int chatId = -0)
        {
            if (chatId == 0) return;
            if (text == null) return;

            // Chat chat = this.chats[chatIndex];

            Chat chat = this.chats.FirstOrDefault(c => c.id == chatId);

            if (chat == null) return;

            this.SendMessage(text, null, 0, chat.id);

            Debug.Log(chat.name);

            // Contact contact = this.contacts[chatIndex];

            // Debug.Log(contact.alias);
        }

        public void SendMessage(string content, string replyUuid, long contactId = 0, long chatId = 0, long amount = 0, long messagePrice = 0, bool boost = false)
        {
            string encrypted = this.encryptText(1, content);

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
                parameters["amount"] = amount;
            }

            parameters.Add("reply_uuid", replyUuid);
            parameters.Add("boost", boost);

            if (messagePrice != 0) parameters.Add("message_price", messagePrice);

            if (chatId == 0)
            {
                Debug.Log("no chat id");

                Action<string, object, string, Action<string>> request = this.api_Relay.AddMethod("POST", this.api_Relay.url);
                request(string.Format("/messages"), parameters, null, delegate (string text)
                {
                    Json_Message json = JsonUtility.FromJson<Json_Message>(text);

                    if (json.success)
                    {
                        Debug.Log("id: " + json.response.id);
                    }
                });
            }
            else
            {
                int type = boost ? (int)MESSAGE_TYPE.boost : (int)MESSAGE_TYPE.message;
                amount = boost && messagePrice != 0 && messagePrice < amount ? amount - messagePrice : amount;

                // this.Insert(this.messages, null, chatId);

                Action<string, object, string, Action<string>> request = this.api_Relay.AddMethod("POST", this.api_Relay.url);
                request(string.Format("/messages"), parameters, null, delegate (string text)
                {
                    Json_Message json = JsonUtility.FromJson<Json_Message>(text);

                    if (json.success)
                    {
                        Debug.Log("id: " + json.response.id);
                    }
                });

                // Deduct balance
            }
        }

        void Insert(List<Msg> pool, Msg message, long chatId = 0)
        {
            // TBD
        }

        string encryptText(long contactId, string text)
        {
            Contact contact = this.contacts.FirstOrDefault(i => i.id == contactId);

            if (contact == null) return string.Empty;

            string encrypted = Helpers.encryptPublic(text, contact.contact_key);
            return encrypted;
        }
        #endregion
    }
}