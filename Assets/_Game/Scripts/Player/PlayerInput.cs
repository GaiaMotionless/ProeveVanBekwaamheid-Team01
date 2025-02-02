using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] Player player; //The parent object of the entire player.
    [SerializeField]
    GameObject playerVisual; //The visual part of the player which rotates.

    [SerializeField] GameObject targetPoint, //The object which the player will always rotate towards. AKA targetting point.
                                playerEmitter; //The emit point for bullets and the taser projectiles.
    private readonly float cameraZDistance = 11.45f;

    [SerializeField] private bool controller = false; //While true, the input will go via a Controller (PS4 Controller).
    public float sensitivity = 20; //Sensitivity of aiming with the controller

    [SerializeField]
    private int groundLayer = 7;

    [SerializeField]
    private float interactRange = 4;

    [SerializeField]
    private KeyCode interactButton = KeyCode.Space;

    //Keyboard/Mouse
    private readonly KeyCode moveLeft = KeyCode.A,
    moveRight = KeyCode.D,
    moveUp = KeyCode.W,
    moveDown = KeyCode.S,
    switchWeapon = KeyCode.LeftControl;
    //Controller
    private readonly string conMoveSide = "LR", //Left Right for walking
    conMoveFrontBack = "FB", //Front Back for walking
    conAimLR = "AimLR", //Left Right for aiming
    conAimFB = "AimFB", //Front Back for aming
    interactKeyController = "InteractCon", //Key used for interacting
    shootKeyController = "ShootCon"; //Key used for shooting 

    private float shootTimer = 0;

    void FixedUpdate()
    {
        shootTimer += Time.deltaTime;
        if (!controller)
        { //When Mouse and Keyboard mode is active (Controller=talse)
            if (Input.GetKey(moveLeft)) { MoveLeft(); }
            if (Input.GetKey(moveRight)) { MoveRight(); }
            if (Input.GetKey(moveUp)) { MoveUp(); }
            if (Input.GetKey(moveDown)) { MoveDown(); }
            Aim();

            if (Input.GetKeyDown(switchWeapon))
            {
                player.SwitchWeapon();
            }

            if (Input.GetKeyDown(interactButton))
            {
                Interact();
            }

            if (Input.GetMouseButton(0))
            {
                Shoot();
            }
        }
        else //When controller mode is active (Controller=true)
        {
            MoveController();
            AimController();
            if (Input.GetAxis(interactKeyController) > 0)
            {
                Interact();
            }

            if (Input.GetAxis(shootKeyController) > 0)
            {
                Shoot();
            }
        }
    }
    //MOUSE AND KEYBOARD
    //Movement for Mouse and Keyboard input. Will run when controller is false.
    private void MoveLeft() { player.transform.Translate(-player.speed * Time.deltaTime, 0, 0); }
    private void MoveRight() { player.transform.Translate(player.speed * Time.deltaTime, 0, 0); }
    private void MoveUp() { player.transform.Translate(0, 0, player.speed * Time.deltaTime); }
    private void MoveDown() { player.transform.Translate(0, 0, -player.speed * Time.deltaTime); }

    //Moves the aim (crosshair) to the mouse's world position. Also rotates the player's visual object towards the crosshair.
    private void Aim()
    {
        //Create and invert the layermask.
        int layerMask = 1 << groundLayer;

        //Casts a raycast from the camera to the mouse position and moves the targetting point to the mouse's location.
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cameraZDistance));
        targetPoint.transform.position = mousePos;

        // old raycast based aim
        /*if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, layerMask))
        {
            targetPoint.transform.position = new Vector3(hit.point.x,
                                                 playerEmitter.transform.position.y,
                                                 hit.point.z);
        }*/



        //Rotate the visual object of the player towards the targetting point.
        Vector3 tarDir = targetPoint.transform.position - playerVisual.transform.position;
        Vector3 newDirection = Vector3.RotateTowards(playerVisual.transform.forward, tarDir, 1, 0.0f);
        playerVisual.transform.rotation = Quaternion.LookRotation(newDirection);
        playerVisual.transform.localEulerAngles = new Vector3(0, playerVisual.transform.localEulerAngles.y, 0);
    }

    //CONTROLLER
    //Movement for Controller input. Will run when controller is true.
    private void MoveController()
    {
        player.transform.Translate(Input.GetAxis(conMoveSide) * player.speed * Time.deltaTime,
                                   0,
                                   -Input.GetAxis(conMoveFrontBack) * player.speed * Time.deltaTime);
    }

    //Moves the aim (crosshair) across the screen. Also rotates the player's visual object towards the crosshair.
    private void AimController()
    {
        targetPoint.transform.Translate(Input.GetAxis(conAimLR) * sensitivity * Time.deltaTime,
                                        0,
                                       -Input.GetAxis(conAimFB) * sensitivity * Time.deltaTime);

        //Rotate the visual object of the player towards the targetting point.
        Vector3 tarDir = targetPoint.transform.position - playerVisual.transform.position;
        Vector3 newDirection = Vector3.RotateTowards(playerVisual.transform.forward, tarDir, 1, 0.0f);
        playerVisual.transform.rotation = Quaternion.LookRotation(newDirection);
        playerVisual.transform.localEulerAngles = new Vector3(0, playerVisual.transform.localEulerAngles.y, 0);
    }

    //The general function for shooting both the primary (Assault Rifle) and secondary weapon (Taser).
    private void Shoot()
    {
        if (player.currentWeapon == 0 && shootTimer > player.HK416.GetFireRate())
        {
            player.HK416.Shoot(); //Assault Rifle
            shootTimer = 0;
        }
        if (player.currentWeapon == 1 && shootTimer > player.X26.GetFireRate())
        {
            player.X26.Shoot(); //Taser 
            shootTimer = 0;
        }
        
    }

    //The general function to interact with interactable objects within range (interactRange).
    private void Interact()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, playerVisual.transform.forward, interactRange);

        foreach (RaycastHit hit in hits)
        {
            hit.transform.gameObject.GetComponent<Interactable>()?.Interact(transform);
        }
    }
}
