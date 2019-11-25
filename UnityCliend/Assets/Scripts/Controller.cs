using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    Character ch;
    void Start()
    {
        ch = GetComponent<Character>();
        ch.Init();
    }

    void Update()
    {
        ch.Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); //移动
        ch.Jump(Input.GetKeyDown(KeyCode.Space)); //跳跃
    }
}
