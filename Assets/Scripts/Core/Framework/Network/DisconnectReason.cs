namespace Core.Framework.Network
{
    public enum DisconnectReason
    {
        None,
        User,
        Remote,
        ForGameServer,
        Network,
        TimeOut,
        KickedByOthers,
        LoginFailed,
        ServerShutdown,
        KeepAliveFail,
        Banned,
        RetryConnection,
    }
}
