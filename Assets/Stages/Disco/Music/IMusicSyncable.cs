/// <summary>
/// QUANDO IMPLEMENTANDO, ADICIONE <br></br>
/// DiscoMusicEventManager.AddSyncableObject(this);
/// <br></br>
/// NO MÈTODO Start()!
/// </summary>
public interface IMusicSyncable
{
    public int BeatsPerAction { get; }
    public void DoMusicSyncedAction();
}
