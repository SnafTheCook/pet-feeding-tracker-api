namespace DidWeFeedTheCatToday.Client.Services
{
    public class ToastService
    {
        public string Message { get; private set; } = string.Empty;
        public string CssClass { get; private set; } = "bg-success";
        public bool IsVisible { get; private set; }

        public event Action? OnShow;
        public event Action? OnHide;

        public void ShowSuccess(string message) => ShowToast(message, "bg-success");
        public void ShowError(string message) => ShowToast(message, "bg-danger");

        private void ShowToast(string message, string cssClass)
        {
            Message = message;
            CssClass = cssClass;
            IsVisible = true;
            OnShow?.Invoke();

            Task.Delay(3000).ContinueWith(_ => HideToast());
        }

        private void HideToast()
        {
            IsVisible = false;
            OnHide?.Invoke();
        }
    }
}
