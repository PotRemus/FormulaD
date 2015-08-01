using UnityEngine;
using System.Collections;
using FormuleD.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System;
using FormuleD.Managers;
using FormuleD.Models.Board;
using FormuleD.Models.Contexts;
using FormuleD.Managers.Course.Board;
using FormuleD.Managers.Course;

namespace FormuleD.Engines
{
    public class BoardEngine : MonoBehaviour
    {
        public BoardManager boardManager;
        public CameraManager cameraManager;

        private string boardPath = @"Assets\Resources\Maps";
        private List<BoardDataSource> _boards;

        public static BoardEngine Instance = null;
        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Multiple instances of BoardEngine!");
            }
            Instance = this;
        }

        public void LoadBoard(string name)
        {
            if (_boards == null)
            {
                this.LoadBoardDataSource();
            }

            var boardDataSource = _boards.FirstOrDefault(m => m.name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            boardManager.InitBoard(boardDataSource);
            cameraManager.bounds = boardManager.GetBounds();
        }

        public CaseManager GetNextCase(CaseManager current)
        {
            CaseManager result = null;
            if (current != null && boardManager != null)
            {
                var nextIndex = current.itemDataSource.targets.FirstOrDefault(t => t.enable && t.column == current.itemDataSource.index.column);
                if (nextIndex == null)
                {
                    nextIndex = current.itemDataSource.targets.FirstOrDefault(t => t.enable);
                }
                result = boardManager.FindCaseManager(nextIndex);
            }
            return result;
        }

        public CaseManager GetCase(IndexDataSource index)
        {
            CaseManager result = null;
            if (index != null && boardManager != null)
            {
                result = boardManager.FindCaseManager(index);
            }
            return result;
        }

        public List<CaseManager> GetStartCase()
        {
            List<CaseManager> result = new List<CaseManager>();
            if (boardManager != null)
            {
                foreach (var startIndex in boardManager.boardDataSource.starts)
                {
                    var startCase = boardManager.FindCaseManager(startIndex);
                    if (startCase != null)
                    {
                        result.Add(startCase);
                    }
                }
            }
            return result;
        }

        public bool ContainsFinishCase(IEnumerable<IndexDataSource> route)
        {
            bool result = false;
            foreach (var itemRoute in route.Skip(1))
            {
                foreach (var finishCase in boardManager.firstIndex)
                {
                    if (finishCase.Equals(itemRoute))
                    {
                        result = true;
                        break;
                    }
                }
                if (result)
                {
                    break;
                }
            }

            if (!result)
            {
                var firstIndex = route.First();
                var firstCase = boardManager.FindCaseManager(firstIndex);
                if (firstCase.standDataSource != null)
                {
                    var lastIndex = route.Last();
                    var lastCase = boardManager.FindCaseManager(lastIndex);
                    if (lastCase.standDataSource == null)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        public SearchRouteResult FindRoutes(PlayerContext player, int min, int max)
        {
            SearchRouteResult result = new SearchRouteResult();
            var context = new SearchRouteContext();
            context.bendName = player.lastBend;
            context.bendStop = player.stopBend;
            context.min = min;
            context.max = max;
            context.tire = player.features.tire;
            context.playerIndex = player.index;
            context.isLastLap = ContextEngine.Instance.gameContext.totalLap - player.lap <= 1;
            List<CaseManager> baseRoute = new List<CaseManager>();
            baseRoute.Add(boardManager.FindCaseManager(player.GetLastIndex()));
            this.SearchRoutes(result, context, baseRoute, 0, 0, 0, 0);

            return result;
        }

        private void SearchRoutes(SearchRouteResult result, SearchRouteContext context, List<CaseManager> route, int rowMove, int exceeding, int outOfBend, int nbLineMove)
        {
            var currentMove = route.Count - 1;
            var currentCase = route.Last();
            if (context.min <= currentMove && currentMove <= context.max)
            {
                result.AddRoute(route.ToList(), outOfBend, currentCase.standDataSource != null, false, nbLineMove);
            }

            if (currentMove < context.max)
            {
                if (currentCase.standDataSource != null)
                {
                    this.ComputeStandWay(result, context, route, currentCase, currentMove, outOfBend, nbLineMove, rowMove, exceeding);
                }
                else
                {
                    var isNewOutOfBend = false;
                    var nextTarget = currentCase.itemDataSource.targets.FirstOrDefault(t => t.enable && t.column == currentCase.itemDataSource.index.column);
                    var nextcase = boardManager.FindCaseManager(nextTarget);
                    if (currentCase.bendDataSource != null && nextcase.bendDataSource == null)
                    {
                        var stop = 0;
                        if (currentCase.bendDataSource.name == context.bendName)
                        {
                            stop = context.bendStop;
                        }
                        if (currentCase.bendDataSource.stop > stop)
                        {
                            isNewOutOfBend = true;
                        }
                    }
                    if (outOfBend > 0 || isNewOutOfBend)
                    {
                        this.ComputeOutOfBendWay(result, context, route, currentCase, nextcase, currentMove, outOfBend, nbLineMove);
                    }
                    else if (currentCase.bendDataSource != null)
                    {
                        this.ComputeBendWay(result, context, route, currentCase, nbLineMove, currentMove);
                    }
                    else
                    {
                        this.ComputeLineWay(result, context, route, currentCase, rowMove, exceeding, nbLineMove, currentMove);
                    }
                }
            }
        }

        private void ComputeStandWay(SearchRouteResult result, SearchRouteContext context, List<CaseManager> route, CaseManager currentCase, int currentMove, int outOfBend, int nbLineMove, int rowMove, int exceeding)
        {
            var targets = currentCase.itemDataSource.targets.Select(t => boardManager.FindCaseManager(t)).ToList();
            var playerTarget = targets.FirstOrDefault(t => t.standDataSource != null && t.standDataSource.playerIndex.HasValue && t.standDataSource.playerIndex.Value == context.playerIndex);
            CaseManager targetCase;
            if (playerTarget != null)
            {
                targetCase = playerTarget;
            }
            else
            {
                targetCase = targets.FirstOrDefault();
            }

            if (targetCase.hasPlayer && currentMove < context.min)
            {
                result.AddRoute(route.ToList(), outOfBend, true, false, nbLineMove);
            }
            else if (!targetCase.hasPlayer && targetCase == playerTarget)
            {
                var newRoute = route.ToList();
                newRoute.Add(targetCase);
                result.AddRoute(newRoute, outOfBend, true, false, nbLineMove);
            }
            else if (!targetCase.hasPlayer)
            {
                var newRoute = route.ToList();
                newRoute.Add(targetCase);
                this.SearchRoutes(result, context, newRoute, rowMove, exceeding, outOfBend, nbLineMove);
            }
        }

        private void ComputeOutOfBendWay(SearchRouteResult result, SearchRouteContext context, List<CaseManager> route, CaseManager currentCase, CaseManager nextCase, int currentMove, int outOfBend, int nbLineMove)
        {
            if (!nextCase.hasPlayer)
            {
                if (outOfBend == 0)
                {
                    var stop = 0;
                    if (currentCase.bendDataSource.name == context.bendName)
                    {
                        stop = context.bendStop;
                    }
                    var stopDif = currentCase.bendDataSource.stop - stop;
                    if (stopDif > 1)
                    {
                        var newRoute = route.ToList();
                        newRoute.Add(nextCase);
                        result.AddRoute(newRoute, 1, false, true, nbLineMove);
                        return;
                    }
                }
                var newOutOfBend = outOfBend + 1;
                if (newOutOfBend <= context.tire)
                {
                    var newRoute = route.ToList();
                    newRoute.Add(nextCase);
                    this.SearchRoutes(result, context, newRoute, 0, 0, newOutOfBend, nbLineMove);
                }
                else
                {
                    var newRoute = route.ToList();
                    newRoute.Add(nextCase);
                    result.AddRoute(newRoute, 1, false, true, nbLineMove);
                }
            }
            else if (currentMove < context.min)
            {
                result.AddRoute(route.ToList(), outOfBend, false, true, nbLineMove);
            }
        }

        private void ComputeBendWay(SearchRouteResult result, SearchRouteContext context, List<CaseManager> route, CaseManager currentCase, int nbLineMove, int currentMove)
        {
            bool findWay = false;
            foreach (var target in currentCase.itemDataSource.targets.Where(t => t.enable))
            {
                var targetCase = boardManager.FindCaseManager(target);
                if (!targetCase.hasPlayer)
                {
                    var newRoute = route.ToList();
                    newRoute.Add(targetCase);
                    this.SearchRoutes(result, context, newRoute, 0, 0, 0, nbLineMove);
                    findWay = true;
                }
            }
            if (!findWay && currentMove < context.min)
            {
                result.AddRoute(route.ToList(), 0, false, true, nbLineMove);
            }
        }

        private void ComputeLineWay(SearchRouteResult result, SearchRouteContext context, List<CaseManager> route, CaseManager currentCase, int rowMove, int exceeding, int nbLineMove, int currentMove)
        {
            var isNewExceeding = false;
            var isEndExceeding = false;
            bool findWay = false;
            foreach (var target in currentCase.itemDataSource.targets.Where(t => t.enable))
            {
                var targetCase = boardManager.FindCaseManager(target);
                if (!targetCase.hasPlayer)
                {
                    var newExceeding = exceeding;
                    var newRowMove = rowMove;
                    if (targetCase.standDataSource != null && context.isLastLap)
                    {
                        continue;
                    }
                    else if (targetCase.standDataSource != null)
                    {
                        var newRoute = route.ToList();
                        newRoute.Add(targetCase);
                        this.SearchRoutes(result, context, newRoute, rowMove, exceeding, 0, nbLineMove);
                    }
                    else
                    {
                        var isValideWay = false;
                        var columnDif = target.column - currentCase.itemDataSource.index.column;
                        if (isNewExceeding)
                        {
                            newExceeding = columnDif * -1;
                            isValideWay = true;
                        }
                        else
                        {
                            if (columnDif == 0)
                            {
                                if (newExceeding != 0 && isEndExceeding)
                                {
                                    newExceeding = 0;
                                }
                                isValideWay = true;
                            }
                            else if (newExceeding != 0 && columnDif == newExceeding)
                            {
                                isValideWay = true;
                                newExceeding = 0;
                            }
                            else if (columnDif > 0 && rowMove >= 0 && rowMove < 2)
                            {
                                newRowMove = newRowMove + 1;
                                isValideWay = true;
                                newExceeding = 0;
                            }
                            else if (columnDif < 0 && rowMove <= 0 && rowMove > -2)
                            {
                                newRowMove = newRowMove - 1;
                                isValideWay = true;
                                newExceeding = 0;
                            }
                        }

                        if (isValideWay)
                        {
                            findWay = true;
                            var newRoute = route.ToList();
                            newRoute.Add(targetCase);
                            this.SearchRoutes(result, context, newRoute, newRowMove, newExceeding, 0, nbLineMove + 1);
                        }
                    }
                }
            }
            if (!findWay && currentMove < context.min)
            {
                result.AddRoute(route.ToList(), 0, false, true, nbLineMove);
            }
        }

        private void LoadBoardDataSource()
        {
            _boards = new List<BoardDataSource>();
            if (Directory.Exists(boardPath))
            {
                foreach (var filePath in Directory.GetFiles(boardPath))
                {
                    if (filePath.EndsWith(".xml"))
                    {
                        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(BoardDataSource));
                            var board = (BoardDataSource)serializer.Deserialize(fileStream);
                            if (board != null)
                            {
                                _boards.Add(board);
                            }
                        }
                    }
                }
            }
        }

        public void DrawRoute(List<CaseManager> route, Color? playerColor = null)
        {
            CaseManager previousCaseRoute = null;
            foreach (var caseRoute in route)
            {
                caseRoute.UpdateBorder(playerColor);
                if (previousCaseRoute != null && !caseRoute.itemDataSource.index.Equals(previousCaseRoute.itemDataSource.index))
                {
                    var lineRoute = boardManager.FindLineManager(previousCaseRoute, caseRoute);
                    if (lineRoute != null)
                    {
                        lineRoute.UpdateColor(playerColor);
                    }
                }
                previousCaseRoute = caseRoute;
            }
        }

        public void CleanRoute(List<CaseManager> route, Color playerColor)
        {
            int count = 1;
            CaseManager previousCaseRoute = null;
            foreach (var caseRoute in route)
            {
                if (route.Count != count)
                {
                    caseRoute.SetDefaultBorder(playerColor);
                }
                if (previousCaseRoute != null && !caseRoute.itemDataSource.index.Equals(previousCaseRoute.itemDataSource.index))
                {
                    var lineRoute = boardManager.FindLineManager(previousCaseRoute, caseRoute);
                    if (lineRoute != null)
                    {
                        lineRoute.SetDefaultColor(playerColor);
                    }
                }
                previousCaseRoute = caseRoute;
                count++;
            }
        }

        public bool IsBestColumnTurn(IndexDataSource indexDataSource)
        {
            var turn = boardManager.GetNextTurn(indexDataSource);
            return indexDataSource.column == turn.bestColumn;
        }

        public List<CaseManager> GetClashCandidate(CaseManager target)
        {
            List<CaseManager> result = new List<CaseManager>();
            result.AddRange(target.itemDataSource.targets.Select(t => boardManager.FindCaseManager(t)).Where(c => c.hasPlayer));
            result.AddRange(boardManager.boardDataSource.cases.Where(c => c.targets.Any(t => t == target.itemDataSource.index)).Select(c => boardManager.FindCaseManager(c.index)).Where(c => c.hasPlayer));
            return result;
        }
    }

    public class SearchRouteContext
    {
        public int min;
        public int max;
        public int tire;
        public int bendStop;
        public string bendName;
        public int playerIndex;
        public bool isLastLap;
    }

    public class SearchRouteResult
    {
        public SearchRouteResult()
        {
            routes = new Dictionary<IndexDataSource, List<RouteResult>>();
        }

        public Dictionary<IndexDataSource, List<RouteResult>> routes;

        public void AddRoute(List<CaseManager> route, int nbOutOfBend, bool isStandWay, bool isBadWay, int nbLineMove)
        {
            if (route != null && route.Count > 0)
            {
                List<RouteResult> currentRoutes = null;
                var target = route.Last().itemDataSource.index;
                if (routes.ContainsKey(target))
                {
                    currentRoutes = routes[target];
                }
                else
                {
                    currentRoutes = new List<RouteResult>();
                    routes.Add(target, currentRoutes);
                }
                currentRoutes.Add(new RouteResult()
                {
                    isBadWay = isBadWay,
                    isStandWay = isStandWay,
                    nbLineMove = nbLineMove,
                    nbOutOfBend = nbOutOfBend,
                    route = route
                });
            }
        }
    }

    public class OutOfBendRoute
    {
        public List<CaseManager> route;
        public int nbOut;
    }

    public class RouteResult
    {
        public List<CaseManager> route;
        public int nbOutOfBend;
        public bool isStandWay;
        public bool isBadWay;
        public int nbLineMove;
    }
}