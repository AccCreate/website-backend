using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using W3ChampionsStatisticService.CommonValueObjects;
using W3ChampionsStatisticService.PlayerProfiles;
using W3ChampionsStatisticService.PlayerProfiles._2v2Stats;

namespace WC3ChampionsStatisticService.UnitTests
{
    [TestFixture]
    public class PlayerTests : IntegrationTestBase
    {
        [Test]
        public async Task LoadAndSave()
        {
            var playerRepository = new PlayerRepository(MongoClient);

            var player = PlayerProfile.Create("peter#123");
            await playerRepository.UpsertPlayer(player);
            var playerLoaded = await playerRepository.LoadPlayer(player.BattleTag);

            Assert.AreEqual(player.BattleTag, playerLoaded.BattleTag);
        }

        [Test]
        public async Task PlayerMapping()
        {
            var playerRepository = new PlayerRepository(MongoClient);

            var player = PlayerProfile.Create("peter#123");
            player.RecordWin(Race.HU, GameMode.GM_1v1, GateWay.Europe, 0, true);
            await playerRepository.UpsertPlayer(player);
            var playerLoaded = await playerRepository.LoadPlayer(player.BattleTag);
            playerLoaded.RecordWin(Race.UD, GameMode.GM_1v1, GateWay.Europe, 0, false);
            playerLoaded.UpdateRank(GameMode.GM_1v1, GateWay.Europe, 234, 123, 0);
            await playerRepository.UpsertPlayer(playerLoaded);

            var playerLoadedAgain = await playerRepository.LoadPlayer(player.BattleTag);

            Assert.AreEqual(player.BattleTag, playerLoaded.BattleTag);
            Assert.AreEqual(player.BattleTag, playerLoadedAgain.BattleTag);
            Assert.AreEqual(234, playerLoadedAgain.GetStatForGateway(GateWay.Europe).GameModeStats[0].MMR);
            Assert.AreEqual(123, playerLoadedAgain.GetStatForGateway(GateWay.Europe).GameModeStats[0].RankingPoints);
        }

        [Test]
        public async Task PlayerStatsMapping()
        {
            var playerRepository = new PlayerRepository(MongoClient);

            var battleTagIdCombined = new BattleTagIdCombined(new List<PlayerId>
                {
                    PlayerId.Create("peter#123")
                },
                GateWay.Europe,
                GameMode.GM_1v1,
                1);
            var player = GameModeStatPerGateway.Create(battleTagIdCombined);

            await playerRepository.UpsertPlayerGameModeStatPerGateway(player);

            var playerLoadedAgain = await playerRepository.LoadGameModeStatPerGateway("peter#123", GateWay.Europe, GameMode.GM_1v1, 1);


            playerLoaded.UpdateRank(GameMode.GM_1v1, GateWay.Europe, 234, 123, 0);

        }

        [Test]
        public async Task PlayerIdMappedRight()
        {
            var playerRepository = new PlayerRepository(MongoClient);

            var player1 = PlayerProfile.Create("peter#123");
            var player2 = PlayerProfile.Create("wolf#456");

            await playerRepository.UpsertPlayer(player1);
            await playerRepository.UpsertPlayer(player2);

            var playerLoaded = await playerRepository.LoadPlayer(player2.BattleTag);

            Assert.IsNotNull(playerLoaded);
            Assert.AreEqual(player2.BattleTag, playerLoaded.BattleTag);
        }

        [Test]
        public async Task PlayerMultipleWinRecords()
        {
            var playerRepository = new PlayerRepository(MongoClient);
            var handler = new PlayerProfileHandler(playerRepository);
            var handler2 = new GameModeStatPerGatewayHandler(playerRepository);

            var ev = TestDtoHelper.CreateFakeEvent();
            ev.match.players[0].battleTag = "peter#123";
            ev.match.players[0].race = Race.HU;
            ev.match.players[1].race = Race.OC;

            ev.match.gateway = GateWay.America;

            for (int i = 0; i < 100; i++)
            {
                await handler.Update(ev);
                await handler2.Update(ev);
            }

            var playerLoaded = await playerRepository.LoadPlayer("peter#123");
            var playerLoadedStats = await playerRepository.LoadPlayerGameModeStat("peter#123", GameMode.GM_1v1, GateWay.Europe, 0);

            Assert.AreEqual(100, playerLoadedStats.Single().Wins);
            Assert.AreEqual(100, playerLoaded.RaceStats[0].Wins);
        }
    }
}