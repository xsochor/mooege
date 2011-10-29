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

namespace Mooege.Net.GS.Message
{
    public enum Opcodes : int
    {
        TryConsoleCommand1 = 1,
        TryConsoleCommand2 = 2,
        QuitGameMessage = 3, // len: 	12
        CreateBNetGameMessage = 4, // len: 	88
        CreateBNetGameResultMessage = 5, // len: 	40
        DWordDataMessage1 = 6, // len: 	12
        RequestJoinBNetGameMessage = 8, // len: 	56
        BNetJoinGameRequestResultMessage = 9, // len: 	72
        JoinBNetGameMessage = 10, // len: 	80
        JoinLANGameMessage = 11, // len: 	196
        VersionsMessage = 13, // len: 	48
        GenericBlobMessage1 = 14, // len: 	
        NetworkAddressMessage = 15, // len: 	16
        GameIdMessage = 17, // len: 	32
        UInt64DataMessage = 18, // len: 	16
        IntDataMessage1 = 20, // len: 	12
        EntityIdMessage = 22, // len: 	24
        CreateHeroMessage = 23, // len: 	68
        CreateHeroResultMessage = 24, // len: 	32
        SimpleMessage1 = 25, // len: 	8
        BlizzconCVarsMessage = 26, // len: 	20
        SimpleMessage2 = 27, // len: 	8
        GenericBlobMessage2 = 28, // len: 	
        GenericBlobMessage3 = 29, // len: 	
        GenericBlobMessage4 = 30, // len: 	
        GenericBlobMessage5 = 31, // len: 	
        ANNDataMessage1 = 32, // len: 	12 // send OUT to open trade window
        SimpleMessage3 = 33, // len: 	8
        OpenTradeWindow = 34, // len: 	12          former ANNDataMessage2 // send OUT to CLOSE trade window
        RequestBuyItemMessage = 35, // len: 	12, former ANNDataMessage3
        RequestSellItemMessage = 36, // len: 	12, former ANNDataMessage4
        RequestUseCauldronOfJordanMessage = 37, // len: 	12  former ANNDataMessage5
        LogoutContextMessage1 = 38, // len: 	16
        LogoutTickTimeMessage = 39, // len: 	20
        LogoutComplete = 40, // len: 	8
        LogoutContextMessage2 = 41, // len: 	16
        PlayerIndexMessage1 = 42, // len: 	12
        PlayerIndexMessage2 = 43, // len: 	12
        SimpleMessage5 = 44, // len: 	8
        SimpleMessage6 = 45, // len: 	8
        ConnectionEstablishedMessage = 46, // len: 	20
        GameSetupMessage = 47, // len: 	20
        SimpleMessage7 = 48, // len: 	8
        NewPlayerMessage = 49, // len: 	16916
        GenericBlobMessage6 = 50, // len: 	
        HeroStateData = 50, // len: 	
        EnterWorldMessage = 51, // len: 	28
        RevealSceneMessage = 52, // len: 	1292
        DestroySceneMessage = 53, // len: 	16
        SwapSceneMessage = 54, // len: 	20
        RevealWorldMessage = 55, // len: 	16
        RevealTeamMessage = 56, // len: 	20
        PlayerActorSetInitialMessage = 57, // len: 	16
        HeroStateMessage = 58, // len: 	16652
        ACDEnterKnownMessage = 59, // len: 	132
        ACDDestroyActorMessage = 60, // len: 	12
        PlayerEnterKnownMessage = 61, // len: 	16
        ACDCreateActorMessage = 62, // len: 	12
        ACDWorldPositionMessage = 63, // len: 	48
        ACDInventoryPositionMessage = 64, // len: 	32
        ACDInventoryUpdateActorSNO = 65, // len: 	16
        TrickleMessage = 66, // len: 	116
        ANNDataMessage8 = 67, // len: 	12 // nothing when sent OUT on player's ID (solo)
        MapRevealSceneMessage = 68, // len: 	52
        SavePointInfoMessage = 69, // len: 	12
        HearthPortalInfoMessage = 70, // len: 	16
        ReturnPointInfoMessage = 71, // len: 	12
        AffixMessage = 72, // len: 	148
        RareMonsterNamesMessage = 73, // len: 	52
        RareItemNameMessage = 74, // len: 	28
        PortalSpecifierMessage = 75, // len: 	24
        AttributeSetValueMessage = 76, // len: 	28
        AttributesSetValuesMessage = 77, // len: 	256
        VisualInventoryMessage = 78, // len: 	140
        ProjectileStickMessage = 79, // len: 	28
        TargetMessage = 80, // len: 	60
        SecondaryAnimationPowerMessage = 81, // len: 	28
        SNODataMessage1 = 82, // len: 	12 // sent IN when used Stone of Recall: Field0 = 0x0002EC66 (PowerSNO)
        DWordDataMessage2 = 83, // len: 	12
        DWordDataMessage3 = 84, // len: 	12 // sent IN when stopped using stationary power (e.g. disintigrate, firebats) Field0 = PowerSNO
        DWordDataMessage4 = 85, // len: 	12
        DWordDataMessage5 = 86, // len: 	12 // sent IN when stopped using moving power (e.g. tempest rush) Field0 = PowerSNO
        TryChatMessage = 87, // len: 	528
        ChatMessage = 88, // len: 	528
        ANNDataMessage9 = 89, // len: 	12 // crashed client when sent OUT on player's ID
        InventoryRequestMoveMessage1 = 90, // len: 	28
        InventoryRequestSocketMessage = 91, // len: 	16
        InventoryRequestMoveMessage2 = 92, // len: 	28
        InventorySplitStackMessage = 93, // len: 	40
        InventoryStackTransferMessage = 94, // len: 	24
        ANNDataMessage10 = 95, // len: 	12 // nothing when sent OUT on player's ID (solo)
        ANNDataMessage11 = 96, // len: 	12 // crashed client when sent OUT on player's ID
        InventoryRequestUseMessage = 97, // len: 	36
        SocketSpellMessage = 98, // len: 	16
        HelperDetachMessage = 99, // len: 	12
        AssignSkillMessage1 = 100, // len: 	16
        AssignSkillMessage2 = 101, // len: 	16
        AssignSkillMessage3 = 102, // len: 	16
        AssignSkillMessage4 = 103, // len: 	16
        HirelingRequestLearnSkillMessage = 104, // len: 	20
        ANNDataMessage12 = 105, // len: 	// crashed client when sent OUT on player's ID
        HotbarButtonData = 105, // len: 	12
        PlayerChangeHotbarButtonMessage = 106, // len: 	20
        IntDataMessage2 = 107, // len: 	
        PlayAnimationMessageSpec = 107, // len: 	12
        PlayAnimationMessage = 108, // len: 	72
        ANNDataMessage13 = 109, // len: 	12 // nothing when sent OUT on player's ID (solo)
        NotifyActorMovementMessage = 110, // len: 	76
        ACDTranslateSnappedMessage = 111, // len: 	36
        ACDTranslateFacingMessage1 = 112, // len: 	20
        ACDTranslateFixedMessage = 113, // len: 	36
        ACDTranslateArcMessage = 114, // len: 	60
        ACDTranslateDetPathMessage = 115, // len: 	88
        ACDTranslateDetPathSinMessage = 116, // len: 	104
        ACDTranslateDetPathSpiralMessage = 117, // len: 	72
        ACDTranslateSyncMessage = 118, // len: 	32
        ACDTranslateFixedUpdateMessage = 119, // len: 	36
        PlayerMovementMessage = 120, // len: 	76
        ACDTranslateFacingMessage2 = 121, // len: 	20
        PlayEffectMessage = 122, // len: 	24
        PlayHitEffectMessage = 123, // len: 	24
        PlayHitEffectOverrideMessage = 124, // len: 	20
        PlayNonPositionalSoundMessage = 125, // len: 	12
        PlayErrorSoundMessage = 126, // len: 	12
        PlayMusicMessage = 127, // len: 	12
        PlayCutsceneMessage = 128, // len: 	12
        ComplexEffectAddMessage = 129, // len: 	36
        FlippyMessage = 130, // len: 	32
        WaypointActivatedMessage = 131, // len: 	20
        OpenWaypointSelectionWindowMessage = 132, // len: 	12
        ANNDataMessage15 = 133, // len: 	12 // send OUT Warning with OK - Cancel buttons
        ANNDataMessage16 = 134, // len: 	12 // sent IN after clicking OK from 133 
        AimTargetMessage = 135, // len: 	36
        ACDChangeGBHandleMessage = 136, // len: 	20
        GameTickMessage = 137, // len: 	12
        LearnedSkillMessage = 138, // len: 	524
        DataIDDataMessage1 = 139, // len: 	12
        DataIDDataMessage2 = 140, // len: 	12
        EndOfTickMessage = 141, // len: 	16
        TryWaypointMessage = 142, // len: 	16
        NPCInteractOptionsMessage = 143, // len: 	340
        ANNDataMessage17 = 144, // len: 	12 // crashed client when sent OUT on player's ID
        ANNDataMessage18 = 145, // len: 	12 // crashed client when sent OUT on player's ID
        SimpleMessage8 = 146, // len: 	8
        QuestUpdateMessage = 147, // len: 	28
        QuestMeterMessage = 148, // len: 	20
        QuestCounterMessage = 149, // len: 	20
        GenericBlobMessage7 = 150, // len: 	
        PlayerInteractMessage = 151, // len: 	16
        PlayerLevel = 152, // len: 	16
        OpenSharedStashMessage = 153, // len: 	12, former ANNDataMessage19
        ACDPickupFailedMessage = 154, // len: 	16
        PetMessage = 155, // len: 	24
        ANNDataMessage20 = 156, // len: 	12 // nothing when sent OUT on player's ID (solo)
        HirelingInfoUpdateMessage = 157, // len: 	24
        UIElementMessage = 158, // len: 	16
        PlayerBusyMessage = 159, // len: 	12      //  former: BoolDataMessage
        TradeMessage1 = 160, // len: 	56
        TradeMessage2 = 161, // len: 	56
        PlayerIndexMessage3 = 162, // len: 	12
        SimpleMessage9 = 163, // len: 	8
        PlayerIndexMessage4 = 164, // len: 	12
        SetIdleAnimationMessage = 165, // len: 	16
        ACDCollFlagsMessage = 166, // len: 	16
        GoldModifiedMessage = 167, // len: 	12
        ActTransitionMessage = 168, // len: 	16
        InterstitialMessage = 169, // len: 	16
        EffectGroupACDToACDMessage = 170, // len: 	20
        RopeEffectMessageACDToACD = 171, // len: 	28
        RopeEffectMessageACDToPlace = 172, // len: 	36
        ANNDataMessage21 = 173, // len: 	12 // nothing when sent OUT on player's ID (solo)
        ANNDataMessage22 = 174, // len: 	12 // nothing when sent OUT on player's ID (solo)
        GameSyncedDataMessage = 175, // len: 	96
        ACDChangeActorMessage = 176, // len: 	16
        PlayerWarpedMessage = 177, // len: 	16
        VictimMessage = 178, // len: 	48
        KillCountMessage = 179, // len: 	24
        WorldStatusMessage = 180, // len: 	16
        WeatherOverrideMessage = 181, // len: 	16
        SimpleMessage10 = 182, // len: 	8
        ACDShearMessage = 183, // len: 	16
        ACDGroupMessage = 184, // len: 	20
        //PlayLineParams = 185, // len: 	
        SimpleMessage11 = 185, // len: 	8
        PlayConvLineMessage = 186, // len: 	168
        StopConvLineMessage = 187, // len: 	16
        IntDataMessage3 = 188, // len: 	12
        RequestCloseConversationWindowMessage = 189, // len: 	8
        EndConversationMessage = 190, // len: 	20
        SNODataMessage2 = 191, // len: 	12
        FinishConversationMessage = 192, // len: 	12
        HirelingSwapMessage = 193, // len: 	12
        SimpleMessage13 = 194, // len: 	8
        DeathFadeTimeMessage = 195, // len: 	24
        ANNDataMessage23 = 196, // len: 	12 // nothing when sent OUT on player's ID (solo)
        ANNDataMessage24 = 197, // len: 	12 // nothing when sent OUT on player's ID (solo)
        DisplayGameTextMessage = 198, // len: 	536
        IntDataMessage4 = 199, // len: 	12
        DWordDataMessage7 = 200, // len: 	12
        GBIDDataMessage1 = 201, // len: 	12
        ANNDataMessage25 = 202, // len: 	12 // nothing when sent OUT on player's ID (solo)
        ANNDataMessage26 = 203, // len: 	12 // nothing when sent OUT on player's ID (solo)
        ACDLookAtMessage = 204, // len: 	16
        KillCounterUpdateMessage = 205, // len: 	24
        LowHealthCombatMessage = 206, // len: 	16
        SaviorMessage = 207, // len: 	16
        FloatingNumberMessage = 208, // len: 	20
        FloatingAmountMessage = 209, // len: 	40
        RemoveRagdollMessage = 210, // len: 	16
        SNONameDataMessage = 211, // len: 	16
        LoreMessage1 = 212, // len: 	16
        LoreMessage2 = 213, // len: 	16
        SimpleMessage14 = 216, // len: 	8
        WorldDeletedMessage = 217, // len: 	12
        SimpleMessage15 = 218, // len: 	8
        IntDataMessage5 = 219, // len: 	12
        TimedEventStartedMessage = 220, // len: 	20
        SNODataMessage4 = 221, // len: 	12
        ActTransitionStartedMessage = 222, // len: 	16
        SimpleMessage16 = 223, // len: 	8
        RequestBuySharedStashSlotsMessage = 224, // len: 	8   former SimpleMessage17
        PlayerQuestMessage1 = 225, // len: 	16
        PlayerQuestMessage2 = 226, // len: 	16
        PlayerDeSyncSnapMessage = 227, // len: 	28
        RequestUseNephalemCubeMessage = 228, // len: 	12              former ANNDataMessage27
        SalvageResultsMessage = 229, // len: 	60
        SimpleMessage18 = 230, // len: 	8
        ChatMessage2 = 231, // len: 	528
        SimpleMessage19 = 232, // len: 	8
        MapMarkerInfoMessage = 233, // len: 	72
        GenericBlobMessage8 = 234, // len: 	
        GenericBlobMessage9 = 235, // len: 	
        GenericBlobMessage10 = 236, // len: 	
        GenericBlobMessage11 = 237, // len: 	
        GenericBlobMessage12 = 238, // len: 	
        GenericBlobMessage13 = 239, // len: 	
        ANNDataMessage28 = 240, // len: 	12 // crashed client - unknown message received on client
        DebugActorTooltipMessage = 241, // len: 	524
        BossEncounterMessage1 = 242, // len: 	16
        SimpleMessage20 = 243, // len: 	8
        SimpleMessage21 = 244, // len: 	8
        BossEncounterMessage2 = 245, // len: 	16
        SimpleMessage22 = 246, // len: 	8
        SimpleMessage23 = 247, // len: 	8
        EncounterInviteStateMessage = 248, // len: 	12
        SimpleMessage24 = 249, // len: 	8
        SimpleMessage25 = 250, // len: 	8
        PlayerIndexMessage5 = 251, // len: 	12
        SimpleMessage26 = 252, // len: 	8
        SimpleMessage27 = 253, // len: 	8
        SimpleMessage28 = 254, // len: 	8
        SimpleMessage29 = 255, // len: 	8
        CameraFocusMessage = 256, // len: 	20
        CameraZoomMessage = 257, // len: 	20
        CameraYawMessage = 258, // len: 	20
        SimpleMessage30 = 259, // len: 	8
        BoolDataMessage2 = 260, // len: 	12
        BossZoomMessage = 261, // len: 	16
        EnchantItemMessage = 262, // len: 	16
        ANNDataMessage29 = 263, // len: 	12 // crashed client when sent OUT on player's ID
        SimpleMessage31 = 264, // len: 	8
        SimpleMessage32 = 265, // len: 	8
        ANNDataMessage30 = 266, // len: 	12 // crashed client when sent OUT on player's ID
        SimpleMessage33 = 267, // len: 	8
        IntDataMessage6 = 268, // len: 	12
        DebugDrawPrimMessage = 269, // len: 	188
        GBIDDataMessage2 = 270, // len: 	12
        CraftingResultsMessage = 271, // len: 	20
        CrafterLevelUpMessage = 272, // len: 	20
        SimpleMessage34 = 273, // len: 	8
        ANNDataMessage31 = 274, // len: 	12 // crashed client when sent OUT on player's ID
        ANNDataMessage32 = 275, // len: 	12 // crashed client - unknown message received on client
        IntDataMessage7 = 276, // len: 	12
        IntDataMessage8 = 277, // len: 	12
        SimpleMessage35 = 278, // len: 	8
        SimpleMessage36 = 279, // len: 	8
        GameTestingSamplingStartMessage = 280, // len: 	16
        SimpleMessage37 = 281, // len: 	8
        TutorialShownMessage = 282, // len: 	12
        RequestBuffCancelMessage = 283, // len: 	16
        SimpleMessage38 = 284, // len: 	8
        PlayerIndexMessage6 = 285, // len: 	12
        SimpleMessage39 = 286, // len: 	8
        SimpleMessage40 = 287, // len: 	8
        DWordDataMessage8 = 288, // len: 	12
        DWordDataMessage9 = 289, // len: 	12
        DWordDataMessage10 = 290, // len: 	12
        DWordDataMessage11 = 291, // len: 	12
        BroadcastTextMessage = 292, // len: 	520
        SimpleMessage41 = 293, // len: 	8
        SimpleMessage42 = 294, // len: 	8
        SNODataMessage6 = 295, // len: 	12
        ANNDataMessage33 = 296, // len: 	12 // crashed client - unknown message received on client
        SimpleMessage43 = 297, // len: 	8
        SimpleMessage44 = 298, // len: 	8
        SimpleMessage45 = 299, // len: 	8
        SNODataMessage7 = 300, // len:  12
        SimpleMessage46 = 301, // len: 	8
    }
}
