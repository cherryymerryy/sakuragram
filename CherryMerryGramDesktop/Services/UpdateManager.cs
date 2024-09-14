namespace CherryMerryGramDesktop.Services;

public class UpdateManager
{
    private bool _bCanCheckForUpdates = true;
    private bool _bUpdateAvailable = false;
    private bool _bUpdateDownloaded = false;
    private bool _bUpdateDownloading = false;
    
    public void CheckForUpdates()
    {
        while (_bCanCheckForUpdates)
        {
            // TODO: check for update
        }
    }
    
    public void DownloadUpdate() { }
    
    public void Update() { }
    
    public void Restart() { }
    
    public void Shutdown() { }
}