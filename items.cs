using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Util;
using Vintagestory.API.Datastructures;


internal class itemLeadSlug : Item{
	public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo){
		base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
		dsc.AppendLine("5 blunt damage when used with sling");	
	}
}


internal class itemClimbtool : Item{
	public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo){
		base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
		dsc.AppendLine("Use to climb blocks you are looking at");	
	}
	
	public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling){
		handHandling = EnumHandHandling.Handled;
	}
	
	public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel){
		if(blockSel!=null){
			Vec3d bp = new Vec3d(blockSel.Position.X,blockSel.Position.Y,blockSel.Position.Z);
			if(byEntity.Pos.DistanceTo(bp)<=2.6 && blockSel.Position.Y>byEntity.Pos.Y){
				if(byEntity.WatchedAttributes.GetString("characterClass")=="malefactor"){
					byEntity.Pos.Motion.Y = 0.08;
				}else{
					byEntity.Pos.Motion.Y = 0.05;
				}
				
				if(secondsUsed>=2){
					slot.Itemstack.Attributes.SetInt("durability",slot.Itemstack.Collectible.GetRemainingDurability(slot.Itemstack)-1);
					if(slot.Itemstack.Collectible.GetRemainingDurability(slot.Itemstack)<=0){
						slot.TakeOutWhole();
					}
					slot.MarkDirty();
					return false;
				}
			}
		}
		return true;
	}
}






public class Parts{
	public void spawnParts(EntityAgent entity,Vec3d pos){
		//copied from vanilla translocator
		SimpleParticleProperties teleportParticles = new SimpleParticleProperties(
			0.5f, 1,
			ColorUtil.ToRgba(150, 92, 111, 107),
			new Vec3d(),
			new Vec3d(),
			new Vec3f(-0.2f, -0.2f, -0.2f),
			new Vec3f(0.2f, 0.2f, 0.2f),
			4.5f,
			0,
			0.5f,
			0.75f,
			EnumParticleModel.Quad
		);

		teleportParticles.OpacityEvolve = EvolvingNatFloat.create(EnumTransformFunction.QUADRATIC, -1f);
		teleportParticles.AddPos.Set(1, 2, 1);
		teleportParticles.addLifeLength = 0.5f;
		
		teleportParticles.MinPos.Set(pos.X, pos.Y, pos.Z);
		teleportParticles.AddPos.Set(1, 1.8, 1);
		teleportParticles.MinVelocity.Set(-1, -1, -1);
		teleportParticles.AddVelocity.Set(2, 2, 2);
		teleportParticles.MinQuantity = 150;
		teleportParticles.AddQuantity = 0.5f;


		int r = 53;
		int g = 221;
		int b = 172;
		teleportParticles.Color = (r << 16) | (g << 8) | (b << 0) | (100 << 24);

		teleportParticles.BlueEvolve = null;
		teleportParticles.RedEvolve = null;
		teleportParticles.GreenEvolve = null;
		teleportParticles.MinSize = 0.1f;
		teleportParticles.MaxSize = 0.2f;
		teleportParticles.SizeEvolve = null;
		teleportParticles.OpacityEvolve = EvolvingNatFloat.create(EnumTransformFunction.QUADRATIC, -10f);

		entity.World.SpawnParticles(teleportParticles);
	}
}


internal class itemReststable : Item{
	readonly double hours = 2.5;
	public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling){
		handHandling = EnumHandHandling.Handled;
		
		double lastUsed = slot.Itemstack.Attributes.GetDouble("lastUsed",0);
		if(byEntity.World.Calendar.TotalHours-lastUsed>hours && byEntity.WatchedAttributes.GetDouble("temporalStability")<0.99f){
			slot.Itemstack.Attributes.SetDouble("lastUsed",byEntity.World.Calendar.TotalHours);
			byEntity.WatchedAttributes.SetDouble("temporalStability",byEntity.WatchedAttributes.GetDouble("temporalStability")+0.2);
			slot.MarkDirty();
			
			byEntity.World.PlaySoundAt(new AssetLocation("game:sounds/effect/translocate-breakdimension"),byEntity.ServerPos.X,byEntity.ServerPos.Y,byEntity.ServerPos.Z,null,true,10f,2f);
			
			Parts p = new Parts();
			p.spawnParts(byEntity,new Vec3d(byEntity.ServerPos.X,byEntity.ServerPos.Y,byEntity.ServerPos.Z));
		}
	}
	
	public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo){
		base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
		if(world.Side==EnumAppSide.Client){
			var capi = world.Api as ICoreClientAPI;
			var player = capi?.World?.Player;
			if(player!=null && player.Entity.WatchedAttributes.GetString("characterClass")=="clockmaker"){
				string rdy = "Status: Ready";
				double lastUsed = inSlot.Itemstack.Attributes.GetDouble("lastUsed",0);
				if(player.Entity.World.Calendar.TotalHours-lastUsed<=hours){
					rdy = "Status: Unwinding";
				}			
				dsc.AppendLine("Restores 20% temporal stability when used\n\n"+rdy);
			}else{
				dsc.AppendLine("A strange clock.\nYou have no idea how it works.");
			}
		}
	}
}

