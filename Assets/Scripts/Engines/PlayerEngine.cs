using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using FormuleD.Managers;
using System;
using FormuleD.Models;
using UnityEngine.UI;
using FormuleD.Models.Board;
using FormuleD.Models.Contexts;
using FormuleD.Managers.Course.Player;
using FormuleD.Managers.Course.Board;
using FormuleD.Managers.Course;

namespace FormuleD.Engines
{
    public class PlayerEngine : MonoBehaviour
    {
        public CarLayoutManager carLayoutManager;

        public DePanelManager dePanelManager;
        public PlayerPanelManager playerPanelManager;
        

        public CameraManager cameraManager;

        private PlayerContext _currentPlayer;

        public static PlayerEngine Instance = null;
        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Multiple instances of PlayerEngine!");
            }
            Instance = this;
        }

        public void LoadPlayers(List<PlayerContext> players)
        {
            var starts = BoardEngine.Instance.GetStartCase();

            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (ContextEngine.Instance.gameContext.state == GameStateType.Qualification)
                {
                    if (player.qualification == null)
                    {
                        player.qualification = new QualificationPlayerContext();
                        player.qualification.state = QualificationStateType.NoPlay;
                        player.state = PlayerStateType.Waiting;
                    }
                    if (player.qualification.turnHistories.Count == 0)
                    {
                        var startCase = starts[0];
                        var history = new HistoryContext();
                        history.paths = new List<List<IndexDataSource>>() { new List<IndexDataSource>() { startCase.itemDataSource.index } };
                        player.qualification.turnHistories.Add(history);
                        player.qualification.startDate = DateTime.UtcNow;
                        player.qualification.state = QualificationStateType.Playing;
                    }
                }
                else
                {
                    if (!player.turnHistories.Any())
                    {
                        var startCase = starts[i];
                        var history = new HistoryContext();
                        history.paths = new List<List<IndexDataSource>>() { new List<IndexDataSource>() { startCase.itemDataSource.index } };
                        player.turnHistories.Add(history);
                        player.state = PlayerStateType.Waiting;
                        player.lap = -1;
                    }
                }
            }

            if (ContextEngine.Instance.gameContext.state == GameStateType.Qualification)
            {
                _currentPlayer = players.First();
                if (_currentPlayer.state == PlayerStateType.Waiting)
                {
                    _currentPlayer.currentTurn = new HistoryContext();
                    _currentPlayer.state = PlayerStateType.RollDice;
                }
            }
            else
            {
                _currentPlayer = players.FirstOrDefault(p => p.IsPlayable() && p.state != PlayerStateType.Waiting);
            }

            carLayoutManager.BuildCars(players);
            playerPanelManager.BuildPlayers(players);

            if (_currentPlayer == null)
            {
                this.NextPlayer();
            }
            else
            {
                this.SelectedPlayerView(_currentPlayer);
            }
        }

        private PlayerContext _previousViewPlayer;
        public void SelectedPlayerView(PlayerContext playerContext)
        {
            RaceEngine.Instance.CleanCurrent();
            if (_previousViewPlayer != null)
            {
                var previousTurnHistories = this.GetTurnHistories(_previousViewPlayer);
                var previousHistory = previousTurnHistories.Last();
                if (previousHistory.paths.Any())
                {
                    BoardEngine.Instance.CleanRoute(previousHistory.paths.SelectMany(p => p.Select(i => BoardEngine.Instance.GetCase(i))).ToList(), _previousViewPlayer.GetColor());
                }
            }
            var turnHistories = this.GetTurnHistories(playerContext);
            if (_previousViewPlayer == null || _previousViewPlayer.name != playerContext.name)
            {
                Vector3 previousPoint;
                var car = carLayoutManager.FindCarManager(playerContext);
                if (turnHistories.Any())
                {
                    CaseManager previousCase = BoardEngine.Instance.GetCase(turnHistories.Last().paths.First().First());
                    previousPoint = previousCase.transform.position;
                }
                else
                {
                    previousPoint = car.transform.position;
                }
                cameraManager.UpdateZoomPosition(car.transform.position, previousPoint);
            }

            _previousViewPlayer = playerContext;
            playerPanelManager.SelectedPlayer(playerContext);
            FeatureEngine.Instance.DisplayFeature(playerContext);
            dePanelManager.UpdateDe(playerContext);
            if (playerContext.state == PlayerStateType.RollDice)
            {
                int gear = turnHistories.Last().gear;
                if (gear <= 0)
                {
                    gear = 1;
                }
                var de = dePanelManager.buttonDes[gear - 1];
                RaceEngine.Instance.OnViewGear(gear, de.min, de.max);
            }
            else if (playerContext.state == PlayerStateType.ChoseRoute)
            {
                var gear = playerContext.currentTurn.gear;
                RaceEngine.Instance.OnViewRollDice(gear, playerContext.currentTurn.de);
            }
            else if (playerContext.state == PlayerStateType.Aspiration)
            {
                RaceEngine.Instance.OnAspiration(true);
            }
            else if (playerContext.state == PlayerStateType.StandOut)
            {
                RaceEngine.Instance.OnStandOut();
            }

            if (turnHistories.Count > 1)
            {
                var previousHistory = turnHistories.Last();
                if (previousHistory.paths.Count > 0)
                {
                    BoardEngine.Instance.DrawRoute(previousHistory.paths.SelectMany(p => p.Select(i => BoardEngine.Instance.GetCase(i))).ToList(), playerContext.GetColor());
                }
            }
        }

        public void SelectedDe(int gear)
        {
            FeatureEngine.Instance.WarningDemotion(_currentPlayer, gear);
            dePanelManager.UpdateSelectedDe(gear);
        }

        public void LoadAspirationView()
        {
            dePanelManager.UpdateDe(_currentPlayer);
        }

        public void UpdateGear(int gear, int deValue)
        {
            if (_currentPlayer != null && _currentPlayer.state == PlayerStateType.RollDice)
            {
                FeatureEngine.Instance.ApplyDemotion(_currentPlayer, gear);
                _currentPlayer.currentTurn.gear = gear;
                _currentPlayer.currentTurn.de = deValue;
                _currentPlayer.state = PlayerStateType.ChoseRoute;
                dePanelManager.UpdateDe(_currentPlayer);

                this.SelectedPlayerView(_currentPlayer);
            }
        }

        public void SelectedRoute(RouteResult candidate)
        {
            var endCase = candidate.route.Last();
            StandDataSource standData = null;
            if (endCase.standDataSource != null)
            {
                standData = endCase.standDataSource;
            }
            FeatureEngine.Instance.ApplyRoute(_currentPlayer, candidate);

            bool hasNewLap = BoardEngine.Instance.ContainsFinishCase(candidate.route.Select(r => r.itemDataSource.index));
            if (hasNewLap)
            {
                _currentPlayer.lap = _currentPlayer.lap + 1;
            }

            if (endCase.bendDataSource != null)
            {
                if (candidate.nbOutOfBend == 0 && _currentPlayer.state != PlayerStateType.Aspiration)
                {
                    if (_currentPlayer.lastBend == endCase.bendDataSource.name)
                    {
                        _currentPlayer.stopBend = _currentPlayer.stopBend + 1;
                    }
                    else
                    {
                        _currentPlayer.stopBend = 1;
                        _currentPlayer.lastBend = endCase.bendDataSource.name;
                    }
                }
                else if (_currentPlayer.lastBend != endCase.bendDataSource.name)
                {
                    _currentPlayer.stopBend = 0;
                    _currentPlayer.lastBend = endCase.bendDataSource.name;
                }
            }
            else
            {
                _currentPlayer.lastBend = string.Empty;
                _currentPlayer.stopBend = 0;
            }

            FeatureEngine.Instance.ApplyDangerousRoute(_currentPlayer, candidate);

            if (_currentPlayer.state == PlayerStateType.ChoseRoute)
            {
                var turnHistories = this.GetTurnHistories(_currentPlayer);
                if (turnHistories.Any())
                {
                    var previousHistory = turnHistories.Last();
                    if (previousHistory.paths.Any())
                    {
                        BoardEngine.Instance.CleanRoute(previousHistory.paths.SelectMany(p => p.Select(i => BoardEngine.Instance.GetCase(i))).ToList(), _currentPlayer.GetColor());
                    }
                }
                _currentPlayer.currentTurn.paths.Add(candidate.route.Select(r => r.itemDataSource.index).ToList());
                _currentPlayer.currentTurn.outOfBend = candidate.nbOutOfBend;
                if (candidate.nbOutOfBend == 0 && standData == null && !candidate.isBadWay && this.CheckAspiration(candidate.route))
                {
                    _currentPlayer.state = PlayerStateType.Aspiration;
                    this.CheckIsDead(_currentPlayer);
                }
                else
                {
                    this.EndTurn(standData);
                }
            }
            else if (_currentPlayer.state == PlayerStateType.Aspiration || _currentPlayer.state == PlayerStateType.StandOut)
            {
                _currentPlayer.currentTurn.paths.Add(candidate.route.Select(r => r.itemDataSource.index).ToList());
                if (standData == null && !candidate.isBadWay && this.CheckAspiration(candidate.route))
                {
                    _currentPlayer.state = PlayerStateType.Aspiration;
                    this.CheckIsDead(_currentPlayer);
                }
                else
                {
                    this.EndTurn(standData);
                }
            }

            if (_currentPlayer.state == PlayerStateType.EndTurn || _currentPlayer.state == PlayerStateType.Dead)
            {
                var turnHistories = this.GetTurnHistories(_currentPlayer);
                turnHistories.Add(_currentPlayer.currentTurn);
                _currentPlayer.currentTurn = null;
                ContextEngine.Instance.gameContext.lastTurn = DateTime.Now;

                if (ContextEngine.Instance.gameContext.state == GameStateType.Qualification)
                {
                    if (_currentPlayer.lap > 0)
                    {
                        var previousHistory = turnHistories.Last();
                        if (previousHistory.paths.Any())
                        {
                            BoardEngine.Instance.CleanRoute(previousHistory.paths.SelectMany(p => p.Select(i => BoardEngine.Instance.GetCase(i))).ToList(), _currentPlayer.GetColor());
                        }

                        _currentPlayer.qualification.endDate = DateTime.UtcNow;
                        int nbDe = _currentPlayer.qualification.turnHistories.Count - 1;
                        var duration = _currentPlayer.qualification.endDate - _currentPlayer.qualification.startDate;
                        int totalMin = Mathf.FloorToInt((float)duration.TotalMinutes);
                        _currentPlayer.qualification.total = nbDe + totalMin + _currentPlayer.qualification.outOfBend;
                        _currentPlayer.qualification.state = QualificationStateType.Completed;
                        _currentPlayer.state = PlayerStateType.Finish;
                    }
                    else if (_currentPlayer.state == PlayerStateType.Dead)
                    {
                        _currentPlayer.qualification.endDate = DateTime.UtcNow;
                        _currentPlayer.qualification.state = QualificationStateType.Completed;
                        _currentPlayer.qualification.isDead = true;
                    }
                }
                else if (_currentPlayer.lap == ContextEngine.Instance.gameContext.totalLap)
                {
                    _currentPlayer.state = PlayerStateType.Finish;
                    _currentPlayer.position = ContextEngine.Instance.gameContext.players.Where(p => p.state == PlayerStateType.Finish).Count();
                }
                else if (_currentPlayer.state != PlayerStateType.Dead)
                {
                    _currentPlayer.state = PlayerStateType.Waiting;
                }
            }
        }

        public void CheckIsDead(PlayerContext player)
        {
            if (FeatureEngine.Instance.CheckIsDead(player))
            {
                player.state = PlayerStateType.Dead;
            }
        }

        public void EndTurn(StandDataSource standDataSource)
        {
            if (standDataSource != null)
            {
                if (standDataSource.playerIndex.HasValue && standDataSource.playerIndex.Value == _currentPlayer.index)
                {
                    _currentPlayer.features.tire = 6;
                    var blackDice = RaceEngine.Instance.BlackDice();
                    if (blackDice <= 10)
                    {
                        _currentPlayer.currentTurn.standMovement = Mathf.CeilToInt(((float)blackDice) / 2.0f);
                        _currentPlayer.currentTurn.gear = 4;
                        _currentPlayer.state = PlayerStateType.StandOut;
                    }
                    else
                    {
                        if (_currentPlayer.currentTurn.gear > 4)
                        {
                            _currentPlayer.currentTurn.gear = 4;
                        }
                        _currentPlayer.state = PlayerStateType.EndTurn;
                    }
                }
                else
                {
                    _currentPlayer.state = PlayerStateType.EndTurn;
                }
            }
            else
            {
                RaceEngine.Instance.OnClash(_currentPlayer);
                RaceEngine.Instance.OnBrokenEngine(_currentPlayer);
                _currentPlayer.state = PlayerStateType.EndTurn;
            }
            this.CheckIsDead(_currentPlayer);
        }

        public void MoveCar(List<CaseManager> route)
        {
            if (route != null && _currentPlayer != null)
            {
                var carManager = carLayoutManager.FindCarManager(_currentPlayer);
                if (carManager != null)
                {
                    var firstCase = route.First();
                    firstCase.hasPlayer = false;
                    var lastCase = route.Last();
                    lastCase.hasPlayer = true;
                    var nextCase = BoardEngine.Instance.GetNextCase(lastCase);

                    var nextPosition = new Vector3(nextCase.transform.position.x, nextCase.transform.position.y, carManager.transform.position.z);
                    var movements = route.Select(d => new Vector3(d.transform.position.x, d.transform.position.y, carManager.transform.position.z));
                    carManager.AddMovements(movements, nextPosition, _currentPlayer.currentTurn.gear, lastCase);
                }
            }
        }

        public PlayerContext GetCurrent()
        {
            return _currentPlayer;
        }

        public void NextPlayer()
        {
            PlayerContext nextPlayer = null;
            if (_currentPlayer != null)
            {
                var history = this.GetTurnHistories(_currentPlayer).Last();
                var fullPath = history.paths.SelectMany(p => p.Select(i => i)).ToList();
                if (history.outOfBend > 0 && _currentPlayer.features.tire == 0 && fullPath.Count > 1)
                {
                    var carManager = carLayoutManager.FindCarManager(_currentPlayer);
                    var previousCase = BoardEngine.Instance.GetCase(fullPath[fullPath.Count - 2]);
                    carManager.ReturnCar(previousCase.transform.position);
                    history.gear = 0;
                }
                nextPlayer = this.FindNextPlayer(_currentPlayer);
            }

            if (ContextEngine.Instance.gameContext.state == GameStateType.Qualification)
            {
                _currentPlayer.currentTurn = new HistoryContext();
                _currentPlayer.state = PlayerStateType.RollDice;
                this.SelectedPlayerView(_currentPlayer);
            }
            else
            {
                var playablePlayer = ContextEngine.Instance.gameContext.players.Count(p => p.IsPlayable());
                if (playablePlayer > 1)
                {
                    if (nextPlayer == null)
                    {
                        ContextEngine.Instance.gameContext.players = this.OrderedPlayerTurn();
                        var test = ContextEngine.Instance.gameContext.players.Count(p => p.IsPlayable());
                        if (ContextEngine.Instance.gameContext.players.Count(p => p.IsPlayable()) > 1)
                        {
                            ContextEngine.Instance.gameContext.turn++;
                            nextPlayer = ContextEngine.Instance.gameContext.players.FirstOrDefault(p => p.IsPlayable());
                        }
                    }

                    nextPlayer.currentTurn = new HistoryContext();
                    nextPlayer.state = PlayerStateType.RollDice;
                    _currentPlayer = nextPlayer;
                    this.SelectedPlayerView(_currentPlayer);
                }
                else
                {
                    RaceEngine.Instance.OnEndGame();
                }
            }
        }

        private List<PlayerContext> OrderedPlayerTurn()
        {
            IEnumerable<PlayerContext> result = ContextEngine.Instance.gameContext.players;
            //TODO gerer la règle des arrets au stande
            result = result
                .OrderBy(p => p.position)
                .ThenByDescending(p => p.lap)
                .ThenByDescending(p => BoardEngine.Instance.GetCase(this.GetCurrentIndex(p)).itemDataSource.order)
                .ThenByDescending(p => this.GetTurnHistories(p).Last().gear)
                .ThenByDescending(p => BoardEngine.Instance.IsBestColumnTurn(this.GetCurrentIndex(p)));
            return result.ToList();
        }

        private PlayerContext FindNextPlayer(PlayerContext player)
        {
            PlayerContext result = null;
            int currentIndex = int.MaxValue;
            for (int i = 0; i < ContextEngine.Instance.gameContext.players.Count; i++)
            {
                var current = ContextEngine.Instance.gameContext.players[i];
                if (current == player)
                {
                    currentIndex = i;
                }
                else if (current.IsPlayable() && currentIndex < i)
                {
                    result = current;
                    break;
                }
            }
            return result;
        }

        //private void UpdateFeature(PlayerContext player)
        //{
        //    var currentCase = BoardEngine.Instance.GetCase(this.GetCurrentIndex(player));
        //    var currentStop = 0;
        //    var maxStop = 0;
        //    if (currentCase.bendDataSource != null)
        //    {
        //        maxStop = currentCase.bendDataSource.stop;
        //        if (currentCase.bendDataSource.name == player.lastBend)
        //        {
        //            currentStop = player.stopBend;
        //        }
        //    }
        //    featurePanelManager.UpdateFeature(player.features, currentStop, maxStop);
        //}

        private bool CheckAspiration(List<CaseManager> route)
        {
            bool result = false;

            var target = route.Last();
            var nbCase = route.Count - 1;
            var currentGear = _currentPlayer.currentTurn.gear;
            if (_currentPlayer.currentTurn.de == nbCase && currentGear >= 4)
            {
                var nextCase = BoardEngine.Instance.GetNextCase(target);
                if (nextCase.hasPlayer)
                {
                    var nextPlayer = ContextEngine.Instance.gameContext.players.FirstOrDefault(p => p.name != _currentPlayer.name && this.GetCurrentIndex(p).Equals(nextCase.itemDataSource.index));
                    var turnHistories = this.GetTurnHistories(nextPlayer);
                    var nextGear = turnHistories.Last().gear;
                    if (nextGear >= 4 && currentGear >= nextGear)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        public PlayerContext FindPlayer(IndexDataSource index)
        {
            return ContextEngine.Instance.gameContext.players.FirstOrDefault(p => this.GetCurrentIndex(p).Equals(index));
        }

        public List<PlayerContext> FindBrokenCandidate()
        {
            List<PlayerContext> result = new List<PlayerContext>();
            foreach (var player in ContextEngine.Instance.gameContext.players)
            {
                var gear = 0;
                var currentCase = BoardEngine.Instance.GetCase(this.GetCurrentIndex(player));
                var isInStand = currentCase.standDataSource != null;
                if (player.currentTurn != null)
                {
                    gear = player.currentTurn.gear;
                }
                else
                {
                    var history = this.GetTurnHistories(player).Last();
                    gear = history.gear;
                }
                if (gear >= 5 && !isInStand)
                {
                    result.Add(player);
                }
            }

            return result;
        }

        public void PlayerDead(PlayerContext player)
        {
            if (player.state == PlayerStateType.Dead)
            {
                var carManager = carLayoutManager.FindCarManager(player);
                carManager.Dead();
                var target = BoardEngine.Instance.GetCase(this.GetCurrentIndex(player));
                RaceEngine.Instance.AddDangerousCase(target);
            }
        }

        public List<HistoryContext> GetTurnHistories(PlayerContext player)
        {
            List<HistoryContext> result = null;
            if (ContextEngine.Instance.gameContext.state == GameStateType.Qualification)
            {
                if (player.qualification != null)
                {
                    result = player.qualification.turnHistories;
                }
            }
            else
            {
                result = player.turnHistories;
            }
            return result;
        }

        public IndexDataSource GetCurrentIndex(PlayerContext player)
        {
            IndexDataSource result = null;
            HistoryContext history = null;
            if (player.currentTurn != null && player.currentTurn.paths.Any())
            {
                history = player.currentTurn;
            }
            else
            {
                if (ContextEngine.Instance.gameContext.state == GameStateType.Qualification)
                {
                    if (player.qualification != null)
                    {
                        history = player.qualification.turnHistories.Last();
                    }
                }
                else
                {
                    history = player.turnHistories.Last();
                }
            }

            if (history != null && history.paths.Any())
            {
                result = history.paths.Last().Last();
            }

            return result;
        }
    }
}