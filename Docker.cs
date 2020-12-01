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
    /*
    * Спасибо ему https://www.youtube.com/channel/UCBC9faYOxS0yBBSS3uOx7LQ
    * Параметры
    * Stop - остановка стыковки и удаление точек стыковки
    * Pause - остановка стыковки без удаление точек
    * Любой параметр - 1 запуск:Добовляется в список точек стыковок
    *                  2 запуск:Запускает стыковку к заданой точке стыковки
    * Без параметров
    * Вывод Hello
    */
    List<IMyCockpit> Cockpits = new List<IMyCockpit>();
    IMyCockpit MainCockpit;
    List<IMyThrust> Thrusters = new List<IMyThrust>();
    List<IMyGyro> Gyros = new List<IMyGyro>();
    List<IMyShipConnector> Connectors = new List<IMyShipConnector>();
    WayPoint wayPointHome;
    List<WayPoint> waypoints = new List<WayPoint>();
    IMyTextSurface panel;
    double MinPower;
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
        /*public override string ToString()
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
        }*/
    }
    public void stop()
    {
        foreach (IMyThrust Thruster in Thrusters)
        {
            Thruster.ThrustOverridePercentage = 0f;
        }
        foreach (IMyGyro Gyro in Gyros)
        {
            Gyro.Pitch = 0;
            Gyro.Roll = 0;
            Gyro.Yaw = 0;
            Gyro.GyroOverride = false;
        }
        //Storage = "";
        MainCockpit.DampenersOverride = true;
        Runtime.UpdateFrequency = UpdateFrequency.None;
    }
    public void Run ()
    {
        GyroAngl();
        CompensateWeight();
        GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(Connectors, (block) => (block.Status == MyShipConnectorStatus.Connectable));
            if (Connectors.Count > 0)
            {
                Connectors[0].Connect();
                stop();
            }
    }
    public void Initialization()
    {
        GridTerminalSystem.GetBlocksOfType<IMyThrust>(Thrusters);
        GridTerminalSystem.GetBlocksOfType<IMyGyro>(Gyros);
        GridTerminalSystem.GetBlocksOfType<IMyCockpit>(Cockpits, (block) => block.IsUnderControl);
        if(Cockpits.Count > 1)
        {
            foreach (IMyCockpit Cockpit in Cockpits)
            {
                if (Cockpit.IsMainCockpit)
                    MainCockpit = Cockpit;
            }
        }
        else
        {
            MainCockpit = Cockpits[0];
        }
        panel = MainCockpit.GetSurface(0);
        CalMinPower();
    }
    public void CalMinPower ()
    {
        Matrix CockpitMatrix = new MatrixD();
        Matrix ThrusterMatrix = new MatrixD();

        MainCockpit.Orientation.GetMatrix(out CockpitMatrix);
        double[] powerThrs = { 0, 0, 0, 0, 0, 0 };
        /*double UpThrMax = 0;
        double DownThrMax = 1;
        double LeftThrMax = 2;
        double RightThrMax = 3;
        double ForwardThrMax = 4;
        double BackwardThrMax = 5;*/

        foreach (IMyThrust Thruster in Thrusters)
        {
            Thruster.Orientation.GetMatrix(out ThrusterMatrix);
            //Y
            if (ThrusterMatrix.Forward == CockpitMatrix.Up)
            {
                powerThrs[0] += Thruster.MaxEffectiveThrust;
            }
            else if (ThrusterMatrix.Forward == CockpitMatrix.Down)
            {
                powerThrs[1] += Thruster.MaxEffectiveThrust;
            }
            //X
            else if (ThrusterMatrix.Forward == CockpitMatrix.Left)
            {
                powerThrs[2] += Thruster.MaxEffectiveThrust;
            }
            else if (ThrusterMatrix.Forward == CockpitMatrix.Right)
            {
                powerThrs[3] += Thruster.MaxEffectiveThrust;
            }
            //Z
            else if (ThrusterMatrix.Forward == CockpitMatrix.Forward)
            {
                powerThrs[4] += Thruster.MaxEffectiveThrust;
            }
            else if (ThrusterMatrix.Forward == CockpitMatrix.Backward)
            {
                powerThrs[5] += Thruster.MaxEffectiveThrust;
            }
        }
        MinPower = powerThrs.Min();
    }
    public void ThrusterOverride (Vector3D VectorHome, Vector3D VectorMove)
    {
        double ForwardThrust = (VectorHome+VectorMove).Dot(MainCockpit.WorldMatrix.Forward);
        double LeftThrust = (VectorHome+VectorMove).Dot(MainCockpit.WorldMatrix.Left);
        double UpThrust = (VectorHome+VectorMove).Dot(MainCockpit.WorldMatrix.Up);

        double BackwardThrust = -ForwardThrust;
        double RightThrust = -LeftThrust;
        double DownThrust = -UpThrust;

        Matrix CockpitMatrix = new MatrixD();
        Matrix ThrusterMatrix = new MatrixD();

        MainCockpit.Orientation.GetMatrix(out CockpitMatrix);
        foreach (IMyThrust Thruster in Thrusters)
        {
            Thruster.Orientation.GetMatrix(out ThrusterMatrix);
            //Y
            if (ThrusterMatrix.Forward == CockpitMatrix.Up)
            {
                Thruster.ThrustOverride = (float)(UpThrust*Thruster.MaxEffectiveThrust);
            }
            else if (ThrusterMatrix.Forward == CockpitMatrix.Down)
            {
                Thruster.ThrustOverride = (float)(DownThrust*Thruster.MaxEffectiveThrust);
            }
            //X
            else if (ThrusterMatrix.Forward == CockpitMatrix.Left)
            {
                Thruster.ThrustOverride = (float)(LeftThrust*Thruster.MaxEffectiveThrust);
            }
            else if (ThrusterMatrix.Forward == CockpitMatrix.Right)
            {
                Thruster.ThrustOverride = (float)(RightThrust*Thruster.MaxEffectiveThrust);
            }
            //Z
            else if (ThrusterMatrix.Forward == CockpitMatrix.Forward)
            {
                Thruster.ThrustOverride = (float)(ForwardThrust*Thruster.MaxEffectiveThrust);
            }
            else if (ThrusterMatrix.Forward == CockpitMatrix.Backward)
            {
                Thruster.ThrustOverride = (float)(BackwardThrust*Thruster.MaxEffectiveThrust);
            }
        }
    }
    public void GyroAngl ()
    {
        foreach (IMyGyro Gyro in Gyros)
        {
            Gyro.GyroOverride = true;
            Vector3D axisForward = wayPointHome.Matrix.Forward.Cross(MainCockpit.WorldMatrix.Forward);
            Vector3D axisUp = wayPointHome.Matrix.Up.Cross(MainCockpit.WorldMatrix.Up);
            Vector3D axisLeft = wayPointHome.Matrix.Left.Cross(MainCockpit.WorldMatrix.Left);
            Vector3D axis = axisForward + axisUp + axisLeft;
            float Roll = (float)axis.Dot(Gyro.WorldMatrix.Backward);
            float Yaw = (float)axis.Dot(Gyro.WorldMatrix.Up);
            float Pitch = (float)axis.Dot(Gyro.WorldMatrix.Right);
            Gyro.Roll = Roll;
            Gyro.Pitch = Pitch;
            Gyro.Yaw = Yaw;
        }
    }
    public void CompensateWeight()
    {

        Vector3D LinearVelocity = MainCockpit.GetShipVelocities().LinearVelocity;
        double ShipMass = MainCockpit.CalculateShipMass().PhysicalMass;
        double MinBoost = MinPower/ShipMass; //a = F/m минимальное ускорение коробля
        double TimeBraking = LinearVelocity.Length()/MinBoost; //t = u/a время торможения
        double BrakingWay = LinearVelocity.Length() * TimeBraking - ((MinBoost*(TimeBraking*TimeBraking))/2); //S = u*t-((a*t^2)/2) тормозной путь
        Vector3D VectorMove = LinearVelocity + Vector3D.Normalize(LinearVelocity) * BrakingWay;

        Vector3D VectorHome = (MainCockpit.GetPosition()-wayPointHome.Position);

        ThrusterOverride(VectorHome, VectorMove);

    }
    public Program()
    {
        Initialization();
    }

    public void Main(string argument, UpdateType uType)
    {
        if(uType==UpdateType.Update1)
        {
            Run();
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
                case "":
                    Echo("Hello");
                    break;
                case null:
                    Echo("Hello");
                    break;
                default:
                    Initialization();
                    if (waypoints.Exists(x => x.Name == argument))
                    {
                        wayPointHome = waypoints.Find(x => x.Name.Contains(argument));
                        Runtime.UpdateFrequency = UpdateFrequency.Update1;
                        Run();
                    }
                    else
                    {
                        Echo("added");
                        waypoints.Add(new WayPoint(argument, MainCockpit.GetPosition(), MainCockpit.WorldMatrix, waypoints.Count));
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

///////
    }
    
}
