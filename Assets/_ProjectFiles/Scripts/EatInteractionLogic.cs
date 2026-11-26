namespace EJETAGame
{

    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.InputSystem;

    public class EatInteractionLogic : MonoBehaviour, IInteractable
    {
        [SerializeField] private int scrapeAmount = 5;
        [SerializeField] private float healingAmount = 20f;
        [SerializeField] private InputActionReference interactionAction;
        [SerializeField] private string displayMessage= "Eat";
        [SerializeField] private Transform alignmentAnchor;
        private PlayerEatingAlignmentHandler playerEatingAlignmentHandler;
        public UnityEvent OnEating;
        private bool isEaten = false;
        private GameObject playerGameObject;
        private PlayerScrapesHandler playerScrapesHandler;
        private HealthHandler playerHealthHandler;
        private EatInteractionVisual eatInteractionVisual;
        private void Awake()
        {
            eatInteractionVisual = GetComponent<EatInteractionVisual>();
        }
        private void OnEnable()
        {
            isEaten = false;

            int interactableLayer = LayerMask.NameToLayer("Interactable");
            if (interactableLayer != -1)
            {
                gameObject.layer = interactableLayer;
            }
            else
            {
                Debug.LogError($"[{nameof(EatInteractionLogic)}] Layer 'Interactable' does not exist in Tags & Layers!", this);
            }

            if (eatInteractionVisual != null) eatInteractionVisual.ResetMaterial();
        }
        void Start()
        {
            playerGameObject = GameObject.FindGameObjectWithTag("Player");
            if(playerGameObject != null)
            {
            playerScrapesHandler = playerGameObject.GetComponent<PlayerScrapesHandler>();
            playerHealthHandler = playerGameObject.GetComponent<HealthHandler>();
                playerEatingAlignmentHandler = playerGameObject.GetComponent<PlayerEatingAlignmentHandler>();
            }
            

        }
        public void Interact()
        {
            if (playerGameObject == null) return;

            if (isEaten) return;
            if (playerEatingAlignmentHandler != null)
            {
                Transform targetAnchor = alignmentAnchor != null ? alignmentAnchor : transform;
                if (!playerEatingAlignmentHandler.StartEat(targetAnchor)) return;
            }
            isEaten = true;
            gameObject.layer = LayerMask.NameToLayer("Default");
            
            if(playerScrapesHandler != null)
            {
                playerScrapesHandler.AddScrapesToPlayer(scrapeAmount);
            }
            if(playerHealthHandler != null)
            {
                playerHealthHandler.Heal(healingAmount);
            }

            if (eatInteractionVisual != null) eatInteractionVisual.PlayDissolve();
            OnEating?.Invoke();
        }

        public void OnInteractEnter()
        {
            string keyBinding = (interactionAction != null && interactionAction.action != null)
                ? interactionAction.action.GetBindingDisplayString()
                : "E";

            if (InteractionText.instance != null)
            {
                InteractionText.instance.SetText($"[{keyBinding}] {displayMessage}");
            }
        }

        public void OnInteractExit()
        {
            
        }

        


    }
}
