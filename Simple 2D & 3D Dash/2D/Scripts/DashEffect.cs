using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DashEffect : MonoBehaviour
{
    // Reference to player transform. 
    private Transform player;

    private SpriteRenderer spriteRenderer;

    // Current player sprite.
    private SpriteRenderer playerSpriteRenderer;

    private float characterLocalScaleX;

    private void OnEnable()
    {
        // Reference to the empty "DashEffect" game object sprite renderer.
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Find player in scene
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player)
        {
            // Reference to the player sprite renderer.
            playerSpriteRenderer = player.GetComponentInChildren<SpriteRenderer>();

            // Store the current player sprite and assign it to the current enabled game object "DashEffect".
            spriteRenderer.sprite = playerSpriteRenderer.sprite;

            // Set the current "DashEffect" game object sprite layer's name and order to the 
            // current player in scene beforehand.
            spriteRenderer.sortingLayerName = playerSpriteRenderer.sortingLayerName;
            spriteRenderer.sortingOrder = playerSpriteRenderer.sortingOrder;

            // Set "DashEffect" game object transforem properties.
            transform.SetPositionAndRotation(player.position, player.rotation);

            characterLocalScaleX = player.GetChild(0).localScale.x;

            transform.localScale = player.localScale * characterLocalScaleX;
        }
    }
}