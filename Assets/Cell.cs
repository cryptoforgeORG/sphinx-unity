//
//  http://playentertainment.company
//  

using PlayEntertainment.Sphinx;

using RNCryptor;

using System;
using System.IO;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;


using UnityEngine.UI;

public class Media
{
    public static async Task<Texture2D> FetchAndStore(string url, string path, string key, string token = "")
    {
        // url = "https://memes-staging.n2n2.chat/file/bWVtZXMtc3RhZ2luZy5uMm4yLmNoYXQ=.95ciUHSO1jPl7PuT7JDmWaujd-XU7jDRl4nMBgh-Lew=.A9F_8pBCsujd0VVg7OnpC56yRUOOAHv1_DzfuDVvoqxM.YlnfEA==.YW10PTEw.H5h1xib_Fa25AYN9IErXWTViVRkFxehO8QL5z9OAEksvbN7yNt6-K9doQq9MTP0v2GuO5NsY9RLoUmudrFcZkZA=";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("Authorization", string.Format("Bearer {0}", token));

            // begin request:
            var asyncOp = webRequest.SendWebRequest();


            // await until it's done: 
            while (asyncOp.isDone == false)
                await Task.Delay(1000 / 30); //30 hertz

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    break;
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string filepath = string.Format("{0}/{1}", Application.persistentDataPath, path);

                File.WriteAllBytes(filepath, webRequest.downloadHandler.data);

                string disp = webRequest.GetResponseHeader("Content-Disposition");

                string[] words = disp.Split('=');

                string filename = words[1];

                var decryptor = new Decryptor();

                filepath = decryptor.DecryptFileAndSave(filepath, key, "");

                byte[] bytes = File.ReadAllBytes(filepath);

                Texture2D texture = new Texture2D(0, 0);
                texture.LoadImage(bytes);
                return texture;
            }

            return null;
        }
    }

    public static Texture2D ReadLocal(string filepath, string key)
    {
        if (!File.Exists(filepath))
        {
            return null;
        }

        byte[] bytes = File.ReadAllBytes(filepath);

        var decryptor = new Decryptor();

        filepath = decryptor.DecryptFileAndSave(filepath, key, "");

        bytes = File.ReadAllBytes(filepath);

        Texture2D texture = new Texture2D(0, 0);
        texture.LoadImage(bytes);
        return texture;
    }
}

public class Cell : MonoBehaviour
{
    // Start is called before the first frame update
    public Text value_Description;
    public Text value_Price;

    [SerializeField] string _imageUrl;

    public Image image;

    Texture2D _texture;

    public Msg message;
    public LDAT ldat;

    public Server server;

    public Button button_Pay;

    long chatId = 0;
    long contactId = 0;


    void Start()
    {
        this.button_Pay.onClick.AddListener(this.Purchase);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Purchase()
    {
        long amount = this.ldat.meta.amount;
        string media_token = this.message.media_token;

        Sphinx.Instance.PurchaseMedia(this.contactId, amount, this.chatId, media_token, delegate ()
        {
            this.Trigger();
        });
    }

    async public void Trigger()
    {
        string path = string.Format("msg_{0}", this.message.id);
        string filepath = string.Format("{0}/{1}", Application.persistentDataPath, path);

        this._texture = Media.ReadLocal(filepath, message.media_key);

        if (this._texture)
        {
            Sprite sprite = Sprite.Create(this._texture, new Rect(0, 0, this._texture.width, this._texture.height), new Vector2(this._texture.width / 2, this._texture.height / 2));
            this.image.overrideSprite = sprite;
        }
        else
        {
            if (message.media_key == "") return;

            this._imageUrl = this.GetUrl();
            await Media.FetchAndStore(this._imageUrl, path, message.media_key, server.token);
            this.Trigger();
        }
    }

    public void Activate(long chatId, long contactId)
    {
        this.value_Description.text = string.Format("{0}:{1}", this.message.id, this.message.sender);

        this.ldat = Helpers.parseLDAT(this.message.media_token);

        this.contactId = contactId;
        this.chatId = chatId;

        if (this.ldat.sig != null)
        {
            this.button_Pay.gameObject.SetActive(false);
        }

        this.value_Price.text = string.Format("{0} sats", this.ldat.meta.amount);

        this.Trigger();
    }

    public string GetUrl()
    {
        return string.Format("https://{0}/file/{1}", this.server.host, this.message.media_token);
    }

    void OnDestroy() => Dispose();
    public void Dispose() => UnityEngine.Object.Destroy(_texture);
}
