void Main(){
	IMyCargoContainer Container = GridTerminalSystem.GetBlockWithName("Medium Cargo Container") as IMyCargoContainer;
	IMyInventory Inventory = Container.GetInventory();
	List<IMyInventoryItem> ItemsInventory = Inventory.GetItems();
	IMyInventoryItem Item = ItemsInventory[0];
	int Amount = (int)Item.Amount;
	Inventory.TransferItemTo(Inventory, 0, ItemsInventory.Count, true, -39000);
}
