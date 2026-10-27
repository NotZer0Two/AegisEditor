using AegisEditor.API.Attributes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace AegisEditor.API.Enum.Map;

public enum RoomType
{
    Unknown,
    
    [AssetsName("EZ_CollapsedTunnel")]
    EzCollapsedTunnel,

    [AssetsName("LCZ_TCross")]
    LczTCross,

    [AssetsName("EZ_Intercom")]
    EzIntercom,

    [AssetsName("LCZ_Toilets")]
    LczToilets,

    [AssetsName("EZ_PCs_small")]
    EzOfficeSmall,

    [AssetsName("HCZ_Curve")]
    HczCurve,

    [AssetsName("LCZ_330")]
    Lcz330,

    [AssetsName("LCZ_ClassDSpawn")]
    LczClassDSpawn,

    [AssetsName("LCZ_Airlock")]
    LczAirlock,

    [AssetsName("LCZ_Crossing")]
    LczCrossing,

    [AssetsName("HCZ_TArmory")]
    HczArmory,

    [AssetsName("LCZ_173")]
    Lcz173,

    [AssetsName("EZ_GateB")]
    EzGateB,

    [AssetsName("EZ_Cafeteria")]
    EzCafeteria,

    [AssetsName("EZ_PCs")]
    EzOfficeLarge,

    [AssetsName("EZ_Smallrooms2")]
    EzSmallrooms2,

    [AssetsName("LCZ_Armory")]
    LczArmory,

    [AssetsName("HCZ_079")]
    Hcz079,

    [AssetsName("LCZ_372")]
    LczGlassroom,

    [AssetsName("HCZ_Testroom")]
    HczTestroom,

    [AssetsName("HCZ_Intersection")]
    HczThreeWayIntersection,

    [AssetsName("PocketWorld")]
    PocketWorld,

    [AssetsName("EZ_Curve")]
    EzCurve,

    [AssetsName("HCZ_Nuke")]
    HczWarhead,

    [AssetsName("LCZ_ChkpA")]
    LczCheckpointA,

    [AssetsName("HCZ_Straight")]
    HczStraight,

    [AssetsName("LCZ_Straight")]
    LczStraight,

    [AssetsName("HCZ_EZ_Checkpoint Part")]
    HczCheckpointToEntranceZone,

    [AssetsName("LCZ_Plants")]
    LczGreenhouse,

    [AssetsName("EZ_Shelter")]
    EzEvacShelter,

    [AssetsName("EZ_upstairs")]
    EzOfficeStoried,

    [AssetsName("HCZ_106_Rework")]
    Hcz106,

    [AssetsName("EZ_Endoof")]
    EzRedroom,

    [AssetsName("HCZ_939")]
    Hcz939,

    [AssetsName("EZ_Straight")]
    EzStraight,

    [AssetsName("EZ_GateA")]
    EzGateA,

    [AssetsName("LCZ_914")]
    Lcz914,

    [AssetsName("HCZ_ChkpB")]
    HczCheckpointB,

    [AssetsName("HCZ_049")]
    Hcz049,

    [AssetsName("HCZ_MicroHID_New")]
    HczMicroHid,

    [AssetsName("Outside")]
    Surface,

    [AssetsName("HCZ_Tesla_Rework")]
    HczTesla,

    [AssetsName("HCZ_096")]
    Hcz096,

    [AssetsName("LCZ_ChkpB")]
    LczCheckpointB,

    [AssetsName("HCZ_ServerRoom")]
    HczServers,

    [AssetsName("HCZ_Crossing")]
    HczCrossing,

    [AssetsName("EZ_ThreeWay")]
    EzThreeWay,

    [AssetsName("HCZ_ChkpA")]
    HczCheckpointA,

    [AssetsName("EZ_Crossing")]
    EzCrossing,

    [AssetsName("EZ_HCZ_Checkpoint Part")]
    EzCheckpointToHeavyContainment,

    [AssetsName("EZ_Smallrooms1")]
    EzSmallrooms1,

    [AssetsName("HCZ_Corner_Deep")]
    HczCornerDeep,

    [AssetsName("HCZ_Crossroom_Water")]
    HczAcroamaticAbatement,

    [AssetsName("HCZ_Intersection_Junk")]
    HczThreeWayIntersectionJunk,

    [AssetsName("HCZ_Straight_C")]
    HczStraightC,

    [AssetsName("HCZ_Straight_PipeRoom")]
    HczStraightPipeRoom,

    [AssetsName("LCZ_Curve")]
    LczCurve,

    [AssetsName("LCZ_Cafe")]
    LczComputerRoom,

    [AssetsName("EZ_Chef")]
    EzChef,

    [AssetsName("EZ_StraightColumn")]
    EzStraightColumn,

    [AssetsName("HCZ_127")]
    Hcz127,

    [AssetsName("HCZ_IncineratorWayside")]
    HczWaysideIncinerator,

    [AssetsName("HCZ_Intersection_Ramp")]
    HczRampTunnel
}