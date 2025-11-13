using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;


public class EntityBehaviorClassOverhaul : EntityBehavior{
	public override string PropertyName() => "classHandler";
	
	public EntityBehaviorClassOverhaul(Entity entity): base(entity){
	}
	
	public override void OnGameTick(float deltaTime){
		if(entity==null){return;}
		entity.Stats.Set("meleeWeaponsDamage","classoverhaul:blackguard",0);
		entity.Stats.Set("meleeWeaponsDamage","classoverhaul:blackguard2",0);
		entity.Stats.Set("hungerrate","classoverhaul:blackguard3",0);
		entity.Stats.Set("miningSpeedMul","classoverhaul:blackguard4",0);
		
		switch(entity.WatchedAttributes.GetString("characterClass")){
			case "clockmaker":
				EntityBehaviorTemporalStabilityAffected stab = entity.GetBehavior<EntityBehaviorTemporalStabilityAffected>();
				if(stab.TempStabChangeVelocity>0){
					stab.TempStabChangeVelocity = 0;
				}
			break;
			case "blackguard":
				float speed = entity.Stats.GetBlended("walkspeed");
				float addon = 0;
				for(float i=1f; i>speed; i-=0.01f){
					addon+=0.02f;
				}
				entity.Stats.Set("meleeWeaponsDamage","classoverhaul:blackguard",addon);
				
				EntityBehaviorHunger hunger = entity.GetBehavior<EntityBehaviorHunger>();
				if(hunger!=null){
					float amount = (hunger.Saturation/hunger.MaxSaturation);
					
					float min = 0.3f;
					float mid = 0.6f;
					float max = 0.8f;
					
					if(amount<=min){
						entity.Stats.Set("meleeWeaponsDamage","classoverhaul:blackguard2",-0.3f);
						entity.Stats.Set("hungerrate","classoverhaul:blackguard3",-0.15f);
						entity.Stats.Set("miningSpeedMul","classoverhaul:blackguard4",-0.2f);
					}
					if(amount>=mid){
						entity.Stats.Set("meleeWeaponsDamage","classoverhaul:blackguard2",0.15f);
						entity.Stats.Set("hungerrate","classoverhaul:blackguard3",0.15f);
						entity.Stats.Set("miningSpeedMul","classoverhaul:blackguard4",0.3f);
					}
					if(amount>=max){
						entity.Stats.Set("meleeWeaponsDamage","classoverhaul:blackguard2",0.6f);
						entity.Stats.Set("hungerrate","classoverhaul:blackguard3",0.3f);
						entity.Stats.Set("miningSpeedMul","classoverhaul:blackguard4",0.7f);
					}
				}
			break;
		}
	}
}
