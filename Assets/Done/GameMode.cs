using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OrbItProcs
{
    public enum GameModes
    {
        t2vs2,
        tAllvs1,
        FreeForAll,
        Cooperative,
    }
    public enum ScoringModes
    {
        playerKills,
        nodeKills,
        allKills,
        allDamage,
        damageDone,
        leastDamageTaken,
    }
    public class GameMode
    {
        public OrbIt game;
        /// <summary>
        /// If enabled, the players will damage each other, multiplied by the given value.
        /// </summary>
        [Info(UserLevel.User, "If enabled, the players will damage each other, multiplied by the given value.")]
        public Toggle<float> playersHurtPlayers { get; set; }
        /// <summary>
        /// If enabled, normal nodes will hurt players upon collision, by the given value.
        /// </summary>
        [Info(UserLevel.User, "If enabled, normal nodes will hurt players upon collision.")]
        public Toggle<float> nodesHurtPlayers { get; set; }
        /// <summary>
        /// If enabled, the players can hurt nodes.
        /// </summary>
        [Info(UserLevel.User, "If enabled, the players can hurt nodes.")]
        public Toggle<float> playersHurtNodes { get; set; }
        /// <summary>
        /// If enabled, non-player-nodes will hurt eachother on collision.
        /// </summary>
        [Info(UserLevel.User, "If enabled, non-player-nodes will hurt eachother on collision.")]
        public Toggle<float> nodesHurtNodes { get; set; }
        /// <summary>
        /// The game mode determines who the players will be aiming to kill during gameplay.
        /// </summary>
        [Info(UserLevel.User, "The game mode determines who the players will be aiming to kill during gameplay.")]
        public GameModes gameMode
        {
            get { return _gameMode; }
            set
            {
                bool changed = value != _gameMode;
                _gameMode = value;
                if (changed) SetUpTeams();
            }
        }
        private GameModes _gameMode;
        /// <summary>
        /// The scoring mode determines how each player will increase their score.
        /// </summary>
        [Info(UserLevel.User, "The scoring mode determines how each player will increase their score.")]
        public ScoringModes scoringMode { get; set; }
        public Dictionary<Player, HashSet<Player>> playerTeammates;

        public Color globalColor { get; set; }



        public GameMode(OrbIt game)
        {
            this.game = game;
            playersHurtPlayers = new Toggle<float>(1f, true);
            nodesHurtPlayers = new Toggle<float>(1f, false);
            playersHurtNodes = new Toggle<float>(1f, true);
            nodesHurtNodes = new Toggle<float>(1f, false);
            scoringMode = ScoringModes.playerKills;
            _gameMode = GameModes.FreeForAll;
            SetUpTeams();
            globalColor = Color.magenta;
        }

        public void Draw()
        {
            if (gameMode == GameModes.FreeForAll || gameMode == GameModes.Cooperative) return;
            foreach (var p in playerTeammates.Keys)
            {
                if (playerTeammates[p] != null)
                {
                    foreach (var pp in playerTeammates[p])
                    {
                        if (p == pp) continue;
                        game.room.camera.DrawLine(p.node.body.pos, pp.node.body.pos, 3f, Color.white, Layers.Under1);
                    }
                }
            }
        }

        public void SetUpTeams()
        {
            playerTeammates = new Dictionary<Player, HashSet<Player>>();
            if (gameMode == GameModes.tAllvs1)
            {
                if (game.room.groups.player.entities.Count <= 2)
                {
                    gameMode = GameModes.FreeForAll;
                    return;
                }
                else if (game.room.groups.player.entities.Count == 3)
                {
                    var hs = new HashSet<Player>() { game.room.groups.player.entities.ElementAt(0).player, game.room.groups.player.entities.ElementAt(1).player };
                    playerTeammates[game.room.groups.player.entities.ElementAt(0).player] = hs;
                    playerTeammates[game.room.groups.player.entities.ElementAt(1).player] = hs;
                    playerTeammates[game.room.groups.player.entities.ElementAt(2).player] = new HashSet<Player>() { game.room.groups.player.entities.ElementAt(2).player };
                }
                else if (game.room.groups.player.entities.Count == 4)
                {
                    var hs = new HashSet<Player>() { game.room.groups.player.entities.ElementAt(0).player, game.room.groups.player.entities.ElementAt(1).player, game.room.groups.player.entities.ElementAt(2).player };
                    playerTeammates[game.room.groups.player.entities.ElementAt(0).player] = hs;
                    playerTeammates[game.room.groups.player.entities.ElementAt(1).player] = hs;
                    playerTeammates[game.room.groups.player.entities.ElementAt(2).player] = hs;
                    playerTeammates[game.room.groups.player.entities.ElementAt(3).player] = new HashSet<Player>() { game.room.groups.player.entities.ElementAt(3).player };
                }
            }
            else if (gameMode == GameModes.t2vs2)
            {
                if (game.room.groups.player.entities.Count <= 3)
                {
                    gameMode = GameModes.tAllvs1;
                    return;
                }
                var hs1 = new HashSet<Player>() { game.room.groups.player.entities.ElementAt(0).player, game.room.groups.player.entities.ElementAt(1).player };
                var hs2 = new HashSet<Player>() { game.room.groups.player.entities.ElementAt(2).player, game.room.groups.player.entities.ElementAt(3).player };
                playerTeammates[game.room.groups.player.entities.ElementAt(0).player] = hs1;
                playerTeammates[game.room.groups.player.entities.ElementAt(1).player] = hs1;
                playerTeammates[game.room.groups.player.entities.ElementAt(2).player] = hs2;
                playerTeammates[game.room.groups.player.entities.ElementAt(3).player] = hs2;
            }
            else if (gameMode == GameModes.Cooperative)
            {
                HashSet<Player> playerset = new HashSet<Player>();
                foreach (Node n in game.room.groups.player.entities)
                {
                    playerset.Add(n.player);
                }
                foreach (Node n in game.room.groups.player.entities)
                {
                    playerTeammates[n.player] = playerset;
                }
            }

        }
        public float DetermineDamage(Node damager, Node damagee, float dmg)
        {
            if (damager == null)
            {
                return dmg;
            }
            if (damager.IsPlayer)
            {
                //both players
                if (damagee.IsPlayer)
                {
                    if (gameMode == GameModes.Cooperative) return 0;
                    float mult = playersHurtPlayers.enabled ? playersHurtPlayers.value : 0;
                    if (gameMode == GameModes.FreeForAll) return dmg * mult;
                    if (playerTeammates[damager.player].Contains(damagee.player)) return 0;
                    return dmg * mult;
                    //
                }
                //player hurting node
                else
                {
                    if (playersHurtNodes.enabled)
                    {
                        return dmg * playersHurtNodes.value;
                    }
                    return 0;
                }
            }
            else
            {
                //node hurting player
                if (damagee.IsPlayer)
                {
                    if (nodesHurtPlayers.enabled)
                    {
                        return dmg * nodesHurtPlayers.value;
                    }
                    return 0;
                }
                //both nodes
                else
                {
                    if (nodesHurtNodes.enabled)
                    {
                        return dmg * nodesHurtNodes.value;
                    }
                    return 0;
                }
            }
            //return dmg;
        }

    }
}