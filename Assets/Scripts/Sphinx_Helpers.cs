using System.Web;
//
//  http://playentertainment.company
//  

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace PlayEntertainment.Sphinx
{
    public static class Extensions
    {
        public static T[] SubArray<T>(this T[] array, int offset, int length)
        {
            T[] result = new T[length];
            Array.Copy(array, offset, result, 0, length);
            return result;
        }
        public static List<T> GetClone<T>(this List<T> source)
        {
            return source.GetRange(0, source.Count);
        }
    }

    enum CHAT_TYPE
    {
        conversation = 0,
        group = 1,
        tribe = 2
    }

    enum MESSAGE_TYPE
    {
        message = 0,
        confirmation = 1,
        invoice = 2,
        payment = 3,
        cancellation = 4,
        direct_payment = 5,
        attachment = 6,
        purchase = 7,
        purchase_accept = 8,
        purchase_deny = 9,
        contact_key = 10,
        contact_key_confirmation = 11,
        group_create = 12,
        group_invite = 13,
        group_join = 14,
        group_leave = 15,
        group_kick = 16,
        delete = 17,
        repayment = 18,
        member_request = 19,
        member_approve = 20,
        member_reject = 21,
        tribe_delete = 22,
        bot_install = 23,
        bot_cmd = 24,
        bot_res = 25,
        keysend = 28,
        boost = 29
    }

    class Helpers
    {
        static public List<Msg> decodeMessages(List<Msg> messages)
        {
            List<Msg> buffer = new List<Msg>();

            int[] typesToDecrypt = { (int)MESSAGE_TYPE.message, (int)MESSAGE_TYPE.invoice, (int)MESSAGE_TYPE.attachment, (int)MESSAGE_TYPE.purchase, (int)MESSAGE_TYPE.purchase_accept, (int)MESSAGE_TYPE.purchase_deny, (int)MESSAGE_TYPE.bot_res, (int)MESSAGE_TYPE.boost };

            foreach (Msg message in messages)
            {
                int pos = Array.IndexOf(typesToDecrypt, message.type);
                if (pos > -1)
                {
                    Msg temp = Helpers.decodeSingle(message);
                    buffer.Add(temp);
                }
                else
                {
                    buffer.Add(message);
                }
            }

            return buffer;
        }

        static public Dictionary<long, List<Msg>> orgMsgsFromExisting(Dictionary<long, List<Msg>> all, List<Msg> messages)
        {
            var buffer = all.ToDictionary(c => c.Key, c => c.Value.ToList());

            foreach (Msg message in messages)
            {
                Helpers.Combine(ref buffer, message, message.chat_id);
            }

            return buffer;
        }

        static public void sortAllMsgs(ref Dictionary<long, List<Msg>> all)
        {
            var buffer = all.ToDictionary(c => c.Key, c => c.Value.ToList());
            foreach (KeyValuePair<long, List<Msg>> entry in buffer)
            {
                all[entry.Key] = entry.Value.OrderBy(l => l.date).ToList();
            }
        }

        static public void Combine(ref Dictionary<long, List<Msg>> combined, Msg message, long chatId)
        {
            if (combined.ContainsKey(chatId))
            {
                int index = combined[chatId].FindIndex(m => m.id == message.id);

                if (index == -1)
                {
                    combined[chatId].Insert(0, Helpers.skinny(message));

                    int MAX_MSGS_PER_CHAT = 100;

                    if (combined[chatId].Count > MAX_MSGS_PER_CHAT)
                    {
                        combined[chatId].Remove(combined[chatId].Last());
                    }
                }
                else
                {
                    combined[chatId][index] = Helpers.skinny(message);
                }
            }
            else
            {
                combined[chatId] = new List<Msg>() {
                    Helpers.skinny(message)
                };
            }
        }

        static public Msg skinny(Msg message)
        {

            return message;
        }

        static public string decryptPrivate(string encrypted)
        {
            // int KEY_SIZE = 2048;
            // string KEY_TAG = "sphinx";

            int BLOCK_SIZE = 256;
            int MAX_CHUNK_SIZE = BLOCK_SIZE - 11; // 11 is the PCKS1 padding

            byte[] encryptedBytes = Convert.FromBase64String(encrypted);

            int n = (int)Math.Ceiling((double)encryptedBytes.Length / BLOCK_SIZE);

            int[] _ = new int[n];
            _ = _.Select(i => 0).ToArray();

            List<string> blocks = new List<string>();

            for (int i = 0; i < _.Length; i += 1)
            {
                byte[] buffer = encryptedBytes.SubArray(i * BLOCK_SIZE, BLOCK_SIZE);
                blocks.Add(Convert.ToBase64String(buffer));
            }

            string result = string.Empty;

            foreach (string block in blocks)
            {
                result += Crypto.Decrypt(block, Sphinx.Instance.GetPrivateKey().Trim());
            }

            return result;
        }
        static public Msg decodeSingle(Msg message)
        {
            if (message.type == (int)MESSAGE_TYPE.keysend)
            {
                return message;
            }

            if (message.message_content != null)
            {
                string content = Helpers.decryptPrivate(message.message_content);
                message.message_content = content;
            }

            if (message.media_key != null)
            {
                string media_key = Helpers.decryptPrivate(message.media_key);
                message.media_key = media_key;
            }

            return message;
        }
        static public string encryptPublic(string text, string publicKey)
        {
            string encrypted = Crypto.Encrypt(text, publicKey);
            return encrypted;
        }


        // const termKeys = [{
        //   key:'host',
        //   func: buf=> buf.toString('ascii')
        // },{
        //   key:'muid',
        //   func: buf=> urlBase64(buf)
        // },{
        //   key:'pubkey',
        //   func: buf=> buf.toString('hex')
        // },{
        //   key:'ts',
        //   func: buf=> parseInt('0x' + buf.toString('hex'))
        // },{
        //   key:'meta',
        //   func: buf=> {
        //     const ascii = buf.toString('ascii')
        //     return ascii?deserializeMeta(ascii):{} // parse this
        //   }
        // },{
        //   key:'sig',
        //   func: buf=> urlBase64(buf)
        // }]

        // export function parseLDAT(ldat){
        //   const a = ldat.split('.')
        //   const o: {[k:string]:any} = {}
        //   termKeys.forEach((t,i)=>{
        //     if(a[i]) o[t.key] = t.func(Buffer.from(a[i], 'base64'))
        //   })
        //   return o
        // }

        public static string Parse(string text)
        {
            text = text.Replace('_', '/').Replace('-', '+');
            switch (text.Length % 4)
            {
                case 2:
                    text += "==";
                    break;
                case 3:
                    text += "=";
                    break;
            }
            return text;
        }

        static public LDAT parseLDAT(string text)
        {
            string[] keys = new string[] { "host", "muid", "pubkey", "ts", "meta", "sig" };

            string[] words = text.Split('.');

            LDAT ldat = new LDAT();

            for (int i = 0; i < 6; i += 1)
            {
                string key = keys[i];

                if (i == words.Length) return ldat;

                string word = Helpers.Parse(words[i]);

                if (key == "ts")
                {
                    // issue parsing the word
                    continue;
                }

                if (word.Length == 0)
                {
                    continue;
                }

                byte[] bytes = Convert.FromBase64String(word);

                switch (key)
                {
                    case "host":
                        ldat.host = Encoding.UTF8.GetString(bytes);
                        break;
                    case "muid":
                        ldat.muid = urlBase64(bytes);
                        break;
                    case "pubkey":
                        ldat.pubkey = BitConverter.ToString(bytes).Replace("-", "");
                        break;
                    case "ts":
                        string hex = "0x" + BitConverter.ToString(bytes).Replace("-", "");
                        ldat.ts = Convert.ToInt16(hex);
                        break;
                    case "meta":
                        string ascii = Encoding.UTF8.GetString(bytes);
                        ldat.meta = Helpers.deserializeMeta(ascii);
                        break;
                    case "sig":
                        ldat.sig = urlBase64(bytes);
                        break;
                }
            }

            return ldat;
        }

        static string urlBase64(byte[] bytes)
        {
            string temp = Convert.ToBase64String(bytes);
            string output = temp.Replace("/", "_").Replace("+", "-");
            return output;
        }

        public static Dictionary<string, string> ParseQueryString(string queryString)
        {
            var nvc = HttpUtility.ParseQueryString(queryString);
            return nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
        }

        static LDAT_Meta deserializeMeta(string text)
        {
            if (text.Length <= 2)
                return null;

            Dictionary<string, string> meta = Helpers.ParseQueryString(text);

            LDAT_Meta output = new LDAT_Meta();

            output.amount = meta.ContainsKey("amt") ? long.Parse(meta["amt"]) : 0;
            output.ttl = meta.ContainsKey("ttl") ? (output.ttl.Equals("undefined") ? 0 : 0) : 0;

            return output;
        }

        static public Dictionary<object, object> makeRemoteTextMap(string text, long contactId = 0, long chatId = 0, bool includeSelf = false)
        {
            Dictionary<object, object> idToKeyMap = new Dictionary<object, object>();
            Dictionary<object, object> remoteTextMap = new Dictionary<object, object>();

            if (chatId != 0)
            {
                Chat chat = Sphinx.Instance.chats.FirstOrDefault(c => c.id == chatId);

                if (chat != null)
                {
                    // Tribe
                    if (chat.type == (int)CHAT_TYPE.tribe && chat.group_key != null)
                    {
                        idToKeyMap.Add("chat", chat.group_key);

                        Contact me = Sphinx.Instance.contacts.FirstOrDefault(c => c.id == 1);

                        if (me != null)
                        {
                            idToKeyMap[1] = me.contact_key;
                        }
                    }
                    // Non-Tribe
                    else
                    {
                        List<Contact> contacts = new List<Contact>();

                        if (includeSelf)
                        {
                            contacts = Sphinx.Instance.contacts.Where(c => Array.IndexOf(chat.contact_ids, c.id) > -1).ToList<Contact>();
                        }
                        else
                        {
                            contacts = Sphinx.Instance.contacts.Where(c => Array.IndexOf(chat.contact_ids, c.id) > -1 && c.id != 1).ToList<Contact>();
                        }
                    }
                }
            }
            else
            {
                Contact contact = Sphinx.Instance.contacts.FirstOrDefault(c => c.id == contactId);

                if (contact != null)
                {
                    idToKeyMap[contactId] = contact.contact_key;
                }
            }

            foreach (KeyValuePair<object, object> pair in idToKeyMap)
            {
                string encrypted = Crypto.Encrypt(text, (string)pair.Value);
                remoteTextMap.Add(pair.Key, encrypted);
            }
            return remoteTextMap;
        }
    }

    //  const constants = {
    //   invite_statuses: {
    //     pending: 0,
    //     ready: 1,
    //     delivered: 2,
    //     in_progress: 3,
    //     complete: 4,
    //     expired: 5,
    //     payment_pending: 6
    //   },
    //   contact_statuses: {
    //     pending: 0,
    //     confirmed: 1
    //   },
    //   statuses: {
    //     pending: 0,
    //     confirmed: 1,
    //     cancelled: 2,
    //     received: 3,
    //     failed: 4,
    //     deleted: 5
    //   },
    //   chat_statuses: {
    //     approved: 0,
    //     pending: 1,
    //     rejected: 2
    //   },
    //   message_types: {
    //     message: 0,
    //     confirmation: 1,
    //     invoice: 2,
    //     payment: 3,
    //     cancellation: 4,
    //     direct_payment: 5,
    //     attachment: 6,
    //     purchase: 7,
    //     purchase_accept: 8,
    //     purchase_deny: 9,
    //     contact_key: 10,
    //     contact_key_confirmation: 11,
    //     group_create: 12,
    //     group_invite: 13,
    //     group_join: 14,
    //     group_leave: 15,
    //     group_kick: 16,
    //     delete: 17,
    //     repayment: 18,
    //     member_request: 19,
    //     member_approve: 20,
    //     member_reject: 21,
    //     tribe_delete: 22,
    //     bot_install: 23,
    //     bot_cmd: 24,
    //     bot_res: 25,
    //     keysend: 28,
    //     boost: 29
    //   },
    //   payment_errors: {
    //     timeout: 'Timed Out',
    //     no_route: 'No Route To Receiver',
    //     error: 'Error',
    //     incorrect_payment_details: 'Incorrect Payment Details',
    //     unknown: 'Unknown'
    //   },
    // }
}