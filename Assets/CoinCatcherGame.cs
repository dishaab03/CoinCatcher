
using System.Collections.Generic;
using UnityEngine;

public class CoinCatcherGame : MonoBehaviour
{
    class FallingItem
    {
        public GameObject obj;
        public bool bomb;
    }

    GameObject catcher;
    Sprite circleSprite, squareSprite;
    List<FallingItem> items = new List<FallingItem>();

    float spawnTimer;
    float spawnRate = 0.75f;
    int score = 0;
    int lives = 3;
    bool gameOver = false;

    float leftEdge, rightEdge;

    void Start()
    {
        Camera cam = Camera.main;
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.backgroundColor = new Color(0.08f, 0.12f, 0.30f);

        float width = 5f * Screen.width / Screen.height;
        leftEdge = -width + 0.6f;
        rightEdge = width - 0.6f;

        circleSprite = MakeSprite(true);
        squareSprite = MakeSprite(false);

        catcher = CreateObject(
            "Catcher", squareSprite,
            new Vector2(0, -3.8f),
            new Vector2(1.5f, 0.3f),
            new Color(0.1f, 1f, 0.25f)
        );
    }

    Sprite MakeSprite(bool circle)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - 31.5f) / 31.5f;
                float dy = (y - 31.5f) / 31.5f;
                bool inside = !circle || dx * dx + dy * dy <= 1f;
                tex.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 64);
    }

    GameObject CreateObject(string objName, Sprite sprite,
        Vector2 pos, Vector2 scale, Color color)
    {
        GameObject obj = new GameObject(objName);
        obj.transform.position = pos;
        obj.transform.localScale = new Vector3(scale.x, scale.y, 1);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;

        return obj;
    }

    void Update()
    {
        if (gameOver) return;

        float input = 0;

        if (Input.GetKey(KeyCode.LeftArrow) ||
            Input.GetKey(KeyCode.A))
            input -= 1;

        if (Input.GetKey(KeyCode.RightArrow) ||
            Input.GetKey(KeyCode.D))
            input += 1;

        Vector3 p = catcher.transform.position;
        p.x = Mathf.Clamp(p.x + input * 8f * Time.deltaTime,
                          leftEdge, rightEdge);
        catcher.transform.position = p;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnRate)
        {
            SpawnItem();
            spawnTimer = 0;
        }

        for (int i = items.Count - 1; i >= 0; i--)
        {
            FallingItem item = items[i];
            item.obj.transform.position +=
                Vector3.down * 3.5f * Time.deltaTime;

            Vector3 ip = item.obj.transform.position;
            Vector3 cp = catcher.transform.position;

            bool caught =
                Mathf.Abs(ip.x - cp.x) < 0.85f &&
                Mathf.Abs(ip.y - cp.y) < 0.4f;

            if (caught)
            {
                if (item.bomb)
                {
                    lives--;
                    if (lives <= 0) gameOver = true;
                }
                else
                {
                    score++;
                    spawnRate = Mathf.Max(0.3f, 0.75f - score * 0.01f);
                }

                Destroy(item.obj);
                items.RemoveAt(i);
            }
            else if (ip.y < -5.5f)
            {
                Destroy(item.obj);
                items.RemoveAt(i);
            }
        }
    }

    void SpawnItem()
    {
        bool bomb = Random.value < 0.28f;

        GameObject obj = CreateObject(
            bomb ? "Bomb" : "Coin",
            circleSprite,
            new Vector2(Random.Range(leftEdge, rightEdge), 5.5f),
            Vector2.one * 0.48f,
            bomb ? new Color(1f, 0.15f, 0.15f)
                 : new Color(1f, 0.82f, 0.05f)
        );

        items.Add(new FallingItem { obj = obj, bomb = bomb });
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 24;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(20, 15, 250, 40),
            "COINS: " + score, style);
        GUI.Label(new Rect(20, 50, 250, 40),
            "LIVES: " + lives, style);

        if (gameOver)
        {
            GUIStyle center = new GUIStyle(GUI.skin.label);
            center.fontSize = 38;
            center.fontStyle = FontStyle.Bold;
            center.alignment = TextAnchor.MiddleCenter;
            center.normal.textColor = Color.white;

            GUI.Label(new Rect(0, Screen.height / 2 - 70,
                Screen.width, 60), "GAME OVER", center);

            GUI.Label(new Rect(0, Screen.height / 2 - 20,
                Screen.width, 50), "Final Score: " + score, center);

            if (GUI.Button(new Rect(Screen.width / 2 - 65,
                Screen.height / 2 + 45, 130, 45), "RESTART"))
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager
                    .GetActiveScene().buildIndex);
        }
    }
}
