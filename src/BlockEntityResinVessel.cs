using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;
using Vintagestory.API.MathTools;
using Newtonsoft.Json.Linq;

namespace ResinVessel
{
    public class BlockEntityResinVessel : BlockEntityGenericTypedContainer
    {
        public BlockPos leakingLogBlockPos;
        public JProperty leakingLogTransientProps;
        
        public int inGameHours
        {
            get { return (int) leakingLogTransientProps.Value["inGameHours"]; }
        }

        public string harvestBlockCode
        {
            get { return (string) leakingLogTransientProps.Value["convertFrom"]; }
        }

        public string resinBlockCode
        {
            get { return (string) leakingLogTransientProps.Value["convertTo"]; }
        }


        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            RegisterGameTickListener(OnTickInChunk, 1000);
            retrieveOnly = true;
        }

        public void OnTickInChunk(float par)
        {
            if (leakingLogBlockPos != null)
            {
                Block leakingLogBlock = Api.World.BlockAccessor.GetBlock(leakingLogBlockPos);
                if (CheckLeakingLogBlock(leakingLogBlock))
                {
                    BlockBehaviorHarvestable harvestableLog = GetBlockBehaviorHarvestable(leakingLogBlock);
                    HarvestResin(harvestableLog);
                    ReplaceWithHarvested(leakingLogBlockPos);
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
                        leakingLogBlockPos = blockPos;
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
            AssetLocation harvestedLogBlockCode = AssetLocation.Create(harvestBlockCode, leakingBlock.Code.Domain);
            Block harvestedLogBlock = Api.World.GetBlock(harvestedLogBlockCode);
            Api.World.BlockAccessor.SetBlock(harvestedLogBlock.BlockId, blockPos);
        }

        private void HarvestResin(BlockBehaviorHarvestable behavior)
        {
            ItemStack resinVesselstack = Inventory[0].Itemstack;

            float dropRate = 1; // normally multiplied with player harvestrate

            ItemStack resinLogStack = behavior.harvestedStack.GetNextItemStack(dropRate);
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
                    leakingLogTransientProps = obj;
                }
            }
        }
    }
}