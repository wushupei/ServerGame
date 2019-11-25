
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Character : MonoBehaviour
{
    ClientControl client; //自身客户端
    Dictionary<string, Transform> otherClient = new Dictionary<string, Transform>(); //其它客户端
    public float moveSpeed, rotateSpeed; //移动速度,旋转速度
    public float jumpForce, downSpeed; //跳跃力度,下落速度
    CharacterController cc;
    float y; //y轴速度
    string dataStr = "_";
    public void Init() //初始化
    {
        cc = GetComponent<CharacterController>();
        name += UnityEngine.Random.Range(1, 10000).ToString("0000"); //初始化名字
        Connection();
        client.msg += (s) => dataStr = s;
    }
    public void Move(float x, float z)
    {
        Vector3 moveDir = new Vector3(x, 0, z) * moveSpeed; //移动方向
        if (x != 0 || z != 0) //移动时旋转
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * rotateSpeed);
        cc.Move((moveDir + Vector3.up * y) * Time.deltaTime); //在空间中移动  
    }
    public void Jump(bool keyDown) //跳
    {
        if (cc.isGrounded)
        {
            if (keyDown)
                y = jumpForce;
        }
        else //空中时下落
            y -= downSpeed * Time.deltaTime;
    }

    public void Connection() //连接服务器
    {
        client = new ClientControl();
        try
        {
            client.ConnectionServer("192.168.1.171", 16383, GetSelfData()); //连接服务器,初始化数据
        }
        catch
        {
            Debug.Log("该服务器未运行");
        }
    }
    public string GetSelfData() //获取自身数据,发给服务器
    {
        Vector3 p = transform.position; //位置数据
        Quaternion q = transform.rotation; //旋转数据
        //将名字,位置,旋转等信息组成一条数据进行发送
        string data = name + ","
            + p.x + "," + p.y + "," + p.z + ","
            + q.x + "," + q.y + "," + q.z + "," + q.w + "|";
        return data;
    }


    void SynchronData() //同步数据
    {
        if (dataStr == "_") return;

        char[] ch1 = { '|' };
        char[] ch2 = { ',' };
        //其它客户端的数据有时会几个客户端一起发过来,将它们分开
        string[] arr1 = dataStr.Split(ch1, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < arr1.Length; i++)
        {
            //拿到单条客户端的数据,在解析里面的每条数据
            string[] arr2 = arr1[0].Split(ch2, StringSplitOptions.RemoveEmptyEntries);
            if (!otherClient.ContainsKey(arr2[0])) //如果没有存储的客户端,要存储
            {
                Transform clientTh = Instantiate(Resources.Load<Transform>("PlyaerPrefab/Player"),
                    new Vector3(float.Parse(arr2[1]), float.Parse(arr2[2]), float.Parse(arr2[3])),
                    new Quaternion(float.Parse(arr2[4]), float.Parse(arr2[5]), float.Parse(arr2[6]), float.Parse(arr2[7])));
                clientTh.name = arr2[0];
                otherClient.Add(arr2[0], clientTh);
            }
            else //已经存储的客户端要实时更新位置信息
            {
                otherClient[arr2[0]].SetPositionAndRotation(new Vector3(float.Parse(arr2[1]), float.Parse(arr2[2]), float.Parse(arr2[3])),
                    new Quaternion(float.Parse(arr2[4]), float.Parse(arr2[5]), float.Parse(arr2[6]), float.Parse(arr2[7])));
            }
        }
    }
    private void FixedUpdate()
    {
        try
        {
            client.SendData(GetSelfData());
        }
        catch
        {
            Debug.Log("服务器停止运行");
        }
        SynchronData();
    }
}
