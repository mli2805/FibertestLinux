namespace Fibertest.Rtu;

public enum CharonOperationResult
{
    OtdrError = -9,
    AdditionalOtauError = -2,
    MainOtauError = -1,
    LogicalError = 0,
    Ok = 1,
}