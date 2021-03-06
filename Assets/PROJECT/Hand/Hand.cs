﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;


public enum EHandSide{LEFT, RIGHT}


public class Hand : MonoBehaviour
{

    [SerializeField]
    EHandSide handSide;

    public EHandSide HandSide { get => handSide; }
    
    OVRPlayerController playerController; //Handles movement of Avatar when grabbing

    int layer_GrabHandle = 8;
    int layer_HandDevice = 12;
    int layer_ZipLineHandle = 13;

    Vector3 DefaultLocalPosition = new Vector3(0, 0, 0);

    //We need reference to handle because we need handle position
    //every update to place hand correctly on moving handles
    GameObject handle;
    Vector3 offsettToHandleOnGrab;

    Vector3 handOffsetToPlayerControllerOnZipLineGrab;

    static Hand leftHand;
    static Hand rightHand;

    Hand otherHand;

    public Hand OtherHand { get => otherHand; }

    OVRInput.Button grabButton;

    bool shouldGrab = false;
    bool shouldRelease = false;

    float grabHandleRumble = 0;

    //(OmniDevice controller default for right arm when not holding other device)

    HandDevice handDevice;

    HandDevice omniDevice;

    HandDeviceData handDeviceData;

    public HandDevice OmniDevice { set => omniDevice = value; get => omniDevice; }


    HandSync handSync;

    public HandSync HandSync { get => handSync; }


    /// <summary>
    /// Holding non-gravity-controller-device (Only those need manually syncing with hand when holding)
    /// </summary>
    bool grabbingHandDevice = false;

    bool usingHandDevice = false;

    public bool UsingHandDevice { get => usingHandDevice; }

    bool grabbingZipLine = false;
    

    /// <summary>
    /// UI screen that shows details about the held device
    /// </summary>
    UIHandDevice deviceUI;

    // Start is called before the first frame update (1. Awake -> 2. Start)
    void Awake()
    {
        playerController = transform.root.GetComponent<OVRPlayerController>();

        //deviceUI = GetComponentInChildren<UIHandDevice>();
        //deviceUI.Initialize();

        if (handSide == EHandSide.LEFT)
        {
            leftHand = this;
            grabButton = OVRInput.Button.PrimaryHandTrigger;
        }

        else if (handSide == EHandSide.RIGHT)
        {
            rightHand = this;
            grabButton = OVRInput.Button.SecondaryHandTrigger;
        }

        omniDevice = GetComponent<OmniDevice>();

        //deviceUI.Set(omniDevice.GetUIData());

        handDeviceData.controllingStructure = false;
    }

    private void Start()
    {
        otherHand = (handSide == EHandSide.LEFT) ? rightHand : leftHand;

        //Temp
        if (handSide == EHandSide.LEFT) rightHand.otherHand = this;

    }

