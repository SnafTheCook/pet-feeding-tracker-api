namespace DidWeFeedTheCatToday.Client.Services
{
    public class ToastService
    {
        public string Message { get; private set; } = string.Empty;
        public string CssClass { get; private set; } = "bg-success";
        public bool IsInvisible { get; private set; }

        public event Action? OnShow;
        public event Action? OnHide;

        public void ShowSuccess(string message) => ShowToast(message, "bg-success");
        public void ShowError(string message) => ShowToast(message, "bg-danger");

        private void ShowToast(string message, string cssClass)
        {
            Message = message;
            CssClass = cssClass;
            IsInvisible = true;
            OnShow?.Invoke();

            Task.Delay(3000).ContinueWith(_ => HideToast());
        }

        private void HideToast()
        {
            IsInvisible = false;
            OnHide?.Invoke();
        }
    }
}
