int FeAmount = 0;
int CbAmount = 0;
int NiAmount = 0;
int MgAmount = 0;
int AuAmount = 0;
int AgAmount = 0;
int PtAmount = 0;
int SiAmount = 0;
int UAmount = 0;
int StoneAmount = 0;
int IceAmount = 0;
List<IMyTerminalBlock> list = new List<IMyTerminalBlock>();
void Main()
{
GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(list);
FeAmount = 0;
CbAmount = 0;
NiAmount = 0;
MgAmount = 0;
AuAmount = 0;
AgAmount = 0;
PtAmount = 0;
SiAmount = 0;
UAmount = 0;
StoneAmount = 0;
IceAmount = 0;
for (int i = 0; i < list.Count; i++) 
{ 
IMyTerminalBlock CargoOwner = list[i]; 
if (CargoOwner != null) 
{ 
 
var crateItems = CargoOwner.GetInventory(0).GetItems(); 

for (int j = crateItems.Count - 1; j >= 0; j--) 
{ 
if (crateItems[j].Content.SubtypeName == "Iron") 
FeAmount += (int)crateItems[j].Amount; 
else if (crateItems[j].Content.SubtypeName == "Cobalt") 
CbAmount += (int)crateItems[j].Amount; 
else if (crateItems[j].Content.SubtypeName == "Nickel") 
NiAmount += (int)crateItems[j].Amount; 
else if (crateItems[j].Content.SubtypeName == "Magnesium") 
MgAmount += (int)crateItems[j].Amount; 
else if (crateItems[j].Content.SubtypeName == "Gold") 
AuAmount += (int)crateItems[j].Amount; 
else if (crateItems[j].Content.SubtypeName == "Silver") 
AgAmount += (int)crateItems[j].Amount; 
else if (crateItems[j].Content.SubtypeName == "Platinum") 
PtAmount += (int)crateItems[j].Amount; 
else if (crateItems[j].Content.SubtypeName == "Silicon") 
SiAmount += (int)crateItems[j].Amount; 
else if (crateItems[j].Content.SubtypeName == "Uranium") 
UAmount += (int)crateItems[j].Amount; 
else if (crateItems[j].Content.SubtypeName == "Stone") 
StoneAmount += (int)crateItems[j].Amount; 
else if (crateItems[j].Content.SubtypeName == "Ice") 
IceAmount += (int)crateItems[j].Amount; 
} 
}
}
//}
IMyTextPanel panel = GridTerminalSystem.GetBlockWithName("Amount") as IMyTextPanel;
panel.WritePublicText("В КОНТЕЙНЕРАХ\nКол-во Железа: "+FeAmount.ToString()+"\n"+"Кол-во Кобальта: "+CbAmount.ToString()+"\n"+"Кол-во Никеля: "+NiAmount.ToString()+"\n"+"Кол-во Мг: "+MgAmount.ToString()+"\n"+"Кол-во Сильвера: "+AuAmount.ToString()+"\n"+"Кол-во Золота: "+AgAmount.ToString()+"\n"+"Кол-во Платины: "+PtAmount.ToString()+"\n"+"Кол-во Кремня: "+SiAmount.ToString()+"\n"+"Кол-во Урана: "+UAmount.ToString()+"\n"+"Кол-во Камня: "+StoneAmount.ToString()+"\n"+"Кол-во Льда: "+IceAmount.ToString());

}
