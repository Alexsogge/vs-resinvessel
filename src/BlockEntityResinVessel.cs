using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using Vintagestory.API.MathTools;
using Newtonsoft.Json.Linq;


namespace ResinVessel
{
    public class BlockEntityResinVessel : BlockEntityGenericTypedContainer
    {
        public double LastHarvested;

        public BlockPos LeakingLogBlockPos;
        private JProperty LeakingLogTransientProps;
        
        public int InGameHours
        {
            get { return (int) LeakingLogTransientProps.Value["inGameHours"]; }
        }

        public string HarvestBlockCode
        {
            get { return (string) LeakingLogTransientProps.Value["convertFrom"]; }
        }

        public string ResinBlockCode
        {
            get { return (string) LeakingLogTransientProps.Value["convertTo"]; }
        }


        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (api.Side == EnumAppSide.Server)
            {
                RegisterGameTickListener(OnTickInChunk, 1000);
            }

            if (LastHarvested == 0)
            {
                LastHarvested = Api.World.Calendar.TotalHours;
            }

            retrieveOnly = true;
        }

        public void OnTickInChunk(float par)
        {
            if (LeakingLogBlockPos != null)
            {
                Block leakingLogBlock = Api.World.BlockAccessor.GetBlock(LeakingLogBlockPos);
                if (CheckLeakingLogBlock(leakingLogBlock))
                {
                    if (Api.Side == EnumAppSide.Server)
                    {
                        ItemStack resinVesselstack = Inventory[0].Itemstack;
                        if (Inventory[0].Itemstack == null || resinVesselstack.Item.MaxStackSize > resinVesselstack.StackSize)
                        {
                            BlockBehaviorHarvestable harvestableLog = GetBlockBehaviorHarvestable(leakingLogBlock);
                            HarvestResin(harvestableLog);
                            ReplaceWithHarvested(LeakingLogBlockPos);
                        }
                        LastHarvested = Api.World.Calendar.TotalHours;
                        MarkDirty();
                    }
                }
            }
            else
            {
                SelectLeakingLog();
            }
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);

            int stacksize = (Inventory.Empty) ? 0 : Inventory[0].Itemstack.StackSize;
            {

            }

            dsc.Clear();

            dsc.AppendLine(Lang.Get("Resin stored: {0}", stacksize));
        }

        private void SelectLeakingLog()
        {
            foreach (int i in new int[] {-1, 1})
            {
                int[] vectorX = {i, 0, 0};
                int[] vectorZ = {0, 0, i};
                foreach (int[] j in new int[][] {vectorX, vectorZ})
                {
                    BlockPos blockPos = Pos.AddCopy(j[0], j[1], j[2]);
                    Block leakingBlock = Api.World.BlockAccessor.GetBlock(blockPos);
                    if (CheckLeakingLogBlock(leakingBlock, false))
                    {
                        LeakingLogBlockPos = blockPos;
                        UpdateTransientProps(blockPos);
                    }
                }
            }
        }

        public bool CheckLeakingLogBlock(Block block, bool checkLeaking = true)
        {
            if (block != null)
            {
                if (block.Code != null)
                {
                    string code = "log-resin";
                    code += (checkLeaking) ? "-" : "";
                    if (block.Code.BeginsWith("game", code))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private BlockBehaviorHarvestable GetBlockBehaviorHarvestable(Block block)
        {
            foreach (BlockBehavior blockBehavior in block.BlockBehaviors)
            {
                if (blockBehavior is BlockBehaviorHarvestable)
                {
                    return (BlockBehaviorHarvestable) blockBehavior;
                }
            }

            return null;
        }

        private void ReplaceWithHarvested(BlockPos blockPos)
        {
            Block leakingBlock = Api.World.BlockAccessor.GetBlock(blockPos);
            AssetLocation harvestedLogBlockCode = AssetLocation.Create(HarvestBlockCode, leakingBlock.Code.Domain);
            Block harvestedLogBlock = Api.World.GetBlock(harvestedLogBlockCode);
            Api.World.BlockAccessor.SetBlock(harvestedLogBlock.BlockId, blockPos);
        }

        private void HarvestResin(BlockBehaviorHarvestable behavior)
        {
            ItemStack resinVesselstack = Inventory[0].Itemstack;
            var missedHarvests = (int) Math.Floor((Api.World.Calendar.TotalHours - LastHarvested) / InGameHours);
            missedHarvests = Math.Max(missedHarvests, 1);  // at this point we are sure that there is at least one resin -> minimum 1 harvest!

            float dropRate = 1; // normally multiplied with player harvestrate

            ItemStack resinLogStack = behavior.harvestedStack.GetNextItemStack(dropRate);
            for (var i = 0; i < missedHarvests; i++) {
                if (resinVesselstack != null)
                {
                    if (resinVesselstack.Item.Code.Path == behavior.harvestedStack.Code.Path)
                    {
                        resinVesselstack.StackSize += resinLogStack.StackSize;
                    }
                }
                else
                {
                    Inventory[0].Itemstack = resinLogStack;
                }
            }

            UpdateAsset();
        }

        public void UpdateAsset()
        {
            string codePath = (!Inventory.Empty)
                ? Block.Code.Path.Replace("empty", "filled")
                : Block.Code.Path.Replace("filled", "empty");
            AssetLocation filledBlockAsset = AssetLocation.Create(codePath, Block.Code.Domain);
            Block filledBlock = Api.World.GetBlock(filledBlockAsset);
            Api.World.BlockAccessor.ExchangeBlock(filledBlock.BlockId, Pos);
        }

        private void UpdateTransientProps(BlockPos leakingBlockPos)
        {
            Block leakingBlock = Api.World.BlockAccessor.GetBlock(leakingBlockPos);
            
            // only harvested block has transientProps
            if (!leakingBlock.Code.Path.Contains("harvested"))
            {
                AssetLocation leakingBlockAssetLocation =
                    AssetLocation.Create(leakingBlock.Code.Path.Replace("resin", "resinharvested"),
                        leakingBlock.Code.Domain);
                leakingBlock = Api.World.GetBlock(leakingBlockAssetLocation);
            }

            foreach (JProperty obj in leakingBlock.Attributes.Token)
            {
                if (obj.Name == "transientProps")
                {
                    LeakingLogTransientProps = obj;
                }
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetDouble("LastHarvested", LastHarvested);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            LastHarvested = tree.GetDouble("LastHarvested");
        }
    }
}