namespace Klik.Services;

public interface IKeyboardService
{
    void SendTextAndKey(string text, int virtualKeyCode);
}
