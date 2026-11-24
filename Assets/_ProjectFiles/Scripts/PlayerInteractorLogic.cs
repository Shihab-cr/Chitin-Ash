namespace EJETAGame
{

    using UnityEngine;
    using UnityEngine.InputSystem;

    public class PlayerInteractorLogic : MonoBehaviour
    {
        [SerializeField] private Transform interactionSource;
        [SerializeField] private float interactionRange=2;
        [SerializeField] private LayerMask interactionLayer;
        private IInteractable currInteractable;
        
        
        public void OnInteractPressed(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (currInteractable as Object == null) return;
                else
                {
                    currInteractable.Interact();
                }
            }
        }
        void Update()
        {
            Collider[] interactables = Physics.OverlapSphere(interactionSource.position, interactionRange, interactionLayer);
            float closestDist = Mathf.Infinity;
            GameObject detectedObject = null;
            IInteractable closestInteractable = null;
            foreach(Collider interactable in interactables)
            {
                float dist = Vector3.Distance(interactionSource.position, interactable.gameObject.transform.position);
                if (dist < closestDist)
                {
                    detectedObject = interactable.gameObject;
                    if (detectedObject != null && detectedObject.TryGetComponent(out IInteractable detectInteractable))
                    {
                        closestDist = dist;
                        if (detectInteractable != closestInteractable)
                        {
                            closestInteractable = detectInteractable;

                        }
                    }
                    else
                    {
                        detectedObject = null;
                    }
                    

                }
                
            }
            if (closestInteractable == null)
            {
                InteractionText.instance.textAppear.gameObject.SetActive(false);
                if(currInteractable as Object!= null)
                {
                    currInteractable.OnInteractExit();
                    currInteractable = null;
                }
            }
            else
            {
                if(currInteractable != closestInteractable)
                {
                    if (currInteractable as Object != null) currInteractable.OnInteractExit();
                    currInteractable = closestInteractable;
                    currInteractable.OnInteractEnter();
                }
                InteractionText.instance.textAppear.gameObject.SetActive(true);
                
            }
            
        }
    }

}