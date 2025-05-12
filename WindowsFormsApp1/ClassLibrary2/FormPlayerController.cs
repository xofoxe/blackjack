using ClassLibrary1;
using ClassLibrary2;
using System.Threading;
using System;

public class FormPlayerController : IDisposable
{
    private PlayerAction _nextAction;
    private ManualResetEvent _waitHandle = new ManualResetEvent(false);

    public void SetAction(PlayerAction action)
    {
        _nextAction = action;
        _waitHandle.Set();
    }

    public PlayerAction GetNextAction(Player player, Player dealer)
    {
        _waitHandle.Reset();
        _waitHandle.WaitOne();
        return _nextAction;
    }

    public void Dispose()
    {
        _waitHandle.Dispose();
    }
}
