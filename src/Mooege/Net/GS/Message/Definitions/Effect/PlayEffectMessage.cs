/*
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

using System.Text;

namespace Mooege.Net.GS.Message.Definitions.Effect
{
    [Message(Opcodes.PlayEffectMessage)]
    public class PlayEffectMessage : GameMessage
    {
        // Tye of effect - partial list
        public enum EffectType : int
        {
            USED_IN_KILL_0 = 0, // used in monster death (combination actorID, 0, Field2 = 0x2)
            NEED_MORE_INFO5 = 5, // stars gain from one direction (spirit/fury gain?)
            LevelInfo = 6,// level info (+ unlocked skills), taken from actor's attributes
            HealthGain = 7,// health gain/steal
            ArcanePowerGain = 8,// arcane power gain/steal
            NEED_MORE_INFO9 = 9,// shimmering impacts above head
            NEED_MORE_INFO10 = 10,// gaining stars from surrounding area
            USED_IN_KILL12 = 12, // used in monster death
            NEED_MORE_INFO13 = 13,// spawning magenta marker (on actor position, when expires, spawns new one on current actor's position)
            NEED_MORE_INFO16 = 16,// spawning magenta marker as in 13 
            NotPickableItemError = 17,// error message: "That item cannot be picked up."
            FullInventoryError = 18,// error message: "You have no place to put that item."
            OnlyOneKindOfItemError = 19,// error message: "You are not allowed to have more than one of this item. 
            NotOwnerOfItem = 20,// error message: "That item belongs to someone else and cannot be picked up."
            BloodSplash = 24,// blood splash (upward)
            WhiteFlash = 27,// white flash (sphered) + small white sphere
            SmallSphereBlue = 28,// small blue sphere
            BlueFlash = 29,// big blue sphere flash
            ClientEffect = 32,// client effect
            MetalImpactSound = 34,// metal impact sound
            CrashingSoundNEED_MORE_INFO = 35,// crashing sound (needs specification)
            FlashResource = 37,// flash in resource bubble
            ItemDroppedSoundNEED_MORE_INFO = 39, // used in Item.Reveal
            DeathExplosionBones = 40,// death anim - bone explosion
            DeathExplosionFire = 41,// death anim - fire explosion
            DeathExplosionPoison = 42,// death anim - poison explosion
            DeathExplosionArcane = 43,// death anim - arcane explosion
            DeathExplosionHoly = 44,// death anim - holy explosion
            DeathExplosionLightning = 45,// death anim - lightning explosion
            DeathExplosionCold = 46,// death anim - frozen explosion
            BigExplosionFire = 47,// big explosion - fire (meteor?) 
            OverlayBlood = 49,// blood actor overlay - on, permanent
            OverlayNone = 50,// normal actor skin
            OverlayDark = 52,// dark actor overlay - on, permanent
            OverlaySilver = 53,// silver actor overlay - on, permanent
            OverlayBlack = 55,// black overlay - on, permanent
            OverlayGreen = 56,// green overlay - on, permanent
            ImpactSoundNEED_MORE_INFO = 61,// impact sound
            ManaGain = 62,// mana gain/steal
        }; 
        
        public uint ActorID; // Actor's DynamicID
        public int Field1;
        public int? Field2;

        public PlayEffectMessage() : base(Opcodes.PlayEffectMessage) { }

        public override void Parse(GameBitBuffer buffer)
        {
            ActorID = buffer.ReadUInt(32);
            Field1 = buffer.ReadInt(7) + (-1);
            if (buffer.ReadBool())
            {
                Field2 = buffer.ReadInt(32);
            }
        }

        public override void Encode(GameBitBuffer buffer)
        {
            buffer.WriteUInt(32, ActorID);
            buffer.WriteInt(7, Field1 - (-1));
            buffer.WriteBool(Field2.HasValue);
            if (Field2.HasValue)
            {
                buffer.WriteInt(32, Field2.Value);
            }
        }

        public override void AsText(StringBuilder b, int pad)
        {
            b.Append(' ', pad);
            b.AppendLine("PlayEffectMessage:");
            b.Append(' ', pad++);
            b.AppendLine("{");
            b.Append(' ', pad); b.AppendLine("ActorID: 0x" + ActorID.ToString("X8") + " (" + ActorID + ")");
            b.Append(' ', pad); b.AppendLine("Field1: 0x" + Field1.ToString("X8") + " (" + Field1 + ")");
            if (Field2.HasValue)
            {
                b.Append(' ', pad); b.AppendLine("Field2.Value: 0x" + Field2.Value.ToString("X8") + " (" + Field2.Value + ")");
            }
            b.Append(' ', --pad);
            b.AppendLine("}");
        }


    }
}
