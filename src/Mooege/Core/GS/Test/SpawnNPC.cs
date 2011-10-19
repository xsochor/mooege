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
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, write to the Free Software
* Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Core.GS.Map;
using Mooege.Net.GS.Message.Definitions.ACD;
using Mooege.Net.GS.Message.Fields;
using Mooege.Net.GS.Message.Definitions.Misc;
using Mooege.Net.GS.Message.Definitions.Attribute;
using Mooege.Net.GS.Message;
using Mooege.Net.GS.Message.Definitions.Animation;

namespace Mooege.Core.GS.Test
{
    // conversation start: TargetMessage, powerSNO = 30022
    // portal/stash interaction: TargetMessage, powerSNO = 30021
    class SpawnNPC
    {
        public static void revealTest(World world, Player.Player player)
        {
            spawnFerryman(world); // OK
//            spawnStartLocation0(world); // ERROR?
            spawnWaypoint(world); // OK
            spawnStash(world, player); // OK
//            spawnRaven0(world); // ERROR?
            spawnMystic(world); // OK
            spawnBlacksmith(world); // OK
            spawnJeweller(world); // OK
            spawnMarket(world); // OK
            spawnEnchantress(world); // OK
//            spawnTemplar(world); // ERROR
            spawnScoundrel(world); // OK
        }

        private static void spawnRaven0(World world)
        {
            uint aID = world.NewActorID;
            #region ACDEnterKnown 0x789A00DE
            world.BroadcastGlobal(new ACDEnterKnownMessage()
            {
                Id = 0x003B,
                ActorID = aID,
                ActorSNO = 0x00013871,
                Field2 = 0x00000008,
                Field3 = 0x00000000,
                WorldLocation = new WorldLocationMessageData()
                {
                    Scale = 1f,
                    Transform = new PRTransform()
                    {
                        Rotation = new Quaternion()
                        {
                            Amount = -0.7652696f,
                            Axis = new Vector3D()
                            {
                                X = 0f,
                                Y = 0f,
                                Z = 0.6437099f,
                            },
                        },
                        ReferencePoint = new Vector3D()
                        {
                            X = 3113.659f,
                            Y = 2803.692f,
                            Z = 73.26618f,
                        },
                    },
                    WorldID = world.DynamicID,
                },
                InventoryLocation = null,
                GBHandle = new GBHandle()
                {
                    Type = -1,
                    GBID = -1,
                },
                Field7 = 0x00000001,
                Field8 = 0x00013871,
                Field9 = 0x00000000,
                Field10 = 0x00,
                Field12 = 0x00012A04,
                Field13 = 0x00000003,
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000001,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000002,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new ACDCollFlagsMessage()
            {
                Id = 0x00A6,
                ActorID = aID,
                CollFlags = 0x00000001,
            });

            world.BroadcastGlobal(new AttributesSetValuesMessage()
            {
                Id = 0x004D,
                ActorID = aID,
                atKeyVals = new NetAttributeKeyValue[6]
    {
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0056], // Hitpoints_Max_Total
    Int = 0x00000000,
    Float = 1f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0054], // Hitpoints_Max
    Int = 0x00000000,
    Float = 0.0009994507f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0051], // Hitpoints_Total_From_Level
    Int = 0x00000000,
    Float = 3.051758E-05f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x004D], // Hitpoints_Cur
    Int = 0x00000000,
    Float = 0.0009994507f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0043], // TeamID
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0026], // Level
    Int = 0x00000001,
    Float = 0f,
    },
    },
            });

            world.BroadcastGlobal(new ACDGroupMessage()
            {
                Id = 0x00B8,
                ActorID = aID,
                Field1 = -1,
                Field2 = -1,
            });

            world.BroadcastGlobal(new ANNDataMessage(Opcodes.ANNDataMessage7)
            {
                Id = 0x003E,
                ActorID = aID,
            });

            world.BroadcastGlobal(new ACDTranslateFacingMessage()
            {
                Id = 0x0070,
                ActorID = aID,
                Angle = 4.884459f,
                Field2 = false,
            });

            world.BroadcastGlobal(new SNONameDataMessage()
            {
                Id = 0x00D3,
                Name = new SNOName()
                {
                    Group = 0x00000001,
                    Handle = 0x00013871,
                },
            });
            #endregion

        }

        public static void spawnStash(World world, Player.Player player)
        {
            uint aID = world.NewActorID;
            #region ACDEnterKnown 0x77C50009
            player.InGameClient.SendMessage(new ACDEnterKnownMessage()
            {
                Id = 0x003B,
                ActorID = aID,
                ActorSNO = 0x0001FD60,
                Field2 = 0x00000000,
                Field3 = 0x00000000,
                WorldLocation = new WorldLocationMessageData()
                {
                    Scale = 1f,
                    Transform = new PRTransform()
                    {
                        Rotation = new Quaternion()
                        {
                            Amount = 0.9997472f,
                            Axis = new Vector3D()
                            {
                                X = 1.02945E-05f,
                                Y = 3.217819E-05f,
                                Z = 0.0224885f,
                            },
                        },
                        ReferencePoint = new Vector3D()
                        {
                            X = 2970.619f,
                            Y = 2789.915f,
                            Z = 23.94531f,
                        },
                    },
                    WorldID = world.DynamicID,
                },
                InventoryLocation = null,
                GBHandle = new GBHandle()
                {
                    Type = -1,
                    GBID = -1,
                },
                Field7 = 0x00000001,
                Field8 = 0x0001FD60,
                Field9 = 0x00000000,
                Field10 = 0x00,
                Field12 = 0x0000DBD3,
                Field13 = 0x0000000D,
            });

            player.InGameClient.SendMessage(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000001,
                aAffixGBIDs = new int[0]
    {
    },
            });

            player.InGameClient.SendMessage(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000002,
                aAffixGBIDs = new int[0]
    {
    },
            });

            player.InGameClient.SendMessage(new ACDCollFlagsMessage()
            {
                Id = 0x00A6,
                ActorID = aID,
                CollFlags = 0x00000411,
            });

            player.InGameClient.SendMessage(new AttributesSetValuesMessage()
            {
                Id = 0x004D,
                ActorID = aID,
                atKeyVals = new NetAttributeKeyValue[11]
    {
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x02BC], // MinimapActive
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x000FFFFF,
    Attribute = GameAttribute.Attributes[0x01B9], // Buff_Visual_Effect
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0000F50B,
    Attribute = GameAttribute.Attributes[0x0230], // Buff_Icon_Count0
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0000F50B,
    Attribute = GameAttribute.Attributes[0x01CC], // Buff_Active
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0056], // Hitpoints_Max_Total
    Int = 0x00000000,
    Float = 1f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0054], // Hitpoints_Max
    Int = 0x00000000,
    Float = 0.0009994507f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0051], // Hitpoints_Total_From_Level
    Int = 0x00000000,
    Float = 3.051758E-05f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x004D], // Hitpoints_Cur
    Int = 0x00000000,
    Float = 0.0009994507f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0045], // Invulnerable
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0043], // TeamID
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0026], // Level
    Int = 0x00000001,
    Float = 0f,
    },
    },
            });

            player.InGameClient.SendMessage(new ACDGroupMessage()
            {
                Id = 0x00B8,
                ActorID = aID,
                Field1 = -1,
                Field2 = -1,
            });

            player.InGameClient.SendMessage(new ANNDataMessage(Opcodes.ANNDataMessage7)
            {
                Id = 0x003E,
                ActorID = aID,
            });

            player.InGameClient.SendMessage(new ACDTranslateFacingMessage()
            {
                Id = 0x0070,
                ActorID = aID,
                Angle = 0.04497663f,
                Field2 = false,
            });
            #endregion