internal class itemRuststable : Item{
	public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling){
		handHandling = EnumHandHandling.Handled;
		
		slot.TakeOut(1);
		slot.MarkDirty();
		byEntity.WatchedAttributes.SetDouble("temporalStability",byEntity.WatchedAttributes.GetDouble("temporalStability")+0.1);
		
		Parts p = new Parts();
		p.spawnParts(byEntity,new Vec3d(byEntity.ServerPos.X,byEntity.ServerPos.Y,byEntity.ServerPos.Z));
	}
	
	public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo){
		base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
		dsc.AppendLine("These old gears may still have some power left..\nRestores 10% temporal stability when used");
	}
}


internal class itemTempcloth : Item{
	public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo){
		base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
		
		if(world.Side==EnumAppSide.Client){
			var capi = world.Api as ICoreClientAPI;
			var player = capi?.World?.Player;
			if(player!=null && player.Entity.WatchedAttributes.GetString("characterClass")=="tailor"){
				dsc.AppendLine("Instantly restores 5hp regardless of healing bonus for all players within 5 blocks.\n\nWhen used at full health, provides an overheal, granting invulnurability for 1 hit.");
			}else{
				dsc.AppendLine("Instantly restores 5hp regardless of healing bonus.\n\nWhen used at full health, provides an overheal, granting invulnurability for 1 hit.");
			}
		}
	}
	
		public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling){
			handHandling = EnumHandHandling.Handled;
			if(byEntity.WatchedAttributes.GetString("characterClass")!="tailor"){
				EntityBehaviorHealth hp = byEntity.GetBehavior<EntityBehaviorHealth>();
				if(hp!=null){
					hp.Health+=5f;
					byEntity.WatchedAttributes.SetDouble("temporalStability",byEntity.WatchedAttributes.GetDouble("temporalStability")-0.1);
					
					if(hp.Health>hp.MaxHealth){
						hp.Health=99999f;
					}
					slot.TakeOut(1);
					slot.MarkDirty();
				}
			}else{
				foreach(var plr in byEntity.World.AllOnlinePlayers){
					if(plr.Entity.ServerPos.DistanceTo(byEntity.ServerPos)<=5){
						EntityBehaviorHealth hp = byEntity.GetBehavior<EntityBehaviorHealth>();
						if(hp!=null){
							hp.Health+=5f;
							
							if(hp.Health>hp.MaxHealth){
								hp.Health=99999f;
							}
						}
					}
				}
				
				byEntity.WatchedAttributes.SetDouble("temporalStability",byEntity.WatchedAttributes.GetDouble("temporalStability")-0.05);
				Parts p = new Parts();
				p.spawnParts(byEntity,new Vec3d(byEntity.ServerPos.X,byEntity.ServerPos.Y,byEntity.ServerPos.Z));
				
				slot.TakeOut(1);
				slot.MarkDirty();
			}
		}
	
}

