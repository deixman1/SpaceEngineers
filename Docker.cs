using System;
using System.Runtime;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Linq;

namespace Class4
{
    public sealed class Program : MyGridProgram
    {
//////
    //блок кабины и список блоков двигателей
    IMyCockpit Cockpit;
    IMyGyro Gyro;
    List<IMyThrust> Thrusters;
    Vector3D Home = new Vector3D(10,10,10);
    WayPoint wayPointHome;
    List<WayPoint> waypoints = new List<WayPoint>();
    IMyTextSurface panel;
    public class WayPoint : IEquatable<WayPoint>
    {
        public WayPoint(string name, Vector3D position, Matrix matrix, int id)
        {
            Name = name; Position = position; Matrix = matrix; Id = id;
        }
        public string Name { get; set; }
        public Vector3D Position { get; set; }
        public Matrix Matrix { get; set; }
        public int Id { get; set; }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            WayPoint objAsPart = obj as WayPoint;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }
        public bool Equals(WayPoint other)
        {
            if (other == null) return false;
            return (this.Id.Equals(other.Id));
        }
        public override int GetHashCode()
        {
            return Id;
        }
        public override string ToString()
        {
            return this.Name + ","
            + this.Position.X + ";" + this.Position.Y + ";" + this.Position.Z + ","
            + this.Matrix.Forward.X + ";" + this.Matrix.Forward.Y + ";" + this.Matrix.Forward.Z + ";"
            + this.Matrix.Left.X + ";" + this.Matrix.Left.Y + ";" + this.Matrix.Left.Z + ";"
            + this.Matrix.Up.X + ";" + this.Matrix.Up.Y + ";" + this.Matrix.Up.Z + ","
            + this.Id;
        }
        public static WayPoint ConvertTo (string name, string position, string matrix, string id)
        {
            string [] split = position.Split(';');
            Vector3D pos = new Vector3D(Convert.ToDouble(split[0]),Convert.ToDouble(split[1]),Convert.ToDouble(split[2]));
            split = matrix.Split(';');
            Matrix mat = new Matrix();
            mat.Forward = new Vector3(Convert.ToDouble(split[0]),Convert.ToDouble(split[1]),Convert.ToDouble(split[2]));
            mat.Left = new Vector3(Convert.ToDouble(split[3]),Convert.ToDouble(split[4]),Convert.ToDouble(split[5]));
            mat.Up = new Vector3(Convert.ToDouble(split[6]),Convert.ToDouble(split[7]),Convert.ToDouble(split[8]));
            return new WayPoint(name,pos,mat,Convert.ToInt32(id));
        }
    }

    //находим нужные блоки
    public Program()
    {
        Cockpit = GridTerminalSystem.GetBlockWithName("MainCockpit") as IMyCockpit;
        Gyro = GridTerminalSystem.GetBlockWithName("Gyro") as IMyGyro;
        panel = Cockpit.GetSurface(0);
        Thrusters = new List<IMyThrust>();
        
    }

