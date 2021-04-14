//
//  http://playentertainment.company
//  
//  Copyright (c) Play Entertainment LLC, California. All rights reserved.
//

using RNCryptor;

using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace PlayEntertainment.Sphinx
{
    public class Canvas_Relay : MonoBehaviour
    {
        public string host_Memes = "";
        public string host_Tribes = "";

        public string code_Invite = string.Empty;
        public string code_Recovery = string.Empty;

        public string pin = "555555";

        public API api;

        void Awake()
        {

        }

        void Start()
        {
            if (PlayerPrefs.HasKey("invite_code"))
            {
                this.code_Invite = PlayerPrefs.GetString("invite_code");
                Debug.Log(this.code_Invite);
            }
            else
            {
                Debug.Log("invite_code not set");
            }

            if (PlayerPrefs.HasKey("recovery_code"))
            {
                this.code_Recovery = PlayerPrefs.GetString("recovery_code");
                Debug.Log(this.code_Recovery);
            }
            else
            {
                Debug.Log("recovery_code not set");
            }

            if (this.code_Recovery != string.Empty)
            {
                this.SetRecoveryCode(this.code_Recovery);
            }
        }

        public void SetInviteCode(string code)
        {
            Debug.Log(code);

            PlayerPrefs.SetString("invite_code", code);

            this.code_Invite = code;
        }

        public void SetRecoveryCode(string code)
        {
            Debug.Log(code);

            PlayerPrefs.SetString("recovery_code", code);

            this.code_Recovery = code;
        }

        public void Initialize()
        {
            if (PlayerPrefs.HasKey("invite_code"))
            {
                this.code_Invite = PlayerPrefs.GetString("invite_code");

                byte[] decodedBytes = Convert.FromBase64String(this.code_Invite);
                string decodedText = Encoding.UTF8.GetString(decodedBytes);

                Debug.Log(decodedText);

                this.SignupWithIP(decodedText);
            }
            else
            {
                Debug.Log("invite_code not set");
            }
        }

        public void Recover()
        {
            if (PlayerPrefs.HasKey("recovery_code"))
            {
                this.code_Recovery = PlayerPrefs.GetString("recovery_code");

                byte[] decodedBytes = Convert.FromBase64String(this.code_Recovery);
                string decodedText = Encoding.UTF8.GetString(decodedBytes);

                string[] separatingStrings = { "::" };
                string[] words = decodedText.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);

                string encrypted = words[1];

                Debug.Log(encrypted);

                var decryptor = new Decryptor();

                byte[] encryptedBytes = Convert.FromBase64String(encrypted);

                byte[] decryptedBytes = decryptor.Decrypt(encryptedBytes, this.pin);

                string decryptedString = Encoding.UTF8.GetString(decryptedBytes);

                Debug.Log("decryptedString " + decryptedString);

                this.Restore(decryptedString);

                Action<string, object, string, Action<object>> request = this.api.AddMethod("GET", this.api.url);
                request(string.Format("/balance"), null, null, delegate (object result)
                {
                    Dictionary<string, object> data = (Dictionary<string, object>)result;

                    bool success = (bool)data["success"];

                    if (success)
                    {
                        Dictionary<string, object> response = (Dictionary<string, object>)data["response"];

                        long balance_Reserve = (long)response["reserve"];
                        long balance_Full = (long)response["full_balance"];
                        long balance_Pending = (long)response["pending_open_balance"];
                        long balance_Available = (long)response["balance"];

                        Debug.Log(balance_Available);
                    }
                });
            }
            else
            {
                Debug.Log("recovery_code not set");
            }
        }

        public void Restore(string restoreString)
        {
            string[] separatingStrings = { "::" };
            string[] words = restoreString.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);

            string key = words[0];
            string ip = words[2];
            string token = words[3];

            this.api = new API(ip, "x-user-token", token, this, delegate ()
            {

            });
        }

        [ContextMenu("Procedure_Invite")]
        public void Procedure_Invite()
        {
            this.Initialize();
        }

        [ContextMenu("Procedure_Recover")]
        public void Procedure_Recover()
        {
            this.Recover();

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
    }
}
