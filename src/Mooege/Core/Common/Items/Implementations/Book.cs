using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Common.Helpers;
using System.Diagnostics;
using Mooege.Net.GS.Message;
using Mooege.Common;
using Mooege.Core.GS.Players;
using Mooege.Net.GS.Message.Fields;
using Mooege.Core.GS.Map;
using Mooege.Common.MPQ;
using Mooege.Net.GS.Message.Definitions.World;
using Mooege.Core.GS.Common.Types.SNO;
using Mooege.Core.GS.Markers;

namespace Mooege.Core.Common.Items.Implementations
{
    [HandledType("Book")]
    public class Book : Item
    {
        public static readonly Logger Logger = LogManager.CreateLogger();
        public int LoreSNOId { get; private set; }

        public Book(World world, Mooege.Common.MPQ.FileFormats.ItemTable definition)
            : base(world, definition)
        {
            var y = MPQStorage.Data.Assets[SNOGroup.Actor].FirstOrDefault(x => x.Value.SNOId == this.SNOId);
            var e = (y.Value.Data as Mooege.Common.MPQ.FileFormats.Actor).TagMap.TagMapEntries.FirstOrDefault(z => z.TagID == (int)MarkerTagTypes.LoreSNOId);
            if (e != null)
            {
                LoreSNOId = e.Int2;
            }
            else
            {
                LoreSNOId = -1;
            }
        }

        public override void OnTargeted(Player player, TargetMessage message)
        {
            //Logger.Trace("OnTargeted");
            if (LoreSNOId != -1)
            {
                if (!player.LearnedLore.m_snoLoreLearned.Contains(LoreSNOId))
                {
                    // play lore to player
                    player.InGameClient.SendMessage(new Mooege.Net.GS.Message.Definitions.Quest.LoreMessage { Id = 213, snoLore = LoreSNOId });
                    // add lore to player's lores
                    if (player.LearnedLore.Count < player.LearnedLore.m_snoLoreLearned.Length)
                    {
                        player.LearnedLore.m_snoLoreLearned[player.LearnedLore.Count] = LoreSNOId;
                        player.LearnedLore.Count++; // Count
                        player.UpdateHeroState();
                    }
                }
                if (player.GroundItems.ContainsKey(this.DynamicID))
                    player.GroundItems.Remove(this.DynamicID);
                this.Destroy();
            }
            else
            {
                // monster lore, shouldn't occure
                player.Inventory.PickUp(this);
            }
        }
    }
}
