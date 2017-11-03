    int Ticks, Clock;
    public Program()
    {
        Ticks = 0;
        Clock = 4;
    }
    public void Main(string args)
    {
        Ticks++;
        IMyRadioAntenna Antenna = GridTerminalSystem.GetBlockWithName("Antenna") as IMyRadioAntenna;
        IMyTimerBlock Timer = GridTerminalSystem.GetBlockWithName("TimerBlock") as IMyTimerBlock;
        Antenna.GetActionWithName("OnOff_On").Apply(Antenna);
        Timer.GetActionWithName("OnOff_On").Apply(Timer);
        if (Ticks % Clock == 0)
        {
            IMyTextPanel TextPanel = GridTerminalSystem.GetBlockWithName("Debug") as IMyTextPanel;
            //MyTransmitTarget target = MyTransmitTarget.Ally; 
            var pos = Antenna.GetPosition();
            if (Antenna.TransmitMessage(pos.ToString()))
                Antenna.GetActionWithName("OnOff_Off").Apply(Antenna);
            TextPanel.WritePublicText(pos.ToString());
            Timer.GetActionWithName("OnOff_Off").Apply(Timer);
        }
        Timer.GetActionWithName("TriggerNow").Apply(Timer);
    }
    public void Save()
    {
    }
