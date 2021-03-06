      
IMyTimerBlock Timer;         
IMyRemoteControl RemCon;        
int TickCount;      
int Clock = 1;   
int distance = 50000;      
bool Stop;   
bool or;    
PIDcontrolV3D PID;   
float GyroMult=1;      
 float GyroBrake=100;      
Vector3D PrevPos, Velocity, PrevAngle;   
Vector3D Bunker = new Vector3D(22575.44,140950.11,-108046.63); 
//GPS:Power of happy incest #2:22575.44:140950.11:-108046.63:
    
Vector3D Earth = new Vector3D(0,0,0);    
void Main(string argument)      
{      
    TickCount++;      
    if (Timer==null)      
        Timer=GridTerminalSystem.GetBlockWithName("Timer") as IMyTimerBlock;      
   
    if (PID==null)       
        PID= new PIDcontrolV3D(this,3,0.000,100,0.005,0.05,1,1,3);                
    if (RemCon==null)      
        RemCon=GridTerminalSystem.GetBlockWithName("RemCon") as IMyRemoteControl;          
    switch (argument)          
    {          
        case "Start":          
        {          
            Stop=false;         
                        (GridTerminalSystem.GetBlockWithName("FThruster") as IMyTerminalBlock).ApplyAction("OnOff_On");    
                        (GridTerminalSystem.GetBlockWithName("FThruster2") as IMyTerminalBlock).ApplyAction("OnOff_On");         
            break;      
        }        
        case "Stop":          
        {          
            Stop=true;          
            break;          
        }       
        default:      
            break;      
    }       
      
          
    if ((TickCount%Clock==0)&&(TickCount>0))      
    {        
        if(RemCon.GetNaturalGravity().Length() == 0)  
        {  
        or = true;  
        Vector3D GyrAng = GetNavAngles(Bunker);     
        SetGyroOverride(true, PID.update(GyrAng,TickCount));   
        }  
        else  
        {  
        or = false;  
        Vector3D NavAngle;   
        Velocity = RemCon.GetPosition()-PrevPos;   
        PrevPos = RemCon.GetPosition();  
  
        if ((RemCon.GetPosition()-Bunker).Length()>distance)   
            NavAngle=GetNavAnglesCruise(Bunker);   
        else   
            NavAngle=GetNavAnglesJaveline(Bunker);             
           
        Vector3D AngVel =  NavAngle - PrevAngle;     
        PrevAngle = NavAngle;     
        NavAngle =  NavAngle + AngVel/Clock*GyroBrake;    
        SetGyroOverride(true, NavAngle*GyroMult);   
        }  
    }        
          
    if (!Stop)      
        Timer.ApplyAction("TriggerNow");      
    else      
        SetGyroOverride(false, new Vector3(0,0,0));      
}      
Vector3D GetNavAnglesCruise(Vector3D Target)          
{          
    Vector3D V3Dcenter = RemCon.GetPosition();          
    Vector3D V3Dfow = RemCon.WorldMatrix.Forward;          
    Vector3D V3Dup = RemCon.WorldMatrix.Up;          
    Vector3D V3Dleft = RemCon.WorldMatrix.Left;          
    Vector3D GravNorm = Vector3D.Normalize(RemCon.GetNaturalGravity());   
       
    Vector3D TargetNorm = Vector3D.Normalize(Vector3D.Reject(Target - V3Dcenter, GravNorm));        
      
    double TargetPitch = Math.Acos(Vector3D.Dot(V3Dup, Vector3D.Normalize(Vector3D.Reject(TargetNorm,V3Dleft)))) - (Math.PI / 2);     
       
    double TargetYaw = Math.Acos(Vector3D.Dot(V3Dleft, Vector3D.Normalize(Vector3D.Reject(TargetNorm,V3Dup)))) - (Math.PI / 2);          
          
    double TargetRoll = Math.Acos(Vector3D.Dot(V3Dleft, Vector3D.Normalize(Vector3D.Reject(-GravNorm,V3Dfow)))) - (Math.PI / 2);      
          
    return new Vector3D(TargetYaw, -TargetPitch, TargetRoll);          
}       
   
