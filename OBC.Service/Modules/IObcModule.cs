namespace OBC.Service.Modules;

internal interface IObcModule
{
    void Start();
    void Stop();
    void Sleep();
    void Wake();
}
