using UnityEngine;
using System.Linq;
using System.Collections;
using FormuleD.Managers;
using System.Collections.Generic;
using FormuleD.Models.Board;
using FormuleD.Models.Contexts;
using FormuleD.Managers.Course.Board;
using FormuleD.Managers.Course;

namespace FormuleD.Engines
{
    public class RaceEngine : MonoBehaviour
    {
        public Transform loaderTransform;
        public EndGameManager endGameManager;

        public bool isHoverGUI;

        public static RaceEngine Instance = null;
        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Multiple instances of GameEngine!");
            }
            Instance = this;
            isHoverGUI = true;
            loaderTransform.gameObject.SetActive(true);
        }

        // Use this for initialization
        void Start()
        {
            BoardEngine.Instance.LoadBoard(ContextEngine.Instance.gameContext.mapName);
            PlayerEngine.Instance.LoadPlayers(BoardEngine.Instance.GetStartCase());
            loaderTransform.gameObject.SetActive(false);
            isHoverGUI = false;
        }

        private SearchRouteResult _candidateRoutes;
        private RouteResult _candidateRoute;

        public void OnViewGear(int gear, int min, int max)
        {
            if (_candidateRoutes != null)
            {
                this.CleanCurrent();
            }

            PlayerEngine.Instance.SelectedDe(gear);
            _candidateRoutes = BoardEngine.Instance.FindRoutes(PlayerEngine.Instance.GetCurrent(), min, max);
            var player = PlayerEngine.Instance.GetCurrent();

            foreach (var routes in _candidateRoutes.routes)
            {
                var caseCandidate = routes.Value.First().route.Last();
                if (routes.Value.Any(r => !r.isBadWay))
                {
                    var minCandidate = routes.Value.Where(r => !r.isBadWay).Min(r => r.route.Count) - 1;
                    var maxCandidate = routes.Value.Where(r => !r.isBadWay).Max(r => r.route.Count) - 1;
                    caseCandidate.UpdateContent(gear, minCandidate, maxCandidate, routes.Value.Any(r => r.nbOutOfBend > 0), false);
                }
                else
                {
                    var minCandidate = max;
                    var maxCandidate = min;
                    var hasRouteCandidate = false;
                    foreach (var route in routes.Value)
                    {
                        if (!_candidateRoutes.routes.Any(cr => cr.Value.Any(r => !r.isBadWay && r.route.Count == route.route.Count)))
                        {
                            if (route.route.Count < minCandidate)
                            {
                                minCandidate = route.route.Count;
                            }
                            if (route.route.Count > maxCandidate)
                            {
                                maxCandidate = route.route.Count;
                            }
                            hasRouteCandidate = true;
                        }
                    }
                    if (hasRouteCandidate)
                    {
                        caseCandidate.UpdateContent(gear, minCandidate, maxCandidate, routes.Value.Any(r => r.nbOutOfBend > 0), true);
                    }
                }
            }
        }

        public void OnRollDice(int gear, int min, int max)
        {
            if (_candidateRoutes != null)
            {
                this.CleanCurrent();
            }
            var deValue = this.ComputeDice(min, max);
            this.OnViewRollDice(gear, deValue);
        }

        public void OnViewRollDice(int gear, int deValue)
        {
            var player = PlayerEngine.Instance.GetCurrent();
            if (deValue == 0)
            {
                var current = BoardEngine.Instance.GetCase(player.GetLastIndex());
                player.state = PlayerStateType.ChoseRoute;
                PlayerEngine.Instance.SelectedRoute(new RouteResult()
                {
                    route = new List<CaseManager>() { current },
                    nbOutOfBend = 0,
                    isStandWay = false,
                    isBadWay = false,
                    nbLineMove = 0
                });
                this.OnFinishMouvement();
            }
            else
            {
                int minValue;
                int maxValue;
                PlayerEngine.Instance.UpdateGear(gear, deValue, out minValue, out maxValue);
                _candidateRoutes = BoardEngine.Instance.FindRoutes(player, minValue - 1, maxValue);

                var playerStandCase = _candidateRoutes.routes.Select(r => r.Value.First().route.Last()).Where(r => r.standDataSource != null && r.standDataSource.playerIndex.HasValue).FirstOrDefault();
                if (playerStandCase != null)
                {
                    var standWay = _candidateRoutes.routes[playerStandCase.itemDataSource.index]
                        .OrderBy(r => r.nbLineMove)
                        .ThenBy(r => r.route.Skip(1).Count(c => c.isDangerous))
                        .FirstOrDefault();
                    if (standWay != null && standWay.route.Count > 1)
                    {
                        var caseCandidate = standWay.route.Last();
                        caseCandidate.UpdateContent(gear, standWay.route.Count - 1, standWay.route.Count - 1, standWay.route.Any(r => r.isDangerous), false);
                        caseCandidate.isCandidate = true;
                    }
                }
                else
                {
                    var standWay = _candidateRoutes.routes.SelectMany(r => r.Value.Where(c => c.isStandWay).Select(c => c.route))
                        .OrderByDescending(r => r.Last().itemDataSource.order)
                        .ThenBy(r => r.Skip(1).Count(c => c.isDangerous))
                        .FirstOrDefault();
                    if (standWay != null && standWay.Count > 1)
                    {
                        var caseCandidate = standWay.Last();
                        caseCandidate.UpdateContent(gear, standWay.Count - 1, standWay.Count - 1, standWay.Any(r => r.isDangerous), false);
                        caseCandidate.isCandidate = true;
                    }
                }

                foreach (var routes in _candidateRoutes.routes.Where(r => !r.Value.Any(rv => rv.isStandWay)))
                {
                    var route = routes.Value
                        .OrderBy(r => r.nbLineMove)
                        .ThenByDescending(r => r.route.Count - deValue)
                        .ThenBy(r => r.route.Skip(1).Count(c => c.isDangerous)).First();

                    if (route.isBadWay || route.route.Count - 1 >= minValue)
                    {
                        var caseCandidate = route.route.Last();
                        var hasWarning = route.nbOutOfBend > 0;
                        if (!hasWarning && deValue - (route.route.Count - 1) >= 1)
                        {
                            hasWarning = true;
                        }
                        if (!hasWarning && route.route.Skip(1).Any(c => c.isDangerous))
                        {
                            hasWarning = true;
                        }
                        caseCandidate.UpdateContent(gear, route.route.Count - 1, route.route.Count - 1, hasWarning, route.isBadWay);
                        caseCandidate.isCandidate = true;
                    }
                }
            }
        }

        private int ComputeDice(int min, int max)
        {
            int result = 0;

            bool specialDeparture = false;
            if (ContextEngine.Instance.gameContext.turn == 1)
            {
                //TODO Voir si j'ajoute un teasing pour le mauvais et bon départ
                int handicape = this.BlackDice();
                if (handicape == 1)
                {
                    specialDeparture = true;
                    result = 0;
                }
                else if (handicape == 20)
                {
                    specialDeparture = true;
                    result = 4;
                }
            }
            if (!specialDeparture)
            {
                result = Random.Range(min, max + 1);
            }

            return result;
        }

        public void OnAspiration(bool enable)
        {
            if (enable)
            {
                PlayerEngine.Instance.LoadAspirationView();
                var player = PlayerEngine.Instance.GetCurrent();
                var min = 3 - player.features.brake;
                if (min < 1)
                {
                    min = 1;
                }
                var max = 3;
                _candidateRoutes = BoardEngine.Instance.FindRoutes(player, min, max);
                if (_candidateRoutes.routes.Count > 0)
                {
                    var gear = player.currentTurn.gear;
                    foreach (var routes in _candidateRoutes.routes)
                    {
                        var route = routes.Value
                            .OrderBy(r => r.nbLineMove)
                            .ThenByDescending(r => r.route.Count - max)
                            .ThenBy(r => r.route.Skip(1).Count(c => c.isDangerous)).First();

                        var hasWarning = route.nbOutOfBend > 0;
                        if (!hasWarning && 3 - (route.route.Count - 1) >= 1)
                        {
                            hasWarning = true;
                        }
                        if (!hasWarning && route.route.Skip(1).Any(c => c.isDangerous))
                        {
                            hasWarning = true;
                        }

                        var caseCandidate = route.route.Last();
                        caseCandidate.UpdateContent(gear, route.route.Count - 1, route.route.Count - 1, hasWarning, route.isBadWay);
                        caseCandidate.isCandidate = true;
                    }
                }
                else
                {
                    PlayerEngine.Instance.EndTurn(null);
                }
            }
            else
            {
                PlayerEngine.Instance.EndTurn(null);
            }
        }

        public void OnStandOut()
        {
            var player = PlayerEngine.Instance.GetCurrent();
            bool hasRoute = false;
            _candidateRoutes = BoardEngine.Instance.FindRoutes(player, 1, player.currentTurn.standMovement);
            if (_candidateRoutes.routes.Count > 0)
            {
                var maxCount = _candidateRoutes.routes.Max(cr => cr.Value.Max(r => r.route.Count));
                if (maxCount > 1)
                {
                    foreach (var routes in _candidateRoutes.routes)
                    {
                        var route = routes.Value.Where(r => r.route.Count == maxCount)
                            .OrderBy(r => r.nbLineMove)
                            .ThenBy(r => r.route.Count(c => c.isDangerous)).FirstOrDefault();

                        if (route != null && !route.isBadWay)
                        {
                            hasRoute = true;
                            var caseCandidate = route.route.Last();
                            caseCandidate.UpdateContent(player.currentTurn.gear, route.route.Count - 1, route.route.Count - 1, false, false);
                            caseCandidate.isCandidate = true;
                        }
                    }
                }
            }

            if (!hasRoute)
            {
                PlayerEngine.Instance.EndTurn(null);
            }
        }

        public void OnViewRoute(CaseManager target, bool enable)
        {
            if (!isHoverGUI && _candidateRoutes != null)
            {
                if (enable)
                {
                    var player = PlayerEngine.Instance.GetCurrent();
                    var features = player.features;
                    var de = player.currentTurn.de;

                    if (_candidateRoutes.routes.ContainsKey(target.itemDataSource.index))
                    {
                        var routes = _candidateRoutes.routes[target.itemDataSource.index];
                        _candidateRoute = routes
                            .OrderBy(r => r.nbLineMove)
                            .ThenByDescending(r => r.route.Count - de)
                            .ThenBy(r => r.route.Skip(1).Count(c => c.isDangerous)).First();

                        if (_candidateRoute.nbOutOfBend > 0)
                        {
                            features = PlayerEngine.Instance.ComputeOutOfBend(features, _candidateRoute.nbOutOfBend);
                        }

                        if (!_candidateRoute.isStandWay)
                        {
                            var nbCase = _candidateRoute.route.Count - 1;
                            features = PlayerEngine.Instance.ComputeUseBrake(features, player.state, nbCase);
                            PlayerEngine.Instance.DisplayWarningFeature(features);
                        }

                        BoardEngine.Instance.DrawRoute(_candidateRoute.route, player.GetColor());
                    }
                }
                else
                {
                    if (_candidateRoute != null)
                    {
                        PlayerEngine.Instance.ResetPlayerState();
                        BoardEngine.Instance.DrawRoute(_candidateRoute.route);
                    }
                }
            }
        }

        public void OnSelectRoute(CaseManager target)
        {
            if (!isHoverGUI && _candidateRoutes != null)
            {
                var dest = _candidateRoute.route.Last();
                if (_candidateRoute == null || !target.itemDataSource.index.Equals(dest.itemDataSource.index))
                {
                    this.OnViewRoute(target, true);
                }
                else
                {
                    PlayerEngine.Instance.SelectedRoute(_candidateRoute);
                    PlayerEngine.Instance.MoveCar(_candidateRoute.route);
                    _candidateRoute = null;
                    this.CleanCurrent();
                }
            }
        }

        public void OnFinishMouvement()
        {
            //TODO Accrochage
            //TODO Fin de course
            var player = PlayerEngine.Instance.GetCurrent();
            if (player.state == PlayerStateType.Aspiration)
            {
                this.OnAspiration(true);
            }
            else if (player.state == PlayerStateType.StandOut)
            {
                this.OnStandOut();
            }
            else
            {
                if (player.state == PlayerStateType.Dead)
                {
                    PlayerEngine.Instance.PlayerDead(player);
                }
                PlayerEngine.Instance.NextPlayer();
            }
        }

        public void OnClash(PlayerContext player)
        {
            var target = BoardEngine.Instance.GetCase(player.GetLastIndex());
            List<CaseManager> candidates = BoardEngine.Instance.GetClashCandidate(target);
            if (candidates.Any())
            {
                foreach (var candidate in candidates)
                {
                    var playerTarget = PlayerEngine.Instance.FindPlayer(candidate.itemDataSource.index);
                    var targetDe = this.BlackDice();
                    var currentDe = this.BlackDice();
                    //TODO faire une animation pour le choque
                    if (targetDe == 1)
                    {
                        playerTarget.features.handling = playerTarget.features.handling - 1;
                        this.AddDangerousCase(candidate);
                        PlayerEngine.Instance.CheckIsDead(playerTarget);
                        if (playerTarget.state == PlayerStateType.Dead)
                        {
                            PlayerEngine.Instance.PlayerDead(playerTarget);
                        }
                    }
                    if (currentDe == 1)
                    {
                        player.features.handling = player.features.handling - 1;
                        this.AddDangerousCase(target);
                    }
                }
            }
        }

        public void OnBrokenEngine(PlayerContext player)
        {
            var gear = player.currentTurn.gear;
            if (gear >= 5)
            {
                var de = player.currentTurn.de;
                if ((gear == 5 && de == 20) || (gear == 6 && de == 30))
                {
                    foreach (var candidate in PlayerEngine.Instance.FindBrokenCandidate())
                    {
                        var blackDe = this.BlackDice();
                        if (blackDe <= 4)
                        {
                            //TODO faire une animation pour la case moteur
                            candidate.features.motor = candidate.features.motor - 1;
                            var target = BoardEngine.Instance.GetCase(candidate.GetLastIndex());
                            this.AddDangerousCase(target);
                            if (player.name != candidate.name)
                            {
                                PlayerEngine.Instance.CheckIsDead(candidate);
                                if (candidate.state == PlayerStateType.Dead)
                                {
                                    PlayerEngine.Instance.PlayerDead(candidate);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AddDangerousCase(CaseManager target)
        {
            target.SetDangerous(true);
            if (!ContextEngine.Instance.gameContext.dangerousCases.Contains(target.itemDataSource.index))
            {
                ContextEngine.Instance.gameContext.dangerousCases.Add(target.itemDataSource.index);
            }
        }

        public int BlackDice()
        {
            return Random.Range(1, 20 + 1);
        }

        public void CleanCurrent()
        {
            if (_candidateRoutes != null)
            {
                foreach (var candidate in _candidateRoutes.routes)
                {
                    var lastCaseCandidate = candidate.Value.First().route.Last();
                    lastCaseCandidate.ResetContent();
                    lastCaseCandidate.isCandidate = false;
                }
            }
        }

        public void MouseEnterGUI()
        {
            isHoverGUI = true;
        }

        public void MouseLeaveGUI()
        {
            isHoverGUI = false;
        }

        public void OnEndGame()
        {
            ContextEngine.Instance.gameContext.type = GameType.Completed;
            endGameManager.gameObject.SetActive(true);
            endGameManager.OnOpen(ContextEngine.Instance.gameContext.players);
        }
    }
}