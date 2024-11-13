using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCursor : MonoBehaviour
{
    [SerializeField] private float radius;
    [SerializeField] private ContactFilter2D contactFilter;
    // Start is called before the first frame update

    private Camera mainCam;
    private List<Collider2D> results = new();
    private Collider2D previousDetectedCollider = new();
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D detectedCollider = null;

        Physics2D.OverlapCircle(transform.position, radius, contactFilter, results);

        Vector3 mousePosition = Input.mousePosition;

        mousePosition.z += 9f;
        mousePosition.x = Mathf.Clamp(mousePosition.x, 0f, Screen.width);
        mousePosition.y = Mathf.Clamp(mousePosition.y, 0f, Screen.height);
        transform.position = mainCam.ScreenToWorldPoint(mousePosition);

        if(results.Count < 1){
            UnHoverPreviousTarget();
            return;
        }
        else if(results.Count > 1){
            UnHoverPreviousTarget();
            Debug.LogWarning("Too many targets in area");
            return;
        }
        else{
            detectedCollider = results[0];
            UnHoverPreviousTarget(detectedCollider);
            HoverTarget(detectedCollider);
        }

        if(Input.GetMouseButtonDown(0)){
            SelectTarget(detectedCollider);
        }

        previousDetectedCollider = detectedCollider;
    }   

    private void HoverTarget(Collider2D collider){
        if(collider.TryGetComponent(out Target target)){
            target.OnHoverEnter();
        }
        else{
            Debug.LogWarning("Not a valid target?");
        }
    }

    private void UnHoverPreviousTarget(){
        if(previousDetectedCollider != null){
            if(previousDetectedCollider.TryGetComponent(out Target target)){
                target.OnHoverExit();
            }
        }
    }

    private void UnHoverPreviousTarget(Collider2D collider){
        if(previousDetectedCollider != null && collider != previousDetectedCollider){
            if(previousDetectedCollider.TryGetComponent(out Target target)){
                target.OnHoverExit();
            }
        }
    }

    void SelectTarget(Collider2D collider){
        if(collider.TryGetComponent(out Target target)){
            target.OnSelect();
        }
    }
}
