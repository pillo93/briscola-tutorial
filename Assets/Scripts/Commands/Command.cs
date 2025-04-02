using System.Collections.Generic;

public abstract class Command
{

    private static bool isQueuePlaying;
    private static Queue<Command> queue = new();

    public void AddToQueue()
    {
        queue.Enqueue(this);
        if (!isQueuePlaying)
        {
            PlayFirstCommand();
        }
    }

    private void PlayFirstCommand()
    {
        isQueuePlaying = true;
        var cmd = queue.Dequeue();
        cmd.StartCommandExecution();
    }

    protected abstract void StartCommandExecution();

    protected void CommandExecutionComplete()
    {
        if(queue.Count > 0)
        {
            PlayFirstCommand();
        }
        else
        {
            isQueuePlaying = false;
        }
    }

}
