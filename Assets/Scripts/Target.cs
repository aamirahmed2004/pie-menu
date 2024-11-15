using System.Collections;
using UnityEngine;
using TMPro;

public class Target : MonoBehaviour
{
    private SpriteRenderer sprite;
    private bool onSelect;
    private Color originalColor;
    private bool canHover;

    private bool isGoal = false;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        originalColor = sprite.color;  // Store the original color
        StartCoroutine(EnableHoverAfterDelay(0.3f));
    }

    public void SetGoalTarget()
    {
        isGoal = true;
        Debug.Log("Set to Goal!");
        ChangeColor(Color.red);
    }

    public bool IsGoalTarget()
    {
        return isGoal;
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

    private IEnumerator EnableHoverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canHover = true;
    }

    // Deletes the Target GameObject, the Label GameObject, and the parent TargetWithLabel's GameObject after "seconds" seconds
    public IEnumerator DestroyGameObject(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        
        // If the parent is a child of Camera.transform (in our case, that means the parent is a TargetWithLabel GameObject)
        if (gameObject.transform.parent.IsChildOf(Camera.main?.transform))
        {
            // Delete the parent
            Destroy(gameObject.transform.parent.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}