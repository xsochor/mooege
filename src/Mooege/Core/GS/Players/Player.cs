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

using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Mooege.Common;
using Mooege.Core.Common.Toons;
using Mooege.Core.Common.Items;
using Mooege.Core.GS.Common.Types.Math;
using Mooege.Core.GS.Objects;
using Mooege.Core.GS.Map;
using Mooege.Core.GS.Actors;
using Mooege.Core.GS.Skills;
using Mooege.Net.GS;
using Mooege.Net.GS.Message;
using Mooege.Net.GS.Message.Definitions.Actor;
using Mooege.Net.GS.Message.Definitions.Misc;
using Mooege.Net.GS.Message.Definitions.Waypoint;
using Mooege.Net.GS.Message.Definitions.World;
using Mooege.Net.GS.Message.Fields;
using Mooege.Net.GS.Message.Definitions.Hero;
using Mooege.Net.GS.Message.Definitions.Player;
using Mooege.Net.GS.Message.Definitions.Skill;
using Mooege.Net.GS.Message.Definitions.Effect;
using Mooege.Net.GS.Message.Definitions.Conversation;
using Mooege.Common.Helpers;
using Mooege.Net.GS.Message.Definitions.Combat;
using System;
using Mooege.Net.GS.Message.Definitions.Trade;
using Mooege.Core.GS.Actors.Implementations;
using Mooege.Net.GS.Message.Definitions.Artisan;
using Mooege.Core.GS.Actors.Implementations.Artisans;
using Mooege.Core.GS.Test;

namespace Mooege.Core.GS.Players
{
    public class Player : Actor, IMessageConsumer
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        /// <summary>
        /// The ingame-client for player.
        /// </summary>
        public GameClient InGameClient { get; set; }

        /// <summary>
        /// The player index.
        /// </summary>
        public int PlayerIndex { get; private set; }

        /// <summary>
        /// The player's toon.
        /// We need a better name /raist.
        /// </summary>
        public Toon Properties { get; private set; }

        /// <summary>
        /// Skillset for the player (or actually for player's toons class).
        /// </summary>
        public SkillSet SkillSet { get; private set; }

        /// <summary>
        /// The inventory of player's toon.
        /// </summary>
        public Inventory Inventory { get; private set; }

        /// <summary>
        /// ActorType = Player.
        /// </summary>
        public override ActorType ActorType { get { return ActorType.Player; } }

        /// <summary>
        /// Did player enter the world?
        /// </summary>
        public bool EnteredWorld { get; set; }

        /// <summary>
        /// Revealed objects to player.
        /// </summary>
        public Dictionary<uint, IRevealable> RevealedObjects { get; private set; }

        // Collection of items that only the player can see. This is only used when items drop from killing an actor
        // TODO: Might want to just have a field on the item itself to indicate whether it is visible to only one player
        /// <summary>
        /// Dropped items for the player
        /// </summary>
        public Dictionary<uint, Item> GroundItems { get; private set; }

        /// <summary>
        /// Open converstations.
        /// </summary>
        public List<OpenConversation> OpenConversations { get; set; }

        // Used for Exp-Bonuses
        // Move them to a class or a better position please /raist.
        private int _killstreakTickTime;
        private int _killstreakPlayer;
        private int _killstreakEnvironment;
        private int _lastMonsterKillTick;
        private int _lastMonsterAttackTick;
        private int _lastMonsterAttackKills;
        private int _lastEnvironmentDestroyTick;
        private int _lastEnvironmentDestroyMonsterKills;
        private int _lastEnvironmentDestroyMonsterKillTick;

