using System.Collections.Generic;
namespace Core.Framework.Nav.AStar
{
    interface IPathFinder
    {
        event PathFinderDebugHandler PathFinderDebug;
        bool Stopped
        {
            get;
        }
        HeuristicFormula Formula
        {
            get;
            set;
        }
        bool Diagonals
        {
            get;
            set;
        }
        bool HeavyDiagonals
        {
            get;
            set;
        }
        int HeuristicEstimate
        {
            get;
            set;
        }
        bool PunishChangeDirection
        {
            get;
            set;
        }
        bool ReopenCloseNodes
        {
            get;
            set;
        }
        bool TieBreaker
        {
            get;
            set;
        }
        int SearchLimit
        {
            get;
            set;
        }
        double CompletedTime
        {
            get;
            set;
        }
        bool DebugProgress
        {
            get;
            set;
        }
        bool DebugFoundPath
        {
            get;
            set;
        }
        void FindPathStop();
        List<PathFinderNode> FindPath(Point2D start, Point2D end, int nSearchLimit = 0);
    }
}
