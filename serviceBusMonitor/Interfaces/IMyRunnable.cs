using ConsoleApp5;

namespace serviceBusMonitor.Interfaces
{
    interface IMyRunnable
    {
        void Run();
        void Stop();

        event MyEventHandler OnMessage;

        bool IsRunning { get; }

    }
}