Vector3D GetNavAnglesJaveline(Vector3D Target)          
{          
    Vector3D V3Dcenter = RemCon.GetPosition();          
    Vector3D V3Dfow = RemCon.WorldMatrix.Forward;          
    Vector3D V3Dup = RemCon.WorldMatrix.Up;          
    Vector3D V3Dleft = RemCon.WorldMatrix.Left;          
    Vector3D GravNorm = Vector3D.Normalize(RemCon.GetNaturalGravity());      
    Vector3D TargetNorm = Vector3D.Normalize(Target - V3Dcenter);        
    if (GravNorm.Dot(TargetNorm)<0.98)   
        TargetNorm = -Vector3D.Normalize(Vector3D.Reflect(GravNorm,TargetNorm));       
       
    double TargetPitch = Math.Acos(Vector3D.Dot(V3Dup, Vector3D.Normalize(Vector3D.Reject(TargetNorm,V3Dleft)))) - (Math.PI / 2);     
       
    double TargetYaw = Math.Acos(Vector3D.Dot(V3Dleft, Vector3D.Normalize(Vector3D.Reject(TargetNorm,V3Dup)))) - (Math.PI / 2);          
          
    double TargetRoll = Math.Acos(Vector3D.Dot(V3Dleft, Vector3D.Normalize(Vector3D.Reject(-Velocity,V3Dfow)))) - (Math.PI / 2);      
          
    return new Vector3D(TargetYaw, -TargetPitch, TargetRoll)*5;          
}   
  
Vector3D GetNavAngles(Vector3D Target)          
{          
    Vector3D V3Dcenter = RemCon.GetPosition();          
    Vector3D V3Dfow = RemCon.WorldMatrix.Forward;          
    Vector3D V3Dup = RemCon.WorldMatrix.Up;          
    Vector3D V3Dleft = RemCon.WorldMatrix.Left;          
    Vector3D TargetNorm = Vector3D.Normalize(Target - V3Dcenter);          
    Vector3D GravNorm = Vector3D.Normalize(-RemCon.GetNaturalGravity());          
   
       
    double TargetYaw = Math.Acos(Vector3D.Dot(V3Dfow, Vector3D.Normalize(Vector3D.Reject(TargetNorm,V3Dup))));   
    if ((Vector3D.Dot(V3Dleft, Vector3D.Normalize(Vector3D.Reject(TargetNorm,V3Dup)))>0))   
        TargetYaw=-TargetYaw;   
       
    double TargetPitch = Math.Acos(Vector3D.Dot(V3Dfow, Vector3D.Normalize(Vector3D.Reject(TargetNorm,V3Dleft))));   
    if ((Vector3D.Dot(V3Dup, Vector3D.Normalize(Vector3D.Reject(TargetNorm,V3Dleft)))>0))   
        TargetPitch=-TargetPitch;   
       
    double TargetRoll = Math.Acos(Vector3D.Dot(V3Dup, Vector3D.Normalize(Vector3D.Reject(GravNorm,V3Dfow))));   
    if ((Vector3D.Dot(V3Dleft, Vector3D.Normalize(Vector3D.Reject(GravNorm,V3Dfow)))>0))   
        TargetRoll=-TargetRoll;   
    if (Math.Abs(TargetPitch)>Math.PI/2) TargetPitch=0;   
    if (double.IsNaN(TargetYaw)) TargetYaw=0;          
    if (double.IsNaN(TargetPitch)) TargetPitch=0;          
    if (double.IsNaN(TargetRoll)) TargetRoll=0;        
    return new Vector3D(TargetYaw, TargetPitch, TargetRoll);          
}       
      
