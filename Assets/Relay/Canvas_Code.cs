//
//  http://playentertainment.company
//  
//  Copyright (c) Play Entertainment LLC, California. All rights reserved.
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
        }

        void ProcessCode()
        {
            string code = this.inputField_Code.text;
            Manager_Sphinx.Instance.ProcessCode(code, delegate (bool askForPin, Action<string> callback)
            {
                if (askForPin)
                {
                    this.canvas_Pin.gameObject.SetActive(true);
                    this.canvas_Pin.callback = callback;
                }
            });
        }
    }
}
