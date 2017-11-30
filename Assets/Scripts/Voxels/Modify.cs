﻿//William Dewing 2017
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//Test script for modifying terrain
public class Modify : MonoBehaviour
{
    Vector2 rot;
    public byte selectedBlockID = 1;
    public byte minBlock = 1;
    public byte maxBlock = 9;

    public bool useCameraControls = true;
    public bool allowModify = true;

    public Text blockNameDis;
    private void Start()
    {
    }

    void Update()
    {
        if (!allowModify)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 100))
            {
                if (World.singleton.isClient)
                {
                    NetworkBlocksClient.SetBlock(hit, new BlockAir());
                }
                else
                {
                    EditTerrain.SetBlock(hit, new BlockAir());
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 100))
            {
                if (World.singleton.isClient)
                {
                    NetworkBlocksClient.SetBlock(hit, BlockIDManager.GetBlock(selectedBlockID), true);
                }
                else
                {
                    EditTerrain.SetBlock(hit, BlockIDManager.GetBlock(selectedBlockID), true);
                }
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") >= .1)
        {
            selectedBlockID++;
            if (blockNameDis != null)
            {
                blockNameDis.text = BlockIDManager.GetBlock(selectedBlockID).ToString();
            }
        }
        else if(Input.GetAxis("Mouse ScrollWheel") <= -.1)
        {
            selectedBlockID--;
            if (blockNameDis != null)
            {
                blockNameDis.text = BlockIDManager.GetBlock(selectedBlockID).ToString();
            }
        }
        selectedBlockID = (byte)Mathf.Clamp(selectedBlockID, minBlock, maxBlock);

        if(!useCameraControls)
        {
            return;
        }

        rot = new Vector2(
            rot.x + Input.GetAxis("Mouse X") * 2,
            rot.y + Input.GetAxis("Mouse Y") * 2);

        transform.localRotation = Quaternion.AngleAxis(rot.x, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(rot.y, Vector3.left);

        transform.position += transform.forward * 2 * Input.GetAxis("Vertical");
        transform.position += transform.right * 2 * Input.GetAxis("Horizontal");

        if (blockNameDis == null)
        {
            GameObject dis = GameObject.FindGameObjectWithTag("BlockDis");
            if (dis != null)
            {
                blockNameDis = dis.GetComponent<Text>();
            }
        }
    }


}
