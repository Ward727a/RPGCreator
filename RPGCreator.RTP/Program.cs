using RPGCreator.RTP;
using RPGCreator.RTP.Services;
using RPGCreator.SDK;
using RPGCreator.SDK.EngineService;

RuntimeServices.MapService = new MapService();
RuntimeServices.LayerService = new LayerService();

using var game = new EditorGame();
game.Run();
