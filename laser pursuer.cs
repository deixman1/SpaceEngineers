//Subsidiary//
List<IMyTerminalBlock> list = new List<IMyTerminalBlock>();   
int tik = 0;
bool work;

void Main(string arg)    
{
GridTerminalSystem.GetBlocksOfType<IMyTimerBlock>(list);
IMyTimerBlock Timer = list[0] as IMyTimerBlock;
if(arg == "Pause")
{
work = false;
tik = 0;
Timer.GetActionWithName("OnOff_Off").Apply(Timer);
GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(list);
IMyProgrammableBlock prog = list[0] as IMyProgrammableBlock; 
prog.GetActionWithName("OnOff_Off").Apply(prog);
GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(list);
IMyRemoteControl remote = list[0] as IMyRemoteControl; 
remote.SetAutoPilotEnabled(false);
Echo("Pause");
}
if(arg == "Start")
{
	GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(list);
IMyProgrammableBlock prog = list[0] as IMyProgrammableBlock; 
prog.GetActionWithName("OnOff_On").Apply(prog);
prog.TryRun("");
Timer.GetActionWithName("OnOff_On").Apply(Timer);
Timer.GetActionWithName("TriggerNow").Apply(Timer);
work = true;
//tik = 10;
Echo("Start");
}
tik++;
if(work)
if(tik % 2 == 0)
{
//GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(list);
//IMyProgrammableBlock prog = list[0] as IMyProgrammableBlock; 
//prog.GetActionWithName("OnOff_Off").Apply(prog);
GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(list);
IMyTextPanel mem = list[1] as IMyTextPanel;
GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(list);
IMyTextPanel panel = list[0] as IMyTextPanel;

Double[] array = new Double[3]; 
int i = 0;
string[] args = mem.GetPublicText().Split(new Char [] {' ', ':', 'Z', 'X', 'Y', ',', '{', '}'}); 
arg = "GPS:Корабль:"; 
foreach (string s in args)  
{   
if (s.Trim() != "")   
{ 
array[i] = Convert.ToDouble(s); 
arg = arg + s + ":"; 
i++; 
} 
}
panel.WritePublicText(arg);
Vector3D stan = new Vector3D(array[0], array[1], array[2]);

double Distan; 
GridTerminalSystem.GetBlocksOfType<IMyLaserAntenna>(list);  
var antenna = list[0] as IMyLaserAntenna;
var pos = antenna.GetPosition();
GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(list);
IMyRemoteControl remote = list[0] as IMyRemoteControl; 
bool GoHome = false;
Distan = (pos - stan).Length(); 
Echo(Distan.ToString());
panel.WritePublicText(arg+"\n"+Distan.ToString());

if(Distan < 50000 && Distan > 200)
GoHome = true;
else
GoHome = false;
remote.SetAutoPilotEnabled(false);
remote.ClearWaypoints();
if(GoHome)
{
remote.AddWaypoint(stan, "GoHome");
remote.SetAutoPilotEnabled(true);
}
else
remote.SetAutoPilotEnabled(false);


//prog.GetActionWithName("OnOff_On").Apply(prog);
//prog.TryRun(stan.ToString());

Timer.GetActionWithName("TriggerNow").Apply(Timer);
}


}
/////////////
//SmallShip//
/////////////
List<IMyTerminalBlock> list = new List<IMyTerminalBlock>();   

void Main(string arg)     
{  

Getp(); 
if(arg != "") 
Out(arg); 

} 
void Out(string arg) 
{ 
Double[] array = new Double[3]; 
int i = 0;    
GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(list); 
var panel = list[ 0 ] as IMyTextPanel; 
string[] args = arg.Split(new Char [] {' ', ':', 'Z', 'X', 'Y', ',', '{', '}'}); 
arg = "GPS:Корабль:"; 
foreach (string s in args)  
{   
if (s.Trim() != "")   
{ 
array[i] = Convert.ToDouble(s); 
arg = arg + s + ":"; 
i++; 
} 
}  
Vector3D stan = new Vector3D(array[0], array[1], array[2]);
var panel2 = list[ 1 ] as IMyTextPanel; 
panel2.WritePublicText(stan.ToString());


} 
 
void Getp() 
{ 
GridTerminalSystem.GetBlocksOfType<IMyLaserAntenna>(list);  
var antenna = list[0] as IMyLaserAntenna;  
//MyTransmitTarget target = MyTransmitTarget.Ally; 
var pos = antenna.GetPosition();  
antenna.TransmitMessage(pos.ToString()); 
Echo(pos.ToString());  
}
/////////
//Stan///
/////////
List<IMyTerminalBlock> list = new List<IMyTerminalBlock>();   
  
void Main(string arg)    
{    
Getp(); 
if(arg != "") 
Out(arg); 
} 
void Out(string arg) 
{ 
GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(list); 
var panel = list[ 0 ] as IMyTextPanel; 
string[] args = arg.Split(new Char [] {' ', ':', 'Z', 'X', 'Y', ',', '{', '}'}); 
arg = "GPS:SmallКорабль:"; 
foreach (string s in args)  
{   
if (s.Trim() != "")   
{ 

arg = arg + s + ":"; 

} 
}  
panel.WritePublicText(arg); 
} 
 
void Getp() 
{ 
GridTerminalSystem.GetBlocksOfType<IMyLaserAntenna>(list);  
var antenna = list[0] as IMyLaserAntenna;  
//MyTransmitTarget target = MyTransmitTarget.Ally; 
var pos = antenna.GetPosition();  
antenna.TransmitMessage(pos.ToString()); 
Echo(pos.ToString());  
}
