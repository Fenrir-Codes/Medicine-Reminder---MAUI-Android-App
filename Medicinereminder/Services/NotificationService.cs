

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

public class NotificationService
{
    public async Task DisplayAlerMessage(string title, string message, ToastDuration duration, float textsize)
    {
        var messageBox = Toast.Make(message, duration, textsize);
        await messageBox.Show();
    }

}