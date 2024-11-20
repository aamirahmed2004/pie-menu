using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCursor : MonoBehaviour
{
    // [SerializeField] private float radius;
    // [SerializeField] private ContactFilter2D contactFilter;

    private Camera mainCam;
    private Collider2D detectedCollider = null;
    private Collider2D previousDetectedCollider = new();

    

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {

        //Get Mouse Position on screen, and get the corresponding position in a Vector3 World Co-Ordinate
        Vector3 mousePosition = Input.mousePosition;

        //Change the z position so that cursor does not get occluded by the camera
        mousePosition.z += 1f;
        mousePosition.x = Mathf.Clamp(mousePosition.x, 0f, Screen.width);
        mousePosition.y = Mathf.Clamp(mousePosition.y, 0f, Screen.height);

        transform.position = mainCam.ScreenToWorldPoint(mousePosition);

        // Vector3 screenCentre = new Vector3(Screen.width / 2, Screen.height / 2, 1f);
        // Vector3 worldCentre = mainCam.ScreenToWorldPoint(screenCentre);

        // float distanceFromCenter = Vector3.Distance(worldCentre, transform.position);
        // Debug.Log("Amplitude: " + distanceFromCenter);

        // Casting a ray straight down, below the cursor
        Collider2D detectedCollider = Physics2D.OverlapPoint(transform.position);

        if (detectedCollider != null)
        {
            this.detectedCollider = detectedCollider;
            UnHoverPreviousTarget(detectedCollider);
            HoverTarget(detectedCollider);

            if (Input.GetMouseButtonDown(0))
            {
                if (detectedCollider.TryGetComponent(out Target target) && target.IsGoalTarget()) SelectTarget(detectedCollider);      
            }
        }
        else
        {
            UnHoverPreviousTarget();
        }

        previousDetectedCollider = this.detectedCollider;
    }   

    private void HoverTarget(Collider2D collider){
        if(collider.TryGetComponent(out Target target)){
            target.OnHoverEnter();
        }
        else{
            //Debug.LogWarning("Not a valid target?");
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
