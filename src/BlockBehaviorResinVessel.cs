using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace resinvessel
{
    public class BlockBehaviorResinVessel : BlockBehavior
    {
        // public static string NAME { get; } = "ResinVessel";


        public BlockBehaviorResinVessel(Block block)
            : base(block)
        {


        }

        public override bool CanAttachBlockAt(IBlockAccessor world, Block block, BlockPos pos, BlockFacing blockFace,
            ref EnumHandling handling, Cuboidi attachmentArea)
        {
            if (BlockFacing.VERTICALS.Contains(blockFace))
            {
                handling = EnumHandling.PreventDefault;
                return true;
            }

            handling = EnumHandling.PassThrough;
            return false;
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack,
            BlockSelection blockSel, ref EnumHandling handling, ref string failureCode)
        {
            handling = EnumHandling.PreventDefault;

            BlockPos placePos = blockSel.Position.Copy();
            Block placeOn =
                world.BlockAccessor.GetBlock(placePos.Add(blockSel.Face.Opposite));


            // Prefer selected block face
            if (blockSel.Face.IsHorizontal && placeOn.Code.BeginsWith("game", "log-resin"))

            {
                foreach (BlockFacing face in BlockFacing.HORIZONTALS)
                {
                    BlockPos testPos = placePos.AddCopy(face);
                    if (IsResinVesel(world.BlockAccessor.GetBlock(testPos)))
                    {
                        /*
                        List<BlockPos> tmpList = new List<BlockPos>();
                        tmpList.Add(testPos);
                        world.HighlightBlocks(byPlayer, 2, tmpList);
                        */
                        return false;
                    }
                }


                Block orientedBlock = world.BlockAccessor.GetBlock(block.CodeWithParts(blockSel.Face.Code));
                orientedBlock.DoPlaceBlock(world, byPlayer, blockSel, itemstack);
                //block.DoPlaceBlock(world, byPlayer, blockSel, itemstack);

                return true;
            }

            return false;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel,
            ref EnumHandling handling)
        {
            handling = EnumHandling.PreventSubsequent;

            BlockEntity entity = world.BlockAccessor.GetBlockEntity(blockSel.Position);

            if (entity is BlockEntityResinVessel)
            {
                BlockEntityResinVessel vessel = (BlockEntityResinVessel) entity;

                if (!vessel.Inventory.Empty)
                {
                    ItemStack stack = vessel.Inventory[0].TakeOutWhole();
                    if (!byPlayer.InventoryManager.TryGiveItemstack(stack))
                    {
                        world.SpawnItemEntity(stack, blockSel.Position.ToVec3d().Add(0.5, 0.5, 0.5));
                    }

                    vessel.UpdateAsset();
                    return true;
                }
                else
                {
                    return false;
                }

            }

            return false;
        }

        public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos, ref EnumHandling handling)
        {
            handling = EnumHandling.PassThrough;

            BlockEntity entity = world.BlockAccessor.GetBlockEntity(pos);

            if (entity is BlockEntityResinVessel)
            {
                BlockEntityResinVessel vessel = (BlockEntityResinVessel) entity;

                IPlayer[] players = world.AllOnlinePlayers;
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i].InventoryManager.HasInventory(vessel.Inventory))
                    {
                        players[i].InventoryManager.CloseInventory(vessel.Inventory);
                    }
                }
            }
        }

        private bool IsResinVesel(Block block)
        {
            return block.Code.BeginsWith("resinvessel", "resinvessel");
        }
    }
}