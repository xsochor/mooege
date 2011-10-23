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

using System.IO;
using CrystalMpq;
using Mooege.Common.Extensions;

namespace Mooege.Common.MPQ.FileFormats
{
    [FileFormat(SNOGroup.Lore)]
    public class Lore : FileFormat
    {
        public Header Header;
        public int SNO;
        public int i0, i1, i2;
        public int snoConversation;
        public int i3;

        public Lore(MpqFile file)
        {
            var stream = file.Open();
            this.Header = new Header(stream);
            this.SNO = stream.ReadInt32();
            this.i0 = stream.ReadInt32();
            this.i1 = stream.ReadInt32();
            this.i2 = stream.ReadInt32();
            this.snoConversation = stream.ReadInt32();
            this.i3 = stream.ReadInt32();
            stream.Close();
        }
    }
}