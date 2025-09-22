using UnityEngine;

public class GameStarter : MonoBehaviour
{
    public Machine cutterMachine;
    public Machine benderMachine;
    public SimpleLogist logist;
    public Transform resourceSpawnPoint;

    void Start()
    {
        StartCoroutine(SimulateProduction());
    }

    private System.Collections.IEnumerator SimulateProduction()
    {
        while (true)
        {
            // Создаем сырую трубу каждые 10 секунд
            CreateRawPipe();
            yield return new WaitForSeconds(10f);
        }
    }

    private void CreateRawPipe()
    {
        GameObject pipeObj = new GameObject("RawPipe");
        pipeObj.transform.position = resourceSpawnPoint.position;
        
        Product product = pipeObj.AddComponent<Product>();
        product.Initialize(ProductType.RawPipe, null);
        
        SpriteRenderer sr = pipeObj.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("Sprites/RawPipe");
        sr.color = Color.gray;
        
        cutterMachine.SetInputProduct(product);
    }
}
