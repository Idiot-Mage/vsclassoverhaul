using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using HarmonyLib;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.API.Datastructures;
using System;
using System.Text;

namespace classoverhaul;

[HarmonyPatch]
public sealed class harmonyPatches : ModSystem{
	public static void updateRarity(ItemStack item,ItemSlot outputSlot){
		Random rng = new Random();
		if(item!=null){
			if(!item.Attributes.HasAttribute("chrarity")){
				int rare = rng.Next(6);
				item.Attributes.SetInt("chrarity",rare);
				
				switch(rare){
					case 1:
					item.Attributes.SetFloat("chwalkspeed",0.05f);
					break;
					case 2:
					item.Attributes.SetFloat("chhunger",-0.05f);
					break;
					case 3:
					item.Attributes.SetFloat("chminespeed",0.1f);
					break;
					case 4:
					item.Attributes.SetFloat("chdurability",0.15f);
					break;
					case 5:
					item.Attributes.SetFloat("chhealing",0.07f);
					break;
				}
				outputSlot.MarkDirty();
			}
		}
	}
}

[HarmonyPatch]
[HarmonyPatch(typeof(ItemWearable))]
public class ItemWearablePatch{
	[HarmonyPostfix]
	[HarmonyPatch("OnCreatedByCrafting"),HarmonyPriority(Priority.Last)]
	public static void Postfix_CollectibleObject_OnCreatedByCrafting(ItemSlot[] inSlots, ItemSlot outputSlot, GridRecipe byRecipe){
		if(outputSlot is DummySlot){return;}
		if(outputSlot.Itemstack.Attributes.HasAttribute("chrarity")){return;}
		
		var inv = inSlots[0].Inventory;
		Entity byPlayer = (inv as InventoryBasePlayer).Owner;
		byPlayer.Api.Event.RegisterCallback(dt =>{
		if(byPlayer!=null && byPlayer.WatchedAttributes.GetString("characterClass")=="tailor"){
			harmonyPatches.updateRarity(outputSlot.Itemstack,outputSlot);	
		}
		},1);
	}
}

[HarmonyPatch]
[HarmonyPatch(typeof(CollectibleObject))]
public class CollectibleObjectPatch{
	[ThreadStatic] private static int callDepth;
	
	[HarmonyPrefix]
	[HarmonyPatch("GetHeldItemInfo")]
	public static void Prefix_GetHeldItemInfo() {
		callDepth++;
	}

	[HarmonyPostfix]
	[HarmonyPatch("GetHeldItemInfo")]
	public static void Postfix_GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo){
		if(callDepth==1){
			int rarity = inSlot.Itemstack.Attributes.GetInt("chrarity",0);
			if(rarity>0){
				string des = "";
				if(inSlot.Itemstack.Attributes.HasAttribute("chwalkspeed")){
					des+="movement boost: "+(inSlot.Itemstack.Attributes.GetFloat("chwalkspeed")*100f).ToString()+"%\n";
				}
				if(inSlot.Itemstack.Attributes.HasAttribute("chhunger")){
					des+="hunger boost: "+(inSlot.Itemstack.Attributes.GetFloat("chhunger")*100f).ToString()+"%\n";
				}
				if(inSlot.Itemstack.Attributes.HasAttribute("chminespeed")){
					des+="mining boost: "+(inSlot.Itemstack.Attributes.GetFloat("chminespeed")*100f).ToString()+"%\n";
				}
				if(inSlot.Itemstack.Attributes.HasAttribute("chdurability")){
					des+="armor durability boost: "+(Math.Round(inSlot.Itemstack.Attributes.GetFloat("chdurability")*100f)).ToString()+"%\n";
				}
				if(inSlot.Itemstack.Attributes.HasAttribute("chhealing")){
					des+="healing boost: "+(inSlot.Itemstack.Attributes.GetFloat("chhealing")*100f).ToString()+"%\n";
				}
				dsc.AppendLine("Masterfully crafted by a skilled tailor.\n"+des);
			}
		}
		callDepth--;
	}
}
