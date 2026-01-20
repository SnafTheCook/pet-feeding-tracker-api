namespace DidWeFeedTheCatToday.Common
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public ServiceResultError? Error { get; set; }

        public static ServiceResult Ok() => new ServiceResult { Success = true };
        public static ServiceResult Fail(ServiceResultError error) => new ServiceResult { Success = false, Error = error };
    }

    public enum ServiceResultError 
    { 
        NotFound,
        ConcurrencyConflict
    }
}
