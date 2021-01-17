﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using Vintagestory.API.MathTools;

namespace resinvessel.src
{
    class ResinVesselMod: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("resinvessel", typeof(ResinVesselBlock));
            api.RegisterBlockBehaviorClass("resinvesselb", typeof(ResinVesselBehavior));
            api.RegisterBlockEntityClass("resinvessel", typeof(ResinVesselBlockEntity));
            Console.WriteLine("Start mod system");
        }
    }

    public class ResinVesselBlock: Block
    {

    }

    public class ResinVesselBlockEntity : BlockEntityGenericTypedContainer
    {
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            RegisterGameTickListener(OnTick, 1000);
            retrieveOnly = true;
        }
        public void OnTick(float par)
        {
            Api.World.Logger.Chat("Tick");
            foreach (int i in new int[] { -1, 1 })
            {
                int[] vectorX = { i, 0, 0 };
                int[] vectorZ = { 0, 0, i };
                foreach (int[] j in new int[][] { vectorX, vectorZ })
                {
                    BlockPos blockPos = Pos.AddCopy(j[0], j[1], j[2]);
                    Block leakingPineLogBlock = ConvertBlockToLeakingPineBlockBlock(Api.World.BlockAccessor.GetBlock(blockPos));
                    if (leakingPineLogBlock != null)
                    {
                        BlockBehaviorHarvestable harvestablePineLog = GetBlockBehaviorHarvestable(leakingPineLogBlock);
                        Api.World.Logger.Chat("Resin!!");
                        HarvestResin(leakingPineLogBlock, harvestablePineLog);
                        ReplaceWithHarvested(blockPos);

                    }
                }
            }
        }

        private BlockBehaviorHarvestable GetBlockBehaviorHarvestable(Block block)
        {
            foreach (BlockBehavior blockBehavior in block.BlockBehaviors)
            {
                if (blockBehavior is BlockBehaviorHarvestable)
                {
                    return (BlockBehaviorHarvestable)blockBehavior;
                }
            }
            return null;
        }

        private void ReplaceWithHarvested(BlockPos blockPos)
        {
            AssetLocation harvestedPineLogBlockCode = AssetLocation.Create("log-resinharvested-pine-ud");
            Block harvestedPineLogBlock = Api.World.GetBlock(harvestedPineLogBlockCode);
            Api.World.BlockAccessor.SetBlock(harvestedPineLogBlock.BlockId, blockPos);
        }

        private void HarvestResin(Block leakingPineLog, BlockBehaviorHarvestable behavior)
        {
            ItemStack resinVesselstack = Inventory.First().Itemstack;

            float dropRate = 1;  // normally multiplied with player harvestrate

            ItemStack resinLogStack = behavior.harvestedStack.GetNextItemStack(dropRate);
            Api.Logger.Chat("" + behavior.harvestedStack.Code);
            if (resinVesselstack != null)
            {
                if (resinVesselstack.Item.Code.Path == "resin")
                {
                    resinVesselstack.StackSize += resinLogStack.StackSize;
                }
            }
            else
            {
                Inventory[0].Itemstack = resinLogStack;
            }
        }
        private Block ConvertBlockToLeakingPineBlockBlock(Block block)
        {
            if (block != null)
            {
                if (block.Code != null)
                {
                    if (block.Code.BeginsWith("game", "log-resin-"))
                    {
                        return block;
                    }
                }
            }
            return null;
        }
    }

    public class ResinVesselBehavior : BlockBehaviorContainer
    {
        // public static string NAME { get; } = "ResinVessel";

        public ResinVesselBehavior(Block block)
            : base(block) { }

        public override bool CanAttachBlockAt(IBlockAccessor world, Block block, BlockPos pos, BlockFacing blockFace, ref EnumHandling handling, Cuboidi attachmentArea)
        {
            Console.WriteLine("Can attach?");
            if (BlockFacing.VERTICALS.Contains(blockFace))
            {
                handling = EnumHandling.PreventDefault;
                return true;
            }
            handling = EnumHandling.PassThrough;
            return false;
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref EnumHandling handling, ref string failureCode)
        {
            handling = EnumHandling.PreventDefault;

            BlockPos placePos = blockSel.Position.Copy();
            Block placeOn =
                world.BlockAccessor.GetBlock(placePos.Add(blockSel.Face.Opposite));

            // Prefer selected block face
            if (blockSel.Face.IsHorizontal && (placeOn.Code.Path == "log-resin-pine-ud" || placeOn.Code.Path == "log-resinharvested-pine-ud"))
            {
                Block orientedBlock = world.BlockAccessor.GetBlock(block.CodeWithParts(blockSel.Face.Code));
                orientedBlock.DoPlaceBlock(world, byPlayer, blockSel, itemstack);
                //block.DoPlaceBlock(world, byPlayer, blockSel, itemstack);

                return true;
            }

            return false;
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ref EnumHandling handled)
        {
            Console.WriteLine("placed");
        }
    }
}