    //Called after hand prefabs are instantiated on network.
    public void Initialize(GameObject spawnedHand)
    {
        handSync = spawnedHand.GetComponent<HandSync>();

        

        if (handSync)
        {
            handSync.OnOmniDeviceActiveChanged += SetOmniDeviceActive;

            OmniDevice od = (OmniDevice)omniDevice;

            od.Initialize(spawnedHand, handSide);

            //Initialize OmniDeviceMenu
            //((UIOmniDeviceMenu)deviceUI).NumberOfDevices = od.NumberOfDevices;
            //((UIOmniDeviceMenu)deviceUI).OnMenuChange += od.SetDeviceMode;


            SetOmniDeviceActive(false);


            if (playerController.OmniDeviceStartupState == OVRPlayerController.EOmniDeviceStartup.OnlyLeft && handSide == EHandSide.LEFT)
            {
                SetOmniDeviceActive(true);
            }

            if (playerController.OmniDeviceStartupState == OVRPlayerController.EOmniDeviceStartup.OnlyRight && handSide == EHandSide.RIGHT)
            {
                SetOmniDeviceActive(true);
            }
            if (playerController.OmniDeviceStartupState == OVRPlayerController.EOmniDeviceStartup.Both)
            {
                SetOmniDeviceActive(true);
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Keeping device object attached to hand while holding
        if (grabbingHandDevice) handDevice.transform.SetPositionAndRotation(transform.position, transform.rotation);

        //Operate handheld device 
        if (handDevice)
        {
            //deviceUI.Operate(handSide);

            //(NOTE!) handDevice.Using() actually operates the device with input detection and all
            handDevice.Using(ref handDeviceData);

            usingHandDevice = handDeviceData.controllingStructure;


            if ((handDeviceData.controllingStructure && handDeviceData.targetStructureAllowsRotation) || 
                  (otherHand.handDeviceData.controllingStructure && otherHand.handDeviceData.targetStructureAllowsRotation) && 
                  playerController.EnableLinearMovement)
                playerController.EnableLinearMovement = playerController.EnableRotation = false;

            else if ((!handDeviceData.controllingStructure && !otherHand.handDeviceData.controllingStructure) && !playerController.EnableLinearMovement) 
                playerController.EnableLinearMovement = playerController.EnableRotation = true;

        }

        if (OVRInput.GetDown(grabButton))
        {
            shouldGrab = true;
            shouldRelease = false;
        }
        else if (OVRInput.GetUp(grabButton))
        {
            shouldRelease = true;
            shouldGrab = false;
        }

        if (grabHandleRumble > 0)
        {
            grabHandleRumble -= 9.0f * Time.deltaTime;




            if (grabHandleRumble <= 0)
            {
                grabHandleRumble = 0;
                OVRInput.SetControllerVibration(0.0f, 0.0f, (handSide == EHandSide.RIGHT) ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch);
            }
        }

        //Extend default rumble limit of 2 seconds
        if (grabbingZipLine && timeOfZiplineRumble > 1.8f)
            OVRInput.SetControllerVibration(0.01f, 0.18f, (handSide == EHandSide.RIGHT) ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch);

    }

    private void OnTriggerStay(Collider other)
    {
        if (usingHandDevice) return;

        else if (shouldGrab)
        {
            shouldGrab = false;

            if (other.gameObject.layer.Equals(layer_GrabHandle))
            {
                GrabHandle(other.gameObject);
                otherHand.ReleaseHandle();
            }

            else if (other.gameObject.layer.Equals(layer_HandDevice))
                GrabDevice(other.gameObject);

            else if (other.gameObject.layer.Equals(layer_ZipLineHandle))
                GrabZipLine(other.gameObject.GetComponent<ZipLineTransport>().TransportDirection);
        }

        else if (shouldRelease) 
        {
            shouldRelease = false;

            if (other.gameObject.layer.Equals(layer_GrabHandle))
                ReleaseHandle();

            else if (other.gameObject.layer.Equals(layer_HandDevice))
                DropDevice();

            else if (other.gameObject.layer.Equals(layer_ZipLineHandle))
                ReleaseZipLine();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (grabbingZipLine && other.gameObject.layer.Equals(layer_ZipLineHandle))
            ReleaseZipLine();
    }

    void GrabHandle(GameObject handle)
    {
        this.handle = handle;
        offsettToHandleOnGrab = transform.position - handle.transform.position;

        handSync.GrabbingGrabHandle = true;
        if (otherHand.handSync.GrabbingGrabHandle) otherHand.handSync.GrabbingGrabHandle = false;

        playerController.RegisterGrabHandleEvent(false, (int)otherHand.handSide);
        playerController.RegisterGrabHandleEvent(true, (int)handSide, handle.transform);

        grabHandleRumble = 1;
        OVRInput.SetControllerVibration(0.001f, 0.5f, (handSide == EHandSide.RIGHT) ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch);
    }

    void ReleaseHandle()
    {
        handle = null;
        handSync.GrabbingGrabHandle = false;
        playerController.RegisterGrabHandleEvent(false, (int)handSide);
    }

    //Rumble stops at 2 seconds as default from OVRInput
    float timeOfZiplineRumble = 0;

    void GrabZipLine(Vector3 moveDirection)
    {
        playerController.SetGrabbingZipLine(true, moveDirection);

        handOffsetToPlayerControllerOnZipLineGrab = transform.position - playerController.transform.position;

        grabbingZipLine = true;

        timeOfZiplineRumble = Time.time;

        handSync.GrabbingGrabHandle = true;

        OVRInput.SetControllerVibration(0.01f, 0.18f, (handSide == EHandSide.RIGHT) ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch);
    }

    void ReleaseZipLine()
    {
        playerController.SetGrabbingZipLine(false, new Vector3(0, 0, 0));

        grabbingZipLine = false;

        handSync.GrabbingGrabHandle = false;

        OVRInput.SetControllerVibration(0, 0, (handSide == EHandSide.RIGHT) ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch);
    }

    void GrabDevice(GameObject device)
    {
        grabbingHandDevice = true;

        handDevice = device.GetComponent<HandDevice>();
        handDevice.Equip(handSide);

        if (handDevice && handDevice.GetRB()) handDevice.GetRB().useGravity = false;

        deviceUI.Set(handDevice.GetUIData());
    }

    void DropDevice()
    {
        grabbingHandDevice = false;

        if (handDevice && handDevice.GetRB()) handDevice.GetRB().useGravity = true;

        handDevice = null;

        if (handSide == EHandSide.RIGHT)
        {
            handDevice = omniDevice;
            deviceUI.Set(omniDevice.GetUIData());
        }
    }

    public void SetOmniDeviceActive(bool active)
    {
        if (active)
        {

            handSync.OmniDeviceActive = true;

            handDevice = omniDevice;
        }

        else
        {
            handDevice = null;

            handSync.OmniDeviceActive = false;
        }
    }
}