internal class itemTempclock : Item{
	public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo){
		base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
		dsc.AppendLine("Fast forward 10 days.");
	}
	
	public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling){
		handHandling = EnumHandHandling.Handled;
		if(blockSel == null){return;}
		
		int prevDura = slot.Itemstack.Collectible.GetRemainingDurability(slot.Itemstack);
		BlockEntity lookingat = byEntity.World.BlockAccessor.GetBlockEntity(blockSel.Position);
		if(lookingat is BlockEntityBarrel barrel){
			if(barrel.CurrentRecipe==null){return;}
			barrel.SealedSinceTotalHours-=byEntity.World.Calendar.HoursPerDay*10;
			byEntity.World.BlockAccessor.MarkBlockEntityDirty(blockSel.Position);
			barrel.MarkDirty(true);

			slot.Itemstack.Attributes.SetInt("durability",prevDura-1);
			slot.MarkDirty();
			
			Parts p = new Parts(); 
			p.spawnParts(byEntity,new Vec3d(blockSel.Position.X,blockSel.Position.Y,blockSel.Position.Z));
			byEntity.World.PlaySoundAt(new AssetLocation("game:sounds/effect/translocate-breakdimension"),blockSel.Position.X,blockSel.Position.Y,blockSel.Position.Z,null,true,10f,2f);
		}
		
		if(prevDura<=0){
			slot.TakeOutWhole();
			slot.MarkDirty();
		}
	}
}

internal class itemQuickstep : Item{
	public override int GetMergableQuantity(ItemStack sinkStack, ItemStack sourceStack, EnumMergePriority priority){
		if(priority == EnumMergePriority.DirectMerge && sourceStack.ItemAttributes?["nightVisionFuelHours"].AsFloat(0) > 0){
			return 1;
		}

		return base.GetMergableQuantity(sinkStack, sourceStack, priority);
	}

	public override void TryMergeStacks(ItemStackMergeOperation op){
		if(op.CurrentPriority == EnumMergePriority.DirectMerge){
			int old = op.SinkSlot.Itemstack.Attributes.GetInt("fuel",0);
			op.SinkSlot.Itemstack.Attributes.SetInt("fuel",old+75);
			
			op.MovedQuantity=1;
			op.SourceSlot.TakeOut(1);
			op.SinkSlot.MarkDirty();
		}
	}

	public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo){
		base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
		
		if(world.Side==EnumAppSide.Client){
			var capi = world.Api as ICoreClientAPI;
			var player = capi?.World?.Player;
			if(player!=null && player.Entity.WatchedAttributes.GetString("characterClass")!="clockmaker"){
				dsc.AppendLine("A strange clock.\nYou have no idea how it works.");
				return;
			}else{
		 		int fuel = inSlot.Itemstack.Attributes.GetInt("fuel",0);
		 		string fuelstatus = "Uses remaining: "+fuel;
		 		if(fuel<=0){
		 			fuelstatus = "Insert temporal gear to add fuel.";
		 		}
				dsc.AppendLine("Send yourself a few moments into the future.\n\n"+fuelstatus);
			}
		}
	}
	
	public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling){
	 	handHandling = EnumHandHandling.Handled;
		if(byEntity.WatchedAttributes.GetString("characterClass")!="clockmaker"){return;}
		int fuel = slot.Itemstack.Attributes.GetInt("fuel",0);
		if(fuel<=0){return;}

		Vec3i tploc = new Vec3i(0,0,0);
		Vec3f pos = new Vec3f((float)(byEntity.LocalEyePos.X+byEntity.ServerPos.X),(float)(byEntity.LocalEyePos.Y+byEntity.ServerPos.Y),(float)(byEntity.LocalEyePos.Z+byEntity.ServerPos.Z));
		Vec3f lookDir = byEntity.Pos.GetViewVector();
		for(int i=0; i<30; i++){
			Vec3f check = pos+(lookDir*i);
			BlockPos bp = new BlockPos((int)check.X,(int)check.Y,(int)check.Z);
			Block block = byEntity.World.BlockAccessor.GetBlock(bp);
			
			BlockPos bp2 = new BlockPos((int)check.X,(int)check.Y+2,(int)check.Z);
			Block block2 = byEntity.World.BlockAccessor.GetBlock(bp2);
			if(block.Id!=0 && block2.Id==0){
				tploc = new Vec3i((int)check.X,(int)check.Y+1,(int)check.Z);
				byEntity.TeleportTo(tploc.X,tploc.Y,tploc.Z);
				byEntity.World.PlaySoundAt(new AssetLocation("game:sounds/effect/translocate-breakdimension"),tploc.X,tploc.Y,tploc.Z,null,true,10f,2f);
				byEntity.WatchedAttributes.SetDouble("temporalStability",byEntity.WatchedAttributes.GetDouble("temporalStability")-0.05);
				byEntity.ServerPos.Motion.Y = 0.1f;
				
				slot?.Itemstack.Attributes.SetInt("fuel",fuel-1);
				slot.MarkDirty();
				
				Parts p = new Parts(); 
				p.spawnParts(byEntity,new Vec3d(byEntity.Pos.X,byEntity.Pos.Y,byEntity.Pos.Z));
				break;
			}
		}
	}
}

