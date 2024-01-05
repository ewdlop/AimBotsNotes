//client

using Swed32;

const int entityList = 0x4D4B1B4;
const int localPlayer = 0x4D3B664;

//engine

const int viewAngles = 0x4D90;

//offsets
const int hp = 0x94;
const int xyz = 0x4;
const int dormant = 0xE9;
const int team = 0xF;

Swed swed32 = new Swed("hl2");
Entity player = new Entity();
List<Entity> entities = new List<Entity>();

var client = swed32.GetModuleBase("client.dll");
var engine = swed32.GetModuleBase("engine.dll");

void UpdateLocal()
{
    var buffer = swed32.ReadPointer(client, localPlayer);
    var teamNumber = swed32.ReadPointer(buffer,team,4);
    var position = swed32.ReadBytes(buffer,xyz,12); //x,y,z 4 bytes each
    player.X = BitConverter.ToSingle(position, 0);
    player.Y = BitConverter.ToSingle(position, 4);
    player.Z = BitConverter.ToSingle(position, 8);
    //player.Team = BitConverter.ToInt32(teamNumber, 0);
}

//not finished
public class Entity
{
    public int Health { get; set; }
    public int Team { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}