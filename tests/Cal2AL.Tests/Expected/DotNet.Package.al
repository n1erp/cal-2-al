dotnet
{
    assembly('System')
    {
        Version = '4.0.0.0';
        Culture = 'neutral';
        PublicKeyToken = 'b77a5c561934e089';

        type('System.Timers.ElapsedEventArgs'; ElapsedEventArgs)
        {
        }
        type('System.Timers.Timer'; Timer)
        {
        }
    }
}