//            player.Inventory.AssignSharedStash(aID);
        }


        private static void spawnWaypoint(World world)
        {
            uint aID = world.NewActorID;
            #region ACDEnterKnown 0x77C10005
            world.BroadcastGlobal(new ACDEnterKnownMessage()
            {
                Id = 0x003B,
                ActorID = aID,
                ActorSNO = 0x0000192A,
                Field2 = 0x00000000,
                Field3 = 0x00000000,
                WorldLocation = new WorldLocationMessageData()
                {
                    Scale = 1f,
                    Transform = new PRTransform()
                    {
                        Rotation = new Quaternion()
                        {
                            Amount = 0.9228876f,
                            Axis = new Vector3D()
                            {
                                X = 0f,
                                Y = 0f,
                                Z = -0.3850694f,
                            },
                        },
                        ReferencePoint = new Vector3D()
                        {
                            X = 2981.73f,
                            Y = 2835.009f,
                            Z = 24.66344f,
                        },
                    },
                    WorldID = world.DynamicID,
                },
                InventoryLocation = null,
                GBHandle = new GBHandle()
                {
                    Type = -1,
                    GBID = -1,
                },
                Field7 = 0x00000001,
                Field8 = 0x0000192A,
                Field9 = 0x00000000,
                Field10 = 0x00,
                Field12 = 0x0000DBD3,
                Field13 = 0x00000009,
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000001,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000002,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new ACDCollFlagsMessage()
            {
                Id = 0x00A6,
                ActorID = aID,
                CollFlags = 0x00000080,
            });

            world.BroadcastGlobal(new AttributesSetValuesMessage()
            {
                Id = 0x004D,
                ActorID = aID,
                atKeyVals = new NetAttributeKeyValue[6]
    {
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0056], // Hitpoints_Max_Total
    Int = 0x00000000,
    Float = 1f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0054], // Hitpoints_Max
    Int = 0x00000000,
    Float = 0.0009994507f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0051], // Hitpoints_Total_From_Level
    Int = 0x00000000,
    Float = 3.051758E-05f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x004D], // Hitpoints_Cur
    Int = 0x00000000,
    Float = 0.0009994507f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0043], // TeamID
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0026], // Level
    Int = 0x00000001,
    Float = 0f,
    },
    },
            });

            world.BroadcastGlobal(new ACDGroupMessage()
            {
                Id = 0x00B8,
                ActorID = aID,
                Field1 = -1,
                Field2 = -1,
            });

            world.BroadcastGlobal(new ANNDataMessage(Opcodes.ANNDataMessage7)
            {
                Id = 0x003E,
                ActorID = aID,
            });

            world.BroadcastGlobal(new ACDTranslateFacingMessage()
            {
                Id = 0x0070,
                ActorID = aID,
                Angle = 5.492608f,
                Field2 = false,
            });
            #endregion
        }

        private static void spawnStartLocation0(World world)
        {
            uint aID = world.NewActorID;
            #region ACDEnterKnown 0x78DD0118
            world.BroadcastGlobal(new ACDEnterKnownMessage()
            {
                ActorID = aID,
                ActorSNO = 0x0000157E,
                Field2 = 0x00000008,
                Field3 = 0x00000000,
                WorldLocation = new WorldLocationMessageData()
                {
                    Scale = 1f,
                    Transform = new PRTransform()
                    {
                        Rotation = new Quaternion()
                        {
                            Amount = -0.01089788f,
                            Axis = new Vector3D()
                            {
                                X = 0f,
                                Y = 0f,
                                Z = 0.9999406f,
                            },
                        },
                        ReferencePoint = new Vector3D()
                        {
                            X = 3125.888f,
                            Y = 2602.642f,
                            Z = 1.050535f,
                        },
                    },
                    WorldID = world.DynamicID,
                },
                InventoryLocation = null,
                GBHandle = new GBHandle()
                {
                    Type = -1,
                    GBID = -1,
                },
                Field7 = 0x00000001,
                Field8 = 0x0000157E,
                Field9 = 0x00000000,
                Field10 = 0x00,
                Field12 = 0x00011B71,
                Field13 = 0x00000000,
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                ActorID = aID,
                Field1 = 0x00000001,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                ActorID = aID,
                Field1 = 0x00000002,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new ACDCollFlagsMessage()
            {
                ActorID = aID,
                CollFlags = 0x00000000,
            });

            world.BroadcastGlobal(new AttributesSetValuesMessage()
            {
                ActorID = aID,
                atKeyVals = new NetAttributeKeyValue[6]
    {
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0056], // Hitpoints_Max_Total
    Int = 0x00000000,
    Float = 1f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0054], // Hitpoints_Max
    Int = 0x00000000,
    Float = 0.0009994507f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0051], // Hitpoints_Total_From_Level
    Int = 0x00000000,
    Float = 3.051758E-05f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x004D], // Hitpoints_Cur
    Int = 0x00000000,
    Float = 0.0009994507f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0043], // TeamID
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0026], // Level
    Int = 0x00000001,
    Float = 0f,
    },
    },
            });

            world.BroadcastGlobal(new ACDGroupMessage()
            {
                ActorID = aID,
                Field1 = -1,
                Field2 = -1,
            });

            world.BroadcastGlobal(new ANNDataMessage(Opcodes.ANNDataMessage7)
            {
                ActorID = aID,
            });

            world.BroadcastGlobal(new ACDTranslateFacingMessage()
            {
                ActorID = aID,
                Angle = 3.163388f,
                Field2 = false,
            });

            world.BroadcastGlobal(new SNONameDataMessage()
            {
                Id = 0x00D3,
                Name = new SNOName()
                {
                    Group = 0x00000001,
                    Handle = 0x0000157E,
                },
            });
            #endregion
        }

        private static void spawnFerryman(World world)
        {
            uint aID = world.NewActorID;
            #region ACDEnterKnown 0x78DF011A
            world.BroadcastGlobal(new ACDEnterKnownMessage()
            {
                Id = 0x003B,
                ActorID = aID,
                ActorSNO = 0x000255BB,
                Field2 = 0x00000008,
                Field3 = 0x00000000,
                WorldLocation = new WorldLocationMessageData()
                {
                    Scale = 1.13f,
                    Transform = new PRTransform()
                    {
                        Rotation = new Quaternion()
                        {
                            Amount = 0.1261874f,
                            Axis = new Vector3D()
                            {
                                X = 0f,
                                Y = 0f,
                                Z = 0.9920065f,
                            },
                        },
                        ReferencePoint = new Vector3D()
                        {
                            X = 3131.338f,
                            Y = 2597.316f,
                            Z = 0.9298096f,
                        },
                    },
                    WorldID = world.DynamicID,
                },
                InventoryLocation = null,
                GBHandle = new GBHandle()
                {
                    Type = -1,
                    GBID = -1,
                },
                Field7 = 0x00000001,
                Field8 = 0x000255BB,
                Field9 = 0x00000000,
                Field10 = 0x00,
                Field12 = 0x00011B71,
                Field13 = 0x00000002,
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000001,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000002,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new ACDCollFlagsMessage()
            {
                Id = 0x00A6,
                ActorID = aID,
                CollFlags = 0x00000001,
            });

            world.BroadcastGlobal(new AttributesSetValuesMessage()
            {
                Id = 0x004D,
                ActorID = aID,
                atKeyVals = new NetAttributeKeyValue[13]
    {
    new NetAttributeKeyValue()
    {
    Field0 = 0x000FFFFF,
    Attribute = GameAttribute.Attributes[0x01B9], // Buff_Visual_Effect
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x00000000,
    Attribute = GameAttribute.Attributes[0x0158], // NPC_Has_Interact_Options
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x00000000,
    Attribute = GameAttribute.Attributes[0x0159], // Conversation_Icon
    Int = 0x00000000,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0001F972,
    Attribute = GameAttribute.Attributes[0x01CC], // Buff_Active
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0001F972,
    Attribute = GameAttribute.Attributes[0x0230], // Buff_Icon_Count0
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0156], // NPC_Is_Operatable
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0056], // Hitpoints_Max_Total
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0155], // Is_NPC
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0054], // Hitpoints_Max
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0051], // Hitpoints_Total_From_Level
    Int = 0x00000000,
    Float = 3.051758E-05f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x004D], // Hitpoints_Cur
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0043], // TeamID
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0026], // Level
    Int = 0x00000004,
    Float = 0f,
    },
    },
            });

            world.BroadcastGlobal(new ACDGroupMessage()
            {
                Id = 0x00B8,
                ActorID = aID,
                Field1 = -1,
                Field2 = -1,
            });

            world.BroadcastGlobal(new ANNDataMessage(Opcodes.ANNDataMessage7)
            {
                Id = 0x003E,
                ActorID = aID,
            });

            world.BroadcastGlobal(new ACDTranslateFacingMessage()
            {
                Id = 0x0070,
                ActorID = aID,
                Angle = 2.888546f,
                Field2 = false,
            });

            world.BroadcastGlobal(new SetIdleAnimationMessage()
            {
                Id = 0x00A5,
                ActorID = aID,
                AnimationSNO = 0x00011150,
            });

            world.BroadcastGlobal(new SNONameDataMessage()
            {
                Id = 0x00D3,
                Name = new SNOName()
                {
                    Group = 0x00000001,
                    Handle = 0x000255BB,
                },
            });
            world.BroadcastGlobal(new TrickleMessage()
            {
                Id = 0x0042,
                ActorID = aID,
                Field1 = 0x000255BB,
                Field2 = new WorldPlace()
                {
                    Position = new Vector3D()
                    {
                        X = 3131.338f,
                        Y = 2597.316f,
                        Z = 0.9298096f,
                    },
                    WorldID = world.DynamicID,
                },
                Field4 = 0x00004DEB,
                Field5 = 1f,
                Field6 = 0x00000008,
                Field7 = 0x00000024,
                Field10 = 0x0AF96544,
                Field12 = 0x0000F063,
            });
            #endregion
        }

        private static void spawnMystic(World world)
        {
            uint aID = world.NewActorID;
            #region ACDEnterKnown 0x78DF011A
            world.BroadcastGlobal(new ACDEnterKnownMessage()
            {
                Id = 0x003B,
                ActorID = aID,
                ActorSNO = 56948,
                Field2 = 0x00000008,
                Field3 = 0x00000000,
                WorldLocation = new WorldLocationMessageData()
                {
                    Scale = 1.13f,
                    Transform = new PRTransform()
                    {
                        Rotation = new Quaternion()
                        {
                            Amount = 0.1261874f,
                            Axis = new Vector3D()
                            {
                                X = 0f,
                                Y = 0f,
                                Z = 0.9920065f,
                            },
                        },
                        ReferencePoint = new Vector3D()
                        {
                            X = 2980.198f,
                            Y = 2795.186f,
                            Z = 24.04533f,
                        },
                    },
                    WorldID = world.DynamicID,
                },
                InventoryLocation = null,
                GBHandle = new GBHandle()
                {
                    Type = -1,
                    GBID = -1,
                },
                Field7 = 0x00000001,
                Field8 = 56948,
                Field9 = 0x00000000,
                Field10 = 0x00,
                Field12 = 0x00011B71,
                Field13 = 0x00000002,
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000001,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000002,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new ACDCollFlagsMessage()
            {
                Id = 0x00A6,
                ActorID = aID,
                CollFlags = 0x00000001,
            });

            world.BroadcastGlobal(new AttributesSetValuesMessage()
            {
                Id = 0x004D,
                ActorID = aID,
                atKeyVals = new NetAttributeKeyValue[13]
    {
    new NetAttributeKeyValue()
    {
    Field0 = 0x000FFFFF,
    Attribute = GameAttribute.Attributes[0x01B9], // Buff_Visual_Effect
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x00000000,
    Attribute = GameAttribute.Attributes[0x0158], // NPC_Has_Interact_Options
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x00000000,
    Attribute = GameAttribute.Attributes[0x0159], // Conversation_Icon
    Int = 0x00000000,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0001F972,
    Attribute = GameAttribute.Attributes[0x01CC], // Buff_Active
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0001F972,
    Attribute = GameAttribute.Attributes[0x0230], // Buff_Icon_Count0
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0156], // NPC_Is_Operatable
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0056], // Hitpoints_Max_Total
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0155], // Is_NPC
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0054], // Hitpoints_Max
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0051], // Hitpoints_Total_From_Level
    Int = 0x00000000,
    Float = 3.051758E-05f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x004D], // Hitpoints_Cur
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0043], // TeamID
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0026], // Level
    Int = 0x00000004,
    Float = 0f,
    },
    },
            });

            world.BroadcastGlobal(new ACDGroupMessage()
            {
                Id = 0x00B8,
                ActorID = aID,
                Field1 = -1,
                Field2 = -1,
            });

            world.BroadcastGlobal(new ANNDataMessage(Opcodes.ANNDataMessage7)
            {
                Id = 0x003E,
                ActorID = aID,
            });

            world.BroadcastGlobal(new ACDTranslateFacingMessage()
            {
                Id = 0x0070,
                ActorID = aID,
                Angle = 2.888546f,
                Field2 = false,
            });

            world.BroadcastGlobal(new SetIdleAnimationMessage()
            {
                Id = 0x00A5,
                ActorID = aID,
                AnimationSNO = 0x00011150,
            });

            world.BroadcastGlobal(new SNONameDataMessage()
            {
                Id = 0x00D3,
                Name = new SNOName()
                {
                    Group = 0x00000001,
                    Handle = 56948,
                },
            });
            world.BroadcastGlobal(new TrickleMessage()
            {
                Id = 0x0042,
                ActorID = aID,
                Field1 = 56948,
                Field2 = new WorldPlace()
                {
                    Position = new Vector3D()
                    {
                        X = 2980.198f,
                        Y = 2795.186f,
                        Z = 24.04533f,
                    },
                    WorldID = world.DynamicID,
                },
                Field4 = 0x00004DEB,
                Field5 = 1f,
                Field6 = 0x00000008,
                Field7 = 0x00000024,
                Field10 = 0x0AF96544,
                Field12 = 0x0000F063,
            });
            #endregion
        }

        private static void spawnBlacksmith(World world)
        {
            uint aID = world.NewActorID;
            #region ACDEnterKnown 0x78DF011A
            world.BroadcastGlobal(new ACDEnterKnownMessage()
            {
                Id = 0x003B,
                ActorID = aID,
                ActorSNO = 56947,
                Field2 = 0x00000008,
                Field3 = 0x00000000,
                WorldLocation = new WorldLocationMessageData()
                {
                    Scale = 1.13f,
                    Transform = new PRTransform()
                    {
                        Rotation = new Quaternion()
                        {
                            Amount = 1.6f,
                            Axis = new Vector3D()
                            {
                                X = 0f,
                                Y = 0f,
                                Z = 0.9920065f,
                            },
                        },
                        ReferencePoint = new Vector3D()
                        {
                            X = 3003.498f,
                            Y = 2788.01f,
                            Z = 24.04532f,
                        },
                    },
                    WorldID = world.DynamicID,
                },
                InventoryLocation = null,
                GBHandle = new GBHandle()
                {
                    Type = -1,
                    GBID = -1,
                },
                Field7 = 0x00000001,
                Field8 = 56947,
                Field9 = 0x00000000,
                Field10 = 0x00,
                Field12 = 0x00011B71,
                Field13 = 0x00000002,
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000001,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000002,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new ACDCollFlagsMessage()
            {
                Id = 0x00A6,
                ActorID = aID,
                CollFlags = 0x00000001,
            });

            world.BroadcastGlobal(new AttributesSetValuesMessage()
            {
                Id = 0x004D,
                ActorID = aID,
                atKeyVals = new NetAttributeKeyValue[13]
    {
    new NetAttributeKeyValue()
    {
    Field0 = 0x000FFFFF,
    Attribute = GameAttribute.Attributes[0x01B9], // Buff_Visual_Effect
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x00000000,
    Attribute = GameAttribute.Attributes[0x0158], // NPC_Has_Interact_Options
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x00000000,
    Attribute = GameAttribute.Attributes[0x0159], // Conversation_Icon
    Int = 0x00000000,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0001F972,
    Attribute = GameAttribute.Attributes[0x01CC], // Buff_Active
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0001F972,
    Attribute = GameAttribute.Attributes[0x0230], // Buff_Icon_Count0
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0156], // NPC_Is_Operatable
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0056], // Hitpoints_Max_Total
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0155], // Is_NPC
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0054], // Hitpoints_Max
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0051], // Hitpoints_Total_From_Level
    Int = 0x00000000,
    Float = 3.051758E-05f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x004D], // Hitpoints_Cur
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0043], // TeamID
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0026], // Level
    Int = 0x00000004,
    Float = 0f,
    },
    },
            });

            world.BroadcastGlobal(new ACDGroupMessage()
            {
                Id = 0x00B8,
                ActorID = aID,
                Field1 = -1,
                Field2 = -1,
            });

            world.BroadcastGlobal(new ANNDataMessage(Opcodes.ANNDataMessage7)
            {
                Id = 0x003E,
                ActorID = aID,
            });

            world.BroadcastGlobal(new ACDTranslateFacingMessage()
            {
                Id = 0x0070,
                ActorID = aID,
                Angle = 2.888546f,
                Field2 = false,
            });

            world.BroadcastGlobal(new SetIdleAnimationMessage()
            {
                Id = 0x00A5,
                ActorID = aID,
                AnimationSNO = 0x00011150,
            });

            world.BroadcastGlobal(new SNONameDataMessage()
            {
                Id = 0x00D3,
                Name = new SNOName()
                {
                    Group = 0x00000001,
                    Handle = 56947,
                },
            });
            world.BroadcastGlobal(new TrickleMessage()
            {
                Id = 0x0042,
                ActorID = aID,
                Field1 = 56947,
                Field2 = new WorldPlace()
                {
                    Position = new Vector3D()
                    {
                        X = 3003.498f,
                        Y = 2788.01f,
                        Z = 24.04532f,
                    },
                    WorldID = world.DynamicID,
                },
                Field4 = 0x00004DEB,
                Field5 = 1f,
                Field6 = 0x00000008,
                Field7 = 0x00000024,
                Field10 = 0x0AF96544,
                Field12 = 0x0000F063,
            });
            #endregion
        }

        private static void spawnJeweller(World world)
        {
            uint aID = world.NewActorID;
            #region ACDEnterKnown 0x78DF011A
            world.BroadcastGlobal(new ACDEnterKnownMessage()
            {
                Id = 0x003B,
                ActorID = aID,
                ActorSNO = 56949,
                Field2 = 0x00000008,
                Field3 = 0x00000000,
                WorldLocation = new WorldLocationMessageData()
                {
                    Scale = 1.13f,
                    Transform = new PRTransform()
                    {
                        Rotation = new Quaternion()
                        {
                            Amount = 1.6f,
                            Axis = new Vector3D()
                            {
                                X = 0f,
                                Y = 0f,
                                Z = 0.9920065f,
                            },
                        },
                        ReferencePoint = new Vector3D()
                        {
                            X = 2940.198f,
                            Y = 2795.186f,
                            Z = 24.04533f,
                        },
                    },
                    WorldID = world.DynamicID,
                },
                InventoryLocation = null,
                GBHandle = new GBHandle()
                {
                    Type = -1,
                    GBID = -1,
                },
                Field7 = 0x00000001,
                Field8 = 56949,
                Field9 = 0x00000000,
                Field10 = 0x00,
                Field12 = 0x00011B71,
                Field13 = 0x00000002,
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000001,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000002,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new ACDCollFlagsMessage()
            {
                Id = 0x00A6,
                ActorID = aID,
                CollFlags = 0x00000001,
            });

            world.BroadcastGlobal(new AttributesSetValuesMessage()
            {
                Id = 0x004D,
                ActorID = aID,
                atKeyVals = new NetAttributeKeyValue[13]
    {
    new NetAttributeKeyValue()
    {
    Field0 = 0x000FFFFF,
    Attribute = GameAttribute.Attributes[0x01B9], // Buff_Visual_Effect
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x00000000,
    Attribute = GameAttribute.Attributes[0x0158], // NPC_Has_Interact_Options
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x00000000,
    Attribute = GameAttribute.Attributes[0x0159], // Conversation_Icon
    Int = 0x00000000,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0001F972,
    Attribute = GameAttribute.Attributes[0x01CC], // Buff_Active
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0001F972,
    Attribute = GameAttribute.Attributes[0x0230], // Buff_Icon_Count0
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0156], // NPC_Is_Operatable
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0056], // Hitpoints_Max_Total
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0155], // Is_NPC
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0054], // Hitpoints_Max
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0051], // Hitpoints_Total_From_Level
    Int = 0x00000000,
    Float = 3.051758E-05f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x004D], // Hitpoints_Cur
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0043], // TeamID
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0026], // Level
    Int = 0x00000004,
    Float = 0f,
    },
    },
            });

            world.BroadcastGlobal(new ACDGroupMessage()
            {
                Id = 0x00B8,
                ActorID = aID,
                Field1 = -1,
                Field2 = -1,
            });

            world.BroadcastGlobal(new ANNDataMessage(Opcodes.ANNDataMessage7)
            {
                Id = 0x003E,
                ActorID = aID,
            });

            world.BroadcastGlobal(new ACDTranslateFacingMessage()
            {
                Id = 0x0070,
                ActorID = aID,
                Angle = 2.888546f,
                Field2 = false,
            });

            world.BroadcastGlobal(new SetIdleAnimationMessage()
            {
                Id = 0x00A5,
                ActorID = aID,
                AnimationSNO = 0x00011150,
            });

            world.BroadcastGlobal(new SNONameDataMessage()
            {
                Id = 0x00D3,
                Name = new SNOName()
                {
                    Group = 0x00000001,
                    Handle = 56949,
                },
            });
            world.BroadcastGlobal(new TrickleMessage()
            {
                Id = 0x0042,
                ActorID = aID,
                Field1 = 56949,
                Field2 = new WorldPlace()
                {
                    Position = new Vector3D()
                    {
                        X = 2940.198f,
                        Y = 2795.186f,
                        Z = 24.04533f,
                    },
                    WorldID = world.DynamicID,
                },
                Field4 = 0x00004DEB,
                Field5 = 1f,
                Field6 = 0x00000008,
                Field7 = 0x00000024,
                Field10 = 0x0AF96544,
                Field12 = 0x0000F063,
            });
            #endregion
        }

        private static void spawnMarket(World world)
        {
            uint aID = world.NewActorID;
            #region ACDEnterKnown 0x78DF011A
            world.BroadcastGlobal(new ACDEnterKnownMessage()
            {
                Id = 0x003B,
                ActorID = aID,
                ActorSNO = 81610,//0x00001243,
                Field2 = 0x00000008,
                Field3 = 0x00000000,
                WorldLocation = new WorldLocationMessageData()
                {
                    Scale = 1.13f,
                    Transform = new PRTransform()
                    {
                        Rotation = new Quaternion()
                        {
                            Amount = 1.6f,
                            Axis = new Vector3D()
                            {
                                X = 0f,
                                Y = 0f,
                                Z = 0.9920065f,
                            },
                        },
                        ReferencePoint = new Vector3D()
                        {
                            X = 2922.286f,
                            Y = 2796.864f,
                            Z = 23.94531f
                        },
                    },
                    WorldID = world.DynamicID,
                },
                InventoryLocation = null,
                GBHandle = new GBHandle()
                {
                    Type = -1,
                    GBID = -1,
                },
                Field7 = 0x00000001,
                Field8 = 81610,//0x00001243,
                Field9 = 0x00000000,
                Field10 = 0x00,
                Field12 = 0x00011B71,
                Field13 = 0x00000002,
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000001,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000002,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new ACDCollFlagsMessage()
            {
                Id = 0x00A6,
                ActorID = aID,
                CollFlags = 0x00000001,
            });

            world.BroadcastGlobal(new AttributesSetValuesMessage()
            {
                Id = 0x004D,
                ActorID = aID,
                atKeyVals = new NetAttributeKeyValue[13]
    {
    new NetAttributeKeyValue()
    {
    Field0 = 0x000FFFFF,
    Attribute = GameAttribute.Attributes[0x01B9], // Buff_Visual_Effect
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x00000000,
    Attribute = GameAttribute.Attributes[0x0158], // NPC_Has_Interact_Options
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x00000000,
    Attribute = GameAttribute.Attributes[0x0159], // Conversation_Icon
    Int = 0x00000000,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0001F972,
    Attribute = GameAttribute.Attributes[0x01CC], // Buff_Active
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0001F972,
    Attribute = GameAttribute.Attributes[0x0230], // Buff_Icon_Count0
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0156], // NPC_Is_Operatable
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0056], // Hitpoints_Max_Total
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0155], // Is_NPC
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0054], // Hitpoints_Max
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0051], // Hitpoints_Total_From_Level
    Int = 0x00000000,
    Float = 3.051758E-05f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x004D], // Hitpoints_Cur
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0043], // TeamID
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0026], // Level
    Int = 0x00000004,
    Float = 0f,
    },
    },
            });

            world.BroadcastGlobal(new ACDGroupMessage()
            {
                Id = 0x00B8,
                ActorID = aID,
                Field1 = -1,
                Field2 = -1,
            });

            world.BroadcastGlobal(new ANNDataMessage(Opcodes.ANNDataMessage7)
            {
                Id = 0x003E,
                ActorID = aID,
            });

            world.BroadcastGlobal(new ACDTranslateFacingMessage()
            {
                Id = 0x0070,
                ActorID = aID,
                Angle = 2.888546f,
                Field2 = false,
            });

            world.BroadcastGlobal(new SetIdleAnimationMessage()
            {
                Id = 0x00A5,
                ActorID = aID,
                AnimationSNO = 0x00011150,
            });

            world.BroadcastGlobal(new SNONameDataMessage()
            {
                Id = 0x00D3,
                Name = new SNOName()
                {
                    Group = 0x00000001,
                    Handle = 81610,//0x00001243,
                },
            });
            world.BroadcastGlobal(new TrickleMessage()
            {
                Id = 0x0042,
                ActorID = aID,
                Field1 = 81610,//0x00001243,
                Field2 = new WorldPlace()
                {
                    Position = new Vector3D()
                    {
                        X = 2922.286f,
                        Y = 2796.864f,
                        Z = 23.94531f
                    },
                    WorldID = world.DynamicID,
                },
                Field4 = 0x00004DEB,
                Field5 = 1f,
                Field6 = 0x00000008,
                Field7 = 0x00000024,
                Field10 = 0x0AF96544,
                Field12 = 0x0000F063,
            });
            #endregion
        }

        private static void spawnEnchantress(World world)
        {
            uint aID = world.NewActorID;
            #region ACDEnterKnown 0x78DF011A
            world.BroadcastGlobal(new ACDEnterKnownMessage()
            {
                Id = 0x003B,
                ActorID = aID,
                ActorSNO = 4062,
                Field2 = 0x00000008,
                Field3 = 0x00000000,
                WorldLocation = new WorldLocationMessageData()
                {
                    Scale = 1.13f,
                    Transform = new PRTransform()
                    {
                        Rotation = new Quaternion()
                        {
                            Amount = 1.6f,
                            Axis = new Vector3D()
                            {
                                X = 0f,
                                Y = 0f,
                                Z = 0.9920065f,
                            },
                        },
                        ReferencePoint = new Vector3D()
                        {
                            X = 2962.286f,
                            Y = 2796.864f,
                            Z = 23.94531f
                        },
                    },
                    WorldID = world.DynamicID,
                },
                InventoryLocation = null,
                GBHandle = new GBHandle()
                {
                    Type = -1,
                    GBID = -1,
                },
                Field7 = 0x00000001,
                Field8 = 4062,
                Field9 = 0x00000000,
                Field10 = 0x00,
                Field12 = 0x00011B71,
                Field13 = 0x00000002,
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000001,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000002,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new ACDCollFlagsMessage()
            {
                Id = 0x00A6,
                ActorID = aID,
                CollFlags = 0x00000001,
            });

            world.BroadcastGlobal(new AttributesSetValuesMessage()
            {
                Id = 0x004D,
                ActorID = aID,
                atKeyVals = new NetAttributeKeyValue[13]
    {
    new NetAttributeKeyValue()
    {
    Field0 = 0x000FFFFF,
    Attribute = GameAttribute.Attributes[0x01B9], // Buff_Visual_Effect
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x00000000,
    Attribute = GameAttribute.Attributes[0x0158], // NPC_Has_Interact_Options
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x00000000,
    Attribute = GameAttribute.Attributes[0x0159], // Conversation_Icon
    Int = 0x00000000,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0001F972,
    Attribute = GameAttribute.Attributes[0x01CC], // Buff_Active
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0001F972,
    Attribute = GameAttribute.Attributes[0x0230], // Buff_Icon_Count0
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0156], // NPC_Is_Operatable
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0056], // Hitpoints_Max_Total
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0155], // Is_NPC
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0054], // Hitpoints_Max
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0051], // Hitpoints_Total_From_Level
    Int = 0x00000000,
    Float = 3.051758E-05f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x004D], // Hitpoints_Cur
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0043], // TeamID
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0026], // Level
    Int = 0x00000004,
    Float = 0f,
    },
    },
            });

            world.BroadcastGlobal(new ACDGroupMessage()
            {
                Id = 0x00B8,
                ActorID = aID,
                Field1 = -1,
                Field2 = -1,
            });

            world.BroadcastGlobal(new ANNDataMessage(Opcodes.ANNDataMessage7)
            {
                Id = 0x003E,
                ActorID = aID,
            });

            world.BroadcastGlobal(new ACDTranslateFacingMessage()
            {
                Id = 0x0070,
                ActorID = aID,
                Angle = 2.888546f,
                Field2 = false,
            });

            world.BroadcastGlobal(new SetIdleAnimationMessage()
            {
                Id = 0x00A5,
                ActorID = aID,
                AnimationSNO = 0x00011150,
            });

            world.BroadcastGlobal(new SNONameDataMessage()
            {
                Id = 0x00D3,
                Name = new SNOName()
                {
                    Group = 0x00000001,
                    Handle = 4062,
                },
            });
            world.BroadcastGlobal(new TrickleMessage()
            {
                Id = 0x0042,
                ActorID = aID,
                Field1 = 4062,
                Field2 = new WorldPlace()
                {
                    Position = new Vector3D()
                    {
                        X = 2962.286f,
                        Y = 2796.864f,
                        Z = 23.94531f
                    },
                    WorldID = world.DynamicID,
                },
                Field4 = 0x00004DEB,
                Field5 = 1f,
                Field6 = 0x00000008,
                Field7 = 0x00000024,
                Field10 = 0x0AF96544,
                Field12 = 0x0000F063,
            });
            #endregion
        }

        private static void spawnTemplar(World world)
        {
            uint aID = world.NewActorID;
            #region ACDEnterKnown 0x78DF011A
            world.BroadcastGlobal(new ACDEnterKnownMessage()
            {
                Id = 0x003B,
                ActorID = aID,
                ActorSNO = 87037, // or 4538, both crashing
                Field2 = 0x00000008,
                Field3 = 0x00000000,
                WorldLocation = new WorldLocationMessageData()
                {
                    Scale = 1.13f,
                    Transform = new PRTransform()
                    {
                        Rotation = new Quaternion()
                        {
                            Amount = 1.6f,
                            Axis = new Vector3D()
                            {
                                X = 0f,
                                Y = 0f,
                                Z = 0.9920065f,
                            },
                        },
                        ReferencePoint = new Vector3D()
                        {
                            X = 2970.241f,
                            Y = 2794.907f,
                            Z = 24.04533f,
                        },
                    },
                    WorldID = world.DynamicID,
                },
                InventoryLocation = null,
                GBHandle = new GBHandle()
                {
                    Type = -1,
                    GBID = -1,
                },
                Field7 = 0x00000001,
                Field8 = 87037,
                Field9 = 0x00000000,
                Field10 = 0x00,
                Field12 = 0x00011B71,
                Field13 = 0x00000002,
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000001,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000002,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new ACDCollFlagsMessage()
            {
                Id = 0x00A6,
                ActorID = aID,
                CollFlags = 0x00000001,
            });

            world.BroadcastGlobal(new AttributesSetValuesMessage()
            {
                Id = 0x004D,
                ActorID = aID,
                atKeyVals = new NetAttributeKeyValue[13]
    {
    new NetAttributeKeyValue()
    {
    Field0 = 0x000FFFFF,
    Attribute = GameAttribute.Attributes[0x01B9], // Buff_Visual_Effect
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x00000000,
    Attribute = GameAttribute.Attributes[0x0158], // NPC_Has_Interact_Options
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x00000000,
    Attribute = GameAttribute.Attributes[0x0159], // Conversation_Icon
    Int = 0x00000000,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0001F972,
    Attribute = GameAttribute.Attributes[0x01CC], // Buff_Active
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0001F972,
    Attribute = GameAttribute.Attributes[0x0230], // Buff_Icon_Count0
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0156], // NPC_Is_Operatable
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0056], // Hitpoints_Max_Total
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0155], // Is_NPC
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0054], // Hitpoints_Max
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0051], // Hitpoints_Total_From_Level
    Int = 0x00000000,
    Float = 3.051758E-05f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x004D], // Hitpoints_Cur
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0043], // TeamID
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0026], // Level
    Int = 0x00000004,
    Float = 0f,
    },
    },
            });

            world.BroadcastGlobal(new ACDGroupMessage()
            {
                Id = 0x00B8,
                ActorID = aID,
                Field1 = -1,
                Field2 = -1,
            });

            world.BroadcastGlobal(new ANNDataMessage(Opcodes.ANNDataMessage7)
            {
                Id = 0x003E,
                ActorID = aID,
            });

            world.BroadcastGlobal(new ACDTranslateFacingMessage()
            {
                Id = 0x0070,
                ActorID = aID,
                Angle = 2.888546f,
                Field2 = false,
            });

            world.BroadcastGlobal(new SetIdleAnimationMessage()
            {
                Id = 0x00A5,
                ActorID = aID,
                AnimationSNO = 0x00011150,
            });

            world.BroadcastGlobal(new SNONameDataMessage()
            {
                Id = 0x00D3,
                Name = new SNOName()
                {
                    Group = 0x00000001,
                    Handle = 87037,
                },
            });
            world.BroadcastGlobal(new TrickleMessage()
            {
                Id = 0x0042,
                ActorID = aID,
                Field1 = 87037,
                Field2 = new WorldPlace()
                {
                    Position = new Vector3D()
                    {
                        X = 2970.241f,
                        Y = 2794.907f,
                        Z = 24.04533f,
                    },
                    WorldID = world.DynamicID,
                },
                Field4 = 0x00004DEB,
                Field5 = 1f,
                Field6 = 0x00000008,
                Field7 = 0x00000024,
                Field10 = 0x0AF96544,
                Field12 = 0x0000F063,
            });
            #endregion
        }

        private static void spawnScoundrel(World world)
        {
            uint aID = world.NewActorID;
            #region ACDEnterKnown 0x78DF011A
            world.BroadcastGlobal(new ACDEnterKnownMessage()
            {
                Id = 0x003B,
                ActorID = aID,
                ActorSNO = 4644,
                Field2 = 0x00000008,
                Field3 = 0x00000000,
                WorldLocation = new WorldLocationMessageData()
                {
                    Scale = 1.13f,
                    Transform = new PRTransform()
                    {
                        Rotation = new Quaternion()
                        {
                            Amount = 1.6f,
                            Axis = new Vector3D()
                            {
                                X = 0f,
                                Y = 0f,
                                Z = 0.9920065f,
                            },
                        },
                        ReferencePoint = new Vector3D()
                        {
                            X = 2932.286f,
                            Y = 2796.864f,
                            Z = 23.94531f
                        },
                    },
                    WorldID = world.DynamicID,
                },
                InventoryLocation = null,
                GBHandle = new GBHandle()
                {
                    Type = -1,
                    GBID = -1,
                },
                Field7 = 0x00000001,
                Field8 = 4644,
                Field9 = 0x00000000,
                Field10 = 0x00,
                Field12 = 0x00011B71,
                Field13 = 0x00000002,
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000001,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new AffixMessage()
            {
                Id = 0x0048,
                ActorID = aID,
                Field1 = 0x00000002,
                aAffixGBIDs = new int[0]
    {
    },
            });

            world.BroadcastGlobal(new ACDCollFlagsMessage()
            {
                Id = 0x00A6,
                ActorID = aID,
                CollFlags = 0x00000001,
            });

            world.BroadcastGlobal(new AttributesSetValuesMessage()
            {
                Id = 0x004D,
                ActorID = aID,
                atKeyVals = new NetAttributeKeyValue[13]
    {
    new NetAttributeKeyValue()
    {
    Field0 = 0x000FFFFF,
    Attribute = GameAttribute.Attributes[0x01B9], // Buff_Visual_Effect
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x00000000,
    Attribute = GameAttribute.Attributes[0x0158], // NPC_Has_Interact_Options
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x00000000,
    Attribute = GameAttribute.Attributes[0x0159], // Conversation_Icon
    Int = 0x00000000,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0001F972,
    Attribute = GameAttribute.Attributes[0x01CC], // Buff_Active
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Field0 = 0x0001F972,
    Attribute = GameAttribute.Attributes[0x0230], // Buff_Icon_Count0
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0156], // NPC_Is_Operatable
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0056], // Hitpoints_Max_Total
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0155], // Is_NPC
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0054], // Hitpoints_Max
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0051], // Hitpoints_Total_From_Level
    Int = 0x00000000,
    Float = 3.051758E-05f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x004D], // Hitpoints_Cur
    Int = 0x00000000,
    Float = 14.00781f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0043], // TeamID
    Int = 0x00000001,
    Float = 0f,
    },
    new NetAttributeKeyValue()
    {
    Attribute = GameAttribute.Attributes[0x0026], // Level
    Int = 0x00000004,
    Float = 0f,
    },
    },
            });

            world.BroadcastGlobal(new ACDGroupMessage()
            {
                Id = 0x00B8,
                ActorID = aID,
                Field1 = -1,
                Field2 = -1,
            });

            world.BroadcastGlobal(new ANNDataMessage(Opcodes.ANNDataMessage7)
            {
                Id = 0x003E,
                ActorID = aID,
            });

            world.BroadcastGlobal(new ACDTranslateFacingMessage()
            {
                Id = 0x0070,
                ActorID = aID,
                Angle = 2.888546f,
                Field2 = false,
            });

            world.BroadcastGlobal(new SetIdleAnimationMessage()
            {
                Id = 0x00A5,
                ActorID = aID,
                AnimationSNO = 0x00011150,
            });

            world.BroadcastGlobal(new SNONameDataMessage()
            {
                Id = 0x00D3,
                Name = new SNOName()
                {
                    Group = 0x00000001,
                    Handle = 4644,
                },
            });
            world.BroadcastGlobal(new TrickleMessage()
            {
                Id = 0x0042,
                ActorID = aID,
                Field1 = 4644,
                Field2 = new WorldPlace()
                {
                    Position = new Vector3D()
                    {
                        X = 2932.286f,
                        Y = 2796.864f,
                        Z = 23.94531f
                    },
                    WorldID = world.DynamicID,
                },
                Field4 = 0x00004DEB,
                Field5 = 1f,
                Field6 = 0x00000008,
                Field7 = 0x00000024,
                Field10 = 0x0AF96544,
                Field12 = 0x0000F063,
            });
            #endregion
        }

    }
}
