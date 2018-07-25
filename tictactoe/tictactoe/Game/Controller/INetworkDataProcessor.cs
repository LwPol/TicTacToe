namespace TicTacToe.Game.Controller
{
    /// <summary>
    /// Provides mechanism for processing received input and assembling output for communication via internet.
    /// </summary>
    public interface INetworkDataProcessor
    {
        /// <summary>
        /// Processes received data.
        /// </summary>
        /// <param name="code">Operation code.</param>
        /// <param name="body">Additional information about operation.</param>
        void Process(string code, string body);

        /// <summary>
        /// Assembles data to be sent by network.
        /// </summary>
        /// <param name="code">Operation code.</param>
        /// <param name="data">Additional information about operation.</param>
        /// <returns>Text representation of data to be sent.</returns>
        string Assemble(string code, object data);
    }
}
