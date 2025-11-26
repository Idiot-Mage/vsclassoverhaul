using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;
using HarmonyLib;
using Vintagestory.API.Util;
using Vintagestory.API.Datastructures;
using ProtoBuf;
using Vintagestory.API.MathTools;
using System;

namespace classoverhaul;

public class classoverhaulModSystem : ModSystem{
	public const string harmonyID = "classoverhaulPatches";
	public readonly Harmony harmonyInstance = new Harmony(harmonyID);
	
	public override void Start(ICoreAPI api){
		harmonyInstance.PatchAll();
		
		api.RegisterItemClass(Mod.Info.ModID+".backtreck",typeof(itemBacktreck));
		api.RegisterItemClass(Mod.Info.ModID+".quickstep",typeof(itemQuickstep));
		api.RegisterItemClass(Mod.Info.ModID+".tempclock",typeof(itemTempclock));
		api.RegisterItemClass(Mod.Info.ModID+".tempcloth",typeof(itemTempcloth));
		api.RegisterItemClass(Mod.Info.ModID+".climbtool",typeof(itemClimbtool));
		api.RegisterItemClass(Mod.Info.ModID+".overhaulsling",typeof(itemOverhaulSling));
		api.RegisterItemClass(Mod.Info.ModID+".leadslug",typeof(itemLeadSlug));
		api.RegisterItemClass(Mod.Info.ModID+".reststable",typeof(itemReststable));
		api.RegisterItemClass(Mod.Info.ModID+".ruststable",typeof(itemRuststable));
		
		api.RegisterBlockClass(Mod.Info.ModID+".fakegrass",typeof(blockFakegrass));
		api.RegisterBlockClass(Mod.Info.ModID+".woodenspike",typeof(blockWoodenspike));
		
		api.RegisterEntityBehaviorClass("classHandler", typeof(EntityBehaviorClassOverhaul));
	}
	
	public override void Dispose(){
		if(harmonyInstance!=null){
			harmonyInstance.UnpatchAll("classoverhaulPatches");
		}
	}
	
	public static ICoreServerAPI chsapi;
	public override void StartServerSide(ICoreServerAPI api){
		api.Event.RegisterGameTickListener(onTick,50,0);
		
		chsapi=api;
	}

	ICoreClientAPI chcapi;
	IClientNetworkChannel clientChannel;
	int dashCooldown = 0;
	public override void StartClientSide(ICoreClientAPI api){
		api.Event.RegisterGameTickListener(onTickClient1s,1000,200);
		
		chcapi = api;
		chcapi.Input.RegisterHotKey("Class Abillity", "Class Overhaul: Ability", GlKeys.F, HotkeyType.CharacterControls);
		chcapi.Input.SetHotKeyHandler("Class Abillity", ClassAbility);
	}
	
	private bool ClassAbility(KeyCombination key){
		var plr = chcapi.World.Player.Entity;
		switch(plr.WatchedAttributes.GetString("characterClass")){
			case "malefactor":
				if(plr.Pos.Motion!=Vec3d.Zero && plr.OnGround){
					if(dashCooldown>0){break;}
					double mult = (double)plr.Stats.GetBlended("walkspeed")*15;
					double dx = plr.Pos.Motion.X*mult;
					double dy = plr.Pos.Motion.Y+0.1;
					double dz = plr.Pos.Motion.Z*mult;
					plr.Pos.Motion = new Vec3d(dx,dy,dz);
				}
			break;
			case "hunter":
				
			break;
		}
		return true;
	}
	
	private void onTickClient1s(float dt){
		if(dashCooldown>0){
			dashCooldown--;
		}
	}

	private void onTick(float dt){
		foreach(var plr in chsapi.World.AllOnlinePlayers){
			//to fix old versions, leave this in
			plr.Entity.Stats.Set("walkspeed","classoverhaul:clockmaker",0);
			
			var inv = plr.InventoryManager.GetOwnInventory(GlobalConstants.characterInvClassName);
			if(inv==null){continue;}
			
			//no clue why i cant loop through the enumerator itself and have to do this, but whatever.
			var slots = new []{
				inv[(int)EnumCharacterDressType.Head],
				inv[(int)EnumCharacterDressType.Neck],
				inv[(int)EnumCharacterDressType.Face],
				inv[(int)EnumCharacterDressType.UpperBody],
				inv[(int)EnumCharacterDressType.UpperBodyOver],
				inv[(int)EnumCharacterDressType.Shoulder],
				inv[(int)EnumCharacterDressType.Arm],
				inv[(int)EnumCharacterDressType.Hand],
				inv[(int)EnumCharacterDressType.LowerBody],
				inv[(int)EnumCharacterDressType.Foot],
				inv[(int)EnumCharacterDressType.ArmorHead],
				inv[(int)EnumCharacterDressType.ArmorBody],
				inv[(int)EnumCharacterDressType.ArmorLegs],
				inv[(int)EnumCharacterDressType.Waist],
				inv[(int)EnumCharacterDressType.Emblem]
			};
			
			//tryon please add the ability to change item stats at runtime and add any item stat to any item.
			float speedam = 0f;
			float hungeram = 0f;
			float mineam = 0f;
			float duraam = 0f;
			float healam = 0f;
			foreach(var slot in slots){
				if(slot==null || slot.Itemstack==null || slot.Itemstack.Attributes==null){continue;}
				bool walk = slot.Itemstack.Attributes.HasAttribute("chwalkspeed");
				bool hunger = slot.Itemstack.Attributes.HasAttribute("chhunger");
				bool minespeed = slot.Itemstack.Attributes.HasAttribute("chminespeed");
				bool durab = slot.Itemstack.Attributes.HasAttribute("chdurability");
				bool healr = slot.Itemstack.Attributes.HasAttribute("chhealing");
				
				if((walk||hunger||minespeed||durab||healr) && !slot.Itemstack.Attributes.HasAttribute("equipped")){
					slot.Itemstack.Attributes.SetBool("equipped",true);
					slot.MarkDirty();
				}
				
				if(walk){
					speedam+=slot.Itemstack.Attributes.GetFloat("chwalkspeed");
				}
				if(hunger){
					hungeram+=slot.Itemstack.Attributes.GetFloat("chhunger");
				}
				if(minespeed){
					mineam+=slot.Itemstack.Attributes.GetFloat("chminespeed");
				}
				if(durab){
					duraam+=slot.Itemstack.Attributes.GetFloat("chdurability");
				}
				if(healr){
					healam+=slot.Itemstack.Attributes.GetFloat("chhealing");
				}
			}
			plr.Entity.Stats.Set("walkspeed", "chwalkspeed", speedam);
			plr.Entity.Stats.Set("hungerrate", "chhunger", hungeram);
			plr.Entity.Stats.Set("miningSpeedMul", "chminespeed", mineam);
			plr.Entity.Stats.Set("armorDurabilityLoss", "chdurability", duraam);
			plr.Entity.Stats.Set("healingeffectivness", "chhealing", healam);
		}
	}
}