void SetGyroOverride(bool OverrideOnOff, Vector3 settings, float Power = 1)          
{          
    var Gyros = new List<IMyTerminalBlock>();          
    GridTerminalSystem.SearchBlocksOfName("Gyro", Gyros);               
        IMyGyro Gyro = Gyros[0] as IMyGyro;          
        if (Gyro != null)          
        {          
            if ((!Gyro.GyroOverride && OverrideOnOff) || (Gyro.GyroOverride && !OverrideOnOff))         
                Gyro.ApplyAction("Override");          
            Gyro.SetValue("Power", Power);          
            Gyro.SetValue("Yaw", settings.GetDim(0));   
            ///  
            if(or)  
            Gyro.SetValue("Pitch", -settings.GetDim(1));  
            else  
            Gyro.SetValue("Pitch", settings.GetDim(1));  
            ////  
            Gyro.SetValue("Roll", settings.GetDim(2));          
        }                 
}   
   
      
public class PIDcontrolV3D    
{    
    public double kP{get;set;}      
    public double kI{get;set;}    
    public double kD{get;set;}    
    public double ITresh{get;set;}    
    public double ILock{get;set;}    
    public double IUpLim{get;set;}    
    public double ILowLim{get;set;}    
    public double SigLim{get;set;}    
        
    public Vector3D prevP{get;private set;}    
    public Vector3D P{get;private set;}    
    public Vector3D I{get;private set;}    
    public Vector3D D{get;private set;}    
    public double prevT{get;private set;}    
    public double T{get;private set;}    
    public Vector3D Signal{get;private set;}    
    public double dT{get;private set;}    
   
    internal static Program ParentProgram;        
        
    public PIDcontrolV3D(Program MyProg, double setKP, double setKI, double setKD, double setITresh, double setILock, double setIUpLim, double setILowLim, double setSigLim)    
    {    
        ParentProgram = MyProg;   
        kP = setKP;     
        kI = setKI;    
        kD = setKD;    
        ITresh = setITresh;    
        ILock = setILock;    
        IUpLim = setIUpLim;    
        ILowLim = setILowLim;    
        SigLim = setSigLim;   
   
        P=new Vector3D(0,0,0);    
        prevP=new Vector3D(0,0,0);    
        I=new Vector3D(0,0,0);    
        D=new Vector3D(0,0,0);    
        prevT=0;    
        Signal=new Vector3D(0,0,0);    
        dT=1;    
    }   
    public void reset()   
    {   
        I = new Vector3D(0,0,0);       
    }       
    public Vector3D update(Vector3D newP, double newT = -1)    
    {    
        prevT=T;        
        T=newT;    
        if (T==-1)    
        dT=1;    
        else        
        dT = T - prevT;    
            
        prevP=P;        
        P=newP;    
        D = (P-prevP) / dT;    
        int P0sign =(P.GetDim(0)>=0)?1:-1;    
        int P1sign =(P.GetDim(1)>=0)?1:-1;    
        int P2sign =(P.GetDim(2)>=0)?1:-1;    
        double I0=0, I1=0, I2=0;;    
        if ((D.GetDim(0)*P0sign<ITresh/dT) && (P.GetDim(0)*P0sign<ILock))   
            I0=Math.Max(Math.Min(Math.Abs(P.GetDim(0)),IUpLim),ILowLim)*P0sign/dT;      
        if ((D.GetDim(1)*P1sign<ITresh/dT) && (P.GetDim(1)*P1sign<ILock))   
            I1=Math.Max(Math.Min(Math.Abs(P.GetDim(1)),IUpLim),ILowLim)*P1sign/dT;      
        if ((D.GetDim(2)*P2sign<ITresh/dT) && (P.GetDim(2)*P2sign<ILock))    
            I2=Math.Max(Math.Min(Math.Abs(P.GetDim(2)),IUpLim),ILowLim)*P2sign/dT;      
        I=I+(new Vector3D(I0,I1,I2) / dT);    
        Signal = kP*P + kI*I + kD*D;    
//      if (T%6==0) ParentProgram.TP.WritePublicText(P.ToString()+";"+D.ToString()+";"+I.ToString()+";"+"\n",true);   
        return V3DLimit(Signal, SigLim);    
    }    
   
    private Vector3D V3DLimit(Vector3D vec, double lim)   
    {   
        return new Vector3D(   
        vec.GetDim(0) > 0 ? Math.Min(vec.GetDim(0),lim) : Math.Max(vec.GetDim(0),-lim),   
        vec.GetDim(1) > 0 ? Math.Min(vec.GetDim(1),lim) : Math.Max(vec.GetDim(1),-lim),   
        vec.GetDim(2) > 0 ? Math.Min(vec.GetDim(2),lim) : Math.Max(vec.GetDim(2),-lim)   
        );   
    }    
}   
