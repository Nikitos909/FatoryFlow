using UnityEngine;
using System.Collections;

public class GameStarter : MonoBehaviour
{
    public Machine cutterMachine;
    public Machine benderMachine;
    public SimpleLogist logist;
    public Transform resourceSpawnPoint;
    public float spawnInterval = 10f;

    void Start()
    {
        if (cutterMachine == null || benderMachine == null)
        {
            Debug.LogError("Не назначены станки в GameStarter!");
            return;
        }

        StartCoroutine(SimulateProduction());
        Debug.Log("Запущено тестовое производство");
    }

    private IEnumerator SimulateProduction()
    {
        while (true)
        {
            // Создаем сырую трубу каждые 10 секунд
            CreateRawPipe();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

     private void CreateRawPipe()
    {
        GameObject pipeObj = new GameObject("RawPipe");
        pipeObj.transform.position = resourceSpawnPoint.position;
        
        Product product = pipeObj.AddComponent<Product>();
        product.Initialize(ProductType.RawPipe, null);
        
        SpriteRenderer sr = pipeObj.GetComponent<SpriteRenderer>();
        sr.sprite = CreateDefaultSprite();
        sr.color = Color.gray;
        
        if (cutterMachine != null && cutterMachine.CanAcceptInput(ProductType.RawPipe))
        {
            cutterMachine.SetInputProduct(product);
            Debug.Log("Создана сырая труба");
        }
        else
        {
            Debug.Log("Резак не готов принять сырую трубу");
            Destroy(pipeObj);
        }
    }

     private Sprite CreateDefaultSprite()
    {
        // Создаем простой спрайт программно
        Texture2D texture = new Texture2D(32, 32);
        for (int x = 0; x < 32; x++)
            for (int y = 0; y < 32; y++)
                texture.SetPixel(x, y, Color.white);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f);
    }
}
