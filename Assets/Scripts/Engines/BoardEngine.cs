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

namespace FormuleD.Engines
{
    public class BoardEngine : MonoBehaviour
    {
        public BoardManager boardManager;

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

            return result;
        }

        public RouteResult FindRoutes(PlayerContext player, int min, int max)
        {
            RouteResult result = new RouteResult();
            var context = new SearchRouteContext();
            context.bendName = player.lastBend;
            context.bendStop = player.stopBend;
            context.min = min;
            context.max = max;
            context.tire = player.features.tire;
            List<CaseManager> baseRoute = new List<CaseManager>();
            baseRoute.Add(boardManager.FindCaseManager(player.GetLastIndex()));
            this.SearchRoutes(result, context, baseRoute, 0, 0, 0);

            return result;
        }
        private void SearchRoutes(RouteResult result, SearchRouteContext context, List<CaseManager> route, int rowMove, int exceeding, int outOfBend)
        {
            var currentMove = route.Count - 1;
            var currentCase = route.Last();
            if (context.min <= currentMove && currentMove <= context.max)
            {
                if (outOfBend > 0)
                {
                    this.AddOutOfBendRoute(result.outOfBendWay, currentCase.itemDataSource.index, route, outOfBend);
                }
                else
                {
                    this.AddRoute(result.goodWay, currentCase.itemDataSource.index, route);
                }
            }

            if (currentMove < context.max)
            {
                bool isNewOutOfBend = false;
                bool isNewExceeding = false;
                bool isEndExceeding = false;
                var nextTarget = currentCase.itemDataSource.targets.FirstOrDefault(t => t.enable && t.column == currentCase.itemDataSource.index.column);
                var nextcase = boardManager.FindCaseManager(nextTarget);
                if (currentCase.bendDataSource == null)
                {
                    if (nextcase.hasPlayer || nextcase.isDangerous)
                    {
                        isNewExceeding = true;
                    }
                }
                else if (nextcase.bendDataSource == null)
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
                bool hasTargetWay = false;
                foreach (var target in currentCase.itemDataSource.targets.Where(t => t.enable))
                {
                    var targetCase = boardManager.FindCaseManager(target);
                    if (!targetCase.hasPlayer)
                    {
                        bool continueRoute = false;
                        bool isBadWay = false;

                        int newRowMove = rowMove;
                        int newExceeding = exceeding;
                        int newOutOfBend = outOfBend;

                        if (isNewOutOfBend || outOfBend > 0)
                        {
                            if (isNewOutOfBend)
                            {
                                var stop = 0;
                                if (currentCase.bendDataSource.name == context.bendName)
                                {
                                    stop = context.bendStop;
                                }
                                var stopDif = currentCase.bendDataSource.stop - stop;
                                if (stopDif > 1)
                                {
                                    isBadWay = true;
                                    continueRoute = false;
                                }
                            }
                            if (!isBadWay)
                            {
                                if (newOutOfBend < context.tire)
                                {
                                    if (currentCase.itemDataSource.index.column == target.column)
                                    {
                                        continueRoute = true;
                                        newOutOfBend = newOutOfBend + 1;
                                    }
                                }
                                else
                                {
                                    isBadWay = true;
                                }
                            }
                        }
                        else if (currentCase.bendDataSource == null || targetCase.bendDataSource == null)
                        {
                            var columnDif = target.column - currentCase.itemDataSource.index.column;
                            if (isNewExceeding)
                            {
                                if (columnDif > 0 || columnDif < 0)
                                {
                                    newExceeding = columnDif * -1;
                                    continueRoute = true;
                                }
                                else
                                {
                                    newExceeding = 0;
                                    continueRoute = true;
                                }
                            }
                            else
                            {
                                if (columnDif == 0)
                                {
                                    if (newExceeding != 0 && isEndExceeding)
                                    {
                                        newExceeding = 0;
                                    }
                                    continueRoute = true;
                                }
                                else if (newExceeding != 0 && columnDif == newExceeding)
                                {
                                    continueRoute = true;
                                    newExceeding = 0;
                                }
                                else if (columnDif > 0 && rowMove >= 0 && rowMove < 2)
                                {
                                    newRowMove = newRowMove + 1;
                                    continueRoute = true;
                                }
                                else if (columnDif < 0 && rowMove <= 0 && rowMove > -2)
                                {
                                    newRowMove = newRowMove - 1;
                                    continueRoute = true;
                                }
                            }
                        }
                        else
                        {
                            newExceeding = 0;
                            newRowMove = 0;
                            continueRoute = true;
                        }

                        if (continueRoute)
                        {
                            hasTargetWay = true;
                            var newRoute = route.ToList();
                            newRoute.Add(targetCase);
                            this.SearchRoutes(result, context, newRoute, newRowMove, newExceeding, newOutOfBend);
                        }
                        if (isBadWay)
                        {
                            hasTargetWay = true;
                            var newRoute = route.ToList();
                            newRoute.Add(targetCase);
                            this.AddRoute(result.badWay, targetCase.itemDataSource.index, newRoute);
                        }
                    }
                }

                if (!hasTargetWay && currentMove < context.min)
                {
                    this.AddRoute(result.badWay, currentCase.itemDataSource.index, route);
                }
            }
        }

        private void AddOutOfBendRoute(Dictionary<IndexDataSource, List<OutOfBendRoute>> routes, IndexDataSource index, List<CaseManager> route, int outOfBend)
        {
            List<OutOfBendRoute> currentRoutes = null;
            if (routes.ContainsKey(index))
            {
                currentRoutes = routes[index];
            }
            else
            {
                currentRoutes = new List<OutOfBendRoute>();
                routes.Add(index, currentRoutes);
            }
            var outOfBendRoute = new OutOfBendRoute();
            outOfBendRoute.nbOut = outOfBend;
            outOfBendRoute.route = route;
            currentRoutes.Add(outOfBendRoute);
        }

        private void AddRoute(Dictionary<IndexDataSource, List<List<CaseManager>>> routes, IndexDataSource index, List<CaseManager> route)
        {
            List<List<CaseManager>> currentRoutes = null;
            if (routes.ContainsKey(index))
            {
                currentRoutes = routes[index];
            }
            else
            {
                currentRoutes = new List<List<CaseManager>>();
                routes.Add(index, currentRoutes);
            }
            currentRoutes.Add(route);
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
                if (previousCaseRoute != null)
                {
                    var lineRoute = boardManager.FindLineManager(previousCaseRoute, caseRoute);
                    lineRoute.UpdateColor(playerColor);
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
                if (previousCaseRoute != null)
                {
                    var lineRoute = boardManager.FindLineManager(previousCaseRoute, caseRoute);
                    lineRoute.SetDefaultColor(playerColor);
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
    }

    public class RouteResult
    {
        public RouteResult()
        {
            goodWay = new Dictionary<IndexDataSource, List<List<CaseManager>>>();
            outOfBendWay = new Dictionary<IndexDataSource, List<OutOfBendRoute>>();
            badWay = new Dictionary<IndexDataSource, List<List<CaseManager>>>();
        }
        public Dictionary<IndexDataSource, List<List<CaseManager>>> goodWay;
        public Dictionary<IndexDataSource, List<OutOfBendRoute>> outOfBendWay;
        public Dictionary<IndexDataSource, List<List<CaseManager>>> badWay;
    }

    public class OutOfBendRoute
    {
        public List<CaseManager> route;
        public int nbOut;
    }
}