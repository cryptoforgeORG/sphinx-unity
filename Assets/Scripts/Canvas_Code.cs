﻿//
//  http://playentertainment.company
//  


using System;

using UnityEngine;
using UnityEngine.UI;

namespace PlayEntertainment.Sphinx
{
    public class Canvas_Code : MonoBehaviour
    {

        public Canvas_Pin canvas_Pin;

        public InputField inputField_Code;
        public Button button_Go;

        public API api;

        void Awake()
        {
        }

        void Start()
        {
            this.button_Go.onClick.AddListener(this.ProcessCode);

            // // Clear PlayerPrefs
            // if (true)
            // {
            //     PlayerPrefs.DeleteAll();

            // }

            if (PlayerPrefs.HasKey("invite_code"))
            {
                this.inputField_Code.text = PlayerPrefs.GetString("invite_code");
            }
            else
            {
                Debug.Log("invite_code not stored");
            }

            if (PlayerPrefs.HasKey("recovery_code"))
            {
                this.inputField_Code.text = PlayerPrefs.GetString("recovery_code");
            }
            else
            {
                Debug.Log("recovery_code not set");
            }


            if (PlayerPrefs.HasKey("restore_string"))
            {
                Sphinx.Instance.restoreString = PlayerPrefs.GetString("restore_string");
                Sphinx.Instance.Launch();
                this.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("restore_string not set");
            }
        }

        void ProcessCode()
        {
            string code = this.inputField_Code.text;
            Sphinx.Instance.ProcessCode(code, delegate (bool askForPin, Action<string> callback)
            {
                if (askForPin)
                {
                    this.canvas_Pin.gameObject.SetActive(true);
                    this.canvas_Pin.callback = callback;
                    this.gameObject.SetActive(false);
                }
            });
        }
    }
}
