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

using CrystalMpq;
using Gibbed.IO;
using Mooege.Net.GS.Message.Fields;

namespace Mooege.Common.MPQ.FileFormats.Types
{
    public class Header
    {
        public int DeadBeef;
        public int SnoType;
        public int Unknown1, Unknown2, Unknown3, Unknown4;
        public int SNOId;

        public Header(MpqFileStream stream)
        {
            this.DeadBeef = stream.ReadValueS32();
            this.SnoType = stream.ReadValueS32();
            this.Unknown1 = stream.ReadValueS32();
            this.Unknown2 = stream.ReadValueS32();
            this.SNOId = stream.ReadValueS32();
            this.Unknown3 = stream.ReadValueS32();
            this.Unknown4 = stream.ReadValueS32();
        }
    }

    public class Vector2D
    {
        public readonly int Field0, FIeld1;

        public Vector2D(MpqFileStream stream)
        {
            Field0 = stream.ReadValueS32();
            FIeld1 = stream.ReadValueS32();
        }
    }

    public class PRTransform
    {
        public Quaternion Q;
        public Vector3D V;

        public PRTransform(MpqFileStream stream)
        {
            Q = new Quaternion(stream);
            V = new Vector3D(stream.ReadValueF32(), stream.ReadValueF32(), stream.ReadValueF32());
        }
    }

    public class Quaternion
    {
        public float Float0;
        public Vector3D Vector3D;

        public Quaternion(MpqFileStream stream)
        {
            Float0 = stream.ReadValueF32();
            Vector3D = new Vector3D(stream.ReadValueF32(), stream.ReadValueF32(), stream.ReadValueF32());
        }
    }

    public class AABB
    {
        public Vector3D Min { get; private set; }
        public Vector3D Max { get; private set; }

        public AABB(MpqFileStream stream)
        {
            this.Min = new Vector3D(stream.ReadValueF32(), stream.ReadValueF32(), stream.ReadValueF32());
            this.Max = new Vector3D(stream.ReadValueF32(), stream.ReadValueF32(), stream.ReadValueF32());
        }
    }

    public class SNOName
    {
        public SNOGroup SNOGroup { get; private set; }
        public int SNOId { get; private set; }
        public string Name { get; private set; }

        public SNOName(MpqFileStream stream)
        {
            this.SNOGroup = (SNOGroup)stream.ReadValueS32();
            this.SNOId = stream.ReadValueS32();

            if (!MPQStorage.Data.Assets.ContainsKey(this.SNOGroup))
                return; // it's here because of the SNOGroup 0, could it be the Act? /raist
            this.Name = MPQStorage.Data.Assets[this.SNOGroup].ContainsKey(this.SNOId)
                            ? MPQStorage.Data.Assets[this.SNOGroup][SNOId].Name
                            : ""; // put it here because it seems we miss loading some scenes there /raist.
        }
    }

    public class TagMap : ISerializableData
    {
        public int TagMapSize;
        public TagMapEntry[] TagMapEntries;

        public void Read(MpqFileStream stream)
        {
            TagMapSize = stream.ReadValueS32();
            TagMapEntries = new TagMapEntry[TagMapSize];

            for (int i = 0; i < TagMapSize; i++)
            {
                TagMapEntries[i] = new TagMapEntry(stream);
            }
        }
    }

    public class TagMapEntry
    {
        public int Type;
        public int Int1;

        public ScriptFormula ScriptFormula;
        public int Int2;
        public float Float0;

        public TagMapEntry(MpqFileStream stream)
        {
            this.Type = stream.ReadValueS32();
            this.Int1 = stream.ReadValueS32();

            switch (this.Type)
            {
                case 0:
                    this.Int2 = stream.ReadValueS32();
                    break;
                case 1:
                    Float0 = stream.ReadValueF32();
                    break;
                case 2: // SNO
                    this.Int2 = stream.ReadValueS32();
                    break;
                case 4:
                    this.ScriptFormula = new ScriptFormula(stream);
                    break;
                default:
                    this.Int2 = stream.ReadValueS32();
                    break;
            }
        }
    }
    public class ScriptFormula
    {
        int i0, i1, i2, i3, i4;
        int name_size;
        int i5;
        int opcode_size;
        public byte[] OpCodeName { get; private set; }
        public byte[] OpCodeArray { get; private set; }
        public ScriptFormula(MpqFileStream stream)
        {
            this.i0 = stream.ReadValueS32();
            this.i1 = stream.ReadValueS32();
            this.i2 = stream.ReadValueS32();
            this.i3 = stream.ReadValueS32();
            this.i4 = stream.ReadValueS32();
            this.name_size = stream.ReadValueS32();
            this.i5 = stream.ReadValueS32();
            this.opcode_size = stream.ReadValueS32();
            this.OpCodeName = new byte[name_size];
            stream.Read(OpCodeName, 0, name_size);
            switch(name_size % 4)
            {
                case 0:
                    break;
                case 1:
                    stream.Position += 3;
                    break;
                case 2:
                    stream.Position += 2;
                    break;
                case 3:
                    stream.Position += 1;
                    break;

            }
            this.OpCodeArray = new byte[opcode_size];
            stream.Read(OpCodeArray, 0, opcode_size);
        }
    }

