using FlowBlox.Core.Models.FlowBlocks.Base;

namespace FlowBlox.Grid.Views.Interfaces
{
    /// <summary>
    /// Ihre Ansicht zur Anzeige und Bearbeitung Ihres Grid-Elements muss dieses Interface implementieren.
    /// </summary>
    public interface IBaseFlowBlockView
    {
        /// <summary>
        /// Methode zum Aktualisieren der UI aufgrund der übergebenen Metainformationen. Das ReadOnly Flag sollte hier ausgewertet werden.
        /// </summary>
        void UpdateUI();

        /// <summary>
        /// Diese Methode muss implementiert werden, um auf die ReadOnly Notification der WebFlowIDE.Runtime zu reagieren.
        /// </summary>
        /// <param name="readOnly"></param>
        void SetReadOnly(bool readOnly);

        /// <summary>
        /// In dieser Methode wird das übergebene Grid-Element initialisiert. Wurde es erstellt, hat es den <see cref="BaseFlowBlock.Status"/>: Created.
        /// </summary>
        /// <param name="gridElement"></param>
        void Initialize(BaseFlowBlock gridElement);
    }
}
