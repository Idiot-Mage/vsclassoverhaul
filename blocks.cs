using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;
using Vintagestory.API.MathTools;
using System;

internal class blockFakegrass : Block{
	public override void OnEntityInside(IWorldAccessor world, Entity entity, BlockPos pos){
		world.BlockAccessor.BreakBlock(pos,null);
	}
}


internal class blockWoodenspike : Block{
	public override void OnEntityInside(IWorldAccessor world, Entity entity, BlockPos pos){
		base.OnEntityInside(world, entity, pos);
		float motion = (float)Math.Abs(entity.Pos.Motion.X + entity.Pos.Motion.Y + entity.Pos.Motion.Z);
		if(entity.Alive && entity.Class!="EntityItem" && motion>0){
			entity.ReceiveDamage(new DamageSource() { Source = EnumDamageSource.Block, SourceBlock = this, Type = EnumDamageType.PiercingAttack, SourcePos = pos.ToVec3d() }, motion + 0.1f * 10);
		}
	}
	
	public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact){
		base.OnEntityCollide(world, entity, pos, facing, collideSpeed, isImpact);
		if(entity.Alive){
			double fallIntoDamageMul = 10;

			var dmg = (float)Math.Abs(collideSpeed.Y * fallIntoDamageMul);
			entity.ReceiveDamage(new DamageSource() { Source = EnumDamageSource.Block, SourceBlock = this, Type = EnumDamageType.PiercingAttack, SourcePos = pos.ToVec3d() }, dmg);
		}
	}
}
