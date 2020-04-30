﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsStatisticService.Ports;

namespace W3ChampionsStatisticService.PlayerProfiles
{
    [ApiController]
    [Route("api/players")]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IRankRepository _rankRepository;

        public PlayersController(IPlayerRepository playerRepository, IRankRepository rankRepository)
        {
            _playerRepository = playerRepository;
            _rankRepository = rankRepository;
        }

        [HttpGet("{battleTag}")]
        public async Task<IActionResult> GetPlayer([FromRoute] string battleTag)
        {
            var player = await _playerRepository.Load(battleTag) ?? PlayerProfile.Default();
            var loadPlayerOfLeagueLike = await _rankRepository.LoadPlayerOfLeague(battleTag);
            player.GameModeStats[0].Rank = loadPlayerOfLeagueLike.RankNumber;
            player.GameModeStats[0].LeagueId = loadPlayerOfLeagueLike.League;
            return Ok(player);
        }

        [HttpGet("{battleTag}/winrate")]
        public async Task<IActionResult> GetPlayerWinrate([FromRoute] string battleTag)
        {
            var wins = await _playerRepository.LoadPlayerWinrate(battleTag);
            return Ok(wins);
        }
    }
}