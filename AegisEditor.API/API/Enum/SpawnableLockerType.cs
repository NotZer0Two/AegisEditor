#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AegisEditor.API.Attributes;

namespace AegisEditor.API.Enum;

public enum SpawnableLockerType
{
    [AssetsName("RegularMedkitStructure")]
    RegularMedkit,
    
    [AssetsName("AdrenalineMedkitStructure")]
    AdrenalineMedkit,
    
    [AssetsName("LargeGunLockerStructure")]
    LargeGunLocker,
    
    [AssetsName("Experimental Weapon Locker")]
    ExperimentalLocker,
    
    [AssetsName("RifleRackStructure")]
    RifleRack,
    
    [AssetsName("MiscLocker")]
    MiscLocker,
    
    [AssetsName("Scp018PedestalStructure Variant")]
    Scp018Pedestal,
    
    [AssetsName("Scp207PedestalStructure Variant")]
    Scp207Pedestal,
    
    [AssetsName("Scp244PedestalStructure Variant")]
    Scp244Pedestal,
    
    [AssetsName("Scp268PedestalStructure Variant")]
    Scp268Pedestal,
    
    [AssetsName("Scp500PedestalStructure Variant")]
    Scp500Pedestal,
    
    [AssetsName("Scp1853PedestalStructure Variant")]
    Scp1853Pedestal,
    
    [AssetsName("Scp2176PedestalStructure Variant")]
    Scp2176Pedestal,
    
    [AssetsName("Scp1576PedestalStructure Variant")]
    Scp1576Pedestal,
    
    [AssetsName("AntiScp207PedestalStructure Variant")]
    AntiScp207Pedestal,
    
    [AssetsName("Scp1344PedestalStructure Variant")]
    Scp1344Pedestal,
}