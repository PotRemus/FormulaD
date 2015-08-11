using UnityEngine;
using System.Collections;
using System.Linq;
using FormuleD.Managers.Course.Player;
using FormuleD.Models.Contexts;

namespace FormuleD.Engines
{
    public class FeatureEngine : MonoBehaviour
    {
        public FeaturePanelManager featurePanelManager;

        public static FeatureEngine Instance = null;
        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Multiple instances of FeatureEngine!");
            }
            Instance = this;
        }

        public void DisplayFeature(PlayerContext player)
        {
            var currentCase = BoardEngine.Instance.GetCase(PlayerEngine.Instance.GetCurrentIndex(player));
            var currentStop = 0;
            var maxStop = 0;
            if (currentCase.bendDataSource != null)
            {
                maxStop = currentCase.bendDataSource.stop;
                if (currentCase.bendDataSource.name == player.lastBend)
                {
                    currentStop = player.stopBend;
                }
            }
            featurePanelManager.UpdateFeature(ContextEngine.Instance.gameContext.state, player.features, currentStop, maxStop);
        }

        public void WarningDemotion(PlayerContext player, int gear)
        {
            this.DisplayFeature(player);
            var features = this.ComputeDemotion(player, gear);
            featurePanelManager.WarningFeature(ContextEngine.Instance.gameContext.state, player.features, features);
        }

        public void ApplyDemotion(PlayerContext player, int gear)
        {
            player.features = this.ComputeDemotion(player, gear);
            this.DisplayFeature(player);
        }

        public void WarningRoute(PlayerContext player, RouteResult route)
        {
            this.DisplayFeature(player);
            var features = this.ComputeRoute(player, route);
            featurePanelManager.WarningFeature(ContextEngine.Instance.gameContext.state, player.features, features);
        }

        public void ApplyRoute(PlayerContext player, RouteResult route)
        {
            player.features = this.ComputeRoute(player, route);
            player.qualification.outOfBend = player.features.outOfBend;
            this.DisplayFeature(player);
        }

        public void ApplyDangerousRoute(PlayerContext player, RouteResult route)
        {
            bool hasChange = false;
            foreach (var current in route.route.Where(c => c.isDangerous))
            {
                var de = RaceEngine.Instance.BlackDice();
                if (de <= 4)
                {
                    player.features.handling = player.features.handling - 1;
                    hasChange = true;
                }
            }
            if (hasChange)
            {
                this.DisplayFeature(player);
            }
        }

        public bool ApplyClash(PlayerContext player)
        {
            bool result = false;
            var de = RaceEngine.Instance.BlackDice();
            if (de == 1)
            {
                player.features.handling = player.features.handling - 1;
                this.DisplayFeature(player);
                result = true;
            }
            return result;
        }

        public bool ApplyBrokenEngine(PlayerContext player)
        {
            bool result = false;
            var blackDe = RaceEngine.Instance.BlackDice();
            if (blackDe <= 4)
            {
                player.features.motor = player.features.motor - 1;
                this.DisplayFeature(player);
                result = true;
            }
            return result;
        }

        public void ComputeMinMaxGearbox(PlayerContext player, out int min, out int max)
        {
            var gear = PlayerEngine.Instance.GetTurnHistories(player).Last().gear;
            if (ContextEngine.Instance.gameContext.state == GameStateType.Race)
            {
                min = gear - 1;
                if (player.features.gearbox > 0 && player.features.brake > 0 && player.features.motor > 0)
                {
                    min -= 3;
                }
                else if (player.features.gearbox > 0 && player.features.brake > 0)
                {
                    min -= 2;
                }
                else if (player.features.gearbox > 0)
                {
                    min -= 1;
                }
                max = gear + 1;
            }
            else
            {
                min = gear - 1 - 3;
                max = gear + 1;
            }

            if (min < 1)
            {
                min = 1;
            }
            if (max > 6)
            {
                max = 6;
            }
        }

        public void ComputeMinMaxUseBrake(PlayerContext player, int de, out int min, out int max)
        {
            if (player.state == PlayerStateType.ChoseRoute && ContextEngine.Instance.gameContext.state == GameStateType.Race)
            {
                if (player.features.brake >= 3)
                {
                    if (player.features.tire >= 3)
                    {
                        min = de - 6;
                    }
                    else if (player.features.tire == 2)
                    {
                        min = de - 5;
                    }
                    else if (player.features.tire == 1)
                    {
                        min = de - 4;
                    }
                    else
                    {
                        min = de - 3;
                    }
                }
                else if (player.features.brake == 2)
                {
                    min = de - 2;
                }
                else if (player.features.brake == 1)
                {
                    min = de - 1;
                }
                else
                {
                    min = de;
                }
                if (min < 1)
                {
                    min = 1;
                }
                max = de;
            }
            else
            {
                min = de;
                max = de;
            }
        }

        public bool CheckIsDead(PlayerContext player)
        {
            bool result = false;
            if (player.features.tire < 0 || player.features.brake < 0 || player.features.body == 0 || player.features.motor == 0 || player.features.handling == 0)
            {
                result = true;
            }
            return result;
        }

        private FeatureContext ComputeDemotion(PlayerContext player, int targetGear)
        {
            FeatureContext result = player.features.Clone();
            if (ContextEngine.Instance.gameContext.state == GameStateType.Race)
            {
                var previousGear = PlayerEngine.Instance.GetTurnHistories(player).Last().gear;
                var gearDif = previousGear - targetGear;
                if (gearDif >= 4)
                {
                    result.gearbox = player.features.gearbox - 1;
                    result.brake = player.features.brake - 1;
                    result.motor = player.features.motor - 1;
                }
                else if (gearDif == 3)
                {
                    result.gearbox = player.features.gearbox - 1;
                    result.brake = player.features.brake - 1;
                }
                else if (gearDif == 2)
                {
                    result.gearbox = player.features.gearbox - 1;
                }
            }
            return result;
        }

        private FeatureContext ComputeRoute(PlayerContext player, RouteResult route)
        {
            bool isEnd = false;
            var hasFinishCase = BoardEngine.Instance.ContainsFinishCase(route.route.Select(r => r.itemDataSource.index));
            if (hasFinishCase)
            {
                if (ContextEngine.Instance.gameContext.state == GameStateType.Qualification)
                {
                    isEnd = true;
                }
                else if (player.lap + 1 == ContextEngine.Instance.gameContext.totalLap)
                {
                    isEnd = true;
                }
            }

            var features = player.features;
            if (!isEnd)
            {
                features = this.ComputeOutOfBend(player.features, route.nbOutOfBend);
                var nbCase = route.route.Count - 1;
                var de = 0;
                if (player.state == PlayerStateType.Aspiration)
                {
                    de = 3;
                }
                else if (player.state == PlayerStateType.StandOut)
                {
                    de = nbCase;
                }
                else
                {
                    if (player.currentTurn != null)
                    {
                        de = player.currentTurn.de;
                    }
                    else
                    {
                        de = PlayerEngine.Instance.GetTurnHistories(player).Last().de;
                    }
                }
                features = this.ComputeUseBrake(features, de, route.isStandWay, nbCase);
            }
            return features;
        }

        private FeatureContext ComputeOutOfBend(FeatureContext features, int nbOut)
        {
            if (nbOut > 0)
            {
                FeatureContext result = features.Clone();
                if (ContextEngine.Instance.gameContext.state == GameStateType.Race)
                {
                    result.tire = features.tire - nbOut;
                }
                else if (ContextEngine.Instance.gameContext.state == GameStateType.Qualification)
                {
                    result.outOfBend += nbOut;
                }
                return result;
            }
            else
            {
                return features;
            }
        }

        private FeatureContext ComputeUseBrake(FeatureContext features, int de, bool isStandWay, int nbCase)
        {
            FeatureContext result = features.Clone();
            if (ContextEngine.Instance.gameContext.state == GameStateType.Race && !isStandWay)
            {
                int difDe = de - nbCase;
                if (difDe >= 6)
                {
                    result.brake = features.brake - 3;
                    result.tire = features.tire - 3;
                }
                else if (difDe == 5)
                {
                    result.brake = features.brake - 3;
                    result.tire = features.tire - 2;
                }
                else if (difDe == 4)
                {
                    result.brake = features.brake - 3;
                    result.tire = features.tire - 1;
                }
                else if (difDe == 3)
                {
                    result.brake = features.brake - 3;
                }
                else if (difDe == 2)
                {
                    result.brake = features.brake - 2;
                }
                else if (difDe == 1)
                {
                    result.brake = features.brake - 1;
                }
            }
            return result;
        }
    }
}