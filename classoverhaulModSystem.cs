using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;

namespace classoverhaul;

public class classoverhaulModSystem : ModSystem
{

	// Called on server and client
	// Useful for registering block/entity classes on both sides
	public override void Start(ICoreAPI api){
		api.RegisterItemClass(Mod.Info.ModID+".backtreck",typeof(itemBacktreck));
		api.RegisterItemClass(Mod.Info.ModID+".quickstep",typeof(itemQuickstep));
		api.RegisterItemClass(Mod.Info.ModID+".tempclock",typeof(itemTempclock));
		api.RegisterItemClass(Mod.Info.ModID+".tempcloth",typeof(itemTempcloth));
	}
	
	ICoreServerAPI sapi;
	public override void StartServerSide(ICoreServerAPI api){
		api.Event.RegisterGameTickListener(onTickServer1s, 1000, 200);
		
		sapi=api;
	}

	public override void StartClientSide(ICoreClientAPI api){

	}

	private void onTickServer1s(float dt){
		foreach(var plr in sapi.World.AllOnlinePlayers){
			if(plr.Entity.WatchedAttributes.GetString("characterClass")=="blackguard"){
				float speed = plr.Entity.Stats.GetBlended("walkspeed");
				float addon = 0;
				for(float i=1f; i>speed; i-=0.01f){
					addon+=0.03f;
				}
				plr.Entity.Stats.Set("meleeWeaponsDamage","classoverhaul:blackguard",addon);
				return;
			}else{
				plr.Entity.Stats.Set("meleeWeaponsDamage","classoverhaul:blackguard",0);
			}
		}
	}
}
