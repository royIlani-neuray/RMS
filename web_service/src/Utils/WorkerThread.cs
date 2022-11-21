
namespace WebService.Utils;

public abstract class WorkerThread<T> 
{
    private Queue<T> WorkQueue;
    private Task? WorkerTask;
    private bool RunWorker;
    private AutoResetEvent RunWorkerSignal;
    private int MaxCapacity;
    private object QueueLock;

    public WorkerThread(int maxCapacity)
    {
        WorkQueue = new Queue<T>();
        RunWorker = false;
        RunWorkerSignal = new AutoResetEvent(true);
        MaxCapacity = maxCapacity;
        QueueLock = new Object();
    }

    protected void Enqueue(T workItem)
    {
        lock (QueueLock)
        {
            if (WorkQueue.Count == MaxCapacity)
            {
                // remove the oldest item from the queue in favor of the new one.
                WorkQueue.Dequeue();
            }

            WorkQueue.Enqueue(workItem);
            RunWorkerSignal.Set();            
        }
    }

    private bool TryDequeue(out T workItem)
    {
        lock (QueueLock)
        {
            return WorkQueue.TryDequeue(out workItem!);        
        }        
    }

    public void StartWorker()
    {
        if (WorkerTask != null)
            return;

        WorkerTask = new Task(() => 
        {
            WorkerMainLoop();
        });

        RunWorker = true;
        RunWorkerSignal.Set();
        WorkerTask.Start();
    }

    public void StopWorker()
    {
        if (WorkerTask == null)
            return;
        
        RunWorker = false;
        RunWorkerSignal.Set();
        WorkerTask.Wait();
        WorkerTask = null;
    }

    protected abstract void DoWork(T workItem);

    private void WorkerMainLoop()
    {
        while (RunWorker)
        {
            T workItem;

            if (TryDequeue(out workItem!))
            {
                try
                {
                    DoWork(workItem); 
                }
                catch
                {
                    System.Console.WriteLine("Unexpected exception in worker thread!");
                } 
            }
            else
            {
                // wait for signal that either the quere has content or we need to exit
                RunWorkerSignal.WaitOne();
            }                
        }
    }
}
