using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputController : MonoBehaviour
{
    private float horizontalInput;
    private float verticalInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        
        if(Mouse.current.leftButton.isPressed)
        {
            Debug.Log("Left mouse button is pressed.");
        }

        if (Mouse.current.leftButton.ReadValue() > 0.5f)
        {
            Debug.Log("Left mouse button is pressed with a value greater than 0.5.");
        }

        if (Mouse.current.rightButton.isPressed)
        {
                       Debug.Log("Right mouse button is pressed.");
        }
        else if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            Debug.Log("Right mouse button was released this frame.");
        }

        if (Mouse.current.middleButton.isPressed)
        {
            Debug.Log("Middle mouse button is pressed.");
        }

        if(Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q key is pressed.");
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key is pressed.");
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key is pressed.");
        }

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.Log("Left Shift key is pressed.");
        }

       
    }
}
