//
//  http://playentertainment.company
// 

using System;
using System.Collections.Generic;

namespace PlayEntertainment.Sphinx
{
    [Serializable]
    public class Json_Ask
    {
        public string id;
        public string challenge;
    }

    [Serializable]
    public class Json_Signer
    {
        public bool success;
        public Signature response;
    }

    [Serializable]
    public class Signature
    {
        public string sig;
    }

    [Serializable]
    public class Json_Verify
    {
        public string token;
    }

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
    public class Json_Purchase
    {
        public bool success;
        public Response_Purchase response;
    }

    [Serializable]
    public class Response_Purchase
    {
        public bool seen;
        public long id;
        public string uuid;
        public long sender;
        public int type;
        public long amount;
        public string media_token;
        public string date;
        public string created_at;
        public string updated_at;
        public int network_type;
        public long amount_msat;

        // public string status_map;

        public Chat chat;
        public Contact contact;
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
    public class Json_Message
    {
        public bool success;
        public Response_Message response;
    }

    [Serializable]
    public class Response_Message : Msg
    {
    }

    // {
    //   "success": true,
    //   "response": {
    //     "seen": false,
    //     "id": 225,
    //     "chat_id": 2,
    //     "uuid": "9ZWm3S4LHTS5eeLhYw4i4o",
    //     "type": 0,
    //     "sender": 1,
    //     "amount": 0,
    //     "date": "2021-04-15T15:23:11.000Z",
    //     "message_content": "y4COJLfPKiYg1HZgMvguny/JTQGjoH2WmMFXtPJyOrtwlIyF+EgAWxH4s2JBhrwYEl+XguRZG9tDN9nx6KKrkkr5V6HuZXt5tNmVjFMg8azi1mJ5sMEVEytPjkeio/m6H6x0IciAOZqc6a6KH58LUuZBMrl8WZoz8O3ZXR7mtg9YeTylSMCKe6EKkleWhT9fTTRJ57tiPWvE8Gq6xk9fgLySdi78xEH/Rc++yIutzpUWHGafxk0H9n0LWPVR16SQzWD+RTe+GLT64pyWxbFU7mTYSippSi+ane4ZlOQnUU7ZD3Y+DusYZxPUo4+lVnWMA5m1eR7OhZfk/E68hUN35g==",
    //     "status": 0,
    //     "created_at": "2021-04-15T15:23:11.000Z",
    //     "updated_at": "2021-04-15T15:23:11.842Z",
    //     "network_type": 1,
    //     "amount_msat": 0,
    //     "status_map": null,
    //     "chat": {
    //       "id": 2,
    //       "uuid": "YG9Vnx_MlTLsIyn0Rva_6AUEdVXdnCJ7WDoi-vP0Ylz48OHnJl0zb3sLU3yN6DJCyAxWxcQ8zFmbPb5jwYNDBflkMx7z",
    //       "name": "Feed 2",
    //       "photo_url": "",
    //       "type": 2,
    //       "status": 0,
    //       "contact_ids": [
    //         1,
    //         3
    //       ],
    //       "is_muted": null,
    //       "created_at": "2021-04-08T21:05:30.000Z",
    //       "updated_at": "2021-04-15T15:19:27.312Z",
    //       "deleted": 0,
    //       "group_key": "MIIBCgKCAQEA4UzVGPrjZx0ajtcQO7WOgM+F014MbAmmFKw1BSLYhL7ZznbZbYGA/zehoZCSYXobCkm8mZVBfho1Q7UkU2n+8MAtSPS7Zg46DbnrvyuJbrww0f1XRHV5Qma5j/Ht9+rtlipzcImAOQTyygaEbytDOI2ByRXbjfVVSWx2rS/chQWhTqQgQl/wRKuQl7za4oLwU0juRVJ5mmZzx8GrGY2vk337X2IyvyhGYlvyxLILIMfx7B4EV0LeeHWwhytNT23GfmBUNw5BkpZKpzMlNQ00GhsfSIeb/whqDURaeLMpeKaejQsps8RSEMVjILNUA8cIZCrDvPNisTpXay7JivqvsQIDAQAB",
    //       "host": "tribes-staging.n2n2.chat",
    //       "price_to_join": 0,
    //       "price_per_message": null,
    //       "escrow_amount": null,
    //       "escrow_millis": null,
    //       "unlisted": false,
    //       "private": false,
    //       "owner_pubkey": "02c47129168a1771d1e645eb9204098920cadaee6e16f9455008272d7bbd573dd9",
    //       "seen": true,
    //       "app_url": null,
    //       "feed_url": null,
    //       "meta": null,
    //       "my_photo_url": null,
    //       "my_alias": null
    //     },
    //     "contact": null
    //   }
    // }

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

    [Serializable]
    public class LDAT_Meta
    {
        public long amount;
        public int ttl;
    }

    [Serializable]
    public class LDAT
    {
        public string host;
        public string muid;
        public string pubkey;
        public int ts;
        public LDAT_Meta meta;
        public string sig;
    }

    [Serializable]
    public class Server
    {
        public string host;
        public string token;
    }
}