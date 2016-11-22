namespace PoGo.NecroBot.FORM.WebSocketHandler
{
    internal interface IWebSocketResponce
    {
        string RequestID { get; }
        string Command { get; }
        dynamic Data { get; }
    }
}