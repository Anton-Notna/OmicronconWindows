namespace OmicronWindows
{
    public interface IWindowRoot
    {
        public void ForceFront();

        public void ClearForceFront();

        public void Close();
    }
}