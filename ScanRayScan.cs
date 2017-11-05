    int Clock = 1;
    int Tick = 0;
    bool runMode; // 0-stop, 1-test 
    IMyTimerBlock Timer;
    //IMyTextPanel TPDebug;
    IMyRadioAntenna Antenna;
    List<IMyTerminalBlock> Cams;
    MyDetectedEntityInfo info;
    List<string> BlackList = new List<string>();
    double SCAN_DISTANCE = 100;
    float PITCH;
    float PITCHChanges;
    float YAW;
    float YAWChanges;
    float step;


    public Program()
    {
        Timer = GridTerminalSystem.GetBlockWithName("Timer") as IMyTimerBlock;
        //TPDebug = GridTerminalSystem.GetBlockWithName("TPDebug") as IMyTextPanel;
        Antenna = GridTerminalSystem.GetBlockWithName("Antenna") as IMyRadioAntenna;
        Cams = new List<IMyTerminalBlock>();
        GridTerminalSystem.GetBlocksOfType<IMyCameraBlock>(Cams);
        PITCH = -45;
        PITCHChanges = PITCH;
        YAW = -45;
        YAWChanges = YAW;
        step = 1;
        BlackList.Add("Large Grid 3624");
        BlackList.Add("Power of happy incest");
    }

    public void Main(string args)
    {
        Tick++;
        if (args == "Start")
        {
            Timer.GetActionWithName("OnOff_On").Apply(Timer);
            runMode = true;
        }
        if (args == "Stop")
        {
            Timer.GetActionWithName("OnOff_Off").Apply(Timer);
            runMode = false;
        }
        if (runMode && Tick % Clock == 0)
            Run();
        Timer.GetActionWithName("TriggerNow").Apply(Timer);
    }
    public void Run()
    {
        List<IMyCameraBlock> list = new List<IMyCameraBlock>();
        //TPDebug.WritePublicText("");
        list = WhoIsReady();
        if (Convert.ToBoolean(list.Count))
        {
            //for(int i = 0; i < list.Count; i++)
            foreach (IMyCameraBlock cam in list)
            {
                if (PITCHChanges >= Math.Abs(PITCH) && YAWChanges >= Math.Abs(YAW))
                {
                    PITCHChanges = PITCH;
                    YAWChanges = YAW;
                    //TPDebug.WritePublicText(TPDebug.GetPublicText() + "\nDone");
                    //StrSend.Append("\nDone");
                    //Timer.GetActionWithName("OnOff_Off").Apply(Timer);
                    //break;
                }
                RayCastway(cam);
            }

        }
    }
    public void RayCastway(IMyCameraBlock cam)
    {
        info = cam.Raycast(SCAN_DISTANCE, PITCHChanges, YAWChanges);
        if (info.HitPosition.HasValue && BlackList.IndexOf(info.Name) == -1)
        {
            StringBuilder ListToWrite = new StringBuilder();
            ListToWrite.Append("{" + PITCHChanges.ToString("0.00") + " " + YAWChanges.ToString("0.00") + ", ");
            ListToWrite.Append(info.Name + ", " + Vector3D.Distance(cam.GetPosition(), info.HitPosition.Value).ToString("0.00") + ", " + info.Position.ToString("0.00") + "}\n");
            Antenna.TransmitMessage(ListToWrite.ToString());
            //TPDebug.WritePublicText(TPDebug.GetPublicText() + "{" + PITCHChanges.ToString("0.00") + " " + YAWChanges.ToString("0.00") + ", ");
            //TPDebug.WritePublicText(TPDebug.GetPublicText() + info.Name + ", " + Vector3D.Distance(cam.GetPosition(), info.HitPosition.Value).ToString("0.00") + ", " + info.Position.ToString("0.00"));
            //TPDebug.WritePublicText(TPDebug.GetPublicText() + "}\n");
        }
        if (YAWChanges >= Math.Abs(YAW))
        {
            PITCHChanges += step;
            YAWChanges = YAW;
            return;
        }
        YAWChanges += step;
    }
    public List<IMyCameraBlock> WhoIsReady()
    {
        List<IMyCameraBlock> list = new List<IMyCameraBlock>();
        foreach (IMyCameraBlock cam in Cams)
        {
            cam.EnableRaycast = true;
            //TPDebug.WritePublicText(TPDebug.GetPublicText() + cam.AvailableScanRange.ToString("0.00") + " " + YAW + " " + PITCH + "\n");
            if (cam.CanScan(SCAN_DISTANCE))
                list.Add(cam);
        }
        return list;
    }


    public void Save()
    {
    }
//FOr Stantion
/*
    IMyRadioAntenna Antenna = GridTerminalSystem.GetBlockWithName("Antenna") as IMyRadioAntenna;
    IMyTextPanel TPDebug = GridTerminalSystem.GetBlockWithName("TPDebug") as IMyTextPanel;
    TPDebug.WritePublicText(TPDebug.GetPublicText() + args);
*/
