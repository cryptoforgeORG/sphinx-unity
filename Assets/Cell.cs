//
//  http://playentertainment.company
//  

using PlayEntertainment.Sphinx;

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;


using UnityEngine.UI;

public class Media
{
    public static async Task<Texture2D> GetRemoteTexture(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {
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
                return DownloadHandlerTexture.GetContent(webRequest);

            }

            return null;
        }
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

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    async public void Activate()
    {
        this.value_Description.text = message.media_token;
        LDAT ldat = Helpers.parseLDAT(message.media_token);

        this.value_Price.text = string.Format("{0} sats", ldat.meta.amount);

        _texture = await Media.GetRemoteTexture(_imageUrl);

        Sprite sprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(_texture.width / 2, _texture.height / 2));
        this.image.overrideSprite = sprite;
    }

    public void FetchAsset(string host, string token)
    {
        Server server = new Server();

        server.host = "memes-staging.n2n2.chat";
        server.token = "";

        string url = string.Format("https://{0}/file/{1}", server.host, server.token);

    }

    void OnDestroy() => Dispose();
    public void Dispose() => Object.Destroy(_texture);
}
