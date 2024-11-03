using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    private SpriteRenderer sprite;
    private bool onSelect;
    private Color originalColor;
    private bool canHover;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        originalColor = sprite.color;  // Store the original color
        StartCoroutine(EnableHoverAfterDelay(0.3f));
    }

    public void OnHoverEnter()
    {
        if (onSelect || !canHover) return;
        sprite.color = Color.yellow;
    }

    public void OnHoverExit()
    {
        if (onSelect || !canHover) return;
        sprite.color = originalColor;
    }

    public void OnSelect()
    {
        onSelect = true;
        sprite.color = Color.green;
        StartCoroutine(DestroyGameObject(0.1f));
    }

    public void ChangeColor(Color color)
    {
        if (sprite == null)
            sprite = GetComponent<SpriteRenderer>();

        sprite.color = color;
        originalColor = color;  // Update the original color if the color is changed
    }

    public bool IsRed()
    {
        return originalColor == Color.red;
    }

    private IEnumerator EnableHoverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canHover = true;
    }

    public IEnumerator DestroyGameObject(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }
}