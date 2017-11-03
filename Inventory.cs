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
            if(list[i].HasInventory)
            {
                for (int j = 0; j < list[i].InventoryCount; j++)
                {
                    List<IMyInventoryItem> crateItems = list[i].GetInventory(j).GetItems();
                    for (int k = 0; k < crateItems.Count; k++)
                    {
                        if (CountItems.ContainsKey(crateItems[k].Content.SubtypeName))
                            CountItems[crateItems[k].Content.SubtypeName] += (int)crateItems[k].Amount;
                        else
                            CountItems.Add(crateItems[k].Content.SubtypeName, (int)crateItems[k].Amount);
                    }
                }
            }
        }
        foreach (string TypeItem in CountItems.Keys)
            ListToWrite.Append("Кол-во "+ TypeItem + ": " + CountItems[TypeItem].ToString()+"\n");
        panel.WritePublicText(ListToWrite.ToString());

    }
    public void Save()
    {
    }
