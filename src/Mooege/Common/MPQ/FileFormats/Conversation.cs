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

using System.Collections.Generic;
using CrystalMpq;
using Gibbed.IO;
using Mooege.Common.MPQ.FileFormats.Types;
using Mooege.Core.GS.Common.Types.SNO;

namespace Mooege.Common.MPQ.FileFormats
{
    [FileFormat(SNOGroup.Conversation)]
    class Conversation : FileFormat
    {
        public Header Header { get; private set; }
        public ConversationTypes ConversationType { get; private set; }
        public int I0 { get; private set; }
        public int I1 { get; private set; }
        public int SNOQuest { get; private set; }
        public int I2 { get; private set; }
        public int I3 { get; private set; }
        public int SNOConvPiggyback { get; private set; }
        public int SNOConvUnlock { get; private set; }
        public int I4 { get; private set; }
        public string Unknown { get; private set; }
        public int SNOPrimaryNpc { get; private set; }
        public int SNOAltNpc1 { get; private set; }
        public int SNOAltNpc2 { get; private set; }
        public int SNOAltNpc3 { get; private set; }
        public int SNOAltNpc4 { get; private set; }
        public int I5 { get; private set; }              // not total nodes :-(
        public List<ConversationTreeNode> RootTreeNodes { get; private set; }
        public string Unknown2 { get; private set; }
        public int I6 { get; private set; }
        public byte[] CompiledScript { get; private set; }
        public int SNOBossEncounter { get; private set; }

        public Conversation(MpqFile file)
        {
            MpqFileStream stream = file.Open();

            this.Header = new Header(stream);
            this.ConversationType = (ConversationTypes)stream.ReadValueS32();
            this.I0 = stream.ReadValueS32();
            this.I1 = stream.ReadValueS32();
            this.SNOQuest = stream.ReadValueS32();
            this.I2 = stream.ReadValueS32();
            this.I3 = stream.ReadValueS32();
            this.SNOConvPiggyback = stream.ReadValueS32();
            this.SNOConvUnlock = stream.ReadValueS32();
            this.I4 = stream.ReadValueS32();
            this.Unknown = stream.ReadString(128, true);
            this.SNOPrimaryNpc = stream.ReadValueS32();
            this.SNOAltNpc1 = stream.ReadValueS32();
            this.SNOAltNpc2 = stream.ReadValueS32();
            this.SNOAltNpc3 = stream.ReadValueS32();
            this.SNOAltNpc4 = stream.ReadValueS32();
            this.I5 = stream.ReadValueS32();

            stream.Position += (2 * 4);
            RootTreeNodes = stream.ReadSerializedData<ConversationTreeNode>();

            this.Unknown2 = stream.ReadString(256, true);
            this.I6 = stream.ReadValueS32();

            stream.Position += (2 * 4);
            SerializableDataPointer compiledScriptPointer = stream.GetSerializedDataPointer();

            stream.Position += 44; // these bytes are unaccounted for in the xml
            this.SNOBossEncounter = stream.ReadValueS32();

            // reading compiled script, placed it here so i dont have to move the offset around
            CompiledScript = new byte[compiledScriptPointer.Size];
            stream.Position = compiledScriptPointer.Offset + 16;
            stream.Read(CompiledScript, 0, compiledScriptPointer.Size);

            stream.Close();
        }
    }


    public class ConversationTreeNode : ISerializableData
    {
        public int I0 { get; private set; }
        public int I1 { get; private set; }
        public int I2 { get; private set; }              // clasid ? 
        public Speaker Speaker1 { get; private set; }
        public Speaker Speaker2 { get; private set; }
        public int I3 { get; private set; }
        public int I4 { get; private set; }
        public int I5 { get; private set; }
        public ConvLocalDisplayTimes[] ConvLocalDisplayTimes = new ConvLocalDisplayTimes[18];
        public string Comment { get; private set; }
        public int I6 { get; private set; }
        public List<ConversationTreeNode> TrueNodes { get; private set; }
        public List<ConversationTreeNode> FalseNodes { get; private set; }
        public List<ConversationTreeNode> ChildNodes { get; private set; }

        public void Read(MpqFileStream stream)
        {
            I0 = stream.ReadValueS32();
            I1 = stream.ReadValueS32();
            I2 = stream.ReadValueS32();
            Speaker1 = (Speaker)stream.ReadValueS32();
            Speaker2 = (Speaker)stream.ReadValueS32();
            I3 = stream.ReadValueS32();
            I4 = stream.ReadValueS32();
            I5 = stream.ReadValueS32();

            for (int i = 0; i < ConvLocalDisplayTimes.Length; i++)
                ConvLocalDisplayTimes[i] = new ConvLocalDisplayTimes(stream);

            stream.Position += (2 * 4);
            Comment = stream.ReadSerializedString();
            this.I6 = stream.ReadValueS32();

            stream.Position += 4;       // these are unaccounted for...xml offsets just skips ahead

            stream.Position += (2 * 4);
            TrueNodes = stream.ReadSerializedData<ConversationTreeNode>();

            stream.Position += (2 * 4);
            FalseNodes = stream.ReadSerializedData<ConversationTreeNode>();

            stream.Position += (2 * 4);
            ChildNodes = stream.ReadSerializedData<ConversationTreeNode>();
        }
    }

    public class ConvLocalDisplayTimes
    {
        public int[] I0 = new int[10];

        public ConvLocalDisplayTimes(CrystalMpq.MpqFileStream stream)
        {
            for (int i = 0; i < I0.Length; i++)
                I0[i] = stream.ReadValueS32();
        }
    }


    public enum ConversationTypes
    {
        FollowerSoundset = 0,
        PlayerEmote = 1,
        AmbientFloat = 2,
        FollowerBanter = 3,
        FollowerCallout = 4,
        PlayerCallout = 5,
        GlobalChatter = 6,
        GlobalFloat = 7,
        LoreBook = 8,
        AmbientGossip = 9,
        TalkMenuGossip = 10,
        QuestStandard = 11,
        QuestFloat = 12,
        QuestEvent = 13
    }


    public enum Speaker
    {
        None = -1,
        Player = 0,
        PrimaryNPC = 1,
        AltNPC1 = 2,
        AltNPC2 = 3,
        AltNPC3 = 4,
        AltNPC4 = 5,
        TemplarFollower = 6,
        ScoundrelFollower = 7,
        EnchantressFollower = 8
    }
}