internal class itemBacktreck : Item{

	public override int GetMergableQuantity(ItemStack sinkStack, ItemStack sourceStack, EnumMergePriority priority){
		if(priority == EnumMergePriority.DirectMerge && sourceStack.ItemAttributes?["nightVisionFuelHours"].AsFloat(0) > 0){
			return 1;
		}

		return base.GetMergableQuantity(sinkStack, sourceStack, priority);
	}

	public override void TryMergeStacks(ItemStackMergeOperation op){
		if(op.CurrentPriority == EnumMergePriority.DirectMerge){
			int old = op.SinkSlot.Itemstack.Attributes.GetInt("fuel",0);
			op.SinkSlot.Itemstack.Attributes.SetInt("fuel",old+1);
			
			op.MovedQuantity=1;
			op.SourceSlot.TakeOut(1);
			op.SinkSlot.MarkDirty();
		}
	}
	
	public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling){
	 	handHandling = EnumHandHandling.Handled;
		if(byEntity.WatchedAttributes.GetString("characterClass")!="clockmaker"){return;}
	 	
	 	ItemStack stack = slot?.Itemstack;
	 	int xpos = stack.Attributes.GetInt("X",0);
	 	int ypos = stack.Attributes.GetInt("Y",-1);
	 	int zpos = stack.Attributes.GetInt("Z",0);
	 	
	 	if(ypos==-1){
	 		stack.Attributes.SetInt("X",(int)byEntity.Pos.X-byEntity.World.DefaultSpawnPosition.AsBlockPos.X);
	 		stack.Attributes.SetInt("Y",(int)byEntity.Pos.Y);
	 		stack.Attributes.SetInt("Z",(int)byEntity.Pos.Z-byEntity.World.DefaultSpawnPosition.AsBlockPos.Z);
	 		slot.MarkDirty();
	 	}else if(stack.Attributes.GetInt("fuel",0)>0){
	 		stack.Attributes.SetInt("fuel",stack.Attributes.GetInt("fuel",0)-1);
	 		slot.MarkDirty();
	 		int xx = xpos+byEntity.World.DefaultSpawnPosition.AsBlockPos.X;
	 		int zz = zpos+byEntity.World.DefaultSpawnPosition.AsBlockPos.Z;
	 		foreach(var plr in byEntity.World.AllOnlinePlayers){
	 			if(plr.Entity.ServerPos.DistanceTo(byEntity.ServerPos)<=3){
	 				plr.Entity.TeleportTo(xx,ypos,zz);
	 			}
	 		}
	 		byEntity.World.PlaySoundAt(new AssetLocation("game:sounds/effect/translocate-breakdimension"),xx,ypos,zz,null,true,32f,5f);
	 		byEntity.WatchedAttributes.SetDouble("temporalStability",byEntity.WatchedAttributes.GetDouble("temporalStability")-0.75);
	 		
			Parts p = new Parts(); 
			p.spawnParts(byEntity,new Vec3d(byEntity.Pos.X,byEntity.Pos.Y,byEntity.Pos.Z));
	 	}
	 }

	public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo){
		base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
		
		if(world.Side==EnumAppSide.Client){
			var capi = world.Api as ICoreClientAPI;
			var player = capi?.World?.Player;
			if(player!=null && player.Entity.WatchedAttributes.GetString("characterClass")!="clockmaker"){
				dsc.AppendLine("A strange clock.\nYou have no idea how it works.");
				return;
			}
		}
		
		ItemStack stack = inSlot?.Itemstack;
	 	int xpos = stack.Attributes.GetInt("X",0);
	 	int ypos = stack.Attributes.GetInt("Y",-1);
	 	int zpos = stack.Attributes.GetInt("Z",0);
	 	
	 	if(ypos==-1){
	 		dsc.AppendLine("Use to set a point in time to return to.");
	 	}else{
	 		int fuel = stack.Attributes.GetInt("fuel",0);
	 		string fuelstatus = "Uses remaining: "+fuel;
	 		if(fuel<=0){
	 			fuelstatus = "Insert temporal gear to add fuel.";
	 		}
	 		dsc.AppendLine("Return to X: "+xpos+" Y: "+ypos+" Z: "+zpos+"\n\n"+fuelstatus);
	 	}
	}
}
