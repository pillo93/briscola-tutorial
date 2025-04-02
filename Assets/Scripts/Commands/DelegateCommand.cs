using UnityEngine.Events;

public class DelegateCommand : Command
{

    UnityAction callback;

    public DelegateCommand(UnityAction callback)
    {
        this.callback = callback;
    }

    protected override void StartCommandExecution()
    {
        callback.Invoke();
        CommandExecutionComplete();
    }

}
