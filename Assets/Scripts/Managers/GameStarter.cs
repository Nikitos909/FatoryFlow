using UnityEngine;
using System.Collections;

public class GameStarter : MonoBehaviour
{
    public Machine cutterMachine;
    public Machine benderMachine;
    public Logist logist;
    public Transform resourceSpawnPoint;
    public float spawnInterval = 8f;

    void Start()
    {
        // Регистрируем логиста
        LogisticsManager.Instance.availableLogists.Add(logist);

        // Запускаем производство
        StartCoroutine(SpawnRawPipes());
        Debug.Log("🎮 Игра запущена!");
    }

    private IEnumerator SpawnRawPipes()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            CreateRawPipe();
        }
    }

    private void CreateRawPipe()
    {
        if (!cutterMachine.CanAcceptInput(ProductType.RawPipe)) return;

        GameObject pipeObj = new GameObject("RawPipe");
        pipeObj.transform.position = resourceSpawnPoint.position;

        Product product = pipeObj.AddComponent<Product>();
        product.Initialize(ProductType.RawPipe, null);

        SpriteRenderer sr = pipeObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateDefaultSprite();
        sr.color = Color.gray;
        sr.sortingOrder = 1;

        cutterMachine.PutInputProduct(product);
        Debug.Log("📦 Создана сырая труба");
    }

    private Sprite CreateDefaultSprite()
    {
        Texture2D texture = new Texture2D(16, 16);
        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
                texture.SetPixel(x, y, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 16, 16), Vector2.one * 0.5f);
    }
}