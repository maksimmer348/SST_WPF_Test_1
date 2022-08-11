namespace SST_WPF_Test_1;

/// <summary>
/// Тип текущего теста
/// </summary>
public enum TypeOfTestRun
{
    None,
    Stop,
    PrimaryCheckDevices,
    PrimaryCheckDevicesReady,
    PrimaryCheckVips,
    PrimaryCheckVipsReady,
    DeviceOperation,
    DeviceOperationReady,
    MeasurementZero,
    MeasurementZeroReady,
    WaitSettingToOperatingMode,
    WaitSettingToOperatingModeReady,
    CyclicMeasurement,
    CyclicMeasurementReady,
    CycleWait,
    Error
}