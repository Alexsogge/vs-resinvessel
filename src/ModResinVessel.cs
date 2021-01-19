using System;
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
using Newtonsoft.Json.Linq;
using Vintagestory.API.Server;

namespace resinvessel
{
    class ModResinVessel : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("resinvessel", typeof(BlockResinVessel));
            api.RegisterBlockBehaviorClass("resinvesselb", typeof(BlockBehaviorResinVessel));
            api.RegisterBlockEntityClass("resinvessel", typeof(BlockEntityResinVessel));
        }
    }

}