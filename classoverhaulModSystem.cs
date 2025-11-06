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

public class classoverhaulModSystem : ModSystem
{
	private readonly Harmony harmonyInstance = new Harmony(harmonyID);
	public const string harmonyID = "classoverhaulPatches";
	
	public override void Start(ICoreAPI api){
		harmonyInstance.PatchAll();
		
		api.RegisterItemClass(Mod.Info.ModID+".backtreck",typeof(itemBacktreck));
		api.RegisterItemClass(Mod.Info.ModID+".quickstep",typeof(itemQuickstep));
		api.RegisterItemClass(Mod.Info.ModID+".tempclock",typeof(itemTempclock));
		api.RegisterItemClass(Mod.Info.ModID+".tempcloth",typeof(itemTempcloth));
		api.RegisterItemClass(Mod.Info.ModID+".climbtool",typeof(itemClimbtool));
	}
	
	public static ICoreServerAPI chsapi;
	public override void StartServerSide(ICoreServerAPI api){
		api.Event.RegisterGameTickListener(onTickServer1s,1000,200);
		
		chsapi=api;
	}

	ICoreClientAPI chcapi;
	IClientNetworkChannel clientChannel;
	int dashCooldown = 0;
	public override void StartClientSide(ICoreClientAPI api){
		api.Event.RegisterGameTickListener(onTickClient1s,1000,200);
		
		chcapi = api;
		chcapi.Input.RegisterHotKey("Class Abillity", "Class Ability", GlKeys.F, HotkeyType.CharacterControls);
		chcapi.Input.SetHotKeyHandler("Class Abillity", ClassAbility);
	}
	
	private bool ClassAbility(KeyCombination key){
		var plr = chcapi.World.Player.Entity;
		switch(plr.WatchedAttributes.GetString("characterClass")){
			case "malefactor":
				if(plr.Pos.Motion!=Vec3d.Zero && plr.OnGround){
					if(dashCooldown>0){break;}
					dashCooldown=2;
					double mult = (double)plr.Stats.GetBlended("walkspeed")*15;
					double dx = plr.Pos.Motion.X*mult;
					double dy = plr.Pos.Motion.Y+0.1;
					double dz = plr.Pos.Motion.Z*mult;
					plr.Pos.Motion = new Vec3d(dx,dy,dz);
				}
			break;
		}
		return true;
	}
	
	private void onTickClient1s(float dt){
		if(dashCooldown>0){
			dashCooldown--;
		}
	}

	private void onTickServer1s(float dt){
		foreach(var plr in chsapi.World.AllOnlinePlayers){
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
			
			if(plr.Entity.WatchedAttributes.GetString("characterClass")=="clockmaker"){
				double stab = plr.Entity.WatchedAttributes.GetDouble("temporalStability");
				double addon = 0;
				for(double i=1; i>stab; i-=0.01){
					addon+=0.01;
				}
				plr.Entity.Stats.Set("walkspeed","classoverhaul:clockmaker",(float)addon);
			}else{
				plr.Entity.Stats.Set("walkspeed","classoverhaul:clockmaker",0);
			}
			
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
				inv[(int)EnumCharacterDressType.ArmorLegs]
			};
			
			//tryon please add the ability to change item stats at runtime and add any item stat to any item.
			float speedam = 0f;
			float hungeram = 0f;
			float mineam = 0f;
			float duraam = 0f;
			float healam = 0f;
			foreach(var slot in slots){
				if(slot==null || slot.Itemstack==null || slot.Itemstack.Attributes==null){continue;}
				if(slot.Itemstack.Attributes.HasAttribute("chwalkspeed")){
					speedam+=slot.Itemstack.Attributes.GetFloat("chwalkspeed");
				}
				if(slot.Itemstack.Attributes.HasAttribute("chhunger")){
					hungeram+=slot.Itemstack.Attributes.GetFloat("chhunger");
				}
				if(slot.Itemstack.Attributes.HasAttribute("chminespeed")){
					mineam+=slot.Itemstack.Attributes.GetFloat("chminespeed");
				}
				if(slot.Itemstack.Attributes.HasAttribute("chdurability")){
					duraam+=slot.Itemstack.Attributes.GetFloat("chdurability");
				}
				if(slot.Itemstack.Attributes.HasAttribute("chhealing")){
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
