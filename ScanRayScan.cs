    int Clock = 300;
    int Tick = 0;
    bool runMode; // 0-stop, 1-test 
    IMyTimerBlock Timer;
    IMyTextPanel TPDebug;
    List<IMyTerminalBlock> Cams;
    MyDetectedEntityInfo info;
    double SCAN_DISTANCE = 100;
    float PITCH;
    float PITCHChanges;
    float YAW;
    float YAWChanges;
    float step;
    //Vector3D target = new Vector3D(65, 53, -499011);
    Vector3D target = new Vector3D(128.65, 85.17, -498971.66);


    public Program()
    {
        Timer = GridTerminalSystem.GetBlockWithName("Timer") as IMyTimerBlock;
        TPDebug = GridTerminalSystem.GetBlockWithName("TPDebug") as IMyTextPanel;
        Cams = new List<IMyTerminalBlock>();
        GridTerminalSystem.GetBlocksOfType<IMyCameraBlock>(Cams);
        PITCH = -10;
        PITCHChanges = PITCH;
        YAW = -10;
        YAWChanges = YAW;
        step = 5;
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
                    TPDebug.WritePublicText(TPDebug.GetPublicText() + "\nDone");
                    Timer.GetActionWithName("OnOff_Off").Apply(Timer);
                    break;
                }
                RayCastway(cam);
            }

        }
    }
    public void RayCastway(IMyCameraBlock cam)
    {
        info = cam.Raycast(SCAN_DISTANCE, PITCHChanges, YAWChanges);
        if (info.HitPosition.HasValue)
        {
            TPDebug.WritePublicText(TPDebug.GetPublicText() + "{" + PITCHChanges.ToString("0.00") + " " + YAWChanges.ToString("0.00") + ", ");
            TPDebug.WritePublicText(TPDebug.GetPublicText() + info.Name + ", " + Vector3D.Distance(cam.GetPosition(), info.HitPosition.Value).ToString("0.00") + ", " + info.Position.ToString("0.00"));
            TPDebug.WritePublicText(TPDebug.GetPublicText() + "}\n");
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
