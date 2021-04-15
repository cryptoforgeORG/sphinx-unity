using System.Diagnostics;
//
//  http://playentertainment.company
//  


using RNCryptor;

using System;
using System.Collections.Generic;
using System.Linq;

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
    //   chat_types: {
    //     conversation: 0,
    //     group: 1,
    //     tribe: 2
    //   }
    // }
}