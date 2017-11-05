    int Clock = 3;
    int Tick;
    bool runMode; // 0-stop, 1-test 
    IMyTimerBlock Timer;
    IMyRadioAntenna Antenna;
    IMyTextPanel TPDebug;
    Vector3D LastPos, correctedTargetLocation;
    List<string> BlackList = new List<string>();
    List<IMyTerminalBlock> Cams;
    MyDetectedEntityInfo info;
    double SCAN_DISTANCE = 100;
    float PITCH;
    float PITCHChanges;
    float YAW;
    float YAWChanges;
    float step;
    int LastLockTick;

    public Program()
    {
        Timer = GridTerminalSystem.GetBlockWithName("Timer") as IMyTimerBlock;
        Antenna = GridTerminalSystem.GetBlockWithName("Antenna") as IMyRadioAntenna;
        //TPDebug = GridTerminalSystem.GetBlockWithName("TPDebug") as IMyTextPanel;
        Cams = new List<IMyTerminalBlock>();
        GridTerminalSystem.GetBlocksOfType<IMyCameraBlock>(Cams);
        PITCH = -45;
        PITCHChanges = PITCH;
        YAW = -45;
        YAWChanges = YAW;
        step = 1;
        Tick = 0;
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
        Run();
        Timer.GetActionWithName("TriggerNow").Apply(Timer);
    }
    public void Run()
    {
        List<IMyCameraBlock> list = new List<IMyCameraBlock>();
        list = WhoIsReady();
        if (Convert.ToBoolean(list.Count))
        {
            foreach (IMyCameraBlock cam in list)
            {
                if (PITCHChanges >= Math.Abs(PITCH) && YAWChanges >= Math.Abs(YAW))
                {
                    //TPDebug.WritePublicText(TPDebug.GetPublicText() + "\nDone");
                    //Timer.GetActionWithName("OnOff_Off").Apply(Timer);
                    PITCHChanges = PITCH;
                    YAWChanges = YAW;
                    //break;
                }
                RayCastway(cam, list.Count);
            }
        }
    }

    public void RayCastway(IMyCameraBlock cam, int count)
    {
        if (Vector3D.IsZero(correctedTargetLocation))
            info = cam.Raycast(SCAN_DISTANCE, PITCHChanges, YAWChanges);
        else
            info = cam.Raycast(correctedTargetLocation);
        if (info.HitPosition.HasValue && BlackList.IndexOf(info.Name) == -1)
        {
            int TicksPassed = Tick - LastLockTick;
            LastLockTick = Tick;
            correctedTargetLocation = info.Position + (info.Velocity * TicksPassed / count);
            Antenna.TransmitMessage(correctedTargetLocation.ToString() + "\n" + info.Name);
            //TPDebug.WritePublicText(PITCHChanges.ToString("0.00") + " " + YAWChanges.ToString("0.00") + "\n" + info.Name + "\n" + info.HitPosition.Value);
            LastPos = info.HitPosition.Value;
        }
        else
            correctedTargetLocation = new Vector3D(0, 0, 0);
        /*if (Convert.ToBoolean(correctedTargetLocation.Length()))
            SetGyroOverride(true, correctedTargetLocation);
        else
            SetGyroOverride(true, LastPos);*/
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
            if (Vector3D.IsZero(correctedTargetLocation))
            {
                if (cam.CanScan(SCAN_DISTANCE))
                    list.Add(cam);
            }
            else
                if (cam.CanScan(correctedTargetLocation))
                    list.Add(cam);
        }
        return list;
    }

    public void SetGyroOverride(bool OverrideOnOff, Vector3D TV, float Power = 1)
    {
        var Gyros = new List<IMyTerminalBlock>();
        GridTerminalSystem.SearchBlocksOfName("Gyro", Gyros);
        IMyGyro Gyro = Gyros[0] as IMyGyro;
        if (Gyro != null)
        {
            Vector3D OV = Gyro.GetPosition();     //Get positions of reference blocks.
            Vector3D FV = Gyro.GetPosition() + Gyro.WorldMatrix.Forward;
            Vector3D UV = Gyro.GetPosition() + Gyro.WorldMatrix.Up;
            Vector3D RV = Gyro.GetPosition() + Gyro.WorldMatrix.Right;
            double TVOV = (OV - TV).Length();  //Get magnitudes of vectors.
            double TVFV = (FV - TV).Length();
            double TVUV = (UV - TV).Length();
            double TVRV = (RV - TV).Length();
            double OVFV = (FV - OV).Length();
            double OVUV = (UV - OV).Length();
            double OVRV = (RV - OV).Length();
            double ThetaP = Math.Acos((TVUV * TVUV - OVUV * OVUV - TVOV * TVOV) / (-2 * OVUV * TVOV));  //Use law of cosines to determine angles.
            double ThetaY = Math.Acos((TVRV * TVRV - OVRV * OVRV - TVOV * TVOV) / (-2 * OVRV * TVOV));
            double RPitch = 90 - (ThetaP * 180 / Math.PI);  //Convert from radians to degrees.
            double RYaw = 90 - (ThetaY * 180 / Math.PI);
            if (TVOV < TVFV) RPitch = 180 - RPitch;  //Normalize angles to -180 to 180 degrees.
            if (RPitch > 180) RPitch = -1 * (360 - RPitch);
            if (TVOV < TVFV) RYaw = 180 - RYaw;
            if (RYaw > 180) RYaw = -1 * (360 - RYaw);
            //Pitch = RPitch;  //Set Pitch and Yaw outputs.
            //Yaw = RYaw;
            if ((!Gyro.GyroOverride && OverrideOnOff) || (Gyro.GyroOverride && !OverrideOnOff))
                Gyro.ApplyAction("Override");
            Gyro.SetValue("Power", Power);
            Gyro.SetValue("Yaw", (float)RYaw / 10);
            Gyro.SetValue("Pitch", (float)RPitch / 10);
            //Gyro.SetValue("Yaw", settings.GetDim(0));
            /////  
            //Gyro.SetValue("Pitch", settings.GetDim(1));
            //////  
            //Gyro.SetValue("Roll", settings.GetDim(2));
        }
    }

    public void Save()
    {
    }
