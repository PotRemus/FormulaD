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

        private RouteResult _candidateRoutes;
        private List<CaseManager> _candidateRoute;
        private int _nbOutOfBend;
        private bool _isBadWay;

        public void OnViewGear(int gear, int min, int max)
        {
            if (_candidateRoutes != null)
            {
                this.CleanCurrent();
            }

            PlayerEngine.Instance.SelectedDe(gear);
            _candidateRoutes = BoardEngine.Instance.FindRoutes(PlayerEngine.Instance.GetCurrent(), min, max);
            foreach (var route in _candidateRoutes.badWay)
            {
                var caseCandidate = route.Value.First().Last();
                var minCandidate = route.Value.Min(r => r.Count) - 1;
                caseCandidate.UpdateContent(gear, minCandidate, max, false, true);
            }
            foreach (var route in _candidateRoutes.outOfBendWay)
            {
                var caseCandidate = route.Value.First().route.Last();
                var minCandidate = route.Value.Min(r => r.route.Count) - 1;
                var maxCandidate = route.Value.Max(r => r.route.Count) - 1;
                caseCandidate.UpdateContent(gear, minCandidate, maxCandidate, true, false);
            }
            foreach (var route in _candidateRoutes.goodWay)
            {
                var caseCandidate = route.Value.First().Last();
                var minCandidate = route.Value.Min(r => r.Count) - 1;
                var maxCandidate = route.Value.Max(r => r.Count) - 1;
                caseCandidate.UpdateContent(gear, minCandidate, maxCandidate, false, false);
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
                PlayerEngine.Instance.SelectedRoute(new List<CaseManager>() { current }, 0, false);
                this.OnFinishMouvement();
            }
            else
            {
                int minValue;
                int maxValue;
                PlayerEngine.Instance.UpdateGear(gear, deValue, out minValue, out maxValue);
                _candidateRoutes = BoardEngine.Instance.FindRoutes(player, minValue, maxValue);

                foreach (var routes in _candidateRoutes.badWay)
                {
                    //if(_candidateRoutes.goodWay.Any(r => r.Key =)
                    var route = routes.Value
                        .OrderByDescending(r => r.Count - deValue)
                        //.OrderBy(r => r.Count - deValue)
                        //.OrderBy(r => r.Count)
                        .ThenByDescending(r => r.Any(c => c.isDangerous)).First();
                    var caseCandidate = route.Last();
                    caseCandidate.UpdateContent(gear, route.Count - 1, route.Count - 1, false, true);
                    caseCandidate.isCandidate = true;
                }
                foreach (var routes in _candidateRoutes.outOfBendWay)
                {
                    var route = routes.Value
                        .OrderByDescending(r => r.route.Count - deValue)
                        //.OrderBy(r => r.route.Count - deValue)
                        //.OrderBy(r => r.route.Count)
                        .ThenByDescending(r => r.route.Any(c => c.isDangerous)).First();
                    var caseCandidate = route.route.Last();
                    caseCandidate.UpdateContent(gear, route.route.Count - 1, route.route.Count - 1, true, false);
                    caseCandidate.isCandidate = true;
                }
                foreach (var routes in _candidateRoutes.goodWay)
                {
                    var route = routes.Value
                        .OrderByDescending(r => r.Count - deValue)
                        //.OrderBy(r => r.Count - deValue)
                        //.OrderBy(r => r.Count)
                        .ThenByDescending(r => r.Any(c => c.isDangerous)).First();
                    var caseCandidate = route.Last();
                    var difDe = deValue - (route.Count - 1);
                    bool hasWarning = difDe >= 1;
                    if (!hasWarning && route.Any(r => r.isDangerous))
                    {
                        hasWarning = true;
                    }
                    caseCandidate.UpdateContent(gear, route.Count - 1, route.Count - 1, hasWarning, false);
                    caseCandidate.isCandidate = true;
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
                if (_candidateRoutes.goodWay.Any())
                {
                    var gear = player.currentTurn.gear;
                    foreach (var routes in _candidateRoutes.goodWay)
                    {
                        var route = routes.Value
                            .OrderByDescending(r => r.Count - 3)
                            //.OrderBy(r => r.Count - 3)
                            //.OrderBy(r => r.Count)
                            .ThenByDescending(r => r.Any(c => c.isDangerous)).First();
                        var caseCandidate = route.Last();
                        var difDe = 3 - (route.Count - 1);
                        bool hasWarning = difDe >= 1;
                        if (!hasWarning && route.Any(r => r.isDangerous))
                        {
                            hasWarning = true;
                        }
                        caseCandidate.UpdateContent(gear, route.Count - 1, route.Count - 1, hasWarning, false);
                        caseCandidate.isCandidate = true;
                    }
                }
                else
                {
                    PlayerEngine.Instance.EndTurn();
                }
            }
            else
            {
                PlayerEngine.Instance.EndTurn();
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
                    if (_candidateRoutes.goodWay.ContainsKey(target.itemDataSource.index))
                    {
                        var routes = _candidateRoutes.goodWay[target.itemDataSource.index];
                        _candidateRoute = routes
                                //.OrderBy(r => r.Count - de)
                                .OrderByDescending(r => r.Count - de)
                                //.OrderBy(r => r.Count)
                                .ThenByDescending(r => r.Any(c => c.isDangerous)).First();
                    }
                    else if (_candidateRoutes.outOfBendWay.ContainsKey(target.itemDataSource.index))
                    {
                        var routes = _candidateRoutes.outOfBendWay[target.itemDataSource.index];
                        var candidateTemp = routes
                                //.OrderBy(r => r.route.Count - de)
                                .OrderByDescending(r => r.route.Count - de)
                                //.OrderBy(r => r.route.Count)
                                .ThenByDescending(r => r.route.Any(c => c.isDangerous)).First();
                        _candidateRoute = candidateTemp.route;
                        _nbOutOfBend = candidateTemp.nbOut;
                        features = PlayerEngine.Instance.ComputeOutOfBend(features, candidateTemp.nbOut);
                    }
                    else if (_candidateRoutes.badWay.ContainsKey(target.itemDataSource.index))
                    {
                        var routes = _candidateRoutes.badWay[target.itemDataSource.index];
                        _candidateRoute = routes
                                //.OrderBy(r => r.Count - de)
                                .OrderByDescending(r => r.Count - de)
                                //.OrderBy(r => r.Count)
                                .ThenByDescending(r => r.Any(c => c.isDangerous)).First();
                        _isBadWay = true;
                    }


                    var nbCase = _candidateRoute.Count - 1;
                    features = PlayerEngine.Instance.ComputeUseBrake(features, player.state, nbCase);
                    PlayerEngine.Instance.DisplayWarningFeature(features);
                    var playerColor = player.GetColor();
                    BoardEngine.Instance.DrawRoute(_candidateRoute, playerColor);
                }
                else
                {
                    if (_candidateRoute != null)
                    {
                        _nbOutOfBend = 0;
                        _isBadWay = false;
                        PlayerEngine.Instance.ResetPlayerState();
                        BoardEngine.Instance.DrawRoute(_candidateRoute);
                    }
                }
            }
        }

        public void OnSelectRoute(CaseManager target)
        {
            if (!isHoverGUI && _candidateRoutes != null)
            {
                var dest = _candidateRoute.Last();
                if (_candidateRoute == null || !target.itemDataSource.index.Equals(dest.itemDataSource.index))
                {
                    this.OnViewRoute(target, true);
                }
                else
                {
                    PlayerEngine.Instance.SelectedRoute(_candidateRoute, _nbOutOfBend, _isBadWay);
                    PlayerEngine.Instance.MoveCar(_candidateRoute);
                    _candidateRoute = null;
                    _nbOutOfBend = 0;
                    _isBadWay = false;
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
                if (_candidateRoutes.goodWay.Any())
                {
                    foreach (var route in _candidateRoutes.goodWay)
                    {
                        var lastCaseCandidate = route.Value.First().Last();
                        lastCaseCandidate.ResetContent();
                        lastCaseCandidate.isCandidate = false;
                    }
                }
                if (_candidateRoutes.outOfBendWay.Any())
                {
                    foreach (var route in _candidateRoutes.outOfBendWay)
                    {
                        var lastCaseCandidate = route.Value.First().route.Last();
                        lastCaseCandidate.ResetContent();
                        lastCaseCandidate.isCandidate = false;
                    }
                }

                if (_candidateRoutes.badWay.Any())
                {
                    foreach (var route in _candidateRoutes.badWay)
                    {
                        var lastCaseCandidate = route.Value.First().Last();
                        lastCaseCandidate.ResetContent();
                        lastCaseCandidate.isCandidate = false;
                    }
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