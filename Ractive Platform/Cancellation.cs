namespace RactivePlatform
{
    public class Cancellation
    {
        private enum CancellationState
        {
            None = 0,
            CancellationRequested = 1
        }

        private CancellationState _state;

        public bool IsCancellationRequested => _state == CancellationState.CancellationRequested;

        public void RequestCancellation()
        {
            _state = CancellationState.CancellationRequested;
        }
    }
}