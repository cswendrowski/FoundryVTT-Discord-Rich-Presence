(() => {
    const version = 1;  //Current Version
  
    //Bootstrap
    if (!window.DiscordRichPresence) {
      window.DiscordRichPresence = {loaded: 0};
      window.DiscordRichPresence.setup = () => console.error('Discord Rich Presence | Failed to setup Discord Rich Presence');
      $(() => window.DiscordRichPresence.setup());
    }
  
    if (window.DiscordRichPresence.loaded >= version) {
      return;
    }
    window.DiscordRichPresence.loaded = version;

    function getCurrentSceneName() {
      var scenes = game.scenes.entities;
      var firstActiveScene = scenes.find(function(element) { return element.active; });

      if (!firstActiveScene) return "Unknown";

      return firstActiveScene.name;
    }

    function getCurrentActorName() {
      var actor = getCurrentActor();
      if (!actor) return "Unknown";
      return actor.data.token.name;
    }

    function getCurrentActorHealth() {
      var actor = getCurrentActor();
      if (!actor) return 0;
      return actor.data.data.health.value;
    }

    function getCurrentActorMaxHealth() {
      var actor = getCurrentActor();
      if (!actor) return 0;
      return actor.data.data.health.max;
    }

    function getCurrentActor() {
      return game.actors.entities[0];
    }

    function getCurrentPlayers() {
      return game.users.entities.filter(element => element.active ).length;
    }

    function getMaxPlayers() {
      return game.users.entities.length;
    }
  
    window.DiscordRichPresence.setup = () => {
      console.log(`Discord Rich Presence | Initializing v${version}`);
  
      Hooks.on('ready', () => {
        console.log("Forge initited!");
        console.log("Player is currently on scene " + getCurrentSceneName());
        console.log("Player is currently playing Actor " + getCurrentActorName() + " which has health " + getCurrentActorHealth() + " / " + getCurrentActorMaxHealth());
        console.log("There are currently " + getCurrentPlayers() + " / " + getMaxPlayers() + " Players connected");
      });
    };
  })();
  