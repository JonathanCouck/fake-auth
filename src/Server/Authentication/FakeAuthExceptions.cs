namespace BogusStore.Server.Authentication
{
    public class FakeAuthPersonaException: Exception
    {
        public FakeAuthPersonaException(string message) : base(message) { }
        public FakeAuthPersonaException() : base() { }
    }
}