    public class ScriptFormulaDetails : ISerializableData
    {
        public string CharArray1 { get; private set; }
        public string CharArray2 { get; private set; }
        int i0, i1;
        public void Read(MpqFileStream stream)
        {
            CharArray1 = stream.ReadString(256, true);
            CharArray2 = stream.ReadString(512, true);
            i0 = stream.ReadValueS32();
            i1 = stream.ReadValueS32();
        }
    }
    // Replace each Look with just a chararay? DarkLotus
    public class HardPointLink
    {
        public string Name;
        public int i0;
        public HardPointLink(MpqFileStream stream)
        {
            this.Name = stream.ReadString(64, true);
            i0 = stream.ReadValueS32();
        }
    }
    
    public class TriggerConditions
    {
        // Unsure if these should be ints or floats - DarkLotus
        public float float0;
        public int int1;
        public int int2;
        public int int3;
        public int int4;
        public int int5;
        public int int6;
        public int int7;
        public int int8;
        public TriggerConditions(MpqFileStream stream)
        {
            float0 = stream.ReadValueF32();
            int1 = stream.ReadValueS32();
            int2 = stream.ReadValueS32();
            int3 = stream.ReadValueS32();
            int4 = stream.ReadValueS32();
            int5 = stream.ReadValueS32();
            int6 = stream.ReadValueS32();
            int7 = stream.ReadValueS32();
            int8 = stream.ReadValueS32();
        }
    }

    public class TriggerEvent
    {
        public int i0;
        public TriggerConditions TriggerConditions;
        public int i1;
        public SNOName SnoName;
        public int i2, i3;
        //pad 12
        public HardPointLink[] HardPointLinks;
        public string LookLink;
        public string ConstraintLink;
        int i4;
        float f0;
        int i5, i6, i7, i8, i9;
        float f1, f2;
        int i10, i11;
        float f3;
        int i12;
        float Velocity;
        int i13; // DT_TIME
        int RuneType, UseRuneType;
        public RGBAColor Color1;
        int i14; // DT_TIME
        public RGBAColor Color2;
        int i15; // DT_TIME
        public TriggerEvent(MpqFileStream stream)
        {
            i0 = stream.ReadValueS32();
            TriggerConditions = new TriggerConditions(stream);
            i1 = stream.ReadValueS32();
            SnoName = new SNOName(stream);
            i2 = stream.ReadValueS32();
            i3 = stream.ReadValueS32();
            HardPointLinks = new HardPointLink[2];
            HardPointLinks[0] = new HardPointLink(stream);
            HardPointLinks[1] = new HardPointLink(stream);
            this.LookLink = stream.ReadString(64, true);
            this.ConstraintLink = stream.ReadString(64, true);
            i4 = stream.ReadValueS32();
            f0 = stream.ReadValueF32();
            i5 = stream.ReadValueS32();
            i6 = stream.ReadValueS32();
            i7 = stream.ReadValueS32();
            i8 = stream.ReadValueS32();
            i9 = stream.ReadValueS32();
            f1 = stream.ReadValueF32();
            f2 = stream.ReadValueF32();
            i10 = stream.ReadValueS32();
            i11 = stream.ReadValueS32();
            f3 = stream.ReadValueF32();
            i12 = stream.ReadValueS32();
            Velocity = stream.ReadValueF32();
            i13 = stream.ReadValueS32();
            RuneType = stream.ReadValueS32();
            UseRuneType = stream.ReadValueS32();
            Color1 = new RGBAColor(stream);
            i14 = stream.ReadValueS32();
            Color2 = new RGBAColor(stream);
            i15 = stream.ReadValueS32();

        }
    }

    public class MsgTriggeredEvent : ISerializableData
    {
        public int i0;
        public TriggerEvent TriggerEvent;

        public void Read(MpqFileStream stream)
        {
            i0 = stream.ReadValueS32();
            TriggerEvent = new TriggerEvent(stream);
        }
    }

    public class RGBAColor
    {
        public byte Red;
        public byte Green;
        public byte Blue;
        public byte Alpha;

        public RGBAColor(MpqFileStream stream)
        {
            var buf = new byte[4];
            stream.Read(buf, 0, 4);
            Red = buf[0];
            Green = buf[1];
            Blue = buf[2];
            Alpha = buf[3];
        }
    }

    //public class PostFXParams // unused for now. /raist.
    //{
    //    public float[] Float0;
    //    public float[] Float1;

    //    public PostFXParams(MpqFileStream stream)
    //    {
    //        Float0 = new float[4];
    //        for (int i = 0; i < Float0.Length; i++)
    //        {
    //            Float0[i] = stream.ReadInt32();
    //        }
    //        Float1 = new float[4];
    //        for (int i = 0; i < Float1.Length; i++)
    //        {
    //            Float1[i] = stream.ReadInt32();
    //        }
    //    }
    //}
}
