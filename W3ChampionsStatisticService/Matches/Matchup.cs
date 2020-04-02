﻿using System;
using System.Collections.Generic;
using System.Linq;
using W3ChampionsStatisticService.MatchEvents;

namespace W3ChampionsStatisticService.Matches
{
    public class Matchup
    {
        public string Map { get; set; }
        public long Id { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public GameMode GameMode { get; set; }
        public IList<Team> Teams { get; set; } = new List<Team>();

        public Matchup(MatchFinishedEvent matchFinishedEvent)
        {
            var data = matchFinishedEvent.match;
            Map = matchFinishedEvent.match.map.Split("/")[3].Replace(".w3x", "").Substring(3);
            Id = data.id;

            GameMode = (GameMode) matchFinishedEvent.match.gameMode;

            StartTime = DateTimeOffset.Now;
            EndTime =  DateTimeOffset.FromUnixTimeMilliseconds(matchFinishedEvent.match.endTime);
            StartTime =  DateTimeOffset.FromUnixTimeMilliseconds(matchFinishedEvent.match.startTime);
            Duration = EndTime - StartTime;

            var winners = data.players.Where(p => p.won);
            var loosers = data.players.Where(p => !p.won);

            Teams.Add(CreateTeam(winners));
            Teams.Add(CreateTeam(loosers));
        }

        private static Team CreateTeam(IEnumerable<PlayerMMrChange> loosers)
        {
            var team = new Team();
            team.Players.AddRange(CreatePlayerArray(loosers));
            return team;
        }

        private static IEnumerable<PlayerOverviewMatches> CreatePlayerArray(IEnumerable<PlayerMMrChange> players)
        {
            return players.Select(w => new PlayerOverviewMatches {
                Name = w.battleTag.Split("#")[0],
                BattleTag = w.battleTag.Split("#")[1],
                CurrentMmr = (int) w.updatedMmr.rating,
                OldMmr = (int) w.mmr.rating,
            });
        }
    }
}