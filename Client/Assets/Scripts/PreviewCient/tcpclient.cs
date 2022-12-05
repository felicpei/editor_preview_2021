using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class tcpclient : MonoBehaviour
{
    public Text uiText;
    public Button btnTest;
    public Button btnTest1;
    public Camera cam;
    public Text ui_netinfo;


    Socket serverSocket; //��������socket
    IPAddress ip; //����ip
    IPEndPoint ipEnd;
    string recvStr; //���յ��ַ���
    string sendStr; //���͵��ַ���
    byte[] recvData = new byte[1024]; //���յ����ݣ�����Ϊ�ֽ�
    byte[] sendData = new byte[1024]; //���͵����ݣ�����Ϊ�ֽ�
    int recvLen; //���յ����ݳ���
    Thread connectThread; //�����߳�
                          //string strServerIP = "10.168.1.115";
    string strServerIP = "127.0.0.1";
    const int initport = 5566;
    int port = 0;
    List<string> m_listMsg = new List<string>();
    object lockObj = new object();
    bool m_bConnected = false;
    bool m_bInit = false;
    GameObject m_myGameObject;
    private string[] args = System.Environment.GetCommandLineArgs();

    public bool Connected { get { return m_bConnected; } }
    public bool Inited { get { return m_bInit; } }

    //��ʼ��
    void InitSocket()
    {
        StringBuilder strServerIPBuf = new StringBuilder(512);
        StringBuilder strEnginePathBuf = new StringBuilder(512);

        port = initport;
        for (int i = 0; i < args.Length; i++)
        {
            string str = args[i];
            string[] kv = str.Split(':');
            if (kv.Length > 0)
            {
                if (kv[0] == "-port")
                {
                    port = int.Parse(kv[1]);
                }
                ui_netinfo.text= args[i] + ":" + kv[0] + "#" + (kv.Length > 1 ? kv[1] : "");
                Debug.Log(ui_netinfo.text);
            }
        }

        //�����������IP�Ͷ˿ڣ��˿����������Ӧ
        ip = IPAddress.Parse(strServerIP); //�����Ǿ�����������ip���˴��Ǳ���
        ipEnd = new IPEndPoint(ip, port);

        //����һ���߳����ӣ�����ģ��������߳̿���
        connectThread = new Thread(new ThreadStart(SocketReceive));
        connectThread.Start();
        m_bInit = true;
    }

    public void AddLog(string strMsg)
    {
        lock (lockObj)
        {
            m_listMsg.Add(strMsg.Clone().ToString());
        }
    }
    public string RemoveLog()
    {
        string strRet = "";
        lock (lockObj)
        {
            if (m_listMsg.Count > 0)
            {
                strRet = m_listMsg[0];
                m_listMsg.RemoveAt(0);
            }
        }
        return strRet;
    }

    bool SocketConnect()
    {
        bool bRet = false;
        if (serverSocket != null)
            serverSocket.Close();
        //�����׽�������,���������߳��ж���
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        AddLog("ready to connect port:" + port);
        //����
        //serverSocket.Connect(ipEnd);
        IAsyncResult connResult = serverSocket.BeginConnect(ip, port, null, null);
        connResult.AsyncWaitHandle.WaitOne(200, true);  //�ȴ�2��
        if (!connResult.IsCompleted)
        {
            serverSocket.Close();
            //�������Ӳ��ɹ��Ķ���
            AddLog("connect failed port:" + port);
            bRet = false;
        }
        else
        {
            //�������ӳɹ��Ķ���
            bRet = true;
            AddLog(recvStr);
            recvLen = serverSocket.Receive(recvData);
            recvStr = Encoding.UTF8.GetString(recvData, 0, recvLen);
        }

        //������������յ����ַ���
        m_bConnected = bRet;
        return bRet;
    }

    void SocketSend(string sendStr)
    {
        if (!m_bConnected) return;
        //��շ��ͻ���
        sendData = new byte[1024];
        //��������ת��
        //sendData = Encoding.ASCII.GetBytes(sendStr);
        sendData = Encoding.UTF8.GetBytes(sendStr);
        //����
        if (serverSocket.Connected)
            serverSocket.Send(sendData, sendData.Length, SocketFlags.None);
    }

    void SocketReceive()
    {
        int nRetry = 0;
        while (nRetry < 30)
        {
            bool bContinue = true;
            try
            {
                bContinue = SocketConnect();
                if (bContinue == false)
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception e)
            {
                bContinue = false;
                AddLog("failed connect port:" + port);
                Thread.Sleep(1000);
            }
            try
            {
                //���Ͻ��շ���������������
                while (bContinue)
                {
                    recvData = new byte[1024];
                    recvLen = serverSocket.Receive(recvData);
                    if (recvLen == 0)
                    {
                        SocketConnect();
                        continue;
                    }
                    recvStr = Encoding.UTF8.GetString(recvData, 0, recvLen);
                    AddLog("get message from port:" + port + ", recv string: " + recvStr);

                    //�������reject������Ҫ�������˿ڵ�server
                    if (recvStr == "reject")
                        break;
                }
            }
            catch
            {
                AddLog("**********server shutdown, exit!**********");
                return;
            }
            //port++;
            nRetry++;
            
        }
        AddLog("!!!!!!!!!retry 30 finish��faild connect !");
    }

    void SocketQuit()
    {
        //�ر��߳�
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
        //���رշ�����
        if (serverSocket != null)
            serverSocket.Close();
        AddLog("diconnect port:" + port);
    }   

    //�����˳���ر�����
    public void OnQuit()
    {
        SocketQuit();
    }

    public void Send(string strSend)
    {
        SocketSend(strSend);
    }

    // Start is called before the first frame update
    void Start()
    {
        btnTest.onClick.AddListener(OnClick);

        InitSocket();
    }
    private void OnClick()
    {
        LoadObj("/CartoonFX/CFX Prefabs/Fire/Flames Looped/Color Variants/CFX_FlameA Green Looped.prefab");        
    }
    // Update is called once per frame
    void Update()
    {
        if (m_listMsg.Count > 0)
        {
            string strMsg = m_listMsg[0];
            uiText.text = strMsg;
            Debug.Log(strMsg);

            ProcessMessage(strMsg);
            m_listMsg.RemoveAt(0);
        }
    }
    private void OnApplicationQuit()
    {
        OnQuit();
    }
    void ProcessMessage(string strMsg)
    {
        string[] strArray = strMsg.Split("#");

        if (strArray.Length > 0)
        {
            string strCmd = strArray[0];
            switch (strCmd)
            {
                case "loadobj":
                    Debug.Log("load obj exec:");
                    if (strArray.Length > 1)
                        LoadObj(strArray[1]);
                    break;
                case "unloadobj":
                    Debug.Log("unload obj exec:");
                    UnloadObj();
                    break;
            }
        }
    }
    void UnloadObj()
    {
        Destroy(m_myGameObject);
    }
    void LoadObj(string strFileFullName)
    {
        Debug.Log("load obj exec:" + strFileFullName);
        try
        {
            UnloadObj();
            FirstStart.ShowPreview(strFileFullName);
            ui_netinfo.text = "load obj ok: " + strFileFullName;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            uiText.text = e.Message;
            ui_netinfo.text = e.Message;
        }
    }
}
