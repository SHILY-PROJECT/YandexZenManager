namespace Yandex.Zen.Core.Toolkit.Extensions.Enums
{
    public enum LineOptions
    {
        /// <summary>
        /// Случайная строка без удаления.
        /// </summary>
        Random = 0,
        /// <summary>
        /// Случайная строка с удалением.
        /// </summary>
        RandomWithRemoved = 1,
        /// <summary>
        /// Первая строка без удаления.
        /// </summary>
        First = 2,
        /// <summary>
        /// Первая строка с удалением.
        /// </summary>
        FirstWithRemoved = 3,
        /// <summary>
        /// Первая строка с переносом её в конец списка.
        /// </summary>
        FirstWithMoveToEnd = 4
    }
}
