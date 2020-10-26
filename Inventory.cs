using System;
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

namespace space
{
    public sealed class Program : MyGridProgram
    {
        List<IMyTerminalBlock> list;
        Dictionary<string, int> CountItems;
        StringBuilder ListToWrite;
        IMyTextPanel panel;
        public Program()
        {
            panel = GridTerminalSystem.GetBlockWithName("Amount") as IMyTextPanel;
        }

        public void Main(string args)
        {
            ListToWrite = new StringBuilder();
            list = new List<IMyTerminalBlock>();
            CountItems = new Dictionary<string, int>();
            GridTerminalSystem.GetBlocks(list);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].HasInventory && list[i].InventoryCount > 0)
                {
                    for (int j = 0; j < list[i].InventoryCount; j++)
                    {
                        var crateItems = new List<MyInventoryItem>();
                        list[i].GetInventory(j).GetItems(crateItems);
                        for (int k = 0; k < crateItems.Count; k++)
                        {
                            string itemType = crateItems[k].Type.TypeId.Replace("MyObjectBuilder_", "");
                            string itemSubType = crateItems[k].Type.SubtypeId;
                            int itemAmount = (int)crateItems[k].Amount;
                            string itemSubAndType = itemType + "/" + itemSubType;
                            if (CountItems.ContainsKey(itemSubAndType))
                                CountItems[itemSubAndType] += itemAmount;
                            else
                                CountItems.Add(itemSubAndType, itemAmount);
                        }
                    }
                }
            }
            CountItems = CountItems.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
            int maxLength = 0;
            int count = 1;
            foreach (string TypeItem in CountItems.Keys)
            {
                string str = TypeItem + ": " + CountItems[TypeItem].ToString() + " ";
                if (maxLength < str.Length)
                    maxLength = str.Length;
            }
            foreach (string TypeItem in CountItems.Keys)
            {
                string str = TypeItem + ": " + CountItems[TypeItem].ToString();
                if (count % 3 == 0)
                    str += "\n";
                else
                    while (str.Length < maxLength)
                        str += " ";
                count++;
                ListToWrite.Append(str);
            }
            if (maxLength*2 > 68)
                panel.FontSize = 0.680f;
            panel.WriteText(ListToWrite.ToString());
        }

        public void Save()
        {
        }
    }
    
}
