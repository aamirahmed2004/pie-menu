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
    private bool isStart = false;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        originalColor = sprite.color;  // Store the original color
        StartCoroutine(EnableHoverAfterDelay(0.3f));
    }

    public void SetGoalTarget()
    {
        isGoal = true;
        // Debug.Log("Set to Goal!");
        ChangeColor(Color.red);
    }
    
    public void SetStartTarget()
    {
        isStart = true;
        isGoal = true;
        // Debug.Log("Set to Goal!");
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
        if (isGoal && !isStart)
        {
            var sm = GameObject.Find("StudyManager").GetComponent<StudyManager>();
            if (sm.TrialState == TrialState.Active)
                sm.TrialState = TrialState.Ended;
        } 
        else if (isStart)
        {
            var sm = GameObject.Find("StudyManager").GetComponent<StudyManager>();
            if (sm.TrialState == TrialState.Limbo)
                sm.TrialState = TrialState.Start; 
        }
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
        DestroyGameObject();
    }

    public void DestroyGameObject()
    {
        // Note that the parent GameObject is TargetWithLabel, which has children Target (this) and Label.
        // Find the Label gameObject in the children of the parent (i.e. should be a sibling component, but I couldnt find a function to directly look for siblings)

        TextMeshPro label = this.transform.parent.GetComponentInChildren<TextMeshPro>();
        if (label != null) Destroy(label.gameObject);

        Destroy(this.gameObject);

        // Delete the parent
        Destroy(this.transform.parent.gameObject);
    }

}