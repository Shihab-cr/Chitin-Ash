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
                if (currInteractable as Object == null) {
                    Debug.Log("Nothing to interact with");
                    return;
                }
                else
                {
                    currInteractable.Interact();
                }
            }
        }
        void Update()
        {
            Vector3 origin = interactionSource != null ? interactionSource.position : transform.position;
            Collider[] interactables = Physics.OverlapSphere(origin, interactionRange, interactionLayer, QueryTriggerInteraction.Collide);

            float closestDist = Mathf.Infinity;
            IInteractable closestInteractable = null;

            foreach (Collider col in interactables)
            {
                IInteractable detectInteractable = col.GetComponentInParent<IInteractable>();
                if (detectInteractable == null) continue;

                float dist = Vector3.Distance(origin, col.ClosestPoint(origin));
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestInteractable = detectInteractable;
                }
            }

            if (closestInteractable == null)
            {
                if (InteractionText.instance != null)
                    InteractionText.instance.textAppear.gameObject.SetActive(false);

                if (currInteractable as Object != null)
                {
                    currInteractable.OnInteractExit();
                    currInteractable = null;
                }
            }
            else
            {
                if (currInteractable != closestInteractable)
                {
                    if (currInteractable as Object != null) currInteractable.OnInteractExit();
                    currInteractable = closestInteractable;
                    currInteractable.OnInteractEnter();
                }

                if (InteractionText.instance != null)
                    InteractionText.instance.textAppear.gameObject.SetActive(true);
            }
        }
    }

}