﻿/*
 * Copyright (C) 2011 mooege project
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using Google.ProtocolBuffers;
using Mooege.Common;
using Mooege.Core.MooNet.Channels;
using Mooege.Net.MooNet;

namespace Mooege.Core.MooNet.Services
{
    [Service(serviceID: 0x0D, serviceName: "bnet.protocol.party.PartyService")]
    public class PartyService : bnet.protocol.party.PartyService,IServerService
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        public MooNetClient Client { get; set; }

        // PartyService just uses ChannelService to create a new channel for the party.
        public override void CreateChannel(IRpcController controller, bnet.protocol.channel.CreateChannelRequest request, Action<bnet.protocol.channel.CreateChannelResponse> done)
        {
            var channel = ChannelManager.CreateNewChannel(this.Client, request.ObjectId);
            var builder = bnet.protocol.channel.CreateChannelResponse.CreateBuilder()
                .SetObjectId(channel.DynamicId)
                .SetChannelId(channel.BnetEntityId);

            done(builder.Build());
            channel.SetOwner(Client); // Set the client that requested the creation of channel as the owner

            Logger.Trace("CreateChannel() {0} for {1}", channel, Client.CurrentToon);

            // send our MOTD - though this is actually not the right place for it /raist.
            if (Config.Instance.MOTD.Trim() != string.Empty)
                this.Client.SendServerWhisper(Config.Instance.MOTD);
        }

        public override void JoinChannel(IRpcController controller, bnet.protocol.channel.JoinChannelRequest request, Action<bnet.protocol.channel.JoinChannelResponse> done)
        {
            throw new NotImplementedException();
        }

        public override void GetChannelInfo(IRpcController controller, bnet.protocol.channel.GetChannelInfoRequest request, Action<bnet.protocol.channel.GetChannelInfoResponse> done)
        {
            Logger.Trace("GetChannelInfoRequest() to channel {0}:{1} by toon {2}", request.ChannelId.High, request.ChannelId.Low, Client.CurrentToon.Name);

            var builder = bnet.protocol.channel.GetChannelInfoResponse.CreateBuilder();
            var channel = ChannelManager.GetChannelByEntityId(request.ChannelId);
            if (channel != null)
                builder.SetChannelInfo(channel.Info);
            else
                Logger.Warn("Channel does not exist!");

            done(builder.Build());
        }
    }
}
