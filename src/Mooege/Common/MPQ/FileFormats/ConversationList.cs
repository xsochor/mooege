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
using System.Collections.Generic;
using System.Text;
using CrystalMpq;
using Mooege.Common.MPQ.FileFormats.Types;
using Gibbed.IO;

namespace Mooege.Common.MPQ.FileFormats
{
    /// <summary>
    ///  There are no conversationLists in the CoreData with size > 0, so this is not used (yet)
    /// </summary>
    [FileFormat(SNOGroup.ConversationList)]
    public class ConversationList : FileFormat
    {
        public Header Header;
        public List<ConversationListEntry> ConversationListEntries;

        public ConversationList(MpqFile file)
        {
            MpqFileStream stream = file.Open();
            this.Header = new Header(stream);
            stream.Position += (2 * 4);
            ConversationListEntries = stream.ReadSerializedData<ConversationListEntry>();

            stream.Close();
        }

    }

    public class ConversationListEntry : ISerializableData
    {
        public int SNOConv;
        public int I0;
        public int I1;
        public int I2;
        public int GbidItem;
        public string Noname1;
        public string Noname2;
        public int SNOQuestCurrent;
        public int I3;
        public int SNOQuestAssigned;
        public int SNOQuestActive;
        public int SNOQuestComplete;
        public int SNOQuestRange;
        public int SNOLevelArea;

        public void Read(CrystalMpq.MpqFileStream stream)
        {
            SNOConv = stream.ReadValueS32();
            I0 = stream.ReadValueS32();
            I1 = stream.ReadValueS32();
            I2 = stream.ReadValueS32();
            GbidItem = stream.ReadValueS32();
            Noname1 = stream.ReadString(128, true);
            Noname2 = stream.ReadString(128, true);
            SNOQuestCurrent = stream.ReadValueS32();
            I3 = stream.ReadValueS32();
            SNOQuestAssigned = stream.ReadValueS32();
            SNOQuestActive = stream.ReadValueS32();
            SNOQuestComplete = stream.ReadValueS32();
            SNOQuestRange = stream.ReadValueS32();
            SNOLevelArea = stream.ReadValueS32();
        }
    }

}