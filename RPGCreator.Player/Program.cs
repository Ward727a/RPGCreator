using RPGCreator.Player;

var launchArgs = PlayerLaunchArgs.Parse(args);
using var game = new GamePlayer(launchArgs);
game.Run();