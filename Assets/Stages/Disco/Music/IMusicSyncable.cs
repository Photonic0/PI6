/// <summary>
/// QUANDO IMPLEMENTANDO, ADICIONE <br></br>
/// DiscoMusicEventManager.AddSyncableObject(this);
/// <br></br>
/// NO M�TODO Start()!
/// </summary>
public interface IMusicSyncable
{
    public int BeatsPerAction { get; }
    public void DoMusicSyncedAction();
}