    //в главной функции запускаем скрипт в рабочем режиме или останавливаем в зависимости от аргумента
    public void Main(string argument, UpdateType uType)
    {
        Echo(Storage.Length.ToString());
        GridTerminalSystem.GetBlocksOfType<IMyThrust>(Thrusters);
        if(uType==UpdateType.Update1)
        {
            gyroAngl ();
            CompensateWeight();
        }
        else
        {
            switch(argument)
            {
                case "Stop":
                    stop();
                    waypoints = new List<WayPoint>();
                    break;
                case "Pause":
                    stop();
                    break;
                default:
                    if (waypoints.Exists(x => x.Name == argument))
                    {
                        wayPointHome = waypoints.Find(x => x.Name.Contains(argument));
                        Runtime.UpdateFrequency = UpdateFrequency.Update1;
                        gyroAngl();
                        CompensateWeight();
                    }
                    else
                    {
                        Echo("add");
                        waypoints.Add(new WayPoint(argument, Cockpit.GetPosition(), Cockpit.WorldMatrix, waypoints.Count));
                    }
                    break;
            }

        }
    }
    public void Save() 
    {
        //string str = "";
        //string outPrint = "";
        //foreach (WayPoint waypoint in waypoints)
        //{
        //    str += waypoint.ToString() + ":";
        //    outPrint += waypoint.ToString() + "\n";
        //}
        //str = str.Remove(str.Length - 1);
        //Storage = str;
        //panel.WriteText(outPrint);
        //Echo(Storage);
    }
    public void stop()
    {
        foreach (IMyThrust Thruster in Thrusters)
        {
            Thruster.ThrustOverridePercentage = 0f;
        }
        //Storage = "";
        Gyro.GyroOverride = false;
        Cockpit.DampenersOverride = true;
        Runtime.UpdateFrequency = UpdateFrequency.None;
    }
    public void gyroAngl ()
    {
        //panel.WriteText($"{wayPointHome.Matrix.Forward},{Cockpit.WorldMatrix.Forward}\n{wayPointHome.Matrix.Up},{Cockpit.WorldMatrix.Up}\n{wayPointHome.Matrix.Left},{Cockpit.WorldMatrix.Left}");
        Gyro.GyroOverride = true;
        Vector3D axisForward = wayPointHome.Matrix.Forward.Cross(Cockpit.WorldMatrix.Forward);
        Vector3D axisUp = wayPointHome.Matrix.Up.Cross(Cockpit.WorldMatrix.Up);
        Vector3D axisLeft = wayPointHome.Matrix.Left.Cross(Cockpit.WorldMatrix.Left);
        Vector3D axis = axisForward + axisUp + axisLeft;
        float Roll = (float)axis.Dot(Gyro.WorldMatrix.Backward);
        float Yaw = (float)axis.Dot(Gyro.WorldMatrix.Up);
        float Pitch = (float)axis.Dot(Gyro.WorldMatrix.Right);
        Gyro.Roll = Roll;
        Gyro.Pitch = Pitch;
        Gyro.Yaw = Yaw;
        panel.WriteText($"Pitch {Pitch},Yaw {Yaw},Roll {Roll}\n");
    }
    //это рабочая процедура скрипта. 
    public void CompensateWeight()
    {
        Matrix CockpitMatrix = new MatrixD();
        Matrix ThrusterMatrix = new MatrixD();

        Cockpit.Orientation.GetMatrix(out CockpitMatrix);
        double UpThrMax = 0;
        double DownThrMax = 0;
        double LeftThrMax = 0;
        double RightThrMax = 0;
        double ForwardThrMax = 0;
        double BackwardThrMax = 0;

        foreach (IMyThrust Thruster in Thrusters)
        {
            Thruster.Orientation.GetMatrix(out ThrusterMatrix);
            //Y
            if (ThrusterMatrix.Forward == CockpitMatrix.Up)
            {
                UpThrMax += Thruster.MaxEffectiveThrust;
            }
            else if (ThrusterMatrix.Forward == CockpitMatrix.Down)
            {
                DownThrMax += Thruster.MaxEffectiveThrust;
            }
            //X
            else if (ThrusterMatrix.Forward == CockpitMatrix.Left)
            {
                LeftThrMax += Thruster.MaxEffectiveThrust;
            }
            else if (ThrusterMatrix.Forward == CockpitMatrix.Right)
            {
                RightThrMax += Thruster.MaxEffectiveThrust;
            }
            //Z
            else if (ThrusterMatrix.Forward == CockpitMatrix.Forward)
            {
                ForwardThrMax += Thruster.MaxEffectiveThrust;
            }
            else if (ThrusterMatrix.Forward == CockpitMatrix.Backward)
            {
                BackwardThrMax += Thruster.MaxEffectiveThrust;
            }
        }

        //Vector3D GravityVector = Cockpit.GetNaturalGravity();
        Vector3D LinearVelocity = Cockpit.GetShipVelocities().LinearVelocity;
        float ShipMass = Cockpit.CalculateShipMass().PhysicalMass;
        //double distance = Math.Round((Cockpit.GetPosition()-Home).Length());
        //Vector3D stopWay = ((GravityVector/10)*(GravityVector/10))*ShipMass;
        Vector3D ShipWeight = LinearVelocity*ShipMass;

        Vector3D VectorHome = (Cockpit.GetPosition()-wayPointHome.Position) * 0.1 * ShipMass;

        double ForwardThrust = (VectorHome+ShipWeight).Dot(Cockpit.WorldMatrix.Forward);
        double LeftThrust = (VectorHome+ShipWeight).Dot(Cockpit.WorldMatrix.Left);
        double UpThrust = (VectorHome+ShipWeight).Dot(Cockpit.WorldMatrix.Up);

        double BackwardThrust = -ForwardThrust;
        double RightThrust = -LeftThrust;
        double DownThrust = -UpThrust;
        //panel.WriteText($"{distance}\n{GravityVector.Length().ToString()}\n{ShipWeight.ToString()}\nF{ForwardThrust}, L{LeftThrust},\nU{UpThrust}, B{BackwardThrust},\nR{RightThrust}, D{DownThrust}");

        foreach (IMyThrust Thruster in Thrusters)
        {
            Thruster.Orientation.GetMatrix(out ThrusterMatrix);
            //Y
            if (ThrusterMatrix.Forward == CockpitMatrix.Up)
            {
                Thruster.ThrustOverride = (float)(UpThrust);
            }
            else if (ThrusterMatrix.Forward == CockpitMatrix.Down)
            {
                Thruster.ThrustOverride = (float)(DownThrust);
            }
            //X
            else if (ThrusterMatrix.Forward == CockpitMatrix.Left)
            {
                Thruster.ThrustOverride = (float)(LeftThrust);
            }
            else if (ThrusterMatrix.Forward == CockpitMatrix.Right)
            {
                Thruster.ThrustOverride = (float)(RightThrust);
            }
            //Z
            else if (ThrusterMatrix.Forward == CockpitMatrix.Forward)
            {
                Thruster.ThrustOverride = (float)(ForwardThrust);
            }
            else if (ThrusterMatrix.Forward == CockpitMatrix.Backward)
            {
                Thruster.ThrustOverride = (float)(BackwardThrust);
            }
        }
    }
///////
    }
    
}
