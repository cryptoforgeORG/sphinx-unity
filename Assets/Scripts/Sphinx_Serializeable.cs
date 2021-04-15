//
//  http://playentertainment.company
// 

using System;
using System.Collections.Generic;

namespace PlayEntertainment.Sphinx
{
    [Serializable]
    public class Json_Balance
    {
        public bool success;
        public Balance response;
    }

    [Serializable]
    public class Balance
    {
        public long reserve;
        public long full_balance;
        public long pending_open_balance;
        public long balance;
    }

    [Serializable]
    public class Json_Contacts
    {
        public bool success;
        public Response_Contacts response;
    }

    [Serializable]
    public class Response_Contacts
    {
        public List<Contact> contacts;
        public List<Subscription> subscriptions;
        public List<Chat> chats;
    }

    [Serializable]
    public class Json_Messages
    {
        public bool success;
        public Response_Messages response;
    }

    [Serializable]
    public class Response_Messages
    {
        public List<Msg> new_messages;
    }

    [Serializable]
    public class Chat
    {
        public long id;
        public string uuid;
        public string name;
        public string photo_url;
        public int type;
        public int status;
        public long[] contact_ids;
        public bool is_muted;
        public string created_at;
        public string updated_at;
        public bool deleted;
        public string feed_url;
        public string app_url;
        public string group_key;
        public string host;
        public long price_to_join;
        public long price_per_message;
        public long escrow_amount;
        public long escrow_millis;
        public string owner_pubkey;
        public bool unlisted;
        public long[] pending_contact_ids;
        public Invite invite;
        public string photo_uri;
        public long pricePerMinute;
        public string my_alias;
        public string my_photo_url;
    }

    [Serializable]
    public class Subscription
    {

    }

    [Serializable]
    public class Invite
    {
        public long id;
        public string invite_string;
        public string welcome_message;
        public long contact_id;
        public int status;
        public long price;
        public string created_at;
        public string updated_at;
    }

    [Serializable]
    public class Contact
    {
        public long id;
        public string public_key;
        public string node_alias;
        public string alias;
        public string photo_url;
        public string private_photo;
        public bool is_owner;
        public bool deleted;
        public string auth_token;
        public long remote_id;
        public int status;
        public string contact_key;
        public string device_id;
        public string created_at;
        public string updated_at;
        public bool from_group;
        public string notification_sound;
        public string last_active;
        public long tip_amount;
        public Invite invite;
    }

    [Serializable]
    public class Msg
    {

        public long id;
        public string uuid;
        public long chat_id;
        public int type;
        public long sender;
        public long receiver;
        public long amount;
        public long amount_msat;
        public string payment_hash;
        public string payment_request;
        public string date;
        public string expiration_date;
        public string message_content;
        public string remote_message_content;
        public int status;
        //   status_map: { [k: number]: number }
        public long parent_id;
        public long subscription_id;
        public string media_type;
        public string media_token;
        public string media_key;
        public bool seen;
        public string created_at;
        public string updated_at;
        public string sender_alias;
        public string sender_pic;
        public string original_muid;
        public string reply_uuid;
        public int network_type;
        public Chat chat;
        public Contact contact;

        // public bool sold; // this is a marker to tell if a media has been sold
        // public bool showInfoBar; // marks whether to show the date and name
        // public string reply_message_content;
        // public string reply_message_sender_alias;
        // public long reply_message_sender;
        // public long boosts_total_sats;
        // public BoostMsg[] boosts;
    }

    [Serializable]
    public class BoostMsg
    {
        public long amount;
        public string date;
        public string sender_alias;
    }
}