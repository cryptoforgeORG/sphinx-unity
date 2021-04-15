//
//  http://playentertainment.company
//  


using QFSW.QC;

using System;

using UnityEngine;
using UnityEngine.UI;

namespace PlayEntertainment.Sphinx
{
    public class Canvas_Pin : MonoBehaviour
    {
        public Action<string> callback;

        private string pin = string.Empty;

        public Text text_Pin;

        private void FlashPin()
        {
            this.text_Pin.text = pin;
        }

        public void Click_1()
        {
            this.pin += "1";
            this.FlashPin();
        }

        public void Click_2()
        {
            this.pin += "2";
            this.FlashPin();
        }

        public void Click_3()
        {
            this.pin += "3";
            this.FlashPin();
        }

        public void Click_4()
        {
            this.pin += "4";
            this.FlashPin();
        }

        public void Click_5()
        {
            this.pin += "5";
            this.FlashPin();
        }

        public void Click_6()
        {
            this.pin += "6";
            this.FlashPin();
        }

        public void Click_7()
        {
            this.pin += "7";
            this.FlashPin();
        }

        public void Click_8()
        {
            this.pin += "8";
            this.FlashPin();
        }

        public void Click_9()
        {
            this.pin += "9";
            this.FlashPin();
        }

        public void Click_10()
        {
            callback(this.pin);
            this.gameObject.SetActive(false);
        }

        public void Click_11()
        {
            this.pin += "0";
            this.FlashPin();
        }

        public void Click_12()
        {
            this.pin = string.Empty;
            this.FlashPin();
        }

    }
}
