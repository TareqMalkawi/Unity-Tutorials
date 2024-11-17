using UnityEngine;

public class DashEffect : MonoBehaviour
{
    // Reference to player transform. 
    private Transform player;

    // Reference to the material color.
    private Color matColor;
    private Material matInstance;

    private void OnEnable()
    {
        // Find player in scene.
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player)
        {
            // Set "DashEffect" game object transform properties.
            transform.SetPositionAndRotation(player.position, player.rotation);
            transform.localScale = player.GetChild(0).localScale;
        }

        matInstance = GetComponent<MeshRenderer>().material;
        matColor = matInstance.color;
        
        // Reset alpha.
        matColor.a = 1.0f;
        matInstance.color = matColor;
    }

    private void Update()
    {
        // Gradually reduce the alpha value over time.
        matColor.a = Mathf.Lerp(matColor.a, 0.0f, 5.0f * Time.deltaTime);

        matInstance.color = matColor;

        if(matColor.a <= 0.2)
        {
            Pool.instance.Return(gameObject);
        }
    }
}