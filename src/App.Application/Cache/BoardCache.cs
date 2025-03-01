using System.Collections.Concurrent;

namespace App.Application.Cache
{
    public static class BoardCache
    {
        private static readonly ConcurrentDictionary<string, BoardData> _boards = new ConcurrentDictionary<string, BoardData>();

        public static void AddOrUpdate(string boardName, string boardId, ConcurrentDictionary<string, string> lists = null)
        {
            var boardData = new BoardData
            {
                BoardId = boardId,
                Lists = lists ?? new ConcurrentDictionary<string, string>()
            };
            _boards.AddOrUpdate(boardName, boardData, (key, oldValue) => boardData);
        }

        public static bool TryGetBoardData(string boardName, out BoardData boardData)
        {
            return _boards.TryGetValue(boardName, out boardData);
        }

        public static bool Remove(string boardName)
        {
            return _boards.TryRemove(boardName, out _);
        }
    }

    public class BoardData
    {
        public string BoardId { get; set; }
        public ConcurrentDictionary<string, string> Lists { get; set; } = new ConcurrentDictionary<string, string>();
    }

}