        /// <summary>
        /// Creates a new player.
        /// </summary>
        /// <param name="world">The world player joins initially.</param>
        /// <param name="client">The gameclient for the player.</param>
        /// <param name="bnetToon">Toon of the player.</param>
        public Player(World world, GameClient client, Toon bnetToon)
            : base(world, world.NewPlayerID)
        {
            this.EnteredWorld = false;
            this.InGameClient = client;
            this.PlayerIndex = Interlocked.Increment(ref this.InGameClient.Game.PlayerIndexCounter); // get a new playerId for the player and make it atomic.
            this.Properties = bnetToon;
            this.GBHandle.Type = (int)GBHandleType.Player;
            this.GBHandle.GBID = this.Properties.ClassID;
            this.Position = this.World.StartingPoints.First().Position; // set the player position to current world's very first startpoint. - should be actually set based on act & quest /raist.

            // actor values.
            this.SNOId = this.ClassSNO;
            this.Field2 = 0x00000009;
            this.Field3 = 0x00000000;
            this.Scale = this.ModelScale;
            this.RotationAmount = 0.05940768f;
            this.RotationAxis = new Vector3D(0f, 0f, 0.9982339f);
            this.CollFlags = 0x00000000;
            this.Field7 = -1;
            this.Field8 = -1;
            this.Field9 = 0x00000000;
            this.Field10 = 0x0;

            this.RevealedObjects = new Dictionary<uint, IRevealable>();
            this.SkillSet = new SkillSet(this.Properties.Class);
            this.GroundItems = new Dictionary<uint, Item>();
            this.OpenConversations = new List<OpenConversation>();

            this._killstreakTickTime = 400;
            this._killstreakPlayer = 0;
            this._killstreakEnvironment = 0;
            this._lastMonsterKillTick = 0;
            this._lastMonsterAttackTick = 0;
            this._lastMonsterAttackKills = 0;
            this._lastEnvironmentDestroyTick = 0;
            this._lastEnvironmentDestroyMonsterKills = 0;
            this._lastEnvironmentDestroyMonsterKillTick = 0;

            #region Attributes

            //Skills
            this.Attributes[GameAttribute.SkillKit] = this.SkillKit;
            this.Attributes[GameAttribute.Skill_Total, 0x7545] = 1; //Axe Operate Gizmo
            this.Attributes[GameAttribute.Skill, 0x7545] = 1;
            this.Attributes[GameAttribute.Skill_Total, 0x76B7] = 1; //Punch!
            this.Attributes[GameAttribute.Skill, 0x76B7] = 1;
            // TODO: remove and substitute with MPQ based unlocking skill
            this.Attributes[GameAttribute.Skill_Total, 0x216FA] = 1; //Monk's Blinding Flash
            this.Attributes[GameAttribute.Skill, 0x216FA] = 1;
            this.Attributes[GameAttribute.Skill_Total, 0x176C4] = 1; //Monk's Fist of Thunder
            this.Attributes[GameAttribute.Skill, 0x176C4] = 1;
            // END of todo
            this.Attributes[GameAttribute.Skill_Total, 0x6DF] = 1; //Use Item
            this.Attributes[GameAttribute.Skill, 0x6DF] = 1;
            this.Attributes[GameAttribute.Skill_Total, 0x7780] = 1; //Basic Attack
            this.Attributes[GameAttribute.Skill, 0x7780] = 1;
            this.Attributes[GameAttribute.Skill_Total, 0x0002EC66] = 0; //stone of recall
            this.Attributes[GameAttribute.Skill_Total, 0xFFFFF] = 1;
            this.Attributes[GameAttribute.Skill, 0xFFFFF] = 1;

            //Buffs
            this.Attributes[GameAttribute.Buff_Active, 0x33C40] = true;
            this.Attributes[GameAttribute.Buff_Icon_End_Tick0, 0x00033C40] = 0x000003FB;
            this.Attributes[GameAttribute.Buff_Icon_Start_Tick0, 0x00033C40] = 0x00000077;
            this.Attributes[GameAttribute.Buff_Icon_Count0, 0x00033C40] = 1;
            this.Attributes[GameAttribute.Buff_Visual_Effect, 0xFFFFF] = true;

            //Resistance
            this.Attributes[GameAttribute.Resistance, 0xDE] = 0.5f;
            this.Attributes[GameAttribute.Resistance, 0x226] = 0.5f;
            this.Attributes[GameAttribute.Resistance_Total, 0] = 10f; // im pretty sure key = 0 doesnt do anything since the lookup is (attributeId | (key << 12)), maybe this is some base resistance? /cm
            // likely the physical school of damage, it probably doesn't actually do anything in this case (or maybe just not for the player's hero)
            // but exists for the sake of parity with weapon damage schools
            this.Attributes[GameAttribute.Resistance_Total, 1] = 10f; //Fire
            this.Attributes[GameAttribute.Resistance_Total, 2] = 10f; //Lightning
            this.Attributes[GameAttribute.Resistance_Total, 3] = 10f; //Cold
            this.Attributes[GameAttribute.Resistance_Total, 4] = 10f; //Poison
            this.Attributes[GameAttribute.Resistance_Total, 5] = 10f; //Arcane
            this.Attributes[GameAttribute.Resistance_Total, 6] = 10f; //Holy
            this.Attributes[GameAttribute.Resistance_Total, 0xDE] = 0.5f;
            this.Attributes[GameAttribute.Resistance_Total, 0x226] = 0.5f;

            //Damage
            this.Attributes[GameAttribute.Damage_Delta_Total, 0] = 1f;
            this.Attributes[GameAttribute.Damage_Delta_Total, 1] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Delta_Total, 2] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Delta_Total, 3] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Delta_Total, 4] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Delta_Total, 5] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Delta_Total, 6] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Min_Total, 0] = 2f;
            this.Attributes[GameAttribute.Damage_Min_Total, 1] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Min_Total, 2] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Min_Total, 3] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Min_Total, 4] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Min_Total, 5] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Min_Total, 6] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Min_Total, 0xFFFFF] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Min_Subtotal, 0] = 2f;
            this.Attributes[GameAttribute.Damage_Min_Subtotal, 1] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Min_Subtotal, 2] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Min_Subtotal, 3] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Min_Subtotal, 4] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Min_Subtotal, 5] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Min_Subtotal, 6] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Min_Subtotal, 0xFFFFF] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, 0] = 2f;
            this.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, 1] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, 2] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, 3] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, 4] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, 5] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, 6] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Weapon_Min, 0] = 2f;
            this.Attributes[GameAttribute.Damage_Weapon_Min_Total, 0] = 2f;
            this.Attributes[GameAttribute.Damage_Weapon_Min_Total_All] = 2f;
            this.Attributes[GameAttribute.Damage_Weapon_Min_Total_MainHand, 0] = 2f;
            this.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, 0xFFFFF] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Weapon_Delta, 0] = 1f;
            this.Attributes[GameAttribute.Damage_Weapon_Delta_SubTotal, 0] = 1f;
            this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand, 0] = 1f;
            this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand, 1] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand, 2] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand, 3] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand, 4] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand, 5] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand, 6] = 3.051758E-05f;
            this.Attributes[GameAttribute.Damage_Weapon_Delta_Total, 0] = 1f;
            this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_All] = 1f;
            this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_MainHand, 0] = 1f;
            this.Attributes[GameAttribute.Damage_Weapon_Max, 0] = 3f;
            this.Attributes[GameAttribute.Damage_Weapon_Max_Total, 0] = 3f;

            //Bonus stats
            this.Attributes[GameAttribute.Get_Hit_Recovery] = 6f;
            this.Attributes[GameAttribute.Get_Hit_Recovery_Per_Level] = 1f;
            this.Attributes[GameAttribute.Get_Hit_Recovery_Base] = 5f;
            this.Attributes[GameAttribute.Get_Hit_Max] = 60f;
            this.Attributes[GameAttribute.Get_Hit_Max_Per_Level] = 10f;
            this.Attributes[GameAttribute.Get_Hit_Max_Base] = 50f;
            this.Attributes[GameAttribute.Hit_Chance] = 1f;
            this.Attributes[GameAttribute.Dodge_Rating_Total] = 3.051758E-05f;
            this.Attributes[GameAttribute.Attacks_Per_Second_Item_CurrentHand] = 1.199219f;
            this.Attributes[GameAttribute.Attacks_Per_Second_Item_Total_MainHand] = 1.199219f;
            this.Attributes[GameAttribute.Attacks_Per_Second_Total] = 1.199219f;
            this.Attributes[GameAttribute.Attacks_Per_Second] = 1f;
            this.Attributes[GameAttribute.Attacks_Per_Second_Item_MainHand] = 1.199219f;
            this.Attributes[GameAttribute.Attacks_Per_Second_Item_Total] = 1.199219f;
            this.Attributes[GameAttribute.Attacks_Per_Second_Item_Subtotal] = 3.051758E-05f;
            this.Attributes[GameAttribute.Attacks_Per_Second_Item] = 3.051758E-05f;
            this.Attributes[GameAttribute.Crit_Percent_Cap] = 0x3F400000;
            this.Attributes[GameAttribute.Casting_Speed_Total] = 1f;
            this.Attributes[GameAttribute.Casting_Speed] = 1f;

            //Basic stats
            this.Attributes[GameAttribute.Level_Cap] = 60;
            this.Attributes[GameAttribute.Level] = this.Properties.Level;
            this.Attributes[GameAttribute.Experience_Next] = LevelBorders[this.Properties.Level];
            this.Attributes[GameAttribute.Experience_Granted] = 1000;
            this.Attributes[GameAttribute.Armor_Total] = 0;
            this.Attributes[GameAttribute.Attack] = this.InitialAttack;
            this.Attributes[GameAttribute.Precision] = this.InitialPrecision;
            this.Attributes[GameAttribute.Defense] = this.InitialDefense;
            this.Attributes[GameAttribute.Vitality] = this.InitialVitality;

            //Hitpoints have to be calculated after Vitality
            this.Attributes[GameAttribute.Hitpoints_Factor_Level] = 4f;
            this.Attributes[GameAttribute.Hitpoints_Factor_Vitality] = 4f;
            //this.Attributes[GameAttribute.Hitpoints_Total_From_Level] = 3.051758E-05f;
            this.Attributes[GameAttribute.Hitpoints_Total_From_Level] = 0f;
            this.Attributes[GameAttribute.Hitpoints_Total_From_Vitality] = this.Attributes[GameAttribute.Vitality] * this.Attributes[GameAttribute.Hitpoints_Factor_Vitality];
            this.Attributes[GameAttribute.Hitpoints_Max] = 40f; // STATIC
            this.Attributes[GameAttribute.Hitpoints_Max_Total] = GetMaxTotalHitpoints();
            this.Attributes[GameAttribute.Hitpoints_Cur] = this.Attributes[GameAttribute.Hitpoints_Max_Total];

            //Resource
            this.Attributes[GameAttribute.Resource_Cur, this.ResourceID] = 200f;
            this.Attributes[GameAttribute.Resource_Max, this.ResourceID] = 200f;
            this.Attributes[GameAttribute.Resource_Max_Total, this.ResourceID] = 200f;
            this.Attributes[GameAttribute.Resource_Effective_Max, this.ResourceID] = 200f;
            this.Attributes[GameAttribute.Resource_Regen_Total, this.ResourceID] = 0f;
            this.Attributes[GameAttribute.Resource_Type_Primary] = this.ResourceID;

            //Movement
            this.Attributes[GameAttribute.Movement_Scalar_Total] = 1f;
            this.Attributes[GameAttribute.Movement_Scalar_Capped_Total] = 1f;
            this.Attributes[GameAttribute.Movement_Scalar_Subtotal] = 1f;
            this.Attributes[GameAttribute.Movement_Scalar] = 1f;
            this.Attributes[GameAttribute.Walking_Rate_Total] = 0.2797852f;
            this.Attributes[GameAttribute.Walking_Rate] = 0.2797852f;
            this.Attributes[GameAttribute.Running_Rate_Total] = 0.3598633f;
            this.Attributes[GameAttribute.Running_Rate] = 0.3598633f;
            this.Attributes[GameAttribute.Sprinting_Rate_Total] = 3.051758E-05f;
            this.Attributes[GameAttribute.Strafing_Rate_Total] = 3.051758E-05f;

            //Miscellaneous

            //this.Attributes[GameAttribute.Disabled] = true; // we should be making use of these ones too /raist.
            //this.Attributes[GameAttribute.Loading] = true;
            //this.Attributes[GameAttribute.Invulnerable] = true;

            this.Attributes[GameAttribute.Hidden] = false;
            this.Attributes[GameAttribute.Immobolize] = true;
            this.Attributes[GameAttribute.Untargetable] = true;
            this.Attributes[GameAttribute.CantStartDisplayedPowers] = true;
            this.Attributes[GameAttribute.IsTrialActor] = true;
            this.Attributes[GameAttribute.TeamID] = 2;
            this.Attributes[GameAttribute.Shared_Stash_Slots] = 14;
            this.Attributes[GameAttribute.Backpack_Slots] = 60;
            this.Attributes[GameAttribute.General_Cooldown] = 0;
            #endregion // Attributes

            this.Inventory = new Inventory(this); // Here because it needs attributes /fasbat

 // Class specific
            switch (this.Properties.Class)
            {
                case ToonClass.Barbarian:
                    this.Attributes[GameAttribute.Skill_Total, 30078] = 1;  //Fury Trait
                    this.Attributes[GameAttribute.Skill, 30078] = 1;
                    this.Attributes[GameAttribute.Trait, 30078] = 1;
                    break;
                case ToonClass.DemonHunter:
                    /* // unknown
                    this.Attributes[GameAttribute.Skill_Total, ] = 1;  // Hatred Trait
                    this.Attributes[GameAttribute.Skill, ] = 1;
                    this.Attributes[GameAttribute.Skill_Total, ] = 1;  // Discipline Trait
                    this.Attributes[GameAttribute.Skill, ] = 1;
                     */
                    //Secondary Resource for the Demon Hunter
                    int Discipline = this.ResourceID + 1; //0x00000006
                    this.Attributes[GameAttribute.Resource_Cur, Discipline] = 15f;
                    this.Attributes[GameAttribute.Resource_Max, Discipline] = 30f;
                    this.Attributes[GameAttribute.Resource_Max_Total, Discipline] = 30f;
                    this.Attributes[GameAttribute.Resource_Effective_Max, Discipline] = 30f;
                    this.Attributes[GameAttribute.Resource_Type_Secondary] = Discipline;
                    this.Attributes[GameAttribute.Resource_Regen_Per_Second, this.ResourceID] = 30f;
                    this.Attributes[GameAttribute.Resource_Regen_Per_Second, Discipline] = 5f;
                    AttributeMath.ComputeResourceRegen(this, Discipline); // doesn't work
                    break;
                case ToonClass.Monk:
                    this.Attributes[GameAttribute.Skill_Total, 0x0000CE11] = 1;  //Spirit Trait
                    this.Attributes[GameAttribute.Skill, 0x0000CE11] = 1;
                    this.Attributes[GameAttribute.Trait, 0x0000CE11] = 1;
                    this.Attributes[GameAttribute.Buff_Active, 0xCE11] = true;
                    this.Attributes[GameAttribute.Buff_Icon_Count0, 0x0000CE11] = 1;
                    break;
                case ToonClass.WitchDoctor:
                    /* // unknown
                    this.Attributes[GameAttribute.Skill_Total, ] = 1;  //Mana Trait
                    this.Attributes[GameAttribute.Skill, ] = 1;
                     */
                    this.Attributes[GameAttribute.Resource_Regen_Per_Second, this.ResourceID] = 50f; // needs proper number
                    break;
                case ToonClass.Wizard:
                    /* // unknown
                    this.Attributes[GameAttribute.Skill_Total, ] = 1;  //Arcane Power Trait
                    this.Attributes[GameAttribute.Skill, ] = 1;
                     */
                    this.Attributes[GameAttribute.Resource_Regen_Per_Second, this.ResourceID] = 30f; // needs proper number
                    break;
            }

            AttributeMath.ComputeResourceRegen(this, this.ResourceID);
            AttributeMath.ComputeStats(this, new GameAttributeMap(), true);
        }

        #region game-message handling & consumers

        /// <summary>
        /// Consumes the given game-message.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="message">The GameMessage.</param>
        public void Consume(GameClient client, GameMessage message)
        {
            if (message is AssignActiveSkillMessage) OnAssignActiveSkill(client, (AssignActiveSkillMessage)message);
            else if (message is AssignPassiveSkillMessage) OnAssignPassiveSkill(client, (AssignPassiveSkillMessage)message);
            else if (message is PlayerChangeHotbarButtonMessage) OnPlayerChangeHotbarButtonMessage(client, (PlayerChangeHotbarButtonMessage)message);
            else if (message is TargetMessage) OnObjectTargeted(client, (TargetMessage)message);
            else if (message is PlayerMovementMessage) OnPlayerMovement(client, (PlayerMovementMessage)message);
            else if (message is TryWaypointMessage) OnTryWaypoint(client, (TryWaypointMessage)message);
            else if (message is RequestBuyItemMessage) OnRequestBuyItem(client, (RequestBuyItemMessage)message);
            else if (message is RequestAddSocketMessage) OnRequestAddSocket(client, (RequestAddSocketMessage)message);
            else if (message is SocketSpellMessage) OnSocketSpell(client, (SocketSpellMessage)message);
            else if (message is RequestAddSocketMessage) OnRequestAddSocket(client, (RequestAddSocketMessage)message);
            else return;
        }

        private void OnSocketSpell(GameClient client, SocketSpellMessage socketSpellMessage)
        {
            Item rune = this.World.GetItem(unchecked((uint)socketSpellMessage.Field0));
            int PowerSNO = socketSpellMessage.Field1;
            int skillIndex = -1; // find index of powerSNO.
            for (int i = 0; i < this.SkillSet.ActiveSkills.Length; i++)
            {
                if (this.SkillSet.ActiveSkills[i] == PowerSNO)
                {
                    skillIndex = i;
                    break;
                }
            }
            if (skillIndex == -1)
            {
                // validity of message is controlled on client side, this shouldn't happen
                return;
            }
            // removes old rune (if present)
            Item oldRune = this.Inventory.RemoveRune(skillIndex);
            // type of rune is in Name
            //            Attributes[GameAttribute.Rune_<x>] = <rank>; // on attuned runes ONLY
            //            Attributes[GameAttribute.Rune_Rank] = <in spec>; // on unattuned rune ONLY, inititalized in creation
            //            Attributes[GameAttribute.Rune_Attuned_Power] = 0; // 0 on unattuned or  random value from all powers, inititalized in creation
            if (rune.Attributes[GameAttribute.Rune_Rank] != 0)
            {
                // if unattuned, pick random color and set attunement to new rune
                int rank = rune.Attributes[GameAttribute.Rune_Rank];
                int colorIndex = RandomHelper.Next(0, 5);
                Item newRune = ItemGenerator.Cook(this, "Runestone_" + (char)('A' + colorIndex) + "_0" + rank);
                newRune.Attributes[GameAttribute.Rune_Attuned_Power] = PowerSNO;
                switch (colorIndex)
                {
                    case 0:
                        newRune.Attributes[GameAttribute.Rune_A] = rank;
                        this.Attributes[GameAttribute.Rune_A, PowerSNO] = rank;
                        break;
                    case 1:
                        newRune.Attributes[GameAttribute.Rune_B] = rank;
                        this.Attributes[GameAttribute.Rune_B, PowerSNO] = rank;
                        break;
                    case 2:
                        newRune.Attributes[GameAttribute.Rune_C] = rank;
                        this.Attributes[GameAttribute.Rune_C, PowerSNO] = rank;
                        break;
                    case 3:
                        newRune.Attributes[GameAttribute.Rune_D] = rank;
                        this.Attributes[GameAttribute.Rune_D, PowerSNO] = rank;
                        break;
                    case 4:
                        newRune.Attributes[GameAttribute.Rune_E] = rank;
                        this.Attributes[GameAttribute.Rune_E, PowerSNO] = rank;
                        break;
                }
                newRune.Owner = this;
                newRune.Attributes.SendChangedMessage(this.InGameClient, newRune.DynamicID);
                rune.SetInventoryLocation(-1, -1, -1);
                this.Inventory.DestroyInventoryItem(rune);
                newRune.Reveal(this);
                newRune.SetInventoryLocation(16, skillIndex, 0); // skills (16), index, 0
                this.Inventory.SetRune(newRune, skillIndex);
            }
            else
            {
                // TODO: optimization possible?
                if (rune.Attributes[GameAttribute.Rune_A] != 0)
                {
                    Attributes[GameAttribute.Rune_A, PowerSNO] = rune.Attributes[GameAttribute.Rune_A];
                } else if (rune.Attributes[GameAttribute.Rune_B] != 0)
                {
                    Attributes[GameAttribute.Rune_B, PowerSNO] = rune.Attributes[GameAttribute.Rune_B];
                } else if (rune.Attributes[GameAttribute.Rune_C] != 0)
                {
                    Attributes[GameAttribute.Rune_C, PowerSNO] = rune.Attributes[GameAttribute.Rune_C];
                }
                else if (rune.Attributes[GameAttribute.Rune_D] != 0)
                {
                    Attributes[GameAttribute.Rune_D, PowerSNO] = rune.Attributes[GameAttribute.Rune_D];
                }
                else if (rune.Attributes[GameAttribute.Rune_E] != 0)
                {
                    Attributes[GameAttribute.Rune_E, PowerSNO] = rune.Attributes[GameAttribute.Rune_E];
                }
                this.Inventory.SetRune(rune, skillIndex);
            }
            if (oldRune != null) {
                this.Inventory.PickUp(oldRune);
            }
            // need info from BETA if and how this changes 
            //            this.SkillKeyMappings[0].Power = PowerSNO;
            //            this.SkillKeyMappings[0].Field1 = unchecked((int)rune.DynamicID); // ???
            //            this.SkillKeyMappings[0].Field2 = rune.Attributes[GameAttribute.Rune_Rank]; // ???
            UpdateHeroState();
        }

        private void OnAssignActiveSkill(GameClient client, AssignActiveSkillMessage message)
        {
            var oldSNOSkill = this.SkillSet.ActiveSkills[message.SkillIndex]; // find replaced skills SNO.
            if (oldSNOSkill != -1)
            {
                // switch off old skill in hotbar
                this.Attributes[GameAttribute.Skill, oldSNOSkill] = 0;
                this.Attributes[GameAttribute.Skill_Total, oldSNOSkill] = 0;
                // if old power was socketted, pickup rune
                Item oldRune = this.Inventory.RemoveRune(message.SkillIndex);
                if (oldRune != null)
                {
                    this.Inventory.PickUp(oldRune);
                }
            }
            // switch on new skill in hotbar
            this.Attributes[GameAttribute.Skill, message.SNOSkill] = 1;
            this.Attributes[GameAttribute.Skill_Total, message.SNOSkill] = 1;
            this.Attributes.SendChangedMessage(InGameClient, this.DynamicID);

            foreach (HotbarButtonData button in this.SkillSet.HotBarSkills.Where(button => button.SNOSkill == oldSNOSkill)) // loop through hotbar and replace the old skill with new one
            {
                button.SNOSkill = message.SNOSkill;
            }
            this.SkillSet.ActiveSkills[message.SkillIndex] = message.SNOSkill;
            this.UpdateHeroState();
        }

        private void OnAssignPassiveSkill(GameClient client, AssignPassiveSkillMessage message)
        {
            var oldSNOSkill = this.SkillSet.PassiveSkills[message.SkillIndex]; // find replaced skills SNO.
            if (oldSNOSkill != -1)
            {
                // switch off old passive skill
                this.Attributes[GameAttribute.Trait, oldSNOSkill] = 0;
                this.Attributes[GameAttribute.Skill, oldSNOSkill] = 0;
                this.Attributes[GameAttribute.Skill_Total, oldSNOSkill] = 0;
            }
            // switch on new passive skill
            this.Attributes[GameAttribute.Trait, message.SNOSkill] = 1;
            this.Attributes[GameAttribute.Skill, message.SNOSkill] = 1;
            this.Attributes[GameAttribute.Skill_Total, message.SNOSkill] = 1;
            this.Attributes.SendChangedMessage(InGameClient, this.DynamicID);
            this.SkillSet.PassiveSkills[message.SkillIndex] = message.SNOSkill;
            this.UpdateHeroState();
        }

        private void OnPlayerChangeHotbarButtonMessage(GameClient client, PlayerChangeHotbarButtonMessage message)
        {
            this.SkillSet.HotBarSkills[message.BarIndex] = message.ButtonData;
        }

        private void OnObjectTargeted(GameClient client, TargetMessage message)
        {
            Mooege.Core.GS.FXEffect.ClientEffect.ProcessSkill(this, message);
            Actor actor = this.World.GetActor(message.TargetID);
            if (actor == null) return;

            if ((actor.GBHandle.Type == 1) && (actor.Attributes[GameAttribute.TeamID] == 10))
            {
                this._lastMonsterAttackTick = this.InGameClient.Game.Tick;
            }

            actor.OnTargeted(this, message);
            CheckExpBonus(2);
        }

        private void OnPlayerMovement(GameClient client, PlayerMovementMessage message)
        {
            // here we should also be checking the position and see if it's valid. If not we should be resetting player to a good position with ACDWorldPositionMessage
            // so we can have a basic precaution for hacks & exploits /raist.

            if (message.Position != null)
                this.Position = message.Position;

            var msg = new NotifyActorMovementMessage
            {
                ActorId = message.ActorId,
                Position = this.Position,
                Angle = message.Angle,
                Field3 = false,
                Speed = message.Speed,
                Field5 = message.Field5,
                AnimationTag = message.AnimationTag
            };

            this.World.BroadcastExclusive(msg, this); // TODO: We should be instead notifying currentscene we're in. /raist.

            this.CollectGold();
            this.CollectHealthGlobe();
        }

        private void OnTryWaypoint(GameClient client, TryWaypointMessage tryWaypointMessage)
        {
            var wayPoint = this.World.GetWayPointById(tryWaypointMessage.Field1);
            if (wayPoint == null) return;

            this.Position = wayPoint.Position;
            InGameClient.SendMessage(this.ACDWorldPositionMessage);
        }

        private void OnRequestBuyItem(GameClient client, RequestBuyItemMessage requestBuyItemMessage)
        {
            var item = World.GetItem(requestBuyItemMessage.ItemId);
            if (item == null || item.Owner == null || !(item.Owner is Vendor))
                return;
            (item.Owner as Vendor).OnRequestBuyItem(this, item);
        }

        private void OnRequestAddSocket(GameClient client, RequestAddSocketMessage requestAddSocketMessage)
        {
            var item = World.GetItem(requestAddSocketMessage.ItemID);
            if (item == null || item.Owner != this)
                return;
            var jeweler = World.GetInstance<Jeweler>();
            if (jeweler == null)
                return;

            jeweler.OnAddSocket(this, item);
        }

        #endregion

        #region update-logic

        public override void Update()
        {
            // Check the Killstreaks
            CheckExpBonus(0);
            CheckExpBonus(1);

            // Check if there is an conversation to close in this tick
            CheckOpenConversations();
//            if (this.EnteredWorld)
//            {
                base.Update();
//            }
            this.InGameClient.SendTick(); // if there's available messages to send, will handle ticking and flush the outgoing buffer.
        }

        #endregion

        #region enter, leave, reveal handling

        public override void OnEnter(World world)
        {
            this.World.Reveal(this);

            // FIXME: hackedy hack
            var attribs = new GameAttributeMap();
            attribs[GameAttribute.Hitpoints_Healed_Target] = 76f;
            attribs.SendMessage(InGameClient, this.DynamicID);
        }

        public override void OnLeave(World world)
        { }

        public override bool Reveal(Player player)
        {
            if (!base.Reveal(player))
                return false;

            if (this == player) // only send this when player's own actor being is revealed. /raist.
            {
                player.InGameClient.SendMessage(new PlayerWarpedMessage()
                {
                    Field0 = 9,
                    Field1 = 0f,
                });
            }

            player.InGameClient.SendMessage(new PlayerEnterKnownMessage()
            {
                PlayerIndex = this.PlayerIndex,
                ActorId = this.DynamicID,
            });

            this.Inventory.SendVisualInventory(player);

            if (this == player) // only send this to player itself. Warning: don't remove this check or you'll make the game start crashing! /raist.
            {
                player.InGameClient.SendMessage(new PlayerActorSetInitialMessage()
                {
                    ActorId = this.DynamicID,
                    PlayerIndex = this.PlayerIndex,
                });
            }

            return true;
        }

        #endregion

        #region proximity based actor & scene revealing

        protected override void OnPositionChange(Vector3D prevPosition)
        {
            if (!this.EnteredWorld) return;
            this.World.RevealScenesInProximity(this);
            this.World.RevealActorsInProximity(this);
        }

        #endregion

        #region hero-state

        /// <summary>
        /// Allows hero state message to be sent when hero's some property get's updated.
        /// </summary>
        public void UpdateHeroState()
        {
            this.InGameClient.SendMessage(new HeroStateMessage
            {
                State = this.GetStateData()
            });
        }

        public HeroStateData GetStateData()
        {
            return new HeroStateData()
            {
                Field0 = 0x00000000,
                Field1 = 0x00000000,
                Field2 = 0x00000000,
                Gender = Properties.Gender,
                PlayerSavedData = this.GetSavedData(),
                Field5 = 0x00000000,
                tQuestRewardHistory = QuestRewardHistory,
            };
        }

        #endregion

        #region player attribute handling

        public float InitialAttack // Defines the amount of attack points with which a player starts
        {
            get
            {
                switch (this.Properties.Class)
                {
                    case ToonClass.Barbarian:
                        return 10f + ((this.Properties.Level - 1) * 2);
                    case ToonClass.DemonHunter:
                        return 10f + ((this.Properties.Level - 1) * 2);
                    case ToonClass.Monk:
                        return 10f + ((this.Properties.Level - 1) * 2);
                    case ToonClass.WitchDoctor:
                        return 10f + ((this.Properties.Level - 1) * 2);
                    case ToonClass.Wizard:
                        return 10f + ((this.Properties.Level - 1) * 2);
                }
                return 10f + (this.Properties.Level - 1) * 2;
            }
        }

        public float InitialPrecision // Defines the amount of precision points with which a player starts
        {
            get
            {
                switch (this.Properties.Class)
                {
                    case ToonClass.Barbarian:
                        return 9f + (this.Properties.Level - 1);
                    case ToonClass.DemonHunter:
                        return 11f + ((this.Properties.Level - 1) * 2);
                    case ToonClass.Monk:
                        return 11f + ((this.Properties.Level - 1) * 2);
                    case ToonClass.WitchDoctor:
                        return 9f + ((this.Properties.Level - 1) * 2);
                    case ToonClass.Wizard:
                        return 10f + ((this.Properties.Level - 1) * 2);
                }
                return 10f + ((this.Properties.Level - 1) * 2);
            }
        }

        public float InitialDefense // Defines the amount of defense points with which a player starts
        {
            get
            {
                switch (this.Properties.Class)
                {
                    case ToonClass.Barbarian:
                        return 11f + ((this.Properties.Level - 1) * 2);
                    case ToonClass.DemonHunter:
                        // For DH and Wizard, half the levels (starting with the first) give 2 defense => (Level / 2) * 2
                        // and half give 1 defense => ((Level - 1) / 2) * 1
                        // Note: We can't cancel the twos in ((Level - 1) / 2) * 2 because of integer divison
                        return 9f + (((this.Properties.Level / 2) * 2) + ((this.Properties.Level - 1) / 2));
                    case ToonClass.Monk:
                        return 10f + ((this.Properties.Level - 1) * 2);
                    case ToonClass.WitchDoctor:
                        return 9f + ((this.Properties.Level - 1) * 2);
                    case ToonClass.Wizard:
                        return 8f + (((this.Properties.Level / 2) * 2) + ((this.Properties.Level - 1) / 2));
                }
                return 10f + ((this.Properties.Level - 1) * 2);
            }
        }

        public float InitialVitality // Defines the amount of vitality points with which a player starts
        {
            get
            {
                switch (this.Properties.Class)
                {
                    case ToonClass.Barbarian:
                        return 11f + ((this.Properties.Level - 1) * 2);
                    case ToonClass.DemonHunter:
                        // For DH and Wizard, half the levels give 2 vit => ((Level - 1) / 2) * 2
                        // and half (starting with the first) give 1 vit => (Level / 2) * 1
                        // Note: We can't cancel the twos in ((Level - 1) / 2) * 2 because of integer divison
                        return 9f + ((((this.Properties.Level - 1) / 2) * 2) + (this.Properties.Level / 2));
                    case ToonClass.Monk:
                        return 9f + (this.Properties.Level - 1);
                    case ToonClass.WitchDoctor:
                        return 10f + (this.Properties.Level - 1);
                    case ToonClass.Wizard:
                        return 9f + ((((this.Properties.Level - 1) / 2) * 2) + (this.Properties.Level / 2));
                }
                return 10f + ((this.Properties.Level - 1) * 2);
            }
        }

        // Notes on attribute increment algorithm:
        // Precision: Barbarian => +1, else => +2
        // Defense:   Wizard or Demon Hunter => (lvl+1)%2+1, else => +2
        // Vitality:  Wizard or Demon Hunter => lvl%2+1, Barbarian => +2, else +1
        // Attack:    All +2
        public float AttackIncrement
        {
            get
            {
                switch (this.Properties.Class)
                {
                    case ToonClass.Barbarian:
                        return 2f;
                    case ToonClass.DemonHunter:
                        return 2f;
                    case ToonClass.Monk:
                        return 2f;
                    case ToonClass.WitchDoctor:
                        return 2f;
                    case ToonClass.Wizard:
                        return 2f;
                }
                return 2f;
            }
        }

        public float VitalityIncrement
        {
            get
            {
                switch (this.Properties.Class)
                {
                    case ToonClass.Barbarian:
                        return 2f;
                    case ToonClass.DemonHunter:
                        return (this.Attributes[GameAttribute.Level] % 2) + 1f;
                    case ToonClass.Monk:
                        return 1f;
                    case ToonClass.WitchDoctor:
                        return 1f;
                    case ToonClass.Wizard:
                        return (this.Attributes[GameAttribute.Level] % 2) + 1f;
                }
                return 1f;
            }
        }

        public float DefenseIncrement
        {
            get
            {
                switch (this.Properties.Class)
                {
                    case ToonClass.Barbarian:
                        return 2f;
                    case ToonClass.DemonHunter:
                        return ((this.Attributes[GameAttribute.Level] + 1) % 2) + 1f;
                    case ToonClass.Monk:
                        return 2f;
                    case ToonClass.WitchDoctor:
                        return 2f;
                    case ToonClass.Wizard:
                        return ((this.Attributes[GameAttribute.Level] + 1) % 2) + 1f;
                }
                return 2f;
            }
        }

        public float PrecisionIncrement
        {
            get
            {
                switch (this.Properties.Class)
                {
                    case ToonClass.Barbarian:
                        return 1f;
                    case ToonClass.DemonHunter:
                        return 2f;
                    case ToonClass.Monk:
                        return 2f;
                    case ToonClass.WitchDoctor:
                        return 2f;
                    case ToonClass.Wizard:
                        return 2f;
                }
                return 2f;
            }
        }
        #endregion

        #region saved-data

        private PlayerSavedData GetSavedData()
        {
            return new PlayerSavedData()
            {
                HotBarButtons = this.SkillSet.HotBarSkills,
                SkilKeyMappings = this.SkillKeyMappings,

                Field2 = 0x00000000,
                Field3 = 0x7FFFFFFF,

                Field4 = new HirelingSavedData()
                {
                    HirelingInfos = this.HirelingInfo,
                    Field1 = 0x00000000,
                    Field2 = 0x00000000,
                },

                Field5 = 0x00000000,

                LearnedLore = this.LearnedLore,
                snoActiveSkills = this.SkillSet.ActiveSkills,
                snoTraits = this.SkillSet.PassiveSkills,
                Field9 = new SavePointData { snoWorld = -1, Field1 = -1, },
                m_SeenTutorials = this.SeenTutorials,
            };
        }

        public LearnedLore LearnedLore = new LearnedLore()
        {
            Field0 = 0x00000000,
            m_snoLoreLearned = new int[256]
             {
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000
             },
        };

        public int[] SeenTutorials = new int[64]
        {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        };

        public PlayerQuestRewardHistoryEntry[] QuestRewardHistory = new PlayerQuestRewardHistoryEntry[0] { };

        public HirelingInfo[] HirelingInfo = new HirelingInfo[4]
        {
            new HirelingInfo { Field0 = 0x00000000, Field1 = -1, Field2 = 0x00000000, Field3 = 0x00000000, Field4 = false, Field5 = -1, Field6 = -1, Field7 = -1, Field8 = -1, },
            new HirelingInfo { Field0 = 0x00000000, Field1 = -1, Field2 = 0x00000000, Field3 = 0x00000000, Field4 = false, Field5 = -1, Field6 = -1, Field7 = -1, Field8 = -1, },
            new HirelingInfo { Field0 = 0x00000000, Field1 = -1, Field2 = 0x00000000, Field3 = 0x00000000, Field4 = false, Field5 = -1, Field6 = -1, Field7 = -1, Field8 = -1, },
            new HirelingInfo { Field0 = 0x00000000, Field1 = -1, Field2 = 0x00000000, Field3 = 0x00000000, Field4 = false, Field5 = -1, Field6 = -1, Field7 = -1, Field8 = -1, },
        };

        public SkillKeyMapping[] SkillKeyMappings = new SkillKeyMapping[15]
        {
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
        };

        #endregion

        #region cooked messages

        public GenericBlobMessage GetPlayerBanner()
        {
            var playerBanner = D3.GameMessage.PlayerBanner.CreateBuilder()
                .SetPlayerIndex((uint) this.PlayerIndex)
                .SetBanner(this.Properties.Owner.BannerConfiguration)
                .Build();

            return new GenericBlobMessage(Opcodes.GenericBlobMessage6) {Data = playerBanner.ToByteArray()};
        }

        public GenericBlobMessage GetBlacksmithData()
        {
            var blacksmith = D3.ItemCrafting.CrafterData.CreateBuilder()
                .SetLevel(45)
                .SetCooldownEnd(0)
                .Build();
            return new GenericBlobMessage(Opcodes.GenericBlobMessage8) { Data = blacksmith.ToByteArray() };
        }

        public GenericBlobMessage GetJewelerData()
        {
            var jeweler = D3.ItemCrafting.CrafterData.CreateBuilder()
                .SetLevel(9)
                .SetCooldownEnd(0)
                .Build();
            return new GenericBlobMessage(Opcodes.GenericBlobMessage9) { Data = jeweler.ToByteArray() };
        }

        public GenericBlobMessage GetMysticData()
        {
            var mystic = D3.ItemCrafting.CrafterData.CreateBuilder()
                .SetLevel(45)
                .SetCooldownEnd(0)
                .Build();
            return new GenericBlobMessage(Opcodes.GenericBlobMessage10) { Data = mystic.ToByteArray() };
        }

        #endregion

        #region generic properties

        public int ClassSNO
        {
            get
            {
                if (this.Properties.Gender == 0)
                {
                    switch (this.Properties.Class)
                    {
                        case ToonClass.Barbarian:
                            return 0x0CE5;
                        case ToonClass.DemonHunter:
                            return 0x0125C7;
                        case ToonClass.Monk:
                            return 0x1271;
                        case ToonClass.WitchDoctor:
                            return 0x1955;
                        case ToonClass.Wizard:
                            return 0x1990;
                    }
                }
                else
                {
                    switch (this.Properties.Class)
                    {
                        case ToonClass.Barbarian:
                            return 0x0CD5;
                        case ToonClass.DemonHunter:
                            return 0x0123D2;
                        case ToonClass.Monk:
                            return 0x126D;
                        case ToonClass.WitchDoctor:
                            return 0x1951;
                        case ToonClass.Wizard:
                            return 0x197E;
                    }
                }
                return 0x0;
            }
        }

        public float ModelScale
        {
            get
            {
                switch (this.Properties.Class)
                {
                    case ToonClass.Barbarian:
                        return 1.2f;
                    case ToonClass.DemonHunter:
                        return 1.35f;
                    case ToonClass.Monk:
                        return 1.43f;
                    case ToonClass.WitchDoctor:
                        return 1.1f;
                    case ToonClass.Wizard:
                        return 1.3f;
                }
                return 1.43f;
            }
        }

        public int ResourceID
        {
            get
            {
                switch (this.Properties.Class)
                {
                    case ToonClass.Barbarian:
                        return 0x00000002;
                    case ToonClass.DemonHunter:
                        return 0x00000005;
                    case ToonClass.Monk:
                        return 0x00000003;
                    case ToonClass.WitchDoctor:
                        return 0x00000000;
                    case ToonClass.Wizard:
                        return 0x00000001;
                }
                return 0x00000000;
            }
        }

        public int SkillKit
        {
            get
            {
                switch (this.Properties.Class)
                {
                    case ToonClass.Barbarian:
                        return 0x00008AF4;
                    case ToonClass.DemonHunter:
                        return 0x00008AFC;
                    case ToonClass.Monk:
                        return 0x00008AFA;
                    case ToonClass.WitchDoctor:
                        return 0x00008AFF;
                    case ToonClass.Wizard:
                        return 0x00008B00;
                }
                return 0x00000001;
            }
        }

        #endregion

        #region queries

        public List<T> GetRevealedObjects<T>() where T : class, IRevealable
        {
            return this.RevealedObjects.Values.OfType<T>().Select(@object => @object).ToList();
        }

        #endregion

        #region experience handling

        private float GetMaxTotalHitpoints()
        {
            // Defines the Max Total hitpoints for the current level
            // May want to move this into a property if it has to made class-specific
            // This is still a work in progress on getting the right algorithm for all the classes

            return (this.Attributes[GameAttribute.Hitpoints_Total_From_Vitality]) +
                    (this.Attributes[GameAttribute.Hitpoints_Total_From_Level]);
        }

        public static int[] LevelBorders =
        {
            0, 1200, 2250, 4000, 6050, 8500, 11700, 15400, 19500, 24000, /* Level 1-10 */
            28900, 34200, 39900, 44100, 45000, 46200, 48300, 50400, 52500, 54600, /* Level 11-20 */
            56700, 58800, 60900, 63000, 65100, 67200, 69300, 71400, 73500, 75600, /* Level 21-30 */
            77700, 81700, 85800, 90000, 94300, 98700, 103200, 107800, 112500, 117300, /* Level 31-40 */
            122200, 127200, 132300, 137500, 142800, 148200, 153700, 159300, 165000, 170800, /* Level 41-50 */
            176700, 182700, 188800, 195000, 201300, 207700, 214200, 220800, 227500, 234300, /* Level 51-60 */
            241200, 248200, 255300, 262500, 269800, 277200, 284700, 292300, 300000, 307800, /* Level 61-70 */
            315700, 323700, 331800, 340000, 348300, 356700, 365200, 373800, 382500, 391300, /* Level 71-80 */
            400200, 409200, 418300, 427500, 436800, 446200, 455700, 465300, 475000, 484800, /* Level 81-90 */
            494700, 504700, 514800, 525000, 535300, 545700, 556200, 566800, 577500 /* Level 91-99 */
        };

        public static int[] LevelUpEffects =
        {
            85186, 85186, 85186, 85186, 85186, 85190, 85190, 85190, 85190, 85190, /* Level 1-10 */
            85187, 85187, 85187, 85187, 85187, 85187, 85187, 85187, 85187, 85187, /* Level 11-20 */
            85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, /* Level 21-30 */
            85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, /* Level 31-40 */
            85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, /* Level 41-50 */
            85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, /* Level 51-60 */
            85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, /* Level 61-70 */
            85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, /* Level 71-80 */
            85195, 85195, 85195, 85195, 85195, 85195, 85195, 85195, 85195, 85195, /* Level 81-90 */
            85195, 85195, 85195, 85195, 85195, 85195, 85195, 85195, 85195, 85195 /* Level 91-99 */
        };

        public void UpdateExp(int addedExp)
        {

            this.Attributes[GameAttribute.Experience_Next] -= addedExp;

            // Levelup
            if ((this.Attributes[GameAttribute.Experience_Next] <= 0) && (this.Attributes[GameAttribute.Level] < this.Attributes[GameAttribute.Level_Cap]))
            {
                this.Attributes[GameAttribute.Level]++;
                this.Properties.LevelUp();
                if (this.Attributes[GameAttribute.Level] < this.Attributes[GameAttribute.Level_Cap]) { this.Attributes[GameAttribute.Experience_Next] = this.Attributes[GameAttribute.Experience_Next] + LevelBorders[this.Attributes[GameAttribute.Level]]; }
                else { this.Attributes[GameAttribute.Experience_Next] = 0; }

                // 4 main attributes are incremented according to class
                this.Attributes[GameAttribute.Attack] += this.AttackIncrement;
                this.Attributes[GameAttribute.Precision] += this.PrecisionIncrement;
                this.Attributes[GameAttribute.Vitality] += this.VitalityIncrement;
                this.Attributes[GameAttribute.Defense] += this.DefenseIncrement;

                // Hitpoints from level may actually change. This needs to be verified by someone with the beta.
                //this.Attributes[GameAttribute.Hitpoints_Total_From_Level] = this.Attributes[GameAttribute.Level] * this.Attributes[GameAttribute.Hitpoints_Factor_Level];

                /*
                // For now, hit points are based solely on vitality and initial hitpoints received.
                // This will have to change when hitpoint bonuses from items are implemented.
                this.Attributes[GameAttribute.Hitpoints_Total_From_Vitality] = this.Attributes[GameAttribute.Vitality] * this.Attributes[GameAttribute.Hitpoints_Factor_Vitality];
                this.Attributes[GameAttribute.Hitpoints_Max] = GetMaxTotalHitpoints();
                this.Attributes[GameAttribute.Hitpoints_Max_Total] = GetMaxTotalHitpoints();
                */
                AttributeMath.ComputeStats(this, this.Inventory.GetEquippedMap());
                // On level up, health is set to max
                this.Attributes[GameAttribute.Hitpoints_Cur] = this.Attributes[GameAttribute.Hitpoints_Max_Total];

                this.Attributes.SendChangedMessage(this.InGameClient, this.DynamicID);

                this.InGameClient.SendMessage(new PlayerLevel()
                {
                    Id = 0x98,
                    Field0 = 0x00000000,
                    Field1 = this.Attributes[GameAttribute.Level],
                });

                this.InGameClient.SendMessage(new PlayEffectMessage()
                {
                    ActorId = this.DynamicID,
                    Effect = Effect.LevelUp,
                });

                this.World.BroadcastGlobal(new PlayEffectMessage()
                {
                    ActorId = this.DynamicID,
                    Effect = Effect.PlayEffectGroup,
                    OptionalParameter = LevelUpEffects[this.Attributes[GameAttribute.Level]],
                });
            }

            // constant 0 exp at Level_Cap
            if (this.Attributes[GameAttribute.Experience_Next] < 0) 
            { 
                this.Attributes[GameAttribute.Experience_Next] = 0;

            }
            this.Attributes.SendChangedMessage(this.InGameClient, this.DynamicID);
            //this.Attributes.SendMessage(this.InGameClient, this.DynamicID); kills the player atm
        }

        public void UpdateExpBonusData(int attackerActorType, int defeatedActorType)
        {
            if (attackerActorType == 7) // Player
            {
                if (defeatedActorType == 1) // Monster
                {
                    // Massacre
                    if (this._lastMonsterKillTick + this._killstreakTickTime > this.InGameClient.Game.Tick)
                    {
                        this._killstreakPlayer++;
                    }
                    else
                    {
                        this._killstreakPlayer = 1;
                    }

                    // MightyBlow
                    if (Math.Abs(this._lastMonsterAttackTick - this.InGameClient.Game.Tick) <= 20)
                    {
                        this._lastMonsterAttackKills++;
                    }
                    else
                    {
                        this._lastMonsterAttackKills = 1;
                    }

                    this._lastMonsterKillTick = this.InGameClient.Game.Tick;
                }
                else if (defeatedActorType == 5) // Environment
                {
                    // Destruction
                    if (this._lastEnvironmentDestroyTick + this._killstreakTickTime > this.InGameClient.Game.Tick)
                    {
                        this._killstreakEnvironment++;
                    }
                    else
                    {
                        this._killstreakEnvironment = 1;
                    }

                    this._lastEnvironmentDestroyTick = this.InGameClient.Game.Tick;
                }
            }
            else if (attackerActorType == 5) // Environment
            {
                // Pulverized
                if (Math.Abs(this._lastEnvironmentDestroyMonsterKillTick - this.InGameClient.Game.Tick) <= 20)
                {
                    this._lastEnvironmentDestroyMonsterKills++;
                }
                else
                {
                    this._lastEnvironmentDestroyMonsterKills = 1;
                }

                this._lastEnvironmentDestroyMonsterKillTick = this.InGameClient.Game.Tick;
            }
        }

        public void CheckExpBonus(byte BonusType)
        {
            int defeated = 0;
            int expBonus = 0;

            switch (BonusType)
            {
                case 0: // Massacre
                    {
                        if ((this._killstreakPlayer > 5) && (this._lastMonsterKillTick + this._killstreakTickTime <= this.InGameClient.Game.Tick))
                        {
                            defeated = this._killstreakPlayer;
                            expBonus = (this._killstreakPlayer - 5) * 10;

                            this._killstreakPlayer = 0;
                        }
                        break;
                    }
                case 1: // Destruction
                    {
                        if ((this._killstreakEnvironment > 5) && (this._lastEnvironmentDestroyTick + this._killstreakTickTime <= this.InGameClient.Game.Tick))
                        {
                            defeated = this._killstreakEnvironment;
                            expBonus = (this._killstreakEnvironment - 5) * 5;

                            this._killstreakEnvironment = 0;
                        }
                        break;
                    }
                case 2: // Mighty Blow
                    {
                        if (this._lastMonsterAttackKills > 5)
                        {
                            defeated = this._lastMonsterAttackKills;
                            expBonus = (this._lastMonsterAttackKills - 5) * 5;
                        }
                        this._lastMonsterAttackKills = 0;
                        break;
                    }
                case 3: // Pulverized
                    {
                        if (this._lastEnvironmentDestroyMonsterKills > 3)
                        {
                            defeated = this._lastEnvironmentDestroyMonsterKills;
                            expBonus = (this._lastEnvironmentDestroyMonsterKills - 3) * 10;
                        }
                        this._lastEnvironmentDestroyMonsterKills = 0;
                        break;
                    }
                default:
                    {
                        Logger.Warn("Invalid Exp-Bonus-Type was checked.");
                        return;
                    }
            }

            if (expBonus > 0)
            {
                this.InGameClient.SendMessage(new KillCounterUpdateMessage()
                {
                    Id = 0xcd,
                    Field0 = BonusType,
                    Field1 = defeated,
                    Field2 = expBonus,
                    Field3 = false,
                });

                this.UpdateExp(expBonus);
                PlayHeroConversation(0x0002A73F, RandomHelper.Next(0, 8));
            }
        }

        public void PlayHeroConversation(int snoConversation, int lineID)
        {
            this.InGameClient.SendMessage(new PlayConvLineMessage()
            {
                Id = 0xba,
                ActorID = this.DynamicID,
                Field1 = new uint[9]
                    {
                        this.DynamicID, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF
                    },

                Params = new PlayLineParams()
                {
                    SNOConversation = snoConversation,
                    Field1 = 0x00000001,
                    Field2 = false,
                    LineID = lineID,
                    Field4 = 0x00000000,
                    Field5 = -1,
                    TextClass = (Class)this.Properties.VoiceClassID,
                    Gender = (this.Properties.Gender == 0) ? VoiceGender.Male : VoiceGender.Female,
                    AudioClass = (Class)this.Properties.VoiceClassID,
                    SNOSpeakerActor = this.SNOId,
                    Name = this.Properties.Name,
                    Field11 = 0x00000002,
                    Field12 = -1,
                    Field13 = 0x00000069,
                    Field14 = 0x0000006E,
                    Field15 = 0x00000032
                },
                Field3 = 0x00000069,
            });

            this.OpenConversations.Add(new OpenConversation(
                new EndConversationMessage()
                {
                    ActorId = this.DynamicID,
                    Field0 = 0x0000006E,
                    SNOConversation = snoConversation
                },
                this.InGameClient.Game.Tick + 400
            ));
        }

        public void CheckOpenConversations()
        {
            if (this.OpenConversations.Count > 0)
            {
                foreach (OpenConversation openConversation in this.OpenConversations)
                {
                    if (openConversation.endTick == this.InGameClient.Game.Tick)
                    {
                        this.InGameClient.SendMessage(openConversation.endConversationMessage);
                    }
                }
            }
        }

        public struct OpenConversation
        {
            public EndConversationMessage endConversationMessage;
            public int endTick;

            public OpenConversation(EndConversationMessage endConversationMessage, int endTick)
            {
                this.endConversationMessage = endConversationMessage;
                this.endTick = endTick;
            }
        }

        #endregion

        #region gold, heath-glob collection

        private void CollectGold()
        {
            var actorList = this.World.GetActorsInRange(this.Position.X, this.Position.Y, this.Position.Z, 5f);
            foreach (var actor in actorList)
            {
                Item item;
                if (!(actor is Item)) continue;
                item = (Item)actor;
                if (!Item.IsGold(item.ItemType)) continue;

                this.InGameClient.SendMessage(new FloatingAmountMessage()
                {
                    Place = new WorldPlace()
                    {
                        Position = this.Position,
                        WorldID = this.World.DynamicID,
                    },

                    Amount = item.Attributes[GameAttribute.Gold],
                    Type = FloatingAmountMessage.FloatType.Gold,
                });

                this.Inventory.PickUpGold(item.DynamicID);


                item.Destroy();
            }
        }

        private void CollectHealthGlobe()
        {
            var actorList = this.World.GetActorsInRange(this.Position.X, this.Position.Y, this.Position.Z, 5f);
            foreach (Actor actor in actorList)
            {
                Item item;
                if (!(actor is Item)) continue;
                item = (Item)actor;
                if (!Item.IsHealthGlobe(item.ItemType)) continue;

                this.InGameClient.SendMessage(new PlayEffectMessage() //Remember, for PlayEffectMessage, field1=7 are globes picking animation.
                {
                    ActorId = this.DynamicID,
                    Effect = Effect.HealthOrbPickup
                });

                foreach (var pair in this.World.Players) // should be actually checking for players in proximity. /raist
                {
                    pair.Value.AddPercentageHP((int)item.Attributes[GameAttribute.Health_Globe_Bonus_Health]);
                }

                item.Destroy();
            }
        }

        public void AddPercentageHP(int percentage)
        {
            float quantity = (percentage * this.Attributes[GameAttribute.Hitpoints_Max_Total]) / 100;
            this.AddHP(quantity);
        }

        public void AddHP(float quantity)
        {
            if (this.Attributes[GameAttribute.Hitpoints_Cur] + quantity >= this.Attributes[GameAttribute.Hitpoints_Max_Total])
                this.Attributes[GameAttribute.Hitpoints_Cur] = this.Attributes[GameAttribute.Hitpoints_Max_Total];
            else
                this.Attributes[GameAttribute.Hitpoints_Cur] = this.Attributes[GameAttribute.Hitpoints_Cur] + quantity;
        }

        #endregion
    }
}
